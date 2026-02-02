using System.Windows;
using System.Windows.Input;
using TibiaGlassMagnifier.Interop;

namespace TibiaGlassMagnifier;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _working;
    
    
    private bool _isInitializing = true;

    private static readonly double[] ZoomOptions = [2.0, 3.0, 4.0, 5.0, 6.0, 8.0, 10.0];

    public event EventHandler<AppSettings>? Saved;

    public SettingsWindow(AppSettings current)
    {
        InitializeComponent();

        _isInitializing = true;
        _working = new AppSettings
        {
            Zoom = current.Zoom,
            WindowWidthPx = current.WindowWidthPx,
            WindowHeightPx = current.WindowHeightPx,
            Hotkey = new HotkeySettings
            {
                KeyVk = current.Hotkey.KeyVk,
                Ctrl = current.Hotkey.Ctrl,
                Alt = current.Hotkey.Alt,
                Shift = current.Hotkey.Shift,
                Win = current.Hotkey.Win,
            }
        };

        ZoomCombo.Items.Clear();
        foreach (double z in ZoomOptions)
        {
            ZoomCombo.Items.Add($"{z:0.#}x");
        }

        ZoomCombo.SelectedIndex = ClosestZoomIndex(_working.Zoom);
        HotkeyBox.Text = FormatHotkey(_working.Hotkey);

        
        int initialWidth = _working.WindowWidthPx;
        if (initialWidth < 200) initialWidth = 200;
        if (initialWidth > 900) initialWidth = 900;
        SizeSlider.Value = initialWidth;
        UpdateSizeFromSlider();

        _isInitializing = false;
    }

    private void Save_OnClick(object sender, RoutedEventArgs e)
    {
        _working.Zoom = GetSelectedZoom();
        UpdateSizeFromSlider();
        Saved?.Invoke(this, _working);
        Close();
    }

    private void Cancel_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ZoomCombo_OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_isInitializing)
        {
            return;
        }

        
    }

    private void SizeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitializing)
        {
            return;
        }

        UpdateSizeFromSlider();
    }

    private void UpdateSizeFromSlider()
    {
        if (SizeSlider is null || SizeLabel is null)
        {
            return;
        }

        int widthPx = (int)Math.Round(SizeSlider.Value);
        int heightPx = (int)Math.Round(widthPx * 3.0 / 4.0);

        _working.WindowWidthPx = widthPx;
        _working.WindowHeightPx = heightPx;

        SizeLabel.Text = $"{widthPx}×{heightPx}";
    }

    private double GetSelectedZoom()
    {
        int i = ZoomCombo.SelectedIndex;
        if (i < 0 || i >= ZoomOptions.Length)
        {
            return AppSettings.DefaultZoom;
        }

        return ZoomOptions[i];
    }

    private static int ClosestZoomIndex(double zoom)
    {
        int bestIndex = 0;
        double bestDistance = double.MaxValue;

        for (int i = 0; i < ZoomOptions.Length; i++)
        {
            double d = Math.Abs(ZoomOptions[i] - zoom);
            if (d < bestDistance)
            {
                bestDistance = d;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private void HotkeyBox_OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        HotkeyBox.Text = "Press a key combo...";
    }

    private void HotkeyBox_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        HotkeyBox.Text = FormatHotkey(_working.Hotkey);
    }

    private void HotkeyBox_OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        
        if (e.Key is System.Windows.Input.Key.Back or System.Windows.Input.Key.Delete)
        {
            _working.Hotkey = HotkeySettings.DefaultAltHold;
            HotkeyBox.Text = FormatHotkey(_working.Hotkey);
            e.Handled = true;
            return;
        }

        
        if (e.Key is System.Windows.Input.Key.LeftCtrl or System.Windows.Input.Key.RightCtrl or System.Windows.Input.Key.LeftAlt or System.Windows.Input.Key.RightAlt or System.Windows.Input.Key.LeftShift or System.Windows.Input.Key.RightShift
            or System.Windows.Input.Key.LWin or System.Windows.Input.Key.RWin)
        {
            e.Handled = true;
            return;
        }

        ModifierKeys mods = Keyboard.Modifiers;

        int keyVk = KeyInterop.VirtualKeyFromKey(e.Key);
        _working.Hotkey = new HotkeySettings
        {
            KeyVk = keyVk,
            Ctrl = (mods & ModifierKeys.Control) != 0,
            Alt = (mods & ModifierKeys.Alt) != 0,
            Shift = (mods & ModifierKeys.Shift) != 0,
            Win = (mods & ModifierKeys.Windows) != 0,
        };

        HotkeyBox.Text = FormatHotkey(_working.Hotkey);
        e.Handled = true;
    }

    private static string FormatHotkey(HotkeySettings hk)
    {
        List<string> parts = new();
        if (hk.Ctrl) parts.Add("Ctrl");
        if (hk.Alt) parts.Add("Alt");
        if (hk.Shift) parts.Add("Shift");
        if (hk.Win) parts.Add("Win");

        if (hk.IsModifierOnly)
        {
            if (parts.Count == 0)
            {
                return "(none)";
            }

            return string.Join("+", parts);
        }

        string keyName = ((System.Windows.Input.Key)KeyInterop.KeyFromVirtualKey(hk.KeyVk)).ToString();
        parts.Add(keyName);
        return string.Join("+", parts);
    }
}
