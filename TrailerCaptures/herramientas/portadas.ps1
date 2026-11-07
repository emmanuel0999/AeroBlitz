# Tres propuestas de gráfico destacado (1024x500, PNG de 24 bits sin transparencia)
$here = Split-Path $MyInvocation.MyCommand.Path
$u8 = New-Object Text.UTF8Encoding($false)
$src = [IO.File]::ReadAllText("$here\iconos.ps1", $u8)
Invoke-Expression $src.Substring(0, $src.IndexOf('# ---------- A:'))   # reutiliza nave, llamas, estrellas y brillos
$out = "D:\Unity\AeroBlitz\StoreAssets\portadas_nuevas"
New-Item -ItemType Directory -Force $out | Out-Null
$black = New-Object System.Drawing.FontFamily("Segoe UI Black")
$bold = New-Object System.Drawing.FontFamily("Segoe UI")
$cyanC = C 255 80 220 255; $goldC = C 255 255 205 40; $whiteC = C 255 255 255 255

function New-Banner {
    $bmp = New-Object System.Drawing.Bitmap(2048, 1000, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.CompositingQuality = 'HighQuality'; $g.PixelOffsetMode = 'HighQuality'
    $g.ScaleTransform(2, 2)
    return ,@($bmp, $g)
}

function Draw-Txt($g, $text, $fam, $style, $size, $cx, $cy, $fill, $edge, $maxW) {
    $fmt = [System.Drawing.StringFormat]::GenericTypographic
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddString($text, $fam, [int]$style, [single]$size, (P 0 0), $fmt)
    $w0 = [double]$p.GetBounds().Width
    if ($w0 -gt $maxW) {   # se recalcula el tamaño para que el texto quepa siempre
        $size = [double]$size * [double]$maxW / $w0
        $p.Dispose(); $p = New-Object System.Drawing.Drawing2D.GraphicsPath
        $p.AddString($text, $fam, [int]$style, [single]$size, (P 0 0), $fmt)
    }
    $b = $p.GetBounds()
    $m = New-Object System.Drawing.Drawing2D.Matrix; $m.Translate([single]($cx - $b.X - $b.Width / 2), [single]($cy - $b.Y - $b.Height / 2)); $p.Transform($m)
    if ($edge) {
        foreach ($w in 14, 8) {
            $pen = New-Object System.Drawing.Pen((C 90 $edge.R $edge.G $edge.B), [single]$w); $pen.LineJoin = 'Round'; $g.DrawPath($pen, $p); $pen.Dispose()
        }
    }
    $br = New-Object System.Drawing.SolidBrush($fill); $g.FillPath($br, $p); $br.Dispose(); $p.Dispose()
}

function Draw-Bolt($g, $bx, $y0, $y1, $sx, $sy, $ss, $deg) {
    $w = .085
    $shape = @(@(($bx - .03), $y0), @(($bx + .03), $y0), @(($bx + $w), ($y1 - .2)), @($bx, $y1), @(($bx - $w), ($y1 - .2)))
    $pts = Xf $shape $sx $sy $ss $deg
    $pen = New-Object System.Drawing.Pen((C 255 6 60 140), [single]16); $pen.LineJoin = 'Round'; $g.DrawPolygon($pen, $pts); $pen.Dispose()
    $pen = New-Object System.Drawing.Pen((C 255 40 214 255), [single]9); $pen.LineJoin = 'Round'; $g.DrawPolygon($pen, $pts); $pen.Dispose()
    Fill-Poly $g $pts (C 255 255 255 255)
}

function Save-Banner($bmp, $g, $name) {
    $g.Dispose()
    $o = New-Object System.Drawing.Bitmap(1024, 500, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $h = [System.Drawing.Graphics]::FromImage($o)
    $h.InterpolationMode = 'HighQualityBicubic'; $h.PixelOffsetMode = 'HighQuality'; $h.CompositingQuality = 'HighQuality'
    $h.DrawImage($bmp, 0, 0, 1024, 500)
    $o.Save("$out\$name", [System.Drawing.Imaging.ImageFormat]::Png)
    $h.Dispose(); $o.Dispose(); $bmp.Dispose()
    "  $name"
}

# ---------- 1: reto, a juego con el icono nuevo ----------
foreach ($lang in @(@("portada_1_reto.png", "CAN YOU BEAT", "ALL 70 LEVELS?"), @("portada_1_reto_es.png", "¿PUEDES SUPERAR", "LOS 70 NIVELES?"), @("portada_1_reto_fr.png", "PEUX-TU BATTRE", "LES 70 NIVEAUX ?"))) {
    $c = New-Banner; $bmp = $c[0]; $g = $c[1]
    Fill-Radial $g 170 360 @((C 255 54 4 42), (C 255 150 16 34), (C 255 240 90 20), (C 255 255 210 70)) @(0, .46, .72, 1)
    Fill-Poly $g (Star 170 360 340 180 13 3) (C 255 255 130 24)
    Fill-Poly $g (Star 170 360 250 130 13 8) (C 255 255 188 40)
    Fill-Poly $g (Star 170 360 160 88 11 5) (C 255 255 244 170)
    $sx = 215; $sy = 280; $ss = 132; $deg = 55
    Draw-Bolt $g 0 1.42 2.05 $sx $sy $ss $deg; Draw-Bolt $g .55 .4 1.5 $sx $sy $ss $deg; Draw-Bolt $g (-0.55) .4 1.5 $sx $sy $ss $deg
    Draw-Flame $g $sx $sy $ss $deg 1.75
    Draw-Ship $g $sx $sy $ss $deg (C 255 88 218 255) (C 70 8 60 170)
    Draw-Txt $g "AERO BLITZ" $black 0 104 785 145 $whiteC (C 255 30 120 220) 415
    Draw-Txt $g $lang[1] $black 0 58 785 262 $goldC (C 255 170 50 10) 400; Draw-Txt $g $lang[2] $black 0 58 785 330 $goldC (C 255 170 50 10) 400
    Save-Banner $bmp $g $lang[0]
}

# ---------- 2: la nave rompe el muro indestructible ----------
$c = New-Banner; $bmp = $c[0]; $g = $c[1]
Fill-Radial $g 250 240 @((C 255 8 8 44), (C 255 20 16 96), (C 255 52 40 176), (C 255 90 110 250)) @(0, .5, .8, 1)
$st = $g.Save(); $g.SetClip((New-Object System.Drawing.Rectangle(0, 330, 1024, 110)))
$g.Clear((C 255 16 16 24))
for ($x = -160; $x -lt 1120; $x += 120) {
    Fill-Poly $g ([System.Drawing.PointF[]]@((P $x 330), (P ($x + 60) 330), (P ($x + 170) 440), (P ($x + 110) 440))) (C 255 255 205 20)
}
$g.Restore($st)
$pen = New-Object System.Drawing.Pen((C 255 232 236 250), [single]8)
$g.DrawLine($pen, 0, 330, 1024, 330); $g.DrawLine($pen, 0, 440, 1024, 440); $pen.Dispose()
Fill-Poly $g (Star 245 385 175 120 12 21) (C 255 255 120 20)
Fill-Poly $g (Star 245 385 125 90 12 22) (C 255 255 214 80)
$rnd = New-Object System.Random(7)
for ($i = 0; $i -lt 12; $i++) {
    $a = -[Math]::PI * (.1 + $rnd.NextDouble() * .8); $d = 170 + $rnd.NextDouble() * 130
    $x = 245 + $d * [Math]::Cos($a); $y = 330 + $d * .75 * [Math]::Sin($a); $z = 12 + $rnd.NextDouble() * 15
    $col = if ($i % 2 -eq 0) { C 255 255 205 20 } else { C 255 16 16 24 }
    Fill-Poly $g (Star $x $y $z ($z * .7) 2 ($i + 40)) $col
}
Draw-Flame $g 245 245 150 0 1.6
Draw-Ship $g 245 245 150 0 (C 255 88 218 255) (C 70 8 60 170)
Draw-Txt $g "AERO BLITZ" $black 0 104 785 95 $whiteC (C 255 40 130 240) 415
Draw-Txt $g "BREAK THROUGH" $black 0 50 785 196 $goldC (C 255 120 60 10) 400; Draw-Txt $g "70 LEVELS" $black 0 50 785 258 $goldC (C 255 120 60 10) 400
Save-Banner $bmp $g "portada_2_muro.png"

# ---------- 3: ráfaga de plasma en neón ----------
$c = New-Banner; $bmp = $c[0]; $g = $c[1]
Fill-Radial $g 250 330 @((C 255 16 4 46), (C 255 54 8 112), (C 255 124 14 164), (C 255 214 40 186)) @(0, .5, .8, 1)
foreach ($deg in -36, -18, 0, 18, 36) {
    $st = $g.Save()
    $g.TranslateTransform([single]250, [single]270); $g.RotateTransform([single]$deg)
    $beam = [System.Drawing.PointF[]]@((P -4 0), (P 4 0), (P 13 -300), (P 0 -350), (P -13 -300))
    $pen = New-Object System.Drawing.Pen((C 255 255 30 170), [single]14); $pen.LineJoin = 'Round'; $g.DrawPolygon($pen, $beam); $pen.Dispose()
    Fill-Poly $g $beam (C 255 255 244 255)
    $g.Restore($st)
}
Draw-Flame $g 250 370 135 0 1.5
Draw-Ship $g 250 370 135 0 (C 255 88 218 255) (C 70 8 60 170)
Draw-Txt $g "AERO BLITZ" $black 0 104 785 145 $whiteC (C 255 200 30 170) 415
Draw-Txt $g "70 LEVELS" $black 0 58 785 262 (C 255 110 230 255) (C 255 20 60 140) 400; Draw-Txt $g "10 WORLDS" $black 0 58 785 330 (C 255 110 230 255) (C 255 20 60 140) 400
Save-Banner $bmp $g "portada_3_plasma.png"

# ---------- Comparación con el botón de reproducción que suele poner Google Play encima ----------
$names = @(@("Actual", "D:\Unity\AeroBlitz\StoreAssets\cabecera_1024x500.png"), @("1 - Reto", "$out\portada_1_reto.png"), @("2 - Muro", "$out\portada_2_muro.png"), @("3 - Plasma", "$out\portada_3_plasma.png"))
$sheet = New-Object System.Drawing.Bitmap(1320, 760)
$g = [System.Drawing.Graphics]::FromImage($sheet)
$g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'; $g.TextRenderingHint = 'AntiAlias'
$g.Clear((C 255 32 33 36))
$font = New-Object System.Drawing.Font("Segoe UI", 18, [System.Drawing.FontStyle]::Bold)
for ($i = 0; $i -lt 4; $i++) {
    $x = 20 + ($i % 2) * 650; $y = 12 + [Math]::Floor($i / 2) * 372
    $img = [System.Drawing.Image]::FromFile($names[$i][1])
    $g.DrawString($names[$i][0], $font, [System.Drawing.Brushes]::White, [single]$x, [single]$y)
    $g.DrawImage($img, [single]$x, [single]($y + 38), [single]630, [single]308); $img.Dispose()
    $b = New-Object System.Drawing.SolidBrush((C 170 0 0 0)); $bx = $x + 315; $by = $y + 192
    $g.FillEllipse($b, [single]($bx - 34), [single]($by - 34), [single]68, [single]68); $b.Dispose()
    $g.FillPolygon([System.Drawing.Brushes]::White, [System.Drawing.PointF[]]@((P ($bx - 11) ($by - 17)), (P ($bx - 11) ($by + 17)), (P ($bx + 18) $by)))
}
$sheet.Save("$out\comparacion.png", [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $sheet.Dispose()
"  comparacion.png"
