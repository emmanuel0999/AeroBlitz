# Ideas de icono para la versión 1.0.3: naves reales del juego, fondo espacial azul y toques naranjas (como las capturas nuevas)
Add-Type -AssemblyName System.Drawing
$ErrorActionPreference = 'Stop'
$out = "D:\Unity\AeroBlitz\StoreAssets\iconos_v15"
$shipDir = "D:\Temp\storecap\ships"
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

function New-Canvas {
    $bmp = New-Object System.Drawing.Bitmap($SZ, $SZ, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'; $g.PixelOffsetMode = 'HighQuality'; $g.CompositingQuality = 'HighQuality'
    return ,@($bmp, $g)
}

# Espacio: degradado radial + estrellas
function Space($g, $seed, $center, $edge, $cx = 512, $cy = 512) {
    $g.Clear($edge)
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddEllipse([single]($cx - 900), [single]($cy - 900), [single]1800, [single]1800)
    $pb = New-Object System.Drawing.Drawing2D.PathGradientBrush($path)
    $pb.CenterPoint = P $cx $cy; $pb.CenterColor = $center; $pb.SurroundColors = [System.Drawing.Color[]]@($edge)
    $g.FillRectangle($pb, 0, 0, $SZ, $SZ); $pb.Dispose(); $path.Dispose()
    $rnd = New-Object System.Random($seed)
    for ($i = 0; $i -lt 90; $i++) {
        $st = [single](2 + $rnd.NextDouble() * 5); $al = 70 + $rnd.Next(170)
        $b = New-Object System.Drawing.SolidBrush((C $al 220 235 255)); $g.FillEllipse($b, [single]$rnd.Next($SZ), [single]$rnd.Next($SZ), $st, $st); $b.Dispose()
    }
}

# Nave real del juego (PNG exportado), girada; con fuego del motor detrás
function Ship($g, $file, $cx, $cy, $size, $deg, $flame = $true) {
    $g.TranslateTransform([single]$cx, [single]$cy); $g.RotateTransform([single]$deg)
    if ($flame) {
        Glow $g 0 ($size * .42) ($size * .3) (C 200 255 140 40)
        $fp = New-Object System.Drawing.Drawing2D.GraphicsPath
        $fp.AddPolygon([System.Drawing.PointF[]]@((P (-$size * .09) ($size * .3)), (P ($size * .09) ($size * .3)), (P 0 ($size * .78))))
        $br = New-Object System.Drawing.Drawing2D.LinearGradientBrush((P 0 ($size * .3)), (P 0 ($size * .78)), (C 255 255 240 170), (C 0 255 90 20))
        $g.FillPath($br, $fp); $br.Dispose(); $fp.Dispose()
    }
    $img = [System.Drawing.Image]::FromFile($file)
    $g.DrawImage($img, [single](-$size / 2), [single](-$size / 2), [single]$size, [single]$size); $img.Dispose()
    $g.ResetTransform()
}

# Disparo de plasma alargado con brillo
function Bolt($g, $cx, $cy, $len, $wid, $deg, $core, $halo) {
    $g.TranslateTransform([single]$cx, [single]$cy); $g.RotateTransform([single]$deg)
    Glow $g 0 0 ($len * .75) $halo
    $b = New-Object System.Drawing.SolidBrush($core); $g.FillEllipse($b, [single](-$wid / 2), [single](-$len / 2), [single]$wid, [single]$len); $b.Dispose()
    $b = New-Object System.Drawing.SolidBrush((C 235 255 255 255)); $g.FillEllipse($b, [single](-$wid / 5), [single](-$len / 2.6), [single]($wid / 2.5), [single]($len / 1.3)); $b.Dispose()
    $g.ResetTransform()
}

function Burst($g, $cx, $cy, $r, $seed) {
    Glow $g $cx $cy ($r * 1.5) (C 160 255 120 30)
    Fill-Poly $g (Star $cx $cy $r ($r * .55) 13 $seed) (C 255 235 84 22)
    Fill-Poly $g (Star $cx $cy ($r * .78) ($r * .43) 13 ($seed + 1)) (C 255 255 150 30)
    Fill-Poly $g (Star $cx $cy ($r * .6) ($r * .33) 12 ($seed + 2)) (C 255 255 222 110)
    Glow $g $cx $cy ($r * .45) (C 230 255 255 230)
}

# Trozos cuadrados como los del juego al destruir algo
function Debris($g, $cx, $cy, $spread, $n, $seed, $colors) {
    $rnd = New-Object System.Random($seed)
    for ($i = 0; $i -lt $n; $i++) {
        $a = $rnd.NextDouble() * 2 * [Math]::PI; $d = $spread * [Math]::Sqrt($rnd.NextDouble())
        $q = 10 + $rnd.Next(26); $col = $colors[$rnd.Next($colors.Count)]
        $b = New-Object System.Drawing.SolidBrush($col)
        $g.FillRectangle($b, [single]($cx + [Math]::Cos($a) * $d), [single]($cy + [Math]::Sin($a) * $d), [single]$q, [single]$q); $b.Dispose()
    }
}

# Muro con franjas amarillas y negras (como los indestructibles del juego)
function Wall($g, $x, $y, $wd, $ht, $deg) {
    $g.TranslateTransform([single]($x + $wd / 2), [single]($y + $ht / 2)); $g.RotateTransform([single]$deg)
    $rect = New-Object System.Drawing.RectangleF([single](-$wd / 2), [single](-$ht / 2), [single]$wd, [single]$ht)
    $b = New-Object System.Drawing.SolidBrush((C 255 255 200 30)); $g.FillRectangle($b, $rect); $b.Dispose()
    $g.SetClip($rect)
    $b = New-Object System.Drawing.SolidBrush((C 255 25 25 30))
    for ($sx = -$wd; $sx -lt $wd; $sx += 70) {
        $g.FillPolygon($b, [System.Drawing.PointF[]]@((P $sx (-$ht / 2)), (P ($sx + 35) (-$ht / 2)), (P ($sx + 35 - $ht) ($ht / 2)), (P ($sx - $ht) ($ht / 2))))
    }
    $b.Dispose(); $g.ResetClip()
    $pen = New-Object System.Drawing.Pen((C 255 70 80 100), [single]8); $g.DrawRectangle($pen, $rect.X, $rect.Y, $rect.Width, $rect.Height); $pen.Dispose()
    $g.ResetTransform()
}

function Save-Icon($c, $name) {
    $c[0].Save("$out\$name.png", [System.Drawing.Imaging.ImageFormat]::Png)
    $c[1].Dispose(); $c[0].Dispose()
}

# ---------- A: Fénix al ataque (evolución del icono actual: diagonal + explosión, ahora en azul) ----------
$c = New-Canvas; $g = $c[1]
Space $g 5 (C 255 30 70 160) (C 255 4 8 30) 600 420
Burst $g 360 690 430 21
Debris $g 300 720 420 26 3 @((C 255 60 30 40), (C 255 90 50 40), (C 255 40 20 30))
foreach ($i in 0..2) { Bolt $g (640 + $i * 110 - 60) (300 - $i * 60 + 40) 190 46 35 (C 255 255 170 60) (C 170 255 120 30) }
Ship $g "$shipDir\phoenix_metal.png" 500 560 720 35
Save-Icon $c "idea_a_fenix"

# ---------- B: Nova neón (resalta entre iconos oscuros de la tienda) ----------
$c = New-Canvas; $g = $c[1]
Space $g 9 (C 255 40 16 80) (C 255 3 3 14) 512 560
Glow $g 512 560 520 (C 140 255 60 200)
Glow $g 512 560 320 (C 120 90 220 255)
foreach ($i in -2..2) { Bolt $g (512 + $i * 120) (230 + [Math]::Abs($i) * 50) 170 40 ($i * 9) (C 255 255 110 220) (C 190 255 60 200) }
Ship $g "$shipDir\nova_neon.png" 512 600 780 0
Save-Icon $c "idea_b_nova_neon"

# ---------- C: Titán rompe el muro (misiles, lo exclusivo del juego) ----------
$c = New-Canvas; $g = $c[1]
Space $g 13 (C 255 120 50 30) (C 255 20 6 14) 512 300
Wall $g -40 210 460 90 -9
Wall $g 610 190 470 90 8
Burst $g 512 255 250 41
Debris $g 512 250 300 30 7 @((C 255 255 200 30), (C 255 30 30 34), (C 255 255 140 40))
foreach ($m in @(@(430, 520, -8), @(600, 500, 10))) {
    Glow $g $m[0] ($m[1] + 60) 70 (C 200 255 150 40)
    Bolt $g $m[0] $m[1] 120 34 $m[2] (C 255 245 245 255) (C 120 255 220 180)
}
Ship $g "$shipDir\titan_metal.png" 512 760 520 0
Save-Icon $c "idea_c_titan_muro"

# ---------- D: Escuadrón premium (colección de naves) ----------
$c = New-Canvas; $g = $c[1]
Space $g 17 (C 255 28 60 150) (C 255 4 8 30) 512 380
Glow $g 512 300 420 (C 120 255 140 40)
Ship $g "$shipDir\titan_metal.png" 230 650 400 -8
Ship $g "$shipDir\nova_metal.png" 794 650 400 8
foreach ($i in -1..1) { Bolt $g (512 + $i * 80) 170 130 34 ($i * 8) (C 255 255 170 60) (C 160 255 120 30) }
Ship $g "$shipDir\phoenix_metal.png" 512 540 560 0
Save-Icon $c "idea_d_escuadron"

# ---------- Hoja para comparar: tamaño tienda (con esquinas redondeadas) y tamaño pequeño ----------
$names = @("idea_a_fenix", "idea_b_nova_neon", "idea_c_titan_muro", "idea_d_escuadron", "actual")
$labels = @("A · Fénix al ataque", "B · Nova neón", "C · Titán rompe el muro", "D · Escuadrón premium", "Icono actual")
$sheet = New-Object System.Drawing.Bitmap(1700, 560)
$g = [System.Drawing.Graphics]::FromImage($sheet); $g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'
$g.Clear((C 255 245 246 248))
$font = New-Object System.Drawing.Font("Segoe UI Semibold", 17)
for ($i = 0; $i -lt 5; $i++) {
    $file = if ($names[$i] -eq "actual") { "D:\Unity\AeroBlitz\Assets\AeroBlitz\Icon\icon_1024.png" } else { "$out\$($names[$i]).png" }
    $img = [System.Drawing.Image]::FromFile($file)
    $x = 30 + $i * 335; $y = 30
    $rp = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = 60; $rp.AddArc($x, $y, $d, $d, 180, 90); $rp.AddArc($x + 300 - $d, $y, $d, $d, 270, 90); $rp.AddArc($x + 300 - $d, $y + 300 - $d, $d, $d, 0, 90); $rp.AddArc($x, $y + 300 - $d, $d, $d, 90, 90); $rp.CloseFigure()
    $g.SetClip($rp); $g.DrawImage($img, $x, $y, 300, 300); $g.ResetClip()
    # Tamaño real en la lista de la tienda (unos 64 px)
    $sp = New-Object System.Drawing.Drawing2D.GraphicsPath
    $sx = $x + 30; $sy = 400; $sd = 14
    $sp.AddArc($sx, $sy, $sd, $sd, 180, 90); $sp.AddArc($sx + 64 - $sd, $sy, $sd, $sd, 270, 90); $sp.AddArc($sx + 64 - $sd, $sy + 64 - $sd, $sd, $sd, 0, 90); $sp.AddArc($sx, $sy + 64 - $sd, $sd, $sd, 90, 90); $sp.CloseFigure()
    $g.SetClip($sp); $g.DrawImage($img, $sx, $sy, 64, 64); $g.ResetClip()
    $g.DrawString("tamaño real", (New-Object System.Drawing.Font("Segoe UI", 12)), [System.Drawing.Brushes]::DimGray, ($sx + 76), ($sy + 22))
    $g.DrawString($labels[$i], $font, [System.Drawing.Brushes]::Black, $x, 345)
    $img.Dispose(); $rp.Dispose(); $sp.Dispose()
}
$sheet.Save("$out\comparacion.png", [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $sheet.Dispose()
"listo: $out"
