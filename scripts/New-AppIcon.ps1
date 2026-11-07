# Rebuild all icon assets from the editable SVG master (absolute M/L/C/Z paths).
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$assetDirectory = Join-Path (Split-Path $PSScriptRoot -Parent) 'src/Dowlume.App/Assets'
[xml]$svg = Get-Content -LiteralPath (Join-Path $assetDirectory 'dowlume.svg') -Raw
function Render-Icon([int]$Size) {
    $canvas = New-Object System.Drawing.Bitmap(1024, 1024)
    $graphics = [System.Drawing.Graphics]::FromImage($canvas)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.ScaleTransform(4, 4)
    try {
        foreach ($shape in $svg.svg.path) {
            $path = New-Object System.Drawing.Drawing2D.GraphicsPath
            $path.FillMode = [System.Drawing.Drawing2D.FillMode]::Alternate
            $brush = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml([string]$shape.fill))
            try {
                $tokens = [regex]::Matches([string]$shape.d, '[MLCZ]|-?\d+(?:\.\d+)?') | ForEach-Object { $_.Value }
                $index = 0
                [single]$x = 0; [single]$y = 0
                while ($index -lt $tokens.Count) {
                    $command = $tokens[$index++]
                    switch ($command) {
                        'M' {
                            $path.StartFigure()
                            $x = [single]::Parse($tokens[$index++], [Globalization.CultureInfo]::InvariantCulture)
                            $y = [single]::Parse($tokens[$index++], [Globalization.CultureInfo]::InvariantCulture)
                        }
                        'L' {
                            $nextX = [single]::Parse($tokens[$index++], [Globalization.CultureInfo]::InvariantCulture)
                            $nextY = [single]::Parse($tokens[$index++], [Globalization.CultureInfo]::InvariantCulture)
                            $path.AddLine($x, $y, $nextX, $nextY)
                            $x = $nextX; $y = $nextY
                        }
                        'C' {
                            $points = @()
                            for ($point = 0; $point -lt 6; $point++) {
                                $points += [single]::Parse($tokens[$index++], [Globalization.CultureInfo]::InvariantCulture)
                            }
                            $path.AddBezier($x, $y, $points[0], $points[1], $points[2], $points[3], $points[4], $points[5])
                            $x = $points[4]; $y = $points[5]
                        }
                        'Z' { $path.CloseFigure() }
                        default { throw "Unsupported icon path command: $command" }
                    }
                }
                $graphics.FillPath($brush, $path)
            } finally { $brush.Dispose(); $path.Dispose() }
        }
        $output = New-Object System.Drawing.Bitmap($Size, $Size)
        $scaled = [System.Drawing.Graphics]::FromImage($output)
        try {
            $scaled.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $scaled.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $scaled.DrawImage($canvas, 0, 0, $Size, $Size)
        } finally { $scaled.Dispose() }
        return $output
    } finally { $graphics.Dispose(); $canvas.Dispose() }
}
$large = Render-Icon 1024
try { $large.Save((Join-Path $assetDirectory 'dowlume.png'), [System.Drawing.Imaging.ImageFormat]::Png) }
finally { $large.Dispose() }
$linux = Render-Icon 512
try { $linux.Save((Join-Path $assetDirectory 'dowlume-512.png'), [System.Drawing.Imaging.ImageFormat]::Png) }
finally { $linux.Dispose() }
$sizes = @(16, 20, 24, 32, 48, 64, 128, 256)
$images = @()
foreach ($size in $sizes) {
    $bitmap = Render-Icon $size
    $stream = New-Object System.IO.MemoryStream
    try {
        $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
        $images += ,$stream.ToArray()
    } finally { $stream.Dispose(); $bitmap.Dispose() }
}
$file = [IO.File]::Create((Join-Path $assetDirectory 'dowlume.ico'))
$writer = New-Object System.IO.BinaryWriter($file)
try {
    $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$sizes.Count)
    $offset = 6 + 16 * $sizes.Count
    for ($index = 0; $index -lt $sizes.Count; $index++) {
        $dimension = if ($sizes[$index] -eq 256) { 0 } else { $sizes[$index] }
        $writer.Write([byte]$dimension); $writer.Write([byte]$dimension)
        $writer.Write([byte]0); $writer.Write([byte]0)
        $writer.Write([uint16]1); $writer.Write([uint16]32)
        $writer.Write([uint32]$images[$index].Length); $writer.Write([uint32]$offset)
        $offset += $images[$index].Length
    }
    foreach ($bytes in $images) { $writer.Write([byte[]]$bytes) }
} finally { $writer.Dispose(); $file.Dispose() }
