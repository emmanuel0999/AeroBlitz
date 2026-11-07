# Publicaciones de Instagram (1080x1350, formato 4:5) con los colores nuevos del icono
$tools = "D:\Unity\AeroBlitz\TrailerCaptures\herramientas"
$u8 = New-Object Text.UTF8Encoding($false)
$src = [IO.File]::ReadAllText("$tools\iconos.ps1", $u8)
Invoke-Expression $src.Substring(0, $src.IndexOf('# ---------- A:'))
$cap = [IO.File]::ReadAllText("$tools\capturas_nuevas.ps1", $u8)
$a = $cap.IndexOf('function Text-Path'); $b = $cap.IndexOf('function Draw-Caption')
Invoke-Expression $cap.Substring($a, $b - $a)   # Text-Path, Fit-Lines, Draw-Line, Round-Path, Draw-Back, Draw-Phone
$W = 1080; $H = 1350
$black = New-Object System.Drawing.FontFamily("Segoe UI Black")
$bold = New-Object System.Drawing.FontFamily("Segoe UI")
$whiteC = C 255 255 255 255; $goldC = C 255 255 205 40; $edgeC = C 255 90 8 6
$out = "D:\Unity\AeroBlitz\StoreAssets\instagram"; $fr = "$out\fuentes"
New-Item -ItemType Directory -Force $fr | Out-Null
$oldCapt = "C:\Users\EMMANU~1\AppData\Local\Temp\claude\D--Unity\06285d42-49b6-47b3-bf14-e6ea60ee01cb\scratchpad\capt"
Copy-Item "$oldCapt\w70\f_07.0.jpg" "$fr\nivel70.jpg" -Force
Copy-Item "$oldCapt\b\f_11.5.jpg" "$fr\misiles.jpg" -Force
$iconPath = "D:\Unity\AeroBlitz\StoreAssets\icono_512.png"

function New-Post {
    $bmp = New-Object System.Drawing.Bitmap($W, $H, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'
    $g.PixelOffsetMode = 'HighQuality'; $g.CompositingQuality = 'HighQuality'
    return ,@($bmp, $g)
}

function Two-Lines($g, $l1, $l2, $cy, $maxSize, $maxW) {
    $s = Fit-Lines @($l1, $l2) $black 0 $maxSize $maxW
    Draw-Line $g $l1 $black 0 $s 540 ($cy - $s * 0.58) $whiteC $edgeC
    Draw-Line $g $l2 $black 0 $s 540 ($cy + $s * 0.62) $goldC $edgeC
}

function Draw-Icon2($g, $x, $y, $size) {
    $sh = Round-Path ($x - 8) ($y - 8) ($size + 16) ($size + 16) ($size * 0.22)
    $b2 = New-Object System.Drawing.SolidBrush((C 255 255 226 150)); $g.FillPath($b2, $sh); $b2.Dispose(); $sh.Dispose()
    $ip = Round-Path $x $y $size $size ($size * 0.2)
    $icon = [System.Drawing.Image]::FromFile($iconPath)
    $g.SetClip($ip); $g.DrawImage($icon, [single]$x, [single]$y, [single]$size, [single]$size); $g.ResetClip(); $icon.Dispose(); $ip.Dispose()
}

function Draw-Pill($g, $text, $cx, $cy, $w, $h) {
    $p = Round-Path ($cx - $w / 2) ($cy - $h / 2) $w $h ($h / 2)
    $b2 = New-Object System.Drawing.SolidBrush($goldC); $g.FillPath($b2, $p); $b2.Dispose(); $p.Dispose()
    $s = Fit-Lines @($text) $black 0 ($h * 0.42) ($w - 70)
    Draw-Line $g $text $black 0 $s $cx $cy (C 255 40 6 14) $null
}

# Banda inferior con el icono, el nombre y dónde descargarlo
function Draw-Footer($g, $y) {
    Draw-Icon2 $g 70 ($y - 70) 140
    $s = Fit-Lines @("AERO BLITZ") $black 0 60 360
    Draw-Line $g "AERO BLITZ" $black 0 $s 445 ($y - 28) $whiteC $edgeC
    Draw-Line $g "SPACE SHOOTER" $bold 1 30 445 ($y + 26) $goldC $edgeC
    Draw-Pill $g "FREE ON GOOGLE PLAY" 860 $y 360 92
}

# 1. Reto: el nivel 70 dentro del teléfono
$c = New-Post; $bmp = $c[0]; $g = $c[1]
Draw-Back $g 21 540 760
Two-Lines $g "CAN YOU BEAT" "ALL 70 LEVELS?" 160 110 960
$clip = Draw-Phone $g 318 320 444 790
$img = [System.Drawing.Image]::FromFile("$fr\nivel70.jpg")
$g.SetClip($clip); $g.DrawImage($img, 318, 320, 444, 790); $g.ResetClip(); $img.Dispose(); $clip.Dispose()
Draw-Footer $g 1225
$bmp.Save("$out\opcion_1_reto.png", [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $bmp.Dispose()
"  opcion_1_reto.png"

# 2. Acción a pantalla completa: los misiles rompiendo muros
$c = New-Post; $bmp = $c[0]; $g = $c[1]
$img = [System.Drawing.Image]::FromFile("$fr\misiles.jpg")
$srcR = New-Object System.Drawing.Rectangle(0, 570, 1080, 1350)   # la nave queda visible encima de la banda inferior
$g.DrawImage($img, (New-Object System.Drawing.Rectangle(0, 0, $W, $H)), $srcR, [System.Drawing.GraphicsUnit]::Pixel); $img.Dispose()
$st = $g.Save(); $g.TranslateTransform(540, 190); $g.RotateTransform(-3)
$rp = Round-Path -470 -130 940 260 34
$lg = New-Object System.Drawing.Drawing2D.LinearGradientBrush((P -470 -130), (P 470 130), (C 255 150 10 34), (C 255 232 70 22))
$g.FillPath($lg, $rp); $lg.Dispose()
$pen = New-Object System.Drawing.Pen($goldC, [single]9); $g.DrawPath($pen, $rp); $pen.Dispose(); $rp.Dispose()
$g.Restore($st)
$g.TranslateTransform(0, 0)
$st = $g.Save(); $g.TranslateTransform(540, 190); $g.RotateTransform(-3); $g.TranslateTransform(-540, -190)
Two-Lines $g "HOMING MISSILES" "SMASH WALLS" 190 104 840
$g.Restore($st)
$band = New-Object System.Drawing.Drawing2D.LinearGradientBrush((New-Object System.Drawing.Rectangle(0, 1000, $W, 141)), (C 0 30 4 16), (C 255 30 4 16), [single]90)
$g.FillRectangle($band, 0, 1000, $W, 140); $band.Dispose()
$solid = New-Object System.Drawing.SolidBrush((C 255 30 4 16)); $g.FillRectangle($solid, 0, 1139, $W, 211); $solid.Dispose()   # tapa los textos pequeños de la interfaz
Draw-Footer $g 1235
$bmp.Save("$out\opcion_2_misiles.png", [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $bmp.Dispose()
"  opcion_2_misiles.png"

# 3. Pregunta para comentar: los mundos del juego
$c = New-Post; $bmp = $c[0]; $g = $c[1]
Draw-Back $g 61 540 760
Two-Lines $g "WHICH WORLD" "IS YOUR FAVORITE?" 160 104 960
$files = "mundo_02_niveles_8-14", "mundo_03_niveles_15-21", "mundo_05_niveles_29-35", "mundo_07_niveles_43-49", "mundo_08_niveles_50-56", "mundo_10_niveles_64-70"
$names = "Ice Rings", "Crimson Nebula", "Solar Flare", "Laser Corridor", "Plasma Sea", "Galactic Core"
$gx = 90; $gy = 310; $cw = 292; $ch = 360; $gap = 12
$frame = Round-Path ($gx - 14) ($gy - 14) (3 * $cw + 2 * $gap + 28) (2 * $ch + $gap + 28) 40
$b2 = New-Object System.Drawing.SolidBrush((C 255 10 10 26)); $g.FillPath($b2, $frame); $b2.Dispose()
$pen = New-Object System.Drawing.Pen((C 255 255 226 150), [single]6); $g.DrawPath($pen, $frame); $pen.Dispose(); $frame.Dispose()
for ($i = 0; $i -lt 6; $i++) {
    $x = $gx + ($i % 3) * ($cw + $gap); $y = $gy + [Math]::Floor($i / 3) * ($ch + $gap)
    $wimg = [System.Drawing.Image]::FromFile("D:\Unity\AeroBlitz\WorldPreview\$($files[$i]).png")
    $g.DrawImage($wimg, (New-Object System.Drawing.Rectangle($x, $y, $cw, $ch)), (New-Object System.Drawing.Rectangle(0, 440, 540, 703)), [System.Drawing.GraphicsUnit]::Pixel); $wimg.Dispose()
    $bb = New-Object System.Drawing.SolidBrush((C 255 8 6 18)); $g.FillRectangle($bb, $x, ($y + $ch - 58), $cw, 58); $bb.Dispose()
    $s = Fit-Lines @($names[$i]) $bold 1 30 ($cw - 30)
    Draw-Line $g $names[$i] $bold 1 $s ($x + $cw / 2) ($y + $ch - 29) $whiteC $null
}
Draw-Line $g "COMMENT BELOW" $black 0 44 540 1112 $goldC $edgeC
Draw-Footer $g 1240
$bmp.Save("$out\opcion_3_mundos.png", [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $bmp.Dispose()
"  opcion_3_mundos.png"

# 4. Presentación del juego: el icono en grande
$c = New-Post; $bmp = $c[0]; $g = $c[1]
Draw-Back $g 91 540 560
Draw-Icon2 $g 340 170 400
$s = Fit-Lines @("AERO BLITZ") $black 0 150 900
Draw-Line $g "AERO BLITZ" $black 0 $s 540 720 $whiteC $edgeC
Draw-Line $g "SPACE SHOOTER" $bold 1 56 540 830 $goldC $edgeC
$dot = [string][char]0x2022
Draw-Line $g "70 LEVELS  $dot  10 WORLDS  $dot  GLOBAL RANKING" $bold 1 36 540 930 $whiteC $edgeC
Draw-Pill $g "FREE ON GOOGLE PLAY" 540 1080 640 116
$bmp.Save("$out\opcion_4_presentacion.png", [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $bmp.Dispose()
"  opcion_4_presentacion.png"

# Hoja de comparación, con el recorte 3:4 de la cuadrícula del perfil marcado
$list = "opcion_1_reto", "opcion_2_misiles", "opcion_3_mundos", "opcion_4_presentacion"
$sheet = New-Object System.Drawing.Bitmap(1720, 600, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
$g = [System.Drawing.Graphics]::FromImage($sheet); $g.InterpolationMode = 'HighQualityBicubic'; $g.SmoothingMode = 'AntiAlias'
$g.Clear((C 255 32 33 36))
for ($i = 0; $i -lt 4; $i++) {
    $im = [System.Drawing.Image]::FromFile("$out\$($list[$i]).png"); $x = 20 + $i * 425
    $g.DrawImage($im, $x, 20, 405, 506); $im.Dispose()
    $pen = New-Object System.Drawing.Pen((C 200 80 220 255), [single]2); $pen.DashStyle = 'Dash'
    $g.DrawRectangle($pen, [single]($x + 12.7), 20, [single](405 - 25.4), 506); $pen.Dispose()
    $g.DrawString("$($i + 1)", (New-Object System.Drawing.Font("Segoe UI", 20, [System.Drawing.FontStyle]::Bold)), [System.Drawing.Brushes]::White, [single]($x + 190), 540)
}
$sheet.Save("$out\comparacion.png", [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $sheet.Dispose()
"  comparacion.png"
