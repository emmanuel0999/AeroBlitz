# Capturas nuevas para Google Play (1080x1920, PNG de 24 bits), a juego con el icono y la portada
$here = Split-Path $MyInvocation.MyCommand.Path
$u8 = New-Object Text.UTF8Encoding($false)
$src = [IO.File]::ReadAllText("$here\iconos.ps1", $u8)
Invoke-Expression $src.Substring(0, $src.IndexOf('# ---------- A:'))   # reutiliza colores, estrellas y brillos
$root = "D:\Unity\AeroBlitz\StoreAssets"
$out = "$root\capturas_nuevas"
$frames = "$here\capt"
$black = New-Object System.Drawing.FontFamily("Segoe UI Black")
$bold = New-Object System.Drawing.FontFamily("Segoe UI")
$whiteC = C 255 255 255 255; $goldC = C 255 255 205 40
$W = 1080; $H = 1920

function Text-Path($text, $fam, $style, $size) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddString($text, $fam, [int]$style, [single]$size, (P 0 0), [System.Drawing.StringFormat]::GenericTypographic)
    return $p
}

# Tamaño común para que las dos líneas de la frase queden iguales
function Fit-Lines($lines, $fam, $style, $maxSize, $maxW) {
    $widest = 0.0
    foreach ($l in $lines) { $p = Text-Path $l $fam $style 100; $widest = [Math]::Max($widest, [double]$p.GetBounds().Width); $p.Dispose() }
    return [Math]::Min([double]$maxSize, 100.0 * $maxW / $widest)
}

function Draw-Line($g, $text, $fam, $style, $size, $cx, $cy, $fill, $edge) {
    $p = Text-Path $text $fam $style $size
    $b = $p.GetBounds()
    $ref = Text-Path "H" $fam $style $size; $rb = $ref.GetBounds(); $ref.Dispose()
    $m = New-Object System.Drawing.Drawing2D.Matrix
    $m.Translate([single]($cx - $b.X - $b.Width / 2), [single]($cy - $rb.Y - $rb.Height / 2)); $p.Transform($m)
    if ($edge) {
        foreach ($w in 26, 15) {
            $pen = New-Object System.Drawing.Pen((C 120 $edge.R $edge.G $edge.B), [single]$w); $pen.LineJoin = 'Round'; $g.DrawPath($pen, $p); $pen.Dispose()
        }
    }
    $br = New-Object System.Drawing.SolidBrush($fill); $g.FillPath($br, $p); $br.Dispose(); $p.Dispose()
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

# Fondo rojo y naranja del icono: la explosión asoma por los lados del teléfono
function Draw-Back($g, $seed, $bx, $by) {
    $rect = New-Object System.Drawing.Rectangle(0, 0, $W, $H)
    $lg = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, (C 255 70 6 36), (C 255 150 18 30), [single]90)
    $g.FillRectangle($lg, $rect); $lg.Dispose()
    Fill-Poly $g (Star $bx $by 1050 560 13 $seed) (C 255 235 84 22)
    Fill-Poly $g (Star $bx $by 820 440 13 ($seed + 1)) (C 255 255 150 30)
    Fill-Poly $g (Star $bx $by 640 340 12 ($seed + 2)) (C 255 255 222 110)
    # La zona de la frase se oscurece para que el texto se lea siempre bien
    $top = New-Object System.Drawing.Rectangle(0, 0, $W, 440)
    $sh = New-Object System.Drawing.Drawing2D.LinearGradientBrush($top, (C 255 54 4 34), (C 0 54 4 34), [single]90)
    $g.FillRectangle($sh, $top); $sh.Dispose()
}

# Marco de teléfono; devuelve el recorte interior para dibujar dentro
function Draw-Phone($g, $x, $y, $w, $h) {
    Glow $g ($x + $w / 2) ($y + $h / 2) ($h * .62) (C 110 20 0 20)
    $outer = Round-Path ($x - 14) ($y - 14) ($w + 28) ($h + 28) 58
    $b = New-Object System.Drawing.SolidBrush((C 255 10 10 26)); $g.FillPath($b, $outer); $b.Dispose()
    $pen = New-Object System.Drawing.Pen((C 255 255 226 150), [single]6); $g.DrawPath($pen, $outer); $pen.Dispose(); $outer.Dispose()
    return (Round-Path $x $y $w $h 44)
}

function Draw-Caption($g, $lines, $accent) {
    $size = Fit-Lines $lines $black 0 128 980
    $lh = $size * 1.16
    $cy = 205 - ($lines.Count - 1) * $lh / 2
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $fill = if ($i -eq $accent) { $goldC } else { $whiteC }
        Draw-Line $g $lines[$i] $black 0 $size 540 ($cy + $i * $lh) $fill (C 255 90 8 6)
    }
}

function New-Shot {
    $bmp = New-Object System.Drawing.Bitmap($W, $H, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'
    $g.PixelOffsetMode = 'HighQuality'; $g.CompositingQuality = 'HighQuality'
    return ,@($bmp, $g)
}

# Captura de partida a pantalla completa dentro del teléfono
function Shot-Game($path, $lines, $accent, $frame, $seed, $bx, $by) {
    $c = New-Shot; $bmp = $c[0]; $g = $c[1]
    Draw-Back $g $seed $bx $by
    $clip = Draw-Phone $g 118 390 844 1500
    $img = [System.Drawing.Image]::FromFile($frame)
    $g.SetClip($clip); $g.DrawImage($img, 118, 390, 844, 1500); $g.ResetClip(); $img.Dispose(); $clip.Dispose()
    Draw-Caption $g $lines $accent
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $bmp.Dispose()
}

function Shot-Worlds($path, $lines, $accent, $names) {
    $c = New-Shot; $bmp = $c[0]; $g = $c[1]
    Draw-Back $g 61 540 1250
    $clip = Draw-Phone $g 118 390 844 1500
    $g.SetClip($clip)
    $b = New-Object System.Drawing.SolidBrush((C 255 8 8 22)); $g.FillRectangle($b, 118, 390, 844, 1500); $b.Dispose()
    $files = "mundo_02_niveles_8-14", "mundo_03_niveles_15-21", "mundo_05_niveles_29-35", "mundo_07_niveles_43-49", "mundo_08_niveles_50-56", "mundo_10_niveles_64-70"
    $cw = 416; $ch = 492
    for ($i = 0; $i -lt 6; $i++) {
        $x = 118 + ($i % 2) * ($cw + 12); $y = 390 + [Math]::Floor($i / 2) * ($ch + 12)
        $img = [System.Drawing.Image]::FromFile("D:\Unity\AeroBlitz\WorldPreview\$($files[$i]).png")
        $dst = New-Object System.Drawing.Rectangle($x, $y, $cw, $ch)
        $srcR = New-Object System.Drawing.Rectangle(0, 461, 540, 639)
        $g.DrawImage($img, $dst, $srcR, [System.Drawing.GraphicsUnit]::Pixel); $img.Dispose()
        $band = New-Object System.Drawing.SolidBrush((C 190 6 6 18)); $g.FillRectangle($band, $x, ($y + $ch - 70), $cw, 70); $band.Dispose()
        $s = Fit-Lines @($names[$i]) $bold 1 36 ($cw - 36)
        Draw-Line $g $names[$i] $bold 1 $s ($x + $cw / 2) ($y + $ch - 35) $whiteC $null
    }
    $g.ResetClip(); $clip.Dispose()
    Draw-Caption $g $lines $accent
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $bmp.Dispose()
}

function Shot-Hangar($path, $lines, $accent, $hangar) {
    $c = New-Shot; $bmp = $c[0]; $g = $c[1]
    Draw-Back $g 81 540 1150
    $clip = Draw-Phone $g 195 390 690 1500
    $img = [System.Drawing.Image]::FromFile($hangar)
    $g.SetClip($clip)
    $dst = New-Object System.Drawing.Rectangle(195, 390, 690, 1500)
    $srcR = New-Object System.Drawing.Rectangle(190, 336, 700, 1522)
    $g.DrawImage($img, $dst, $srcR, [System.Drawing.GraphicsUnit]::Pixel)
    $g.ResetClip(); $img.Dispose(); $clip.Dispose()
    Draw-Caption $g $lines $accent
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $bmp.Dispose()
}

$langs = [ordered]@{
    en = @{ c1 = @("CAN YOU BEAT", "ALL 70 LEVELS?"); c2 = @("MISSILES THAT", "SMASH WALLS"); c3 = @("CHARGE. FIRE.", "SURVIVE."); c4 = @("10 WORLDS", "70 LEVELS"); c5 = @("UPGRADE", "YOUR SHIP")
        hangar = "$root\capturas_tienda_en\04_hangar.png"
        worlds = @("Ice Rings", "Crimson Nebula", "Solar Flare", "Laser Corridor", "Plasma Sea", "Galactic Core") }
    es = @{ c1 = @("¿PUEDES SUPERAR", "LOS 70 NIVELES?"); c2 = @("MISILES QUE", "ROMPEN MUROS"); c3 = @("CARGA. DISPARA.", "SOBREVIVE."); c4 = @("10 MUNDOS", "70 NIVELES"); c5 = @("MEJORA", "TU NAVE")
        hangar = "$root\capturas_tienda\04_hangar.png"
        worlds = @("Anillos de Hielo", "Nebulosa Carmesí", "Llamarada Solar", "Corredor de Láseres", "Mar de Plasma", "Núcleo Galáctico") }
    fr = @{ c1 = @("PEUX-TU BATTRE", "LES 70 NIVEAUX ?"); c2 = @("DES MISSILES QUI", "BRISENT LES MURS"); c3 = @("CHARGE. TIRE.", "SURVIS."); c4 = @("10 MONDES", "70 NIVEAUX"); c5 = @("AMÉLIORE", "TON VAISSEAU")
        hangar = "$root\capturas_tienda_fr\04_hangar.png"
        worlds = @("Anneaux de Glace", "Nébuleuse Pourpre", "Éruption Solaire", "Couloir Laser", "Mer de Plasma", "Noyau Galactique") }
}

foreach ($k in $langs.Keys) {
    $l = $langs[$k]; $d = "$out\$k"
    New-Item -ItemType Directory -Force $d | Out-Null
    Shot-Game "$d\01_reto.png" $l.c1 1 "$frames\w70\f_07.0.jpg" 21 540 1150
    Shot-Game "$d\02_misiles.png" $l.c2 0 "$frames\b\f_11.5.jpg" 31 300 1300
    Shot-Game "$d\03_cargar.png" $l.c3 1 "$frames\a\f_05.0.jpg" 41 780 1250
    Shot-Worlds "$d\04_mundos.png" $l.c4 0 $l.worlds
    Shot-Hangar "$d\05_mejoras.png" $l.c5 1 $l.hangar
    "  $k listo"
}

# Hoja para revisar las 5 en inglés
$list = @("$out\en\01_reto.png", "$out\en\02_misiles.png", "$out\en\03_cargar.png", "$out\en\04_mundos.png", "$out\en\05_mejoras.png")
$sheet = New-Object System.Drawing.Bitmap(1640, 590, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
$g = [System.Drawing.Graphics]::FromImage($sheet); $g.InterpolationMode = 'HighQualityBicubic'; $g.PixelOffsetMode = 'HighQuality'
$g.Clear((C 255 32 33 36))
for ($i = 0; $i -lt 5; $i++) { $img = [System.Drawing.Image]::FromFile($list[$i]); $g.DrawImage($img, 20 + $i * 325, 20, 310, 551); $img.Dispose() }
$sheet.Save("$out\comparacion.png", [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $sheet.Dispose()
"  comparacion.png"
