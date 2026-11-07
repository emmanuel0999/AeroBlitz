Add-Type -AssemblyName System.Drawing
$ErrorActionPreference = 'Stop'
$out = "D:\Unity\AeroBlitz\TrailerCaptures\rotulos"
$final = "D:\Unity\AeroBlitz\TrailerCaptures"
New-Item -ItemType Directory -Force $out | Out-Null
$icon = [System.Drawing.Image]::FromFile("D:\Unity\AeroBlitz\StoreAssets\icono_512.png")
$famBlack = New-Object System.Drawing.FontFamily("Segoe UI Black")
$famBold = New-Object System.Drawing.FontFamily("Segoe UI")
$bullet = [string][char]0x2022

function C($a, $r, $g, $b) { [System.Drawing.Color]::FromArgb($a, $r, $g, $b) }
$white = C 255 255 255 255
$cyan = C 255 80 220 255
$gold = C 255 255 200 50
$soft = C 255 200 210 235

function New-Canvas($w, $h) {
    $bmp = New-Object System.Drawing.Bitmap($w, $h, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'
    $g.PixelOffsetMode = 'HighQuality'; $g.CompositingQuality = 'HighQuality'
    return , @($bmp, $g)
}

function Draw-Glow($g, $cx, $cy, $r, $color) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddEllipse([single]($cx - $r), [single]($cy - $r), [single](2 * $r), [single](2 * $r))
    $pb = New-Object System.Drawing.Drawing2D.PathGradientBrush($path)
    $pb.CenterColor = $color
    $pb.SetSigmaBellShape([single]1.0, [single]1.0)
    $pb.SurroundColors = [System.Drawing.Color[]]@((C 0 $color.R $color.G $color.B))
    $g.FillPath($pb, $path); $pb.Dispose(); $path.Dispose()
}

function Draw-Space($g, $w, $h, $seed) {
    $rect = New-Object System.Drawing.Rectangle(0, 0, $w, $h)
    $lg = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, (C 255 6 9 28), (C 255 22 12 48), [single]90)
    $g.FillRectangle($lg, $rect); $lg.Dispose()
    $m = [Math]::Min($w, $h)
    Draw-Glow $g ($w * 0.14) ($h * 0.24) ($m * 0.55) (C 105 30 170 255)
    Draw-Glow $g ($w * 0.86) ($h * 0.80) ($m * 0.60) (C 95 220 60 200)
    Draw-Glow $g ($w * 0.88) ($h * 0.14) ($m * 0.32) (C 60 255 150 50)
    $rnd = New-Object System.Random($seed)
    $n = [int]($w * $h / 6500)
    for ($i = 0; $i -lt $n; $i++) {
        $s = [single](1 + $rnd.NextDouble() * 2.4)
        $b = New-Object System.Drawing.SolidBrush((C (80 + $rnd.Next(175)) 255 255 255))
        $g.FillEllipse($b, [single]$rnd.Next($w), [single]$rnd.Next($h), $s, $s); $b.Dispose()
    }
}

function Text-Path($text, $fam, $size) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $style = if ($fam -eq $famBold) { 1 } else { 0 }
    $p.AddString($text, $fam, $style, [single]$size, (New-Object System.Drawing.PointF(0, 0)), [System.Drawing.StringFormat]::GenericTypographic)
    return $p
}

function Text-Width($text, $fam, $size) { $p = Text-Path $text $fam $size; $w = $p.GetBounds().Width; $p.Dispose(); return $w }

# Tamaño de letra para que la línea más ancha quepa en maxW
function Fit($lines, $fam, $maxSize, $maxW) {
    $widest = 0
    foreach ($l in $lines) { $widest = [Math]::Max($widest, (Text-Width $l $fam 100)) }
    return [Math]::Min($maxSize, 100 * $maxW / $widest)
}

# Texto centrado en (cx, cy); la altura de referencia es la de las mayúsculas para alinear bien varias líneas
function Draw-Text($g, $text, $fam, $size, $cx, $cy, $fill, $glow) {
    $p = Text-Path $text $fam $size
    $b = $p.GetBounds()
    $ref = Text-Path "H" $fam $size; $rb = $ref.GetBounds(); $ref.Dispose()
    $mx = New-Object System.Drawing.Drawing2D.Matrix
    $mx.Translate([single]($cx - $b.X - $b.Width / 2), [single]($cy - $rb.Y - $rb.Height / 2))
    $p.Transform($mx)
    if ($glow) {
        foreach ($wd in 22, 14, 8) {
            $pen = New-Object System.Drawing.Pen((C 50 $glow.R $glow.G $glow.B), [single]($wd * $size / 100))
            $pen.LineJoin = 'Round'; $g.DrawPath($pen, $p); $pen.Dispose()
        }
    }
    $br = New-Object System.Drawing.SolidBrush($fill); $g.FillPath($br, $p); $br.Dispose(); $p.Dispose()
}

function Round-Rect($x, $y, $w, $h, $r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = [single](2 * $r); $x = [single]$x; $y = [single]$y; $w = [single]$w; $h = [single]$h
    $p.AddArc($x, $y, $d, $d, [single]180, [single]90)
    $p.AddArc([single]($x + $w - $d), $y, $d, $d, [single]270, [single]90)
    $p.AddArc([single]($x + $w - $d), [single]($y + $h - $d), $d, $d, [single]0, [single]90)
    $p.AddArc($x, [single]($y + $h - $d), $d, $d, [single]90, [single]90)
    $p.CloseFigure()
    return $p
}

function Draw-Icon($g, $cx, $cy, $size) {
    Draw-Glow $g $cx $cy ($size * 0.9) (C 90 60 200 255)
    $x = $cx - $size / 2; $y = $cy - $size / 2
    $rp = Round-Rect $x $y $size $size ($size * 0.18)
    $g.SetClip($rp)
    $g.DrawImage($icon, [single]$x, [single]$y, [single]$size, [single]$size)
    $g.ResetClip()
    $pen = New-Object System.Drawing.Pen((C 160 120 230 255), [single]([Math]::Max(2, $size / 90)))
    $g.DrawPath($pen, $rp); $pen.Dispose(); $rp.Dispose()
}

function Draw-Pill($g, $text, $cx, $cy, $size) {
    $w = (Text-Width $text $famBold $size) + $size * 1.7
    $h = $size * 2.0
    $rp = Round-Rect ($cx - $w / 2) ($cy - $h / 2) $w $h ($h / 2)
    $br = New-Object System.Drawing.SolidBrush($gold); $g.FillPath($br, $rp); $br.Dispose(); $rp.Dispose()
    Draw-Text $g $text $famBold $size $cx $cy (C 255 18 16 34) $null
}

function Draw-Branding($g, $cx, $top, $maxW) {
    Draw-Icon $g $cx ($top + 125) 230
    $s = Fit @("AERO BLITZ") $famBlack 96 $maxW
    Draw-Text $g "AERO BLITZ" $famBlack $s $cx ($top + 330) $white $cyan
    Draw-Text $g "SPACE SHOOTER" $famBold 40 $cx ($top + 410) $cyan $null
    Draw-Pill $g "FREE ON GOOGLE PLAY" $cx ($top + 540) 30
}

# Rótulo 1920x1080: hueco central de 608 px para el juego, marca a la izquierda y frase a la derecha
function Make-Rotulo($file, $lines, $accentIdx) {
    $c = New-Canvas 1920 1080; $bmp = $c[0]; $g = $c[1]
    Draw-Space $g 1920 1080 11
    $slotX = 656; $slotW = 608
    for ($k = 14; $k -ge 1; $k--) {
        $pen = New-Object System.Drawing.Pen((C ([int](80 - $k * 5)) 60 210 255), [single]3)
        $g.DrawLine($pen, [single]($slotX - $k * 3), [single]0, [single]($slotX - $k * 3), [single]1080)
        $g.DrawLine($pen, [single]($slotX + $slotW + $k * 3), [single]0, [single]($slotX + $slotW + $k * 3), [single]1080)
        $pen.Dispose()
    }
    $dark = New-Object System.Drawing.SolidBrush((C 255 4 5 14)); $g.FillRectangle($dark, $slotX, 0, $slotW, 1080); $dark.Dispose()

    Draw-Branding $g 328 230 520

    $size = Fit $lines $famBlack 118 540
    $lh = $size * 1.18
    $cy = 540 - ($lines.Count - 1) * $lh / 2
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $accent = $accentIdx -contains $i
        $fill = if ($accent) { $gold } else { $white }
        $glow = if ($accent) { C 255 255 120 30 } else { $cyan }
        Draw-Text $g $lines[$i] $famBlack $size 1592 ($cy + $i * $lh) $fill $glow
    }
    $bmp.Save("$out\$file", [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
    "  $file  (letra $([int]$size))"
}

Make-Rotulo "rotulo_1_esquiva.png"  @("DODGE.", "CHARGE.", "BLAST!") @(2)
Make-Rotulo "rotulo_2_misiles.png"  @("MISSILES", "THAT SMASH", "ANYTHING") @(0)
Make-Rotulo "rotulo_3_mundos.png"   @("70 LEVELS", "10 WORLDS") @(1)
Make-Rotulo "rotulo_4_mejoras.png"  @("GRAB COINS", "& POWER-UPS") @(1)
Make-Rotulo "rotulo_5_armas.png"    @("UNLOCK", "EPIC", "WEAPONS") @(1)
Make-Rotulo "rotulo_6_ranking.png"  @("CLIMB THE", "GLOBAL", "RANKING") @(1)

$features = "70 LEVELS  $bullet  10 WORLDS  $bullet  GLOBAL RANKING"

# Pantalla final horizontal (Google Play / YouTube)
$c = New-Canvas 1920 1080; $bmp = $c[0]; $g = $c[1]
Draw-Space $g 1920 1080 23
Draw-Icon $g 960 290 300
$s = Fit @("AERO BLITZ") $famBlack 150 1100
Draw-Text $g "AERO BLITZ" $famBlack $s 960 560 $white $cyan
Draw-Text $g "SPACE SHOOTER" $famBold 54 960 660 $cyan $null
Draw-Text $g $features $famBold 34 960 745 $soft $null
Draw-Pill $g "FREE ON GOOGLE PLAY" 960 880 44
$bmp.Save("$final\final_horizontal.png", [System.Drawing.Imaging.ImageFormat]::Png)
# Miniatura opcional para YouTube (1280x720)
$thumb = New-Object System.Drawing.Bitmap(1280, 720)
$tg = [System.Drawing.Graphics]::FromImage($thumb); $tg.InterpolationMode = 'HighQualityBicubic'
$tg.DrawImage($bmp, 0, 0, 1280, 720); $thumb.Save("$final\miniatura_youtube.png", [System.Drawing.Imaging.ImageFormat]::Png)
$tg.Dispose(); $thumb.Dispose(); $g.Dispose(); $bmp.Dispose()
"  final_horizontal.png + miniatura_youtube.png"

# Pantalla final vertical (TikTok / Shorts)
$c = New-Canvas 1080 1920; $bmp = $c[0]; $g = $c[1]
Draw-Space $g 1080 1920 31
Draw-Icon $g 540 600 380
$s = Fit @("AERO BLITZ") $famBlack 150 940
Draw-Text $g "AERO BLITZ" $famBlack $s 540 940 $white $cyan
Draw-Text $g "SPACE SHOOTER" $famBold 58 540 1050 $cyan $null
Draw-Text $g "70 LEVELS  $bullet  10 WORLDS" $famBold 40 540 1150 $soft $null
Draw-Text $g "GLOBAL RANKING" $famBold 40 540 1215 $soft $null
Draw-Pill $g "FREE ON GOOGLE PLAY" 540 1400 46
$bmp.Save("$final\final_vertical.png", [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()
"  final_vertical.png"

# Versión "reto": pregunta al inicio, dificultad al perder y desafío final
Make-Rotulo "rotulo_0_reto.png"     @("THINK YOU", "CAN BEAT ALL", "70 LEVELS?") @(2)
Make-Rotulo "rotulo_7_dificil.png"  @("IT ONLY", "GETS", "HARDER") @(2)

function Make-FinalReto($file, $w, $h, $seed) {
    $c = New-Canvas $w $h; $bmp = $c[0]; $g = $c[1]
    Draw-Space $g $w $h $seed
    $cx = $w / 2
    $vertical = $h -gt $w
    $top = if ($vertical) { 330 } else { 0 }
    $size = Fit @("BET YOU CAN'T", "BEAT THEM ALL.") $famBlack 120 ($w * 0.86)
    Draw-Text $g "BET YOU CAN'T" $famBlack $size $cx ($top + 170) $white $cyan
    Draw-Text $g "BEAT THEM ALL." $famBlack $size $cx ($top + 170 + $size * 1.2) (C 255 255 200 50) (C 255 255 120 30)
    $iy = $top + 170 + $size * 1.2 + ($(if ($vertical) { 330 } else { 230 }))
    $isz = if ($vertical) { 300 } else { 200 }
    Draw-Icon $g $cx $iy $isz
    $ty = $iy + $isz / 2 + 80
    Draw-Text $g "AERO BLITZ" $famBlack 76 $cx $ty $white $cyan
    Draw-Text $g "SPACE SHOOTER" $famBold 34 $cx ($ty + 62) $cyan $null
    Draw-Pill $g "FREE ON GOOGLE PLAY" $cx ($ty + 160) 40
    $bmp.Save("$final\$file", [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
    "  $file"
}
Make-FinalReto "final_reto_horizontal.png" 1920 1080 41
# La miniatura de YouTube es la del reto: se escribe aquí para que no la pise la versión genérica de arriba
$rf = [System.Drawing.Image]::FromFile("$final\final_reto_horizontal.png"); $thumb = New-Object System.Drawing.Bitmap(1280, 720); $tg = [System.Drawing.Graphics]::FromImage($thumb); $tg.InterpolationMode = 'HighQualityBicubic'; $tg.DrawImage($rf, 0, 0, 1280, 720); $thumb.Save("$final\miniatura_youtube.png", [System.Drawing.Imaging.ImageFormat]::Png); $tg.Dispose(); $thumb.Dispose(); $rf.Dispose()
Make-FinalReto "final_reto_vertical.png" 1080 1920 43
$icon.Dispose()
