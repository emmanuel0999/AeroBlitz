# Cabecera de la ficha (1024x500, PNG de 24 bits) para la versión 1.1.0: el Fénix contra el Emperador Galáctico (jefe del nivel 70).
# Jefe grande y central, nave dorada en primer plano, láseres y abanico de disparos; titular con énfasis en "ALL 7 BOSSES".
# El centro queda despejado: Google pone ahí el botón de reproducir del tráiler.
Add-Type -AssemblyName System.Drawing
$ErrorActionPreference = 'Stop'
$out = "D:\Unity\AeroBlitz\StoreAssets\portadas_v17"
$bossHull = "D:\Temp\bosscap\bossart\boss_hull_emperor.png"; $bossLights = "D:\Temp\bosscap\bossart\boss_lights_emperor.png"
$shipFile = "D:\Temp\storecap\ships\phoenix_metal.png"
$logoFile = "D:\Unity\AeroBlitz\Assets\AeroBlitz\Resources\UI\logo.png"
New-Item -ItemType Directory -Force $out | Out-Null
$BW = 1024; $BH = 500

function C($a, $r, $g, $b) { [System.Drawing.Color]::FromArgb($a, $r, $g, $b) }
function P($x, $y) { New-Object System.Drawing.PointF([single]$x, [single]$y) }
$famHead = New-Object System.Drawing.FontFamily("Segoe UI Black")

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

function Text-Path($text, $fam, $style, $size) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddString($text, $fam, [int]$style, [single]$size, (P 0 0), [System.Drawing.StringFormat]::GenericTypographic)
    return $p
}

# Texto centrado que no pasa de maxW; relleno liso o degradado y borde oscuro con sombra
function Draw-Text($g, $text, $fam, $style, $maxSize, $maxW, $cx, $cy, $top, $bottom, $edgeW, $edge) {
    $p = Text-Path $text $fam $style 100
    $size = [Math]::Min([double]$maxSize, 100.0 * $maxW / [Math]::Max(1.0, [double]$p.GetBounds().Width)); $p.Dispose()
    $p = Text-Path $text $fam $style $size
    $b = $p.GetBounds()
    $ref = Text-Path "H" $fam $style $size; $rb = $ref.GetBounds(); $ref.Dispose()
    $m = New-Object System.Drawing.Drawing2D.Matrix
    $m.Translate([single]($cx - $b.X - $b.Width / 2), [single]($cy - $rb.Y - $rb.Height / 2)); $p.Transform($m)
    $sh = $p.Clone(); $m2 = New-Object System.Drawing.Drawing2D.Matrix; $m2.Translate(0, [single]($size * .07)); $sh.Transform($m2)
    $pen = New-Object System.Drawing.Pen((C 160 0 0 10), [single]($edgeW + 6)); $pen.LineJoin = 'Round'; $g.DrawPath($pen, $sh); $pen.Dispose(); $sh.Dispose()
    $pen = New-Object System.Drawing.Pen($edge, [single]$edgeW); $pen.LineJoin = 'Round'; $g.DrawPath($pen, $p); $pen.Dispose()
    $bb = $p.GetBounds()
    if ($bottom) {
        $rect = New-Object System.Drawing.RectangleF($bb.X, ($bb.Y - 1), $bb.Width, ($bb.Height + 2))
        $br = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, $top, $bottom, [single]90)
    } else { $br = New-Object System.Drawing.SolidBrush($top) }
    $g.FillPath($br, $p); $br.Dispose(); $p.Dispose()
}

# El Fénix al ataque del icono (explosión detrás, estelas, contorno azul hielo y disparos de plasma)
function Draw-Phoenix($g, $cx, $cy, $size, $ang) {
    $dx = [Math]::Sin($ang * [Math]::PI / 180); $dy = -[Math]::Cos($ang * [Math]::PI / 180); $px = -$dy; $py = $dx
    $k = $size / 680.0
    $rnd = New-Object System.Random(9)
    foreach ($j in -3..3) {
        $off = ($j * 62 + ($rnd.NextDouble() - .5) * 30) * $k
        $len = (300 + $rnd.Next(220)) * $k; $back = (300 + $rnd.Next(140)) * $k
        $sx = $cx + $px * $off - $dx * $back; $sy = $cy + $py * $off - $dy * $back
        $g.TranslateTransform([single]$sx, [single]$sy); $g.RotateTransform([single]$ang)
        $br = New-Object System.Drawing.Drawing2D.LinearGradientBrush((P 0 (-$len / 2)), (P 0 ($len / 2)), (C 150 230 245 255), (C 0 230 245 255))
        $wd = [single]((8 + $rnd.Next(8)) * $k); $g.FillEllipse($br, -$wd / 2, [single](-$len / 2), $wd, [single]$len); $br.Dispose()
        $g.ResetTransform()
    }
    $g.TranslateTransform([single]$cx, [single]$cy); $g.RotateTransform([single]$ang)
    Glow $g 0 ($size * .5) ($size * .34) (C 220 255 150 40)
    $fp = New-Object System.Drawing.Drawing2D.GraphicsPath
    $fp.AddPolygon([System.Drawing.PointF[]]@((P (-$size * .1) ($size * .32)), (P ($size * .1) ($size * .32)), (P 0 ($size * .95))))
    $br = New-Object System.Drawing.Drawing2D.LinearGradientBrush((P 0 ($size * .32)), (P 0 ($size * .95)), (C 255 255 250 200), (C 0 255 90 20))
    $g.FillPath($br, $fp); $br.Dispose(); $fp.Dispose()
    $img = [System.Drawing.Image]::FromFile($shipFile)
    $cm = New-Object System.Drawing.Imaging.ColorMatrix
    $cm.Matrix00 = 0; $cm.Matrix11 = 0; $cm.Matrix22 = 0; $cm.Matrix33 = 1; $cm.Matrix40 = 0.78; $cm.Matrix41 = 0.93; $cm.Matrix42 = 1.0
    $ia = New-Object System.Drawing.Imaging.ImageAttributes; $ia.SetColorMatrix($cm)
    $half = $size / 2; $rim = 13 * $k
    foreach ($t in 0..23) {
        $a = $t * [Math]::PI / 12
        $dst = New-Object System.Drawing.Rectangle([int](-$half + [Math]::Cos($a) * $rim), [int](-$half + [Math]::Sin($a) * $rim), [int]$size, [int]$size)
        $g.DrawImage($img, $dst, 0, 0, $img.Width, $img.Height, [System.Drawing.GraphicsUnit]::Pixel, $ia)
    }
    $cb = New-Object System.Drawing.Imaging.ColorMatrix
    $cb.Matrix00 = 1.2; $cb.Matrix11 = 1.16; $cb.Matrix22 = 1.1; $cb.Matrix33 = 1; $cb.Matrix40 = 0.03; $cb.Matrix41 = 0.03; $cb.Matrix42 = 0.03
    $ib = New-Object System.Drawing.Imaging.ImageAttributes; $ib.SetColorMatrix($cb)
    $dst = New-Object System.Drawing.Rectangle([int](-$half), [int](-$half), [int]$size, [int]$size)
    $g.DrawImage($img, $dst, 0, 0, $img.Width, $img.Height, [System.Drawing.GraphicsUnit]::Pixel, $ib)
    $img.Dispose(); $ia.Dispose(); $ib.Dispose()
    $g.ResetTransform()
    $nx = $cx + $dx * $size * .44; $ny = $cy + $dy * $size * .44
    foreach ($j in -1..1) {
        $ahead = (108 - [Math]::Abs($j) * 22) * $k
        $bx = $nx + $dx * $ahead + $px * $j * 104 * $k; $by = $ny + $dy * $ahead + $py * $j * 104 * $k
        $g.TranslateTransform([single]$bx, [single]$by); $g.RotateTransform([single]$ang)
        Glow $g 0 0 (150 * $k) (C 190 255 130 30)
        $b = New-Object System.Drawing.SolidBrush((C 255 255 172 60)); $g.FillEllipse($b, [single](-29 * $k), [single](-108 * $k), [single](58 * $k), [single](216 * $k)); $b.Dispose()
        $b = New-Object System.Drawing.SolidBrush((C 240 255 255 245)); $g.FillEllipse($b, [single](-12 * $k), [single](-84 * $k), [single](24 * $k), [single](168 * $k)); $b.Dispose()
        $g.ResetTransform()
    }
}

# Asteroide como los del juego: polígono irregular marrón con cráteres y algo de luz por arriba
function Draw-Asteroid($g, $cx, $cy, $r, $seed, $rot) {
    $rnd = New-Object System.Random($seed)
    $pts = @(); $n = 11
    for ($i = 0; $i -lt $n; $i++) {
        $a = $i / $n * 2 * [Math]::PI + $rot
        $rr = $r * (0.78 + $rnd.NextDouble() * 0.22)
        $pts += P ($cx + [Math]::Cos($a) * $rr) ($cy + [Math]::Sin($a) * $rr)
    }
    $poly = [System.Drawing.PointF[]]$pts
    $rect = New-Object System.Drawing.RectangleF([single]($cx - $r), [single]($cy - $r), [single](2 * $r), [single](2 * $r))
    $br = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, (C 255 205 176 150), (C 255 118 94 78), [single]60)
    $g.FillPolygon($br, $poly); $br.Dispose()
    for ($i = 0; $i -lt 4; $i++) {
        $kx = $cx + ($rnd.NextDouble() - .5) * $r; $ky = $cy + ($rnd.NextDouble() - .5) * $r; $kr = $r * (0.14 + $rnd.NextDouble() * 0.16)
        $b = New-Object System.Drawing.SolidBrush((C 255 96 76 64)); $g.FillEllipse($b, [single]($kx - $kr), [single]($ky - $kr), [single](2 * $kr), [single](2 * $kr)); $b.Dispose()
    }
    $pen = New-Object System.Drawing.Pen((C 160 30 20 22), [single]2.5); $g.DrawPolygon($pen, $poly); $pen.Dispose()
}

# Impacto: destello naranja y trozos de roca y chispas saliendo hacia fuera
function Draw-Impact($g, $x, $y, $r, $seed, $dirA) {
    Glow $g $x $y ($r * 2.2) (C 190 255 140 40)
    Fill-Poly $g (Star $x $y $r ($r * .5) 9 $seed) (C 255 255 150 40)
    Fill-Poly $g (Star $x $y ($r * .6) ($r * .3) 8 ($seed + 1)) (C 255 255 235 160)
    $rnd = New-Object System.Random($seed)
    for ($i = 0; $i -lt 16; $i++) {
        $a = $dirA + ($rnd.NextDouble() - .5) * 2.6; $d = $r * (1.1 + $rnd.NextDouble() * 1.6)
        $fx = $x + [Math]::Cos($a) * $d; $fy = $y + [Math]::Sin($a) * $d
        if ($i % 3 -eq 0) {
            $q = [single](3 + $rnd.Next(4)); $b = New-Object System.Drawing.SolidBrush((C 255 255 (180 + $rnd.Next(60)) 80))
        } else {
            $q = [single](5 + $rnd.Next(8)); $b = New-Object System.Drawing.SolidBrush((C 255 (110 + $rnd.Next(50)) (86 + $rnd.Next(30)) (66 + $rnd.Next(20))))
        }
        $g.FillRectangle($b, [single]($fx - $q / 2), [single]($fy - $q / 2), $q, $q); $b.Dispose()
    }
}

# Disparo de plasma suelto (misma forma que los del morro)
function Draw-Bolt($g, $x, $y, $ang, $k, $alpha) {
    $g.TranslateTransform([single]$x, [single]$y); $g.RotateTransform([single]$ang)
    Glow $g 0 0 (150 * $k) (C ([int](190 * $alpha)) 255 130 30)
    $b = New-Object System.Drawing.SolidBrush((C ([int](255 * $alpha)) 255 172 60)); $g.FillEllipse($b, [single](-29 * $k), [single](-108 * $k), [single](58 * $k), [single](216 * $k)); $b.Dispose()
    $b = New-Object System.Drawing.SolidBrush((C ([int](240 * $alpha)) 255 255 245)); $g.FillEllipse($b, [single](-12 * $k), [single](-84 * $k), [single](24 * $k), [single](168 * $k)); $b.Dispose()
    $g.ResetTransform()
}


# Jefe tal como se ve en el juego: casco gris teñido con su tono y encima las luces (ya con su color)
function Draw-Boss($g, $cx, $cy, $size) {
    Glow $g $cx ($cy + $size * .05) ($size * .62) (C 120 255 40 60)
    $hull = [System.Drawing.Image]::FromFile($bossHull); $lights = [System.Drawing.Image]::FromFile($bossLights)
    $cm = New-Object System.Drawing.Imaging.ColorMatrix
    $cm.Matrix00 = 1.0; $cm.Matrix11 = 0.82; $cm.Matrix22 = 0.5; $cm.Matrix33 = 1
    $ia = New-Object System.Drawing.Imaging.ImageAttributes; $ia.SetColorMatrix($cm)
    $dst = New-Object System.Drawing.Rectangle([int]($cx - $size / 2), [int]($cy - $size / 2), [int]$size, [int]$size)
    $g.DrawImage($hull, $dst, 0, 0, $hull.Width, $hull.Height, [System.Drawing.GraphicsUnit]::Pixel, $ia)
    $g.DrawImage($lights, $dst, 0, 0, $lights.Width, $lights.Height, [System.Drawing.GraphicsUnit]::Pixel)
    $hull.Dispose(); $lights.Dispose(); $ia.Dispose()
}

# Disparo del jefe: centro blanco, anillo rojo y contorno oscuro
function Draw-Orb($g, $x, $y, $r) {
    Glow $g $x $y ($r * 2.4) (C 150 255 60 70)
    $b = New-Object System.Drawing.SolidBrush((C 255 70 14 22)); $g.FillEllipse($b, [single]($x - $r), [single]($y - $r), [single](2 * $r), [single](2 * $r)); $b.Dispose()
    $b = New-Object System.Drawing.SolidBrush((C 255 255 90 100)); $g.FillEllipse($b, [single]($x - $r * .82), [single]($y - $r * .82), [single](1.64 * $r), [single](1.64 * $r)); $b.Dispose()
    $b = New-Object System.Drawing.SolidBrush((C 255 255 235 235)); $g.FillEllipse($b, [single]($x - $r * .5), [single]($y - $r * .5), [single]($r), [single]($r)); $b.Dispose()
}

# Láser del jefe: haz rojo con núcleo blanco que aparece desde arriba
function Draw-Laser($g, $x, $y0, $y1) {
    $rect = New-Object System.Drawing.RectangleF([single]($x - 16), [single]$y0, [single]32, [single]($y1 - $y0))
    $br = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, (C 0 255 50 70), (C 190 255 50 70), [single]90)
    $g.FillRectangle($br, $rect); $br.Dispose()
    $rect2 = New-Object System.Drawing.RectangleF([single]($x - 4), [single]$y0, [single]8, [single]($y1 - $y0))
    $br = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect2, (C 0 255 235 235), (C 245 255 235 235), [single]90)
    $g.FillRectangle($br, $rect2); $br.Dispose()
    Glow $g $x $y1 34 (C 150 255 80 90)
}

$langs = @(
    @("en", "CAN YOU BEAT", "ALL 7", "BOSSES?"),
    @("es", "¿PUEDES VENCER A", "LOS 7", "JEFES?"),
    @("fr", "SAUREZ-VOUS VAINCRE", "LES 7", "BOSS ?")
)
foreach ($lang in $langs) {
    $bmp = New-Object System.Drawing.Bitmap($BW, $BH, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'; $g.PixelOffsetMode = 'HighQuality'; $g.CompositingQuality = 'HighQuality'
    # Espacio azul, más claro detrás del logo
    $g.Clear((C 255 3 7 28))
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddEllipse([single](720 - 760), [single](210 - 760), [single]1520, [single]1520)
    $pb = New-Object System.Drawing.Drawing2D.PathGradientBrush($path)
    $pb.CenterPoint = P 720 210; $pb.CenterColor = (C 255 38 88 196); $pb.SurroundColors = [System.Drawing.Color[]]@((C 255 3 7 28))
    $g.FillRectangle($pb, 0, 0, $BW, $BH); $pb.Dispose(); $path.Dispose()
    Glow $g 900 90 260 (C 70 140 90 255)
    Glow $g 760 430 330 (C 90 120 50 230)   # nebulosa morada abajo a la derecha
    Glow $g 960 250 260 (C 70 40 110 255)   # y azul eléctrico a la derecha
    $rnd = New-Object System.Random(5)
    for ($i = 0; $i -lt 110; $i++) {
        $st = [single](1.5 + $rnd.NextDouble() * 3.5); $al = 80 + $rnd.Next(170)
        $b = New-Object System.Drawing.SolidBrush((C $al 225 238 255)); $g.FillEllipse($b, [single]$rnd.Next($BW), [single]$rnd.Next($BH), $st, $st); $b.Dispose()
    }
    # Resplandor rojo y rayos de luz detrás del jefe
    Glow $g 300 235 470 (C 120 175 30 60)
    $g.TranslateTransform(300, 235)
    for ($i = 0; $i -lt 14; $i++) {
        $g.RotateTransform([single]25.7)
        $ray = [System.Drawing.PointF[]]@((P 0 0), (P -26 -660), (P 26 -660))
        $rb = New-Object System.Drawing.Drawing2D.LinearGradientBrush((P 0 0), (P 0 -660), (C ($(if ($i % 2 -eq 0) { 46 } else { 24 })) 255 120 90), (C 0 255 120 90))
        $g.FillPolygon($rb, $ray); $rb.Dispose()
    }
    $g.ResetTransform()
    $rnd = New-Object System.Random(7)
    for ($i = 0; $i -lt 40; $i++) {
        $st = [single](1.5 + $rnd.NextDouble() * 3); $al = 90 + $rnd.Next(160)
        $b = New-Object System.Drawing.SolidBrush((C $al 255 225 225)); $g.FillEllipse($b, [single]$rnd.Next(620), [single]$rnd.Next(500), $st, $st); $b.Dispose()
    }
    # El Emperador Galáctico, enorme: unos 600 px (casi el 40 % de la imagen), a la izquierda del centro
    Draw-Boss $g 300 235 600
    # Láseres desde las alas inferiores
    Draw-Laser $g 112 330 112 500
    Draw-Laser $g 488 330 488 500
    # Anillo denso de proyectiles que sale del jefe (se omiten los que caen sobre la nave)
    $rr = New-Object System.Random(11)
    foreach ($deg in 8..172) {
        if (($deg - 8) % 11 -ne 0) { continue }
        $a = $deg * [Math]::PI / 180
        $ox = 300 + 262 * [Math]::Cos($a); $oy = 235 + 262 * [Math]::Sin($a)
        if ($ox -gt 135 -and $ox -lt 385 -and $oy -gt 330) { continue }
        Draw-Orb $g $ox $oy (12 + $rr.Next(5))
    }
    # Anillo interior, más pequeño: da densidad al ataque
    foreach ($deg in 14..166) {
        if (($deg - 14) % 13 -ne 0) { continue }
        $a = $deg * [Math]::PI / 180
        Draw-Orb $g (300 + 178 * [Math]::Cos($a)) (235 + 178 * [Math]::Sin($a)) (9 + $rr.Next(4))
    }
    # Segunda tanda más lejos y dispersa
    foreach ($o in @(@(570, 330, 12), @(600, 392, 15), @(560, 452, 13), @(470, 476, 12), @(410, 330, 11), @(36, 372, 13), @(70, 470, 12), @(24, 292, 10), @(330, 484, 10), @(596, 282, 10))) {
        Draw-Orb $g $o[0] $o[1] $o[2]
    }
    # El Fénix dorado en primer plano, pegado al jefe, completo
    Draw-Phoenix $g 240 380 240 (-8)
    $k = 240 / 680.0; $dx = [Math]::Sin(-8 * [Math]::PI / 180); $dy = -[Math]::Cos(-8 * [Math]::PI / 180); $px = -$dy; $py = $dx
    $nx = 240 + $dx * 240 * .44; $ny = 380 + $dy * 240 * .44
    foreach ($j in -1, 0, 1) {
        $bx = $nx + $dx * 100 + $px * $j * 104 * $k; $by = $ny + $dy * 100 + $py * $j * 104 * $k
        Draw-Bolt $g $bx $by (-8) ($k * 0.9) 0.9
    }
    Draw-Bolt $g ($nx + $dx * 190) ($ny + $dy * 190) (-8) ($k * 0.8) 0.7
    Draw-Impact $g 268 222 30 41 (-1.7)

    # Logo en la parte alta de la derecha (algo más pequeño)
    Glow $g 815 80 210 (C 150 2 6 24)
    $logo = [System.Drawing.Image]::FromFile($logoFile)
    $lw = 330; $lh = [int]($lw * $logo.Height / $logo.Width)
    $g.DrawImage($logo, [single](815 - $lw / 2), [single]22, [single]$lw, [single]$lh); $logo.Dispose()
    # Titular: línea pequeña blanca y debajo, enorme y en naranja, "ALL 7 / BOSSES?"
    Draw-Text $g $lang[1] $famHead 2 44 380 815 208 (C 255 255 255 255) $null 8 (C 255 6 12 40)
    Glow $g 815 345 270 (C 140 255 90 20)
    Draw-Text $g $lang[2] $famHead 3 138 330 810 285 (C 255 255 232 96) (C 255 255 112 14) 13 (C 255 30 10 4)
    Draw-Text $g $lang[3] $famHead 3 124 376 810 392 (C 255 255 232 96) (C 255 255 112 14) 13 (C 255 30 10 4)
    $g.Dispose()
    $bmp.Save("$out\cabecera_$($lang[0]).png", [System.Drawing.Imaging.ImageFormat]::Png); $bmp.Dispose()
    "  cabecera_$($lang[0]).png"
}

# Vista previa con el botón de reproducir que Google pone encima cuando hay tráiler
$prev = New-Object System.Drawing.Bitmap(1064, 1620)
$g = [System.Drawing.Graphics]::FromImage($prev); $g.SmoothingMode = 'AntiAlias'; $g.Clear((C 255 245 246 248))
$font = New-Object System.Drawing.Font("Segoe UI Semibold", 15)
$y = 20
foreach ($lang in $langs) {
    $img = [System.Drawing.Image]::FromFile("$out\cabecera_$($lang[0]).png"); $g.DrawImage($img, 20, $y, 1024, 500); $img.Dispose()
    $b = New-Object System.Drawing.SolidBrush((C 150 0 0 0)); $g.FillEllipse($b, 482, ($y + 200), 100, 100); $b.Dispose()
    $g.FillPolygon([System.Drawing.Brushes]::White, [System.Drawing.PointF[]]@((P 520 ($y + 228)), (P 520 ($y + 272)), (P 556 ($y + 250))))
    $g.DrawString("$($lang[0]) (con el botón del tráiler encima)", $font, [System.Drawing.Brushes]::Black, 20, ($y + 504))
    $y += 540
}
$prev.Save("$out\vista_previa.png", [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $prev.Dispose()
"listo"
