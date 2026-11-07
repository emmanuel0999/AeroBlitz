# Rótulos y pantallas finales de la serie de TikTok (1080x1920)
$here = Split-Path $MyInvocation.MyCommand.Path
. "$here\rotulos.ps1" | Out-Null
$icon = [System.Drawing.Image]::FromFile("D:\Unity\AeroBlitz\StoreAssets\icono_512.png")
$tdir = "D:\Unity\AeroBlitz\TikTok\fuentes\rotulos"
New-Item -ItemType Directory -Force $tdir | Out-Null
$GX = 112; $GY = 370; $GW = 856; $GH = 1522

function Make-Rotulo-TT($file, $lines, $accentIdx, $fixedSize = 0) {
    $c = New-Canvas 1080 1920; $bmp = $c[0]; $g = $c[1]
    Draw-Space $g 1080 1920 17
    for ($k = 12; $k -ge 1; $k--) {
        $pen = New-Object System.Drawing.Pen((C ([int](90 - $k * 6)) 60 210 255), [single]3)
        $rp = Round-Rect ($GX - $k * 3) ($GY - $k * 3) ($GW + $k * 6) ($GH + $k * 6) (26 + $k * 3)
        $g.DrawPath($pen, $rp); $rp.Dispose(); $pen.Dispose()
    }
    $dark = New-Object System.Drawing.SolidBrush((C 255 4 5 14)); $g.FillRectangle($dark, $GX, $GY, $GW, $GH); $dark.Dispose()
    $size = if ($fixedSize -gt 0) { $fixedSize } else { Fit $lines $famBlack 104 980 }
    $lh = $size * 1.12
    $cy = 262 - ($lines.Count - 1) * $lh / 2
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $accent = $accentIdx -contains $i
        $fill = if ($accent) { $gold } else { $white }
        $glow = if ($accent) { C 255 255 120 30 } else { $cyan }
        Draw-Text $g $lines[$i] $famBlack $size 540 ($cy + $i * $lh) $fill $glow
    }
    $bmp.Save("$tdir\$file", [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
    "  $file  (letra $([int]$size))"
}

# Pantalla final: frase del video, icono, nombre y cómo encontrarlo (en TikTok no hay enlaces en el texto)
function Make-Fin-TT($file, $l1, $l2, $seed) {
    $c = New-Canvas 1080 1920; $bmp = $c[0]; $g = $c[1]
    Draw-Space $g 1080 1920 $seed
    $size = Fit @($l1, $l2) $famBlack 120 940
    Draw-Text $g $l1 $famBlack $size 540 470 $white $cyan
    Draw-Text $g $l2 $famBlack $size 540 (470 + $size * 1.2) $gold (C 255 255 120 30)
    Draw-Icon $g 540 930 300
    Draw-Text $g "AERO BLITZ" $famBlack 92 540 1200 $white $cyan
    Draw-Text $g "SPACE SHOOTER" $famBold 40 540 1275 $cyan $null
    Draw-Pill $g "SEARCH `"AERO BLITZ`" ON GOOGLE PLAY" 540 1410 34
    $bmp.Save("$tdir\$file", [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
    "  $file"
}

Make-Rotulo-TT "r1_level70.png"     @("LEVEL 70", "IS NOT A JOKE") @(1)
Make-Rotulo-TT "r2_misiles.png"     @("MISSILES", "VS WALLS") @(0)
Make-Rotulo-TT "r3_pov.png"         @("POV: YOU THOUGHT", "LEVEL 38 WAS EASY") @(1)
Make-Rotulo-TT "r4_ice.png"         @("WHICH IS HARDEST?", "ICE RINGS") @(1) 86
Make-Rotulo-TT "r4_nebula.png"      @("WHICH IS HARDEST?", "CRIMSON NEBULA") @(1) 86
Make-Rotulo-TT "r4_station.png"     @("WHICH IS HARDEST?", "ABANDONED STATION") @(1) 86
Make-Rotulo-TT "r4_core.png"        @("WHICH IS HARDEST?", "GALACTIC CORE") @(1) 86
Make-Rotulo-TT "r5_howfar.png"      @("HOW FAR", "CAN YOU GET?") @(1)

Make-Fin-TT "f1.png" "STILL THINK YOU" "CAN MAKE IT?" 71
Make-Fin-TT "f2.png" "NOTHING CAN" "STOP THEM." 72
Make-Fin-TT "f3.png" "BET YOU CAN'T" "BEAT IT." 73
Make-Fin-TT "f4.png" "WHICH ONE?" "COMMENT BELOW" 74
Make-Fin-TT "f5.png" "COMMENT YOUR" "BEST LEVEL" 75
$icon.Dispose()
