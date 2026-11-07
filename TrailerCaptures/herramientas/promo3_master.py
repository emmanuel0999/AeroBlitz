"""Acabado nítido del anuncio: hueco de frecuencias para la voz, ecualización, menos sala y limitador limpio (sin saturación)."""
import os, sys
HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import numpy as np
from numpy.lib.stride_tricks import sliding_window_view
import trailer_audio as ta
import tiktok_audio as tka
import promo_zira_hq as hq
import promo3_musica as pm
from trailer_audio import SR, fft_filter


def eq_stereo(x, fn):
    return np.stack([fn(x[:, c]) for c in range(x.shape[1])], 1)


def gated_rms_db(x):
    """Volumen medio de las partes con sonido (aproxima la sonoridad que mide TikTok)."""
    m = x.mean(1); w = int(0.4 * SR)
    blocks = np.array([np.sqrt(np.mean(m[i:i + w] ** 2)) for i in range(0, len(m) - w, w // 2)]) + 1e-9
    db = 20 * np.log10(blocks); db = db[db > db.max() - 30]
    return 10 * np.log10(np.mean(10 ** (db / 10)))


def limiter(x, ceil=0.891, look=0.004, rel=0.08):
    """Limitador con anticipación: baja el volumen justo antes de un pico, sin distorsionar."""
    req = np.minimum(1.0, ceil / np.maximum(np.abs(x).max(1), 1e-9))
    L = int(look * SR)
    req = sliding_window_view(np.concatenate([req, np.ones(L)]), L + 1).min(1)[:len(req)]
    a = np.exp(-1 / (rel * SR)); g = np.empty_like(req); cur = 1.0
    for i, r in enumerate(req):
        cur = r if r < cur else r + (cur - r) * a
        g[i] = cur
    g = np.minimum(np.convolve(g, np.ones(L) / L, 'same'), req)
    return np.clip(x * g[:, None], -ceil, ceil), float(20 * np.log10(g.min()))


if __name__ == '__main__':
    cfg = tka.VIDEOS['promo2']; n = int(cfg['dur'] * SR)
    pm.REVERB_MIX = 0.28                                            # menos sala: más definición
    music = pm.build_music_cta(cfg['dur'], cfg['end'], cfg['cuts'], cfg['tension'])
    print('Voz (Zira, calidad alta):')
    voice, dry = hq.build_voice_hq('promo2', cfg['lines'], n)

    # Música: sin retumbe bajo 30 Hz, menos "barro" (200-400 Hz) y algo más de aire (>9 kHz)
    def clean(x):
        x = fft_filter(x, lo=30)
        x = x - 0.22 * fft_filter(x, lo=200, hi=400, order=1)
        return x + 0.25 * fft_filter(x, lo=9000)
    music = eq_stereo(music, clean)

    # Hueco para la voz: mientras Zira habla, la música baja solo en 1,2-4,5 kHz (donde se entienden las palabras)
    g = tka.duck_gain(dry, depth=0.6)
    act = np.clip((1 - g) / 0.6, 0, 1)
    mid = eq_stereo(music, lambda x: fft_filter(x, lo=1200, hi=4500, order=1))
    music = music - (0.6 * act)[:, None] * mid

    mix = music * tka.MUSIC_GAIN * g[:, None] + voice * tka.VOICE_GAIN
    mix = eq_stereo(mix, lambda x: fft_filter(x, lo=25))

    # Casi el mismo volumen que la versión aprobada, y limitador limpio en vez de saturar
    prev = tka.read_wav(os.path.join(tka.OUT, 'audio_promo2_cta.wav'))[0] if hasattr(tka, 'read_wav') else ta.read_wav(os.path.join(tka.OUT, 'audio_promo2_cta.wav'))[0]
    target = gated_rms_db(np.stack([prev, prev], 1)) - 0.5   # algo menos de compresión: golpes más limpios
    mix *= 10 ** ((target - gated_rms_db(mix)) / 20)
    out, gr = limiter(mix)
    tka.write_wav(os.path.join(tka.OUT, 'audio_promo2_cta_nitido.wav'), out)
    sat = int((np.abs(out) > 0.999).sum())
    print(f'volumen {gated_rms_db(out):.1f} dB (antes {target + 0.5:.1f} dB)  limitador max {gr:.1f} dB  pico {np.abs(out).max():.3f}  saturadas {sat}')
