# Foto de perfil de la cuenta de Google del estudio: ciudad bajo un planeta con anillo y el nombre debajo
$here = Split-Path $MyInvocation.MyCommand.Path
. "$here\rotulos.ps1" | Out-Null
$dir = "D:\Unity\AeroBlitz\StoreAssets\cuenta_google"
New-Item -ItemType Directory -Force $dir | Out-Null
$W = 800
$dy = -70   # la escena sube para dejar sitio al nombre
$c = New-Canvas $W $W; $bmp = $c[0]; $g = $c[1]

# Cielo: degradado radial violeta oscuro y estrellas
$sky = New-Object System.Drawing.Drawing2D.GraphicsPath
$sky.AddEllipse(-300, -300, 1400, 1400)
$pb = New-Object System.Drawing.Drawing2D.PathGradientBrush($sky)
$pb.CenterPoint = New-Object System.Drawing.PointF(400, (330 + $dy))
$pb.CenterColor = C 255 46 26 92
$pb.SurroundColors = [System.Drawing.Color[]]@((C 255 6 6 22))
$g.FillRectangle($pb, 0, 0, $W, $W); $pb.Dispose(); $sky.Dispose()
$rnd = New-Object System.Random(9)
for ($i = 0; $i -lt 140; $i++) {
    $sz = [single](1.5 + $rnd.NextDouble() * 2.5)
    $b = New-Object System.Drawing.SolidBrush((C (90 + $rnd.Next(165)) 255 255 255))
    $g.FillEllipse($b, [single]$rnd.Next($W), [single]$rnd.Next(450), $sz, $sz); $b.Dispose()
}

# Planeta detrás de la ciudad
$px = 400; $py = 400 + $dy; $pr = 185
Draw-Glow $g $px $py ($pr * 1.6) (C 120 255 140 70)
function Draw-Ring($g, $front) {
    $st = $g.Save()
    $g.TranslateTransform([single]400, [single](400 + $dy)); $g.RotateTransform([single]-16)
    if ($front) { $g.SetClip((New-Object System.Drawing.RectangleF(-420, 0, 840, 300))) }
    foreach ($w in @(@(24, 60), @(11, 255))) {
        $pen = New-Object System.Drawing.Pen((C $w[1] 90 225 255), [single]$w[0])
        $g.DrawEllipse($pen, [single]-300, [single]-70, [single]600, [single]140); $pen.Dispose()
    }
    $g.Restore($st)
}
Draw-Ring $g $false
$rect = New-Object System.Drawing.RectangleF(($px - $pr), ($py - $pr), (2 * $pr), (2 * $pr))
$lg = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, (C 255 255 214 90), (C 255 232 70 130), [single]55)
$g.FillEllipse($lg, $rect); $lg.Dispose()
$clip = New-Object System.Drawing.Drawing2D.GraphicsPath; $clip.AddEllipse($rect); $g.SetClip($clip)
foreach ($band in @(@(-100, 24), @(-45, 16), @(45, 28), @(105, 14))) {
    $bb = New-Object System.Drawing.SolidBrush((C 45 255 255 255))
    $g.FillEllipse($bb, [single]160, [single]($py + $band[0]), [single]480, [single]$band[1]); $bb.Dispose()
}
$g.ResetClip()
Draw-Ring $g $true

# Silueta de la ciudad; las ventanas se quedan por encima de la franja del nombre
$cb = New-Object System.Drawing.SolidBrush((C 255 12 12 34))
$win = New-Object System.Drawing.SolidBrush((C 230 255 205 80))
$buildings = @(@(120, 560, 70), @(185, 505, 60), @(240, 470, 55), @(290, 530, 50), @(335, 440, 62), @(392, 495, 46),
    @(432, 455, 58), @(485, 520, 52), @(532, 480, 60), @(588, 540, 55), @(638, 575, 70))
foreach ($bd in $buildings) {
    $x = $bd[0]; $top = $bd[1] + $dy; $wd = $bd[2]
    $g.FillRectangle($cb, [single]$x, [single]$top, [single]$wd, [single](800 - $top))
    for ($wy = $top + 18; $wy -lt 545; $wy += 28) {
        for ($wx = $x + 10; $wx -lt $x + $wd - 12; $wx += 18) {
            if ($rnd.NextDouble() -lt 0.45) { $g.FillRectangle($win, [single]$wx, [single]$wy, [single]8, [single]11) }
        }
    }
}
$g.FillRectangle($cb, [single]363, [single](395 + $dy), [single]6, [single]50)
Draw-Glow $g 366 (392 + $dy) 18 (C 255 255 80 80)
# Franja oscura para el nombre, fundida con la ciudad
$fadeRect = New-Object System.Drawing.RectangleF(0, 540, $W, 60)
$fade = New-Object System.Drawing.Drawing2D.LinearGradientBrush($fadeRect, (C 0 8 8 24), (C 255 8 8 24), [single]90)
$g.FillRectangle($fade, $fadeRect); $fade.Dispose()
$ground = New-Object System.Drawing.SolidBrush((C 255 8 8 24)); $g.FillRectangle($ground, 0, 599, $W, 201); $ground.Dispose()
$cb.Dispose(); $win.Dispose()

# Estrella fugaz
$trail = New-Object System.Drawing.Drawing2D.LinearGradientBrush((New-Object System.Drawing.PointF(470, (250 + $dy))), (New-Object System.Drawing.PointF(600, (160 + $dy))), (C 0 90 225 255), (C 220 140 240 255))
$tp = New-Object System.Drawing.Pen($trail, [single]7); $tp.StartCap = 'Round'; $tp.EndCap = 'Round'
$g.DrawLine($tp, [single]470, [single](250 + $dy), [single]596, [single](163 + $dy)); $tp.Dispose(); $trail.Dispose()
Draw-Glow $g 600 (160 + $dy) 26 (C 200 200 250 255)

# Nombre del estudio (dentro del círculo que muestra Google)
$s = Fit @("FRAILES CITY") $famBlack 96 540
Draw-Text $g "FRAILES CITY" $famBlack $s 400 640 $white $cyan
Draw-Text $g "STUDIOS" $famBold 48 400 722 (C 255 255 200 50) $null

$bmp.Save("$dir\perfil_gmail_800.png", [System.Drawing.Imaging.ImageFormat]::Png)
$prev = New-Object System.Drawing.Bitmap(400, 400)
$pg = [System.Drawing.Graphics]::FromImage($prev); $pg.SmoothingMode = 'AntiAlias'; $pg.InterpolationMode = 'HighQualityBicubic'
$pg.Clear([System.Drawing.Color]::White)
$cp = New-Object System.Drawing.Drawing2D.GraphicsPath; $cp.AddEllipse(0, 0, 400, 400); $pg.SetClip($cp)
$pg.DrawImage($bmp, 0, 0, 400, 400)
$prev.Save("$dir\vista_circulo.png", [System.Drawing.Imaging.ImageFormat]::Png)
# Tamaño real aproximado en la bandeja de Gmail
$small = New-Object System.Drawing.Bitmap(96, 96)
$sg = [System.Drawing.Graphics]::FromImage($small); $sg.SmoothingMode = 'AntiAlias'; $sg.InterpolationMode = 'HighQualityBicubic'
$sg.Clear([System.Drawing.Color]::White)
$cp2 = New-Object System.Drawing.Drawing2D.GraphicsPath; $cp2.AddEllipse(0, 0, 96, 96); $sg.SetClip($cp2)
$sg.DrawImage($bmp, 0, 0, 96, 96)
$small.Save("$dir\vista_pequena.png", [System.Drawing.Imaging.ImageFormat]::Png)
$sg.Dispose(); $small.Dispose(); $pg.Dispose(); $prev.Dispose(); $g.Dispose(); $bmp.Dispose()
"  perfil_gmail_800.png (letra $([int]$s))"
