# Cabecera de la ficha (1024x500, PNG de 24 bits) para la versión 1.0.3, con el estilo del icono y de las capturas nuevas.
# El centro queda despejado: Google pone ahí el botón de reproducir del tráiler.
Add-Type -AssemblyName System.Drawing
$ErrorActionPreference = 'Stop'
$out = "D:\Unity\AeroBlitz\StoreAssets\portadas_v15"
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

$langs = @(
    @("en", "CAN YOU BEAT", "ALL 70 LEVELS?"),
    @("es", "¿PUEDES SUPERAR", "LOS 70 NIVELES?"),
    @("fr", "POUVEZ-VOUS FINIR", "LES 70 NIVEAUX ?")
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
    $rnd = New-Object System.Random(5)
    for ($i = 0; $i -lt 110; $i++) {
        $st = [single](1.5 + $rnd.NextDouble() * 3.5); $al = 80 + $rnd.Next(170)
        $b = New-Object System.Drawing.SolidBrush((C $al 225 238 255)); $g.FillEllipse($b, [single]$rnd.Next($BW), [single]$rnd.Next($BH), $st, $st); $b.Dispose()
    }
    # Explosión abajo a la izquierda (sale del borde)
    $ex = 120; $ey = 420; $er = 290
    Glow $g $ex $ey ($er * 1.45) (C 170 255 110 25)
    Fill-Poly $g (Star $ex $ey $er ($er * .55) 13 21) (C 255 232 78 20)
    Fill-Poly $g (Star $ex $ey ($er * .78) ($er * .43) 13 22) (C 255 255 148 30)
    Fill-Poly $g (Star $ex $ey ($er * .58) ($er * .32) 12 23) (C 255 255 222 110)
    Glow $g $ex $ey ($er * .42) (C 230 255 255 235)
    $rnd = New-Object System.Random(3)
    for ($i = 0; $i -lt 14; $i++) {
        $a = -0.15 - $rnd.NextDouble() * 1.7; $d = $er * (.8 + $rnd.NextDouble() * .4); $q = [single](10 + $rnd.Next(16))
        $b = New-Object System.Drawing.SolidBrush((C 255 (40 + $rnd.Next(40)) (18 + $rnd.Next(20)) (28 + $rnd.Next(20))))
        $g.FillRectangle($b, [single]($ex + [Math]::Cos($a) * $d), [single]($ey + [Math]::Sin($a) * $d), $q, $q); $b.Dispose()
    }
    # Asteroides: uno recibiendo el impacto arriba (sale por el borde) y otro cerca de la nave
    Draw-Asteroid $g 506 60 44 7 0.4
    Draw-Asteroid $g 462 428 40 12 1.1
    Draw-Asteroid $g 52 112 24 4 0.0
    # Restos de enemigos ya destruidos alrededor de la nave (sin tocar el centro, donde va el botón del tráiler)
    $rnd = New-Object System.Random(31)
    for ($i = 0; $i -lt 12; $i++) {
        $fx = 60 + $rnd.Next(340); $fy = 40 + $rnd.Next(150); $q = [single](4 + $rnd.Next(7))
        $b = New-Object System.Drawing.SolidBrush((C 220 (120 + $rnd.Next(60)) (90 + $rnd.Next(40)) (70 + $rnd.Next(30)))); $g.FillRectangle($b, [single]$fx, [single]$fy, $q, $q); $b.Dispose()
    }
    Draw-Phoenix $g 250 274 400 35   # un 20 % más grande para que la nave se recuerde
    # Segunda ráfaga más adelantada y el impacto contra el asteroide
    $k = 400 / 680.0; $dx = [Math]::Sin(35 * [Math]::PI / 180); $dy = -[Math]::Cos(35 * [Math]::PI / 180); $px = -$dy; $py = $dx
    $nx = 250 + $dx * 400 * .44; $ny = 274 + $dy * 400 * .44
    foreach ($j in -1, 1) {
        $ahead = 128; $bx = $nx + $dx * $ahead + $px * $j * 104 * $k; $by = $ny + $dy * $ahead + $py * $j * 104 * $k
        Draw-Bolt $g $bx $by 35 ($k * 0.85) 0.8
    }
    Draw-Bolt $g ($nx + $dx * 175) ($ny + $dy * 175) 35 ($k * 0.75) 0.6
    Draw-Impact $g 486 82 26 41 (-0.6)

    # Logo del juego con sombra oscura para que se lea sobre el azul
    Glow $g 772 150 250 (C 150 2 6 24)
    $logo = [System.Drawing.Image]::FromFile($logoFile)
    $lw = 430; $lh = [int]($lw * $logo.Height / $logo.Width)
    $g.DrawImage($logo, [single](772 - $lw / 2), [single]40, [single]$lw, [single]$lh); $logo.Dispose()
    # Reto: línea blanca + línea naranja como en las capturas
    Draw-Text $g $lang[1] $famHead 2 54 420 772 312 (C 255 255 255 255) $null 9 (C 255 6 12 40)
    Draw-Text $g $lang[2] $famHead 3 66 420 772 392 (C 255 255 226 92) (C 255 255 120 18) 11 (C 255 30 12 6)
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
