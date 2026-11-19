# Kit de interfaz del menú principal: logo, fondo e iconos (PNG en Resources/UI).
param([switch]$All)  # -All también rehace el logo y el fondo del menú
Add-Type -AssemblyName System.Drawing
$out = "D:\Unity\AeroBlitz\Assets\AeroBlitz\Resources\UI"
New-Item -ItemType Directory -Force $out | Out-Null

function C([double]$r, [double]$g, [double]$b, [double]$a = 255) { [System.Drawing.Color]::FromArgb([int]$a, [int]$r, [int]$g, [int]$b) }
function NewBmp($w, $h) {
    $bmp = New-Object System.Drawing.Bitmap $w, $h, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'; $g.PixelOffsetMode = 'HighQuality'; $g.TextRenderingHint = 'AntiAlias'
    return @($bmp, $g)
}
# Iconos: se dibujan en un espacio de 128 y se guardan a 256 px (más nítidos en pantallas grandes)
function NewIcon {
    $bmp = New-Object System.Drawing.Bitmap 256, 256, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'; $g.PixelOffsetMode = 'HighQuality'; $g.TextRenderingHint = 'AntiAlias'
    $g.ScaleTransform(2, 2)
    return @($bmp, $g)
}
function Save($bmp, $g, $name) { $bmp.Save("$out\$name.png", [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $bmp.Dispose() }
function PF([double]$x, [double]$y) { New-Object System.Drawing.PointF ([float]$x), ([float]$y) }
function Poly($pts) { $a = New-Object 'System.Drawing.PointF[]' ($pts.Count / 2); for ($i = 0; $i -lt $a.Length; $i++) { $a[$i] = PF $pts[2*$i] $pts[2*$i+1] }; return ,$a }
function Brush($c) { New-Object System.Drawing.SolidBrush $c }
function Pen($c, $w) { $p = New-Object System.Drawing.Pen $c, ([float]$w); $p.LineJoin = 'Round'; $p.StartCap = 'Round'; $p.EndCap = 'Round'; return $p }
function VGrad($x, $y1, $y2, $c1, $c2) { New-Object System.Drawing.Drawing2D.LinearGradientBrush (PF $x $y1), (PF $x ($y2 + 0.01)), $c1, $c2 }
function RoundRect([double]$x, [double]$y, [double]$w, [double]$h, [double]$r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddArc([float]$x, [float]$y, [float](2*$r), [float](2*$r), 180, 90)
    $p.AddArc([float]($x+$w-2*$r), [float]$y, [float](2*$r), [float](2*$r), 270, 90)
    $p.AddArc([float]($x+$w-2*$r), [float]($y+$h-2*$r), [float](2*$r), [float](2*$r), 0, 90)
    $p.AddArc([float]$x, [float]($y+$h-2*$r), [float](2*$r), [float](2*$r), 90, 90)
    $p.CloseFigure(); return $p
}
function Star($cx, $cy, $ro, $ri, $n = 5) {
    $pts = @()
    for ($i = 0; $i -lt 2*$n; $i++) { $r = if ($i % 2 -eq 0) { $ro } else { $ri }; $a = -[Math]::PI/2 + $i * [Math]::PI / $n; $pts += ($cx + $r*[Math]::Cos($a)); $pts += ($cy + $r*[Math]::Sin($a)) }
    return ,(Poly $pts)
}
function Radial($g, $cx, $cy, $r, $c) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath; $path.AddEllipse([float]($cx-$r), [float]($cy-$r), [float](2*$r), [float](2*$r))
    $br = New-Object System.Drawing.Drawing2D.PathGradientBrush $path; $br.CenterColor = $c; $br.SurroundColors = @((C $c.R $c.G $c.B 0))
    $g.FillPath($br, $path); $br.Dispose(); $path.Dispose()
}
$white = C 255 255 255; $ink = C 14 18 40

# ---------------- ICONOS 128x128 ----------------
$S = 128
# Usuario
$cv = NewIcon; $g = $cv[1]
$g.FillEllipse((Brush $white), 40, 14, 48, 48)
$g.FillPath((Brush $white), (RoundRect 18 68 92 52 26))
Save $cv[0] $g "icon_user"

# Moneda
$cv = NewIcon; $g = $cv[1]
$g.FillEllipse((VGrad 0 8 120 (C 255 226 110) (C 214 140 20)), 8, 8, 112, 112)
$g.DrawEllipse((Pen (C 150 90 10) 6), 11, 11, 106, 106)
$g.DrawEllipse((Pen (C 255 240 170 200) 5), 26, 26, 76, 76)
$g.FillPolygon((Brush (C 255 248 200 220)), (Star 64 64 28 12))
Save $cv[0] $g "icon_coin"

# Corazón
function HeartPath($s, $ox, $oy) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddBezier((PF ($ox+64*$s) ($oy+112*$s)), (PF ($ox+20*$s) ($oy+80*$s)), (PF ($ox+0*$s) ($oy+50*$s)), (PF ($ox+16*$s) ($oy+26*$s)))
    $p.AddBezier((PF ($ox+16*$s) ($oy+26*$s)), (PF ($ox+32*$s) ($oy+4*$s)), (PF ($ox+58*$s) ($oy+10*$s)), (PF ($ox+64*$s) ($oy+34*$s)))
    $p.AddBezier((PF ($ox+64*$s) ($oy+34*$s)), (PF ($ox+70*$s) ($oy+10*$s)), (PF ($ox+96*$s) ($oy+4*$s)), (PF ($ox+112*$s) ($oy+26*$s)))
    $p.AddBezier((PF ($ox+112*$s) ($oy+26*$s)), (PF ($ox+128*$s) ($oy+50*$s)), (PF ($ox+108*$s) ($oy+80*$s)), (PF ($ox+64*$s) ($oy+112*$s)))
    $p.CloseFigure(); return $p
}
$cv = NewIcon; $g = $cv[1]
$hp = HeartPath 1 0 4
$g.FillPath((VGrad 0 10 116 (C 255 110 130) (C 210 30 60)), $hp)
$g.FillEllipse((Brush (C 255 255 255 120)), 26, 26, 24, 16)
Save $cv[0] $g "icon_heart"

# Engranaje
$cv = NewIcon; $g = $cv[1]
$pts = @(); $n = 8
for ($i = 0; $i -lt $n*4; $i++) {
    $a = $i * 2*[Math]::PI / ($n*4); $r = if (($i % 4) -lt 2) { 58 } else { 44 }
    $pts += (64 + $r*[Math]::Cos($a)); $pts += (64 + $r*[Math]::Sin($a))
}
$gp = New-Object System.Drawing.Drawing2D.GraphicsPath; $gp.AddPolygon((Poly $pts)); $gp.AddEllipse(44, 44, 40, 40); $gp.FillMode = 'Alternate'
$g.FillPath((Brush (C 225 232 245)), $gp)
Save $cv[0] $g "icon_gear"

# Estrella
$cv = NewIcon; $g = $cv[1]
$g.FillPolygon((VGrad 0 6 120 (C 255 235 120) (C 240 160 20)), (Star 64 66 60 26))
$g.DrawPolygon((Pen (C 170 100 10) 3), (Star 64 66 60 26))
Save $cv[0] $g "icon_star"

# Trofeo
$cv = NewIcon; $g = $cv[1]
$gold = VGrad 0 10 120 (C 255 230 120) (C 220 140 20)
$cup = New-Object System.Drawing.Drawing2D.GraphicsPath
$cup.AddLine(30, 14, 98, 14); $cup.AddBezier((PF 98 14), (PF 98 66), (PF 80 80), (PF 64 82)); $cup.AddBezier((PF 64 82), (PF 48 80), (PF 30 66), (PF 30 14)); $cup.CloseFigure()
$g.DrawArc((Pen (C 230 160 30) 9), 8, 22, 34, 34, 90, 180)
$g.DrawArc((Pen (C 230 160 30) 9), 86, 22, 34, 34, 270, 180)
$g.FillPath($gold, $cup)
$g.FillRectangle($gold, 56, 80, 16, 18)
$g.FillPath($gold, (RoundRect 36 96 56 18 6))
$g.FillPolygon((Brush (C 255 250 210 220)), (Star 64 42 15 6.5))
Save $cv[0] $g "icon_trophy"

# Play
$cv = NewIcon; $g = $cv[1]
$tri = New-Object System.Drawing.Drawing2D.GraphicsPath; $tri.AddPolygon((Poly @(34,16, 110,64, 34,112)));
$g.FillPath((Brush $white), $tri); $g.DrawPath((Pen $white 14), $tri)
Save $cv[0] $g "icon_play"

# Cohete (hangar)
$cv = NewIcon; $g = $cv[1]
$g.FillPolygon((Brush (C 255 150 40)), (Poly @(52,96, 76,96, 64,124)))
$g.FillPolygon((Brush (C 120 190 255)), (Poly @(40,70, 22,100, 44,94)))
$g.FillPolygon((Brush (C 120 190 255)), (Poly @(88,70, 106,100, 84,94)))
$body = New-Object System.Drawing.Drawing2D.GraphicsPath
$body.AddBezier((PF 64 6), (PF 96 30), (PF 92 80), (PF 82 98)); $body.AddLine(82, 98, 46, 98); $body.AddBezier((PF 46 98), (PF 36 80), (PF 32 30), (PF 64 6)); $body.CloseFigure()
$g.FillPath((VGrad 0 6 98 (C 255 255 255) (C 190 210 240)), $body)
$g.FillEllipse((Brush (C 40 110 220)), 50, 38, 28, 28); $g.FillEllipse((Brush (C 170 225 255)), 55, 43, 12, 10)
Save $cv[0] $g "icon_rocket"

# Carrito (tienda)
$cv = NewIcon; $g = $cv[1]
$g.DrawLines((Pen $white 9), (Poly @(8,22, 26,22, 40,82, 100,82)))
$g.FillPolygon((Brush $white), (Poly @(30,34, 118,34, 106,72, 40,72)))
$g.FillEllipse((Brush $white), 38, 92, 22, 22); $g.FillEllipse((Brush $white), 84, 92, 22, 22)
$g.FillPolygon((Brush (C 150 110 255)), (Poly @(48,44, 106,44, 99,64, 54,64)))
Save $cv[0] $g "icon_cart"

# Portapapeles con estrella (misiones)
$cv = NewIcon; $g = $cv[1]
$g.FillPath((Brush $white), (RoundRect 22 18 84 104 12))
$g.FillPath((Brush (C 120 80 20)), (RoundRect 44 8 40 22 8))
$g.FillPolygon((VGrad 0 40 100 (C 255 220 90) (C 240 140 20)), (Star 64 70 32 14))
Save $cv[0] $g "icon_missions"

# Barras (ranking)
$cv = NewIcon; $g = $cv[1]
$g.FillPath((Brush $white), (RoundRect 14 70 24 46 6))
$g.FillPath((Brush $white), (RoundRect 52 44 24 72 6))
$g.FillPath((Brush $white), (RoundRect 90 16 24 100 6))
Save $cv[0] $g "icon_bars"

# Globo
$cv = NewIcon; $g = $cv[1]
$g.FillEllipse((VGrad 0 8 120 (C 90 190 255) (C 30 100 220)), 8, 8, 112, 112)
$lp = Pen (C 220 240 255 220) 4
$g.DrawEllipse($lp, 8, 8, 112, 112); $g.DrawEllipse($lp, 38, 8, 52, 112); $g.DrawLine($lp, 64, 8, 64, 120); $g.DrawLine($lp, 8, 64, 120, 64)
$g.DrawArc($lp, 14, 26, 100, 30, 180, 180); $g.DrawArc($lp, 14, 72, 100, 30, 0, 180)
Save $cv[0] $g "icon_globe"

# Medallas 1, 2, 3
$medals = @(@("1", (C 255 225 100), (C 220 150 20)), @("2", (C 235 240 250), (C 150 160 180)), @("3", (C 245 175 110), (C 170 95 40)))
foreach ($m in $medals) {
    $cv = NewIcon; $g = $cv[1]
    $g.FillPolygon((Brush (C 60 120 230)), (Poly @(36,4, 58,4, 70,52, 48,52)))
    $g.FillPolygon((Brush (C 220 60 80)), (Poly @(92,4, 70,4, 58,52, 80,52)))
    $g.FillEllipse((VGrad 0 40 124 $m[1] $m[2]), 18, 40, 92, 84)
    $g.DrawEllipse((Pen (C 255 255 255 150) 4), 26, 47, 76, 70)
    $f = New-Object System.Drawing.Font "Arial Black", 40, ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel)
    $sf = New-Object System.Drawing.StringFormat; $sf.Alignment = 'Center'; $sf.LineAlignment = 'Center'
    $g.DrawString($m[0], $f, (Brush (C 70 45 10)), (New-Object System.Drawing.RectangleF 18, 44, 92, 84), $sf)
    Save $cv[0] $g ("medal_" + $m[0])
}

# Llama (racha)
$cv = NewIcon; $g = $cv[1]
$fl = New-Object System.Drawing.Drawing2D.GraphicsPath
$fl.AddBezier((PF 64 122), (PF 24 120), (PF 14 84), (PF 34 56)); $fl.AddBezier((PF 34 56), (PF 40 72), (PF 50 74), (PF 52 64))
$fl.AddBezier((PF 52 64), (PF 46 36), (PF 62 14), (PF 76 4)); $fl.AddBezier((PF 76 4), (PF 74 34), (PF 112 52), (PF 108 88))
$fl.AddBezier((PF 108 88), (PF 106 112), (PF 88 122), (PF 64 122)); $fl.CloseFigure()
$g.FillPath((VGrad 0 4 122 (C 255 210 60) (C 240 70 20)), $fl)
$in = New-Object System.Drawing.Drawing2D.GraphicsPath
$in.AddBezier((PF 64 118), (PF 44 116), (PF 40 96), (PF 52 80)); $in.AddBezier((PF 52 80), (PF 58 92), (PF 66 88), (PF 68 76)); $in.AddBezier((PF 68 76), (PF 84 90), (PF 88 116), (PF 64 118)); $in.CloseFigure()
$g.FillPath((Brush (C 255 245 170)), $in)
Save $cv[0] $g "icon_flame"

# Video
$cv = NewIcon; $g = $cv[1]
$g.FillPath((Brush $white), (RoundRect 6 30 84 68 14))
$g.FillPolygon((Brush $white), (Poly @(92,52, 122,34, 122,94, 92,76)))
$g.FillPolygon((Brush (C 40 160 80)), (Poly @(38,46, 64,64, 38,82)))
Save $cv[0] $g "icon_video"

# Cofre
$cv = NewIcon; $g = $cv[1]
$g.FillPath((VGrad 0 50 120 (C 170 90 255) (C 100 40 190)), (RoundRect 12 54 104 64 10))
$g.FillPath((VGrad 0 14 60 (C 200 130 255) (C 130 60 220)), (RoundRect 8 16 112 46 22))
$g.FillRectangle((Brush (C 255 205 60)), 56, 16, 16, 102)
$g.FillPath((Brush (C 255 220 90)), (RoundRect 50 50 28 28 6))
$g.FillEllipse((Brush (C 120 70 10)), 60, 58, 8, 8)
Save $cv[0] $g "icon_chest"

# Flechas
foreach ($dir in @("right", "left")) {
    $cv = NewIcon; $g = $cv[1]
    $pts = if ($dir -eq "right") { @(44,20, 88,64, 44,108) } else { @(84,20, 40,64, 84,108) }
    $g.DrawLines((Pen $white 18), (Poly $pts))
    Save $cv[0] $g ("icon_chevron_" + $dir)
}

. "$PSScriptRoot/ui_kit_iconos_extra.ps1"
if (-not $All) { "iconos listos"; return }

# ---------------- LOGO ----------------
$W = 1000; $H = 430
$cv = NewBmp $W $H; $g = $cv[1]
# Alas detrás del texto
foreach ($side in 1, -1) {
    $cx = 500
    function WX($x) { $script:cx + $side * $x }
    $wing = Poly @((WX 120), 150, (WX 470), 40, (WX 430), 85, (WX 330), 120, (WX 470), 105, (WX 410), 150, (WX 300), 170, (WX 420), 175, (WX 340), 215, (WX 140), 225)
    $g.FillPolygon((VGrad 0 40 225 (C 235 245 255) (C 90 140 210)), $wing)
    $g.DrawPolygon((Pen $ink 7), $wing)
    $g.DrawLine((Pen (C 255 255 255 160) 3), [float](WX 160), 168, [float](WX 440), 66)
}
function DrawWord($g, $text, $size, $cy) {
    $font = New-Object System.Drawing.FontFamily "Arial Black"
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $sf = New-Object System.Drawing.StringFormat; $sf.Alignment = 'Center'; $sf.LineAlignment = 'Center'
    $path.AddString($text, $font, [int][System.Drawing.FontStyle]::Bold, [float]$size, (New-Object System.Drawing.RectangleF 0, ($cy - $size), 1000, (2*$size)), $sf)
    $m = New-Object System.Drawing.Drawing2D.Matrix; $m.Shear(-0.18, 0); $m.Translate(($cy * 0.18), 0, 'Append'); $path.Transform($m)
    $g.DrawPath((Pen $ink 26), $path)
    $g.DrawPath((Pen (C 30 90 200) 12), $path)
    $b = $path.GetBounds()
    $g.FillPath((VGrad 0 $b.Top $b.Bottom (C 255 255 255) (C 110 190 255)), $path)
    $g.SetClip($path); $g.FillRectangle((Brush (C 255 255 255 90)), $b.Left, $b.Top, $b.Width, $b.Height * 0.35); $g.ResetClip()
}
DrawWord $g "AERO" 150 150
DrawWord $g "BLITZ" 150 305
Save $cv[0] $g "logo"

# ---------------- FONDO DEL MENÚ 1080x1920 ----------------
$W = 1080; $H = 1920
$cv = NewBmp $W $H; $g = $cv[1]
$g.FillRectangle((VGrad 0 0 $H (C 8 16 44) (C 4 6 20)), 0, 0, $W, $H)
Radial $g 260 500 520 (C 60 40 140 70)
Radial $g 900 1100 600 (C 20 70 150 60)
Radial $g 540 760 420 (C 40 120 255 55)
$rnd = New-Object System.Random 11
for ($i = 0; $i -lt 420; $i++) {
    $x = $rnd.Next(0, $W); $y = $rnd.Next(0, $H); $s = if ($rnd.NextDouble() -lt 0.1) { 3 } else { 2 }
    $g.FillEllipse((Brush (C 255 255 255 ($rnd.Next(50, 200)))), $x, $y, $s, $s)
}
# Planeta a la izquierda
$pl = New-Object System.Drawing.Drawing2D.GraphicsPath; $pl.AddEllipse(-260, 380, 560, 560)
$pb = New-Object System.Drawing.Drawing2D.PathGradientBrush $pl; $pb.CenterPoint = PF 120 560; $pb.CenterColor = C 70 150 230; $pb.SurroundColors = @((C 10 30 80))
$g.FillPath($pb, $pl)
$g.DrawArc((Pen (C 140 210 255 150) 6), -258, 382, 556, 556, 300, 90)
Radial $g 20 660 330 (C 90 170 255 40)
# Asteroides a la derecha
foreach ($a in @(@(960, 520, 46), @(1040, 700, 30), @(900, 860, 22), @(1010, 980, 38))) {
    $pts = @(); $n = 9
    for ($i = 0; $i -lt $n; $i++) { $ang = $i * 2*[Math]::PI / $n; $r = $a[2] * (0.78 + 0.22*$rnd.NextDouble()); $pts += ($a[0] + $r*[Math]::Cos($ang)); $pts += ($a[1] + $r*[Math]::Sin($ang)) }
    $g.FillPolygon((VGrad 0 ($a[1]-$a[2]) ($a[1]+$a[2]) (C 140 120 105) (C 60 50 45)), (Poly $pts))
}
$bmp = $cv[0]
$jpg = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
$ep = New-Object System.Drawing.Imaging.EncoderParameters 1; $ep.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter ([System.Drawing.Imaging.Encoder]::Quality), 88L
$bmp.Save("$out\menu_bg.jpg", $jpg, $ep); $g.Dispose(); $bmp.Dispose()

Get-ChildItem $out -File | Where-Object { $_.Extension -ne '.meta' } | ForEach-Object { "{0,-24} {1,8:N0} bytes" -f $_.Name, $_.Length }
