# Rótulos 1080x1920 para TikTok / Shorts / Reels: frase arriba (debajo de las pestañas de la app) y el juego enmarcado
$here = Split-Path $MyInvocation.MyCommand.Path
. "$here\rotulos.ps1" | Out-Null
$vout = "D:\Unity\AeroBlitz\TrailerCaptures\rotulos_vertical"
New-Item -ItemType Directory -Force $vout | Out-Null
# Zona del juego (también la usa el guion vertical)
$GX = 112; $GY = 370; $GW = 856; $GH = 1522

function Make-RotuloV($file, $lines, $accentIdx) {
    $c = New-Canvas 1080 1920; $bmp = $c[0]; $g = $c[1]
    Draw-Space $g 1080 1920 17
    for ($k = 12; $k -ge 1; $k--) {
        $pen = New-Object System.Drawing.Pen((C ([int](90 - $k * 6)) 60 210 255), [single]3)
        $rp = Round-Rect ($GX - $k * 3) ($GY - $k * 3) ($GW + $k * 6) ($GH + $k * 6) (26 + $k * 3)
        $g.DrawPath($pen, $rp); $rp.Dispose(); $pen.Dispose()
    }
    $dark = New-Object System.Drawing.SolidBrush((C 255 4 5 14)); $g.FillRectangle($dark, $GX, $GY, $GW, $GH); $dark.Dispose()
    $size = Fit $lines $famBlack 104 980
    $lh = $size * 1.12
    $cy = 262 - ($lines.Count - 1) * $lh / 2
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $accent = $accentIdx -contains $i
        $fill = if ($accent) { $gold } else { $white }
        $glow = if ($accent) { C 255 255 120 30 } else { $cyan }
        Draw-Text $g $lines[$i] $famBlack $size 540 ($cy + $i * $lh) $fill $glow
    }
    $bmp.Save("$vout\$file", [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
    "  $file  (letra $([int]$size))"
}

Make-RotuloV "v0_reto.png"     @("THINK YOU CAN BEAT", "ALL 70 LEVELS?") @(1)
Make-RotuloV "v1_esquiva.png"  @("DODGE. CHARGE.", "BLAST!") @(1)
Make-RotuloV "v2_misiles.png"  @("MISSILES THAT", "SMASH ANYTHING") @(0)
Make-RotuloV "v3_mundos.png"   @("70 LEVELS", "10 WORLDS") @(1)
Make-RotuloV "v4_mejoras.png"  @("GRAB COINS", "& POWER-UPS") @(1)
Make-RotuloV "v7_dificil.png"  @("IT ONLY", "GETS HARDER") @(1)
