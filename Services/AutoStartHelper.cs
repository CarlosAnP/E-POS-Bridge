using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace EposBridge.Services;

public static class AutoStartHelper
{
    private const string RunKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
    private const string AppName = "EposBridge";

    public static bool IsAutoStartEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
        return key?.GetValue(AppName) != null;
    }

    public static void SetAutoStart(bool enable)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
        if (key == null) return;

        if (enable)
        {
            key.SetValue(AppName, Environment.ProcessPath ?? Application.ExecutablePath);
        }
        else
        {
            key.DeleteValue(AppName, false);
        }
    }
}
