param([string]$Video, [string]$Times, [string]$Out)
# Hoja de fotogramas completos de un video vertical
Add-Type -AssemblyName System.Drawing
$s = Split-Path $MyInvocation.MyCommand.Path
$tmp = Join-Path $s ("sv_" + [IO.Path]::GetFileNameWithoutExtension($Out))
if (Test-Path $tmp) { Get-ChildItem $tmp -Filter *.jpg | ForEach-Object { [IO.File]::Delete($_.FullName) } }
& "$s\framegrab\FrameGrab.exe" $Video $tmp 270 480 $Times | Out-Null
$files = Get-ChildItem $tmp -Filter *.jpg | Sort-Object Name
$bmp = New-Object System.Drawing.Bitmap((270 * $files.Count), 480)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$font = New-Object System.Drawing.Font("Segoe UI", 12, [System.Drawing.FontStyle]::Bold)
for ($i = 0; $i -lt $files.Count; $i++) {
    $img = [System.Drawing.Image]::FromFile($files[$i].FullName)
    $g.DrawImage($img, $i * 270, 0, 270, 480); $img.Dispose()
    $g.FillRectangle([System.Drawing.Brushes]::Black, $i * 270, 456, 60, 24)
    $g.DrawString($files[$i].BaseName.Substring(2) + "s", $font, [System.Drawing.Brushes]::Yellow, $i * 270 + 3, 457)
}
$bmp.Save($Out, [System.Drawing.Imaging.ImageFormat]::Jpeg); $g.Dispose(); $bmp.Dispose()
$Out
