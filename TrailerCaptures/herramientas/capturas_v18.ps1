# Capturas de Google Play de la v17 (jefes): 8 por idioma, 1080x1920, juego real.
# Orden pensado para el ASO: 1 jefe (gancho), 2 naves, 3 Ruta Galáctica, 4 fases del jefe, 5 armas, 6 misiones, 7 ranking, 8 cofres.
# Usa las funciones de capturas_v17.ps1 (Shot-Action, Shot-Phone, Shot-ShipsBig, Shot-Podium).
# Fotos nuevas de jefes y de la Ruta: D:\Temp\bosscap\ficha (las elige capturas_v18_eleccion.ps1); el resto, las de la v15.
param([string]$Only = "")
$v18Only = $Only   # nombre propio: capturas_v17.ps1 usa $myOnly
. "$PSScriptRoot/capturas_v17.ps1" -Only "__ninguno__"
$Only = $v18Only
$out = "D:\Unity\AeroBlitz\StoreAssets\capturas_v17"
$F = "D:\Temp\bosscap\ficha"
$V15 = $Pick   # fotos de armas y cofres de la v15 (por idioma)

$Cap = @{
    en = @{ boss1 = @("7 EPIC BOSSES", "CAN YOU BEAT THEM ALL?")
            route = @("YOUR GALACTIC ROUTE", "10 WORLDS · 7 BOSSES", "FROM THE OUTER BELT TO THE GALACTIC CORE")
            boss2 = @("EVERY BOSS EVOLVES", "3 PHASES · FURY MODE"); phases = @("PHASE 1", "PHASE 2", "FURY MODE"); foot = "UNIQUE ATTACK PATTERNS IN EVERY PHASE"
            weapons = @("MISSILES & PLASMA", "THAT SMASH EVERYTHING") }
    es = @{ boss1 = @("7 JEFES ÉPICOS", "¿PODRÁS VENCERLOS?")
            route = @("TU RUTA GALÁCTICA", "10 MUNDOS · 7 JEFES", "DEL CINTURÓN EXTERIOR AL NÚCLEO GALÁCTICO")
            boss2 = @("CADA JEFE EVOLUCIONA", "3 FASES · MODO FURIA"); phases = @("FASE 1", "FASE 2", "MODO FURIA"); foot = "NUEVOS PATRONES DE ATAQUE EN CADA FASE"
            weapons = @("MISILES Y PLASMA", "QUE LO DESTROZAN TODO") }
    fr = @{ boss1 = @("7 BOSS ÉPIQUES", "SAUREZ-VOUS LES VAINCRE ?")
            route = @("VOTRE ROUTE GALACTIQUE", "10 MONDES · 7 BOSS", "DE LA CEINTURE EXTÉRIEURE AU NOYAU GALACTIQUE")
            boss2 = @("CHAQUE BOSS ÉVOLUE", "3 PHASES · MODE FUREUR"); phases = @("PHASE 1", "PHASE 2", "MODE FUREUR"); foot = "DE NOUVELLES ATTAQUES À CHAQUE PHASE"
            weapons = @("MISSILES ET PLASMA", "QUI DÉTRUISENT TOUT") }
}

# Fotos elegidas (se cambian en capturas_v18_eleccion.ps1 tras ver las hojas de contacto)
$Sel = "D:\Temp\bosscap\ficha2\sel"
$Pick2 = @{}
foreach ($k in 'es', 'en', 'fr') {
    $Pick2[$k] = @{ boss1 = "$Sel\$($k)_boss1.png"; hero = "$Sel\$($k)_hero4.png"; p1 = "$Sel\$($k)_p1.png"; p2 = "$Sel\$($k)_p2.png"; route = "$F\route_$k\route.png" }
}
if (Test-Path "$PSScriptRoot\capturas_v18_eleccion.ps1") { . "$PSScriptRoot\capturas_v18_eleccion.ps1" }

# Partida con el jefe: se recorta solo el borde de arriba para que el jefe se vea entero
function Shot-Boss($path, $cap, $frame, $seed, $srcTop) {
    $c = New-Shot $seed; $g = $c[1]
    Draw-Text $g $cap[0] $famHead 3 132 960 540 118 $gold1 $gold2 14 $edgeDark | Out-Null
    Draw-Text $g $cap[1] $famHead 2 66 960 540 236 $white $null 10 $navyEdge | Out-Null
    # Recorte algo más cerrado (sin bordes ni botones de abajo): el jefe se ve un 14 % más grande
    $sx = 65; $sw = 950; $sh = 1480
    $x = 40; $y = 318; $w = 1000
    $h = [int]($sh * $w / [double]$sw)
    Glow $g ($x + $w / 2) ($y + $h / 2) ($h * .58) (C 90 30 110 255)
    foreach ($pw in 18, 10) {
        $o = Round-Path ($x - 8) ($y - 8) ($w + 16) ($h + 16) 44
        $pen = New-Object System.Drawing.Pen((C ([int](260 / $pw * 4)) 70 170 255), [single]$pw); $g.DrawPath($pen, $o); $pen.Dispose(); $o.Dispose()
    }
    $clip = Round-Path $x $y $w $h 38
    $img = [System.Drawing.Image]::FromFile($frame)
    $g.SetClip($clip)
    $dst = New-Object System.Drawing.Rectangle($x, $y, $w, $h)
    $src = New-Object System.Drawing.Rectangle($sx, $srcTop, $sw, $sh)
    $g.DrawImage($img, $dst, $src, [System.Drawing.GraphicsUnit]::Pixel); $g.ResetClip(); $img.Dispose()
    $pen = New-Object System.Drawing.Pen((C 255 140 210 255), [single]4); $g.DrawPath($pen, $clip); $pen.Dispose(); $clip.Dispose()
    Save-Shot $c $path
}

# Captura 1: foto ya recortada (jefe grande y mucha acción) dentro del marco
function Shot-Img($path, $cap, $frame, $seed) {
    $c = New-Shot $seed; $g = $c[1]
    Draw-Text $g $cap[0] $famHead 3 132 960 540 118 $gold1 $gold2 14 $edgeDark | Out-Null
    Draw-Text $g $cap[1] $famHead 2 66 960 540 236 $white $null 10 $navyEdge | Out-Null
    $img = [System.Drawing.Image]::FromFile($frame)
    $w = 1000; $h = [int]($img.Height * $w / $img.Width); $img.Dispose()
    Draw-Phone $g $frame 40 318 $w $h
    Save-Shot $c $path
}

function Pill($g, $text, $x, $y, $w, $col) {
    $rp = Round-Path $x $y $w 62 31
    $b = New-Object System.Drawing.SolidBrush((C 235 8 18 56)); $g.FillPath($b, $rp); $b.Dispose()
    $pen = New-Object System.Drawing.Pen($col, [single]4); $g.DrawPath($pen, $rp); $pen.Dispose(); $rp.Dispose()
    Draw-Text $g $text $famHead 0 36 ($w - 30) ($x + $w / 2) ($y + 31) $col $null 0 $null | Out-Null
}

# Captura 4: el mismo jefe en sus fases 1 y 2 (arriba) y en modo furia (grande, abajo)
function Shot-Phases($path, $cap, $labels, $foot, $p1, $p2, $hero, $seed) {
    $c = New-Shot $seed; $g = $c[1]
    Draw-Text $g $cap[0] $famHead 3 132 960 540 118 $gold1 $gold2 14 $edgeDark | Out-Null
    Draw-Text $g $cap[1] $famHead 2 66 960 540 236 $white $null 10 $navyEdge | Out-Null
    Draw-Phone $g $p1 40 330 490 470
    Draw-Phone $g $p2 550 330 490 470
    Pill $g $labels[0] 62 352 220 (C 255 120 200 255)
    Pill $g $labels[1] 572 352 220 (C 255 255 190 70)
    # flechas entre las fases
    Draw-Text $g ">" $famHead 0 90 80 540 565 $gold1 $gold2 6 $edgeDark | Out-Null
    $img = [System.Drawing.Image]::FromFile($hero)
    $w = 1000; $h = [int]($img.Height * $w / $img.Width); $img.Dispose()
    $hy = 850
    Draw-Phone $g $hero 40 $hy $w $h
    Pill $g $labels[2] 62 ($hy + 22) 330 (C 255 255 90 80)
    Draw-Text $g $foot $famSub 1 46 900 540 1850 $cyanText $null 0 $null | Out-Null
    Save-Shot $c $path
}

$TopCrop = 150
if ($null -ne $BossTopCrop) { $TopCrop = $BossTopCrop }

foreach ($k in 'en', 'es', 'fr') {
    $t = $Texts[$k]; $c2 = $Cap[$k]; $P = $V15[$k]; $P2 = $Pick2[$k]
    $d = Join-Path $out $k
    New-Item -ItemType Directory -Force $d | Out-Null
    if (-not $Only) { Get-ChildItem $d -Filter '*.png' | Remove-Item -Force }
    $jobs = [ordered]@{
        "01_jefes"    = { Shot-Img "$d\01_jefes.png" $c2.boss1 $P2.boss1 11 }
        "02_naves"    = { Shot-ShipsBig "$d\02_naves.png" $t.c2 $t.ships }
        "03_ruta"     = { Shot-Phone "$d\03_ruta.png" $c2.route $P2.route 61 }
        "04_fases"    = { Shot-Phases "$d\04_fases.png" $c2.boss2 $c2.phases $c2.foot $P2.p1 $P2.p2 $P2.hero 43 }
        "05_armas"    = { Shot-Action "$d\05_armas.png" $c2.weapons $P.weapons 31 }
        "06_misiones" = { Shot-Phone "$d\06_misiones.png" $t.c6 "$S1\$($k)_missions.png" 47 }
        "07_ranking"  = { Shot-Podium "$d\07_ranking.png" $t $k }
        "08_cofres"   = { Shot-Phone "$d\08_cofres.png" $t.c8 $P.chest 53 }
    }
    foreach ($j in $jobs.Keys) {
        if ($Only -and $j -notlike "*$Only*") { continue }
        & $jobs[$j]; "  $k/$j.png"
    }
}
