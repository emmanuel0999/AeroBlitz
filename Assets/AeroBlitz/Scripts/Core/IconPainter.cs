using System.Collections.Generic;
using UnityEngine;

namespace AeroBlitz
{
    /// <summary>
    /// Dibuja por código los iconos de los artículos (tienda, hangar y pruebas en partida).
    /// Cada icono es un círculo de color con un símbolo. Sin texto, para que sirva también
    /// como icono de producto en Google Play (512 px).
    /// </summary>
    public static class IconPainter
    {
        delegate bool Mask(Vector2 p);

        struct Layer
        {
            public Mask mask;
            public Color color;
        }

        // ---------- Claves de icono ----------
        public static string KeyForProduct(string productId)
        {
            switch (productId)
            {
                case ProductIds.Lives5: return "lives1";
                case ProductIds.Lives15: return "lives2";
                case ProductIds.CoinsSmall: return "coins1";
                case ProductIds.CoinsMedium: return "coins2";
                case ProductIds.CoinsLarge: return "coins3";
                case ProductIds.StarterPack: return "gift";
                case ProductIds.RemoveAds: return "noads";
                case ProductIds.VipMonthly: return "vip";
            }
            var ship = ShipCatalog.ByProduct(productId);
            if (ship != null) return "ship:" + ship.id;
            var eq = UpgradeCatalog.EquipmentByProduct(productId);
            if (eq != null) return "eq:" + eq.id;
            var paint = UpgradeCatalog.PaintByProduct(productId);
            if (paint != null) return "paint:" + paint.id;
            return "gift";
        }

        // ---------- Pintado ----------
        public static Color[] Paint(string key, int size)
        {
            var layers = new List<Layer>();
            Color bg = Build(key, layers);
            int ss = size > 160 ? 3 : 2; // supermuestreo
            var px = new Color[size * size];
            Color bgDark = bg * 0.5f; bgDark.a = 1f;
            Color rim = Color.Lerp(bg, Color.white, 0.35f);

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    Vector2 center = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f);
                    float d = center.magnitude;
                    if (d > 1.02f) { px[y * size + x] = new Color(0, 0, 0, 0); continue; }

                    Color c = Color.Lerp(bgDark, bg, Mathf.Clamp01((center.y + 1f) * 0.55f));
                    if (d > 0.9f) c = Color.Lerp(c, rim, 0.75f);

                    foreach (var l in layers)
                    {
                        int hits = 0;
                        for (int sy = 0; sy < ss; sy++)
                            for (int sx = 0; sx < ss; sx++)
                            {
                                var q = new Vector2((x + (sx + 0.5f) / ss) / size * 2f - 1f, (y + (sy + 0.5f) / ss) / size * 2f - 1f);
                                if (l.mask(q)) hits++;
                            }
                        if (hits > 0) c = Color.Lerp(c, l.color, l.color.a * hits / (float)(ss * ss));
                    }

                    // borde del círculo con antialias
                    float edge = Mathf.Clamp01((1f - d) * size * 0.5f + 0.5f);
                    c.a = edge;
                    px[y * size + x] = c;
                }
            return px;
        }

        static void Add(List<Layer> l, Mask m, Color c) => l.Add(new Layer { mask = m, color = c });

        // ---------- Formas básicas ----------
        static Mask Circle(Vector2 c, float r) => p => (p - c).sqrMagnitude < r * r;
        static Mask Ring(Vector2 c, float r0, float r1) => p => { float d = (p - c).magnitude; return d > r0 && d < r1; };

        static Mask Box(Vector2 c, Vector2 half, float angleDeg = 0f)
        {
            float a = -angleDeg * Mathf.Deg2Rad, cs = Mathf.Cos(a), sn = Mathf.Sin(a);
            return p =>
            {
                var q = p - c;
                var r = new Vector2(q.x * cs - q.y * sn, q.x * sn + q.y * cs);
                return Mathf.Abs(r.x) < half.x && Mathf.Abs(r.y) < half.y;
            };
        }

        static Mask EllipseRing(Vector2 c, float rx, float ry, float thickness, float angleDeg)
        {
            float a = -angleDeg * Mathf.Deg2Rad, cs = Mathf.Cos(a), sn = Mathf.Sin(a);
            return p =>
            {
                var q = p - c;
                var r = new Vector2(q.x * cs - q.y * sn, q.x * sn + q.y * cs);
                float v = (r.x * r.x) / (rx * rx) + (r.y * r.y) / (ry * ry);
                return v < 1f && v > (1f - thickness) * (1f - thickness);
            };
        }

        static Mask Poly(params Vector2[] pts) => p => InPoly(p, pts);

        static Mask Rotated(Mask m, Vector2 pivot, float angleDeg)
        {
            float a = -angleDeg * Mathf.Deg2Rad, cs = Mathf.Cos(a), sn = Mathf.Sin(a);
            return p =>
            {
                var q = p - pivot;
                return m(pivot + new Vector2(q.x * cs - q.y * sn, q.x * sn + q.y * cs));
            };
        }

        static Mask Heart(Vector2 c, float s) => p =>
        {
            float x = (p.x - c.x) / s, y = (p.y - c.y) / s;
            float a = x * x + y * y - 1f;
            return a * a * a - x * x * y * y * y <= 0f;
        };

        static bool InPoly(Vector2 p, Vector2[] poly)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
                if ((poly[i].y > p.y) != (poly[j].y > p.y) &&
                    p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x)
                    inside = !inside;
            return inside;
        }

        static Vector2 V(float x, float y) => new Vector2(x, y);
        static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);

        // ---------- Símbolos compuestos ----------
        static void Coins(List<Layer> l, int count)
        {
            Vector2[] pos = count == 1 ? new[] { V(0, 0) }
                : count == 2 ? new[] { V(-0.17f, -0.12f), V(0.17f, 0.12f) }
                : new[] { V(-0.26f, -0.18f), V(0.26f, -0.18f), V(0, 0.22f) };
            float r = count == 1 ? 0.4f : count == 2 ? 0.33f : 0.29f;
            foreach (var c in pos)
            {
                Add(l, Circle(c, r), C(0.72f, 0.45f, 0.06f));
                Add(l, Circle(c, r * 0.8f), C(1f, 0.82f, 0.28f));
                Add(l, Ring(c, r * 0.5f, r * 0.58f), C(0.85f, 0.6f, 0.12f));
                Add(l, Circle(c + V(-r * 0.32f, r * 0.34f), r * 0.16f), C(1f, 1f, 0.9f, 0.85f));
            }
        }

        static void Hearts(List<Layer> l, int count)
        {
            if (count > 1) Add(l, Heart(V(0.2f, 0.12f), 0.3f), C(1f, 0.62f, 0.7f));
            Add(l, Heart(V(count > 1 ? -0.08f : 0f, count > 1 ? -0.08f : 0.0f), 0.42f), C(1f, 0.28f, 0.38f));
            Add(l, Circle(V(count > 1 ? -0.28f : -0.2f, count > 1 ? 0.05f : 0.18f), 0.07f), C(1f, 1f, 1f, 0.7f));
        }

        static void Gift(List<Layer> l)
        {
            Add(l, Box(V(0, -0.14f), V(0.4f, 0.3f)), C(0.95f, 0.35f, 0.45f));
            Add(l, Box(V(0, 0.2f), V(0.47f, 0.09f)), C(0.8f, 0.22f, 0.32f));
            Add(l, Box(V(0, -0.05f), V(0.07f, 0.4f)), C(1f, 0.85f, 0.3f));
            Add(l, Circle(V(-0.12f, 0.38f), 0.11f), C(1f, 0.85f, 0.3f));
            Add(l, Circle(V(0.12f, 0.38f), 0.11f), C(1f, 0.85f, 0.3f));
        }

        static void PlayBadge(List<Layer> l)
        {
            Add(l, Circle(V(0.48f, -0.48f), 0.24f), C(0.1f, 0.12f, 0.2f));
            Add(l, Circle(V(0.48f, -0.48f), 0.2f), C(0.3f, 0.85f, 0.4f));
            Add(l, Poly(V(0.42f, -0.36f), V(0.6f, -0.48f), V(0.42f, -0.6f)), Color.white);
        }

        static void Bullets(List<Layer> l, float[] angles, Color body, Color tip)
        {
            // Los proyectiles salen de un punto bajo y se separan hacia arriba (abanico)
            var basePt = V(0, -0.62f);
            foreach (float a in angles)
            {
                Add(l, Rotated(Box(V(0, 0.02f), V(0.07f, 0.2f)), basePt, a), body);
                Add(l, Rotated(Poly(V(0, 0.42f), V(0.07f, 0.22f), V(-0.07f, 0.22f)), basePt, a), tip);
            }
        }

        static void Flame(List<Layer> l)
        {
            Add(l, Poly(V(0, 0.58f), V(0.32f, 0.02f), V(0.22f, -0.46f), V(0, -0.3f), V(-0.22f, -0.46f), V(-0.32f, 0.02f)), C(1f, 0.45f, 0.1f));
            Add(l, Poly(V(0, 0.3f), V(0.17f, -0.02f), V(0.11f, -0.3f), V(0, -0.2f), V(-0.11f, -0.3f), V(-0.17f, -0.02f)), C(1f, 0.92f, 0.45f));
        }

        static void Bolt(List<Layer> l, Color c) =>
            Add(l, Poly(V(0.12f, 0.58f), V(-0.32f, -0.02f), V(-0.03f, -0.02f), V(-0.14f, -0.58f), V(0.32f, 0.06f), V(0.03f, 0.06f)), c);

        static void Hexagon(List<Layer> l, float r, Color outer, Color inner)
        {
            var hex = new Vector2[6];
            var hexIn = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float a = (90f + i * 60f) * Mathf.Deg2Rad;
                hex[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
                hexIn[i] = hex[i] * 0.72f;
            }
            Add(l, Poly(hex), outer);
            Add(l, Poly(hexIn), inner);
            for (int i = 0; i < 6; i++) Add(l, Circle(hex[i] * 0.86f, 0.04f), C(0.35f, 0.38f, 0.45f));
        }

        static void Shield(List<Layer> l, Color c)
        {
            Add(l, Poly(V(0, 0.56f), V(0.44f, 0.38f), V(0.39f, -0.1f), V(0, -0.56f), V(-0.39f, -0.1f), V(-0.44f, 0.38f)), c);
            Add(l, Poly(V(0, 0.42f), V(0.3f, 0.29f), V(0.26f, -0.06f), V(0, -0.4f)), C(1f, 1f, 1f, 0.35f));
        }

        static void Missile(List<Layer> l, float angle)
        {
            var pivot = V(0, 0);
            Add(l, Rotated(Poly(V(0, -0.62f), V(0.08f, -0.34f), V(-0.08f, -0.34f)), pivot, angle), C(1f, 0.6f, 0.15f));
            Add(l, Rotated(Poly(V(-0.1f, -0.08f), V(-0.27f, -0.36f), V(-0.1f, -0.32f)), pivot, angle), C(0.9f, 0.2f, 0.2f));
            Add(l, Rotated(Poly(V(0.1f, -0.08f), V(0.27f, -0.36f), V(0.1f, -0.32f)), pivot, angle), C(0.9f, 0.2f, 0.2f));
            Add(l, Rotated(Box(V(0, 0f), V(0.1f, 0.33f)), pivot, angle), C(0.93f, 0.94f, 0.98f));
            Add(l, Rotated(Poly(V(0, 0.6f), V(0.1f, 0.32f), V(-0.1f, 0.32f)), pivot, angle), C(0.9f, 0.2f, 0.2f));
            Add(l, Rotated(Box(V(0, 0.12f), V(0.1f, 0.03f)), pivot, angle), C(0.9f, 0.2f, 0.2f));
        }

        // ---------- Catálogo de iconos ----------
        static Color Build(string key, List<Layer> l)
        {
            if (key.StartsWith("ship:"))
            {
                var def = ShipCatalog.Get(key.Substring(5));
                var pts = new Vector2[def.shape.Length];
                for (int i = 0; i < pts.Length; i++) pts[i] = def.shape[i] * 0.62f + V(0, -0.02f);
                Add(l, Poly(pts), C(0.9f, 0.93f, 1f));
                Add(l, Circle(V(0, 0.12f), 0.08f), def.accent.a > 0 ? def.accent : C(0.4f, 0.8f, 1f));
                return C(0.08f, 0.2f, 0.45f);
            }
            if (key.StartsWith("paint:"))
            {
                var paint = UpgradeCatalog.GetPaint(key.Substring(6));
                Add(l, Circle(V(0, -0.12f), 0.32f), paint.color);
                Add(l, Poly(V(0, 0.52f), V(0.28f, -0.02f), V(-0.28f, -0.02f)), paint.color);
                Add(l, Circle(V(-0.12f, -0.02f), 0.08f), C(1f, 1f, 1f, 0.55f));
                return C(0.14f, 0.14f, 0.24f);
            }
            if (key.StartsWith("up:"))
            {
                switch (key.Substring(3))
                {
                    case nameof(UpgradeStat.Armor): Hexagon(l, 0.48f, C(0.7f, 0.76f, 0.85f), C(0.5f, 0.56f, 0.66f)); return C(0.18f, 0.24f, 0.36f);
                    case nameof(UpgradeStat.Engine): Flame(l); return C(0.32f, 0.12f, 0.05f);
                    case nameof(UpgradeStat.Magazine):
                        foreach (float x in new[] { -0.14f, 0.14f })
                        {
                            Add(l, Box(V(x, -0.1f), V(0.09f, 0.28f)), C(1f, 0.78f, 0.32f));
                            Add(l, Poly(V(x, 0.4f), V(x + 0.09f, 0.18f), V(x - 0.09f, 0.18f)), C(0.85f, 0.55f, 0.2f));
                        }
                        Add(l, Box(V(0.36f, 0.3f), V(0.12f, 0.035f)), C(0.45f, 1f, 0.55f));
                        Add(l, Box(V(0.36f, 0.3f), V(0.035f, 0.12f)), C(0.45f, 1f, 0.55f));
                        return C(0.25f, 0.2f, 0.08f);
                    default: Bolt(l, C(1f, 0.85f, 0.25f)); return C(0.25f, 0.2f, 0.05f);
                }
            }

            switch (key)
            {
                case "coins1": Coins(l, 1); return C(0.36f, 0.22f, 0.05f);
                case "coins2": Coins(l, 2); return C(0.36f, 0.22f, 0.05f);
                case "coins3": Coins(l, 3); return C(0.4f, 0.24f, 0.04f);
                case "lives1": Hearts(l, 1); return C(0.38f, 0.06f, 0.14f);
                case "lives2": Hearts(l, 2); return C(0.38f, 0.06f, 0.14f);
                case "gift": Gift(l); return C(0.32f, 0.12f, 0.48f);
                case "daily": Gift(l); return C(0.42f, 0.3f, 0.06f);
                case "noads":
                    Add(l, Box(V(0, 0.02f), V(0.46f, 0.32f)), C(0.88f, 0.9f, 1f));
                    Add(l, Box(V(0, 0.02f), V(0.39f, 0.25f)), C(0.12f, 0.14f, 0.28f));
                    Add(l, Poly(V(-0.12f, 0.14f), V(0.14f, 0.02f), V(-0.12f, -0.1f)), C(0.88f, 0.9f, 1f));
                    Add(l, Box(V(0, 0), V(0.62f, 0.07f), -40f), C(1f, 0.3f, 0.3f));
                    return C(0.16f, 0.16f, 0.32f);
                case "vip":
                    Add(l, Poly(V(-0.5f, -0.28f), V(-0.52f, 0.24f), V(-0.24f, 0.02f), V(0, 0.4f), V(0.24f, 0.02f), V(0.52f, 0.24f), V(0.5f, -0.28f)), C(1f, 0.82f, 0.25f));
                    Add(l, Box(V(0, -0.36f), V(0.5f, 0.07f)), C(0.9f, 0.65f, 0.15f));
                    Add(l, Circle(V(0, -0.08f), 0.08f), C(0.4f, 0.85f, 1f));
                    Add(l, Circle(V(-0.3f, -0.12f), 0.06f), C(1f, 0.35f, 0.45f));
                    Add(l, Circle(V(0.3f, -0.12f), 0.06f), C(1f, 0.35f, 0.45f));
                    return C(0.34f, 0.12f, 0.52f);
                case "free_life": Hearts(l, 1); PlayBadge(l); return C(0.08f, 0.32f, 0.16f);
                case "free_coins": Coins(l, 2); PlayBadge(l); return C(0.08f, 0.32f, 0.16f);
                case "free_upgrade":
                    Add(l, Poly(V(0, 0.55f), V(0.4f, 0.1f), V(0.16f, 0.1f), V(0.16f, -0.45f), V(-0.16f, -0.45f), V(-0.16f, 0.1f), V(-0.4f, 0.1f)), C(0.45f, 1f, 0.55f));
                    PlayBadge(l);
                    return C(0.08f, 0.32f, 0.16f);

                // ---- Equipamiento ----
                case "eq:eq_shield": Shield(l, C(0.5f, 0.82f, 1f)); return C(0.06f, 0.24f, 0.42f);
                case "eq:eq_twin":
                    Add(l, Box(V(-0.15f, -0.05f), V(0.07f, 0.3f)), C(0.55f, 1f, 1f));
                    Add(l, Box(V(0.15f, -0.05f), V(0.07f, 0.3f)), C(0.55f, 1f, 1f));
                    Add(l, Poly(V(-0.15f, 0.45f), V(-0.08f, 0.25f), V(-0.22f, 0.25f)), C(0.9f, 1f, 1f));
                    Add(l, Poly(V(0.15f, 0.45f), V(0.22f, 0.25f), V(0.08f, 0.25f)), C(0.9f, 1f, 1f));
                    return C(0.05f, 0.3f, 0.36f);
                case "eq:eq_triple": Bullets(l, new[] { -28f, 0f, 28f }, C(0.55f, 1f, 1f), C(0.9f, 1f, 1f)); return C(0.05f, 0.3f, 0.36f);
                case "eq:eq_plasma":
                    Bullets(l, new[] { -44f, -22f, 0f, 22f, 44f }, C(1f, 0.45f, 0.95f), C(1f, 0.85f, 1f));
                    Add(l, Circle(V(0, -0.42f), 0.15f), C(1f, 0.75f, 1f));
                    return C(0.3f, 0.06f, 0.36f);
                case "eq:eq_missiles": Missile(l, -35f); return C(0.36f, 0.1f, 0.06f);
                case "eq:eq_quantum":
                    Add(l, EllipseRing(V(0, 0), 0.52f, 0.2f, 0.22f, 35f), C(0.5f, 0.95f, 1f));
                    Add(l, EllipseRing(V(0, 0), 0.52f, 0.2f, 0.22f, -35f), C(0.5f, 0.95f, 1f));
                    Add(l, Circle(V(0, 0), 0.14f), C(0.85f, 1f, 1f));
                    return C(0.08f, 0.16f, 0.36f);
                case "eq:eq_titanium": Hexagon(l, 0.5f, C(0.72f, 0.77f, 0.86f), C(0.52f, 0.58f, 0.68f)); return C(0.16f, 0.2f, 0.3f);
                case "eq:eq_thrusters": Flame(l); return C(0.3f, 0.1f, 0.04f);
                case "eq:eq_plates":
                    Add(l, Box(V(0, 0), V(0.42f, 0.34f)), C(0.72f, 0.75f, 0.82f));
                    Add(l, Box(V(0, 0.11f), V(0.42f, 0.025f)), C(0.45f, 0.48f, 0.55f));
                    Add(l, Box(V(0, -0.11f), V(0.42f, 0.025f)), C(0.45f, 0.48f, 0.55f));
                    foreach (var r in new[] { V(-0.34f, 0.26f), V(0.34f, 0.26f), V(-0.34f, -0.26f), V(0.34f, -0.26f) }) Add(l, Circle(r, 0.04f), C(0.4f, 0.42f, 0.5f));
                    return C(0.2f, 0.24f, 0.3f);
                case "eq:eq_magazine":
                    foreach (float x in new[] { -0.24f, 0f, 0.24f })
                    {
                        Add(l, Box(V(x, -0.12f), V(0.08f, 0.24f)), C(1f, 0.78f, 0.32f));
                        Add(l, Poly(V(x, 0.32f), V(x + 0.08f, 0.12f), V(x - 0.08f, 0.12f)), C(0.85f, 0.55f, 0.2f));
                    }
                    return C(0.24f, 0.2f, 0.1f);
            }
            Gift(l);
            return C(0.25f, 0.25f, 0.35f);
        }
    }
}
