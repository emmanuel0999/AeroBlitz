"""Música nueva para el anuncio 2: synthwave espacial (128 bpm, Mi menor), con alarma de la nave en la tensión."""
import os, sys
HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import numpy as np
import trailer_audio as ta
import tiktok_audio as tka
import promo_zira_hq as hq
from trailer_audio import SR, add

rng = np.random.default_rng(11)


def clap():
    t = ta.tt(0.3); out = np.zeros(len(t))
    for d, dec in ((0.0, 0.006), (0.011, 0.006), (0.022, 0.1)):
        k = int(d * SR); out[k:] += rng.standard_normal(len(t) - k) * np.exp(-t[:len(t) - k] / dec)
    return ta.fft_filter(out, lo=900, hi=6500) * 0.9


def lead(m, dur):
    t = ta.tt(dur); f = ta.nf(m)
    y = ta.saw(f, dur, 16) + 0.6 * ta.saw(f * 1.007, dur, 16)
    y = ta.fft_filter(y, hi=3800)
    return y * ta.adsr(len(t), 0.004, 0.05) * np.exp(-t / 0.3)


def sub_bass(m, dur):
    t = ta.tt(dur); f = ta.nf(m)
    y = np.sin(2 * np.pi * f * t) + 0.4 * ta.saw(f, dur, 6)
    return np.tanh(1.8 * y) * ta.adsr(len(t), 0.003, 0.02)


def beep(m, dur=0.16):
    t = ta.tt(dur)
    return np.sin(2 * np.pi * ta.nf(m) * t) * ta.adsr(len(t), 0.005, 0.03)


def build_music_v2(dur, end, cuts, tension):
    n = int(dur * SR)
    drums = np.zeros(n); bass = np.zeros(n); fx = np.zeros(n); send = np.zeros(n)
    pads = np.zeros((n, 2)); leads = np.zeros((n, 2))
    B = 60 / 128; S = B / 4                                    # 128 bpm, semicorcheas
    CH = [(52, 55, 59), (48, 52, 55), (55, 59, 62), (50, 54, 57)]   # Em - C - G - D
    RT = [40, 36, 43, 38]
    HOOK = [76, 79, 83, 79, 81, 79, 76, 74]                       # melodía de 8 corcheas por compás
    stop, boom = tension
    pump = np.ones(n)
    add(fx, 0.0, ta.impact(0.9), gain=0.5); add(fx, 0.0, ta.crash(), gain=0.3)

    t = 0.0; step = 0
    while t < stop - 1e-6:
        bar = step // 16; ci = bar % 4; pos = step % 16
        if pos == 0: add(pads, t, ta.pad_chord(CH[ci], 16 * S + 0.05) * 1.1)
        if pos % 4 == 0:
            add(drums, t, ta.kick(), gain=0.95)
            k = int(t * SR); L = min(n - k, int(B * SR))         # bombeo: los pads respiran con el bombo
            pump[k:k + L] = 1 - 0.55 * np.exp(-np.arange(L) / (0.11 * SR))
        if pos in (4, 12): add(drums, t, clap(), gain=0.5); add(send, t, clap(), gain=0.2)
        if pos % 2 == 1 or t >= stop * 0.5: add(drums, t, ta.hat(), gain=0.32 if pos % 2 == 1 else 0.18)
        if pos % 2 == 0: add(bass, t, sub_bass(RT[ci] + (12 if pos % 4 == 2 else 0), 2 * S * 0.9), gain=0.4)
        if t >= 1.4 and pos % 2 == 0:
            add(leads, t, lead(HOOK[(pos // 2) % 8], 2 * S), pan=(-0.25 if (pos // 2) % 2 else 0.25), gain=0.07)
            add(send, t, lead(HOOK[(pos // 2) % 8], 2 * S), gain=0.05)
        t += S; step += 1

    for c in cuts:                                            # cada cambio de escena: subida corta y golpe
        add(fx, c - 0.6, ta.riser(0.6), gain=0.3)
        add(fx, c, ta.impact(0.6), gain=0.4); add(fx, c, ta.crash(), gain=0.28)

    # Tensión: zumbido grave, alarma de la nave y tic-tac que se acelera hasta la explosión
    add(pads, stop, ta.pad_chord((28, 40, 47), boom - stop) * 0.9)
    add(fx, stop + 0.2, ta.riser(boom - stop - 0.2), gain=0.42)
    a = stop + 0.1; k = 0
    while a < boom - 0.1:
        add(fx, a, beep(83 if k % 2 == 0 else 76), gain=0.07); a += 0.42; k += 1
    gap = 0.42; a = stop
    while a < boom - 0.05:
        add(drums, a, ta.hat(), gain=0.3); a += gap; gap = max(0.07, gap * 0.82)
    add(fx, boom, ta.impact(1.3), gain=0.9); add(fx, boom, ta.crash(), gain=0.5)

    # Pantalla final: acorde grande y arpegio de campanas que sube (Mi - Sol - Si - Mi)
    add(fx, end, ta.braam([28, 40, 47, 52], dur=dur - end + 0.5), gain=0.55)
    add(pads, end, ta.pad_chord((52, 55, 59, 64), dur - end) * 1.1)
    add(fx, end, ta.crash(), gain=0.3)
    for i, m in enumerate((76, 79, 83, 88)):
        add(send, end + 0.25 + i * 0.16, ta.bell(m, 2.0), gain=0.2)

    pads *= pump[:, None]; leads *= pump[:, None]
    music = np.stack([drums + bass + fx, drums + bass + fx], 1) * 0.7 + pads + leads
    music += ta.reverb(send + leads.mean(1) * 0.5 + pads.mean(1) * 0.3, secs=2.0, decay=0.5) * 0.5
    fade = np.ones(n); fl = int(1.2 * SR); fade[-fl:] = np.linspace(1, 0, fl) ** 1.5
    return music * fade[:, None] / np.abs(music).max()


if __name__ == '__main__':
    limit = lambda m: np.tanh(1.3 * m) / np.tanh(1.3)
    cfg = tka.VIDEOS['promo2']; n = int(cfg['dur'] * SR)
    music = build_music_v2(cfg['dur'], cfg['end'], cfg['cuts'], cfg['tension'])
    print('Voz (Zira, calidad alta):')
    voice, dry = hq.build_voice_hq('promo2', cfg['lines'], n)
    g = tka.duck_gain(dry, depth=0.68)
    ref = limit(music * 0.55 * g[:, None] + voice * tka.VOICE_GAIN)
    mix = limit(music * tka.MUSIC_GAIN * g[:, None] + voice * tka.VOICE_GAIN) * (0.95 / np.abs(ref).max())
    tka.write_wav(os.path.join(tka.OUT, 'audio_promo2_musica_nueva.wav'), mix)
    tka.write_wav(os.path.join(tka.OUT, 'musica_promo2_nueva.wav'), music * 0.9)
    print('pico', round(float(np.abs(mix).max()), 3), ' saturadas', int((np.abs(mix) > 0.999).sum()))
