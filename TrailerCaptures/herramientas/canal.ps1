# Imagen de perfil y banner del canal de YouTube de Frailes City Studios
$here = Split-Path $MyInvocation.MyCommand.Path
. "$here\rotulos.ps1" | Out-Null   # reutiliza fondo espacial, textos con brillo, icono y botón
$icon = [System.Drawing.Image]::FromFile("D:\Unity\AeroBlitz\StoreAssets\icono_512.png")
$dir = "D:\Unity\AeroBlitz\StoreAssets\youtube_canal"
New-Item -ItemType Directory -Force $dir | Out-Null

# Perfil 800x800: YouTube lo recorta en círculo, todo va en el centro
$c = New-Canvas 800 800; $bmp = $c[0]; $g = $c[1]
Draw-Space $g 800 800 52
Draw-Glow $g 400 400 330 (C 120 60 160 255)
$ring = New-Object System.Drawing.Pen((C 200 80 220 255), [single]10)
$g.DrawEllipse($ring, [single]95, [single]95, [single]610, [single]610); $ring.Dispose()
Draw-Text $g "FCS" $famBlack 230 400 370 $white $cyan
Draw-Text $g "STUDIOS" $famBold 64 400 540 (C 255 255 200 50) $null
$bmp.Save("$dir\perfil_800.png", [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()
"  perfil_800.png"

# Banner 2560x1440: la zona que se ve en todos los dispositivos es el centro de 1546x423
$c = New-Canvas 2560 1440; $bmp = $c[0]; $g = $c[1]
Draw-Space $g 2560 1440 61
$cx = 1280
Draw-Icon $g 690 720 250
$s = Fit @("FRAILES CITY STUDIOS") $famBlack 130 1150
Draw-Text $g "FRAILES CITY STUDIOS" $famBlack $s 1430 640 $white $cyan
Draw-Text $g "INDIE MOBILE GAMES" $famBold 46 1430 740 $cyan $null
Draw-Pill $g "NEW: AERO BLITZ ON GOOGLE PLAY" 1430 840 34
$bmp.Save("$dir\banner_2560x1440.png", [System.Drawing.Imaging.ImageFormat]::Png)
# Vista previa con la zona segura marcada (solo para revisar, no se sube)
$pen = New-Object System.Drawing.Pen((C 200 255 60 60), [single]4)
$g.DrawRectangle($pen, 507, 508, 1546, 423); $pen.Dispose()
$bmp.Save("$dir\banner_vista_zona_segura.png", [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose(); $icon.Dispose()
"  banner_2560x1440.png"
