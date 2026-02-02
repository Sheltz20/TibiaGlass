using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace TibiaGlassMagnifier;

internal sealed class TrayIconService : IDisposable
{
    private NotifyIcon? _notifyIcon;

    public event EventHandler? SettingsRequested;
    public event EventHandler? ExitRequested;

    public void Initialize()
    {
        if (_notifyIcon is not null)
        {
            return;
        }

        ContextMenuStrip menu = new();

        ToolStripMenuItem settingsItem = new("Settings...");
        settingsItem.Click += (_, _) => SettingsRequested?.Invoke(this, EventArgs.Empty);
        menu.Items.Add(settingsItem);

        menu.Items.Add(new ToolStripSeparator());

        ToolStripMenuItem exitItem = new("Exit");
        exitItem.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);
        menu.Items.Add(exitItem);

        Icon icon = LoadTrayIcon();

        _notifyIcon = new NotifyIcon
        {
            Text = "TibiaGlass Magnifier",
            Icon = icon,
            Visible = true,
            ContextMenuStrip = menu
        };

        
        _notifyIcon.DoubleClick += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);
    }

    private static Icon LoadTrayIcon()
    {
        try
        {
            
            string? exePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrWhiteSpace(exePath))
            {
                Icon? extracted = Icon.ExtractAssociatedIcon(exePath);
                if (extracted is not null)
                {
                    return (Icon)extracted.Clone();
                }
            }

            
            string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "TibiaGlass.ico");
            if (File.Exists(iconPath))
            {
                return new Icon(iconPath);
            }
        }
        catch
        {
            
        }

        return SystemIcons.Application;
    }

    public void ShowBalloon(string title, string message)
    {
        if (_notifyIcon is null)
        {
            return;
        }

        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText = message;
        _notifyIcon.ShowBalloonTip(1500);
    }

    public void Dispose()
    {
        if (_notifyIcon is null)
        {
            return;
        }

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _notifyIcon = null;
    }
}
