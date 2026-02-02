using System.Windows;

namespace TibiaGlassMagnifier;

internal sealed class MagnifierController : IDisposable
{
    private MagnifierWindow? _window;
    private double _zoom = AppSettings.DefaultZoom;
    private int _windowWidthPx = AppSettings.DefaultWindowWidthPx;
    private int _windowHeightPx = AppSettings.DefaultWindowHeightPx;

    public void SetZoom(double zoom)
    {
        _zoom = Math.Clamp(zoom, 1.0, 10.0);

        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
        {
            _window?.SetZoom(_zoom);
        });
    }

    public void SetOutputSize(int widthPx, int heightPx)
    {
        
        _windowWidthPx = Math.Clamp(widthPx, 160, 1600);
        _windowHeightPx = Math.Clamp(heightPx, 120, 1200);

        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
        {
            _window?.SetOutputSizePx(_windowWidthPx, _windowHeightPx);
        });
    }

    public void Show()
    {
        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
        {
            _window ??= new MagnifierWindow();
            _window.SetZoom(_zoom);
            _window.SetOutputSizePx(_windowWidthPx, _windowHeightPx);
            if (!_window.IsVisible)
            {
                _window.Show();
            }

            _window.Start();
        });
    }

    public void Hide()
    {
        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
        {
            if (_window is null)
            {
                return;
            }

            _window.Stop();
            _window.Hide();
        });
    }

    public void Dispose()
    {
        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
        {
            if (_window is null)
            {
                return;
            }

            _window.Stop();
            _window.Close();
            _window = null;
        });
    }
}
