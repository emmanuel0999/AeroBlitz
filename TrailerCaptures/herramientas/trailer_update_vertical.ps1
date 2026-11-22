# Tráiler VERTICAL "NEW UPDATE" de la v1.1.0 (1080x1920, 30 fps, 22,5 s) para TikTok / Reels / Shorts.
# Ritmo: 128 bpm, un corte cada 4 tiempos (1,9 s), drop en el tiempo 16. Música y efectos: trailer_update_musica.py.
# Tomas del juego SIN interfaz (D:\Temp\trailer2\takes, grabadas con la build temporal), cofre y misiones de las capturas reales.
# Uso: trailer_update_vertical.ps1 -Lang en|es [-Preview]
param([string]$Lang = "en", [switch]$Preview)
$tuLang = $Lang
. "$PSScriptRoot/capturas_v17_base.ps1" -Only "__ninguno__"
$Lang = $tuLang
$ErrorActionPreference = 'Stop'
$Takes = "D:\Temp\trailer2\takes"; $OldTakes = "D:\Temp\trailercap\takes"; $Ships = "D:\Temp\storecap\ships"; $Shots = "D:\Temp\storecap\shots"
$Root = "D:\Unity\AeroBlitz"
$FramesDir = if ($Preview) { "D:\Temp\trailer2\preview_$Lang" } else { "D:\Temp\trailer2\frames_$Lang" }
$Music = "$Root\TrailerCaptures\musica_trailer_update.wav"
$OutVideo = "$Root\TrailerCaptures\trailer_update_vertical_$Lang.mp4"
$VW = 1080; $VH = 1920; $Fps = 30; $Beat = 60.0 / 128
$SceneBeats = @(0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 48)
$Cuts = $SceneBeats | ForEach-Object { [int][Math]::Round($_ * $Beat * $Fps) }
$Scenes = @('hook', 'boss', 'fury', 'ships', 'worlds', 'missions', 'chest', 'power', 'death', 'ranking', 'end')
$Offset = @{ hook = 150; boss = 20; fury = 84; power = 0; death = 30; chest = 12; hook2 = 147 }
# Acercamiento y centro de cada toma (las tomas son la pantalla entera: así la acción se ve más grande)
$Frame = @{ hook = @(1.15, 0.6, 0.42); boss = @(1.32, 0.3); fury = @(1.12, 0.42); power = @(1.15, 0.6, 0.42); death = @(1.5, 0.3) }
if (Test-Path "$PSScriptRoot\trailer_update_eleccion.ps1") { . "$PSScriptRoot\trailer_update_eleccion.ps1" }

$T = @{
    en = @{ hook = @("NEW UPDATE", "7 BOSSES HAVE ARRIVED"); boss = @("7 EPIC BOSSES", ""); fury = @("FURY MODE", "3 PHASES PER BOSS"); ships = @("11 SHIPS", "")
            worlds = @("10 WORLDS", ""); missions = @("DAILY MISSIONS", "CLAIM YOUR REWARDS"); chest = @("EPIC CHESTS", ""); power = @("POWER UP", "PLASMA & MISSILES")
            death = @("DEFEAT THE BOSSES", ""); ranking = @("WORLD RANKING", "CLIMB TO THE TOP"); end1 = "FREE ON GOOGLE PLAY"; end2 = "NEW UPDATE OUT NOW"
            end3 = @("CAN YOU SURVIVE", "ALL 7 BOSSES?"); claim = "+200 COINS"; world = "WORLD"
            shipnames = @("Falcon", "Wasp", "Hummingbird", "Guardian", "Bastion", "Viper", "Comet", "Specter", "Titan", "Nova", "Phoenix") }
    es = @{ hook = @("NUEVA ACTUALIZACIÓN", "¡LLEGARON 7 JEFES!"); boss = @("7 JEFES ÉPICOS", ""); fury = @("MODO FURIA", "3 FASES POR JEFE"); ships = @("11 NAVES", "")
            worlds = @("10 MUNDOS", ""); missions = @("MISIONES DIARIAS", "RECLAMA TUS PREMIOS"); chest = @("COFRES ÉPICOS", ""); power = @("MÁS PODER", "PLASMA Y MISILES")
            death = @("DERROTA A LOS JEFES", ""); ranking = @("RANKING MUNDIAL", "SUBE A LA CIMA"); end1 = "GRATIS EN GOOGLE PLAY"; end2 = "¡YA DISPONIBLE!"
            end3 = @("¿PUEDES SOBREVIVIR", "A LOS 7 JEFES?"); claim = "+200 MONEDAS"; world = "MUNDO"
            shipnames = @("Halcón", "Avispa", "Colibrí", "Guardián", "Bastión", "Víbora", "Cometa", "Espectro", "Titán", "Nova", "Fénix") }
}
$tx = $T[$Lang]
$ShipFiles = @("falcon_cartoon", "wasp_cartoon", "hummingbird_cartoon", "guardian_cartoon", "bastion_cartoon", "viper_neon", "comet_neon", "specter_neon", "titan_metal", "nova_metal", "phoenix_metal")
$ShipGlow = @((C 150 80 200 255), (C 150 255 220 60), (C 150 60 255 200), (C 150 120 255 140), (C 150 255 90 80), (C 150 190 255 60), (C 150 90 160 255), (C 150 160 120 255), (C 150 190 140 255), (C 150 255 90 220), (C 160 255 150 50))

$jpg = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
$jpgP = New-Object System.Drawing.Imaging.EncoderParameters(1)
$jpgP.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter([System.Drawing.Imaging.Encoder]::Quality, [long]93)

# ---------- piezas ----------
function New-Caption($l1, $l2, $red = $false) {
    $bmp = New-Object System.Drawing.Bitmap(1060, 330, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp); $g.SmoothingMode = 'AntiAlias'; $g.CompositingQuality = 'HighQuality'
    $c1 = if ($red) { C 255 255 120 110 } else { $gold1 }; $c2 = if ($red) { C 255 230 20 40 } else { $gold2 }
    Draw-Text $g $l1 $famHead 3 150 1000 530 112 $c1 $c2 16 $edgeDark | Out-Null
    if ($l2) { Draw-Text $g $l2 $famHead 2 68 980 530 252 $white $null 11 $navyEdge | Out-Null }
    $g.Dispose(); return $bmp
}
function Draw-Faded($g, $img, $x, $y, $w, $h, $alpha) {
    if ($alpha -ge 0.999) { $g.DrawImage($img, [single]$x, [single]$y, [single]$w, [single]$h); return }
    if ($alpha -le 0.001) { return }
    $cm = New-Object System.Drawing.Imaging.ColorMatrix; $cm.Matrix33 = [single]$alpha
    $ia = New-Object System.Drawing.Imaging.ImageAttributes; $ia.SetColorMatrix($cm)
    $g.DrawImage($img, (New-Object System.Drawing.Rectangle([int]$x, [int]$y, [int]$w, [int]$h)), 0, 0, $img.Width, $img.Height, [System.Drawing.GraphicsUnit]::Pixel, $ia); $ia.Dispose()
}
# Rótulo que entra de golpe (de un 35 % más grande a su tamaño en 5 fotogramas)
function Draw-Slam($g, $img, $cx, $cy, $local, $big = 0.35) {
    if ($local -lt 0) { return }
    $p = [Math]::Min(1.0, $local / 5.0); $s = 1 + $big * [Math]::Pow(1 - $p, 2)
    $w = $img.Width * $s; $h = $img.Height * $s
    Draw-Faded $g $img ($cx - $w / 2) ($cy - $h / 2) $w $h ([Math]::Min(1.0, $local / 3.0))
}
function Veil($g, $h = 560) {
    $r = New-Object System.Drawing.Rectangle(0, 0, $VW, $h)
    $vb = New-Object System.Drawing.Drawing2D.LinearGradientBrush($r, (C 215 2 4 18), (C 0 2 4 18), [single]90)
    $g.FillRectangle($vb, $r); $vb.Dispose()
}
# Imagen a pantalla completa con zoom (centro opcional) y temblor
function Draw-Full($g, $file, $zoom, $sx, $sy, $cx = 0.5, $cy = 0.5) {
    $img = [System.Drawing.Image]::FromFile($file)
    $zw = $img.Width / $zoom; $zh = $img.Height / $zoom
    $x0 = [Math]::Max(0, [Math]::Min($img.Width - $zw, $img.Width * $cx - $zw / 2)); $y0 = [Math]::Max(0, [Math]::Min($img.Height - $zh, $img.Height * $cy - $zh / 2))
    $src = New-Object System.Drawing.RectangleF($x0, $y0, $zw, $zh)
    $g.DrawImage($img, (New-Object System.Drawing.RectangleF([single]($sx - 12), [single]($sy - 20), [single]($VW + 24), [single]($VH + 40))), $src, [System.Drawing.GraphicsUnit]::Pixel); $img.Dispose()
}
function TakeFile($dir, $i) {
    $n = (Get-ChildItem $dir -Filter 'f_*.jpg').Count
    return (Join-Path $dir ("f_{0:0000}.jpg" -f [Math]::Min($n - 1, [Math]::Max(0, $i))))
}
function New-Space {
    $bmp = New-Object System.Drawing.Bitmap($VW, $VH, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp); $g.SmoothingMode = 'AntiAlias'
    $rect = New-Object System.Drawing.Rectangle(0, 0, $VW, $VH)
    $lg = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, (C 255 4 6 26), (C 255 18 10 50), [single]90); $g.FillRectangle($lg, $rect); $lg.Dispose()
    Glow $g 260 600 700 (C 80 40 90 255); Glow $g 860 1400 760 (C 70 150 40 220)
    $rnd = New-Object System.Random(9)
    for ($i = 0; $i -lt 380; $i++) { $s = [single](1 + $rnd.NextDouble() * 3); $b = New-Object System.Drawing.SolidBrush((C (60 + $rnd.Next(190)) 225 238 255)); $g.FillEllipse($b, [single]$rnd.Next($VW), [single]$rnd.Next($VH), $s, $s); $b.Dispose() }
    $g.Dispose(); return $bmp
}
# Líneas de velocidad (fondo de las naves)
function Speed-Lines($g, $f) {
    $rnd = New-Object System.Random(31)
    for ($i = 0; $i -lt 40; $i++) {
        $x = $rnd.Next($VW); $len = 120 + $rnd.Next(380); $sp = 60 + $rnd.Next(80)
        $y = (($rnd.Next($VH + 600) + $f * $sp) % ($VH + 600)) - 300
        $pen = New-Object System.Drawing.Pen((C (40 + $rnd.Next(90)) 170 210 255), [single](2 + $rnd.Next(3)))
        $g.DrawLine($pen, [single]$x, [single]$y, [single]$x, [single]($y + $len)); $pen.Dispose()
    }
}
function Draw-Coin($g, $x, $y, $r) {
    $b = New-Object System.Drawing.SolidBrush((C 255 200 120 10)); $g.FillEllipse($b, [single]($x - $r), [single]($y - $r), [single](2 * $r), [single](2 * $r)); $b.Dispose()
    $b = New-Object System.Drawing.SolidBrush((C 255 255 210 60)); $g.FillEllipse($b, [single]($x - $r * .8), [single]($y - $r * .8), [single](1.6 * $r), [single](1.6 * $r)); $b.Dispose()
    $b = New-Object System.Drawing.SolidBrush((C 220 255 250 200)); $g.FillEllipse($b, [single]($x - $r * .45), [single]($y - $r * .55), [single]($r * .5), [single]($r * .5)); $b.Dispose()
}
function Draw-Check($g, $cx, $cy, $r, $alpha) {
    Glow $g $cx $cy ($r * 1.8) (C ([int](120 * $alpha)) 60 255 120)
    $b = New-Object System.Drawing.SolidBrush((C ([int](255 * $alpha)) 40 200 90)); $g.FillEllipse($b, [single]($cx - $r), [single]($cy - $r), [single](2 * $r), [single](2 * $r)); $b.Dispose()
    $pen = New-Object System.Drawing.Pen((C ([int](255 * $alpha)) 255 255 255), [single]($r * .22)); $pen.StartCap = 'Round'; $pen.EndCap = 'Round'; $pen.LineJoin = 'Round'
    $g.DrawLines($pen, [System.Drawing.PointF[]]@((P ($cx - $r * .45) ($cy + $r * .02)), (P ($cx - $r * .1) ($cy + $r * .38)), (P ($cx + $r * .5) ($cy - $r * .35)))); $pen.Dispose()
}

# Golpes (en segundos) que hacen temblar la imagen: los mismos que las explosiones de la música
$B = $Beat
$Hits = @(@(0.0, 22), @(0.35, 16), @(1.05, 16), @(0.94, 18), @(1.65, 16), @((4 * $B + 0.05), 8), @((8 * $B + 0.05), 14), @((16 * $B), 24), @((24 * $B + 0.55), 10), @((28 * $B + 0.3), 14), @((28 * $B + 1.0), 14),
          @((32 * $B + 0.05), 12), @((32 * $B + 0.3), 12), @((32 * $B + 0.55), 12), @((32 * $B + 0.8), 12), @((32 * $B + 1.05), 14), @((32 * $B + 1.35), 30), @((40 * $B), 22), @((46 * $B), 12))
function ShakeAt($f) {
    $t = $f / [double]$Fps; $a = 0.0
    foreach ($h in $Hits) { if ($t -ge $h[0]) { $a += $h[1] * [Math]::Exp(-($t - $h[0]) / 0.12) } }
    return $a
}

# ---------- preparación ----------
Write-Host "Preparando ($Lang)..."
if (Test-Path $FramesDir) { Remove-Item -Recurse -Force $FramesDir }
New-Item -ItemType Directory -Force $FramesDir | Out-Null
$space = New-Space
$caps = @{}
foreach ($k in 'boss', 'ships', 'worlds', 'missions', 'chest', 'power', 'death', 'ranking') { $caps[$k] = New-Caption $tx[$k][0] $tx[$k][1] }
$caps['fury'] = New-Caption $tx.fury[0] $tx.fury[1] $true
$caps['hook'] = New-Caption $tx.hook[0] $tx.hook[1] $true
$shipImgs = @(); foreach ($s in $ShipFiles) { $shipImgs += [System.Drawing.Image]::FromFile("$Ships\$s.png") }
$logo = [System.Drawing.Image]::FromFile("$Root\Assets\AeroBlitz\Resources\UI\logo.png")
$icon = [System.Drawing.Image]::FromFile("$Root\StoreAssets\iconos_v15\icono_final_1024.png")
$bossHull = "D:\Temp\bosscap\bossart\boss_hull_emperor.png"
$podium = [System.Drawing.Image]::FromFile("$Root\StoreAssets\capturas_v17\$Lang\07_ranking.png")
$missionsShot = "$Shots\$($Lang)_missions.png"
$chestDir = "$OldTakes\$($Lang)_chest"
function Txt($text, $fam, $style, $size, $w, $c1, $c2, $edgeW, $edge, $h = 200) {
    $bmp = New-Object System.Drawing.Bitmap($w, $h, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp); $g.SmoothingMode = 'AntiAlias'
    Draw-Text $g $text $fam $style $size ($w - 40) ($w / 2) ($h / 2) $c1 $c2 $edgeW $edge | Out-Null; $g.Dispose(); return $bmp
}
$endFree = Txt $tx.end1 $famHead 3 92 1000 $gold1 $gold2 12 $edgeDark
$endNew = Txt $tx.end2 $famHead 3 70 900 $white $null 0 $null 160
$endQ1 = Txt $tx.end3[0] $famHead 2 84 1000 $white $null 11 $navyEdge
$endQ2 = Txt $tx.end3[1] $famHead 3 128 1020 $gold1 $gold2 15 $edgeDark 230
$claimTxt = Txt $tx.claim $famHead 3 90 800 $gold1 $gold2 12 $edgeDark
$shipNames = @(); foreach ($n in $tx.shipnames) { $shipNames += (Txt $n.ToUpper() $famHead 2 74 900 $white $null 10 $navyEdge 160) }
$worldTxt = @(); for ($i = 1; $i -le 10; $i++) { $worldTxt += (Txt ("{0} {1}" -f $tx.world, $i) $famHead 3 96 900 $gold1 $gold2 12 $edgeDark 200) }

# ---------- fotogramas ----------
$total = $Cuts[-1]
$step = if ($Preview) { 5 } else { 1 }
$sw = [Diagnostics.Stopwatch]::StartNew()
for ($f = 0; $f -lt $total; $f += $step) {
    $si = 0; while ($si -lt 10 -and $f -ge $Cuts[$si + 1]) { $si++ }
    $scene = $Scenes[$si]; $local = $f - $Cuts[$si]; $len = $Cuts[$si + 1] - $Cuts[$si]
    $bmp = New-Object System.Drawing.Bitmap($VW, $VH, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBilinear'; $g.PixelOffsetMode = 'HighQuality'; $g.CompositingQuality = 'HighQuality'
    $amp = ShakeAt $f; $r = New-Object System.Random($f * 7 + 1)
    $sx = ($r.NextDouble() - .5) * 2 * $amp; $sy = ($r.NextDouble() - .5) * 2 * $amp
    $push = 1.0 + 0.06 * $local / $len
    switch ($scene) {
        { $_ -in 'hook', 'boss', 'fury', 'power', 'death' } {
            $dir = Join-Path $Takes $(if ($scene -eq 'boss') { 'bossenter' } elseif ($scene -eq 'power' -or $scene -eq 'hook') { 'hookb' } else { $scene })   # poder: tramo final de la toma del gancho (no repite la volea)
            $fi = $Offset[$scene] + $local
            # gancho: dos cortes rápidos (ráfaga de plasma y volea con explosiones)
            $half = [int]($len / 2)
            if ($scene -eq 'hook' -and $local -ge $half) { $dir = Join-Path $Takes 'hookc'; $fi = $Offset.hook2 + $local - $half }
            if ($scene -eq 'fury') { $sx += ($r.NextDouble() - .5) * 10; $sy += ($r.NextDouble() - .5) * 10 }
            $fx = if ($Frame[$scene].Count -gt 2) { $Frame[$scene][2] } else { 0.5 }   # el gancho y el poder siguen a la nave, que va a la izquierda
            Draw-Full $g (TakeFile $dir $fi) ($Frame[$scene][0] * $push) $sx $sy $fx $Frame[$scene][1]
            if ($scene -eq 'hook' -and $local -ge $half -and $local -lt $half + 3) { $b = New-Object System.Drawing.SolidBrush((C ([int](120 * (1 - ($local - $half) / 3.0))) 255 230 200)); $g.FillRectangle($b, 0, 0, $VW, $VH); $b.Dispose() }
            if ($scene -eq 'fury') {   # pulso rojo de alarma en los bordes
                $a = [int](70 + 50 * [Math]::Sin($local / 3.0))
                foreach ($edge in @(@(0, 0, $VW, 220, 90), @(0, ($VH - 260), $VW, 260, 270))) {
                    $rr = New-Object System.Drawing.Rectangle($edge[0], $edge[1], $edge[2], $edge[3])
                    $vb = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rr, (C $a 255 20 40), (C 0 255 20 40), [single]$edge[4]); $g.FillRectangle($vb, $rr); $vb.Dispose()
                }
            }
            Veil $g
            $cy = if ($scene -eq 'hook') { 300 } else { 270 }
            Draw-Slam $g $caps[$scene] 540 $cy ($local - 2) $(if ($scene -eq 'hook') { 0.6 } else { 0.35 })
        }
        'ships' {
            $g.DrawImage($space, 0, 0, $VW, $VH); Speed-Lines $g $f
            $k = [Math]::Min(10, [int][Math]::Floor($local * 11 / $len)); $kl = $local - [int]($k * $len / 11)
            $punch = 1 + 0.18 * [Math]::Max(0, 1 - $kl / 3.0)
            $sz = 820 * $punch
            Glow $g 540 1000 ($sz * .62) $ShipGlow[$k]
            $g.DrawImage($shipImgs[$k], [single](540 - $sz / 2 + $sx), [single](1000 - $sz / 2 + $sy), [single]$sz, [single]$sz)
            Draw-Faded $g $shipNames[$k] 90 1470 900 160 1.0
            Veil $g 520
            Draw-Slam $g $caps['ships'] 540 270 ($local - 1)
        }
        'worlds' {
            $k = [Math]::Min(9, [int][Math]::Floor($local * 10 / $len)); $kl = $local - [int]($k * $len / 10)
            Draw-Full $g (TakeFile (Join-Path $Takes ("w{0:00}" -f ($k + 1))) (10 + $kl)) (1.04 + 0.03 * $kl) $sx $sy 0.5 0.45
            Veil $g 520
            Draw-Slam $g $caps['worlds'] 540 270 ($local - 1)
            Draw-Slam $g $worldTxt[$k] 540 1600 $kl 0.25
        }
        'missions' {
            Draw-Full $g $missionsShot (1.05 + 0.12 * $local / $len) $sx $sy 0.5 0.33
            $b = New-Object System.Drawing.SolidBrush((C 70 0 0 10)); $g.FillRectangle($b, 0, 0, $VW, $VH); $b.Dispose()
            # la pantalla se acerca a la primera misión; el botón RECLAMAR se pulsa, sello verde y monedas volando
            $z = 1.05 + 0.12 * $local / $len
            $bx = 540 + (878 - 540) * $z; $by = 640 + (540 - 640) * $z
            if ($local -ge 6 -and $local -lt 12) { Glow $g $bx $by 200 (C ([int](200 * (1 - ($local - 6) / 6.0))) 120 255 140) }
            if ($local -ge 9) { $p = [Math]::Min(1.0, ($local - 9) / 4.0); Draw-Check $g (540 + (120 - 540) * $z) (640 + (540 - 640) * $z) (70 * (1.3 - 0.3 * $p)) $p }
            if ($local -ge 10) {
                $rc = New-Object System.Random(5)
                for ($i = 0; $i -lt 12; $i++) {
                    $p = [Math]::Min(1.0, ($local - 10 - $i * 0.6) / 14.0); if ($p -le 0) { continue }
                    $tx0 = $bx; $ty0 = $by; $tx1 = 120 + $rc.Next(840); $ty1 = 560 + $rc.Next(200)
                    $x = $tx0 + ($tx1 - $tx0) * $p; $y = $ty0 + ($ty1 - $ty0) * $p - 260 * [Math]::Sin($p * [Math]::PI)
                    Draw-Coin $g $x $y 26
                }
                Draw-Slam $g $claimTxt 540 1180 ($local - 12) 0.4
            }
            Veil $g 520
            Draw-Slam $g $caps['missions'] 540 270 ($local - 1)
        }
        'chest' {
            Draw-Full $g (TakeFile $chestDir ($Offset.chest + $local)) (1.55 + 0.08 * $local / $len) $sx $sy 0.5 0.47
            Veil $g 520
            Draw-Slam $g $caps['chest'] 540 270 ($local - 1)
        }
        'ranking' {
            $g.DrawImage($space, 0, 0, $VW, $VH)
            $src = New-Object System.Drawing.RectangleF(40, 330, 1000, 1250)
            $p = [Math]::Min(1.0, $local / 8.0); $e = 1 - [Math]::Pow(1 - $p, 3)
            $w = 1040 * (1.0 + 0.05 * $local / $len); $h = $w * 1250 / 1000.0
            $y = 470 + 420 * (1 - $e)
            $g.DrawImage($podium, (New-Object System.Drawing.RectangleF([single](540 - $w / 2 + $sx), [single]($y + $sy), [single]$w, [single]$h)), $src, [System.Drawing.GraphicsUnit]::Pixel)
            Veil $g 520
            Draw-Slam $g $caps['ranking'] 540 270 ($local - 1)
        }
        'end' {
            $g.DrawImage($space, 0, 0, $VW, $VH)
            Glow $g 540 760 720 (C 120 200 30 60)
            # el Emperador al fondo, enorme y oscurecido
            $bh = [System.Drawing.Image]::FromFile($bossHull)
            $cm = New-Object System.Drawing.Imaging.ColorMatrix; $cm.Matrix00 = 0.45; $cm.Matrix11 = 0.25; $cm.Matrix22 = 0.25; $cm.Matrix33 = 0.55
            $ia = New-Object System.Drawing.Imaging.ImageAttributes; $ia.SetColorMatrix($cm)
            $bs = 1500 + 3 * $local
            $g.DrawImage($bh, (New-Object System.Drawing.Rectangle([int](540 - $bs / 2 + $sx), [int](700 - $bs / 2 + $sy), [int]$bs, [int]$bs)), 0, 0, $bh.Width, $bh.Height, [System.Drawing.GraphicsUnit]::Pixel, $ia)
            $bh.Dispose(); $ia.Dispose()
            # logo de golpe, luego "gratis", "ya disponible" y la pregunta con el segundo golpe
            $lp = [Math]::Min(1.0, $local / 5.0); $ls = 1 + 0.5 * [Math]::Pow(1 - $lp, 2)
            $lw = 900 * $ls; $lh = $lw * $logo.Height / $logo.Width
            Glow $g 540 400 420 (C 160 2 6 24)
            Draw-Faded $g $logo (540 - $lw / 2 + $sx) (400 - $lh / 2 + $sy) $lw $lh ([Math]::Min(1.0, $local / 3.0))
            Draw-Slam $g $endFree 540 760 ($local - 8) 0.3
            if ($local -ge 22) {
                $pp = [Math]::Min(1.0, ($local - 22) / 5.0); $pw = 760; $ph = 120
                $rp = Round-Path (540 - $pw / 2) (930 - $ph / 2) $pw $ph 60
                $b = New-Object System.Drawing.SolidBrush((C ([int](240 * $pp)) 225 30 50)); $g.FillPath($b, $rp); $b.Dispose()
                $pen = New-Object System.Drawing.Pen((C ([int](255 * $pp)) 255 200 200), [single]5); $g.DrawPath($pen, $rp); $pen.Dispose(); $rp.Dispose()
                Draw-Slam $g $endNew 540 930 ($local - 22) 0.3
            }
            $qd = [int](6 * $Fps * $Beat)  # segundo golpe de la música
            Draw-Slam $g $endQ1 540 1160 ($local - $qd + 6) 0.3
            Draw-Slam $g $endQ2 540 1310 ($local - $qd) 0.45
            $ip = [Math]::Min(1.0, [Math]::Max(0, $local - 12) / 8.0); $isz = 300
            if ($ip -gt 0) {
                Glow $g 540 1640 ($isz * .8) (C ([int](150 * $ip)) 255 150 50)
                $rp = Round-Path (540 - $isz / 2) (1640 - $isz / 2) $isz $isz ($isz * .2)
                $g.SetClip($rp); Draw-Faded $g $icon (540 - $isz / 2) (1640 - $isz / 2) $isz $isz $ip; $g.ResetClip(); $rp.Dispose()
            }
        }
    }
    # destello blanco en cada corte (y uno más fuerte en el drop)
    if ($local -lt 4 -and $f -gt 0) {
        $base = if ($si -eq 4 -or $si -eq 10) { 190 } else { 110 }
        $b = New-Object System.Drawing.SolidBrush((C ([int]($base * (1 - $local / 4.0))) 255 255 255)); $g.FillRectangle($b, 0, 0, $VW, $VH); $b.Dispose()
    }
    $g.Dispose()
    $bmp.Save((Join-Path $FramesDir ("f_{0:0000}.jpg" -f $f)), $jpg, $jpgP); $bmp.Dispose()
    if ($f % 60 -eq 0) { Write-Host ("  {0}/{1}  {2:0}s" -f $f, $total, $sw.Elapsed.TotalSeconds) }
}
if ($Preview) { "Vista previa en $FramesDir"; return }
Write-Host "Codificando..."
& "$PSScriptRoot\framegrab\FramesToVideo.exe" $FramesDir 30 $OutVideo $Music 1080 1920   # vertical
