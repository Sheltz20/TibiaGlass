using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using TibiaGlassMagnifier.Interop;

namespace TibiaGlassMagnifier;

public partial class MagnifierWindow : Window
{
    
    private int _windowWidthPx = AppSettings.DefaultWindowWidthPx;
    private int _windowHeightPx = AppSettings.DefaultWindowHeightPx;
    private double _zoom = 2.0;

    
    private const int CornerRadiusPx = 16;

    private readonly DispatcherTimer _timer;

    private nint _thumbnail;
    private nint _sourceHwnd;
    private nint _destHwnd;

    public MagnifierWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(33) 
        };
        _timer.Tick += (_, _) => UpdateFrame();

        SourceInitialized += (_, _) => InitializeWindowAndDwm();
    }

    public void SetZoom(double zoom)
    {
        
        if (double.IsNaN(zoom) || double.IsInfinity(zoom))
        {
            return;
        }

        _zoom = Math.Clamp(zoom, 1.0, 10.0);
    }

    public void SetOutputSizePx(int widthPx, int heightPx)
    {
        _windowWidthPx = Math.Clamp(widthPx, 160, 1600);
        _windowHeightPx = Math.Clamp(heightPx, 120, 1200);

        if (_destHwnd != 0)
        {
            ApplyPhysicalPixelSize();
        }
    }

    public void Start()
    {
        if (!_timer.IsEnabled)
        {
            _timer.Start();
        }
    }

    public void Stop()
    {
        if (_timer.IsEnabled)
        {
            _timer.Stop();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        Stop();
        UnregisterThumbnail();
        base.OnClosed(e);
    }

    private void UpdateFrame()
    {
        if (_destHwnd == 0)
        {
            return;
        }

        if (!NativeMethods.GetCursorPos(out NativeMethods.POINT cursorScreenPx))
        {
            return;
        }

        nint sourceHwnd = GetTopLevelWindowUnderCursor(cursorScreenPx);
        EnsureThumbnailRegistered(sourceHwnd);
        if (_thumbnail == 0 || _sourceHwnd == 0)
        {
            return;
        }

        
        (double dpiX, double dpiY) = GetDpiScale();
        double desiredLeftDip = (cursorScreenPx.X / dpiX) - (Width / 2.0);
        double desiredTopDip = (cursorScreenPx.Y / dpiY) - (Height / 2.0);

        
        int vsLeftPx = NativeMethods.GetSystemMetrics(NativeMethods.SM_XVIRTUALSCREEN);
        int vsTopPx = NativeMethods.GetSystemMetrics(NativeMethods.SM_YVIRTUALSCREEN);
        int vsWidthPx = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXVIRTUALSCREEN);
        int vsHeightPx = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYVIRTUALSCREEN);
        int vsRightPx = vsLeftPx + vsWidthPx;
        int vsBottomPx = vsTopPx + vsHeightPx;

        int windowWidthPx = (int)Math.Round(Width * dpiX);
        int windowHeightPx = (int)Math.Round(Height * dpiY);
        int desiredLeftPx = (int)Math.Round(desiredLeftDip * dpiX);
        int desiredTopPx = (int)Math.Round(desiredTopDip * dpiY);

        if (desiredLeftPx < vsLeftPx) desiredLeftPx = vsLeftPx;
        if (desiredTopPx < vsTopPx) desiredTopPx = vsTopPx;
        if (desiredLeftPx + windowWidthPx > vsRightPx) desiredLeftPx = vsRightPx - windowWidthPx;
        if (desiredTopPx + windowHeightPx > vsBottomPx) desiredTopPx = vsBottomPx - windowHeightPx;

        Left = desiredLeftPx / dpiX;
        Top = desiredTopPx / dpiY;

        
        if (!NativeMethods.GetClientRect(_destHwnd, out NativeMethods.RECT destClientPx))
        {
            return;
        }

        int destWpx = Math.Max(1, destClientPx.Width);
        int destHpx = Math.Max(1, destClientPx.Height);

        int srcWpx = Math.Max(1, (int)Math.Round(destWpx / _zoom));
        int srcHpx = Math.Max(1, (int)Math.Round(destHpx / _zoom));

        
        
        if (!NativeMethods.GetWindowRect(_sourceHwnd, out NativeMethods.RECT srcWindowRectPx))
        {
            return;
        }

        int cursorInSourceX = cursorScreenPx.X - srcWindowRectPx.Left;
        int cursorInSourceY = cursorScreenPx.Y - srcWindowRectPx.Top;

        
        int srcTotalW = srcWindowRectPx.Width;
        int srcTotalH = srcWindowRectPx.Height;
        if (NativeMethods.DwmQueryThumbnailSourceSize(_thumbnail, out NativeMethods.POINT srcSize) == 0)
        {
            srcTotalW = srcSize.X;
            srcTotalH = srcSize.Y;
        }

        int desiredSrcLeft = cursorInSourceX - (srcWpx / 2);
        int desiredSrcTop = cursorInSourceY - (srcHpx / 2);

        int srcLeft = desiredSrcLeft;
        int srcTop = desiredSrcTop;

        
        if (srcLeft < 0) srcLeft = 0;
        if (srcTop < 0) srcTop = 0;
        if (srcLeft + srcWpx > srcTotalW) srcLeft = srcTotalW - srcWpx;
        if (srcTop + srcHpx > srcTotalH) srcTop = srcTotalH - srcHpx;
        if (srcLeft < 0) srcLeft = 0;
        if (srcTop < 0) srcTop = 0;

        
        
        int deltaX = desiredSrcLeft - srcLeft;
        int deltaY = desiredSrcTop - srcTop;
        int offsetDestX = (int)Math.Round(-deltaX * _zoom);
        int offsetDestY = (int)Math.Round(-deltaY * _zoom);

        int destLeft = Math.Max(0, offsetDestX);
        int destTop = Math.Max(0, offsetDestY);
        int destRight = Math.Min(destWpx, destWpx + offsetDestX);
        int destBottom = Math.Min(destHpx, destHpx + offsetDestY);

        
        if (destRight <= destLeft)
        {
            destLeft = 0;
            destRight = destWpx;
        }
        if (destBottom <= destTop)
        {
            destTop = 0;
            destBottom = destHpx;
        }

        NativeMethods.DWM_THUMBNAIL_PROPERTIES props = new()
        {
            dwFlags = NativeMethods.DWM_TNP_VISIBLE
                | NativeMethods.DWM_TNP_OPACITY
                | NativeMethods.DWM_TNP_RECTDESTINATION
                | NativeMethods.DWM_TNP_RECTSOURCE
                | NativeMethods.DWM_TNP_SOURCECLIENTAREAONLY,
            fVisible = true,
            opacity = 255,
            
            fSourceClientAreaOnly = false,
            rcDestination = new NativeMethods.RECT
            {
                Left = destLeft,
                Top = destTop,
                Right = destRight,
                Bottom = destBottom
            },
            rcSource = new NativeMethods.RECT
            {
                Left = srcLeft,
                Top = srcTop,
                Right = srcLeft + srcWpx,
                Bottom = srcTop + srcHpx
            }
        };

        _ = NativeMethods.DwmUpdateThumbnailProperties(_thumbnail, ref props);
    }

    private void InitializeWindowAndDwm()
    {
        _destHwnd = new WindowInteropHelper(this).Handle;
        if (_destHwnd == 0)
        {
            return;
        }

        ApplyPhysicalPixelSize();

        ApplyRoundedCorners();

        nint exStyle = NativeMethods.GetWindowLongPtr(_destHwnd, NativeMethods.GWL_EXSTYLE);
        nint newStyle = new nint(exStyle.ToInt64()
            | NativeMethods.WS_EX_TRANSPARENT
            | NativeMethods.WS_EX_TOOLWINDOW
            | NativeMethods.WS_EX_NOACTIVATE);

        NativeMethods.SetWindowLongPtr(_destHwnd, NativeMethods.GWL_EXSTYLE, newStyle);

        EnsureThumbnailRegistered(0);
    }

    private void ApplyPhysicalPixelSize()
    {
        
        (double dpiX, double dpiY) = GetDpiScale();
        Width = _windowWidthPx / dpiX;
        Height = _windowHeightPx / dpiY;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        ApplyRoundedCorners();
    }

    private void ApplyRoundedCorners()
    {
        if (_destHwnd == 0)
        {
            return;
        }

        (double dpiX, double dpiY) = GetDpiScale();
        int widthPx = Math.Max(1, (int)Math.Round(ActualWidth * dpiX));
        int heightPx = Math.Max(1, (int)Math.Round(ActualHeight * dpiY));
        int radiusPx = Math.Max(1, CornerRadiusPx);

        
        nint rgn = NativeMethods.CreateRoundRectRgn(0, 0, widthPx + 1, heightPx + 1, radiusPx * 2, radiusPx * 2);
        if (rgn == 0)
        {
            return;
        }

        _ = NativeMethods.SetWindowRgn(_destHwnd, rgn, true);
    }

    private void EnsureThumbnailRegistered(nint preferredSource)
    {
        nint candidate = preferredSource;
        if (candidate == 0)
        {
            candidate = NativeMethods.GetForegroundWindow();
        }

        if (candidate == 0 || candidate == _destHwnd)
        {
            return;
        }

        if (candidate == _sourceHwnd && _thumbnail != 0)
        {
            return;
        }

        
        UnregisterThumbnail();

        int hr = NativeMethods.DwmRegisterThumbnail(_destHwnd, candidate, out _thumbnail);
        if (hr != 0)
        {
            _thumbnail = 0;
            _sourceHwnd = 0;
            return;
        }

        _sourceHwnd = candidate;
    }

    private nint GetTopLevelWindowUnderCursor(NativeMethods.POINT cursorScreenPx)
    {
        
        
        nint hwnd = NativeMethods.WindowFromPoint(cursorScreenPx);
        if (hwnd == 0)
        {
            return 0;
        }

        if (hwnd == _destHwnd)
        {
            
            nint candidate = _destHwnd;
            for (int i = 0; i < 256; i++)
            {
                candidate = NativeMethods.GetWindow(candidate, NativeMethods.GW_HWNDNEXT);
                if (candidate == 0)
                {
                    hwnd = 0;
                    break;
                }

                if (candidate == _destHwnd)
                {
                    continue;
                }

                if (!NativeMethods.IsWindowVisible(candidate))
                {
                    continue;
                }

                if (!NativeMethods.GetWindowRect(candidate, out NativeMethods.RECT rect))
                {
                    continue;
                }

                if (cursorScreenPx.X < rect.Left || cursorScreenPx.X >= rect.Right
                    || cursorScreenPx.Y < rect.Top || cursorScreenPx.Y >= rect.Bottom)
                {
                    continue;
                }

                hwnd = candidate;
                break;
            }

            if (hwnd == 0)
            {
                return 0;
            }
        }

        
        nint root = NativeMethods.GetAncestor(hwnd, NativeMethods.GA_ROOT);
        if (root != 0)
        {
            hwnd = root;
        }

        if (hwnd == 0 || hwnd == _destHwnd)
        {
            return 0;
        }

        return hwnd;
    }

    private void UnregisterThumbnail()
    {
        if (_thumbnail != 0)
        {
            _ = NativeMethods.DwmUnregisterThumbnail(_thumbnail);
            _thumbnail = 0;
        }

        _sourceHwnd = 0;
    }

    private (double dpiX, double dpiY) GetDpiScale()
    {
        PresentationSource? source = PresentationSource.FromVisual(this);
        if (source?.CompositionTarget is null)
        {
            return (1.0, 1.0);
        }

        return (source.CompositionTarget.TransformToDevice.M11, source.CompositionTarget.TransformToDevice.M22);
    }
}
