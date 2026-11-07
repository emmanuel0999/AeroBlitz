# Tres propuestas de icono para la ficha de Google Play (512x512, sin texto y a sangre)
Add-Type -AssemblyName System.Drawing
$ErrorActionPreference = 'Stop'
$dir = "D:\Unity\AeroBlitz\StoreAssets\iconos_nuevos"
New-Item -ItemType Directory -Force $dir | Out-Null

function C($a, $r, $g, $b) { [System.Drawing.Color]::FromArgb($a, $r, $g, $b) }
function P($x, $y) { New-Object System.Drawing.PointF([single]$x, [single]$y) }

# Forma real de la nave inicial del juego (Halcón): con morro largo se lee como nave, no como estrella
$hull = @(@(0,1.2), @(.35,.1), @(.9,-.5), @(.35,-.35), @(.25,-.8), @(-.25,-.8), @(-.35,-.35), @(-.9,-.5), @(-.35,.1))
$hullRight = @(@(0,1.2), @(.35,.1), @(.9,-.5), @(.35,-.35), @(.25,-.8), @(0,-.8))
$canopy = @(@(0,.62), @(.15,.16), @(.11,-.2), @(-.11,-.2), @(-.15,.16))

function Xf($shape, $cx, $cy, $s, $deg) {
    $r = $deg * [Math]::PI / 180; $c = [Math]::Cos($r); $n = [Math]::Sin($r)
    $out = foreach ($q in $shape) { P ($cx + $s * ($q[0] * $c + $q[1] * $n)) ($cy - $s * (-$q[0] * $n + $q[1] * $c)) }
    return ,([System.Drawing.PointF[]]$out)
}

function New-Icon {
    $bmp = New-Object System.Drawing.Bitmap(2048, 2048, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.CompositingQuality = 'HighQuality'; $g.PixelOffsetMode = 'HighQuality'
    $g.ScaleTransform(2, 2)   # se dibuja en un espacio de 1024
    return ,@($bmp, $g)
}

function Fill-Radial($g, $cx, $cy, $colors, $stops) {
    $g.Clear($colors[0])
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddEllipse([single]($cx - 900), [single]($cy - 900), [single]1800, [single]1800)
    $pb = New-Object System.Drawing.Drawing2D.PathGradientBrush($path)
    $pb.CenterPoint = P $cx $cy
    $blend = New-Object System.Drawing.Drawing2D.ColorBlend($colors.Count)
    $blend.Colors = [System.Drawing.Color[]]$colors
    $blend.Positions = [single[]]$stops
    $pb.InterpolationColors = $blend
    $g.FillRectangle($pb, 0, 0, 1024, 1024); $pb.Dispose(); $path.Dispose()
}

function Glow($g, $cx, $cy, $r, $color) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddEllipse([single]($cx - $r), [single]($cy - $r), [single](2 * $r), [single](2 * $r))
    $pb = New-Object System.Drawing.Drawing2D.PathGradientBrush($path)
    $pb.CenterColor = $color
    $pb.SurroundColors = [System.Drawing.Color[]]@((C 0 $color.R $color.G $color.B))
    $pb.SetSigmaBellShape([single]1, [single]1)
    $g.FillPath($pb, $path); $pb.Dispose(); $path.Dispose()
}

function Star($cx, $cy, $r1, $r2, $n, $seed, $sx = 1) {
    $rnd = New-Object System.Random($seed); $pts = @()
    for ($i = 0; $i -lt 2 * $n; $i++) {
        $a = $i * [Math]::PI / $n + ($rnd.NextDouble() - .5) * .16
        $r = if ($i % 2 -eq 0) { $r1 * (.72 + $rnd.NextDouble() * .4) } else { $r2 * (.85 + $rnd.NextDouble() * .3) }
        $pts += P ($cx + $sx * $r * [Math]::Cos($a)) ($cy + $r * [Math]::Sin($a))
    }
    return ,([System.Drawing.PointF[]]$pts)
}

function Fill-Poly($g, $pts, $color) { $b = New-Object System.Drawing.SolidBrush($color); $g.FillPolygon($b, $pts); $b.Dispose() }

function Draw-Flame($g, $cx, $cy, $s, $deg, $len = 1.75) {
    Fill-Poly $g (Xf @(@(-.22,-.76), @(.22,-.76), @(0,(-1 * $len))) $cx $cy $s $deg) (C 255 255 98 20)
    Fill-Poly $g (Xf @(@(-.12,-.76), @(.12,-.76), @(0,(-1 * $len * .8))) $cx $cy $s $deg) (C 255 255 226 120)
}

function Draw-Ship($g, $cx, $cy, $s, $deg, $main, $edge) {
    $pts = Xf $hull $cx $cy $s $deg
    $pen = New-Object System.Drawing.Pen((C 255 8 12 40), [single]($s * .11)); $pen.LineJoin = 'Round'
    $g.DrawPolygon($pen, $pts); $pen.Dispose()
    $lg = New-Object System.Drawing.Drawing2D.LinearGradientBrush((P ($cx - $s) ($cy - $s)), (P ($cx + $s) ($cy + $s)), (C 255 255 255 255), $main)
    $g.FillPolygon($lg, $pts); $lg.Dispose()
    # Media nave en sombra: da volumen aunque el icono se vea pequeño
    Fill-Poly $g (Xf $hullRight $cx $cy $s $deg) $edge
    $cp = Xf $canopy $cx $cy $s $deg
    $lg = New-Object System.Drawing.Drawing2D.LinearGradientBrush((P ($cx - $s * .2) ($cy - $s * .6)), (P ($cx + $s * .2) ($cy + $s * .3)), (C 255 120 240 255), (C 255 8 40 120))
    $g.FillPolygon($lg, $cp); $lg.Dispose()
}

function Save-Icon($bmp, $g, $name) {
    $g.Dispose()
    $out = New-Object System.Drawing.Bitmap(512, 512, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $h = [System.Drawing.Graphics]::FromImage($out)
    $h.InterpolationMode = 'HighQualityBicubic'; $h.PixelOffsetMode = 'HighQuality'; $h.CompositingQuality = 'HighQuality'
    $h.DrawImage($bmp, 0, 0, 512, 512)
    $out.Save("$dir\$name", [System.Drawing.Imaging.ImageFormat]::Png)
    if ($name -eq "opcion_a2_disparos.png") {   # icono elegido: también el del lanzador del móvil
        $big = New-Object System.Drawing.Bitmap(1024, 1024, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $b2 = [System.Drawing.Graphics]::FromImage($big); $b2.InterpolationMode = 'HighQualityBicubic'; $b2.PixelOffsetMode = 'HighQuality'
        $b2.DrawImage($bmp, 0, 0, 1024, 1024); $big.Save("D:\Unity\AeroBlitz\Assets\AeroBlitz\Icon\icon_1024.png", [System.Drawing.Imaging.ImageFormat]::Png); $b2.Dispose(); $big.Dispose()
    }
    $h.Dispose(); $out.Dispose(); $bmp.Dispose()
    "  $name"
}

# ---------- A: la nave escapa de una gran explosión ----------
$c = New-Icon; $bmp = $c[0]; $g = $c[1]
Fill-Radial $g 400 660 @((C 255 62 4 44), (C 255 176 20 34), (C 255 255 104 20), (C 255 255 214 70)) @(0, .5, .76, 1)
Fill-Poly $g (Star 400 660 600 310 13 3) (C 255 255 130 24)
Fill-Poly $g (Star 400 660 440 230 13 8) (C 255 255 188 40)
Fill-Poly $g (Star 400 660 280 150 11 5) (C 255 255 244 170)
$rnd = New-Object System.Random(4)
for ($i = 0; $i -lt 14; $i++) {
    $a = $rnd.NextDouble() * 2 * [Math]::PI; $d = 300 + $rnd.NextDouble() * 220; $z = 14 + $rnd.NextDouble() * 24
    Fill-Poly $g (Star (400 + $d * [Math]::Cos($a)) (660 + $d * [Math]::Sin($a)) $z ($z * .55) 3 ($i + 10)) (C 255 60 8 30)
}
Draw-Flame $g 560 470 300 38 1.75
Draw-Ship $g 560 470 300 38 (C 255 88 218 255) (C 70 8 60 170)
Save-Icon $bmp $g "opcion_a_explosion.png"

# ---------- A2: como la A, pero la nave dispara mientras escapa ----------
# Rayos paralelos en el sistema de la nave: siguen su dirección y no se amontonan en el morro
function Draw-Bolt($g, $bx, $y0, $y1, $sx, $sy, $ss, $deg) {
    $w = .085
    $shape = @(@(($bx - .03), $y0), @(($bx + .03), $y0), @(($bx + $w), ($y1 - .2)), @($bx, $y1), @(($bx - $w), ($y1 - .2)))
    $pts = Xf $shape $sx $sy $ss $deg
    $pen = New-Object System.Drawing.Pen((C 255 6 60 140), [single]24); $pen.LineJoin = 'Round'; $g.DrawPolygon($pen, $pts); $pen.Dispose()
    $pen = New-Object System.Drawing.Pen((C 255 40 214 255), [single]13); $pen.LineJoin = 'Round'; $g.DrawPolygon($pen, $pts); $pen.Dispose()
    Fill-Poly $g $pts (C 255 255 255 255)
}
$c = New-Icon; $bmp = $c[0]; $g = $c[1]
Fill-Radial $g 330 720 @((C 255 62 4 44), (C 255 176 20 34), (C 255 255 104 20), (C 255 255 214 70)) @(0, .5, .76, 1)
Fill-Poly $g (Star 330 720 600 310 13 3) (C 255 255 130 24)
Fill-Poly $g (Star 330 720 440 230 13 8) (C 255 255 188 40)
Fill-Poly $g (Star 330 720 280 150 11 5) (C 255 255 244 170)
$rnd = New-Object System.Random(4)
for ($i = 0; $i -lt 14; $i++) {
    $a = $rnd.NextDouble() * 2 * [Math]::PI; $d = 300 + $rnd.NextDouble() * 220; $z = 14 + $rnd.NextDouble() * 24
    Fill-Poly $g (Star (330 + $d * [Math]::Cos($a)) (720 + $d * [Math]::Sin($a)) $z ($z * .55) 3 ($i + 10)) (C 255 60 8 30)
}
$sx = 460; $sy = 590; $ss = 262; $deg = 38
$nx = $sx + $ss * 1.2 * [Math]::Sin($deg * [Math]::PI / 180); $ny = $sy - $ss * 1.2 * [Math]::Cos($deg * [Math]::PI / 180)
Draw-Bolt $g 0 1.45 2.42 $sx $sy $ss $deg; Draw-Bolt $g .55 .4 1.72 $sx $sy $ss $deg; Draw-Bolt $g (-0.55) .4 1.72 $sx $sy $ss $deg
Draw-Flame $g $sx $sy $ss $deg 1.75
Draw-Ship $g $sx $sy $ss $deg (C 255 88 218 255) (C 70 8 60 170)
Save-Icon $bmp $g "opcion_a2_disparos.png"


# ---------- B: la nave rompe un muro indestructible (elemento propio del juego) ----------
$c = New-Icon; $bmp = $c[0]; $g = $c[1]
Fill-Radial $g 512 470 @((C 255 8 8 44), (C 255 20 16 96), (C 255 52 40 176), (C 255 90 110 250)) @(0, .5, .8, 1)
$wall = New-Object System.Drawing.Rectangle(0, 420, 1024, 250)
$st = $g.Save(); $g.SetClip($wall)
$g.Clear((C 255 16 16 24))
for ($x = -340; $x -lt 1240; $x += 240) {
    Fill-Poly $g ([System.Drawing.PointF[]]@((P $x 420), (P ($x + 120) 420), (P ($x + 370) 670), (P ($x + 250) 670))) (C 255 255 205 20)
}
$g.Restore($st)
$pen = New-Object System.Drawing.Pen((C 255 232 236 250), [single]16)
$g.DrawLine($pen, 0, 420, 1024, 420); $g.DrawLine($pen, 0, 670, 1024, 670); $pen.Dispose()
Fill-Poly $g (Star 512 540 250 190 12 21) (C 255 255 120 20)
Fill-Poly $g (Star 512 540 190 140 12 22) (C 255 255 214 80)
$rnd = New-Object System.Random(7)
for ($i = 0; $i -lt 14; $i++) {
    $a = -[Math]::PI * (.08 + $rnd.NextDouble() * .84); $d = 270 + $rnd.NextDouble() * 200
    $x = 512 + $d * [Math]::Cos($a); $y = 470 + $d * .8 * [Math]::Sin($a); $z = 24 + $rnd.NextDouble() * 26
    $col = if ($i % 2 -eq 0) { C 255 255 205 20 } else { C 255 16 16 24 }
    Fill-Poly $g (Star $x $y $z ($z * .7) 2 ($i + 40)) $col
}
Draw-Flame $g 512 500 300 0 1.65
Draw-Ship $g 512 500 300 0 (C 255 88 218 255) (C 70 8 60 170)
Save-Icon $bmp $g "opcion_b_muro.png"

# ---------- C: ráfaga de cañón de plasma en abanico ----------
$c = New-Icon; $bmp = $c[0]; $g = $c[1]
Fill-Radial $g 512 520 @((C 255 18 4 50), (C 255 60 8 120), (C 255 130 16 170), (C 255 220 40 190)) @(0, .5, .8, 1)
foreach ($deg in -38, -19, 0, 19, 38) {
    $st = $g.Save()
    $g.TranslateTransform([single]512, [single]520); $g.RotateTransform([single]$deg)
    $beam = [System.Drawing.PointF[]]@((P -8 0), (P 8 0), (P 26 -560), (P 0 -660), (P -26 -560))
    $pen = New-Object System.Drawing.Pen((C 255 255 30 170), [single]26); $pen.LineJoin = 'Round'
    $g.DrawPolygon($pen, $beam); $pen.Dispose()
    Fill-Poly $g $beam (C 255 255 244 255)
    $g.Restore($st)
}
Draw-Flame $g 512 700 270 0 1.6
Draw-Ship $g 512 700 270 0 (C 255 88 218 255) (C 70 8 60 170)
Save-Icon $bmp $g "opcion_c_plasma.png"

# ---------- Comparación: como se vería en la tienda (esquinas redondeadas y tamaños pequeños) ----------
function Rounded($g, $img, $x, $y, $s) {
    $r = $s * .2; $d = 2 * $r
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddArc([single]$x, [single]$y, [single]$d, [single]$d, 180, 90)
    $p.AddArc([single]($x + $s - $d), [single]$y, [single]$d, [single]$d, 270, 90)
    $p.AddArc([single]($x + $s - $d), [single]($y + $s - $d), [single]$d, [single]$d, 0, 90)
    $p.AddArc([single]$x, [single]($y + $s - $d), [single]$d, [single]$d, 90, 90)
    $p.CloseFigure()
    $g.SetClip($p); $g.DrawImage($img, [single]$x, [single]$y, [single]$s, [single]$s); $g.ResetClip(); $p.Dispose()
}
$files = @(@("Actual", "D:\Unity\AeroBlitz\StoreAssets\icono_512.png"), @("A", "$dir\opcion_a_explosion.png"), @("A con disparos", "$dir\opcion_a2_disparos.png"))
$sheet = New-Object System.Drawing.Bitmap(1320, 620)
$g = [System.Drawing.Graphics]::FromImage($sheet)
$g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'; $g.TextRenderingHint = 'AntiAlias'
$g.Clear((C 255 255 255 255))
$dark = New-Object System.Drawing.SolidBrush((C 255 32 33 36)); $g.FillRectangle($dark, 0, 340, 1320, 280); $dark.Dispose()
$font = New-Object System.Drawing.Font("Segoe UI", 22, [System.Drawing.FontStyle]::Bold)
for ($i = 0; $i -lt $files.Count; $i++) {
    $img = [System.Drawing.Image]::FromFile($files[$i][1]); $x = 30 + $i * 325
    $g.DrawString($files[$i][0], $font, [System.Drawing.Brushes]::Black, [single]$x, 8)
    Rounded $g $img $x 56 256
    Rounded $g $img $x 370 128
    Rounded $g $img ($x + 148) 370 72
    Rounded $g $img ($x + 148) 458 48
    $img.Dispose()
}
$sheet.Save("$dir\comparacion.png", [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $sheet.Dispose()
"  comparacion.png"
