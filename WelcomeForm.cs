using System;
using System.Drawing;
using System.Windows.Forms;

namespace EposBridge;

public class WelcomeForm : Form
{
    public WelcomeForm()
    {
        this.Text = "EPOS Bridge - Bienvenido";
        this.Size = new Size(500, 300);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        
        try {
             var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("EposBridge.ico.ico");
             if (stream != null) this.Icon = new Icon(stream);
             else this.Icon = SystemIcons.Application;
        } catch { this.Icon = SystemIcons.Application; }

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            RowCount = 3,
            ColumnCount = 1
        };

        var title = new Label
        {
            Text = "E-POS Bridge",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 120, 212),
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        var description = new Label
        {
            Text = "Un programa puente para la impresión térmica del sistema.\n\n" +
                   "Asegúrate de activar el 'Inicio Automático' para garantizar la conexión " +
                   "permanente entre E-POS y tus impresoras.\n\n" +
                   "La aplicación se ejecutará minimizada en la barra de tareas.",
            Font = new Font("Segoe UI", 11),
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Left,
            MaximumSize = new Size(440, 0)
        };

        var btnAutoStart = new Button
        {
            Text = "Activar inicio con Windows",
            Height = 40,
            Width = 200,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            BackColor = Color.FromArgb(240, 240, 240),
            ForeColor = Color.Black,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnAutoStart.Click += (s, e) => 
        {
            Services.AutoStartHelper.SetAutoStart(true);
            btnAutoStart.Text = "¡Activado ✓!";
            btnAutoStart.Enabled = false;
        };

        // Check if already enabled
        if (Services.AutoStartHelper.IsAutoStartEnabled())
        {
             btnAutoStart.Text = "¡Activado ✓!";
             btnAutoStart.Enabled = false;
        }

        var btnOk = new Button
        {
            Text = "Entendido, iniciar minimizado",
            DialogResult = DialogResult.OK,
            Height = 40,
            Width = 200,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            BackColor = Color.FromArgb(0, 120, 212),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnOk.Click += (s, e) => this.Close();

        var buttonsPanel = new Panel { Height = 50, Dock = DockStyle.Bottom };
        buttonsPanel.Controls.Add(btnAutoStart);
        buttonsPanel.Controls.Add(btnOk);

        panel.Controls.Add(title);
        panel.Controls.Add(description);
        panel.Controls.Add(buttonsPanel);

        this.Controls.Add(panel);
    }
}
