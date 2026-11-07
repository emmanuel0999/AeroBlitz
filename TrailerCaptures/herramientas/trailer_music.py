import array, sys, wave

src = r'D:\Unity\AeroBlitz\MusicPreview\tema_08_niveles_50-56.wav'
dst = sys.argv[2] if len(sys.argv) > 2 else r'D:\Unity\AeroBlitz\TrailerCaptures\musica_trailer_32s.wav'
TOTAL, FADE_IN, FADE_OUT = (float(sys.argv[1]) if len(sys.argv) > 1 else 32.0), 0.15, 2.5

with wave.open(src, 'rb') as w:
    rate, ch, width = w.getframerate(), w.getnchannels(), w.getsampwidth()
    assert width == 2 and ch == 1
    loop = array.array('h', w.readframes(w.getnframes()))

n = int(TOTAL * rate)
out = array.array('h', (loop[i % len(loop)] for i in range(n)))  # el tema ya es un bucle continuo
fi, fo = int(FADE_IN * rate), int(FADE_OUT * rate)
for i in range(fi):
    out[i] = int(out[i] * i / fi)
for i in range(fo):
    k = n - fo + i
    out[k] = int(out[k] * (1 - i / fo) ** 2)

with wave.open(dst, 'wb') as w:
    w.setnchannels(1); w.setsampwidth(2); w.setframerate(rate)
    w.writeframes(out.tobytes())
print(f'loop={len(loop) / rate:.1f}s  salida={n / rate:.1f}s  -> {dst}')
