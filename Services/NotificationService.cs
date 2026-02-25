using System.Windows.Forms;

namespace EposBridge.Services;

public interface INotificationService
{
    void ShowNotification(string title, string message, ToolTipIcon icon);
}

public class NotificationService : INotificationService
{
    private static NotifyIcon? _notifyIcon;

    public static void RegisterNotifyIcon(NotifyIcon icon)
    {
        _notifyIcon = icon;
    }

    public void ShowNotification(string title, string message, ToolTipIcon icon)
    {
        if (_notifyIcon != null && _notifyIcon.Visible)
        {
            _notifyIcon.ShowBalloonTip(3000, title, message, icon);
        }
    }
}
