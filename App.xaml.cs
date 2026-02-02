using System.ComponentModel;
using System.Windows;
using System.Reflection;

namespace TibiaGlassMagnifier;




public partial class App : System.Windows.Application
{
	private TrayIconService? _tray;
	private KeyboardHook? _hook;
	private MagnifierController? _magnifier;
	private SettingsService? _settingsService;
	private AppSettings? _settings;
	private SettingsWindow? _settingsWindow;

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		
		
		TryPreloadWpfTelemetryDependencies();

		
		ShutdownMode = ShutdownMode.OnExplicitShutdown;

		_settingsService = new SettingsService();
		_settings = _settingsService.Load();

		_magnifier = new MagnifierController();
		_magnifier.SetZoom(_settings.Zoom);
		_magnifier.SetOutputSize(_settings.WindowWidthPx, _settings.WindowHeightPx);

		_hook = new KeyboardHook();
		_hook.SetHotkey(_settings.Hotkey);
		_hook.HotkeyDown += (_, _) =>
		{
			_magnifier.Show();
		};
		_hook.HotkeyUp += (_, _) => _magnifier.Hide();
		_hook.Start();

		_tray = new TrayIconService();
		_tray.SettingsRequested += (_, _) => ShowSettings();
		_tray.ExitRequested += (_, _) => Shutdown();
		_tray.Initialize();
		_tray.ShowBalloon("TibiaGlass", "Running in the system tray. Hold Alt to magnify.");
	}

	private static void TryPreloadWpfTelemetryDependencies()
	{
		try
		{
			_ = Assembly.Load(new AssemblyName("System.Diagnostics.Tracing"));
		}
		catch
		{
			
		}
	}

	private void ShowSettings()
	{
		if (_settingsService is null)
		{
			return;
		}

		
		_settings = _settingsService.Load();

		if (_settingsWindow is not null)
		{
			_settingsWindow.Activate();
			return;
		}

		_settingsWindow = new SettingsWindow(_settings);
		_settingsWindow.Saved += (_, updated) =>
		{
			_settings = updated;
			_settingsService.Save(_settings);
			_hook?.SetHotkey(_settings.Hotkey);
			_magnifier?.SetZoom(_settings.Zoom);
			_magnifier?.SetOutputSize(_settings.WindowWidthPx, _settings.WindowHeightPx);
		};
		_settingsWindow.Closed += (_, _) => _settingsWindow = null;
		_settingsWindow.Show();
		_settingsWindow.Activate();
	}

	protected override void OnExit(ExitEventArgs e)
	{
		_tray?.Dispose();
		_hook?.Dispose();
		_magnifier?.Dispose();
		base.OnExit(e);
	}

	protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
	{
		
		try
		{
			_tray?.Dispose();
			_hook?.Dispose();
			_magnifier?.Dispose();
		}
		catch (Win32Exception)
		{
			
		}
		base.OnSessionEnding(e);
	}
}

