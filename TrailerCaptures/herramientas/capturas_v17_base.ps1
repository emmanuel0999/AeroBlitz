# Capturas de Google Play para la versión 1.0.3 (1080x1920, PNG de 24 bits), estilo espacial azul con rótulo naranja.
# Uso: capturas_v15.ps1 -Frames D:\Temp\storecap\shots -Ships D:\Temp\storecap\ships
param([string]$Frames = "D:\Temp\storecap\shots", [string]$Ships = "D:\Temp\storecap\ships", [string]$Only = "")
Add-Type -AssemblyName System.Drawing
$ErrorActionPreference = 'Stop'
$out = "D:\Unity\AeroBlitz\StoreAssets\capturas_v15"
$W = 1080; $H = 1920

function C($a, $r, $g, $b) { [System.Drawing.Color]::FromArgb($a, $r, $g, $b) }
function P($x, $y) { New-Object System.Drawing.PointF([single]$x, [single]$y) }
$famHead = New-Object System.Drawing.FontFamily("Segoe UI Black")
$famSub = New-Object System.Drawing.FontFamily("Roboto Condensed")
$famName = New-Object System.Drawing.FontFamily("Segoe UI")

function Glow($g, $cx, $cy, $r, $color) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddEllipse([single]($cx - $r), [single]($cy - $r), [single](2 * $r), [single](2 * $r))
    $pb = New-Object System.Drawing.Drawing2D.PathGradientBrush($path)
    $pb.CenterColor = $color
    $pb.SurroundColors = [System.Drawing.Color[]]@((C 0 $color.R $color.G $color.B))
    $pb.SetSigmaBellShape([single]1, [single]1)
    $g.FillPath($pb, $path); $pb.Dispose(); $path.Dispose()
}

function Round-Path($x, $y, $w, $h, $r) {
    $d = [single](2 * $r); $x = [single]$x; $y = [single]$y; $w = [single]$w; $h = [single]$h
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddArc($x, $y, $d, $d, [single]180, [single]90)
    $p.AddArc([single]($x + $w - $d), $y, $d, $d, [single]270, [single]90)
    $p.AddArc([single]($x + $w - $d), [single]($y + $h - $d), $d, $d, [single]0, [single]90)
    $p.AddArc($x, [single]($y + $h - $d), $d, $d, [single]90, [single]90)
    $p.CloseFigure()
    return $p
}

function Text-Path($text, $fam, $style, $size) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddString($text, $fam, [int]$style, [single]$size, (P 0 0), [System.Drawing.StringFormat]::GenericTypographic)
    return $p
}

# Texto centrado en (cx, cy) que no pasa de maxW; relleno liso o degradado (arriba -> abajo) y borde oscuro
function Draw-Text($g, $text, $fam, $style, $maxSize, $maxW, $cx, $cy, $top, $bottom, $edgeW = 0, $edge = $null) {
    $p = Text-Path $text $fam $style 100
    $size = [Math]::Min([double]$maxSize, 100.0 * $maxW / [Math]::Max(1.0, [double]$p.GetBounds().Width)); $p.Dispose()
    $p = Text-Path $text $fam $style $size
    $b = $p.GetBounds()
    $ref = Text-Path "H" $fam $style $size; $rb = $ref.GetBounds(); $ref.Dispose()
    $m = New-Object System.Drawing.Drawing2D.Matrix
    $m.Translate([single]($cx - $b.X - $b.Width / 2), [single]($cy - $rb.Y - $rb.Height / 2)); $p.Transform($m)
    if ($edgeW -gt 0) {
        $sh = $p.Clone(); $m2 = New-Object System.Drawing.Drawing2D.Matrix; $m2.Translate(0, [single]($size * .06)); $sh.Transform($m2)
        $pen = New-Object System.Drawing.Pen((C 150 0 0 10), [single]($edgeW + 6)); $pen.LineJoin = 'Round'; $g.DrawPath($pen, $sh); $pen.Dispose(); $sh.Dispose()
        $pen = New-Object System.Drawing.Pen($edge, [single]$edgeW); $pen.LineJoin = 'Round'; $g.DrawPath($pen, $p); $pen.Dispose()
    }
    $bb = $p.GetBounds()
    if ($bottom) {
        $rect = New-Object System.Drawing.RectangleF($bb.X, ($bb.Y - 1), $bb.Width, ($bb.Height + 2))
        $br = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, $top, $bottom, [single]90)
    } else { $br = New-Object System.Drawing.SolidBrush($top) }
    $g.FillPath($br, $p); $br.Dispose(); $p.Dispose()
    return $size
}

function New-Shot($seed) {
    $bmp = New-Object System.Drawing.Bitmap($W, $H, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'
    $g.PixelOffsetMode = 'HighQuality'; $g.CompositingQuality = 'HighQuality'; $g.TextRenderingHint = 'AntiAliasGridFit'
    # Espacio profundo con nebulosas y estrellas
    $rect = New-Object System.Drawing.Rectangle(0, 0, $W, $H)
    $lg = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, (C 255 4 8 26), (C 255 10 20 56), [single]90)
    $g.FillRectangle($lg, $rect); $lg.Dispose()
    $rnd = New-Object System.Random($seed)
    Glow $g (180 + $rnd.Next(200)) (500 + $rnd.Next(400)) 620 (C 70 40 90 255)
    Glow $g (700 + $rnd.Next(300)) (1200 + $rnd.Next(500)) 680 (C 60 140 50 220)
    Glow $g 540 120 520 (C 50 255 140 40)
    for ($i = 0; $i -lt 260; $i++) {
        $s = [single](1 + $rnd.NextDouble() * 2.6); $a = 60 + $rnd.Next(190)
        $b = New-Object System.Drawing.SolidBrush((C $a 220 235 255)); $g.FillEllipse($b, [single]($rnd.Next($W)), [single]($rnd.Next($H)), $s, $s); $b.Dispose()
    }
    # Marco azul con brillo, como un panel del juego
    foreach ($bw in 26, 16, 9) {
        $fp = Round-Path 16 16 ($W - 32) ($H - 32) 46
        $pen = New-Object System.Drawing.Pen((C ([int](190 / ($bw / 4))) 60 150 255), [single]$bw); $g.DrawPath($pen, $fp); $pen.Dispose(); $fp.Dispose()
    }
    $fp = Round-Path 16 16 ($W - 32) ($H - 32) 46
    $pen = New-Object System.Drawing.Pen((C 255 120 200 255), [single]4); $g.DrawPath($pen, $fp); $pen.Dispose(); $fp.Dispose()
    return ,@($bmp, $g)
}

$gold1 = C 255 255 226 92; $gold2 = C 255 255 120 18; $white = C 255 255 255 255; $edgeDark = C 255 30 12 6; $navyEdge = C 255 6 12 40
$cyanText = C 255 150 215 255

# Rótulo: línea 1 grande en naranja/dorado, línea 2 blanca; frase pequeña abajo
function Draw-Caption($g, $cap) {
    Draw-Text $g $cap[0] $famHead 3 150 940 540 150 $gold1 $gold2 14 $edgeDark | Out-Null
    Draw-Text $g $cap[1] $famHead 2 74 940 540 288 $white $null 10 $navyEdge | Out-Null
    if ($cap.Count -gt 2 -and $cap[2]) {
        Draw-Text $g $cap[2] $famSub 1 46 900 540 1852 $cyanText $null 0 $null | Out-Null
    }
}

# Marco con la captura del juego (9:16)
function Draw-Phone($g, $frame, $x, $y, $w, $h) {
    Glow $g ($x + $w / 2) ($y + $h / 2) ($h * .58) (C 90 30 110 255)
    foreach ($pw in 18, 10) {
        $o = Round-Path ($x - 8) ($y - 8) ($w + 16) ($h + 16) 44
        $pen = New-Object System.Drawing.Pen((C ([int](260 / $pw * 4)) 70 170 255), [single]$pw); $g.DrawPath($pen, $o); $pen.Dispose(); $o.Dispose()
    }
    $clip = Round-Path $x $y $w $h 38
    $img = [System.Drawing.Image]::FromFile($frame)
    $g.SetClip($clip); $g.DrawImage($img, [single]$x, [single]$y, [single]$w, [single]$h); $g.ResetClip(); $img.Dispose()
    $pen = New-Object System.Drawing.Pen((C 255 140 210 255), [single]4); $g.DrawPath($pen, $clip); $pen.Dispose(); $clip.Dispose()
}

function Save-Shot($c, $path) { $c[0].Save($path, [System.Drawing.Imaging.ImageFormat]::Png); $c[1].Dispose(); $c[0].Dispose() }

function Shot-Phone($path, $cap, $frame, $seed) {
    $c = New-Shot $seed; $g = $c[1]
    Draw-Phone $g $frame 150 384 780 1387
    Draw-Caption $g $cap
    Save-Shot $c $path
}

function Draw-ShipImg($g, $file, $cx, $cy, $size, $glowColor) {
    if ($glowColor) { Glow $g $cx $cy ($size * .62) $glowColor }
    $img = [System.Drawing.Image]::FromFile($file)
    $g.DrawImage($img, [single]($cx - $size / 2), [single]($cy - $size / 2), [single]$size, [single]$size); $img.Dispose()
}

function Shot-Ships($path, $cap, $t) {
    $c = New-Shot 77; $g = $c[1]
    Draw-Caption $g $cap
    # Premium: Titán, Fénix (centro) y Nova, metálicas
    $prem = @(@("titan", 230, 640, 330, (C 150 120 160 255), $t.titan, $t.titanAb), @("nova", 850, 640, 330, (C 150 255 120 230), $t.nova, $t.novaAb), @("phoenix", 540, 600, 430, (C 170 255 140 60), $t.phoenix, $t.phoenixAb))
    foreach ($s in $prem) {
        Draw-ShipImg $g "$Ships\$($s[0])_metal.png" $s[1] $s[2] $s[3] $s[4]
    }
    foreach ($s in $prem) {
        $ny = $s[2] + $s[3] * .43 + 10
        Draw-Text $g $s[5] $famHead 0 50 300 $s[1] $ny $gold1 $gold2 6 $edgeDark | Out-Null
        Draw-Text $g $s[6] $famSub 1 30 300 $s[1] ($ny + 44) $white $null 0 $null | Out-Null
    }
    # Sello PREMIUM
    $pill = Round-Path 400 372 280 56 28
    $br = New-Object System.Drawing.Drawing2D.LinearGradientBrush((New-Object System.Drawing.RectangleF(400, 372, 280, 56)), $gold1, $gold2, [single]90)
    $g.FillPath($br, $pill); $br.Dispose(); $pill.Dispose()
    Draw-Text $g "PREMIUM" $famHead 0 34 240 540 400 (C 255 60 24 4) $null 0 $null | Out-Null
    # Naves por monedas: fila de serie y fila en neón
    Draw-Text $g $t.coinTitle $famHead 2 44 900 540 1010 $cyanText $null 6 $navyEdge | Out-Null
    $row1 = @("falcon", "wasp", "hummingbird", "guardian"); $row2 = @("bastion", "viper", "comet", "specter")
    for ($i = 0; $i -lt 4; $i++) {
        $cx = 150 + $i * 260
        Draw-ShipImg $g "$Ships\$($row1[$i])_cartoon.png" $cx 1160 210 (C 70 60 140 255)
        Draw-Text $g $t.names[$row1[$i]] $famName 1 32 240 $cx 1282 $white $null 0 $null | Out-Null
        Draw-ShipImg $g "$Ships\$($row2[$i])_neon.png" $cx 1450 210 (C 90 80 200 255)
        Draw-Text $g $t.names[$row2[$i]] $famName 1 32 240 $cx 1572 $white $null 0 $null | Out-Null
    }
    # Estilos
    $chips = $t.styles; $cw = 290
    for ($i = 0; $i -lt 3; $i++) {
        $x = 540 - (3 * $cw + 2 * 24) / 2 + $i * ($cw + 24)
        $rp = Round-Path $x 1660 $cw 74 37
        $b = New-Object System.Drawing.SolidBrush((C 220 10 24 64)); $g.FillPath($b, $rp); $b.Dispose()
        $col = @((C 255 255 170 60), (C 255 90 240 255), (C 255 200 210 230))[$i]
        $pen = New-Object System.Drawing.Pen($col, [single]4); $g.DrawPath($pen, $rp); $pen.Dispose(); $rp.Dispose()
        Draw-Text $g $chips[$i] $famHead 0 36 ($cw - 40) ($x + $cw / 2) 1697 $col $null 0 $null | Out-Null
    }
    Draw-Text $g $cap[2] $famSub 1 46 900 540 1852 $cyanText $null 0 $null | Out-Null
    Save-Shot $c $path
}

function Shot-Worlds($path, $cap, $names) {
    $c = New-Shot 61; $g = $c[1]
    Draw-Caption $g $cap
    $x0 = 64; $y0 = 388; $tw = 466; $th = 266; $gap = 16
    for ($i = 0; $i -lt 10; $i++) {
        $x = $x0 + ($i % 2) * ($tw + $gap); $y = $y0 + [Math]::Floor($i / 2) * ($th + $gap)
        $img = [System.Drawing.Image]::FromFile("$Frames\world_$('{0:00}' -f ($i + 1)).png")
        $rp = Round-Path $x $y $tw $th 22
        $g.SetClip($rp)
        $srcH = [int](1080 * $th / $tw)
        $dst = New-Object System.Drawing.Rectangle($x, $y, $tw, $th)
        $src = New-Object System.Drawing.Rectangle(0, 470, 1080, $srcH)
        $g.DrawImage($img, $dst, $src, [System.Drawing.GraphicsUnit]::Pixel); $img.Dispose()
        $band = New-Object System.Drawing.Drawing2D.LinearGradientBrush((New-Object System.Drawing.RectangleF($x, ($y + $th - 86), $tw, 86)), (C 0 4 8 26), (C 230 4 8 26), [single]90)
        $g.FillRectangle($band, $x, ($y + $th - 86), $tw, 86); $band.Dispose()
        $g.ResetClip()
        $pen = New-Object System.Drawing.Pen((C 255 110 190 255), [single]3); $g.DrawPath($pen, $rp); $pen.Dispose(); $rp.Dispose()
        Draw-Text $g $names[$i] $famHead 0 34 ($tw - 40) ($x + $tw / 2) ($y + $th - 34) $white $null 6 $navyEdge | Out-Null
        Draw-Text $g ("{0}" -f ($i + 1)) $famHead 0 34 60 ($x + 34) ($y + 34) $gold1 $gold2 6 $edgeDark | Out-Null
    }
    Save-Shot $c $path
}

New-Item -ItemType Directory -Force $out | Out-Null

