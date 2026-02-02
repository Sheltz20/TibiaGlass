using System.Text.Json.Serialization;

namespace TibiaGlassMagnifier;

public sealed class AppSettings
{
    public const double DefaultZoom = 2.0;

    public const int DefaultWindowWidthPx = 320;
    public const int DefaultWindowHeightPx = 240;

    public double Zoom { get; set; } = DefaultZoom;

    
    public int WindowWidthPx { get; set; } = DefaultWindowWidthPx;
    public int WindowHeightPx { get; set; } = DefaultWindowHeightPx;

    public HotkeySettings Hotkey { get; set; } = HotkeySettings.DefaultAltHold;
}

public sealed class HotkeySettings
{
    
    public int KeyVk { get; set; }

    public bool Ctrl { get; set; }
    public bool Alt { get; set; }
    public bool Shift { get; set; }
    public bool Win { get; set; }

    [JsonIgnore]
    public bool IsModifierOnly => KeyVk == 0;

    public static HotkeySettings DefaultAltHold => new()
    {
        KeyVk = 0,
        Alt = true
    };
}
