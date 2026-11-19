using UnityEngine;

namespace AeroBlitz
{
    /// <summary>
    /// Arte de las naves, generado por código a partir del polígono de cada nave:
    /// - Cartoon (naves normales): contorno grueso y dos tonos; se tiñe con la pintura.
    /// - Metálico (premium): degradado brillante, reflejo y paneles; se tiñe con su tono metálico o la pintura.
    /// - Detalle: franjas del color de la nave y, en las premium, borde dorado (no se tiñe).
    /// - Neón (skin opcional): casco oscuro + contorno luminoso que se tiñe con el color de la pintura.
    /// </summary>
    public static partial class SpriteFactory
    {
        public enum ShipLayer { Cartoon, Metal, Detail, DetailMetal, NeonBase, NeonGlow }

        const int ShipRes = 320;
        const float ShipPPU = 250f;          // 320 / 250 = 1,28 unidades: mismo tamaño que antes, más nítidas en el menú y el hangar
        public const float ShipScale = 0.86f; // deja margen para el contorno y el brillo neón
        /// <summary>Unidades de mundo por unidad del polígono (para colocar cabina, fuego y luces).</summary>
        public const float ShipUnit = ShipRes / ShipPPU / 2f * ShipScale;

        static readonly Color Ink = new Color(0.1f, 0.07f, 0.2f, 1f);
        static readonly Color MetalInk = new Color(0.07f, 0.08f, 0.12f, 1f);
        static readonly Color GoldRim = new Color(1f, 0.82f, 0.25f, 1f);

        /// <summary>Casco con el estilo de serie (metálico en las premium, cartoon en las demás).</summary>
        public static Sprite Ship(ShipDefinition def) => ShipHull(def, ShipSkins.Default(def));

        public static Sprite ShipHull(ShipDefinition def, ShipSkin skin) =>
            ShipSprite(def, skin == ShipSkin.Metal ? ShipLayer.Metal : skin == ShipSkin.Neon ? ShipLayer.NeonBase : ShipLayer.Cartoon);

        /// <summary>Capa de encima: franjas (+ borde dorado en las premium) o el brillo neón (que se tiñe).</summary>
        public static Sprite ShipDetail(ShipDefinition def, ShipSkin skin) =>
            ShipSprite(def, skin == ShipSkin.Metal ? ShipLayer.DetailMetal : skin == ShipSkin.Neon ? ShipLayer.NeonGlow : ShipLayer.Detail);

        static Sprite ShipSprite(ShipDefinition def, ShipLayer layer) =>
            Cached("ship_" + layer + "_" + def.id, () => Finish(NewTex(ShipRes, ShipRes), ShipPixels(def, layer, ShipRes), ShipPPU));

        /// <summary>Cabina con contorno, degradado y destello (64 px; 0,22 unidades de alto).</summary>
        public static Sprite Cockpit => Cached("cockpit_v2", () => Finish(NewTex(128, 128), CockpitPixels(128), 580f));

        // ---------------- GEOMETRÍA ----------------
        static float SegDist(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(1e-6f, ab.sqrMagnitude));
            return Vector2.Distance(p, a + ab * t);
        }

        static float EdgeDist(Vector2 p, Vector2[] poly)
        {
            float d = float.MaxValue;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++) d = Mathf.Min(d, SegDist(p, poly[j], poly[i]));
            return d;
        }

        static float Cov(float v, float px) => Mathf.Clamp01(v / px + 0.5f); // cobertura con antialias

        static readonly Vector2[] Chevron = { new Vector2(0, 0.7f), new Vector2(0.18f, 0.3f), new Vector2(0, 0.4f), new Vector2(-0.18f, 0.3f) };

        static float StripeDist(Vector2 p) =>
            Mathf.Min(SegDist(p, new Vector2(0.36f, -0.08f), new Vector2(0.85f, -0.45f)), SegDist(p, new Vector2(-0.36f, -0.08f), new Vector2(-0.85f, -0.45f)));

        static Color Over(Color under, Color top) // mezcla "normal" con alfa
        {
            float a = top.a + under.a * (1 - top.a);
            if (a <= 0) return new Color(0, 0, 0, 0);
            var c = (top * top.a + under * under.a * (1 - top.a)) / a;
            c.a = a;
            return c;
        }

        /// <summary>Píxeles de una capa de la nave (público para poder exportar vistas previas).</summary>
        public static Color[] ShipPixels(ShipDefinition def, ShipLayer layer, int S)
        {
            var poly = def.shape;
            var px = new Color[S * S];
            float pw = 2f / S / ShipScale; // tamaño de un píxel en unidades del polígono
            bool metal = layer == ShipLayer.Metal || layer == ShipLayer.DetailMetal; // contorno fino del metálico
            float inkIn = metal ? 0.022f : 0.035f, inkOut = metal ? 0.03f : 0.05f;

            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    var p = new Vector2(((x + 0.5f) / S * 2f - 1f) / ShipScale, ((y + 0.5f) / S * 2f - 1f) / ShipScale);
                    float d = EdgeDist(p, poly);
                    bool inside = InPoly(p, poly);
                    float sd = inside ? -d : d; // negativo dentro
                    Color c = new Color(0, 0, 0, 0);

                    switch (layer)
                    {
                        case ShipLayer.Cartoon:
                        case ShipLayer.Metal:
                        {
                            var ink = layer == ShipLayer.Metal ? MetalInk : Ink;
                            if (!inside)
                            {
                                if (sd < inkOut + pw) { c = ink; c.a = Cov(inkOut - sd, pw); }
                                break;
                            }
                            float v = layer == ShipLayer.Metal ? MetalShade(p, -sd, pw) : CartoonShade(p, pw);
                            c = Color.Lerp(ink, new Color(v, v, v, 1), Cov(-sd - inkIn, pw));
                            c.a = 1;
                            break;
                        }
                        case ShipLayer.Detail:
                        case ShipLayer.DetailMetal:
                        {
                            if (!inside || -sd < inkIn) break;
                            float inner = Cov(-sd - inkIn, pw);
                            var acc = def.accent; acc.a = 1;
                            float stripe = Cov((metal ? 0.04f : 0.05f) - StripeDist(p), pw);
                            float chev = InPoly(p, Chevron) ? Cov(EdgeDist(p, Chevron), pw) : 0f;
                            float a = Mathf.Max(stripe, chev) * inner;
                            if (a > 0) { c = acc; c.a = a; }
                            if (def.IsPremium) // borde dorado: marca de las premium con cualquier estilo
                            {
                                float rim = Cov(-sd - inkIn, pw) * Cov(inkIn + 0.035f + sd, pw);
                                if (rim > 0) c = Over(c, new Color(GoldRim.r, GoldRim.g, GoldRim.b, rim));
                            }
                            break;
                        }
                        case ShipLayer.NeonBase:
                        {
                            if (!inside) break;
                            c = new Color(0.055f, 0.07f, 0.14f, Cov(-sd, pw));
                            if (Mathf.Abs(p.x) < 0.012f) c = Color.Lerp(c, new Color(0.12f, 0.16f, 0.3f, c.a), 0.8f);
                            break;
                        }
                        case ShipLayer.NeonGlow:
                        {
                            // Se tiñe con el color de la pintura: aquí todo es blanco con distinta opacidad
                            float a = 0f;
                            if (sd > 0) a = Mathf.Max(a, 0.6f * Mathf.Exp(-sd / 0.06f));            // brillo exterior
                            a = Mathf.Max(a, Cov(0.022f - Mathf.Abs(sd), pw));                       // línea del borde
                            if (inside)
                            {
                                a = Mathf.Max(a, 0.35f * Mathf.Exp(sd / 0.05f));                     // brillo interior
                                a = Mathf.Max(a, 0.85f * Cov(0.012f - StripeDist(p), pw));           // franjas
                                a = Mathf.Max(a, 0.85f * Cov(0.012f - EdgeDist(p, Chevron), pw));    // chevrón
                                a = Mathf.Max(a, 0.45f * Cov(0.008f - Mathf.Abs(p.x), pw) * (p.y < 0.7f ? 1f : 0f)); // línea central
                            }
                            if (a > 0.003f) c = new Color(1, 1, 1, Mathf.Clamp01(a));
                            break;
                        }
                    }
                    px[y * S + x] = c;
                }
            return px;
        }

        /// <summary>Dos tonos con bandas diagonales: claro a la izquierda, sombra a la derecha (como el icono).</summary>
        static float CartoonShade(Vector2 p, float pw)
        {
            float k = (p.y + 1.2f) / 2.4f;
            float xb = -0.22f + k * 0.24f, xs = 0.08f + k * 0.22f;
            float v = Mathf.Lerp(1f, 0.84f, Cov(p.x - xb, pw));
            return Mathf.Lerp(v, 0.64f, Cov(p.x - xs, pw));
        }

        /// <summary>Metal pulido: centro brillante, bordes oscuros, reflejo diagonal y líneas de panel.</summary>
        static float MetalShade(Vector2 p, float depth, float pw)
        {
            float r = Vector2.Distance(p, new Vector2(-0.1f, 0.2f)) / 1.2f;
            float v = Mathf.Lerp(1f, 0.42f, Mathf.SmoothStep(0f, 1f, r));
            v *= Mathf.Lerp(0.72f, 1f, Mathf.Clamp01(depth / 0.22f));                 // bordes más oscuros
            float spec = Mathf.Abs(p.x - (-0.28f + 0.35f * p.y));                        // reflejo diagonal
            v += 0.3f * Mathf.Exp(-spec * spec / 0.004f);
            v += 0.12f * Mathf.Exp(-(p.x - 0.42f - 0.2f * p.y) * (p.x - 0.42f - 0.2f * p.y) / 0.002f);
            v += 0.015f * Mathf.Sin(p.y * 70f);                                          // pulido fino
            v += 0.12f * Cov(0.012f - Mathf.Abs(p.x), pw);                               // línea central brillante
            float panel = Mathf.Min(SegDist(p, new Vector2(0.12f, -0.05f), new Vector2(0.62f, 0.45f)),
                                    SegDist(p, new Vector2(-0.12f, -0.05f), new Vector2(-0.62f, 0.45f)));
            v -= 0.18f * Cov(0.011f - panel, pw);                                        // paneles
            return Mathf.Clamp01(v);
        }

        public static Color[] CockpitPixels(int S)
        {
            var px = new Color[S * S];
            float pw = 2f / S;
            const float rx = 0.55f, ry = 0.98f;
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float u = (x + 0.5f) / S * 2f - 1f, v = (y + 0.5f) / S * 2f - 1f;
                    float q = Mathf.Sqrt(u * u / (rx * rx) + v * v / (ry * ry));
                    if (q > 1f + pw * 2f) continue;
                    float edge = (1f - q) * rx; // distancia aproximada al borde
                    var fill = Color.Lerp(new Color(0.08f, 0.2f, 0.45f), new Color(0.55f, 0.95f, 1f), (v + 1f) / 2f);
                    var c = Color.Lerp(Ink, fill, Cov(edge - 0.14f, pw));
                    float gq = Mathf.Sqrt((u + 0.18f) * (u + 0.18f) / 0.0144f + (v - 0.35f) * (v - 0.35f) / 0.0484f);
                    if (gq < 1f) c = Color.Lerp(c, Color.white, 0.75f * Cov(1f - gq, 0.25f));
                    c.a = Cov(edge, pw);
                    px[y * S + x] = c;
                }
            return px;
        }
    }

    public enum ShipSkin { Cartoon, Metal, Neon }

    /// <summary>
    /// Estilos de nave. Cualquier nave puede llevar cartoon o neón; el metálico es solo de las premium.
    /// El de serie es gratis (cartoon en las normales, metálico en las premium); los demás se compran con monedas, nave por nave.
    /// </summary>
    public static class ShipSkins
    {
        public static readonly ShipSkin[] All = { ShipSkin.Cartoon, ShipSkin.Metal, ShipSkin.Neon };

        public static string Id(ShipSkin s) => s == ShipSkin.Metal ? "metal" : s == ShipSkin.Neon ? "neon" : "cartoon";
        public static string Name(ShipSkin s) => L.T("skin." + Id(s));
        public static ShipSkin Default(ShipDefinition def) => def.IsPremium ? ShipSkin.Metal : ShipSkin.Cartoon;
        public static bool Available(ShipDefinition def, ShipSkin s) => s != ShipSkin.Metal || def.IsPremium;

        public static int Price(ShipDefinition def, ShipSkin s) =>
            s == Default(def) ? 0 : s == ShipSkin.Neon ? GameConfig.NeonSkinPrice : s == ShipSkin.Cartoon ? GameConfig.CartoonSkinPrice : 0;

        public static bool Owns(ShipState st, ShipDefinition def, ShipSkin s) =>
            Available(def, s) && (s == Default(def) || (st != null && st.ownedSkins != null && st.ownedSkins.Contains(Id(s))));

        /// <summary>El estilo que lleva puesto (si no lo tiene o no vale para esa nave, el de serie).</summary>
        public static ShipSkin Current(ShipState st, ShipDefinition def)
        {
            if (st == null || string.IsNullOrEmpty(st.skin)) return Default(def);
            foreach (var s in All) if (Id(s) == st.skin && Owns(st, def, s)) return s;
            return Default(def);
        }
    }

    /// <summary>Qué sprites y colores usa una nave según su estilo y pintura (juego, menú y hangar).</summary>
    public struct ShipLook
    {
        public Sprite hull, detail;
        public Color hullColor, detailColor;
        public bool neon;

        public static ShipLook Of(ShipDefinition def, ShipState state, ShipSkin? forceSkin = null)
        {
            var skin = forceSkin ?? ShipSkins.Current(state, def);
            var paint = UpgradeCatalog.GetPaint(state != null ? state.paintId : "default");
            bool standard = paint.id == "default";
            if (skin == ShipSkin.Neon)
            {
                var glow = standard ? def.accent : paint.color; // el neón brilla con el color de la pintura
                glow.a = 1;
                return new ShipLook { hull = SpriteFactory.ShipHull(def, skin), hullColor = Color.white, detail = SpriteFactory.ShipDetail(def, skin), detailColor = glow, neon = true };
            }
            var hc = standard && skin == ShipSkin.Metal ? def.metalColor : paint.color; // metálico: cobre, acero o cromo
            hc.a = 1;
            return new ShipLook { hull = SpriteFactory.ShipHull(def, skin), hullColor = hc, detail = SpriteFactory.ShipDetail(def, skin), detailColor = Color.white };
        }
    }
}
