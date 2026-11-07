# Video promocional de Aero Blitz

**Video final:** `trailer_aero_blitz.mp4` (1920x1080, 29,4 s, en inglés, con voz y música).
**Miniatura para YouTube:** `miniatura_youtube.png` (1280x720).

## Contenido
| Segundos | Escena (clip vertical) | Rótulo | Voz |
|---|---|---|---|
| 0–3 | Nivel 70 (clip_182212, 1–4 s) | THINK YOU CAN BEAT ALL 70 LEVELS? | Think you can beat all seventy levels? |
| 3–7 | Nivel 38 (clip_182638, 2,3–6,3 s) | DODGE. CHARGE. BLAST! | I don't think so. / Dodge, charge, blast! |
| 7–11,5 | Misiles, nivel 30 (clip_181715, 9–13,5 s) | MISSILES THAT SMASH ANYTHING | Missiles that smash through anything. |
| 11,5–17,5 | Niveles 10, 20 y 70 | 70 LEVELS · 10 WORLDS | Ten worlds, each one deadlier than the last. |
| 17,5–21,5 | Imán y sobrecarga, nivel 20 (clip_184042) | GRAB COINS & POWER-UPS | Grab coins, power up... you will need it. |
| 21,5–24,9 | Derrota, nivel 38 (clip_182625, 0–3,4 s) | IT ONLY GETS HARDER | Careful... / Told you. |
| 24,9–29,4 | final_reto_horizontal.png | BET YOU CAN'T BEAT THEM ALL. | Bet you can't beat them all. / Aero Blitz, free on Google Play. |

- **Música:** compuesta por código para este video (sin samples ni grabaciones de terceros).
- **Voz:** "Microsoft Mark", la voz de Windows, procesada más grave, con un poco de eco y bajada de volumen.
- Archivos de audio: `audio_trailer.wav` (mezcla), `musica_sin_voz.wav`, `voz_sola.wav`.

## Cómo volver a montarlo (si hay cambios)
Todo está en `herramientas/`:
- `rotulos.ps1`: genera los rótulos (`rotulos/`) y las pantallas finales.
- `trailer_audio.py`: genera la voz y la música. Las frases, los tiempos y los volúmenes (`VOICE_GAIN`, `MUSIC_GAIN`) están al principio y al final del archivo.
- `framegrab/Assemble.exe guion_trailer.txt trailer_aero_blitz.mp4`: monta el video según `guion_trailer.txt`. Hay que ejecutarlo desde esta carpeta.
- `sheet.ps1`: hace una hoja de fotogramas para revisar un clip.

Para grabar tomas nuevas: en Unity, menú **Aero Blitz > Grabar tráiler** (solo en el Editor, no va en el juego publicado).

## Subir a YouTube y ponerlo en Google Play
1. En YouTube: **Crear > Subir vídeo** y elige `trailer_aero_blitz.mp4`.
2. Pon título, descripción y miniatura. Audiencia: lo mismo que en Público objetivo de Play Console. Sin restricción de edad.
3. Visibilidad **Pública** u **Oculta** (no Privada). Copia el enlace `https://www.youtube.com/watch?v=...`.
4. Play Console > **Presencia en Play Store > Ficha principal** > campo **Video de YouTube**: pega el enlace y guarda. Pon el mismo enlace en las traducciones (español y francés).
5. Envía el cambio desde **Resumen de publicación**.
