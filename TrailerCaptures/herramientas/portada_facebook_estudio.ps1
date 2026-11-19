# Portada de Facebook solo para el estudio (sin juegos): 1640x856, misma marca que el banner de YouTube.
# El texto va arriba del centro porque en computadora Facebook recorta a 1640x624 (de y=116 a y=740)
# y la foto de perfil tapa el centro de la parte de abajo.
. "$PSScriptRoot/capturas_v17_base.ps1" -Only "__ninguno__"
$ErrorActionPreference = 'Stop'
$Dest = "D:\Unity\AeroBlitz\StoreAssets\facebook"
New-Item -ItemType Directory -Force $Dest | Out-Null
$CovW = 1640; $CovH = 856

$bmp = New-Object System.Drawing.Bitmap($CovW, $CovH, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'; $g.PixelOffsetMode = 'HighQuality'; $g.CompositingQuality = 'HighQuality'; $g.TextRenderingHint = 'AntiAliasGridFit'

# Espacio: planeta azul a la izquierda, brillo naranja arriba a la derecha y nebulosa morada abajo
$rect = New-Object System.Drawing.Rectangle(0, 0, $CovW, $CovH)
$lg = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rect, (C 255 8 9 32), (C 255 18 11 50), [single]90)
$g.FillRectangle($lg, $rect); $lg.Dispose()
Glow $g 250 380 820 (C 215 14 112 196)
Glow $g 1520 50 560 (C 200 150 82 24)
Glow $g 1480 820 760 (C 215 132 28 134)
# Anillos suaves alrededor del planeta
foreach ($i in 1..5) {
    $r = 430 + $i * 62
    $pen = New-Object System.Drawing.Pen((C ([int](34 - $i * 5)) 120 190 255), [single]2)
    $g.DrawEllipse($pen, [single](250 - $r), [single](380 - $r), [single](2 * $r), [single](2 * $r)); $pen.Dispose()
}
$rnd = New-Object System.Random(21)
for ($i = 0; $i -lt 520; $i++) {
    $s = [single](1 + $rnd.NextDouble() * 2.4); $a = 55 + $rnd.Next(190)
    $b = New-Object System.Drawing.SolidBrush((C $a 225 238 255)); $g.FillEllipse($b, [single]$rnd.Next($CovW), [single]$rnd.Next($CovH), $s, $s); $b.Dispose()
}

# Nombre del estudio: blanco con borde cian y un halo suave detrás para que se lea sobre cualquier zona
Glow $g 820 275 620 (C 130 4 8 30)
Draw-Text $g "FRAILES CITY STUDIOS" $famHead 3 150 1260 820 266 (C 255 255 255 255) $null 16 (C 255 62 176 232) | Out-Null
# Lema y separador con estrella
Draw-Text $g "INDIE MOBILE GAMES" $famName 1 58 800 820 366 (C 255 74 208 255) $null 0 $null | Out-Null
$pen = New-Object System.Drawing.Pen((C 150 74 208 255), [single]3)
$g.DrawLine($pen, 470, 412, 760, 412); $g.DrawLine($pen, 880, 412, 1170, 412); $pen.Dispose()
$star = @(); for ($i = 0; $i -lt 10; $i++) { $r = if ($i % 2 -eq 0) { 17 } else { 7 }; $a = -[Math]::PI / 2 + $i * [Math]::PI / 5; $star += (P (820 + $r * [Math]::Cos($a)) (412 + $r * [Math]::Sin($a))) }
Glow $g 820 412 36 (C 160 255 205 70)
$sb = New-Object System.Drawing.SolidBrush((C 255 255 205 70)); $g.FillPolygon($sb, [System.Drawing.PointF[]]$star); $sb.Dispose()
$g.Dispose()

$png = "$Dest\portada_frailes_1640x856.png"
$bmp.Save($png, [System.Drawing.Imaging.ImageFormat]::Png)
$jpg = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
$ep = New-Object System.Drawing.Imaging.EncoderParameters(1)
$ep.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter([System.Drawing.Imaging.Encoder]::Quality, [long]95)
$bmp.Save("$Dest\portada_frailes_1640x856.jpg", $jpg, $ep)

# Vista previa: cómo la muestra Facebook en computadora (con la foto de perfil encima) y en el móvil
$avatar = [System.Drawing.Image]::FromFile("D:\Unity\AeroBlitz\StoreAssets\youtube_canal\perfil_800.png")
$prev = New-Object System.Drawing.Bitmap(1700, 520)
$pg = [System.Drawing.Graphics]::FromImage($prev); $pg.SmoothingMode = 'AntiAlias'; $pg.InterpolationMode = 'HighQualityBicubic'; $pg.Clear((C 255 242 243 246))
$font = New-Object System.Drawing.Font("Segoe UI Semibold", 13)
# Computadora: franja 1640x624 desde y=116, mostrada a 820x312
$pg.DrawImage($bmp, (New-Object System.Drawing.Rectangle(20, 40, 820, 312)), (New-Object System.Drawing.Rectangle(0, 116, 1640, 624)), [System.Drawing.GraphicsUnit]::Pixel)
$ac = New-Object System.Drawing.Drawing2D.GraphicsPath; $ac.AddEllipse(346, 172 + 40, 148, 148)
$wb = New-Object System.Drawing.SolidBrush((C 255 255 255 255)); $pg.FillEllipse($wb, 340, 166 + 40, 160, 160); $wb.Dispose()
$pg.SetClip($ac); $pg.DrawImage($avatar, 346, 212, 148, 148); $pg.ResetClip()
$pg.DrawString("Computadora (la foto de perfil tapa el centro de abajo)", $font, [System.Drawing.Brushes]::Black, 20, 14)
# Móvil: 1522x856 centrado, mostrado a 640x360
$pg.DrawImage($bmp, (New-Object System.Drawing.Rectangle(900, 40, 640, 360)), (New-Object System.Drawing.Rectangle(59, 0, 1522, 856)), [System.Drawing.GraphicsUnit]::Pixel)
$pg.DrawString("Móvil", $font, [System.Drawing.Brushes]::Black, 900, 14)
$pg.Dispose(); $prev.Save("$Dest\vista_previa.png", [System.Drawing.Imaging.ImageFormat]::Png); $prev.Dispose()
$avatar.Dispose(); $bmp.Dispose()
"listo: $Dest"
