"""Voz de Zira con más calidad para el anuncio: remuestreo limpio, sin cambio de tono, poco eco y ecualización de claridad."""
import os, subprocess, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import numpy as np
import trailer_audio as ta
import tiktok_audio as tt
from trailer_audio import SR, add, fft_filter, reverb, read_wav

HERE = os.path.dirname(os.path.abspath(__file__))


def resample_fft(x, sr_in, sr_out):
    """Remuestreo por FFT: sin la aspereza metálica que deja la interpolación lineal."""
    n_out = int(round(len(x) * sr_out / sr_in))
    X = np.fft.rfft(x)
    Y = np.zeros(n_out // 2 + 1, complex)
    k = min(len(X), len(Y))
    Y[:k] = X[:k]
    t = max(8, int(k * 0.04)); Y[k - t:k] *= np.linspace(1, 0, t)  # corte suave arriba del todo
    return np.fft.irfft(Y, n_out) * (n_out / len(x))


def level(y, ms=25, amount=0.6):
    """Compresión suave: iguala sílabas fuertes y débiles para que se entienda todo."""
    w = int(SR * ms / 1000)
    env = np.sqrt(np.convolve(y ** 2, np.ones(w) / w, 'same')) + 1e-6
    env = env / env.max()
    return y / (env ** amount + 0.15)


def make_line_hq(i, text, rate, pitch):
    ssml = ('<speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xml:lang="en-US">'
            f'<prosody rate="{rate}" pitch="{pitch}">{text}</prosody></speak>')
    sp = os.path.join(HERE, 'voz', f'hq{i}.ssml'); wp = os.path.join(HERE, 'voz', f'hq{i}.wav')
    with open(sp, 'w', encoding='utf-8') as f: f.write(ssml)
    subprocess.run([ta.SPEAK, 'Zira', sp, wp], check=True, capture_output=True)
    x, sr = read_wav(wp)
    idx = np.nonzero(np.abs(x) > 0.01 * np.abs(x).max())[0]
    x = x[max(0, idx[0] - int(0.02 * sr)): idx[-1] + int(0.08 * sr)]
    y = resample_fft(x, sr, SR)
    y = fft_filter(y, lo=90)                                     # quita graves que embarran
    y = y - 0.18 * fft_filter(y, lo=230, hi=480, order=1)        # menos "caja"
    y = y + 0.30 * fft_filter(y, lo=2200, hi=5200, order=1)      # presencia: se entiende mejor
    hf = fft_filter(y, lo=3500, hi=7400)                          # brillo: armónicos suaves por encima de 8 kHz
    ex = fft_filter(np.tanh(3 * hf / (np.abs(hf).max() + 1e-9)), lo=8000)
    y = y / np.abs(y).max() + 0.035 * ex
    y = level(y)
    n = int(0.006 * SR); y[:n] *= np.linspace(0, 1, n); y[-n:] *= np.linspace(1, 0, n)
    return y / np.abs(y).max()


def build_voice_hq(key, lines, n):
    dry = np.zeros(n)
    for i, (start, text, rate, pitch) in enumerate(lines):
        y = make_line_hq(f'{key}_{i}', text, rate, pitch)
        add(dry, start, y)
        print(f'    {start:5.2f}-{start + len(y) / SR:5.2f}s  {text}')
    wet = reverb(dry, secs=0.5, decay=0.09, seed=5) * 0.035       # solo un toque de sala, sin eco
    v = np.stack([dry, dry], 1) + wet
    return v / np.abs(v).max() * 0.9, dry


if __name__ == '__main__':
    limit = lambda m: np.tanh(1.3 * m) / np.tanh(1.3)
    key = sys.argv[1] if len(sys.argv) > 1 else 'promo'
    cfg = tt.VIDEOS[key]; n = int(cfg['dur'] * SR)
    music = tt.build_music(cfg['dur'], cfg['end'], cfg['cuts'], cfg['tension'])
    print('Zira (calidad alta):')
    voice, dry = build_voice_hq(key, cfg['lines'], n)
    g = tt.duck_gain(dry, depth=0.68)   # la música baja algo más mientras habla: más clara sin subir la voz
    ref = limit(music * 0.55 * g[:, None] + voice * tt.VOICE_GAIN)
    mix = limit(music * tt.MUSIC_GAIN * g[:, None] + voice * tt.VOICE_GAIN) * (0.95 / np.abs(ref).max())
    tt.write_wav(os.path.join(tt.OUT, f'audio_{key}_Zira_hq.wav'), mix)
    tt.write_wav(os.path.join(tt.OUT, f'voz_{key}_Zira_hq.wav'), voice)
    # Comparación objetiva con la versión anterior: energía por bandas de la voz sola
    def bands(x):
        X = np.abs(np.fft.rfft(x)) ** 2; fr = np.fft.rfftfreq(len(x), 1 / SR); tot = X.sum()
        return [round(10 * np.log10(X[(fr >= a) & (fr < b)].sum() / tot + 1e-12), 1) for a, b in ((80, 300), (2000, 5000), (8000, 12000))]
    print('voz nueva  graves/presencia/brillo dB:', bands(dry))
