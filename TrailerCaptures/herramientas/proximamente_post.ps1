# Imagen "Pronto llegan nuevas mejoras" para Facebook e Instagram (4:5 y 1:1, español e inglés), con el estilo de la ficha.
. "$PSScriptRoot/capturas_v17_base.ps1" -Only "__ninguno__"
$ErrorActionPreference = 'Stop'
$Dest = "D:\Unity\AeroBlitz\StoreAssets\redes"
New-Item -ItemType Directory -Force $Dest | Out-Null
$UiDir = "D:\Unity\AeroBlitz\Assets\AeroBlitz\Resources\UI"
$logoImg = [System.Drawing.Image]::FromFile("$UiDir\logo.png")

$Txt = @{
    es = @{ l1 = "NUEVAS MEJORAS"; l2 = "LLEGAN PRONTO"; chips = @(@("icon_rocket", "NUEVAS NAVES"), @("icon_missions", "MISIONES DIARIAS"), @("icon_chest", "COFRES ÉPICOS")); foot = "¡NO TE LO PIERDAS!" }
    en = @{ l1 = "NEW IMPROVEMENTS"; l2 = "COMING SOON"; chips = @(@("icon_rocket", "NEW SHIPS"), @("icon_missions", "DAILY MISSIONS"), @("icon_chest", "EPIC CHESTS")); foot = "STAY TUNED!" }
}

function Make-Post($lang, $PW, $PH, $L, $path) {
    $t = $Txt[$lang]
    $bmp = New-Object System.Drawing.Bitmap($PW, $PH, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'; $g.PixelOffsetMode = 'HighQuality'; $g.CompositingQuality = 'HighQuality'; $g.TextRenderingHint = 'AntiAliasGridFit'
    $rect = New-Object System.Drawing.Rectangle(0, 0, $PW, $PH)
    $lg = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, (C 255 4 8 26), (C 255 12 20 58), [single]90)
    $g.FillRectangle($lg, $rect); $lg.Dispose()
    $rnd = New-Object System.Random(12)
    Glow $g 220 ($PH * .35) 620 (C 70 40 90 255); Glow $g 880 ($PH * .75) 680 (C 60 140 50 220); Glow $g 540 120 520 (C 50 255 140 40)
    for ($i = 0; $i -lt 220; $i++) {
        $s = [single](1 + $rnd.NextDouble() * 2.6); $a = 60 + $rnd.Next(190)
        $b = New-Object System.Drawing.SolidBrush((C $a 220 235 255)); $g.FillEllipse($b, [single]$rnd.Next($PW), [single]$rnd.Next($PH), $s, $s); $b.Dispose()
    }
    # Rayos de luz detrás de la nave (dentro del marco)
    $inner = Round-Path 30 30 ($PW - 60) ($PH - 60) 40
    $g.SetClip($inner)
    $g.TranslateTransform($PW / 2, $L.hy)
    for ($i = 0; $i -lt 16; $i++) {
        $g.RotateTransform([single]22.5)
        $ray = [System.Drawing.PointF[]]@((P 0 0), (P -34 -760), (P 34 -760))
        $rb = New-Object System.Drawing.Drawing2D.LinearGradientBrush((P 0 0), (P 0 -760), (C ($(if ($i % 2 -eq 0) { 80 } else { 40 })) 255 190 90), (C 0 255 190 90))
        $g.FillPolygon($rb, $ray); $rb.Dispose()
    }
    $g.ResetTransform()
    for ($i = 0; $i -lt 45; $i++) {
        $px = 70 + $rnd.Next($PW - 140); $py = $L.hy - 330 + $rnd.Next(660); $r = 3 + $rnd.Next(6)
        $col = if ($rnd.Next(2) -eq 0) { C 220 255 215 110 } else { C 220 120 210 255 }
        Glow $g $px $py ($r * 3) (C 90 $col.R $col.G $col.B)
        $b = New-Object System.Drawing.SolidBrush($col); $g.FillEllipse($b, [single]($px - $r / 2), [single]($py - $r / 2), [single]$r, [single]$r); $b.Dispose()
    }
    $g.ResetClip(); $inner.Dispose()
    # Marco azul con brillo
    foreach ($bw in 26, 16, 9) {
        $fp = Round-Path 16 16 ($PW - 32) ($PH - 32) 46
        $pen = New-Object System.Drawing.Pen((C ([int](190 / ($bw / 4))) 60 150 255), [single]$bw); $g.DrawPath($pen, $fp); $pen.Dispose(); $fp.Dispose()
    }
    $fp = Round-Path 16 16 ($PW - 32) ($PH - 32) 46
    $pen = New-Object System.Drawing.Pen((C 255 120 200 255), [single]4); $g.DrawPath($pen, $fp); $pen.Dispose(); $fp.Dispose()

    # Logo y titular
    $lh = $L.logoW * $logoImg.Height / $logoImg.Width
    Glow $g ($PW / 2) ($L.logoY + $lh / 2) ($L.logoW * .55) (C 140 2 6 24)
    $g.DrawImage($logoImg, [single]($PW / 2 - $L.logoW / 2), [single]$L.logoY, [single]$L.logoW, [single]$lh)
    Draw-Text $g $t.l1 $famHead 3 $L.s1 ($PW - 90) ($PW / 2) $L.y1 $gold1 $gold2 14 $edgeDark | Out-Null
    Draw-Text $g $t.l2 $famHead 2 $L.s2 ($PW - 120) ($PW / 2) $L.y2 $white $null 10 $navyEdge | Out-Null

    # Naves: Fénix grande en el centro, Titán y Nova a los lados
    Draw-ShipImg $g "$Ships\titan_metal.png" $L.sideL $L.sideY $L.sideS (C 140 120 160 255)
    Draw-ShipImg $g "$Ships\nova_metal.png" $L.sideR $L.sideY $L.sideS (C 140 255 120 230)
    Draw-ShipImg $g "$Ships\phoenix_metal.png" ($PW / 2) $L.hy $L.hs (C 190 255 190 60)

    # Tres novedades en píldoras
    $cw = 320; $gap = 20; $x0 = $PW / 2 - (3 * $cw + 2 * $gap) / 2
    for ($i = 0; $i -lt 3; $i++) {
        $x = $x0 + $i * ($cw + $gap)
        $rp = Round-Path $x $L.cy $cw 84 42
        $b = New-Object System.Drawing.SolidBrush((C 225 10 24 70)); $g.FillPath($b, $rp); $b.Dispose()
        $pen = New-Object System.Drawing.Pen((C 255 255 190 70), [single]4); $g.DrawPath($pen, $rp); $pen.Dispose(); $rp.Dispose()
        $ic = [System.Drawing.Image]::FromFile("$UiDir\$($t.chips[$i][0]).png")
        $g.DrawImage($ic, [single]($x + 16), [single]($L.cy + 12), [single]60, [single]60); $ic.Dispose()
        Draw-Text $g $t.chips[$i][1] $famHead 0 30 ($cw - 100) ($x + 84 + ($cw - 100) / 2) ($L.cy + 43) $white $null 0 $null | Out-Null
    }
    Draw-Text $g $t.foot $famHead 3 52 900 ($PW / 2) $L.fy $gold1 $gold2 8 $edgeDark | Out-Null
    Draw-Text $g "FRAILES CITY STUDIOS" $famSub 1 38 800 ($PW / 2) $L.sy $cyanText $null 0 $null | Out-Null
    $g.Dispose()
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png); $bmp.Dispose()
}

$L45 = @{ logoW = 600; logoY = 44; s1 = 136; y1 = 355; s2 = 78; y2 = 470; hy = 770; hs = 500; sideL = 205; sideR = 875; sideY = 850; sideS = 300; cy = 1070; fy = 1212; sy = 1292 }
$L11 = @{ logoW = 480; logoY = 28; s1 = 112; y1 = 262; s2 = 62; y2 = 350; hy = 590; hs = 390; sideL = 175; sideR = 905; sideY = 665; sideS = 240; cy = 850; fy = 975; sy = 1038 }
foreach ($lang in 'es', 'en') {
    Make-Post $lang 1080 1350 $L45 "$Dest\proximamente_$($lang)_4x5.png"
    Make-Post $lang 1080 1080 $L11 "$Dest\proximamente_$($lang)_1x1.png"
    "  $lang listo"
}
$logoImg.Dispose()
