using System.Collections.Generic;
using UnityEngine;

namespace AeroBlitz
{
    /// <summary>
    /// Genera todos los sprites por código. Para usar arte propio, reemplaza estos métodos
    /// por referencias a tus sprites (la lógica del juego no cambia).
    /// </summary>
    public static class SpriteFactory
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        static Sprite Cached(string key, System.Func<Sprite> make)
        {
            if (!Cache.TryGetValue(key, out var s) || s == null) { s = make(); Cache[key] = s; }
            return s;
        }

        static Texture2D NewTex(int w, int h)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            return t;
        }

        static Sprite Finish(Texture2D t, Color[] px, float ppu, Vector4 border = default)
        {
            t.SetPixels(px);
            t.Apply(false, true);
            return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect, border);
        }

        public static Sprite White => Cached("white", () =>
        {
            var t = NewTex(4, 4);
            var px = new Color[16];
            for (int i = 0; i < 16; i++) px[i] = Color.white;
            return Finish(t, px, 4);
        });

        /// <summary>Rectángulo redondeado 9-slice para la UI.</summary>
        public static Sprite Rounded => Cached("rounded", () =>
        {
            const int S = 64; const float R = 22f;
            var t = NewTex(S, S);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float cx = Mathf.Clamp(x + 0.5f, R, S - R), cy = Mathf.Clamp(y + 0.5f, R, S - R);
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                    px[y * S + x] = new Color(1, 1, 1, Mathf.Clamp01(R - d + 0.5f));
                }
            return Finish(t, px, 100, new Vector4(24, 24, 24, 24));
        });

        public static Sprite Circle => Cached("circle", () =>
        {
            const int S = 64;
            var t = NewTex(S, S);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(S / 2f, S / 2f));
                    px[y * S + x] = new Color(1, 1, 1, Mathf.Clamp01(S / 2f - d));
                }
            return Finish(t, px, S);
        });

        public static Sprite Glow => Cached("glow", () =>
        {
            const int S = 64;
            var t = NewTex(S, S);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(S / 2f, S / 2f)) / (S / 2f);
                    px[y * S + x] = new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - d), 2f));
                }
            return Finish(t, px, S);
        });

        /// <summary>Planeta lejano: esfera con luz por un lado y borde suave (se tiñe con el color del mundo).</summary>
        public static Sprite Planet => Cached("planet", () =>
        {
            const int S = 128;
            var t = NewTex(S, S);
            var px = new Color[S * S];
            var light = new Vector2(-0.45f, 0.5f).normalized;
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    var p = new Vector2((x + 0.5f) / (S / 2f) - 1f, (y + 0.5f) / (S / 2f) - 1f);
                    float d = p.magnitude;
                    float edge = Mathf.Clamp01((1f - d) * S / 2f);
                    float z = Mathf.Sqrt(Mathf.Max(0f, 1f - d * d));
                    float lit = Mathf.Clamp01(Vector2.Dot(p, light) * 0.6f + z * 0.55f + 0.05f);
                    float band = 0.06f * Mathf.Sin(p.y * 9f + p.x * 1.5f); // franjas suaves
                    float v = Mathf.Clamp01(0.25f + lit * 0.85f + band);
                    px[y * S + x] = new Color(v, v, v, edge);
                }
            return Finish(t, px, S);
        });

        /// <summary>Moneda dorada con borde y brillo.</summary>
        public static Sprite Coin => Cached("coin", () =>
        {
            const int S = 64;
            var t = NewTex(S, S);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    var p = new Vector2((x + 0.5f) / (S / 2f) - 1f, (y + 0.5f) / (S / 2f) - 1f);
                    float d = p.magnitude;
                    float a = Mathf.Clamp01((1f - d) * S / 2f);
                    var gold = Color.Lerp(new Color(0.85f, 0.55f, 0.08f), new Color(1f, 0.92f, 0.45f), Mathf.Clamp01(0.5f + p.y * 0.4f - p.x * 0.2f));
                    if (d > 0.78f) gold *= 0.78f;                                   // borde
                    else if (d > 0.55f && d < 0.62f) gold *= 0.85f;                 // aro interior
                    if ((p - new Vector2(-0.3f, 0.35f)).magnitude < 0.16f) gold = Color.Lerp(gold, Color.white, 0.6f); // brillo
                    gold.a = a;
                    px[y * S + x] = gold;
                }
            return Finish(t, px, S);
        });

        /// <summary>Icono de una mejora temporal: esfera de color con un símbolo blanco.</summary>
        public static Sprite PowerUp(PowerUpType type) => Cached("pu_" + type, () =>
        {
            const int S = 96;
            var t = NewTex(S, S);
            var px = new Color[S * S];
            var col = PowerUps.ColorOf(type);
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    var p = new Vector2((x + 0.5f) / (S / 2f) - 1f, (y + 0.5f) / (S / 2f) - 1f);
                    float d = p.magnitude;
                    if (d > 1f) continue;
                    // halo exterior suave + esfera interior más oscura
                    Color c;
                    if (d > 0.78f) { c = col; c.a = Mathf.Pow(1f - (d - 0.78f) / 0.22f, 1.5f) * 0.7f; }
                    else
                    {
                        c = Color.Lerp(col * 0.35f, col * 0.75f, Mathf.Clamp01(0.5f + p.y * 0.5f));
                        c.a = 1f;
                        if (d > 0.7f) c = Color.Lerp(c, col, 0.8f); // aro
                    }
                    // símbolo con supermuestreo 2x2
                    int hits = 0;
                    for (int sy = 0; sy < 2; sy++)
                        for (int sx = 0; sx < 2; sx++)
                        {
                            var q = new Vector2((x + 0.25f + sx * 0.5f) / (S / 2f) - 1f, (y + 0.25f + sy * 0.5f) / (S / 2f) - 1f);
                            if (Symbol(type, q)) hits++;
                        }
                    if (hits > 0) { c = Color.Lerp(c, Color.white, hits / 4f); c.a = Mathf.Max(c.a, hits / 4f); }
                    px[y * S + x] = c;
                }
            return Finish(t, px, S);
        });

        static bool Symbol(PowerUpType type, Vector2 p)
        {
            switch (type)
            {
                case PowerUpType.Shield:
                    return InPoly(p, new[] { new Vector2(0, 0.5f), new Vector2(0.38f, 0.34f), new Vector2(0.34f, -0.08f), new Vector2(0, -0.5f), new Vector2(-0.34f, -0.08f), new Vector2(-0.38f, 0.34f) });
                case PowerUpType.Overcharge:
                    return InPoly(p, new[] { new Vector2(0.12f, 0.55f), new Vector2(-0.3f, -0.02f), new Vector2(-0.03f, -0.02f), new Vector2(-0.14f, -0.55f), new Vector2(0.3f, 0.06f), new Vector2(0.03f, 0.06f) });
                case PowerUpType.Magnet:
                {
                    float r = (p - new Vector2(0, 0.05f)).magnitude;
                    bool arc = p.y < 0.05f && r > 0.18f && r < 0.4f;
                    bool arms = p.y >= 0.05f && p.y < 0.42f && Mathf.Abs(p.x) > 0.18f && Mathf.Abs(p.x) < 0.4f;
                    return arc || arms;
                }
                default: // Repair: cruz
                    return (Mathf.Abs(p.x) < 0.12f && Mathf.Abs(p.y) < 0.42f) || (Mathf.Abs(p.y) < 0.12f && Mathf.Abs(p.x) < 0.42f);
            }
        }

        /// <summary>Icono de un artículo (ver IconPainter). 128 px en el juego.</summary>
        public static Sprite Icon(string key) => Cached("icon_" + key, () => Finish(NewTex(128, 128), IconPainter.Paint(key, 128), 128));

        static bool InPoly(Vector2 p, Vector2[] poly)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if ((poly[i].y > p.y) != (poly[j].y > p.y) &&
                    p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x)
                    inside = !inside;
            }
            return inside;
        }

        /// <summary>Rellena un polígono (coordenadas -1..1) con antialias por supermuestreo.</summary>
        static Color[] RasterPoly(int S, Vector2[] poly, System.Func<Vector2, Color> shade)
        {
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    int hits = 0;
                    for (int sy = 0; sy < 2; sy++)
                        for (int sx = 0; sx < 2; sx++)
                        {
                            var p = new Vector2((x + 0.25f + sx * 0.5f) / S * 2f - 1f, (y + 0.25f + sy * 0.5f) / S * 2f - 1f);
                            if (InPoly(p, poly)) hits++;
                        }
                    if (hits == 0) continue;
                    var uv = new Vector2((x + 0.5f) / S * 2f - 1f, (y + 0.5f) / S * 2f - 1f);
                    var c = shade(uv);
                    c.a *= hits / 4f;
                    px[y * S + x] = c;
                }
            return px;
        }

        /// <summary>Nave en blanco/gris (se tiñe con la pintura elegida).</summary>
        public static Sprite Ship(ShipDefinition def) => Cached("ship_" + def.id, () =>
        {
            const int S = 128;
            var poly = new Vector2[def.shape.Length];
            for (int i = 0; i < poly.Length; i++) poly[i] = def.shape[i] * 0.96f;
            var px = RasterPoly(S, poly, uv =>
            {
                float shade = 0.65f + 0.35f * (1 - Mathf.Abs(uv.x)) + uv.y * 0.1f;
                return new Color(shade, shade, shade, 1);
            });
            return Finish(NewTex(S, S), px, 100);
        });

        public static Sprite Cockpit => Cached("cockpit", () =>
        {
            const int S = 32;
            var poly = new[] { new Vector2(0, 1), new Vector2(0.6f, -0.2f), new Vector2(0.4f, -0.9f), new Vector2(-0.4f, -0.9f), new Vector2(-0.6f, -0.2f) };
            var px = RasterPoly(S, poly, uv => Color.Lerp(new Color(0.1f, 0.2f, 0.4f), new Color(0.6f, 0.95f, 1f), (uv.y + 1) / 2f));
            return Finish(NewTex(S, S), px, 100);
        });

        public static Sprite Flame => Cached("flame", () =>
        {
            const int S = 32;
            var poly = new[] { new Vector2(-0.6f, 1), new Vector2(0.6f, 1), new Vector2(0, -1) };
            var px = RasterPoly(S, poly, uv => Color.Lerp(new Color(1f, 0.3f, 0.05f, 0.6f), new Color(1f, 0.95f, 0.6f, 1f), (uv.y + 1) / 2f));
            return Finish(NewTex(S, S), px, 64);
        });

        public static Sprite Asteroid(int variant) => Cached("ast_" + variant, () =>
        {
            const int S = 96;
            var rnd = new System.Random(1234 + variant * 77);
            int n = 11;
            var poly = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                float a = i / (float)n * Mathf.PI * 2f;
                float r = 0.75f + (float)rnd.NextDouble() * 0.22f;
                poly[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
            }
            var craters = new List<Vector3>();
            for (int i = 0; i < 4; i++)
                craters.Add(new Vector3((float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f, 0.08f + (float)rnd.NextDouble() * 0.12f));
            var baseCol = new Color(0.55f, 0.45f, 0.38f);
            var px = RasterPoly(S, poly, uv =>
            {
                float light = 0.75f + 0.25f * (uv.y - uv.x) * 0.7f;
                var c = baseCol * light;
                foreach (var cr in craters)
                    if (Vector2.Distance(uv, new Vector2(cr.x, cr.y)) < cr.z) c *= 0.7f;
                c.a = 1;
                return c;
            });
            return Finish(NewTex(S, S), px, 100);
        });

        public static Sprite Drone => Cached("drone", () =>
        {
            const int S = 96;
            var poly = new Vector2[6];
            for (int i = 0; i < 6; i++) { float a = i / 6f * Mathf.PI * 2f; poly[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.7f) * 0.95f; }
            var px = RasterPoly(S, poly, uv =>
            {
                float d = uv.magnitude;
                if (d < 0.25f) return new Color(1f, 0.9f, 0.3f);
                if (d < 0.35f) return new Color(0.2f, 0.05f, 0.05f);
                return new Color(0.8f, 0.15f, 0.2f) * (0.8f + 0.2f * uv.y) + new Color(0, 0, 0, 1);
            });
            return Finish(NewTex(S, S), px, 100);
        });

        public static Sprite Mine => Cached("mine", () =>
        {
            const int S = 96;
            int spikes = 10;
            var poly = new Vector2[spikes * 2];
            for (int i = 0; i < spikes * 2; i++)
            {
                float a = i / (float)(spikes * 2) * Mathf.PI * 2f;
                float r = i % 2 == 0 ? 0.98f : 0.62f;
                poly[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
            }
            var px = RasterPoly(S, poly, uv =>
            {
                float d = uv.magnitude;
                if (d < 0.2f) return new Color(1f, 0.2f, 0.2f);
                return new Color(0.25f, 0.25f, 0.3f) * (0.8f + 0.3f * uv.y) + new Color(0, 0, 0, 1);
            });
            return Finish(NewTex(S, S), px, 100);
        });

        /// <summary>Barra metálica con franjas de peligro (indestructible).</summary>
        public static Sprite Wall => Cached("wall", () =>
        {
            const int W = 128, H = 32;
            var px = new Color[W * H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    bool border = y < 3 || y >= H - 3 || x < 3 || x >= W - 3;
                    bool stripe = ((x + y) / 10) % 2 == 0;
                    px[y * W + x] = border ? new Color(0.55f, 0.6f, 0.7f) : stripe ? new Color(0.95f, 0.75f, 0.1f) : new Color(0.15f, 0.15f, 0.18f);
                }
            return Finish(NewTex(W, H), px, 64);
        });

        public static Sprite Laser => Cached("laser", () =>
        {
            const int W = 64, H = 16;
            var px = new Color[W * H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float d = Mathf.Abs(y + 0.5f - H / 2f) / (H / 2f);
                    px[y * W + x] = Color.Lerp(Color.white, new Color(1f, 0.1f, 0.3f, 0f), d * d * 1.2f);
                }
            return Finish(NewTex(W, H), px, 64);
        });
    }
}
