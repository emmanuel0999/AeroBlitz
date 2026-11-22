# -*- coding: utf-8 -*-
"""Elige y recorta los fotogramas de jefes para las capturas 1 y 4 de la ficha (v17).
Entrada: D:/Temp/bosscap/ficha2/{emp,ai}_{en,es,fr}/ (fotos cada segundo + log con la posición del jefe y las fases).
Salida:  D:/Temp/bosscap/ficha2/sel/{idioma}_boss1.png (captura 1), _hero4.png, _p1.png, _p2.png (captura 4).
"""
import os, re, glob
import numpy as np
from PIL import Image

BASE = r"D:/Temp/bosscap/ficha2"
SEL = BASE + "/sel"
os.makedirs(SEL, exist_ok=True)

ACCENT = {"emp": (255, 64, 77), "ai": (255, 77, 217)}   # color de los disparos de cada jefe
LEVEL = {"emp": 70, "ai": 60}


def parse(folder, level):
    shots, phases = {}, {}
    for line in open(f"{folder}/log_L{level}.txt", encoding="utf-8"):
        p = line.split()
        if len(p) >= 2 and p[1] == "FASE":
            phases.setdefault(int(p[2]), float(p[0]))
        elif len(p) >= 9 and p[1] == "shot":
            shots[int(p[2])] = dict(x=float(p[4]), dead=int(p[6]), enter=int(p[8]))
    return shots, phases


def bullets(path, acc, y0=290, h=729):
    # cuenta solo las balas que quedarán dentro del recorte (la imagen se reduce 4 veces)
    a = np.asarray(Image.open(path).convert("RGB").resize((270, 480))).astype(int)
    d = np.abs(a - np.array(acc)).sum(axis=2)
    return int((d[y0 // 4:(y0 + h) // 4] < 110).sum())


def phase_of(t, phases):
    n = 0
    for k, s in phases.items():
        if t >= s:
            n = max(n, k)
    return n


def best(kind, folder, shots, phases, phase, margin, h=729, cx_range=(400, 680)):
    cands = []
    for tt, s in shots.items():
        t = tt / 10.0
        if s["dead"] or not s["enter"]:
            continue
        if phase_of(t, phases) != phase:
            continue
        if t < phases.get(phase, 1e9) + margin:      # sin el cartel de cambio de fase encima
            continue
        if not (cx_range[0] <= s["x"] <= cx_range[1]):
            continue
        f = f"{folder}/L{LEVEL[kind]}_t{tt:04d}.png"
        if os.path.exists(f):
            cands.append((bullets(f, ACCENT[kind], 290, h), tt, f, s["x"]))
    cands.sort(reverse=True)
    return cands[0] if cands else None


def crop(src, out, cx, w, h, y0=290):   # 290: por debajo de los contadores de mejoras del HUD
    im = Image.open(src).convert("RGB")
    x0 = int(max(0, min(1080 - w, cx - w / 2)))
    im.crop((x0, y0, x0 + w, y0 + h)).save(out)


for lg in ("en", "es", "fr"):
    # captura 1: Emperador, la fase más intensa
    folder = f"{BASE}/emp_{lg}"
    shots, phases = parse(folder, 70)
    c = best("emp", folder, shots, phases, 3, 3.0, 1125) or best("emp", folder, shots, phases, 2, 3.0, 1125)
    # es y fr: fotogramas elegidos a mano donde las balas llegan hasta la nave (más sensación de peligro)
    PELIGRO = {"en": 431, "es": 452, "fr": 452}
    if lg in PELIGRO:
        tt = PELIGRO[lg]
        c = (0, tt, f"{folder}/L70_t{tt:04d}.png", shots[tt]["x"])
    print(lg, "captura 1 ->", c[1], "balas", c[0], "x", c[3])
    crop(c[2], f"{SEL}/{lg}_boss1.png", c[3], 760, 1125)

    # captura 4: Inteligencia Suprema, fases 1, 2 y furia
    folder = f"{BASE}/ai_{lg}"
    shots, phases = parse(folder, 60)
    f3 = best("ai", folder, shots, phases, 3, 3.0, 696)
    f2 = best("ai", folder, shots, phases, 2, 3.0)
    f1 = best("ai", folder, shots, phases, 1, 2.0)
    print(lg, "captura 4 -> fase1", f1[1], "fase2", f2[1], "furia", f3[1])
    crop(f3[2], f"{SEL}/{lg}_hero4.png", f3[3], 800, 696)
    crop(f1[2], f"{SEL}/{lg}_p1.png", f1[3], 760, 729)
    crop(f2[2], f"{SEL}/{lg}_p2.png", f2[3], 760, 729)
print("listo:", SEL)
