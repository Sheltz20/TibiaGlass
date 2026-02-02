using System.Diagnostics;
using System.Runtime.InteropServices;
using TibiaGlassMagnifier.Interop;

namespace TibiaGlassMagnifier;

internal sealed class KeyboardHook : IDisposable
{
    private nint _hookHandle;
    private NativeMethods.LowLevelKeyboardProc? _proc;
    private bool _hotkeyActive;
    private bool _altDown;

    private bool _ctrlDown;
    private bool _shiftDown;
    private bool _winDown;
    private bool _mainKeyDown;

    private HotkeySettings _hotkey = HotkeySettings.DefaultAltHold;

    public event EventHandler? HotkeyDown;
    public event EventHandler? HotkeyUp;

    public void SetHotkey(HotkeySettings hotkey)
    {
        _hotkey = hotkey ?? HotkeySettings.DefaultAltHold;
        
        _hotkeyActive = false;
        _mainKeyDown = false;
    }

    public void Start()
    {
        if (_hookHandle != 0)
        {
            return;
        }

        _proc = HookCallback;

        
        using Process currentProcess = Process.GetCurrentProcess();
        using ProcessModule? currentModule = currentProcess.MainModule;
        nint moduleHandle = NativeMethods.GetModuleHandle(currentModule?.ModuleName);

        _hookHandle = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, _proc, moduleHandle, 0);
        if (_hookHandle == 0)
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    public void Dispose()
    {
        if (_hookHandle != 0)
        {
            NativeMethods.UnhookWindowsHookEx(_hookHandle);
            _hookHandle = 0;
        }

        _proc = null;
    }

    private nint HookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0)
        {
            int msg = unchecked((int)wParam);
            if (msg is NativeMethods.WM_KEYDOWN or NativeMethods.WM_SYSKEYDOWN or NativeMethods.WM_KEYUP or NativeMethods.WM_SYSKEYUP)
            {
                NativeMethods.KBDLLHOOKSTRUCT data = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
                bool isDownMsg = msg is NativeMethods.WM_KEYDOWN or NativeMethods.WM_SYSKEYDOWN;
                bool isUpMsg = msg is NativeMethods.WM_KEYUP or NativeMethods.WM_SYSKEYUP;

                
                if (data.vkCode is NativeMethods.VK_MENU or NativeMethods.VK_LMENU or NativeMethods.VK_RMENU)
                {
                    if (isDownMsg) _altDown = true;
                    if (isUpMsg) _altDown = false;
                }

                
                if (data.vkCode is 0x11 or 0xA2 or 0xA3)
                {
                    if (isDownMsg) _ctrlDown = true;
                    if (isUpMsg) _ctrlDown = false;
                }

                
                if (data.vkCode is 0x10 or 0xA0 or 0xA1)
                {
                    if (isDownMsg) _shiftDown = true;
                    if (isUpMsg) _shiftDown = false;
                }

                
                if (data.vkCode is 0x5B or 0x5C)
                {
                    if (isDownMsg) _winDown = true;
                    if (isUpMsg) _winDown = false;
                }

                
                if (_hotkey.KeyVk != 0 && data.vkCode == (uint)_hotkey.KeyVk)
                {
                    if (isDownMsg) _mainKeyDown = true;
                    if (isUpMsg) _mainKeyDown = false;
                }

                bool modsOk = (!_hotkey.Alt || _altDown)
                    && (!_hotkey.Ctrl || _ctrlDown)
                    && (!_hotkey.Shift || _shiftDown)
                    && (!_hotkey.Win || _winDown);

                bool keyOk = _hotkey.KeyVk == 0 ? true : _mainKeyDown;

                bool nowActive = modsOk && keyOk;
                if (nowActive != _hotkeyActive)
                {
                    _hotkeyActive = nowActive;
                    if (_hotkeyActive)
                    {
                        HotkeyDown?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        HotkeyUp?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }
}
