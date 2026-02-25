using System;
using System.Drawing;
using System.Windows.Forms;
using EposBridge.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EposBridge;

public class DashboardForm : Form
{
    private readonly IServiceProvider _services;
    private readonly PrintHistoryService _historyService;
    private readonly PrintQueueService _queueService;
    private readonly PrinterService _printerService;

    private DataGridView _gridHistory = null!;
    private ComboBox _cmbPrinters = null!;
    private System.Windows.Forms.Timer _timer;

    public DashboardForm(IServiceProvider services)
    {
        _services = services;
        _historyService = services.GetRequiredService<PrintHistoryService>();
        _queueService = services.GetRequiredService<PrintQueueService>();
        _printerService = services.GetRequiredService<PrinterService>();

        InitializeComponent();
        LoadPrinters();
        RefreshHistory();
        
        // Timer for auto-refresh
        _timer = new System.Windows.Forms.Timer { Interval = 5000 };
        _timer.Tick += (s, e) => RefreshHistory();
        _timer.Start();
    }

    private void InitializeComponent()
    {
        this.Text = "EPOS Bridge - Dashboard";
        this.Size = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Icon = SystemIcons.Application;
        try {
             var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("EposBridge.ico.ico");
             if (stream != null) this.Icon = new Icon(stream);
        } catch {}

        var tabs = new TabControl { Dock = DockStyle.Fill };

        // --- Tab 1: Estado ---
        var tabStatus = new TabPage("Estado y Pruebas");
        var panelStatus = new TableLayoutPanel 
        { 
            Dock = DockStyle.Fill, 
            Padding = new Padding(20),
            RowCount = 4,
            AutoSize = true
        };

        var lblStatus = new Label 
        { 
            Text = "Estado del Servicio: ACTIVO (Escuchando en puertos 8000/8001)", 
            ForeColor = Color.Green, 
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            AutoSize = true 
        };

        _cmbPrinters = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
        var btnTestPrint = new Button { Text = "Prueba de Impresión", Width = 150 };
        btnTestPrint.Click += (s, e) => TestPrint();
        
        var btnTestDrawer = new Button { Text = "Abrir Cajón", Width = 150 };
        btnTestDrawer.Click += (s, e) => TestDrawer();

        panelStatus.Controls.Add(lblStatus);
        panelStatus.Controls.Add(new Label { Text = "Seleccionar Impresora:", AutoSize = true });
        panelStatus.Controls.Add(_cmbPrinters);
        
        var flowButtons = new FlowLayoutPanel { AutoSize = true };
        flowButtons.Controls.Add(btnTestPrint);
        flowButtons.Controls.Add(btnTestDrawer);
        panelStatus.Controls.Add(flowButtons);

        tabStatus.Controls.Add(panelStatus);

        // --- Tab 2: Historial ---
        var tabHistory = new TabPage("Historial de Impresión");
        var panelHistory = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        
        _gridHistory = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };

        _gridHistory.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Hora", DataPropertyName = "Timestamp", Width = 120 });
        _gridHistory.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Impresora", DataPropertyName = "PrinterName", Width = 200 });
        _gridHistory.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Estado", DataPropertyName = "Status", Width = 100 });
        _gridHistory.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Error", DataPropertyName = "ErrorMessage", Width = 200 });

        var btnReprint = new Button { Text = "Reimprimir Seleccionado", Dock = DockStyle.Bottom, Height = 40 };
        btnReprint.Click += (s, e) => ReprintSelected();

        panelHistory.Controls.Add(_gridHistory);
        panelHistory.Controls.Add(btnReprint);
        tabHistory.Controls.Add(panelHistory);

        // --- Add Tabs ---
        tabs.TabPages.Add(tabStatus);
        tabs.TabPages.Add(tabHistory);
        this.Controls.Add(tabs);
    }

    private void LoadPrinters()
    {
        _cmbPrinters.Items.Clear();
        foreach (string p in _printerService.GetInstalledPrinters())
        {
            _cmbPrinters.Items.Add(p);
        }
        if (_cmbPrinters.Items.Count > 0) _cmbPrinters.SelectedIndex = 0;
    }

    private async void TestPrint()
    {
        if (_cmbPrinters.SelectedItem == null) return;
        string printer = _cmbPrinters.SelectedItem.ToString()!;
        
        string text = "EPOS Bridge\nTest Nativo\n----------------\nOK\n\n\n";
        string b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text));
        
        var job = new Models.PrintJob { PrinterName = printer, ContentBase64 = b64 };
        await _queueService.EnqueueJobAsync(job);
        MessageBox.Show("Trabajo de prueba enviado a la cola.");
        RefreshHistory();
    }

    private async void TestDrawer()
    {
         if (_cmbPrinters.SelectedItem == null) return;
        string printer = _cmbPrinters.SelectedItem.ToString()!;
        
        // Command: ESC p 0 25 250
        string drawerCmdBase64 = Convert.ToBase64String(new byte[] { 0x1B, 0x70, 0x00, 0x19, 0xFA });
        var job = new Models.PrintJob { PrinterName = printer, ContentBase64 = drawerCmdBase64 };
        
        await _queueService.EnqueueJobAsync(job);
        MessageBox.Show("Señal de cajón enviada.");
        RefreshHistory();
    }

    private void RefreshHistory()
    {
        var history = _historyService.GetHistory();
        // Invoke if needed (though Timer runs on UI thread usually)
        if (_gridHistory.InvokeRequired)
        {
            _gridHistory.Invoke(new Action(() => _gridHistory.DataSource = history));
        }
        else
        {
            _gridHistory.DataSource = history;
        }
    }

    private async void ReprintSelected()
    {
        if (_gridHistory.SelectedRows.Count == 0) return;
        var job = _gridHistory.SelectedRows[0].DataBoundItem as Models.PrintJob;
        if (job == null) return;

        var newJob = new Models.PrintJob
        {
            PrinterName = job.PrinterName,
            ContentBase64 = job.ContentBase64,
            Status = "Pending (Reprint)"
        };

        await _queueService.EnqueueJobAsync(newJob);
        MessageBox.Show("Reimpresión enviada a la cola.");
        RefreshHistory();
    }
}
