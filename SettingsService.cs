using System.IO;
using System.Text.Json;

namespace TibiaGlassMagnifier;

internal sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string SettingsPath { get; }

    public SettingsService()
    {
        string dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TibiaGlassMagnifier");
        Directory.CreateDirectory(dir);
        SettingsPath = Path.Combine(dir, "settings.json");
    }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                AppSettings defaults = new();
                Save(defaults);
                return defaults;
            }

            string json = File.ReadAllText(SettingsPath);
            AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            return Normalize(settings ?? new AppSettings());
        }
        catch
        {
            
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        settings = Normalize(settings);
        string json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SettingsPath, json);
    }

    private static AppSettings Normalize(AppSettings settings)
    {
        if (settings.Zoom is < 1.0 or > 10.0)
        {
            settings.Zoom = AppSettings.DefaultZoom;
        }

        
        if (settings.WindowWidthPx < 160 || settings.WindowWidthPx > 1600)
        {
            settings.WindowWidthPx = AppSettings.DefaultWindowWidthPx;
        }

        if (settings.WindowHeightPx < 120 || settings.WindowHeightPx > 1200)
        {
            settings.WindowHeightPx = AppSettings.DefaultWindowHeightPx;
        }

        settings.Hotkey ??= HotkeySettings.DefaultAltHold;
        return settings;
    }
}
