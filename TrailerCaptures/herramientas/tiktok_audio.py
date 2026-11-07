"""Audio de la serie de TikTok: misma voz de comandante y mismo estilo de música que el tráiler."""
import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import trailer_audio as ta
from trailer_audio import SR, add, kick, snare, hat, crash, impact, braam, riser, pad_chord, bass_note, pluck, bell, reverb, duck_gain, write_wav

OUT = r'D:\Unity\AeroBlitz\TikTok\fuentes'
VOICE_GAIN, MUSIC_GAIN = 0.39, 0.39  # mismos niveles que aprobó el usuario en el tráiler

# Cada video: duración, inicio de la pantalla final, cortes, (inicio de tensión, explosión) y frases (inicio, texto, velocidad, tono)
VIDEOS = {
    # Anuncio 2: la computadora de la nave avisa al piloto (guion distinto a todos los anteriores)
    'promo2': dict(dur=16.0, end=11.8, cuts=[3.0, 5.2], tension=(8.4, 10.77), lines=[
        (0.20, 'Your mission: survive seventy levels.', '+12%', '-6%'),
        (3.10, 'Weapon charged. Fire!', '+5%', '-6%'),
        (5.40, 'Homing missiles locked.', '+5%', '-6%'),
        (7.45, 'Target destroyed.', '+10%', '-6%'),
        (9.00, 'Hull critical!', '+0%', '-6%'),
        (11.00, 'Ship destroyed.', '-5%', '-8%'),
        (12.25, 'Ready to try again?', '+0%', '-6%'),
        (13.70, 'Get Aero Blitz on Google Play.', '+5%', '-6%')]),
    # Anuncio para promocionar en TikTok (colores nuevos, partida a pantalla completa)
    'promo': dict(dur=16.0, end=11.8, cuts=[3.0, 5.2], tension=(8.4, 10.77), lines=[
        (0.25, 'Think you can beat level seventy?', '+0%', '-10%'),
        (3.05, 'Charge, fire, survive!', '+20%', '-10%'),
        (5.40, 'Missiles that smash through anything.', '+5%', '-10%'),
        (8.60, 'Careful...', '-12%', '-12%'),
        (11.00, 'Told you.', '-8%', '-14%'),
        (12.05, "Bet you can't beat them all.", '+0%', '-10%'),
        (13.55, 'Aero Blitz, free on Google Play.', '+8%', '-8%')]),
    'v1': dict(dur=11.8, end=8.5, cuts=[], tension=None, lines=[
        (0.25, 'This is level seventy.', '+0%', '-12%'),
        (2.30, 'Walls, mines, lasers.', '+0%', '-10%'),
        (5.00, 'Still think you can make it?', '+0%', '-10%'),
        (8.65, 'Aero Blitz. Search it on Google Play.', '+5%', '-8%')]),
    'v2': dict(dur=11.3, end=8.0, cuts=[5.0], tension=None, lines=[
        (0.25, 'Walls in your way?', '+0%', '-10%'),
        (1.55, 'Fire the missiles.', '+5%', '-10%'),
        (3.35, 'Gone.', '-10%', '-14%'),
        (5.20, 'Again.', '-10%', '-14%'),
        (8.15, 'Aero Blitz. Search it on Google Play.', '+5%', '-8%')]),
    'v3': dict(dur=9.9, end=6.4, cuts=[3.0], tension=(3.0, 5.37), lines=[
        (0.25, 'Level thirty-eight. Easy, right?', '+0%', '-10%'),
        (3.20, 'Careful...', '-12%', '-12%'),
        (5.60, 'Told you.', '-8%', '-14%'),
        (6.65, "Bet you can't beat it.", '+0%', '-10%'),
        (8.25, 'Aero Blitz.', '+0%', '-8%')]),
    'v4': dict(dur=13.2, end=9.6, cuts=[2.4, 4.8, 7.2], tension=None, lines=[
        (0.25, 'Which world is the hardest?', '+0%', '-10%'),
        (4.95, 'Abandoned Station?', '+0%', '-10%'),
        (7.35, 'Or the Galactic Core?', '+0%', '-10%'),
        (9.80, 'Comment below. Aero Blitz, on Google Play.', '+8%', '-8%')]),
    'v5': dict(dur=10.6, end=8.0, cuts=[2.0, 4.0, 6.0], tension=None, lines=[
        (0.25, 'How far can you get?', '+0%', '-10%'),
        (2.20, 'Level thirty?', '+0%', '-10%'),
        (4.20, 'Thirty-eight?', '+0%', '-10%'),
        (6.20, 'Seventy?', '-5%', '-12%'),
        (8.20, 'Comment your best level.', '+0%', '-10%')]),
}

CHORDS = [(50, 53, 57), (46, 50, 53), (53, 57, 60), (48, 52, 55)]
ROOTS = [38, 34, 41, 36]
BEAT = 0.5


def build_voice(key, lines, n):
    dry = np.zeros(n); log = []
    for i, (start, text, rate, pitch) in enumerate(lines):
        y = ta.make_line(f'{key}_{i}', text, rate, pitch)
        add(dry, start, y)
        log.append((start, start + len(y) / SR, text))
    for k, (a, b, text) in enumerate(log):
        nxt = log[k + 1][0] if k + 1 < len(log) else n / SR
        warn = '  <-- SE SOLAPA' if b > nxt - 0.05 else ''
        print(f'    {a:5.2f}-{b:5.2f}s  {text}{warn}')
    echo = dry.copy()
    for d, g in ((0.12, 0.14), (0.25, 0.06)):
        k = int(d * SR); echo[k:] += dry[:-k] * g
    v = np.stack([echo, echo], 1) + reverb(dry, secs=0.9, decay=0.18, seed=5) * 0.07
    return v / np.abs(v).max() * 0.9, dry


def build_music(dur, end, cuts, tension):
    n = int(dur * SR)
    drums = np.zeros(n); bass = np.zeros(n); fx = np.zeros(n)
    pads = np.zeros((n, 2)); arps = np.zeros((n, 2)); send = np.zeros(n)
    stop = tension[0] if tension else end
    # Arranque con golpe: en TikTok el primer segundo decide si la gente se queda
    add(fx, 0.0, impact(0.8), gain=0.5); add(fx, 0.0, crash(), gain=0.3)
    t = 0.0; beat = 0
    while t < stop - 1e-6:
        ci = int(t // 2.0) % 4
        if beat % 4 == 0: add(pads, t, pad_chord(CHORDS[ci], 2.05))
        add(drums, t, kick(), gain=0.9)
        if beat % 2 == 1: add(drums, t, snare(), gain=0.55); add(send, t, snare(), gain=0.25)
        dense = t >= stop * 0.5
        for k in range(4 if dense else 2):
            add(drums, t + k * BEAT / (4 if dense else 2), hat(open_=(k == 2 and not dense)), gain=0.5)
        for k in range(2):
            add(bass, t + k * BEAT / 2, bass_note(ROOTS[ci] + (12 if k == 1 else 0), BEAT / 2), gain=0.45)
        if t >= 1.0:
            notes = list(CHORDS[ci]) + [CHORDS[ci][0] + 12]
            for k in range(4):
                add(arps, t + k * BEAT / 4, pluck(notes[(beat * 4 + k) % 4] + 12, 0.2), pan=(-0.45 if k % 2 == 0 else 0.45), gain=0.05)
        t += BEAT; beat += 1
    for c in cuts:
        if c < stop:
            for k in range(4): add(drums, c - BEAT + k * BEAT / 4, snare(), gain=0.25 + 0.1 * k)
        add(fx, c, impact(0.6), gain=0.4); add(fx, c, crash(), gain=0.3)
    if tension:
        a, boom = tension
        beats = np.linspace(a, boom - 0.15, 6) ** 1.0
        for b in beats: add(drums, b, kick(), gain=0.8)
        add(pads, a, pad_chord((38, 41, 45), boom - a) * 0.9)
        add(fx, a + 0.3, riser(boom - a - 0.3), gain=0.45)
        add(fx, boom, impact(1.3), gain=0.9); add(fx, boom, crash(), gain=0.5)
    # Pantalla final
    add(fx, end, braam([26, 38, 45, 50], dur=dur - end + 0.5), gain=0.55)
    add(pads, end, pad_chord((50, 53, 57, 62), dur - end) * 1.1)
    add(fx, end, crash(), gain=0.3)
    add(send, end + 0.15, bell(74), gain=0.25); add(send, end + 0.3, bell(81), gain=0.15)
    music = np.stack([drums + bass + fx, drums + bass + fx], 1) * 0.7 + pads + arps
    music += reverb(send + arps.mean(1) * 0.6 + pads.mean(1) * 0.3, secs=2.2, decay=0.6) * 0.5
    fade = np.ones(n); fl = int(1.2 * SR); fade[-fl:] = np.linspace(1, 0, fl) ** 1.5
    return music * fade[:, None] / np.abs(music).max()


if __name__ == '__main__':
    os.makedirs(OUT, exist_ok=True)
    only = sys.argv[1:] or list(VIDEOS)
    limit = lambda m: np.tanh(1.3 * m) / np.tanh(1.3)
    for key in only:
        cfg = VIDEOS[key]; n = int(cfg['dur'] * SR)
        print(f'{key}:')
        voice, dry = build_voice(key, cfg['lines'], n)
        music = build_music(cfg['dur'], cfg['end'], cfg['cuts'], cfg['tension'])
        g = duck_gain(dry)
        ref = limit(music * 0.55 * g[:, None] + voice * VOICE_GAIN)
        mix = limit(music * MUSIC_GAIN * g[:, None] + voice * VOICE_GAIN) * (0.95 / np.abs(ref).max())
        write_wav(os.path.join(OUT, f'audio_{key}.wav'), mix)
