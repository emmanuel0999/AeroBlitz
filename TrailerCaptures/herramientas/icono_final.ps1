# Icono definitivo de la versión 1.0.3 (idea A "Fénix al ataque", pulida): 1024 para el juego y 512 para la ficha
Add-Type -AssemblyName System.Drawing
$ErrorActionPreference = 'Stop'
$out = "D:\Unity\AeroBlitz\StoreAssets\iconos_v15"
$shipFile = "D:\Temp\storecap\ships\phoenix_metal.png"
New-Item -ItemType Directory -Force $out | Out-Null
$SZ = 1024

function C($a, $r, $g, $b) { [System.Drawing.Color]::FromArgb($a, $r, $g, $b) }
function P($x, $y) { New-Object System.Drawing.PointF([single]$x, [single]$y) }

function Glow($g, $cx, $cy, $r, $color) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddEllipse([single]($cx - $r), [single]($cy - $r), [single](2 * $r), [single](2 * $r))
    $pb = New-Object System.Drawing.Drawing2D.PathGradientBrush($path)
    $pb.CenterColor = $color
    $pb.SurroundColors = [System.Drawing.Color[]]@((C 0 $color.R $color.G $color.B))
    $pb.SetSigmaBellShape([single]1, [single]1)
    $g.FillPath($pb, $path); $pb.Dispose(); $path.Dispose()
}

function Star($cx, $cy, $r1, $r2, $n, $seed) {
    $rnd = New-Object System.Random($seed); $pts = @()
    for ($i = 0; $i -lt 2 * $n; $i++) {
        $a = $i * [Math]::PI / $n + ($rnd.NextDouble() - .5) * .16
        $r = if ($i % 2 -eq 0) { $r1 * (.72 + $rnd.NextDouble() * .4) } else { $r2 * (.85 + $rnd.NextDouble() * .3) }
        $pts += P ($cx + $r * [Math]::Cos($a)) ($cy + $r * [Math]::Sin($a))
    }
    return ,([System.Drawing.PointF[]]$pts)
}
function Fill-Poly($g, $pts, $color) { $b = New-Object System.Drawing.SolidBrush($color); $g.FillPolygon($b, $pts); $b.Dispose() }

# Dirección de vuelo: 35° hacia arriba a la derecha
$ang = 35.0
$dx = [Math]::Sin($ang * [Math]::PI / 180); $dy = -[Math]::Cos($ang * [Math]::PI / 180)
$px = -$dy; $py = $dx   # perpendicular

$bmp = New-Object System.Drawing.Bitmap($SZ, $SZ, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'; $g.PixelOffsetMode = 'HighQuality'; $g.CompositingQuality = 'HighQuality'

# Fondo: espacio azul profundo, más claro arriba a la derecha (hacia donde vuela)
$g.Clear((C 255 3 7 28))
$path = New-Object System.Drawing.Drawing2D.GraphicsPath
$path.AddEllipse([single](660 - 950), [single](330 - 950), [single]1900, [single]1900)
$pb = New-Object System.Drawing.Drawing2D.PathGradientBrush($path)
$pb.CenterPoint = P 660 330; $pb.CenterColor = (C 255 40 92 200); $pb.SurroundColors = [System.Drawing.Color[]]@((C 255 3 7 28))
$g.FillRectangle($pb, 0, 0, $SZ, $SZ); $pb.Dispose(); $path.Dispose()
Glow $g 820 180 330 (C 90 120 200 255)
$rnd = New-Object System.Random(5)
for ($i = 0; $i -lt 80; $i++) {
    $st = [single](2 + $rnd.NextDouble() * 5); $al = 80 + $rnd.Next(170)
    $b = New-Object System.Drawing.SolidBrush((C $al 225 238 255)); $g.FillEllipse($b, [single]$rnd.Next($SZ), [single]$rnd.Next($SZ), $st, $st); $b.Dispose()
}

# Explosión detrás de la cola (abajo a la izquierda)
$ex = 300; $ey = 760; $er = 400
Glow $g $ex $ey ($er * 1.45) (C 170 255 110 25)
Fill-Poly $g (Star $ex $ey $er ($er * .55) 13 21) (C 255 232 78 20)
Fill-Poly $g (Star $ex $ey ($er * .78) ($er * .43) 13 22) (C 255 255 148 30)
Fill-Poly $g (Star $ex $ey ($er * .58) ($er * .32) 12 23) (C 255 255 222 110)
Glow $g $ex $ey ($er * .42) (C 230 255 255 235)
# Trozos de roca saliendo de la explosión
$rnd = New-Object System.Random(3)
for ($i = 0; $i -lt 16; $i++) {
    $a = -0.2 - $rnd.NextDouble() * 2.6; $d = $er * (.75 + $rnd.NextDouble() * .45)
    $q = [single](16 + $rnd.Next(26))
    $b = New-Object System.Drawing.SolidBrush((C 255 (40 + $rnd.Next(40)) (18 + $rnd.Next(20)) (28 + $rnd.Next(20))))
    $g.FillRectangle($b, [single]($ex + [Math]::Cos($a) * $d), [single]($ey + [Math]::Sin($a) * $d), $q, $q); $b.Dispose()
}

# Estelas de velocidad detrás de la nave
$cx = 505; $cy = 548; $size = 680
$rnd = New-Object System.Random(9)
foreach ($k in -3..3) {
    $off = $k * 62 + ($rnd.NextDouble() - .5) * 30
    $len = 300 + $rnd.Next(220); $back = 300 + $rnd.Next(140)
    $sx = $cx + $px * $off - $dx * $back; $sy = $cy + $py * $off - $dy * $back
    $g.TranslateTransform([single]$sx, [single]$sy); $g.RotateTransform([single]$ang)
    $br = New-Object System.Drawing.Drawing2D.LinearGradientBrush((P 0 (-$len / 2)), (P 0 ($len / 2)), (C 150 230 245 255), (C 0 230 245 255))
    $wd = [single](8 + $rnd.Next(8)); $g.FillEllipse($br, -$wd / 2, [single](-$len / 2), $wd, [single]$len); $br.Dispose()
    $g.ResetTransform()
}

# Nave: fuego, aro claro para que destaque sobre la explosión, y la nave real del juego (Fénix metálico)
$g.TranslateTransform([single]$cx, [single]$cy); $g.RotateTransform([single]$ang)
Glow $g 0 ($size * .5) ($size * .34) (C 220 255 150 40)
$fp = New-Object System.Drawing.Drawing2D.GraphicsPath
$fp.AddPolygon([System.Drawing.PointF[]]@((P (-$size * .1) ($size * .32)), (P ($size * .1) ($size * .32)), (P 0 ($size * .95))))
$br = New-Object System.Drawing.Drawing2D.LinearGradientBrush((P 0 ($size * .32)), (P 0 ($size * .95)), (C 255 255 250 200), (C 0 255 90 20))
$g.FillPath($br, $fp); $br.Dispose(); $fp.Dispose()
$img = [System.Drawing.Image]::FromFile($shipFile)
# Silueta clara (azul hielo) alrededor de la nave
$cm = New-Object System.Drawing.Imaging.ColorMatrix
$cm.Matrix00 = 0; $cm.Matrix11 = 0; $cm.Matrix22 = 0; $cm.Matrix33 = 1
$cm.Matrix40 = 0.78; $cm.Matrix41 = 0.93; $cm.Matrix42 = 1.0
$ia = New-Object System.Drawing.Imaging.ImageAttributes; $ia.SetColorMatrix($cm)
$half = $size / 2
foreach ($t in 0..23) {
    $a = $t * [Math]::PI / 12; $ox = [Math]::Cos($a) * 13; $oy = [Math]::Sin($a) * 13
    $dst = New-Object System.Drawing.Rectangle([int](-$half + $ox), [int](-$half + $oy), [int]$size, [int]$size)
    $g.DrawImage($img, $dst, 0, 0, $img.Width, $img.Height, [System.Drawing.GraphicsUnit]::Pixel, $ia)
}
# La nave con algo más de brillo para que el cobre no se vea apagado
$cb = New-Object System.Drawing.Imaging.ColorMatrix
$cb.Matrix00 = 1.2; $cb.Matrix11 = 1.16; $cb.Matrix22 = 1.1; $cb.Matrix33 = 1; $cb.Matrix40 = 0.03; $cb.Matrix41 = 0.03; $cb.Matrix42 = 0.03
$ib = New-Object System.Drawing.Imaging.ImageAttributes; $ib.SetColorMatrix($cb)
$dst = New-Object System.Drawing.Rectangle([int](-$half), [int](-$half), [int]$size, [int]$size)
$g.DrawImage($img, $dst, 0, 0, $img.Width, $img.Height, [System.Drawing.GraphicsUnit]::Pixel, $ib)
$img.Dispose(); $ia.Dispose(); $ib.Dispose()
$g.ResetTransform()

# Disparos de plasma por delante del morro
$nx = $cx + $dx * $size * .44; $ny = $cy + $dy * $size * .44
foreach ($k in -1..1) {
    $ahead = 108 - [Math]::Abs($k) * 22
    $bx = $nx + $dx * $ahead + $px * $k * 104; $by = $ny + $dy * $ahead + $py * $k * 104
    $g.TranslateTransform([single]$bx, [single]$by); $g.RotateTransform([single]$ang)
    Glow $g 0 0 150 (C 190 255 130 30)
    $b = New-Object System.Drawing.SolidBrush((C 255 255 172 60)); $g.FillEllipse($b, [single]-29, [single]-108, [single]58, [single]216); $b.Dispose()
    $b = New-Object System.Drawing.SolidBrush((C 240 255 255 245)); $g.FillEllipse($b, [single]-12, [single]-84, [single]24, [single]168); $b.Dispose()
    $g.ResetTransform()
}
$g.Dispose()

$bmp.Save("$out\icono_final_1024.png", [System.Drawing.Imaging.ImageFormat]::Png)
$small = New-Object System.Drawing.Bitmap(512, 512, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($small); $g.InterpolationMode = 'HighQualityBicubic'; $g.PixelOffsetMode = 'HighQuality'
$g.DrawImage($bmp, 0, 0, 512, 512); $g.Dispose()
$small.Save("$out\icono_final_512.png", [System.Drawing.Imaging.ImageFormat]::Png)

# Vista previa: forma de Google Play, círculo (algunos teléfonos) y tamaños pequeños, junto al actual
$prev = New-Object System.Drawing.Bitmap(1180, 420)
$g = [System.Drawing.Graphics]::FromImage($prev); $g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'
$g.Clear((C 255 245 246 248))
$font = New-Object System.Drawing.Font("Segoe UI Semibold", 15)
function Rounded($g, $img, $x, $y, $s, $r) {
    $rp = New-Object System.Drawing.Drawing2D.GraphicsPath; $d = 2 * $r
    $rp.AddArc($x, $y, $d, $d, 180, 90); $rp.AddArc($x + $s - $d, $y, $d, $d, 270, 90); $rp.AddArc($x + $s - $d, $y + $s - $d, $d, $d, 0, 90); $rp.AddArc($x, $y + $s - $d, $d, $d, 90, 90); $rp.CloseFigure()
    $g.SetClip($rp); $g.DrawImage($img, $x, $y, $s, $s); $g.ResetClip(); $rp.Dispose()
}
$old = [System.Drawing.Image]::FromFile("D:\Unity\AeroBlitz\Assets\AeroBlitz\Icon\icon_1024.png")
Rounded $g $bmp 30 30 300 60; $g.DrawString("Nuevo (ficha)", $font, [System.Drawing.Brushes]::Black, 30, 340)
$cp = New-Object System.Drawing.Drawing2D.GraphicsPath; $cp.AddEllipse(370, 60, 240, 240); $g.SetClip($cp); $g.DrawImage($bmp, 370, 60, 240, 240); $g.ResetClip()
$g.DrawString("Nuevo (teléfono redondo)", $font, [System.Drawing.Brushes]::Black, 360, 340)
Rounded $g $bmp 660 120 96 20; Rounded $g $bmp 780 136 64 14; Rounded $g $bmp 870 148 40 9
$g.DrawString("Tamaños pequeños", $font, [System.Drawing.Brushes]::Black, 660, 340)
Rounded $g $old 960 120 96 20; Rounded $g $old 1080 136 64 14
$g.DrawString("Actual", $font, [System.Drawing.Brushes]::Black, 990, 340)
$old.Dispose(); $g.Dispose()
$prev.Save("$out\icono_final_vista.png", [System.Drawing.Imaging.ImageFormat]::Png)
$prev.Dispose(); $small.Dispose(); $bmp.Dispose()
"listo"
