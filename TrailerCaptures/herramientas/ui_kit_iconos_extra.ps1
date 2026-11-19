# Iconos añadidos para ajustes, vidas, ranking, tienda, hangar y diálogos (se dibujan en un espacio de 128 y se guardan a 256 px).
# Lo carga ui_kit.ps1 (usa sus funciones C, NewIcon, Save, PF, Poly, Brush, Pen, VGrad, RoundRect).
$red = C 255 90 100

# Altavoz (sonido) y altavoz tachado
function Speaker($g) { $g.FillPolygon((Brush $white), (Poly @(14,48, 38,48, 66,22, 66,106, 38,80, 14,80))) }
$cv = NewIcon; $g = $cv[1]; Speaker $g
$wp = Pen $white 9; $g.DrawArc($wp, 60, 40, 30, 48, -55, 110); $g.DrawArc($wp, 62, 24, 52, 80, -55, 110)
Save $cv[0] $g "icon_sound"
$cv = NewIcon; $g = $cv[1]; Speaker $g
$xp = Pen $red 11; $g.DrawLine($xp, 80, 46, 114, 82); $g.DrawLine($xp, 114, 46, 80, 82)
Save $cv[0] $g "icon_sound_off"

# Nota musical y nota tachada
function Note($g) {
    $g.FillEllipse((Brush $white), 12, 84, 38, 28); $g.FillEllipse((Brush $white), 68, 72, 38, 28)
    $g.FillRectangle((Brush $white), 41, 26, 9, 74); $g.FillRectangle((Brush $white), 97, 14, 9, 74)
    $g.FillPolygon((Brush $white), (Poly @(41,26, 106,12, 106,34, 41,48)))
}
$cv = NewIcon; $g = $cv[1]; Note $g; Save $cv[0] $g "icon_music"
$cv = NewIcon; $g = $cv[1]; Note $g; $g.DrawLine((Pen $red 11), 12, 12, 116, 116); Save $cv[0] $g "icon_music_off"

# Vibración (teléfono con ondas) y tachada
function Phone($g) { $g.FillPath((Brush $white), (RoundRect 40 14 48 100 10)); $g.FillPath((Brush (C 40 60 110)), (RoundRect 47 26 34 70 4)) }
$cv = NewIcon; $g = $cv[1]; Phone $g
$vp = Pen $white 7
$g.DrawLines($vp, (Poly @(26,36, 16,48, 26,60, 16,72, 26,84, 16,96))); $g.DrawLines($vp, (Poly @(102,36, 112,48, 102,60, 112,72, 102,84, 112,96)))
Save $cv[0] $g "icon_vibrate"
$cv = NewIcon; $g = $cv[1]; Phone $g; $g.DrawLine((Pen $red 11), 12, 12, 116, 116); Save $cv[0] $g "icon_vibrate_off"

# Nube con flecha (copia del progreso en la nube)
$cv = NewIcon; $g = $cv[1]
$wb = Brush $white
$g.FillEllipse($wb, 8, 50, 50, 50); $g.FillEllipse($wb, 32, 22, 64, 64); $g.FillEllipse($wb, 70, 46, 50, 50); $g.FillRectangle($wb, 33, 70, 62, 30)
$g.FillPolygon((Brush (C 40 130 240)), (Poly @(64,40, 86,64, 73,64, 73,90, 55,90, 55,64, 42,64)))
Save $cv[0] $g "icon_cloud"

# Restaurar / reintentar (flecha circular)
$cv = NewIcon; $g = $cv[1]
$rp = Pen $white 12; $rp.EndCap = 'ArrowAnchor'
$g.DrawArc($rp, 22, 22, 84, 84, 50, 280)
Save $cv[0] $g "icon_restore"

# Documento (términos)
$cv = NewIcon; $g = $cv[1]
$g.FillPolygon((Brush $white), (Poly @(26,8, 82,8, 104,30, 104,120, 26,120)))
$g.FillPolygon((Brush (C 190 205 230)), (Poly @(82,8, 82,30, 104,30)))
$dp = Pen (C 60 90 150) 7
foreach ($ly in 50, 68, 86) { $g.DrawLine($dp, 40, $ly, 90, $ly) }
$g.DrawLine($dp, 40, 104, 72, 104)
Save $cv[0] $g "icon_doc"

# Escudo con visto (privacidad)
$cv = NewIcon; $g = $cv[1]
$g.FillPolygon((VGrad 0 8 120 (C 120 220 160) (C 30 150 90)), (Poly @(64,8, 112,26, 106,74, 64,120, 22,74, 16,26)))
$g.DrawLines((Pen $white 12), (Poly @(40,64, 58,82, 90,46)))
Save $cv[0] $g "icon_shield"

# Papelera (borrar cuenta)
$cv = NewIcon; $g = $cv[1]
$g.FillPath((Brush $white), (RoundRect 18 24 92 14 5)); $g.FillPath((Brush $white), (RoundRect 48 12 32 16 5))
$g.FillPolygon((Brush $white), (Poly @(26,44, 102,44, 94,118, 34,118)))
$tp = Pen (C 200 60 70) 7
foreach ($tx in 48, 64, 80) { $g.DrawLine($tp, $tx, 58, $tx, 104) }
Save $cv[0] $g "icon_trash"

# Cerrar
$cv = NewIcon; $g = $cv[1]; $cp = Pen $white 16; $g.DrawLine($cp, 32, 32, 96, 96); $g.DrawLine($cp, 96, 32, 32, 96); Save $cv[0] $g "icon_close"

# Bandera (ranking por país)
$cv = NewIcon; $g = $cv[1]
$g.FillPath((Brush $white), (RoundRect 20 8 10 114 4))
$fg = New-Object System.Drawing.Drawing2D.GraphicsPath
$fg.AddBezier((PF 30 14), (PF 60 2), (PF 80 28), (PF 112 16)); $fg.AddLine(112, 16, 112, 66); $fg.AddBezier((PF 112 66), (PF 80 78), (PF 60 52), (PF 30 64)); $fg.CloseFigure()
$g.FillPath((VGrad 0 8 72 (C 255 120 120) (C 220 40 60)), $fg)
Save $cv[0] $g "icon_flag"

# Casa (menú principal)
$cv = NewIcon; $g = $cv[1]
$g.FillPolygon((Brush $white), (Poly @(64,10, 122,60, 106,60, 106,116, 22,116, 22,60, 6,60)))
$g.FillPath((Brush (C 40 90 180)), (RoundRect 52 78 24 38 4))
Save $cv[0] $g "icon_home"

# Reloj (recarga de vidas)
$cv = NewIcon; $g = $cv[1]
$g.FillEllipse((Brush $white), 10, 10, 108, 108); $g.FillEllipse((Brush (C 40 70 140)), 20, 20, 88, 88)
$hp2 = Pen $white 9; $g.DrawLine($hp2, 64, 64, 64, 32); $g.DrawLine($hp2, 64, 64, 88, 78)
Save $cv[0] $g "icon_clock"

# Regalo (ofertas y premios)
$cv = NewIcon; $g = $cv[1]
$g.FillPath((VGrad 0 54 118 (C 255 120 140) (C 210 40 80)), (RoundRect 16 54 96 64 8))
$g.FillPath((VGrad 0 34 60 (C 255 150 165) (C 230 70 100)), (RoundRect 10 34 108 26 8))
$gb = Brush (C 255 215 80)
$g.FillRectangle($gb, 56, 34, 16, 84); $g.FillEllipse($gb, 32, 10, 32, 28); $g.FillEllipse($gb, 64, 10, 32, 28)
Save $cv[0] $g "icon_gift"

# Candado
$cv = NewIcon; $g = $cv[1]
$lk = Pen $white 13
$g.DrawArc($lk, 36, 12, 56, 64, 180, 180); $g.DrawLine($lk, 36, 44, 36, 60); $g.DrawLine($lk, 92, 44, 92, 60)
$g.FillPath((Brush $white), (RoundRect 22 54 84 64 12))
$g.FillEllipse((Brush (C 40 60 110)), 56, 70, 16, 16); $g.FillRectangle((Brush (C 40 60 110)), 60, 80, 8, 20)
Save $cv[0] $g "icon_lock"

# Corona (VIP)
$cv = NewIcon; $g = $cv[1]
$g.FillPolygon((VGrad 0 20 110 (C 255 230 110) (C 225 150 20)), (Poly @(12,40, 40,66, 64,22, 88,66, 116,40, 104,104, 24,104)))
$g.FillPath((Brush (C 225 150 20)), (RoundRect 22 100 84 16 6))
foreach ($pt in @(@(12,40), @(64,22), @(116,40))) { $g.FillEllipse((Brush (C 255 240 170)), ($pt[0] - 8), ($pt[1] - 8), 16, 16) }
Save $cv[0] $g "icon_crown"

# Compartir
$cv = NewIcon; $g = $cv[1]
$shp = Pen $white 9; $g.DrawLine($shp, 34, 64, 94, 30); $g.DrawLine($shp, 34, 64, 94, 98)
foreach ($pt in @(@(34,64), @(94,30), @(94,98))) { $g.FillEllipse((Brush $white), ($pt[0] - 17), ($pt[1] - 17), 34, 34) }
Save $cv[0] $g "icon_share"

# Flecha hacia arriba (mejoras)
$cv = NewIcon; $g = $cv[1]
$g.FillEllipse((VGrad 0 8 120 (C 110 230 140) (C 30 160 80)), 8, 8, 112, 112)
$g.FillPolygon((Brush $white), (Poly @(64,24, 100,62, 77,62, 77,102, 51,102, 51,62, 28,62)))
Save $cv[0] $g "icon_up"

# Pincel (pinturas)
$cv = NewIcon; $g = $cv[1]
$g.TranslateTransform(64, 64); $g.RotateTransform(40)
$g.FillPath((Brush (C 205 145 85)), (RoundRect -8 -4 16 66 6))
$g.FillRectangle((Brush (C 205 210 222)), -11, -22, 22, 20)
$bp = New-Object System.Drawing.Drawing2D.GraphicsPath
$bp.AddBezier((PF -11 -22), (PF -15 -42), (PF -4 -56), (PF 0 -62)); $bp.AddBezier((PF 0 -62), (PF 4 -56), (PF 15 -42), (PF 11 -22)); $bp.CloseFigure()
$g.FillPath((VGrad 0 -62 -22 (C 255 140 170) (C 230 60 120)), $bp)
$g.ResetTransform(); $g.ScaleTransform(2, 2)
Save $cv[0] $g "icon_brush"

# Rayo (equipamiento y mejoras temporales)
$cv = NewIcon; $g = $cv[1]
$g.FillPolygon((VGrad 0 8 120 (C 255 240 120) (C 255 170 20)), (Poly @(76,6, 24,72, 58,72, 46,122, 104,48, 70,48, 84,6)))
Save $cv[0] $g "icon_bolt"

# Pausa
$cv = NewIcon; $g = $cv[1]; $g.FillPath((Brush $white), (RoundRect 30 22 24 84 6)); $g.FillPath((Brush $white), (RoundRect 74 22 24 84 6)); Save $cv[0] $g "icon_pause"

# Información
$cv = NewIcon; $g = $cv[1]
$g.FillEllipse((Brush $white), 10, 10, 108, 108)
$g.FillEllipse((Brush (C 40 90 180)), 55, 26, 18, 18); $g.FillPath((Brush (C 40 90 180)), (RoundRect 56 50 16 52 6))
Save $cv[0] $g "icon_info"

# Visto (reclamado / seleccionado)
$cv = NewIcon; $g = $cv[1]
$g.FillEllipse((VGrad 0 8 120 (C 110 230 140) (C 30 160 80)), 8, 8, 112, 112)
$g.DrawLines((Pen $white 14), (Poly @(36,66, 56,86, 94,44)))
Save $cv[0] $g "icon_check"
