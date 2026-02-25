using System.Windows.Forms;
using System.Drawing;

namespace EposBridge;

public class BridgeApplicationContext : ApplicationContext
{
    private NotifyIcon _notifyIcon;
    private IServiceProvider _services;

    public BridgeApplicationContext(IServiceProvider services)
    {
        _services = services;
        
        Icon appIcon = SystemIcons.Application;
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            // Note: Namespace.FileName
            var stream = assembly.GetManifestResourceStream("EposBridge.ico.ico");
            if (stream != null)
            {
                appIcon = new Icon(stream);
            }
        }
        catch { }

        _notifyIcon = new NotifyIcon
        {
            Icon = appIcon,
            Visible = true,
            Text = "EPOS Bridge",
            ContextMenuStrip = new ContextMenuStrip()
        };

        // Register icon for notifications
        var notificationService = _services.GetService(typeof(Services.INotificationService)) as Services.NotificationService;
        if (notificationService != null)
        {
            Services.NotificationService.RegisterNotifyIcon(_notifyIcon);
        }

        var menu = _notifyIcon.ContextMenuStrip.Items;
        menu.Add("Abrir Dashboard", null, (s, e) => OpenDashboard());
        
        var autoStartItem = new ToolStripMenuItem("Iniciar con Windows");
        autoStartItem.Click += (s, e) => 
        {
            if (s is ToolStripMenuItem item)
            {
                bool newState = !Services.AutoStartHelper.IsAutoStartEnabled();
                Services.AutoStartHelper.SetAutoStart(newState);
                item.Checked = newState;
                MessageBox.Show(newState ? "Auto-inicio activado." : "Auto-inicio desactivado.", "EPOS Bridge");
            }
        };
        // Initial state check
        autoStartItem.Checked = Services.AutoStartHelper.IsAutoStartEnabled();
        
        menu.Add(autoStartItem);
        menu.Add("-");
        menu.Add("Salir", null, (s, e) => Exit());
    }

    private void OpenDashboard()
    {
        var form = new DashboardForm(_services);
        form.Show();
    }

    private void Exit()
    {
        _notifyIcon.Visible = false;
        Application.Exit();
        Environment.Exit(0);
    }
}
