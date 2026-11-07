"""Música alegre y pegadiza para el anuncio: Re mayor, 125 bpm, con subida y 'drop' en la pantalla final (llamada a descargar)."""
import os, sys
HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import numpy as np
import trailer_audio as ta
import tiktok_audio as tka
import promo_zira_hq as hq
from promo2_musica import clap, sub_bass
from trailer_audio import SR, add

rng = np.random.default_rng(23)
REVERB_MIX = 0.45   # cantidad de sala en la música (el acabado nítido la baja)
B = 60 / 125; S = B / 4
CH = [(62, 66, 69), (57, 61, 64), (59, 62, 66), (55, 59, 62)]   # D - A - Bm - G (alegre)
RT = [38, 45, 47, 43]
HOOK = [78, None, 81, 83, None, 81, 78, 76, 78, None, 74, 76, None, 74, 71, 74]   # melodía pegadiza (semicorcheas, None = silencio)


def whistle(m, dur):
    t = ta.tt(dur); f = ta.nf(m) * (1 + 0.004 * np.sin(2 * np.pi * 5.5 * t))
    ph = 2 * np.pi * np.cumsum(f) / SR
    y = np.sin(ph) + 0.25 * np.sin(2 * ph) + 0.1 * np.sin(3 * ph)
    return y * ta.adsr(len(t), 0.01, 0.06) * np.exp(-t / 0.5)


def stab(chord, dur=0.16):
    return sum(ta.pluck(m + 12, dur) for m in chord) / len(chord)


def shaker():
    t = ta.tt(0.05)
    return ta.fft_filter(rng.standard_normal(len(t)), lo=6000) * np.exp(-t / 0.012) * 0.6


def groove(bufs, t0, t1, big=False, hook_from=0.0):
    drums, bass, leads, chords, send, pump = bufs
    n = len(drums); t = t0; step = 0
    while t < t1 - 1e-6:
        ci = (step // 16) % 4; pos = step % 16
        if pos % 4 == 0:
            add(drums, t, ta.kick(), gain=1.0)
            k = int(t * SR); L = min(n - k, int(B * SR))
            pump[k:k + L] = np.minimum(pump[k:k + L], 1 - 0.5 * np.exp(-np.arange(L) / (0.1 * SR)))
        if pos in (4, 12): add(drums, t, clap(), gain=0.55); add(send, t, clap(), gain=0.18)
        if pos % 4 == 2: add(drums, t, ta.hat(open_=True), gain=0.3)         # charles abierto a contratiempo
        add(drums, t, shaker(), gain=0.22 if pos % 2 else 0.12)
        if pos % 4 == 2: add(bass, t, sub_bass(RT[ci], 2 * S), gain=0.42)       # bajo saltarín a contratiempo
        if pos % 4 == 3: add(bass, t, sub_bass(RT[ci] + 12, S), gain=0.25)
        if pos in (2, 6, 10, 14):                                               # acordes cortos y brillantes
            add(chords, t, stab(CH[ci]), pan=(-0.3 if pos in (2, 10) else 0.3), gain=0.16 if big else 0.11)
        if t >= hook_from and HOOK[pos] is not None:
            w = whistle(HOOK[pos], 2.2 * S)
            add(leads, t, w, gain=0.11 if big else 0.07); add(send, t, w, gain=0.05)
        t += S; step += 1


def build_music_cta(dur, end, cuts, tension):
    n = int(dur * SR)
    drums = np.zeros(n); bass = np.zeros(n); fx = np.zeros(n); send = np.zeros(n)
    leads = np.zeros((n, 2)); chords = np.zeros((n, 2)); pads = np.zeros((n, 2)); pump = np.ones(n)
    bufs = (drums, bass, leads, chords, send, pump)
    stop, boom = tension

    add(fx, 0.0, ta.crash(), gain=0.35)
    groove(bufs, 0.0, stop, hook_from=1.2)
    for i, c in enumerate([0.0] + list(cuts)):
        add(pads, c, ta.pad_chord(CH[i % 4], 2.4) * 0.7)
    for c in cuts:
        add(fx, c - 0.5, ta.riser(0.5), gain=0.25); add(fx, c, ta.crash(), gain=0.3)

    # Subida antes del final: redoble que se acelera y sube, sin bombo, hasta la explosión
    a = stop; gap = 4 * S
    while a < boom - 0.03:
        add(drums, a, ta.snare(), gain=0.25 + 0.35 * (a - stop) / (boom - stop)); a += gap
        gap = max(S / 2, gap * 0.86)
    add(fx, stop + 0.2, ta.riser(boom - stop - 0.2), gain=0.4)
    add(pads, stop, ta.pad_chord((55, 59, 62, 69), boom - stop) * 0.8)
    add(fx, boom, ta.impact(1.1), gain=0.7); add(fx, boom, ta.crash(), gain=0.45)

    # 'Drop' alegre en la pantalla final: todo el grupo a la vez y la melodía más fuerte
    add(fx, end, ta.crash(), gain=0.45)
    groove(bufs, end, dur - 0.55, big=True)
    add(pads, end, ta.pad_chord((50, 62, 66, 69, 74), dur - end) * 0.9)
    for i, m in enumerate((74, 78, 81, 86)):                                # sello final que sube
        add(send, dur - 1.25 + i * 0.09, ta.bell(m, 1.6), gain=0.16)

    chords *= pump[:, None]; pads *= pump[:, None]
    mono = drums + bass + fx
    music = np.stack([mono, mono], 1) * 0.75 + leads + chords + pads
    music += ta.reverb(send + chords.mean(1) * 0.4 + pads.mean(1) * 0.25, secs=1.6, decay=0.45) * REVERB_MIX
    fade = np.ones(n); fl = int(0.5 * SR); fade[-fl:] = np.linspace(1, 0, fl) ** 1.5
    return music * fade[:, None] / np.abs(music).max()


if __name__ == '__main__':
    limit = lambda m: np.tanh(1.3 * m) / np.tanh(1.3)
    cfg = tka.VIDEOS['promo2']; n = int(cfg['dur'] * SR)
    music = build_music_cta(cfg['dur'], cfg['end'], cfg['cuts'], cfg['tension'])
    print('Voz (Zira, calidad alta):')
    voice, dry = hq.build_voice_hq('promo2', cfg['lines'], n)
    g = tka.duck_gain(dry, depth=0.68)   # igual que la versión aprobada: la voz se entiende sobre el final
    ref = limit(music * 0.55 * g[:, None] + voice * tka.VOICE_GAIN)
    mix = limit(music * tka.MUSIC_GAIN * g[:, None] + voice * tka.VOICE_GAIN) * (0.95 / np.abs(ref).max())
    tka.write_wav(os.path.join(tka.OUT, 'audio_promo2_cta.wav'), mix)
    tka.write_wav(os.path.join(tka.OUT, 'musica_promo2_cta.wav'), music * 0.9)
    print('pico', round(float(np.abs(mix).max()), 3), ' saturadas', int((np.abs(mix) > 0.999).sum()))
