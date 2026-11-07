"""Subtítulos del tráiler (inglés, español y francés) con los tiempos reales de la voz."""
import os

OUT = r'D:\Unity\AeroBlitz\TrailerCaptures'
# (inicio, fin) de cada frase según trailer_audio.py
TIMES = [(0.30, 2.35), (3.15, 4.26), (4.40, 6.37), (7.25, 8.96), (11.70, 14.26),
         (17.70, 20.18), (21.65, 22.17), (24.12, 24.86), (25.05, 26.32), (26.75, 28.71)]
TEXT = {
    'en': ['Think you can beat all 70 levels?', "I don't think so.", 'Dodge, charge, blast!',
           'Missiles that smash through anything.', 'Ten worlds, each one deadlier than the last.',
           'Grab coins, power up... you will need it.', 'Careful...', 'Told you.',
           "Bet you can't beat them all.", 'Aero Blitz, free on Google Play.'],
    'es': ['¿Crees que puedes superar los 70 niveles?', 'No lo creo.', '¡Esquiva, carga, dispara!',
           'Misiles que lo destrozan todo.', 'Diez mundos, cada uno más letal que el anterior.',
           'Recoge monedas, mejora... lo vas a necesitar.', 'Cuidado...', 'Te lo dije.',
           'Apuesto a que no los superas todos.', 'Aero Blitz, gratis en Google Play.'],
    'fr': ['Tu crois pouvoir battre les 70 niveaux ?', 'Je ne crois pas.', 'Esquive, charge, tire !',
           'Des missiles qui pulvérisent tout.', 'Dix mondes, chacun plus mortel que le précédent.',
           'Ramasse des pièces, améliore-toi... tu en auras besoin.', 'Attention...', 'Je te l\'avais dit.',
           'Parie que tu ne les battras pas tous.', 'Aero Blitz, gratuit sur Google Play.'],
}


def ts(t):
    ms = int(round(t * 1000))
    return f'{ms // 3600000:02}:{ms // 60000 % 60:02}:{ms // 1000 % 60:02},{ms % 1000:03}'


for lang, lines in TEXT.items():
    rows = []
    for i, ((a, b), text) in enumerate(zip(TIMES, lines)):
        nxt = TIMES[i + 1][0] if i + 1 < len(TIMES) else 29.4
        end = min(b + 0.4, nxt - 0.05)  # se queda un poco más en pantalla sin pisar la siguiente
        rows.append(f'{i + 1}\n{ts(a)} --> {ts(end)}\n{text}\n')
    path = os.path.join(OUT, f'subtitulos_{lang}.srt')
    with open(path, 'w', encoding='utf-8') as f:
        f.write('\n'.join(rows))
    print(path)
