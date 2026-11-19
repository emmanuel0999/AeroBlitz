# Capturas de Google Play centradas en la acción (1080x1920): combate, naves, mundos, armas, tormentas, misiones, ranking y cofres.
# Usa las funciones de capturas_v17_base.ps1. Fotos: D:\Temp\storecap\shots (menús, mundos) y shots2 (tormentas y cofres).
param([string]$Only = "")
$myOnly = $Only
. "$PSScriptRoot/capturas_v17_base.ps1" -Only "__ninguno__"
$Only = $myOnly
$S1 = "D:\Temp\storecap\shots"; $S2 = "D:\Temp\storecap\shots2"

# Partida grande: casi toda la imagen es juego (se recorta la franja de arriba del HUD para que la acción se vea mayor)
function Shot-Action($path, $cap, $frame, $seed) {
    $c = New-Shot $seed; $g = $c[1]
    Draw-Text $g $cap[0] $famHead 3 132 960 540 118 $gold1 $gold2 14 $edgeDark | Out-Null
    Draw-Text $g $cap[1] $famHead 2 66 960 540 236 $white $null 10 $navyEdge | Out-Null
    $x = 40; $y = 318; $w = 1000; $srcTop = 262
    $h = [int]((1920 - $srcTop) * $w / 1080.0)
    Glow $g ($x + $w / 2) ($y + $h / 2) ($h * .58) (C 90 30 110 255)
    foreach ($pw in 18, 10) {
        $o = Round-Path ($x - 8) ($y - 8) ($w + 16) ($h + 16) 44
        $pen = New-Object System.Drawing.Pen((C ([int](260 / $pw * 4)) 70 170 255), [single]$pw); $g.DrawPath($pen, $o); $pen.Dispose(); $o.Dispose()
    }
    $clip = Round-Path $x $y $w $h 38
    $img = [System.Drawing.Image]::FromFile($frame)
    $g.SetClip($clip)
    $dst = New-Object System.Drawing.Rectangle($x, $y, $w, $h)
    $src = New-Object System.Drawing.Rectangle(0, $srcTop, 1080, (1920 - $srcTop))
    $g.DrawImage($img, $dst, $src, [System.Drawing.GraphicsUnit]::Pixel); $g.ResetClip(); $img.Dispose()
    $pen = New-Object System.Drawing.Pen((C 255 140 210 255), [single]4); $g.DrawPath($pen, $clip); $pen.Dispose(); $clip.Dispose()
    Save-Shot $c $path
}

# Naves más grandes: las 3 premium arriba y las 8 de monedas (de serie y neón) debajo
function Shot-ShipsBig($path, $cap, $t) {
    $c = New-Shot 77; $g = $c[1]
    Draw-Text $g $cap[0] $famHead 3 132 960 540 118 $gold1 $gold2 14 $edgeDark | Out-Null
    Draw-Text $g $cap[1] $famHead 2 66 960 540 236 $white $null 10 $navyEdge | Out-Null
    $prem = @(@("titan", 215, 690, 370, (C 150 120 160 255), $t.titan), @("nova", 865, 690, 370, (C 150 255 120 230), $t.nova), @("phoenix", 540, 610, 500, (C 170 255 140 60), $t.phoenix))
    foreach ($s in $prem) { Draw-ShipImg $g "$Ships\$($s[0])_metal.png" $s[1] $s[2] $s[3] $s[4] }
    foreach ($s in $prem) { Draw-Text $g $s[5] $famHead 0 56 330 $s[1] ($s[2] + $s[3] * .42 + 6) $gold1 $gold2 6 $edgeDark | Out-Null }
    $pill = Round-Path 400 318 280 56 28
    $br = New-Object System.Drawing.Drawing2D.LinearGradientBrush((New-Object System.Drawing.RectangleF(400, 318, 280, 56)), $gold1, $gold2, [single]90)
    $g.FillPath($br, $pill); $br.Dispose(); $pill.Dispose()
    Draw-Text $g "PREMIUM" $famHead 0 34 240 540 346 (C 255 60 24 4) $null 0 $null | Out-Null
    $row1 = @("falcon", "wasp", "hummingbird", "guardian"); $row2 = @("bastion", "viper", "comet", "specter")
    for ($i = 0; $i -lt 4; $i++) {
        $cx = 145 + $i * 263
        Draw-ShipImg $g "$Ships\$($row1[$i])_cartoon.png" $cx 1110 270 (C 70 60 140 255)
        Draw-Text $g $t.names[$row1[$i]] $famName 1 34 250 $cx 1262 $white $null 0 $null | Out-Null
        Draw-ShipImg $g "$Ships\$($row2[$i])_neon.png" $cx 1430 270 (C 90 80 200 255)
        Draw-Text $g $t.names[$row2[$i]] $famName 1 34 250 $cx 1582 $white $null 0 $null | Out-Null
    }
    $chips = $t.styles; $cw = 300
    for ($i = 0; $i -lt 3; $i++) {
        $x = 540 - (3 * $cw + 2 * 22) / 2 + $i * ($cw + 22)
        $rp = Round-Path $x 1660 $cw 78 39
        $b = New-Object System.Drawing.SolidBrush((C 220 10 24 64)); $g.FillPath($b, $rp); $b.Dispose()
        $col = @((C 255 255 170 60), (C 255 90 240 255), (C 255 200 210 230))[$i]
        $pen = New-Object System.Drawing.Pen($col, [single]4); $g.DrawPath($pen, $rp); $pen.Dispose(); $rp.Dispose()
        Draw-Text $g $chips[$i] $famHead 0 38 ($cw - 40) ($x + $cw / 2) 1699 $col $null 0 $null | Out-Null
    }
    Draw-Text $g $cap[2] $famSub 1 46 900 540 1850 $cyanText $null 0 $null | Out-Null
    Save-Shot $c $path
}

$UiDir = "D:\Unity\AeroBlitz\Assets\AeroBlitz\Resources\UI"

function Draw-Img($g, $file, $x, $y, $w, $h) {
    $img = [System.Drawing.Image]::FromFile($file)
    $g.DrawImage($img, [single]$x, [single]$y, [single]$w, [single]$h); $img.Dispose()
}

# Nave del juego con el color de una pintura (oro, plata o bronce)
function Draw-TintedShip($g, $file, $cx, $cy, $size, $tr, $tg, $tb, $glow) {
    if ($glow) { Glow $g $cx $cy ($size * .7) $glow }
    $cm = New-Object System.Drawing.Imaging.ColorMatrix
    $k = 1.45
    $cm.Matrix00 = .3 * $tr * $k; $cm.Matrix10 = .59 * $tr * $k; $cm.Matrix20 = .11 * $tr * $k
    $cm.Matrix01 = .3 * $tg * $k; $cm.Matrix11 = .59 * $tg * $k; $cm.Matrix21 = .11 * $tg * $k
    $cm.Matrix02 = .3 * $tb * $k; $cm.Matrix12 = .59 * $tb * $k; $cm.Matrix22 = .11 * $tb * $k
    $cm.Matrix33 = 1; $cm.Matrix40 = .04; $cm.Matrix41 = .04; $cm.Matrix42 = .04
    $ia = New-Object System.Drawing.Imaging.ImageAttributes; $ia.SetColorMatrix($cm)
    $img = [System.Drawing.Image]::FromFile($file)
    $dst = New-Object System.Drawing.Rectangle([int]($cx - $size / 2), [int]($cy - $size / 2), [int]$size, [int]$size)
    $g.DrawImage($img, $dst, 0, 0, $img.Width, $img.Height, [System.Drawing.GraphicsUnit]::Pixel, $ia)
    $img.Dispose(); $ia.Dispose()
}

# Plataforma del podio: frente con degradado, cara superior iluminada, borde de neón y el número del puesto
function Draw-Platform($g, $x, $top, $w, $base, $c1, $c2, $edge, $num) {
    $d = 34
    $topFace = [System.Drawing.PointF[]]@((P ($x + $d) ($top - $d)), (P ($x + $w - $d) ($top - $d)), (P ($x + $w) $top), (P $x $top))
    $b = New-Object System.Drawing.SolidBrush((C 255 ([Math]::Min(255, $c1.R + 40)) ([Math]::Min(255, $c1.G + 40)) ([Math]::Min(255, $c1.B + 40))))
    $g.FillPolygon($b, $topFace); $b.Dispose()
    $rect = New-Object System.Drawing.RectangleF([single]$x, [single]$top, [single]$w, [single]($base - $top))
    $br = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, $c1, $c2, [single]90)
    $g.FillRectangle($br, $rect); $br.Dispose()
    $sheen = New-Object System.Drawing.SolidBrush((C 40 255 255 255)); $g.FillRectangle($sheen, [single]$x, [single]$top, [single]($w * .28), [single]($base - $top)); $sheen.Dispose()
    foreach ($pw in 12, 4) {
        $pen = New-Object System.Drawing.Pen((C ($(if ($pw -eq 12) { 90 } else { 255 })) $edge.R $edge.G $edge.B), [single]$pw)
        $g.DrawLine($pen, [single]$x, [single]$top, [single]($x + $w), [single]$top)
        $g.DrawPolygon($pen, $topFace); $pen.Dispose()
    }
    Draw-Text $g $num $famHead 3 ([Math]::Min(150, ($base - $top) * .62)) ($w * .6) ($x + $w / 2) ($top + ($base - $top) * .5) (C 255 255 255 255) $null 10 (C 200 10 10 30) | Out-Null
}

function Fit-Size($text, $fam, $style, $maxSize, $maxW) {
    $p = Text-Path $text $fam $style 100
    $s = [Math]::Min([double]$maxSize, 100.0 * $maxW / [Math]::Max(1.0, [double]$p.GetBounds().Width)); $p.Dispose()
    $p = Text-Path $text $fam $style $s; $w = $p.GetBounds().Width; $p.Dispose()
    return @($s, $w)
}

function Shot-Podium($path, $t, $lang) {
    $c = New-Shot 91; $g = $c[1]
    # Título con trofeo
    $fs = Fit-Size $t.r1 $famHead 3 132 800
    $total = 120 + 24 + $fs[1]; $sx = 540 - $total / 2
    Draw-Img $g "$UiDir\icon_trophy.png" $sx 58 120 120
    Draw-Text $g $t.r1 $famHead 3 $fs[0] 800 ($sx + 144 + $fs[1] / 2) 118 $gold1 $gold2 14 $edgeDark | Out-Null
    Draw-Text $g $t.r2 $famHead 2 58 960 540 236 $white $null 10 $navyEdge | Out-Null

    # Escenario: halo azul, rayos dorados detrás del #1 y partículas
    $inner = Round-Path 34 34 1012 1852 40
    $g.SetClip($inner)   # rayos y partículas sin pisar el marco
    Glow $g 540 1120 620 (C 120 40 120 255)
    $g.TranslateTransform(540, 690)
    for ($i = 0; $i -lt 16; $i++) {
        $a = $i * 22.5
        $g.RotateTransform([single]22.5)
        $ray = [System.Drawing.PointF[]]@((P 0 0), (P -38 -720), (P 38 -720))
        $rb = New-Object System.Drawing.Drawing2D.LinearGradientBrush((P 0 0), (P 0 -720), (C ($(if ($i % 2 -eq 0) { 90 } else { 45 })) 255 210 90), (C 0 255 210 90))
        $g.FillPolygon($rb, $ray); $rb.Dispose()
    }
    $g.ResetTransform()
    Glow $g 540 690 330 (C 150 255 200 80)
    $rnd = New-Object System.Random(17)
    for ($i = 0; $i -lt 70; $i++) {
        $px = 80 + $rnd.Next(920); $py = 330 + $rnd.Next(860); $r = 3 + $rnd.Next(7)
        $col = if ($rnd.Next(2) -eq 0) { C 230 255 215 110 } else { C 230 120 210 255 }
        Glow $g $px $py ($r * 3) (C 90 $col.R $col.G $col.B)
        $b = New-Object System.Drawing.SolidBrush($col); $g.FillEllipse($b, [single]($px - $r / 2), [single]($py - $r / 2), [single]$r, [single]$r); $b.Dispose()
    }
    $g.ResetClip(); $inner.Dispose()

    # Podio: #2 plata a la izquierda, #1 oro en el centro, #3 bronce a la derecha
    $base = 1190
    Glow $g 540 $base 520 (C 110 60 160 255)
    Draw-Platform $g 110 1000 280 $base (C 255 190 200 220) (C 255 90 100 125) (C 255 210 230 255) "2"
    Draw-Platform $g 690 1050 280 $base (C 255 200 120 70) (C 255 105 55 30) (C 255 255 170 110) "3"
    Draw-Platform $g 380 920 320 $base (C 255 255 205 70) (C 255 180 110 10) (C 255 255 235 140) "1"

    Draw-TintedShip $g "$Ships\titan_metal.png" 250 845 300 0.86 0.9 1.0 (C 120 170 200 255)
    Draw-TintedShip $g "$Ships\nova_metal.png" 830 900 290 1.0 0.62 0.36 (C 120 255 140 70)
    Draw-TintedShip $g "$Ships\phoenix_metal.png" 540 700 410 1.0 0.8 0.28 (C 170 255 200 60)
    # Corona sobre el #1
    Glow $g 540 445 120 (C 160 255 220 90)
    Draw-Img $g "$UiDir\icon_crown.png" 445 360 190 190

    # Top 3 con medallas
    $rows = $t.top
    for ($i = 0; $i -lt 3; $i++) {
        $y = 1228 + $i * 112
        $rp = Round-Path 90 $y 900 98 26
        $fill = @((C 230 70 52 10), (C 230 30 40 64), (C 230 58 34 20))[$i]
        $edgeC = @((C 255 255 210 90), (C 255 190 205 230), (C 255 230 150 100))[$i]
        $b = New-Object System.Drawing.SolidBrush($fill); $g.FillPath($b, $rp); $b.Dispose()
        $pen = New-Object System.Drawing.Pen($edgeC, [single]4); $g.DrawPath($pen, $rp); $pen.Dispose(); $rp.Dispose()
        Draw-Img $g "$UiDir\medal_$($i + 1).png" 108 ($y + 6) 86 86
        $nm = Fit-Size $rows[$i][0] $famName 1 46 470
        $p = Text-Path $rows[$i][0] $famName 1 $nm[0]
        $bb = $p.GetBounds(); $p.Dispose()
        Draw-Text $g $rows[$i][0] $famName 1 46 470 (220 + $nm[1] / 2) ($y + 49) (C 255 255 255 255) $null 0 $null | Out-Null
        Draw-Text $g $rows[$i][1] $famHead 0 48 280 (830) ($y + 49) $edgeC $null 0 $null | Out-Null
    }

    # Tarjeta destacada
    $cy = 1590; $rp = Round-Path 130 $cy 820 200 34
    $br = New-Object System.Drawing.Drawing2D.LinearGradientBrush((New-Object System.Drawing.RectangleF(130, $cy, 820, 200)), (C 240 20 60 150), (C 240 10 24 70), [single]90)
    $g.FillPath($br, $rp); $br.Dispose()
    foreach ($pw in 14, 5) { $pen = New-Object System.Drawing.Pen((C ($(if ($pw -eq 14) { 80 } else { 255 })) 110 200 255), [single]$pw); $g.DrawPath($pen, $rp); $pen.Dispose() }
    $rp.Dispose()
    Draw-Img $g "$UiDir\icon_globe.png" 170 ($cy + 40) 120 120
    Draw-Text $g $t.r3 $famHead 3 86 560 610 ($cy + 72) $gold1 $gold2 10 $edgeDark | Out-Null
    Draw-Text $g $t.r4 $famSub 1 44 560 610 ($cy + 148) $cyanText $null 0 $null | Out-Null
    Draw-Text $g $t.r5 $famSub 1 44 900 540 1852 $cyanText $null 0 $null | Out-Null
    Save-Shot $c $path
}

$Texts = [ordered]@{
    en = @{
        c1 = @("CAN YOU SURVIVE", "70 EXTREME LEVELS?")
        c2 = @("11 UNIQUE SHIPS", "FLY YOUR FAVORITE", "CARTOON, NEON & METALLIC STYLES")
        c3 = @("EXPLORE 10 WORLDS", "EACH WITH ITS OWN MUSIC", "FROM ICE RINGS TO THE GALACTIC CORE")
        c4 = @("MISSILES & PLASMA", "THAT SMASH EVERYTHING")
        c5 = @("ASTEROID STORMS", "ONLY THE BEST SURVIVE")
        c6 = @("REWARDS EVERY DAY", "MISSIONS & DAILY STREAK", "NEW GOALS EVERY DAY AND EVERY WEEK")
        c7 = @("REACH THE TOP", "OF THE WORLD RANKING", "COMPETE WORLDWIDE AND IN YOUR COUNTRY")
        r1 = "REACH THE TOP"; r2 = "COMPETE WITH PILOTS WORLDWIDE"; r3 = "GLOBAL TOP"; r4 = "WORLD AND COUNTRY RANKINGS"; r5 = "CLIMB, BEAT YOUR RIVALS, BE NUMBER 1"
        top = @(@("SkyRaider (US)", "125,430"), @("NovaQueen (BR)", "118,960"), @("Kaito77 (JP)", "104,215"))
        c8 = @("OPEN EPIC CHESTS", "PACKED WITH REWARDS", "EARNED BY PLAYING, NEVER SOLD")
        ships = @{ styles = @("CARTOON", "NEON", "METALLIC"); phoenix = "Phoenix"; titan = "Titan"; nova = "Nova"
            names = @{ falcon = "Falcon"; wasp = "Wasp"; hummingbird = "Hummingbird"; guardian = "Guardian"; bastion = "Bastion"; viper = "Viper"; comet = "Comet"; specter = "Specter" } }
        worlds = @("Outer Belt", "Ice Rings", "Crimson Nebula", "Minefield", "Solar Flare", "Abandoned Station", "Laser Corridor", "Plasma Sea", "Dark Frontier", "Galactic Core")
    }
    es = @{
        c1 = @("¿PUEDES SOBREVIVIR", "A 70 NIVELES EXTREMOS?")
        c2 = @("11 NAVES ÚNICAS", "PILOTA TU FAVORITA", "ESTILOS CARTOON, NEÓN Y METÁLICO")
        c3 = @("EXPLORA 10 MUNDOS", "CADA UNO CON SU MÚSICA", "DE LOS ANILLOS DE HIELO AL NÚCLEO GALÁCTICO")
        c4 = @("MISILES Y PLASMA", "QUE LO DESTROZAN TODO")
        c5 = @("TORMENTAS DE ASTEROIDES", "SOLO LOS MEJORES SOBREVIVEN")
        c6 = @("PREMIOS CADA DÍA", "MISIONES Y RACHA DIARIA", "NUEVOS RETOS CADA DÍA Y CADA SEMANA")
        c7 = @("LLEGA A LA CIMA", "DEL RANKING MUNDIAL", "COMPITE EN EL MUNDO Y EN TU PAÍS")
        r1 = "LLEGA A LA CIMA"; r2 = "COMPITE CONTRA PILOTOS DE TODO EL MUNDO"; r3 = "TOP GLOBAL"; r4 = "RANKING MUNDIAL Y POR PAÍS"; r5 = "SUBE, SUPERA A TUS RIVALES, SÉ EL NÚMERO 1"
        top = @(@("SkyRaider (US)", "125.430"), @("NovaQueen (BR)", "118.960"), @("Kaito77 (JP)", "104.215"))
        c8 = @("ABRE COFRES ÉPICOS", "LLENOS DE PREMIOS", "SE GANAN JUGANDO, NUNCA SE VENDEN")
        ships = @{ styles = @("CARTOON", "NEÓN", "METÁLICO"); phoenix = "Fénix"; titan = "Titán"; nova = "Nova"
            names = @{ falcon = "Halcón"; wasp = "Avispa"; hummingbird = "Colibrí"; guardian = "Guardián"; bastion = "Bastión"; viper = "Víbora"; comet = "Cometa"; specter = "Espectro" } }
        worlds = @("Cinturón Exterior", "Anillos de Hielo", "Nebulosa Carmesí", "Campo Minado", "Llamarada Solar", "Estación Abandonada", "Corredor de Láseres", "Mar de Plasma", "Frontera Oscura", "Núcleo Galáctico")
    }
    fr = @{
        c1 = @("SURVIVREZ-VOUS", "À 70 NIVEAUX EXTRÊMES ?")
        c2 = @("11 VAISSEAUX UNIQUES", "PILOTEZ VOTRE FAVORI", "STYLES CARTOON, NÉON ET MÉTALLIQUE")
        c3 = @("EXPLOREZ 10 MONDES", "CHACUN AVEC SA MUSIQUE", "DES ANNEAUX DE GLACE AU NOYAU GALACTIQUE")
        c4 = @("MISSILES ET PLASMA", "QUI DÉTRUISENT TOUT")
        c5 = @("TEMPÊTES D'ASTÉROÏDES", "SEULS LES MEILLEURS SURVIVENT")
        c6 = @("DES CADEAUX CHAQUE JOUR", "MISSIONS ET SÉRIE QUOTIDIENNE", "DE NOUVEAUX DÉFIS CHAQUE JOUR ET CHAQUE SEMAINE")
        c7 = @("ATTEIGNEZ LE SOMMET", "DU CLASSEMENT MONDIAL", "AFFRONTEZ LE MONDE ET VOTRE PAYS")
        r1 = "ATTEIGNEZ LE SOMMET"; r2 = "AFFRONTEZ DES PILOTES DU MONDE ENTIER"; r3 = "TOP MONDIAL"; r4 = "CLASSEMENTS MONDIAL ET PAR PAYS"; r5 = "GRIMPEZ, BATTEZ VOS RIVAUX, DEVENEZ NUMÉRO 1"
        top = @(@("SkyRaider (US)", "125 430"), @("NovaQueen (BR)", "118 960"), @("Kaito77 (JP)", "104 215"))
        c8 = @("OUVREZ DES COFFRES ÉPIQUES", "REMPLIS DE RÉCOMPENSES", "GAGNÉS EN JOUANT, JAMAIS VENDUS")
        ships = @{ styles = @("CARTOON", "NÉON", "MÉTALLIQUE"); phoenix = "Phénix"; titan = "Titan"; nova = "Nova"
            names = @{ falcon = "Faucon"; wasp = "Guêpe"; hummingbird = "Colibri"; guardian = "Gardien"; bastion = "Bastion"; viper = "Vipère"; comet = "Comète"; specter = "Spectre" } }
        worlds = @("Ceinture Extérieure", "Anneaux de Glace", "Nébuleuse Pourpre", "Champ de Mines", "Éruption Solaire", "Station Abandonnée", "Couloir Laser", "Mer de Plasma", "Frontière Obscure", "Noyau Galactique")
    }
}

# Fotos elegidas (las de partida cambian en cada idioma porque los textos del juego también)
$Pick = @{
    en = @{ combat = "$S2\en_storm_nova_16.png"; weapons = "$S2\en_storm_titan_05.png"; storm = "$S2\en_storm_phoenix_20.png"; chest = "$S2\en_chestB_04.png" }
    es = @{ combat = "$S2\es_storm_nova_06.png"; weapons = "$S2\es_storm_titan_08.png"; storm = "$S2\es_storm_phoenix_16.png"; chest = "$S2\es_chestB_04.png" }
    fr = @{ combat = "$S2\fr_storm_nova_15.png"; weapons = "$S2\fr_storm_titan_09.png"; storm = "$S2\fr_storm_phoenix_19.png"; chest = "$S2\fr_chestB_04.png" }
}
if (Test-Path "$PSScriptRoot\capturas_v17_eleccion.ps1") { . "$PSScriptRoot\capturas_v17_eleccion.ps1" }

$Frames = $S1
foreach ($k in $Texts.Keys) {
    $t = $Texts[$k]; $d = Join-Path $out $k; $P = $Pick[$k]
    New-Item -ItemType Directory -Force $d | Out-Null
    if (-not $Only) { Get-ChildItem $d -Filter '*.png' | Remove-Item -Force }   # quita la serie anterior
    $jobs = [ordered]@{
        "01_combate"   = { Shot-Action "$d\01_combate.png" $t.c1 $P.combat 11 }
        "02_naves"     = { Shot-ShipsBig "$d\02_naves.png" $t.c2 $t.ships }
        "03_mundos"    = { Shot-Worlds "$d\03_mundos.png" $t.c3 $t.worlds }
        "04_armas"     = { Shot-Action "$d\04_armas.png" $t.c4 $P.weapons 31 }
        "05_tormentas" = { Shot-Action "$d\05_tormentas.png" $t.c5 $P.storm 43 }
        "06_misiones"  = { Shot-Phone "$d\06_misiones.png" $t.c6 "$S1\$($k)_missions.png" 47 }
        "07_ranking"   = { Shot-Podium "$d\07_ranking.png" $t $k }
        "08_cofres"    = { Shot-Phone "$d\08_cofres.png" $t.c8 $P.chest 53 }
    }
    foreach ($j in $jobs.Keys) {
        if ($Only -and $j -notlike "*$Only*") { continue }
        & $jobs[$j]; "  $k/$j.png"
    }
}
