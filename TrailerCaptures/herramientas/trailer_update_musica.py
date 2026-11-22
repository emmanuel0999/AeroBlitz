"""Música + efectos del tráiler vertical "NEW UPDATE" (v1.1.0): 128 bpm, La menor, 22,5 s, con drop en el tiempo 16.
Todo se sintetiza aquí (sin samples ni loops de terceros): música propia, sin derechos ajenos.
Las escenas (ESCENAS, en tiempos) son las mismas que usa el montaje trailer_update_vertical.ps1."""
import os, sys
HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import numpy as np
import trailer_audio as ta
from trailer_audio import SR, add
from promo2_musica import clap, sub_bass, lead
from trailer_v17_musica import limiter

BPM = 128
B = 60 / BPM; S = B / 4
# gancho, jefe, furia, naves, mundos (drop), misiones, cofre, poder, jefe derrotado, ranking, final, fin
ESCENAS = [0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 48]
T = [b * B for b in ESCENAS]
DUR = T[-1]
DROP = T[4]
OUT = r'D:\Unity\AeroBlitz\TrailerCaptures\musica_trailer_update.wav'
rng = np.random.default_rng(128)

CH = [(57, 60, 64), (53, 57, 60), (60, 64, 67), (55, 59, 62)]   # Am - F - C - G
RT = [45, 41, 48, 43]
HOOK = [69, None, 72, 76, None, 74, 72, None, 69, 72, None, 79, 76, None, 74, 72]


# ---------------- efectos ----------------
def laser(dur=0.14):
    t = ta.tt(dur)
    f = 2400 * np.exp(-t * 20) + 260
    ph = 2 * np.pi * np.cumsum(f) / SR
    y = np.sign(np.sin(ph)) * 0.45 + np.sin(ph) * 0.55
    return ta.fft_filter(y, hi=7500) * np.exp(-t / 0.05) * 0.55


def boom(dur=1.0, big=1.0):
    t = ta.tt(dur)
    noise = ta.fft_filter(rng.standard_normal(len(t)), hi=1100)
    thump = np.sin(2 * np.pi * (48 + 70 * np.exp(-t * 9)) * t) * np.exp(-t / 0.28)
    crack = ta.fft_filter(rng.standard_normal(len(t)), lo=2500) * np.exp(-t / 0.03)
    return (noise * np.exp(-t / 0.32) * 0.9 + thump * 1.3 + crack * 0.35) * big


def whoosh(dur=0.32):
    t = ta.tt(dur)
    n = rng.standard_normal(len(t))
    lo = ta.fft_filter(n, lo=400, hi=2200); hi = ta.fft_filter(n, lo=2200, hi=9000)
    k = t / dur
    env = np.sin(np.pi * k) ** 2
    return (lo * (1 - k) + hi * k) * env * 0.8


def tick():
    t = ta.tt(0.05)
    return (np.sin(2 * np.pi * 1900 * t) + 0.4 * np.sin(2 * np.pi * 3800 * t)) * np.exp(-t / 0.012) * 0.6


def ui_hit():
    t = ta.tt(0.22)
    y = np.sin(2 * np.pi * 1320 * t) * (t < 0.07) + np.sin(2 * np.pi * 1760 * t) * (t >= 0.07)
    click = ta.fft_filter(rng.standard_normal(len(t)), lo=3000) * np.exp(-t / 0.006)
    return (y * np.exp(-t / 0.09) * 0.55 + click * 0.5)


def sparkle(start_m=84):
    out = np.zeros(int(0.9 * SR))
    for i, m in enumerate((start_m, start_m + 4, start_m + 7, start_m + 12)):
        add(out, i * 0.06, ta.bell(m, 0.7), gain=0.5)
    return out


# ---------------- música ----------------
def build():
    n = int(DUR * SR)
    drums = np.zeros(n); bass = np.zeros(n); fx = np.zeros(n); send = np.zeros(n); sfx = np.zeros((n, 2))
    leads = np.zeros((n, 2)); arps = np.zeros((n, 2)); pads = np.zeros((n, 2)); pump = np.ones(n)
    build0, end = T[3], T[10]
    pre_end = T[9] + 2 * B

    step = 0; t = 0.0
    while t < DUR - 0.5:
        beat = step / 4; ci = int(beat // 4) % 4; pos = step % 16
        in_build = build0 + 2 * B <= t < DROP or pre_end <= t < end   # sin bombo justo antes del drop y del final
        drop = DROP <= t < pre_end
        outro = t >= end
        if outro:
            t += S; step += 1; continue
        if pos % 4 == 0 and not in_build:
            add(drums, t, ta.kick(), gain=1.05 if drop else 0.95)
            k = int(t * SR); L = min(n - k, int(B * SR))
            pump[k:k + L] = np.minimum(pump[k:k + L], 1 - (0.6 if drop else 0.45) * np.exp(-np.arange(L) / (0.09 * SR)))
        if pos in (4, 12) and not in_build:
            add(drums, t, clap(), gain=0.65); add(send, t, clap(), gain=0.15)
            if drop: add(drums, t, ta.snare(), gain=0.3)
        add(drums, t, ta.hat(open_=(pos % 4 == 2)), gain=(0.34 if pos % 4 == 2 else 0.17) * (0.5 if in_build else 1))
        if not in_build:
            if pos % 2 == 1: add(bass, t, sub_bass(RT[ci], 1.6 * S), gain=0.5)
            if drop and pos % 4 == 0: add(bass, t, sub_bass(RT[ci] - 12, 2.2 * S), gain=0.42)
        chord = CH[ci]
        am = chord[pos % 3] + (12 if (pos // 3) % 2 else 0)
        y = ta.saw(ta.nf(am + 12), 1.4 * S, 12) * 0.6
        tt = ta.tt(1.4 * S)
        y = ta.fft_filter(y, hi=4200 if not drop else 6500) * np.exp(-tt / 0.07)
        add(arps, t, y, pan=(-0.4 if pos % 2 else 0.4), gain=0.085)
        if (T[1] <= t < build0 or drop) and HOOK[pos] is not None:
            l = lead(HOOK[pos] + (12 if drop and pos % 8 == 0 else 0), 1.8 * S)
            add(leads, t, l, gain=0.12 if drop else 0.09); add(send, t, l, gain=0.05)
        t += S; step += 1

    for i in range(len(T) - 2):
        add(pads, T[i], ta.pad_chord(CH[i % 4], T[i + 1] - T[i] + 0.3) * 0.5)

    # Arranque en plena acción: golpe + platillo desde el primer fotograma
    add(fx, 0.0, ta.impact(1.1), gain=0.7); add(fx, 0.0, ta.crash(), gain=0.45)
    # Cortes: barrido justo antes y platillo en el corte
    for c in T[1:10]:
        add(sfx, c - 0.3, whoosh(0.32), gain=0.35)
        add(fx, c, ta.crash(), gain=0.22)
    # Subida al drop: redoble que se acelera y riser
    a = build0 + B; gap = 2 * S
    while a < DROP - 0.02:
        add(drums, a, ta.snare(), gain=0.15 + 0.5 * (a - build0) / (DROP - build0)); a += gap
        gap = max(S / 2, gap * 0.82)
    add(fx, build0, ta.riser(DROP - build0), gain=0.45)
    # DROP
    add(fx, DROP, ta.impact(1.3), gain=0.85); add(fx, DROP, ta.crash(), gain=0.55)
    add(fx, DROP, ta.braam((33, 40, 45), 2.0), gain=0.4)

    # ---- efectos de la acción (coinciden con las escenas del montaje) ----
    # gancho: misiles contra muros y plasma
    for i, x in enumerate((0.15, 0.5, 0.82, 1.2, 1.55)):
        add(sfx, x, laser(), pan=(-0.3 if i % 2 else 0.3), gain=0.32)
    for x in (0.35, 1.05, 1.65):
        add(sfx, x, boom(0.9, 0.8), gain=0.5)
    # jefe entrando: rugido grave
    add(fx, T[1] + 0.05, ta.braam((28, 35, 40), 1.6), gain=0.35)
    # furia: alarma y ráfagas
    for k in range(4):
        add(sfx, T[2] + k * 0.42, laser(0.12), pan=(-0.4 if k % 2 else 0.4), gain=0.28)
    add(sfx, T[2] + 0.05, boom(0.8, 0.7), gain=0.35)
    # naves: un tic por nave (11 naves en 4 tiempos)
    for k in range(11):
        add(sfx, T[3] + k * (4 * B / 11), tick(), pan=(-0.2 if k % 2 else 0.2), gain=0.35)
    # mundos: barrido corto por mundo (10 mundos en 4 tiempos)
    for k in range(10):
        add(sfx, T[4] + k * (4 * B / 10), whoosh(0.16), pan=(-0.3 if k % 2 else 0.3), gain=0.22)
    # misiones: golpes de interfaz y monedas
    for x in (0.25, 0.75, 1.25):
        add(sfx, T[5] + x, ui_hit(), gain=0.45)
    add(sfx, T[5] + 1.3, sparkle(84), gain=0.5)
    # cofre: apertura con destello
    add(sfx, T[6] + 0.3, ui_hit(), gain=0.5)
    add(sfx, T[6] + 0.55, boom(0.6, 0.5), gain=0.3)
    add(sfx, T[6] + 0.6, sparkle(88), gain=0.6)
    # poder: plasma y misiles
    for i, x in enumerate((0.1, 0.45, 0.8, 1.15, 1.5)):
        add(sfx, T[7] + x, laser(), pan=(-0.3 if i % 2 else 0.3), gain=0.3)
    for x in (0.3, 1.0):
        add(sfx, T[7] + x, boom(0.8, 0.7), gain=0.42)
    # jefe derrotado: explosiones en cadena y la grande
    for k, x in enumerate((0.05, 0.3, 0.55, 0.8, 1.05)):
        add(sfx, T[8] + x, boom(0.7, 0.6 + 0.1 * k), pan=(-0.3 if k % 2 else 0.3), gain=0.42)
    add(sfx, T[8] + 1.35, boom(1.4, 1.4), gain=0.65)
    # ranking: subida con destellos
    add(sfx, T[9] + 0.2, sparkle(79), gain=0.45)
    add(fx, pre_end, ta.riser(end - pre_end), gain=0.4)
    # FINAL: golpe fuerte en el logo y acorde que suena hasta el final
    add(fx, end, ta.impact(1.4), gain=0.9); add(fx, end, ta.crash(), gain=0.55)
    add(fx, end, boom(1.6, 1.0), gain=0.45)
    add(pads, end, ta.pad_chord((45, 52, 57, 60, 64), DUR - end) * 0.95)
    for i, m in enumerate((69, 72, 76, 81)):
        add(send, end + 0.05 + i * 0.09, ta.bell(m + 12, 2.0), gain=0.13)
    add(fx, end + 6 * B, ta.impact(0.8), gain=0.5)     # segundo golpe con "NEW UPDATE OUT NOW"

    arps *= pump[:, None]; pads *= pump[:, None]
    mono = drums + bass + fx
    music = np.stack([mono, mono], 1) * 0.8 + leads + arps + pads + sfx
    music += ta.reverb(send + arps.mean(1) * 0.3 + pads.mean(1) * 0.2, secs=1.3, decay=0.4) * 0.3
    fade = np.ones(n); fl = int(0.8 * SR); fade[-fl:] = np.linspace(1, 0, fl) ** 1.5
    return music * fade[:, None]


if __name__ == '__main__':
    m = build()
    m = np.stack([ta.fft_filter(m[:, c], lo=30) for c in range(2)], 1)
    m = m / np.abs(m).max() * 1.6
    out = limiter(m)
    ta.write_wav(OUT, out)
    print(f'{OUT}  {DUR:.2f} s  pico {np.abs(out).max():.3f}')
    print('escenas (s):', ', '.join(f'{c:.2f}' for c in T))
