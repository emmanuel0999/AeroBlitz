"""Genera el audio del anuncio de TikTok con otras voces de Windows para comparar."""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import numpy as np
import trailer_audio as ta
import tiktok_audio as tt

VOCES = {'David': 0.95, 'Zira': 0.98}   # voz de Windows y cuánto se baja su tono
limit = lambda m: np.tanh(1.3 * m) / np.tanh(1.3)
cfg = tt.VIDEOS['promo']; n = int(cfg['dur'] * tt.SR)
music = tt.build_music(cfg['dur'], cfg['end'], cfg['cuts'], cfg['tension'])
for voz in (sys.argv[1:] or list(VOCES)):
    ta.VOICE = voz; ta.PITCH_FACTOR = VOCES[voz]
    print(f'{voz}:')
    voice, dry = tt.build_voice(f'promo_{voz}', cfg['lines'], n)
    g = tt.duck_gain(dry)
    ref = limit(music * 0.55 * g[:, None] + voice * tt.VOICE_GAIN)
    mix = limit(music * tt.MUSIC_GAIN * g[:, None] + voice * tt.VOICE_GAIN) * (0.95 / np.abs(ref).max())
    tt.write_wav(os.path.join(tt.OUT, f'audio_promo_{voz}.wav'), mix)