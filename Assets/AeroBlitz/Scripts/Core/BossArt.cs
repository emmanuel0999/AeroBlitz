using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace AeroBlitz
{
    /// <summary>
    /// Arte de los jefes, generado por código a partir de sus piezas (BossDefinition.parts):
    ///  - Casco: piezas metálicas con contorno, canto biselado iluminado desde arriba a la izquierda y líneas de panel
    ///    (se tiñe con el tono del jefe).
    ///  - Luces: borde luminoso y luces con el centro al rojo blanco, ya pintadas con el color de acento.
    /// Cada pieza está definida solo en el lado derecho: la izquierda es su reflejo.
    ///
    /// A 896 px (unos 190 píxeles por unidad, más que la pantalla de un móvil Full HD) el cálculo tarda,
    /// así que se hace en segundo plano (PrepareBoss) mientras se juega el nivel anterior o mientras el jefe se acerca.
    /// Solo se guardan en memoria los sprites del jefe actual.
    /// </summary>
    public static partial class SpriteFactory
    {
        public const int BossRes = 1024;
        /// <summary>El contorno ocupa este fracción de la imagen; el resto es margen para el resplandor (así no se corta en el borde).</summary>
        const float BossScale = 0.78f;
        /// <summary>Proporción del ancho del jefe (BossDefinition.width) que mide el contorno en el mundo.</summary>
        const float BossWorldFill = 0.92f;
        /// <summary>Súbelo si cambia el dibujo de los jefes: así se descartan las imágenes guardadas en el teléfono.</summary>
        const int BossArtVersion = 4;

        class BossJob { public Color32[] px; public byte[] png; }
        static readonly Dictionary<string, Task<BossJob>> BossJobs = new Dictionary<string, Task<BossJob>>();
        static string _bossCacheDir;

        static string BossKey(BossDefinition d, bool lights) => (lights ? "boss_lights_" : "boss_hull_") + d.id;

        /// <summary>Empieza a dibujar el jefe en segundo plano (no hace nada si ya está listo o en marcha).</summary>
        public static void PrepareBoss(BossDefinition d)
        {
            if (d == null) return;
            foreach (bool lights in new[] { false, true })
            {
                string key = BossKey(d, lights);
                if (Cache.ContainsKey(key) || BossJobs.ContainsKey(key)) continue;
                bool l = lights;
                // La primera vez se dibuja (usando todos los núcleos) y se guarda en el teléfono; las siguientes solo se lee
                if (_bossCacheDir == null) _bossCacheDir = Path.Combine(Application.persistentDataPath, "bosscache");
                string dir = _bossCacheDir, file = Path.Combine(dir, $"{key}_v{BossArtVersion}_{BossRes}.png");
                BossJobs[key] = Task.Run(() =>
                {
                    try { if (File.Exists(file)) return new BossJob { png = File.ReadAllBytes(file) }; } catch { }
                    var px = BossPixels32(d, l, BossRes);
                    try
                    {
                        Directory.CreateDirectory(dir);
                        File.WriteAllBytes(file, ImageConversion.EncodeArrayToPNG(px, GraphicsFormat.R8G8B8A8_UNorm, (uint)BossRes, (uint)BossRes));
                    }
                    catch { } // sin espacio o sin permiso: simplemente no se guarda
                    return new BossJob { px = px };
                });
            }
        }

        /// <summary>Si el dibujo en segundo plano terminó, crea los sprites (en el hilo principal). Devuelve true cuando están listos.</summary>
        public static bool TryFinishBoss(BossDefinition d)
        {
            string hk = BossKey(d, false), lk = BossKey(d, true);
            if (Cache.ContainsKey(hk) && Cache.ContainsKey(lk)) return true;
            PrepareBoss(d);
            if (!BossJobs[hk].IsCompleted || !BossJobs[lk].IsCompleted) return false;
            ReleaseBossesExcept(d.id);
            foreach (var key in new[] { hk, lk })
            {
                var job = BossJobs[key];
                BossJobs.Remove(key);
                var r = job.Status == TaskStatus.RanToCompletion ? job.Result : null;
                Texture2D t = null;
                if (r != null && r.png != null)
                {
                    t = NewBossTex();
                    if (!t.LoadImage(r.png, true) || t.width != BossRes) { Object.Destroy(t); t = null; } // imagen guardada dañada
                }
                if (t == null)
                {
                    // Si el hilo falló o la imagen guardada no sirve, se dibuja aquí mismo (más lento, pero el jefe aparece)
                    var px = r != null && r.px != null ? r.px : BossPixels32(d, key == lk, BossRes);
                    t = NewBossTex();
                    t.SetPixels32(px);
                    t.Apply(true, true);
                }
                t.filterMode = FilterMode.Bilinear; t.wrapMode = TextureWrapMode.Clamp; t.mipMapBias = -0.5f;
                Cache[key] = Sprite.Create(t, new Rect(0, 0, BossRes, BossRes), new Vector2(0.5f, 0.5f), BossRes / (d.width * BossWorldFill / BossScale), 0, SpriteMeshType.FullRect);
            }
            return true;
        }

        /// <summary>Sprites del jefe (si no están listos se dibujan en el momento; el juego usa PrepareBoss/TryFinishBoss).</summary>
        public static Sprite BossHull(BossDefinition d) { ForceBoss(d); return Cache[BossKey(d, false)]; }
        public static Sprite BossLights(BossDefinition d) { ForceBoss(d); return Cache[BossKey(d, true)]; }

        static void ForceBoss(BossDefinition d)
        {
            PrepareBoss(d);
            if (TryFinishBoss(d)) return;
            BossJobs[BossKey(d, false)].Wait();
            BossJobs[BossKey(d, true)].Wait();
            TryFinishBoss(d);
        }

        static Texture2D NewBossTex() => new Texture2D(BossRes, BossRes, TextureFormat.RGBA32, true);

        /// <summary>Disparo del jefe: centro al rojo blanco, anillo de su color y contorno oscuro (192 px para 1 unidad).</summary>
        public static Sprite BossShot(BossDefinition d) => Cached("bossshot_" + d.id, () =>
        {
            const int S = 192;
            var px = new Color[S * S];
            var acc = d.accent; acc.a = 1f;
            var dark = new Color(acc.r * 0.22f, acc.g * 0.22f, acc.b * 0.22f, 1f);
            float pw = 2f / S;
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    var p = new Vector2((x + 0.5f) / S * 2f - 1f, (y + 0.5f) / S * 2f - 1f);
                    float r = p.magnitude;
                    if (r > 1f + pw) continue;
                    var c = Color.Lerp(Color.white, Color.Lerp(acc, Color.white, 0.25f), Mathf.SmoothStep(0.3f, 0.7f, r));
                    c = Color.Lerp(c, acc, Mathf.SmoothStep(0.68f, 0.8f, r));
                    c = Color.Lerp(c, dark, Cov(r - 0.86f, pw));
                    if ((p - new Vector2(-0.28f, 0.3f)).magnitude < 0.14f) c = Color.Lerp(c, Color.white, 0.8f); // brillo
                    c.a = Cov(1f - r, pw);
                    px[y * S + x] = c;
                }
            return Finish(NewTex(S, S), px, S);
        });

        /// <summary>Libera la memoria de los demás jefes (cada uno ocupa unos 8 MB).</summary>
        static void ReleaseBossesExcept(string id)
        {
            var remove = new List<string>();
            foreach (var kv in Cache)
                if (kv.Key.StartsWith("boss_") && !kv.Key.EndsWith("_" + id)) remove.Add(kv.Key);
            foreach (var key in remove)
            {
                var sp = Cache[key];
                Cache.Remove(key);
                if (sp != null) { Object.Destroy(sp.texture); Object.Destroy(sp); }
            }
        }

        // ---------------- DIBUJO (seguro para hilos: solo matemáticas sobre datos que no cambian) ----------------

        /// <summary>Distancia al borde de una pieza sin contar el lado que cae sobre el eje central (para que no se vea una costura).</summary>
        static float PartEdgeDist(Vector2 p, Vector2[] poly)
        {
            float d = float.MaxValue;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if (Mathf.Abs(poly[j].x) < 0.001f && Mathf.Abs(poly[i].x) < 0.001f) continue;
                d = Mathf.Min(d, SegDist(p, poly[j], poly[i]));
            }
            return d;
        }

        static float PartSd(Vector2 q, Vector2[] poly)
        {
            float dist = PartEdgeDist(q, poly);
            return InPoly(q, poly) ? -dist : dist; // negativo dentro
        }

        static void Bounds(Vector2[] pts, out Vector2 min, out Vector2 max)
        {
            min = new Vector2(float.MaxValue, float.MaxValue);
            max = new Vector2(float.MinValue, float.MinValue);
            foreach (var v in pts) { min = Vector2.Min(min, v); max = Vector2.Max(max, v); }
        }

        static float BoxDist(Vector2 q, Vector2 min, Vector2 max)
        {
            float dx = Mathf.Max(Mathf.Max(min.x - q.x, q.x - max.x), 0f); // sin Max(params): no crea basura en el bucle
            float dy = Mathf.Max(Mathf.Max(min.y - q.y, q.y - max.y), 0f);
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        static Color32 ToC32(Color c) =>
            new Color32((byte)(Mathf.Clamp01(c.r) * 255f + 0.5f), (byte)(Mathf.Clamp01(c.g) * 255f + 0.5f), (byte)(Mathf.Clamp01(c.b) * 255f + 0.5f), (byte)(Mathf.Clamp01(c.a) * 255f + 0.5f));

        public static Color32[] BossPixels32(BossDefinition d, bool lights, int S)
        {
            var px = new Color32[S * S];
            float pw = 2f / S / BossScale;
            var ink = new Color(0.05f, 0.06f, 0.1f, 1f);
            const float inkIn = 0.024f, inkOut = 0.03f;     // contorno: un poco por dentro y por fuera de cada pieza
            const float bevelW = 0.075f;                     // ancho del canto biselado
            const float panelDepth = 0.11f;                  // línea de panel paralela al borde
            const float glowReach = 0.3f;                    // más allá, el resplandor ya no se ve
            var lightDir = new Vector2(-0.55f, 0.84f);       // luz desde arriba a la izquierda (en pantalla)

            int np = d.parts.Length, nl = d.lightParts.Length;
            var pMin = new Vector2[np]; var pMax = new Vector2[np];
            for (int i = 0; i < np; i++) Bounds(d.parts[i].pts, out pMin[i], out pMax[i]);
            var lMin = new Vector2[nl]; var lMax = new Vector2[nl];
            for (int i = 0; i < nl; i++) Bounds(d.lightParts[i].pts, out lMin[i], out lMax[i]);
            var acc = d.accent; acc.a = 1f;
            float e = pw * 0.75f; // paso para calcular la inclinación del canto

            Parallel.For(0, S, y => // cada fila en el núcleo que esté libre
            {
                for (int x = 0; x < S; x++)
                {
                    var p = new Vector2(((x + 0.5f) / S * 2f - 1f) / BossScale, ((y + 0.5f) / S * 2f - 1f) / BossScale);
                    var q = new Vector2(Mathf.Abs(p.x), p.y); // las piezas están definidas a la derecha
                    float side = p.x < 0f ? -1f : 1f;
                    Color c = new Color(0, 0, 0, 0);
                    float unionSd = glowReach;

                    for (int i = 0; i < np; i++)
                    {
                        float reach = lights ? glowReach : inkOut + pw;
                        if (BoxDist(q, pMin[i], pMax[i]) > reach) continue; // lejos de esta pieza
                        var part = d.parts[i];
                        float sd = PartSd(q, part.pts);
                        if (sd < unionSd) unionSd = sd;
                        if (lights || sd > inkOut + pw) continue;

                        var edge = ink; edge.a = Cov(inkOut - sd, pw);
                        c = Over(c, edge);
                        if (sd >= 0f) continue;
                        float depth = -sd;
                        float v = Mathf.Lerp(0.2f, 1.08f, MetalShade(p, depth, pw)) * part.shade;
                        if (depth < bevelW)
                        {
                            // Canto biselado: según hacia dónde mira el borde, recibe luz o queda en sombra
                            float gx = PartSd(q + new Vector2(e, 0), part.pts) - PartSd(q - new Vector2(e, 0), part.pts);
                            float gy = PartSd(q + new Vector2(0, e), part.pts) - PartSd(q - new Vector2(0, e), part.pts);
                            var n = new Vector2(gx * side, gy);
                            if (n.sqrMagnitude > 1e-12f)
                            {
                                float facing = Vector2.Dot(n.normalized, lightDir);
                                float k = 1f - depth / bevelW;
                                v += 0.32f * facing * k * k;
                            }
                        }
                        v *= 1f - 0.38f * Cov(0.0065f - Mathf.Abs(depth - panelDepth), pw);   // línea de panel
                        v += (Hash(p * 37f) - 0.5f) * 0.03f;                                  // grano fino del metal
                        v = Mathf.Clamp01(v);
                        c = Over(c, new Color(v, v, v, Cov(depth - inkIn, pw)));
                    }

                    if (lights)
                    {
                        float a = 0f, hot = 0f;
                        if (unionSd > 0) a = 0.5f * Mathf.Exp(-unionSd / 0.05f) * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f / BossScale - Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.y))) / 0.08f)); // resplandor, que se apaga antes del borde
                        else a = Cov(0.014f - Mathf.Abs(unionSd + 0.055f), pw) * 0.9f;                       // línea luminosa junto al borde
                        for (int i = 0; i < nl; i++)
                        {
                            if (BoxDist(q, lMin[i], lMax[i]) > glowReach) continue;
                            var lp = d.lightParts[i];
                            float sd = PartSd(q, lp.pts);
                            float inside = Cov(-sd, pw);
                            if (inside > a) a = inside;
                            if (sd > 0) a = Mathf.Max(a, 0.75f * Mathf.Exp(-sd / 0.04f) * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f / BossScale - Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.y))) / 0.08f))); // halo de la luz
                            if (sd < 0 && lp.radius > 0) hot = Mathf.Max(hot, Mathf.SmoothStep(0.3f, 0.85f, -sd / lp.radius) * inside);
                        }
                        if (a > 0.003f)
                        {
                            var col = Color.Lerp(acc, Color.white, hot * 0.9f); // centro de cada luz casi blanco
                            col.a = Mathf.Clamp01(a);
                            c = col;
                        }
                    }
                    px[y * S + x] = ToC32(c);
                }
            });
            return px;
        }
    }
}
