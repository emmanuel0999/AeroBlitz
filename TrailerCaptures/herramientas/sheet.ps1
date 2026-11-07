param([string]$Video, [string]$Times, [string]$Out, [int]$Cols = 6)
# Hoja de contactos: recorta la parte del juego (centro del video horizontal) de varios fotogramas
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$s = Split-Path $MyInvocation.MyCommand.Path
$tmp = Join-Path $s ("sheet_" + [IO.Path]::GetFileNameWithoutExtension($Out))
if (Test-Path $tmp) { Get-ChildItem $tmp -Filter *.jpg | ForEach-Object { [IO.File]::Delete($_.FullName) } }
& "$s\framegrab\FrameGrab.exe" $Video $tmp 960 540 $Times | Out-Null
$files = Get-ChildItem $tmp -Filter *.jpg | Sort-Object Name
$cw = 304; $ch = 540; $rows = [Math]::Ceiling($files.Count / $Cols)
$bmp = New-Object System.Drawing.Bitmap(($cw * $Cols), ($ch * $rows))
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.Clear([System.Drawing.Color]::Black)
$font = New-Object System.Drawing.Font("Segoe UI", 14, [System.Drawing.FontStyle]::Bold)
for ($i = 0; $i -lt $files.Count; $i++) {
    $img = [System.Drawing.Image]::FromFile($files[$i].FullName)
    $x = ($i % $Cols) * $cw; $y = [Math]::Floor($i / $Cols) * $ch
    $g.DrawImage($img, (New-Object System.Drawing.Rectangle($x, $y, $cw, $ch)), (New-Object System.Drawing.Rectangle(328, 0, $cw, $ch)), [System.Drawing.GraphicsUnit]::Pixel)
    $img.Dispose()
    $label = $files[$i].BaseName.Substring(2) + " s"
    $g.FillRectangle([System.Drawing.Brushes]::Black, $x, $y + $ch - 26, 70, 26)
    $g.DrawString($label, $font, [System.Drawing.Brushes]::Yellow, $x + 4, $y + $ch - 26)
}
$bmp.Save($Out, [System.Drawing.Imaging.ImageFormat]::Jpeg)
$g.Dispose(); $bmp.Dispose()
$Out
