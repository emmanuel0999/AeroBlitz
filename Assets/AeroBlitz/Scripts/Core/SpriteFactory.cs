using System.Collections.Generic;
using UnityEngine;

namespace AeroBlitz
{
    /// <summary>
    /// Genera todos los sprites por código. Para usar arte propio, reemplaza estos métodos
    /// por referencias a tus sprites (la lógica del juego no cambia).
    /// </summary>
    public static partial class SpriteFactory
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        static Sprite Cached(string key, System.Func<Sprite> make)
        {
            if (!Cache.TryGetValue(key, out var s) || s == null) { s = make(); Cache[key] = s; }
            return s;
        }

        /// <summary>
        /// Textura para un sprite. Con mipmaps (por defecto) se ve limpia también cuando sale pequeña en pantalla
        /// (estrellas, balas, naves lejanas); el sesgo negativo mantiene la nitidez cuando se ve a su tamaño o algo menor.
        /// </summary>
        static Texture2D NewTex(int w, int h, bool mips = true)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, mips) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            if (mips) t.mipMapBias = -0.5f;
            return t;
        }

        static Sprite Finish(Texture2D t, Color[] px, float ppu, Vector4 border = default)
        {
            t.SetPixels(px);
            t.Apply(t.mipmapCount > 1, true);
            return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect, border);
        }

        /// <summary>
        /// Polígono (coordenadas -1..1) con borde suave exacto y contorno oscuro por dentro (el tamaño no cambia).
        /// shade recibe el punto y la profundidad (distancia al borde) para poder dar volumen.
        /// </summary>
        static Color[] PolyPixels(int S, Vector2[] poly, System.Func<Vector2, float, Color> shade, float ink, Color inkColor)
        {
            var px = new Color[S * S];
            float pw = 2f / S;
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    var p = new Vector2((x + 0.5f) / S * 2f - 1f, (y + 0.5f) / S * 2f - 1f);
                    float dist = EdgeDist(p, poly);
                    bool inside = InPoly(p, poly);
                    float sd = inside ? -dist : dist;
                    if (sd > pw) continue;
                    var c = Color.Lerp(inkColor, shade(p, -sd), Cov(-sd - ink, pw));
                    c.a = Cov(-sd, pw);
                    px[y * S + x] = c;
                }
            return px;
        }

        /// <summary>Ruido fijo por posición (textura fina de roca y metal).</summary>
        static float Hash(Vector2 p)
        {
            float h = Mathf.Sin(p.x * 127.1f + p.y * 311.7f) * 43758.5453f;
            return h - Mathf.Floor(h);
        }

        public static Sprite White => Cached("white", () =>
        {
            var t = NewTex(4, 4, false);
            var px = new Color[16];
            for (int i = 0; i < 16; i++) px[i] = Color.white;
            return Finish(t, px, 4);
        });

        /// <summary>Rectángulo redondeado 9-slice para la UI.</summary>
        public static Sprite Rounded => Cached("rounded", () =>
        {
            const int S = 128; const float R = 44f;
            var t = NewTex(S, S, false);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float cx = Mathf.Clamp(x + 0.5f, R, S - R), cy = Mathf.Clamp(y + 0.5f, R, S - R);
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                    px[y * S + x] = new Color(1, 1, 1, Mathf.Clamp01(R - d + 0.5f));
                }
            return Finish(t, px, 200, new Vector4(48, 48, 48, 48)); // esquinas igual de grandes que antes, el doble de nítidas
        });

        public static Sprite Circle => Cached("circle", () =>
        {
            const int S = 512;
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

        /// <summary>Aro (para la carga del arma alrededor del botón de disparo; se rellena en modo radial).</summary>
        public static Sprite Ring => Cached("ring", () =>
        {
            const int S = 512;
            const float Outer = S / 2f - 1f, Inner = Outer - 44f;
            var t = NewTex(S, S);
            var px = new Color[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(S / 2f, S / 2f));
                    px[y * S + x] = new Color(1, 1, 1, Mathf.Clamp01(Outer - d) * Mathf.Clamp01(d - Inner));
                }
            return Finish(t, px, S);
        });

        public static Sprite Glow => Cached("glow", () =>
        {
            const int S = 128;
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
            const int S = 512;
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
            const int S = 128;
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
            const int S = 192;
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
        public static Sprite Icon(string key) => Cached("icon_" + key, () => Finish(NewTex(256, 256), IconPainter.Paint(key, 256), 256));

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

        // Las naves (cartoon, metálico, detalles, neón) y la cabina están en ShipArt.cs

        public static Sprite Flame => Cached("flame", () =>
        {
            const int S = 128;
            var poly = new[] { new Vector2(-0.6f, 1), new Vector2(0.6f, 1), new Vector2(0, -1) };
            var px = RasterPoly(S, poly, uv => Color.Lerp(new Color(1f, 0.3f, 0.05f, 0.6f), new Color(1f, 0.95f, 0.6f, 1f), (uv.y + 1) / 2f));
            return Finish(NewTex(S, S), px, 256);
        });

        /// <summary>Fuego en escala de grises (núcleo casi blanco) para teñirlo con el color de una estela.</summary>
        public static Sprite FlameMask => Cached("flame_mask", () =>
        {
            const int S = 128;
            var poly = new[] { new Vector2(-0.6f, 1), new Vector2(0.6f, 1), new Vector2(0, -1) };
            var px = RasterPoly(S, poly, uv => Color.Lerp(new Color(0.75f, 0.75f, 0.75f, 0.55f), new Color(1f, 1f, 1f, 1f), (uv.y + 1) / 2f));
            return Finish(NewTex(S, S), px, 256);
        });

        public static Sprite Asteroid(int variant) => Cached("ast_" + variant, () =>
        {
            // 384 px para 0,96 unidades: el asteroide grande (x1,8) sigue nítido en pantallas QHD
            const int S = 384;
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
            var baseCol = new Color(0.58f, 0.47f, 0.39f);
            float pw = 2f / S;
            var px = PolyPixels(S, poly, (uv, depth) =>
            {
                float light = 0.74f + 0.3f * (uv.y - uv.x) * 0.6f;
                float bevel = Mathf.Clamp01(1f - depth / 0.13f);                       // canto iluminado arriba a la izquierda
                light += bevel * 0.2f * Mathf.Clamp((uv.y - uv.x) * 1.2f, -1f, 1f);
                foreach (var cr in craters)
                {
                    var o = uv - new Vector2(cr.x, cr.y);
                    float d = o.magnitude;
                    if (d < cr.z) light *= Mathf.Lerp(0.6f, 0.82f, Mathf.Clamp01((o.x - o.y) / cr.z * 0.5f + 0.5f)); // hueco con sombra
                    light *= 1f - 0.3f * Cov(0.018f - Mathf.Abs(d - cr.z), pw);         // borde del cráter
                }
                light += (Hash(uv * 9f) - 0.5f) * 0.07f;                                  // textura de roca
                var c = baseCol * light;
                c.a = 1;
                return c;
            }, 0.045f, new Color(0.13f, 0.09f, 0.07f, 1f));
            return Finish(NewTex(S, S), px, S / 0.96f);
        });

        public static Sprite Drone => Cached("drone", () =>
        {
            const int S = 320;
            var poly = new Vector2[6];
            for (int i = 0; i < 6; i++) { float a = i / 6f * Mathf.PI * 2f; poly[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.7f) * 0.95f; }
            float pw = 2f / S;
            var px = PolyPixels(S, poly, (uv, depth) =>
            {
                float d = uv.magnitude;
                var body = new Color(0.82f, 0.16f, 0.22f) * (0.78f + 0.25f * uv.y);
                body += new Color(0.25f, 0.1f, 0.1f) * Mathf.Clamp01(1f - depth / 0.1f) * Mathf.Clamp01(uv.y + 0.3f); // canto con luz
                body *= 1f - 0.35f * Cov(0.012f - Mathf.Abs(depth - 0.16f), pw);                                       // panel
                var eyeRing = new Color(0.18f, 0.04f, 0.05f);
                var eye = Color.Lerp(new Color(1f, 0.75f, 0.15f), new Color(1f, 1f, 0.8f), Mathf.Clamp01(1f - d / 0.18f));
                if ((uv - new Vector2(-0.07f, 0.07f)).magnitude < 0.06f) eye = Color.white;                             // brillo del ojo
                var c = Color.Lerp(body, eyeRing, Cov(0.36f - d, pw));
                c = Color.Lerp(c, eye, Cov(0.26f - d, pw));
                c.a = 1;
                return c;
            }, 0.05f, new Color(0.16f, 0.03f, 0.05f, 1f));
            return Finish(NewTex(S, S), px, S / 0.96f);
        });

        public static Sprite Mine => Cached("mine", () =>
        {
            const int S = 320;
            int spikes = 10;
            var poly = new Vector2[spikes * 2];
            for (int i = 0; i < spikes * 2; i++)
            {
                float a = i / (float)(spikes * 2) * Mathf.PI * 2f;
                float r = i % 2 == 0 ? 0.98f : 0.62f;
                poly[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
            }
            float pw = 2f / S;
            var px = PolyPixels(S, poly, (uv, depth) =>
            {
                float d = uv.magnitude;
                var metal = new Color(0.3f, 0.31f, 0.37f) * (0.78f + 0.35f * uv.y);
                metal += new Color(0.2f, 0.2f, 0.24f) * Mathf.Clamp01(1f - depth / 0.08f) * Mathf.Clamp01(uv.y - uv.x + 0.4f);
                metal *= 1f - 0.3f * Cov(0.012f - Mathf.Abs(d - 0.42f), pw);                                            // aro
                var core = Color.Lerp(new Color(1f, 0.15f, 0.15f), new Color(1f, 0.85f, 0.8f), Mathf.Clamp01(1f - d / 0.12f));
                var c = Color.Lerp(metal, new Color(0.12f, 0.02f, 0.03f), Cov(0.25f - d, pw));
                c = Color.Lerp(c, core, Cov(0.2f - d, pw));
                c.a = 1;
                return c;
            }, 0.045f, new Color(0.06f, 0.06f, 0.09f, 1f));
            return Finish(NewTex(S, S), px, S / 0.96f);
        });

        /// <summary>Barra metálica con franjas de peligro (indestructible). 2 x 0,5 unidades, igual que antes.</summary>
        public static Sprite Wall => Cached("wall", () =>
        {
            const int W = 512, H = 128;
            const float B = 12f; // borde metálico
            var px = new Color[W * H];
            var yellow = new Color(0.97f, 0.77f, 0.1f);
            var dark = new Color(0.13f, 0.13f, 0.16f);
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float fx = x + 0.5f, fy = y + 0.5f;
                    float edge = Mathf.Min(Mathf.Min(fx, W - fx), Mathf.Min(fy, H - fy));
                    // franjas diagonales con borde suave
                    float u = (fx + fy) / 44f;
                    float f = u - Mathf.Floor(u);
                    float stripe = Mathf.Clamp01((Mathf.Abs(f - 0.5f) - 0.25f) * 44f + 0.5f);
                    var inner = Color.Lerp(dark, yellow, stripe);
                    inner *= 0.88f + 0.18f * (fy / H);                                             // luz desde arriba
                    // borde biselado: claro arriba, oscuro abajo, con remaches en las puntas
                    float k = fy / H;
                    var rim = Color.Lerp(new Color(0.38f, 0.42f, 0.5f), new Color(0.78f, 0.82f, 0.9f), k);
                    var c = Color.Lerp(rim, inner, Mathf.Clamp01(edge - B + 0.5f));
                    var c1 = new Vector2(B * 2.2f, H / 2f); var c2 = new Vector2(W - B * 2.2f, H / 2f);
                    float rv = Mathf.Min((new Vector2(fx, fy) - c1).magnitude, (new Vector2(fx, fy) - c2).magnitude);
                    c = Color.Lerp(c, new Color(0.75f, 0.78f, 0.85f), Mathf.Clamp01(9f - rv + 0.5f) * 0.9f);
                    c = Color.Lerp(new Color(0.08f, 0.08f, 0.1f), c, Mathf.Clamp01(edge - 3f + 0.5f)); // contorno oscuro
                    c.a = Mathf.Clamp01(edge + 0.5f);
                    px[y * W + x] = c;
                }
            return Finish(NewTex(W, H), px, 256);
        });

        public static Sprite Laser => Cached("laser", () =>
        {
            const int W = 256, H = 64; // 1 x 0,25 unidades, igual que antes
            var px = new Color[W * H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float d = Mathf.Abs(y + 0.5f - H / 2f) / (H / 2f);
                    px[y * W + x] = Color.Lerp(Color.white, new Color(1f, 0.1f, 0.3f, 0f), d * d * 1.2f);
                }
            return Finish(NewTex(W, H), px, 256);
        });
    }
}
