using System.Runtime.InteropServices;

namespace TibiaGlassMagnifier.Interop;

internal static class NativeMethods
{
    internal const int WH_KEYBOARD_LL = 13;

    internal const int WM_KEYDOWN = 0x0100;
    internal const int WM_KEYUP = 0x0101;
    internal const int WM_SYSKEYDOWN = 0x0104;
    internal const int WM_SYSKEYUP = 0x0105;

    internal const int VK_MENU = 0x12; 
    internal const int VK_LMENU = 0xA4;
    internal const int VK_RMENU = 0xA5;

    internal const int GWL_EXSTYLE = -20;
    internal const int WS_EX_TRANSPARENT = 0x00000020;
    internal const int WS_EX_TOOLWINDOW = 0x00000080;
    internal const int WS_EX_NOACTIVATE = 0x08000000;
    internal const int WS_EX_LAYERED = 0x00080000;

    internal const int SM_XVIRTUALSCREEN = 76;
    internal const int SM_YVIRTUALSCREEN = 77;
    internal const int SM_CXVIRTUALSCREEN = 78;
    internal const int SM_CYVIRTUALSCREEN = 79;

    internal const uint GA_ROOT = 2;

    internal const uint GW_HWNDNEXT = 2;

    internal const uint CWP_SKIPINVISIBLE = 0x0001;
    internal const uint CWP_SKIPDISABLED = 0x0002;
    internal const uint CWP_SKIPTRANSPARENT = 0x0004;

    
    internal const int DWM_TNP_RECTDESTINATION = 0x00000001;
    internal const int DWM_TNP_RECTSOURCE = 0x00000002;
    internal const int DWM_TNP_OPACITY = 0x00000004;
    internal const int DWM_TNP_VISIBLE = 0x00000008;
    internal const int DWM_TNP_SOURCECLIENTAREAONLY = 0x00000010;

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public readonly int Width => Right - Left;
        public readonly int Height => Bottom - Top;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public nuint dwExtraInfo;
    }

    internal delegate nint LowLevelKeyboardProc(int nCode, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern nint SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern nint GetModuleHandle(string? lpModuleName);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    internal static extern nint GetForegroundWindow();

    [DllImport("user32.dll")]
    internal static extern nint WindowFromPoint(POINT Point);

    [DllImport("user32.dll")]
    internal static extern nint ChildWindowFromPointEx(nint hWndParent, POINT pt, uint uFlags);

    [DllImport("user32.dll")]
    internal static extern nint GetAncestor(nint hwnd, uint gaFlags);

    [DllImport("user32.dll")]
    internal static extern nint GetWindow(nint hWnd, uint uCmd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IsWindowVisible(nint hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool GetClientRect(nint hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool ScreenToClient(nint hWnd, ref POINT lpPoint);

    [DllImport("user32.dll")]
    internal static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern nint GetWindowLongPtr(nint hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("gdi32.dll", SetLastError = true)]
    internal static extern bool DeleteObject(nint hObject);

    [DllImport("gdi32.dll")]
    internal static extern nint CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    [DllImport("user32.dll")]
    internal static extern int SetWindowRgn(nint hWnd, nint hRgn, bool bRedraw);

    [StructLayout(LayoutKind.Sequential)]
    internal struct DWM_THUMBNAIL_PROPERTIES
    {
        public int dwFlags;
        public RECT rcDestination;
        public RECT rcSource;
        public byte opacity;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fVisible;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fSourceClientAreaOnly;
    }

    [DllImport("dwmapi.dll")]
    internal static extern int DwmRegisterThumbnail(nint hwndDestination, nint hwndSource, out nint phThumbnail);

    [DllImport("dwmapi.dll")]
    internal static extern int DwmUnregisterThumbnail(nint hThumbnail);

    [DllImport("dwmapi.dll")]
    internal static extern int DwmUpdateThumbnailProperties(nint hThumbnail, ref DWM_THUMBNAIL_PROPERTIES ptnProperties);

    [DllImport("dwmapi.dll")]
    internal static extern int DwmQueryThumbnailSourceSize(nint hThumbnail, out POINT pSize);
}
