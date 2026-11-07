# Anuncio de TikTok con los colores nuevos: partida a pantalla completa, frases en etiquetas rojas y pantalla final
$here = Split-Path $MyInvocation.MyCommand.Path
$u8 = New-Object Text.UTF8Encoding($false)
$src = [IO.File]::ReadAllText("$here\iconos.ps1", $u8)
Invoke-Expression $src.Substring(0, $src.IndexOf('# ---------- A:'))
$cap = [IO.File]::ReadAllText("$here\capturas_nuevas.ps1", $u8)
$a = $cap.IndexOf('function Text-Path'); $b = $cap.IndexOf('function Draw-Phone')
Invoke-Expression $cap.Substring($a, $b - $a)   # Text-Path, Fit-Lines, Draw-Line, Round-Path y Draw-Back
$W = 1080; $H = 1920
$black = New-Object System.Drawing.FontFamily("Segoe UI Black")
$bold = New-Object System.Drawing.FontFamily("Segoe UI")
$whiteC = C 255 255 255 255; $goldC = C 255 255 205 40; $darkC = C 255 60 4 12
$tk = "D:\Unity\AeroBlitz\TikTok"; $f = "$tk\fuentes\promo"
New-Item -ItemType Directory -Force $f | Out-Null

# Etiqueta inclinada con la frase. Solo colores sólidos: el montador no mezcla bien la semitransparencia
function Make-Sticker($file, $l1, $l2) {
    $bmp = New-Object System.Drawing.Bitmap($W, 400, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.PixelOffsetMode = 'HighQuality'
    $g.Clear([System.Drawing.Color]::FromArgb(0, 0, 0, 0))
    $g.TranslateTransform(540, 200); $g.RotateTransform(-3)
    $rp = Round-Path -480 -150 960 300 34
    $lg = New-Object System.Drawing.Drawing2D.LinearGradientBrush((P -480 -150), (P 480 150), (C 255 150 10 34), (C 255 232 70 22))
    $g.FillPath($lg, $rp); $lg.Dispose()
    $pen = New-Object System.Drawing.Pen($goldC, [single]9); $g.DrawPath($pen, $rp); $pen.Dispose(); $rp.Dispose()
    $size = Fit-Lines @($l1, $l2) $black 0 116 860
    foreach ($row in @(@($l1, -62, $whiteC), @($l2, 66, $goldC))) {
        $p = Text-Path $row[0] $black 0 $size
        $bb = $p.GetBounds()
        $ref = Text-Path "H" $black 0 $size; $rb = $ref.GetBounds(); $ref.Dispose()
        $m = New-Object System.Drawing.Drawing2D.Matrix; $m.Translate([single](0 - $bb.X - $bb.Width / 2), [single]($row[1] - $rb.Y - $rb.Height / 2)); $p.Transform($m)
        $pen = New-Object System.Drawing.Pen($darkC, [single]16); $pen.LineJoin = 'Round'; $g.DrawPath($pen, $p); $pen.Dispose()
        $br = New-Object System.Drawing.SolidBrush($row[2]); $g.FillPath($br, $p); $br.Dispose(); $p.Dispose()
    }
    $bmp.Save("$f\$file", [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $bmp.Dispose()
    "  $file (letra $([int]$size))"
}

Make-Sticker "t1.png" "CAN YOU BEAT" "LEVEL 70?"
Make-Sticker "t2.png" "CHARGE. FIRE." "SURVIVE."
Make-Sticker "t3.png" "MISSILES THAT" "SMASH WALLS"
Make-Sticker "t4.png" "IT ONLY GETS" "HARDER"

# Pantalla final en rojo y naranja; lo importante va en el centro para que no lo tapen los botones de TikTok
$bmp = New-Object System.Drawing.Bitmap($W, $H, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'; $g.PixelOffsetMode = 'HighQuality'
Draw-Back $g 91 540 1020
$s = Fit-Lines @("BET YOU CAN'T", "BEAT THEM ALL.") $black 0 120 940
Draw-Line $g "BET YOU CAN'T" $black 0 $s 540 400 $whiteC (C 255 90 8 6)
Draw-Line $g "BEAT THEM ALL." $black 0 $s 540 (400 + $s * 1.18) $goldC (C 255 90 8 6)
$icon = [System.Drawing.Image]::FromFile("D:\Unity\AeroBlitz\StoreAssets\icono_512.png")
$ip = Round-Path 360 680 360 360 70
$sh = Round-Path 348 668 384 384 80; $b2 = New-Object System.Drawing.SolidBrush((C 255 255 226 150)); $g.FillPath($b2, $sh); $b2.Dispose(); $sh.Dispose()
$g.SetClip($ip); $g.DrawImage($icon, 360, 680, 360, 360); $g.ResetClip(); $ip.Dispose(); $icon.Dispose()
Draw-Line $g "AERO BLITZ" $black 0 104 540 1150 $whiteC (C 255 90 8 6)
Draw-Line $g "SPACE SHOOTER" $bold 1 46 540 1245 $goldC (C 255 90 8 6)
$pw = 760; $ph = 120; $pill = Round-Path (540 - $pw / 2) 1330 $pw $ph 60
$b2 = New-Object System.Drawing.SolidBrush((C 255 255 205 40)); $g.FillPath($b2, $pill); $b2.Dispose(); $pill.Dispose()
$ps = Fit-Lines @("FREE ON GOOGLE PLAY") $black 0 54 ($pw - 90)
Draw-Line $g "FREE ON GOOGLE PLAY" $black 0 $ps 540 1390 (C 255 40 6 14) $null
$bmp.Save("$f\fin_promo.png", [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $bmp.Dispose()
"  fin_promo.png"

$c = "D:\Unity\AeroBlitz\TrailerCaptures"; $neg = "$c\reddit\negro_vertical.png"; $FULL = "|0|0|1080|1920"; $TOP = "|0|230|1080|400"   # ojo: PowerShell no distingue mayúsculas ($F sería $f)
$guion = @(
    "# Anuncio de TikTok: 16 s, partida a pantalla completa con frases encima",
    "size|1080|1920",
    "clip|$c\clip_182212_vertical.mp4|1.0|4.0|$neg$FULL",
    "clip|$c\clip_182638_vertical.mp4|2.6|4.8|$neg$FULL",
    "clip|$c\clip_181715_vertical.mp4|9.3|12.5|$neg$FULL",
    "clip|$c\clip_182625_vertical.mp4|0.0|3.4|$neg$FULL",
    "image|$f\fin_promo.png|4.2",
    "text|$f\t1.png|0.0|3.0$TOP",
    "text|$f\t2.png|3.0|5.2$TOP",
    "text|$f\t3.png|5.2|8.4$TOP",
    "text|$f\t4.png|8.4|11.8$TOP",
    "music|$tk\fuentes\audio_promo.wav")
[IO.File]::WriteAllLines("$f\guion_promo.txt", $guion, $u8)
& "$here\framegrab\Assemble.exe" "$f\guion_promo.txt" "$tk\promo_aero_blitz.mp4" | Select-Object -Last 1
