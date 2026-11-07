"""Audio del tráiler: voz de "comandante" que reta al jugador + música de acción sincronizada con los cortes."""
import os, subprocess, wave
import numpy as np

SR = 44100
DUR = 29.4
N = int(DUR * SR)
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = r'D:\Unity\AeroBlitz\TrailerCaptures'
SPEAK = os.path.join(HERE, 'framegrab', 'Speak.exe')
VOICE = 'Mark'
PITCH_FACTOR = 0.93  # <1: voz más lenta y grave
rng = np.random.default_rng(7)

# Cortes del video (segundos) y momento de la explosión de la nave
CUTS = [3.0, 7.0, 11.5, 17.5, 21.5]
BOOM = 23.87
END_CARD = 24.9

# (inicio, texto SSML, velocidad, tono)
LINES = [
    (0.30, 'Think you can beat all seventy levels?', '+0%', '-10%'),
    (3.15, "I don't think so.", '-12%', '-14%'),
    (4.40, 'Dodge, charge, blast!', '+8%', '-10%'),
    (7.25, 'Missiles that smash through anything.', '+5%', '-10%'),
    (11.70, 'Ten worlds, each one deadlier than the last.', '+8%', '-10%'),
    (17.70, 'Grab coins, power up... you will need it.', '+8%', '-10%'),
    (21.65, 'Careful...', '-12%', '-12%'),
    (24.12, 'Told you.', '-8%', '-14%'),
    (25.05, "Bet you can't beat them all.", '+0%', '-10%'),
    (26.75, 'Aero Blitz, free on Google Play.', '+8%', '-8%'),
]


# ---------------- utilidades ----------------
def write_wav(path, x):
    y = (np.clip(x, -1, 1) * 32767).astype('<i2')
    with wave.open(path, 'wb') as w:
        w.setnchannels(1 if y.ndim == 1 else y.shape[1]); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes(y.tobytes())


def read_wav(path):
    with wave.open(path) as w:
        sr, ch = w.getframerate(), w.getnchannels()
        d = np.frombuffer(w.readframes(w.getnframes()), '<i2').astype(float) / 32768
    return (d.reshape(-1, ch).mean(1) if ch > 1 else d), sr


def fft_filter(x, lo=None, hi=None, order=2):
    n = len(x); m = 1 << int(np.ceil(np.log2(n + 1)))
    f = np.fft.rfftfreq(m, 1 / SR); g = np.ones_like(f)
    if lo: g /= np.sqrt(1 + (lo / np.maximum(f, 1e-3)) ** (2 * order))
    if hi: g /= np.sqrt(1 + (f / hi) ** (2 * order))
    return np.fft.irfft(np.fft.rfft(x, m) * g, m)[:n]


def conv(x, ir):
    m = 1 << int(np.ceil(np.log2(len(x) + len(ir))))
    return np.fft.irfft(np.fft.rfft(x, m) * np.fft.rfft(ir, m), m)[:len(x)]


def reverb(x, secs=1.6, decay=0.45, seed=1):
    t = np.arange(int(secs * SR)) / SR
    r = np.random.default_rng(seed); out = []
    for _ in range(2):
        ir = r.standard_normal(len(t)) * np.exp(-t / decay)
        ir[:441] *= np.linspace(0, 1, 441)
        ir = fft_filter(ir, hi=5000, order=1)
        out.append(conv(x, ir / np.sqrt(np.sum(ir ** 2))))
    return np.stack(out, 1)


def add(buf, start, sig, pan=0.0, gain=1.0):
    i = int(round(start * SR))
    if i >= len(buf) or i + len(sig) <= 0: return
    if i < 0: sig, i = sig[-i:], 0
    sig = sig[:len(buf) - i]
    a = (pan + 1) * np.pi / 4
    if buf.ndim == 1 or sig.ndim == 2: buf[i:i + len(sig)] += sig * gain
    else:
        buf[i:i + len(sig), 0] += sig * gain * np.cos(a)
        buf[i:i + len(sig), 1] += sig * gain * np.sin(a)


def nf(m): return 440.0 * 2 ** ((m - 69) / 12)
def tt(d): return np.arange(int(d * SR)) / SR


def saw(freq, dur, harm=10):
    t = tt(dur); y = np.zeros_like(t)
    for k in range(1, harm + 1):
        if k * freq > SR / 2.2: break
        y += np.sin(2 * np.pi * k * freq * t) / k
    return y


def adsr(n, a=0.01, r=0.1, sustain=1.0):
    e = np.full(n, sustain); ai = min(n, int(a * SR)); ri = min(n - ai, int(r * SR))
    e[:ai] = np.linspace(0, 1, ai) * sustain if ai else e[:ai]
    if ri: e[n - ri:] *= np.linspace(1, 0, ri)
    return e


# ---------------- voz ----------------
def make_line(i, text, rate, pitch):
    ssml = ('<speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xml:lang="en-US">'
            f'<prosody rate="{rate}" pitch="{pitch}">{text}</prosody></speak>')
    sp = os.path.join(HERE, 'voz', f'l{i}.ssml'); wp = os.path.join(HERE, 'voz', f'l{i}.wav')
    with open(sp, 'w', encoding='utf-8') as f: f.write(ssml)
    subprocess.run([SPEAK, VOICE, sp, wp], check=True, capture_output=True)
    x, sr = read_wav(wp)
    idx = np.nonzero(np.abs(x) > 0.02 * np.abs(x).max())[0]
    x = x[max(0, idx[0] - 200): idx[-1] + 800]
    # Algo más lento y grave: voz de comandante
    F = PITCH_FACTOR
    pos = np.arange(int(len(x) * SR / sr / F)) * (sr * F / SR)
    y = np.interp(pos, np.arange(len(x)), x)
    y = fft_filter(y, lo=110, hi=7000)
    y = y / np.abs(y).max()
    y = y + 0.35 * fft_filter(y, lo=2000, hi=5000)  # presencia: se entiende mejor sin subir volumen
    y = np.tanh(1.15 * y / np.abs(y).max()) / np.tanh(1.15)  # saturación suave para que suene nítida
    return y


def build_voice():
    os.makedirs(os.path.join(HERE, 'voz'), exist_ok=True)
    dry = np.zeros(N); log = []
    for i, (start, text, rate, pitch) in enumerate(LINES):
        y = make_line(i, text, rate, pitch)
        add(dry, start, y)
        log.append((start, start + len(y) / SR, text))
    for k, (a, b, text) in enumerate(log):
        nxt = log[k + 1][0] if k + 1 < len(log) else DUR
        warn = '  <-- SE SOLAPA' if b > nxt - 0.05 else ''
        print(f'  voz {a:5.2f}-{b:5.2f}s  {text[:40]}{warn}')
    # Eco corto y algo de sala para que suene "por la radio de la nave"
    echo = dry.copy()
    for d, g in ((0.12, 0.14), (0.25, 0.06)):
        k = int(d * SR); echo[k:] += dry[:-k] * g
    wet = reverb(dry, secs=0.9, decay=0.18, seed=5) * 0.07
    v = np.stack([echo, echo], 1) + wet
    return v / np.abs(v).max() * 0.9, dry


# ---------------- música ----------------
def kick():
    t = tt(0.45); f = 45 + 110 * np.exp(-t / 0.03)
    y = np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-t / 0.22)
    y += rng.standard_normal(len(t)) * np.exp(-t / 0.003) * 0.3
    return np.tanh(2.2 * y)


def snare():
    t = tt(0.3); nz = np.diff(rng.standard_normal(len(t) + 1))
    return nz * np.exp(-t / 0.11) * 0.55 + np.sin(2 * np.pi * 185 * t) * np.exp(-t / 0.07) * 0.6


def hat(open_=False):
    t = tt(0.25 if open_ else 0.06); nz = np.diff(np.diff(rng.standard_normal(len(t) + 2)))
    return nz * np.exp(-t / (0.09 if open_ else 0.022)) * 0.35


def crash():
    t = tt(2.0); nz = np.diff(rng.standard_normal(len(t) + 1))
    return fft_filter(nz, lo=3000) * np.exp(-t / 0.7) * 0.6


def impact(big=1.0):
    t = tt(2.5); f = 38 + 60 * np.exp(-t / 0.12)
    y = np.sin(2 * np.pi * np.cumsum(f) / SR) * np.exp(-t / (0.9 * big))
    y += fft_filter(rng.standard_normal(len(t)), hi=400) * np.exp(-t / 0.35) * 2.5
    return np.tanh(2.5 * y) * big


def braam(notes, dur=2.6):
    y = sum(saw(nf(m), dur, 24) for m in notes)
    y = fft_filter(y, hi=900)
    t = tt(dur); env = np.minimum(1, t / 0.04) * np.exp(-t / 1.4)
    return np.tanh(1.8 * y * env)


def riser(dur):
    t = tt(dur); ramp = (t / dur) ** 2
    nz = fft_filter(rng.standard_normal(len(t)), lo=800, hi=9000) * ramp
    glide = np.sin(2 * np.pi * np.cumsum(300 + 1500 * ramp) / SR) * ramp * 0.25
    return (nz * 0.5 + glide)


def pad_chord(notes, dur):
    out = np.zeros((int(dur * SR), 2))
    for m in notes:
        for det, pan in ((-0.004, -0.6), (0.004, 0.6)):
            v = saw(nf(m) * (1 + det), dur, 7) * adsr(int(dur * SR), 0.25, 0.4)
            add(out, 0, v, pan, 0.05)
    return out


def bass_note(m, dur):
    t = tt(dur)
    y = saw(nf(m), dur, 6) * np.exp(-t / 0.18) + np.sin(2 * np.pi * nf(m) * t) * 0.8
    return np.tanh(1.5 * y) * adsr(len(t), 0.004, 0.03)


def pluck(m, dur):
    t = tt(dur); y = np.zeros_like(t)
    for k in (1, 3, 5, 7):
        y += np.sin(2 * np.pi * k * nf(m) * t) / k
    return y * np.exp(-t / 0.09)


def bell(m, dur=3.0):
    t = tt(dur); f = nf(m)
    return (np.sin(2 * np.pi * f * t) * np.exp(-t / 1.2) + 0.5 * np.sin(2 * np.pi * 2.76 * f * t) * np.exp(-t / 0.5)
            + 0.25 * np.sin(2 * np.pi * 5.4 * f * t) * np.exp(-t / 0.25))


def build_music():
    drums = np.zeros(N); bass = np.zeros(N); fx = np.zeros(N)
    pads = np.zeros((N, 2)); arps = np.zeros((N, 2)); send = np.zeros(N)
    BEAT = 0.5  # 120 bpm
    # Re menor épico: Dm - Bb - F - C (un acorde cada compás de 2 s desde el primer corte)
    CHORDS = [(50, 53, 57), (46, 50, 53), (53, 57, 60), (48, 52, 55)]
    ROOTS = [38, 34, 41, 36]

    # Intro: golpe grave, zumbido y subida hacia el primer corte
    add(fx, 0.02, braam([26, 38, 45]), gain=0.55)
    add(pads, 0.0, pad_chord((38, 45, 50), 3.0) * 0.8)
    add(fx, 1.2, riser(1.8), gain=0.35)

    # Groove de 3.0 a 21.5
    t = 3.0; beat = 0
    while t < 21.5 - 1e-6:
        bar = int((t - 3.0) // 2.0); ci = bar % 4
        if beat % 4 == 0 and abs((t - 3.0) % 2.0) < 1e-6:
            add(pads, t, pad_chord(CHORDS[ci], 2.05))
        add(drums, t, kick(), gain=0.9)
        if beat % 2 == 1: add(drums, t, snare(), gain=0.55); add(send, t, snare(), gain=0.25)
        dense = t >= 11.5
        for k in range(4 if dense else 2):
            add(drums, t + k * BEAT / (4 if dense else 2), hat(open_=(k == 2 and not dense)), gain=0.5)
        for k in range(2):
            add(bass, t + k * BEAT / 2, bass_note(ROOTS[ci] + (12 if (k == 1 and t >= 7.0) else 0), BEAT / 2), gain=0.45)
        if t >= 7.0:
            notes = list(CHORDS[ci]) + [CHORDS[ci][0] + 12]
            for k in range(4):
                m = notes[(beat * 4 + k) % 4] + 12
                add(arps, t + k * BEAT / 4, pluck(m, 0.2), pan=(-0.45 if k % 2 == 0 else 0.45), gain=0.05)
        t += BEAT; beat += 1

    # Remates de caja antes de cada corte y golpes en los cortes
    for c in CUTS[1:4]:
        for k in range(4):
            add(drums, c - BEAT + k * BEAT / 4, snare(), gain=0.25 + 0.1 * k)
    for c in CUTS[:4]:
        add(fx, c, impact(0.7), gain=0.45); add(fx, c, crash(), gain=0.35)

    # Tensión: sin batería, latido que se acelera y subida hasta la explosión
    for b in (21.5, 22.15, 22.7, 23.15, 23.5, 23.72):
        add(drums, b, kick(), gain=0.8)
    add(pads, 21.5, pad_chord((38, 41, 45), BOOM - 21.5) * 0.9)
    add(fx, 22.0, riser(BOOM - 22.0), gain=0.45)

    # Explosión y pantalla final
    add(fx, BOOM, impact(1.3), gain=0.9); add(fx, BOOM, crash(), gain=0.5)
    add(fx, END_CARD, braam([26, 38, 45, 50]), gain=0.6)
    add(pads, END_CARD, pad_chord((50, 53, 57, 62), DUR - END_CARD) * 1.1)
    add(fx, END_CARD, crash(), gain=0.3)
    add(send, 26.75, bell(74), gain=0.25); add(send, 26.9, bell(81), gain=0.15)  # sello del logo

    music = np.stack([drums + bass + fx, drums + bass + fx], 1) * 0.7 + pads + arps
    music += reverb(send + arps.mean(1) * 0.6 + pads.mean(1) * 0.3, secs=2.2, decay=0.6) * 0.5
    fade = np.ones(N); fl = int(2.2 * SR); fade[-fl:] = np.linspace(1, 0, fl) ** 1.5
    music *= fade[:, None]
    return music / np.abs(music).max()


def duck_gain(dry, depth=0.55):
    env = np.abs(dry); hop = 441
    frames = np.array([env[i:i + hop].max() for i in range(0, len(env), hop)])
    frames = np.convolve(frames, np.ones(4) / 4, 'same') / max(frames.max(), 1e-9)
    hold = np.zeros_like(frames); level = 0.0
    for k, v in enumerate(frames):  # ataque rápido y relajación lenta
        level = v if v > level else level * 0.93
        hold[k] = level
    g = 1 - depth * np.clip(hold * 2.5, 0, 1)
    return np.interp(np.arange(len(dry)), np.arange(len(g)) * hop, g)


if __name__ == '__main__':
    print('Voz:')
    voice, dry = build_voice()
    print('Música...')
    music = build_music()
    g = duck_gain(dry)
    VOICE_GAIN, MUSIC_GAIN = 0.39, 0.39
    limit = lambda m: np.tanh(1.3 * m) / np.tanh(1.3)
    # El volumen general se fija con la mezcla aprobada (música 0.55): al bajar la música la voz no sube
    ref = limit(music * 0.55 * g[:, None] + voice * VOICE_GAIN)
    mix = limit(music * MUSIC_GAIN * g[:, None] + voice * VOICE_GAIN) * (0.95 / np.abs(ref).max())
    write_wav(os.path.join(OUT, 'audio_trailer.wav'), mix)
    write_wav(os.path.join(OUT, 'musica_sin_voz.wav'), music * 0.9)
    write_wav(os.path.join(OUT, 'voz_sola.wav'), voice)
    rms = lambda a, b: 20 * np.log10(np.sqrt(np.mean(mix[int(a * SR):int(b * SR)] ** 2)) + 1e-9)
    print(f'Listo: audio_trailer.wav ({DUR}s)  volumen intro {rms(0, 3):.1f} dB, groove {rms(8, 11):.1f} dB, final {rms(25, 28):.1f} dB')
