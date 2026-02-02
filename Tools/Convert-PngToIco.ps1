param(
    [Parameter(Mandatory = $true)]
    [string]$PngPath,

    [Parameter(Mandatory = $true)]
    [string]$IcoPath
)

if (-not (Test-Path -LiteralPath $PngPath)) {
    throw "PNG not found: $PngPath"
}

Add-Type -AssemblyName System.Drawing

$sizes = @(16, 20, 24, 32, 40, 48, 64, 96, 128, 256)

$img = [System.Drawing.Image]::FromFile($PngPath)
try {
    $entries = @()
    foreach ($s in $sizes) {
        $bmp = New-Object System.Drawing.Bitmap $s, $s, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            $g = [System.Drawing.Graphics]::FromImage($bmp)
            try {
                $g.Clear([System.Drawing.Color]::Transparent)
                $g.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceOver
                $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
                $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
                $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
                $g.DrawImage($img, 0, 0, $s, $s)
            } finally {
                $g.Dispose()
            }

            $pngMs = New-Object System.IO.MemoryStream
            $bmp.Save($pngMs, [System.Drawing.Imaging.ImageFormat]::Png)
            $pngBytes = $pngMs.ToArray()
            $pngMs.Dispose()

            $entries += [PSCustomObject]@{ Size = $s; Bytes = $pngBytes }
        } finally {
            $bmp.Dispose()
        }
    }

    $ms = New-Object System.IO.MemoryStream
    $bw = New-Object System.IO.BinaryWriter($ms)

    # ICONDIR
    $bw.Write([UInt16]0)   # reserved
    $bw.Write([UInt16]1)   # type = icon
    $bw.Write([UInt16]$entries.Count)   # count

    $imageOffset = 6 + (16 * $entries.Count)
    $offset = $imageOffset

    # Directory entries
    foreach ($e in $entries) {
        $s = [int]$e.Size
        $w = if ($s -ge 256) { 0 } else { [byte]$s }
        $h = if ($s -ge 256) { 0 } else { [byte]$s }

        $bw.Write([Byte]$w)      # width
        $bw.Write([Byte]$h)      # height
        $bw.Write([Byte]0)       # color count
        $bw.Write([Byte]0)       # reserved
        $bw.Write([UInt16]1)     # planes
        $bw.Write([UInt16]32)    # bit count
        $bw.Write([UInt32]$e.Bytes.Length) # bytes in resource
        $bw.Write([UInt32]$offset)         # image offset

        $offset += $e.Bytes.Length
    }

    # Image data
    foreach ($e in $entries) {
        $bw.Write($e.Bytes)
    }

    $bw.Flush()
    [System.IO.File]::WriteAllBytes($IcoPath, $ms.ToArray())
    Write-Host "Wrote: $IcoPath"
}
finally {
    $img.Dispose()
}