"""Música del tráiler 1.0.3: electrónica rápida (140 bpm, Mi menor), compuesta por código.
No usa samples, loops ni grabaciones de terceros: cada sonido se sintetiza aquí, así que es música propia y sin derechos de autor ajenos.
Los cortes del video caen en tiempos de la música (lista CUTS_BEATS, compartida con el montaje)."""
import os, sys
HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import numpy as np
from numpy.lib.stride_tricks import sliding_window_view
import trailer_audio as ta
from trailer_audio import SR, add
from promo2_musica import clap, sub_bass, lead

BPM = 140
B = 60 / BPM; S = B / 4
# Escenas en tiempos (beats): gancho, juego, misiles, plasma, naves, mundos, tormenta, misiones, cofres, ranking, final, fin
CUTS_BEATS = [0, 8, 14, 20, 24, 31, 38, 44, 48, 53, 57, 64]
CUTS = [b * B for b in CUTS_BEATS]
DUR = CUTS[-1]
OUT = r'D:\Unity\AeroBlitz\TrailerCaptures\musica_trailer_v17.wav'
rng = np.random.default_rng(140)

CH = [(64, 67, 71), (60, 64, 67), (62, 67, 71), (62, 66, 69)]   # Em - C - G - D
RT = [40, 36, 43, 38]
HOOK = [76, None, 76, 79, None, 78, 76, 74, 76, None, 71, 74, None, 76, 79, 81]


def arp_note(m, dur):
    t = ta.tt(dur)
    y = ta.saw(ta.nf(m), dur, 12) * 0.6 + np.sin(2 * np.pi * ta.nf(m) * t)
    return ta.fft_filter(y, hi=5000) * np.exp(-t / 0.07) * ta.adsr(len(t), 0.002, 0.02)


def build():
    n = int(DUR * SR)
    drums = np.zeros(n); bass = np.zeros(n); fx = np.zeros(n); send = np.zeros(n)
    leads = np.zeros((n, 2)); arps = np.zeros((n, 2)); pads = np.zeros((n, 2)); pump = np.ones(n)
    storm0, storm1 = CUTS[6], CUTS[7]
    end = CUTS[10]

    step = 0; t = 0.0
    while t < DUR - 0.6:
        beat = step / 4; ci = int(beat // 4) % 4; pos = step % 16
        in_build = CUTS[9] + 2 * B <= t < end          # subida antes del final: sin bombo
        heavy = storm0 <= t < storm1
        light = CUTS[7] <= t < CUTS[9]
        if pos % 4 == 0 and not in_build:
            add(drums, t, ta.kick(), gain=1.0 if not light else 0.85)
            k = int(t * SR); L = min(n - k, int(B * SR))
            pump[k:k + L] = np.minimum(pump[k:k + L], 1 - 0.55 * np.exp(-np.arange(L) / (0.09 * SR)))
        if pos in (4, 12) and not in_build:
            add(drums, t, clap(), gain=0.6); add(send, t, clap(), gain=0.15)
        if heavy and pos in (6, 14): add(drums, t, ta.snare(), gain=0.35)
        add(drums, t, ta.hat(open_=(pos % 4 == 2)), gain=(0.32 if pos % 4 == 2 else 0.16) * (0.6 if in_build else 1))
        if not in_build:
            if pos % 2 == 1: add(bass, t, sub_bass(RT[ci], 1.6 * S), gain=0.46)          # bajo rodante a contratiempo
            if heavy and pos % 4 == 0: add(bass, t, sub_bass(RT[ci] - 12, 2 * S), gain=0.35)
        chord = CH[ci]
        am = chord[pos % 3] + (12 if (pos // 3) % 2 else 0)
        add(arps, t, arp_note(am + 12, 1.5 * S), pan=(-0.35 if pos % 2 else 0.35), gain=0.09 if not light else 0.07)
        if CUTS[1] <= t < storm0 and not (CUTS[4] <= t < CUTS[5]) and HOOK[pos] is not None:
            l = lead(HOOK[pos], 1.8 * S)
            add(leads, t, l, gain=0.10); add(send, t, l, gain=0.05)
        t += S; step += 1

    for i in range(len(CUTS) - 1):
        add(pads, CUTS[i], ta.pad_chord(CH[i % 4], CUTS[i + 1] - CUTS[i] + 0.4) * 0.6)
    # Golpe al empezar: el video arranca en plena acción
    add(fx, 0.0, ta.impact(1.0), gain=0.6); add(fx, 0.0, ta.crash(), gain=0.4)
    for c in CUTS[1:10]:
        add(fx, c - 0.45, ta.riser(0.45), gain=0.18); add(fx, c, ta.crash(), gain=0.25)
    add(fx, storm0, ta.impact(1.0), gain=0.5)
    add(fx, storm0, ta.braam((40, 47, 52), 2.4), gain=0.35)
    for c in CUTS[7:10]:                                                   # misiones, cofres, ranking: destellos
        for i, m in enumerate((83, 88)):
            add(send, c + i * 0.07, ta.bell(m, 1.2), gain=0.08)

    # Subida antes del final: redoble que se acelera hasta el golpe de la pantalla final
    a = CUTS[9] + 2 * B; gap = 2 * S
    while a < end - 0.03:
        add(drums, a, ta.snare(), gain=0.2 + 0.4 * (a - CUTS[9]) / (end - CUTS[9])); a += gap
        gap = max(S / 2, gap * 0.85)
    add(fx, CUTS[9] + 2 * B, ta.riser(end - CUTS[9] - 2 * B), gain=0.4)
    add(fx, end, ta.impact(1.2), gain=0.75); add(fx, end, ta.crash(), gain=0.5)
    add(pads, end, ta.pad_chord((52, 59, 64, 67, 71), DUR - end) * 0.9)
    for i, m in enumerate((64, 67, 71, 76)):
        add(send, end + 0.05 + i * 0.08, ta.bell(m + 12, 2.0), gain=0.12)

    arps *= pump[:, None]; pads *= pump[:, None]
    mono = drums + bass + fx
    music = np.stack([mono, mono], 1) * 0.78 + leads + arps + pads
    music += ta.reverb(send + arps.mean(1) * 0.3 + pads.mean(1) * 0.2, secs=1.4, decay=0.4) * 0.3
    fade = np.ones(n); fl = int(0.9 * SR); fade[-fl:] = np.linspace(1, 0, fl) ** 1.6
    return music * fade[:, None]


def limiter(x, ceil=0.891, look=0.004, rel=0.08):
    req = np.minimum(1.0, ceil / np.maximum(np.abs(x).max(1), 1e-9))
    L = int(look * SR)
    req = sliding_window_view(np.concatenate([req, np.ones(L)]), L + 1).min(1)[:len(req)]
    a = np.exp(-1 / (rel * SR)); g = np.empty_like(req); cur = 1.0
    for i, r in enumerate(req):
        cur = r if r < cur else r + (cur - r) * a
        g[i] = cur
    g = np.minimum(np.convolve(g, np.ones(L) / L, 'same'), req)
    return np.clip(x * g[:, None], -ceil, ceil)


if __name__ == '__main__':
    m = build()
    m = np.stack([ta.fft_filter(m[:, c], lo=30) for c in range(2)], 1)
    m = m / np.abs(m).max() * 1.6           # un poco de empuje para que suene fuerte y compacto
    out = limiter(m)
    ta.write_wav(OUT, out)
    print(f'{OUT}  {DUR:.2f} s  pico {np.abs(out).max():.3f}')
    print('cortes (s):', ', '.join(f'{c:.2f}' for c in CUTS))
