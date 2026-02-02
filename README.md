# TibiaGlass Magnifier

A lightweight tray app that shows a magnifier window while you **hold Alt**.

- Runs from the **system tray** (no main window by default)
- While **hotkey is held**: shows a magnifier window above the cursor
- When hotkey is released: hides immediately

## Run

From the workspace folder:

- `dotnet run --project TibiaGlassMagnifier.csproj -c Release`

You should see a tray icon named **TibiaGlass Magnifier**.

## How it works

- A global **low-level keyboard hook** detects your selected hotkey down/up.
- A borderless, click-through WPF window follows the cursor.
- The window displays a DWM thumbnail of the foreground window, zoomed to the area around the cursor (~30fps).

## Customize

Edit constants in [UI/MagnifierWindow.xaml.cs](UI/MagnifierWindow.xaml.cs):

- `WindowWidthPx` / `WindowHeightPx` (magnifier window size)
- `Zoom` (2.0 = 2x zoom)
- `CursorYOffsetPx` (distance above cursor)

## Tray / EXE icon

The app looks for `Assets/TibiaGlass.ico`.

- Tray icon: loaded at runtime from the output folder.
- EXE icon: set via the project `ApplicationIcon` property.


## Notes

- The magnifier window is **click-through** so it won’t block Tibia clicks.
