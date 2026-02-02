Place your tray/app icon files here.

Expected files:
- TibiaGlass.png (your source artwork)
- TibiaGlass.ico (generated from the png)

To generate the .ico (no external tools needed):

  powershell -ExecutionPolicy Bypass -File .\Tools\Convert-PngToIco.ps1 -PngPath .\Assets\TibiaGlass.png -IcoPath .\Assets\TibiaGlass.ico

Then rebuild and run.
