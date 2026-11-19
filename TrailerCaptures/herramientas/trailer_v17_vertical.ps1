# Tráiler 1.0.3 VERTICAL para TikTok / Reels / Shorts (1080x1920, 30 fps, ~27 s). Mismas tomas y música que trailer_v17.ps1.
# Uso: trailer_v17_vertical.ps1 -Lang en|es
param([string]$Lang = "en")
$myLang = $Lang
. "$PSScriptRoot/capturas_v17_base.ps1" -Only "__ninguno__"
$Lang = $myLang
$ErrorActionPreference = 'Stop'
$Takes = "D:\Temp\trailercap\takes"; $Shots = "D:\Temp\storecap\shots"; $Root = "D:\Unity\AeroBlitz"
$FramesDir = "D:\Temp\trailercap\framesV_$Lang"
$Music = "$Root\TrailerCaptures\musica_trailer_v17.wav"
$OutVideo = "$Root\TrailerCaptures\trailer_v17_vertical_$Lang.mp4"
$VW = 1080; $VH = 1920; $Fps = 30; $Beat = 60.0 / 140
$CutBeats = @(0, 8, 14, 20, 24, 31, 38, 44, 48, 53, 57, 64)
$Cuts = $CutBeats | ForEach-Object { [int][Math]::Round($_ * $Beat * $Fps) }
$Offset = @{ hook = 45; core = 20; missiles = 25; plasma = 48; storm = 45; chest = 6; w01 = 25; w02 = 25; w03 = 25; w05 = 25; w08 = 45; w10 = 45 }

$T = @{
    en = @{ hook = @("CAN YOU SURVIVE", "70 EXTREME LEVELS?"); core = @("CHARGE. FIRE.", "SURVIVE."); missiles = @("MISSILES", "THAT SMASH WALLS"); plasma = @("PLASMA CANNONS", "BLAST EVERYTHING")
            storm = @("ASTEROID STORMS", "ONLY THE BEST SURVIVE"); missions = @("DAILY MISSIONS", "REWARDS EVERY DAY"); chest = @("EPIC CHESTS", "PACKED WITH REWARDS")
            worlds = "10 WORLDS"; wnames = @("Outer Belt", "Ice Rings", "Crimson Nebula", "Solar Flare", "Plasma Sea", "Galactic Core")
            end1 = "FREE ON GOOGLE PLAY"; end2 = "CAN YOU BEAT ALL 70 LEVELS?" }
    es = @{ hook = @("¿PUEDES SOBREVIVIR", "A 70 NIVELES EXTREMOS?"); core = @("CARGA. DISPARA.", "SOBREVIVE."); missiles = @("MISILES", "QUE ROMPEN MUROS"); plasma = @("CAÑONES DE PLASMA", "ARRASA CON TODO")
            storm = @("TORMENTAS DE ASTEROIDES", "SOLO LOS MEJORES SOBREVIVEN"); missions = @("MISIONES DIARIAS", "PREMIOS CADA DÍA"); chest = @("COFRES ÉPICOS", "LLENOS DE PREMIOS")
            worlds = "10 MUNDOS"; wnames = @("Cinturón Exterior", "Anillos de Hielo", "Nebulosa Carmesí", "Llamarada Solar", "Mar de Plasma", "Núcleo Galáctico")
            end1 = "GRATIS EN GOOGLE PLAY"; end2 = "¿PUEDES SUPERAR LOS 70 NIVELES?" }
}
$tx = $T[$Lang]
$jpg = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
$jpgP = New-Object System.Drawing.Imaging.EncoderParameters(1)
$jpgP.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter([System.Drawing.Imaging.Encoder]::Quality, [long]93)

function New-Caption($l1, $l2, $w = 1000) {
    $bmp = New-Object System.Drawing.Bitmap($w, 300, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp); $g.SmoothingMode = 'AntiAlias'; $g.CompositingQuality = 'HighQuality'
    Draw-Text $g $l1 $famHead 3 112 ($w - 40) ($w / 2) 100 $gold1 $gold2 14 $edgeDark | Out-Null
    if ($l2) { Draw-Text $g $l2 $famHead 2 62 ($w - 40) ($w / 2) 222 $white $null 10 $navyEdge | Out-Null }
    $g.Dispose(); return $bmp
}
function Draw-Faded($g, $img, $x, $y, $w, $h, $alpha) {
    if ($alpha -ge 0.999) { $g.DrawImage($img, [single]$x, [single]$y, [single]$w, [single]$h); return }
    $cm = New-Object System.Drawing.Imaging.ColorMatrix; $cm.Matrix33 = [single]$alpha
    $ia = New-Object System.Drawing.Imaging.ImageAttributes; $ia.SetColorMatrix($cm)
    $g.DrawImage($img, (New-Object System.Drawing.Rectangle([int]$x, [int]$y, [int]$w, [int]$h)), 0, 0, $img.Width, $img.Height, [System.Drawing.GraphicsUnit]::Pixel, $ia); $ia.Dispose()
}
function Draw-Caption($g, $cap, $cy, $local) {
    $p = [Math]::Min(1.0, $local / 7.0); $s = 1 + 0.25 * [Math]::Pow(1 - $p, 2)
    $w = $cap.Width * $s; $h = $cap.Height * $s
    Draw-Faded $g $cap (($VW - $w) / 2) ($cy - $h / 2) $w $h $p
}
# Pantalla completa con zoom suave; velo oscuro arriba para que el rótulo se lea sobre el HUD del juego
function Draw-Full($g, $file, $zoom, $shakeX, $shakeY, $veil) {
    $img = [System.Drawing.Image]::FromFile($file)
    $zw = $img.Width / $zoom; $zh = $img.Height / $zoom
    $src = New-Object System.Drawing.RectangleF((($img.Width - $zw) / 2), (($img.Height - $zh) / 2), $zw, $zh)
    $g.DrawImage($img, (New-Object System.Drawing.RectangleF([single]$shakeX, [single]$shakeY, [single]$VW, [single]$VH)), $src, [System.Drawing.GraphicsUnit]::Pixel); $img.Dispose()
    if ($veil) {
        $r = New-Object System.Drawing.Rectangle(0, 0, $VW, 520)
        $vb = New-Object System.Drawing.Drawing2D.LinearGradientBrush($r, (C 225 3 6 22), (C 0 3 6 22), [single]90)
        $g.FillRectangle($vb, $r); $vb.Dispose()
    }
}
function TakeFrame($take, $i) {
    $dir = Join-Path $Takes "$($Lang)_$take"; $n = (Get-ChildItem $dir -Filter '*.jpg').Count
    return (Join-Path $dir ("f_{0:0000}.jpg" -f ([Math]::Min($n - 1, [Math]::Max(0, $i)))))
}

Write-Host "Preparando vertical ($Lang)..."
if (Test-Path $FramesDir) { Remove-Item -Recurse -Force $FramesDir }
New-Item -ItemType Directory -Force $FramesDir | Out-Null
$caps = @{}; foreach ($k in 'hook', 'core', 'missiles', 'plasma', 'storm', 'missions', 'chest') { $caps[$k] = New-Caption $tx[$k][0] $tx[$k][1] }
$wcaps = @(); foreach ($wn in $tx.wnames) { $wcaps += New-Caption $tx.worlds $wn }
$shipsImg = [System.Drawing.Image]::FromFile("$Root\StoreAssets\capturas_v15\$Lang\02_naves.png")
$podium = "$Root\StoreAssets\capturas_v15\$Lang\07_ranking.png"
$icon = [System.Drawing.Image]::FromFile("$Root\StoreAssets\iconos_v15\icono_final_1024.png")
$logo = [System.Drawing.Image]::FromFile("$Root\Assets\AeroBlitz\Resources\UI\logo.png")
$e1 = New-Object System.Drawing.Bitmap(1000, 140, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$eg = [System.Drawing.Graphics]::FromImage($e1); $eg.SmoothingMode = 'AntiAlias'; Draw-Text $eg $tx.end1 $famHead 3 96 960 500 70 $gold1 $gold2 12 $edgeDark | Out-Null; $eg.Dispose()
$e2 = New-Object System.Drawing.Bitmap(1000, 100, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$eg = [System.Drawing.Graphics]::FromImage($e2); $eg.SmoothingMode = 'AntiAlias'; Draw-Text $eg $tx.end2 $famHead 2 56 960 500 50 $white $null 8 $navyEdge | Out-Null; $eg.Dispose()

$bg = New-Object System.Drawing.Bitmap($VW, $VH)
$bgg = [System.Drawing.Graphics]::FromImage($bg); $bgg.SmoothingMode = 'AntiAlias'
$rc = New-Object System.Drawing.Rectangle(0, 0, $VW, $VH)
$lgb = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rc, (C 255 4 8 26), (C 255 14 22 62), [single]90); $bgg.FillRectangle($lgb, $rc); $lgb.Dispose()
Glow $bgg 220 520 620 (C 80 40 90 255); Glow $bgg 880 1500 700 (C 70 140 50 220); Glow $bgg 540 140 520 (C 50 255 140 40)
$rnd = New-Object System.Random(3); for ($i = 0; $i -lt 260; $i++) { $s = [single](1 + $rnd.NextDouble() * 2.6); $b = New-Object System.Drawing.SolidBrush((C (60 + $rnd.Next(190)) 220 235 255)); $bgg.FillEllipse($b, [single]$rnd.Next($VW), [single]$rnd.Next($VH), $s, $s); $b.Dispose() }
$bgg.Dispose()

$scenes = @('hook', 'core', 'missiles', 'plasma', 'ships', 'worlds', 'storm', 'missions', 'chest', 'ranking', 'end')
$total = $Cuts[-1]; $sw = [Diagnostics.Stopwatch]::StartNew()
for ($f = 0; $f -lt $total; $f++) {
    $si = 0; while ($si -lt 10 -and $f -ge $Cuts[$si + 1]) { $si++ }
    $scene = $scenes[$si]; $local = $f - $Cuts[$si]; $len = $Cuts[$si + 1] - $Cuts[$si]
    $bmp = New-Object System.Drawing.Bitmap($VW, $VH, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'; $g.PixelOffsetMode = 'HighQuality'; $g.CompositingQuality = 'HighQuality'
    $g.DrawImage($bg, 0, 0, $VW, $VH)
    switch ($scene) {
        { $_ -in 'hook', 'core', 'missiles', 'plasma', 'storm' } {
            $shake = 0; if ($scene -in 'hook', 'storm' -and $local -lt 8) { $shake = (8 - $local) * 2 }
            $r = New-Object System.Random($f)
            Draw-Full $g (TakeFrame $scene ($Offset[$scene] + $local)) (1.0 + 0.05 * $local / $len) (($r.NextDouble() - .5) * $shake) (($r.NextDouble() - .5) * $shake) $true
            Draw-Caption $g $caps[$scene] 250 $(if ($scene -eq 'hook') { $local + 4 } else { $local })
        }
        'worlds' {
            $wi = [Math]::Min(5, [int][Math]::Floor($local / ($len / 6.0))); $wl = $local - [int]($wi * $len / 6.0)
            $take = @('w01', 'w02', 'w03', 'w05', 'w08', 'w10')[$wi]
            Draw-Full $g (TakeFrame $take ($Offset[$take] + $wl)) 1.0 0 0 $true
            Draw-Caption $g $wcaps[$wi] 250 $(if ($wi -eq 0) { $local } else { 7 })
        }
        'chest' {
            Draw-Full $g (TakeFrame 'chest' ($Offset['chest'] + $local)) 1.0 0 0 $false
            Draw-Caption $g $caps['chest'] 250 $local
        }
        'missions' {
            Draw-Full $g "$Shots\$($Lang)_missions.png" (1.0 + 0.05 * $local / $len) 0 0 $true
            Draw-Caption $g $caps['missions'] 250 $local
        }
        'ships' {
            $z = 1.0 + 0.04 * $local / $len; $w = $VW * $z; $h = $VH * $z
            Draw-Faded $g $shipsImg (-($w - $VW) / 2) (-($h - $VH) / 2) $w $h ([Math]::Min(1.0, $local / 6.0))
        }
        'ranking' {
            $img = [System.Drawing.Image]::FromFile($podium); $z = 1.0 + 0.04 * $local / $len; $w = $VW * $z; $h = $VH * $z
            $g.DrawImage($img, [single](-($w - $VW) / 2), [single](-($h - $VH) / 2), [single]$w, [single]$h); $img.Dispose()
        }
        'end' {
            $p = [Math]::Min(1.0, $local / 9.0)
            $isz = 520 * (0.7 + 0.3 * [Math]::Sin($p * [Math]::PI / 2)) * (1 + 0.02 * [Math]::Sin($local / 6.0))
            Glow $g 540 760 ($isz * .75) (C 160 255 150 50)
            $rp = Round-Path (540 - $isz / 2) (760 - $isz / 2) $isz $isz ($isz * .2)
            $g.SetClip($rp); $g.DrawImage($icon, [single](540 - $isz / 2), [single](760 - $isz / 2), [single]$isz, [single]$isz); $g.ResetClip()
            $pen = New-Object System.Drawing.Pen((C 255 255 255 255), [single]5); $g.DrawPath($pen, $rp); $pen.Dispose(); $rp.Dispose()
            $lw = 760; $lh = $lw * $logo.Height / $logo.Width
            Draw-Faded $g $logo (540 - $lw / 2) 250 $lw $lh $p
            Draw-Caption $g $e1 1160 ($local - 6)
            if ($local -ge 12) { Draw-Caption $g $e2 1300 ($local - 12) }
        }
    }
    if ($local -lt 3 -and $f -gt 0) { $b = New-Object System.Drawing.SolidBrush((C ([int](105 * (1 - $local / 3.0))) 255 255 255)); $g.FillRectangle($b, 0, 0, $VW, $VH); $b.Dispose() }
    $g.Dispose()
    $bmp.Save((Join-Path $FramesDir ("f_{0:0000}.jpg" -f $f)), $jpg, $jpgP); $bmp.Dispose()
    if ($f % 120 -eq 0) { Write-Host ("  {0}/{1}  {2:0}s" -f $f, $total, $sw.Elapsed.TotalSeconds) }
}
Write-Host "Codificando..."
& "$PSScriptRoot\framegrab\FramesToVideo.exe" $FramesDir 30 $OutVideo $Music 1080 1920
