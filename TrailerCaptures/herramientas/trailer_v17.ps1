# Tráiler 1.0.3 (1920x1080, 30 fps, ~27 s): acción desde el primer segundo, cortes al ritmo de la música (trailer_v17_musica.py).
# Uso: trailer_v17.ps1 -Lang en|es [-Preview]   (Preview: solo hojas de revisión, sin video)
param([string]$Lang = "en", [switch]$Preview)
$myLang = $Lang
. "$PSScriptRoot/capturas_v17_base.ps1" -Only "__ninguno__"
$Lang = $myLang
$ErrorActionPreference = 'Stop'

$Takes = "D:\Temp\trailercap\takes"
$Shots = "D:\Temp\storecap\shots"
$Ships = "D:\Temp\storecap\ships"
$Root = "D:\Unity\AeroBlitz"
$FramesDir = "D:\Temp\trailercap\frames_$Lang"
$Music = "$Root\TrailerCaptures\musica_trailer_v17.wav"
$OutVideo = "$Root\TrailerCaptures\trailer_v17_$Lang.mp4"
$VW = 1920; $VH = 1080; $Fps = 30
$Beat = 60.0 / 140
$CutBeats = @(0, 8, 14, 20, 24, 31, 38, 44, 48, 53, 57, 64)
$Cuts = $CutBeats | ForEach-Object { [int][Math]::Round($_ * $Beat * $Fps) }

$T = @{
    en = @{ hook = @("CAN YOU SURVIVE", "70 EXTREME LEVELS?"); core = @("CHARGE. FIRE.", "SURVIVE."); missiles = @("MISSILES", "THAT SMASH WALLS")
            plasma = @("PLASMA CANNONS", "BLAST EVERYTHING"); ships = @("11 SHIPS", "TO COLLECT"); worlds = "10 WORLDS"
            storm = @("ASTEROID STORMS", "ONLY THE BEST SURVIVE"); missions = @("DAILY MISSIONS", "REWARDS EVERY DAY"); chest = @("EPIC CHESTS", "PACKED WITH REWARDS")
            ranking = @("CLIMB THE", "WORLD RANKING"); end1 = "FREE ON GOOGLE PLAY"; end2 = "CAN YOU BEAT ALL 70 LEVELS?"
            wnames = @("Outer Belt", "Ice Rings", "Crimson Nebula", "Solar Flare", "Plasma Sea", "Galactic Core") }
    es = @{ hook = @("¿PUEDES SOBREVIVIR", "A 70 NIVELES EXTREMOS?"); core = @("CARGA. DISPARA.", "SOBREVIVE."); missiles = @("MISILES", "QUE ROMPEN MUROS")
            plasma = @("CAÑONES DE PLASMA", "ARRASA CON TODO"); ships = @("11 NAVES", "PARA COLECCIONAR"); worlds = "10 MUNDOS"
            storm = @("TORMENTAS DE ASTEROIDES", "SOLO LOS MEJORES SOBREVIVEN"); missions = @("MISIONES DIARIAS", "PREMIOS CADA DÍA"); chest = @("COFRES ÉPICOS", "LLENOS DE PREMIOS")
            ranking = @("SUBE EN EL", "RANKING MUNDIAL"); end1 = "GRATIS EN GOOGLE PLAY"; end2 = "¿PUEDES SUPERAR LOS 70 NIVELES?"
            wnames = @("Cinturón Exterior", "Anillos de Hielo", "Nebulosa Carmesí", "Llamarada Solar", "Mar de Plasma", "Núcleo Galáctico") }
}
$tx = $T[$Lang]

# Qué parte de cada toma se usa (primer fotograma); se ajusta tras revisar las hojas de fotogramas
$Offset = @{ hook = 40; core = 20; missiles = 40; plasma = 30; storm = 40; chest = 0; w01 = 25; w02 = 25; w03 = 25; w05 = 25; w08 = 25; w10 = 25 }
if (Test-Path "$PSScriptRoot\trailer_v17_eleccion.ps1") { . "$PSScriptRoot\trailer_v17_eleccion.ps1" }

$jpg = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
$jpgP = New-Object System.Drawing.Imaging.EncoderParameters(1)
$jpgP.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter([System.Drawing.Imaging.Encoder]::Quality, [long]93)

# ---------- Fondo espacial (más ancho que el video para moverlo poco a poco) ----------
function New-Background {
    $bmp = New-Object System.Drawing.Bitmap(2240, $VH, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp); $g.SmoothingMode = 'AntiAlias'
    $rect = New-Object System.Drawing.Rectangle(0, 0, 2240, $VH)
    $lg = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, (C 255 4 8 26), (C 255 12 22 60), [single]90)
    $g.FillRectangle($lg, $rect); $lg.Dispose()
    Glow $g 420 300 520 (C 70 40 90 255); Glow $g 1500 760 600 (C 60 140 50 220); Glow $g 1100 80 420 (C 45 255 140 40); Glow $g 2000 300 420 (C 55 60 130 255)
    $rnd = New-Object System.Random(3)
    for ($i = 0; $i -lt 420; $i++) {
        $s = [single](1 + $rnd.NextDouble() * 2.8); $a = 60 + $rnd.Next(190)
        $b = New-Object System.Drawing.SolidBrush((C $a 220 235 255)); $g.FillEllipse($b, [single]$rnd.Next(2240), [single]$rnd.Next($VH), $s, $s); $b.Dispose()
    }
    $g.Dispose(); return $bmp
}

# ---------- Rótulo (dos líneas) en una imagen transparente ----------
function New-Caption($l1, $l2, $w = 900) {
    $bmp = New-Object System.Drawing.Bitmap($w, 330, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp); $g.SmoothingMode = 'AntiAlias'; $g.CompositingQuality = 'HighQuality'
    Draw-Text $g $l1 $famHead 3 122 ($w - 40) ($w / 2) 110 $gold1 $gold2 14 $edgeDark | Out-Null
    if ($l2) { Draw-Text $g $l2 $famHead 2 70 ($w - 40) ($w / 2) 248 $white $null 10 $navyEdge | Out-Null }
    $g.Dispose(); return $bmp
}

function Draw-Faded($g, $img, $x, $y, $w, $h, $alpha) {
    if ($alpha -ge 0.999) { $g.DrawImage($img, [single]$x, [single]$y, [single]$w, [single]$h); return }
    $cm = New-Object System.Drawing.Imaging.ColorMatrix; $cm.Matrix33 = [single]$alpha
    $ia = New-Object System.Drawing.Imaging.ImageAttributes; $ia.SetColorMatrix($cm)
    $g.DrawImage($img, (New-Object System.Drawing.Rectangle([int]$x, [int]$y, [int]$w, [int]$h)), 0, 0, $img.Width, $img.Height, [System.Drawing.GraphicsUnit]::Pixel, $ia)
    $ia.Dispose()
}

# Rótulo con entrada: crece desde un 25 % más grande y aparece en 7 fotogramas
function Draw-Caption($g, $cap, $cx, $cy, $local) {
    $p = [Math]::Min(1.0, $local / 7.0)
    $s = 1 + 0.25 * [Math]::Pow(1 - $p, 2)
    $w = $cap.Width * $s; $h = $cap.Height * $s
    Draw-Faded $g $cap ($cx - $w / 2) ($cy - $h / 2) $w $h $p
}

# Juego dentro de un marco con brillo; zoom > 1 acerca la imagen
function Draw-Game($g, $file, $x, $y, $h, $zoom = 1.0, $dx = 0, $dy = 0, $srcRect = $null) {
    $img = [System.Drawing.Image]::FromFile($file)
    $sr = if ($srcRect) { $srcRect } else { New-Object System.Drawing.RectangleF(0, 0, $img.Width, $img.Height) }
    $w = $h * $sr.Width / $sr.Height
    $x += $dx; $y += $dy
    Glow $g ($x + $w / 2) ($y + $h / 2) ($h * .6) (C 80 30 110 255)
    foreach ($pw in 18, 10) {
        $o = Round-Path ($x - 8) ($y - 8) ($w + 16) ($h + 16) 40
        $pen = New-Object System.Drawing.Pen((C ([int](260 / $pw * 4)) 70 170 255), [single]$pw); $g.DrawPath($pen, $o); $pen.Dispose(); $o.Dispose()
    }
    $clip = Round-Path $x $y $w $h 34
    $g.SetClip($clip)
    $zw = $sr.Width / $zoom; $zh = $sr.Height / $zoom
    $src = New-Object System.Drawing.RectangleF(($sr.X + ($sr.Width - $zw) / 2), ($sr.Y + ($sr.Height - $zh) / 2), $zw, $zh)
    $dst = New-Object System.Drawing.RectangleF([single]$x, [single]$y, [single]$w, [single]$h)
    $g.DrawImage($img, $dst, $src, [System.Drawing.GraphicsUnit]::Pixel)
    $g.ResetClip(); $img.Dispose()
    $pen = New-Object System.Drawing.Pen((C 255 140 210 255), [single]4); $g.DrawPath($pen, $clip); $pen.Dispose(); $clip.Dispose()
    return $w
}

function TakeFrame($take, $i) {
    $dir = Join-Path $Takes "$($Lang)_$take"
    $n = (Get-ChildItem $dir -Filter '*.jpg').Count
    $k = [Math]::Min($n - 1, [Math]::Max(0, $i))
    return (Join-Path $dir ("f_{0:0000}.jpg" -f $k))
}

# ---------- Preparación ----------
Write-Host "Preparando ($Lang)..."
if ($Preview) { $FramesDir = "D:\Temp\trailercap\preview_$Lang" }
if (Test-Path $FramesDir) { Remove-Item -Recurse -Force $FramesDir }
New-Item -ItemType Directory -Force $FramesDir | Out-Null
$bg = New-Background
$caps = @{}
foreach ($k in 'hook', 'core', 'missiles', 'plasma', 'storm', 'missions', 'chest', 'ranking') { $caps[$k] = New-Caption $tx[$k][0] $tx[$k][1] }
$caps['ships'] = New-Caption $tx.ships[0] $tx.ships[1] 1100
$wcaps = @(); foreach ($wn in $tx.wnames) { $wcaps += New-Caption $tx.worlds $wn }
$shipImgs = @{}
foreach ($id in 'phoenix', 'titan', 'nova') { $shipImgs[$id] = [System.Drawing.Image]::FromFile("$Ships\$($id)_metal.png") }
$coinShips = @(@("falcon", "cartoon"), @("wasp", "cartoon"), @("hummingbird", "cartoon"), @("guardian", "cartoon"), @("bastion", "neon"), @("viper", "neon"), @("comet", "neon"), @("specter", "neon"))
foreach ($s in $coinShips) { $shipImgs[$s[0]] = [System.Drawing.Image]::FromFile("$Ships\$($s[0])_$($s[1]).png") }
$icon = [System.Drawing.Image]::FromFile("$Root\StoreAssets\iconos_v15\icono_final_1024.png")
$logo = [System.Drawing.Image]::FromFile("$Root\Assets\AeroBlitz\Resources\UI\logo.png")
$podium = [System.Drawing.Image]::FromFile("$Root\StoreAssets\capturas_v15\$Lang\07_ranking.png")
$endCap1 = New-Object System.Drawing.Bitmap(1000, 140, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$eg = [System.Drawing.Graphics]::FromImage($endCap1); $eg.SmoothingMode = 'AntiAlias'
Draw-Text $eg $tx.end1 $famHead 3 96 960 500 70 $gold1 $gold2 12 $edgeDark | Out-Null; $eg.Dispose()
$endCap2 = New-Object System.Drawing.Bitmap(1000, 100, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$eg = [System.Drawing.Graphics]::FromImage($endCap2); $eg.SmoothingMode = 'AntiAlias'
Draw-Text $eg $tx.end2 $famHead 2 60 960 500 50 $white $null 8 $navyEdge | Out-Null; $eg.Dispose()

$PanelH = 1000; $PanelY = 40
$LeftX = 170; $RightX = 1000
$TextRightCX = 1420; $TextLeftCX = 500
# Juego recortado (sin la franja de arriba ni los botones de abajo) para que se vea más grande
$GameCrop = New-Object System.Drawing.RectangleF(0, 230, 1080, 1440)
$scenes = @('hook', 'core', 'missiles', 'plasma', 'ships', 'worlds', 'storm', 'missions', 'chest', 'ranking', 'end')
$total = $Cuts[-1]
$sw = [Diagnostics.Stopwatch]::StartNew()
$step = if ($Preview) { 6 } else { 1 }

for ($f = 0; $f -lt $total; $f += $step) {
    $si = 0; while ($si -lt 10 -and $f -ge $Cuts[$si + 1]) { $si++ }
    $scene = $scenes[$si]; $local = $f - $Cuts[$si]; $len = $Cuts[$si + 1] - $Cuts[$si]
    $bmp = New-Object System.Drawing.Bitmap($VW, $VH, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'; $g.PixelOffsetMode = 'HighQuality'; $g.CompositingQuality = 'HighQuality'
    $g.DrawImage($bg, [single](-($f * 0.35) % 320), [single]0, [single]2240, [single]$VH)
    $left = ($si % 2 -eq 0)
    $px = if ($left) { $LeftX } else { $RightX }; $tcx = if ($left) { $TextRightCX } else { $TextLeftCX }
    switch ($scene) {
        { $_ -in 'hook', 'core', 'missiles', 'plasma', 'storm', 'chest' } {
            $shake = 0; if ($scene -in 'hook', 'storm' -and $local -lt 8) { $shake = (8 - $local) * 1.5 }
            $r = New-Object System.Random($f)
            $zoom = 1.0 + 0.04 * $local / $len
            $crop = if ($scene -eq 'chest') { $null } else { $GameCrop }
            $gx = if ($scene -eq 'chest') { $px + 90 } else { $px }
            $lc = if ($scene -eq 'hook') { $local + 4 } else { $local }
            Draw-Game $g (TakeFrame $scene ($Offset[$scene] + $local)) $gx $PanelY $PanelH $zoom (($r.NextDouble() - .5) * $shake) (($r.NextDouble() - .5) * $shake) $crop | Out-Null
            Draw-Caption $g $caps[$scene] $tcx 540 $lc
        }
        'worlds' {
            $wi = [Math]::Min(5, [int][Math]::Floor($local / ($len / 6.0)))
            $wl = $local - [int]($wi * $len / 6.0)
            $take = @('w01', 'w02', 'w03', 'w05', 'w08', 'w10')[$wi]
            Draw-Game $g (TakeFrame $take ($Offset[$take] + $wl)) $px $PanelY $PanelH 1.0 0 0 $GameCrop | Out-Null
            Draw-Caption $g $wcaps[$wi] $tcx 540 $(if ($wi -eq 0) { $local } else { 7 + [Math]::Min(0, $wl - 2) })
        }
        'missions' {
            $zoom = 1.0 + 0.08 * $local / $len
            Draw-Game $g "$Shots\$($Lang)_missions.png" ($px + 90) $PanelY $PanelH $zoom | Out-Null
            Draw-Caption $g $caps[$scene] $tcx 540 $local
        }
        'ranking' {
            $src = New-Object System.Drawing.RectangleF(60, 330, 960, 1250)
            $zoom = 1.0 + 0.05 * $local / $len
            $h = 960; $w = $h * 960 / 1250
            Draw-Game $g "$Root\StoreAssets\capturas_v15\$Lang\07_ranking.png" ($px - 80) 60 $h $zoom 0 0 $src | Out-Null
            Draw-Caption $g $caps[$scene] ($tcx - 60) 540 $local
        }
        'ships' {
            Draw-Caption $g $caps['ships'] 960 175 $local
            $big = @(@("titan", 600, 520, 330, 0), @("phoenix", 960, 470, 420, 4), @("nova", 1320, 520, 330, 8))
            foreach ($s in $big) {
                $lp = ($local - $s[4]) / 8.0; if ($lp -le 0) { continue }
                $k = if ($lp -lt 1) { 1.15 * [Math]::Sin($lp * [Math]::PI / 2) } else { 1.0 + 0.15 * [Math]::Max(0, 1 - ($lp - 1) * 2) }
                $sz = $s[3] * $k
                Glow $g $s[1] $s[2] ($sz * .65) (C 140 255 170 70)
                $g.DrawImage($shipImgs[$s[0]], [single]($s[1] - $sz / 2), [single]($s[2] - $sz / 2), [single]$sz, [single]$sz)
            }
            for ($i = 0; $i -lt 8; $i++) {
                $lp = ($local - 16 - $i * 5) / 7.0; if ($lp -le 0) { continue }
                $k = if ($lp -lt 1) { 1.15 * [Math]::Sin($lp * [Math]::PI / 2) } else { 1.0 + 0.15 * [Math]::Max(0, 1 - ($lp - 1) * 2) }
                $cx = 260 + $i * 200; $sz = 200 * $k
                Glow $g $cx 860 ($sz * .6) (C 90 80 160 255)
                $g.DrawImage($shipImgs[$coinShips[$i][0]], [single]($cx - $sz / 2), [single](860 - $sz / 2), [single]$sz, [single]$sz)
            }
        }
        'end' {
            $p = [Math]::Min(1.0, $local / 9.0)
            $isz = 470 * (0.7 + 0.3 * [Math]::Sin($p * [Math]::PI / 2)) * (1 + 0.02 * [Math]::Sin($local / 6.0))
            Glow $g 470 540 ($isz * .75) (C 160 255 150 50)
            $rp = Round-Path (470 - $isz / 2) (540 - $isz / 2) $isz $isz ($isz * .2)
            $g.SetClip($rp); $g.DrawImage($icon, [single](470 - $isz / 2), [single](540 - $isz / 2), [single]$isz, [single]$isz); $g.ResetClip()
            $pen = New-Object System.Drawing.Pen((C 255 255 255 255), [single]5); $g.DrawPath($pen, $rp); $pen.Dispose(); $rp.Dispose()
            $lw = 760; $lh = $lw * $logo.Height / $logo.Width
            Draw-Faded $g $logo (1290 - $lw / 2) 110 $lw $lh $p
            Draw-Caption $g $endCap1 1290 640 ($local - 6)
            if ($local -ge 12) { Draw-Caption $g $endCap2 1290 770 ($local - 12) }
        }
    }
    # Destello blanco en cada corte
    if ($local -lt 3 -and $f -gt 0) {
        $b = New-Object System.Drawing.SolidBrush((C ([int](105 * (1 - $local / 3.0))) 255 255 255)); $g.FillRectangle($b, 0, 0, $VW, $VH); $b.Dispose()
    }
    $g.Dispose()
    $bmp.Save((Join-Path $FramesDir ("f_{0:0000}.jpg" -f $f)), $jpg, $jpgP); $bmp.Dispose()
    if ($f % 120 -eq 0) { Write-Host ("  {0}/{1}  {2:0}s" -f $f, $total, $sw.Elapsed.TotalSeconds) }
}
if ($Preview) { "Vista previa en $FramesDir"; return }
Write-Host "Codificando..."
& "$PSScriptRoot\framegrab\FramesToVideo.exe" $FramesDir 30 $OutVideo $Music
