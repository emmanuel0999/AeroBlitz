using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace AeroBlitz.EditorTools
{
    /// <summary>
    /// Genera el ícono del juego por código (nave Halcón sobre fondo espacial) y lo asigna al proyecto.
    /// Menú: Aero Blitz > Generar ícono. Para usar un ícono propio, reemplaza Assets/AeroBlitz/Icon/icon_1024.png.
    /// OJO: desde la versión 15 (1.0.3) el icono del juego es StoreAssets/iconos_v15/icono_final_1024.png (Fénix al ataque,
    /// hecho con TrailerCaptures/herramientas/icono_final.ps1); el de las versiones 12-14 quedó en StoreAssets/iconos_nuevos/icon_1024_hasta_v14.png.
    /// Este menú sobrescribe el icono con el diseño antiguo, así que no lo uses salvo que quieras volver a él.
    /// </summary>
    public static class IconGenerator
    {
        const string IconFolder = "Assets/AeroBlitz/Icon";
        const string IconPath = IconFolder + "/icon_1024.png";
        const string StoreFolder = "StoreAssets"; // fuera de Assets: material para la ficha de Google Play

        [MenuItem("Aero Blitz/Generar ícono")]
        public static void Generate()
        {
            var big = Draw(1024);
            Directory.CreateDirectory(IconFolder);
            File.WriteAllBytes(IconPath, big.EncodeToPNG());

            Directory.CreateDirectory(StoreFolder);
            File.WriteAllBytes(StoreFolder + "/icono_512.png", Draw(512).EncodeToPNG());

            AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(IconPath);
            importer.textureType = TextureImporterType.Default;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.SaveAndReimport();

            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
            AssetDatabase.SaveAssets();
            Debug.Log("[Aero Blitz] Ícono generado: " + IconPath + " y " + StoreFolder + "/icono_512.png");
        }

        public static void GenerateBatch()
        {
            Generate();
            EditorApplication.Exit(0);
        }

        static bool InPoly(Vector2 p, Vector2[] poly)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
                if ((poly[i].y > p.y) != (poly[j].y > p.y) &&
                    p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x)
                    inside = !inside;
            return inside;
        }

        /// <summary>Cobertura 0..1 de un polígono en un píxel (supermuestreo 3x3).</summary>
        static float Coverage(Vector2 uv, float pixel, Vector2[] poly)
        {
            int hits = 0;
            for (int sy = 0; sy < 3; sy++)
                for (int sx = 0; sx < 3; sx++)
                    if (InPoly(uv + new Vector2((sx - 1) * pixel / 3f, (sy - 1) * pixel / 3f), poly)) hits++;
            return hits / 9f;
        }

        static Vector2[] Transform(Vector2[] shape, float scale, Vector2 offset)
        {
            var r = new Vector2[shape.Length];
            for (int i = 0; i < shape.Length; i++) r[i] = shape[i] * scale + offset;
            return r;
        }

        static Color Over(Color dst, Color src, float a)
        {
            a = Mathf.Clamp01(a);
            return new Color(Mathf.Lerp(dst.r, src.r, a), Mathf.Lerp(dst.g, src.g, a), Mathf.Lerp(dst.b, src.b, a), 1f);
        }

        static Texture2D Draw(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            float pixel = 2f / size;

            // La nave ocupa la zona central (segura para las máscaras redondas de Android)
            var center = new Vector2(0f, 0.08f);
            var ship = Transform(ShipCatalog.Get(ShipCatalog.StarterShipId).shape, 0.5f, center);
            var cockpit = Transform(new[] { new Vector2(0, 1), new Vector2(0.6f, -0.2f), new Vector2(0.4f, -0.9f), new Vector2(-0.4f, -0.9f), new Vector2(-0.6f, -0.2f) }, 0.13f, center + new Vector2(0, 0.07f));
            var flame = Transform(new[] { new Vector2(-0.55f, 1), new Vector2(0.55f, 1), new Vector2(0, -1) }, 0.17f, center + new Vector2(0, -0.56f));
            var flameCore = Transform(new[] { new Vector2(-0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -1) }, 0.09f, center + new Vector2(0, -0.48f));

            // Estrellas (posiciones fijas para que el ícono sea siempre igual)
            var rnd = new System.Random(2026);
            var stars = new Vector3[70];
            for (int i = 0; i < stars.Length; i++)
                stars[i] = new Vector3((float)rnd.NextDouble() * 2f - 1f, (float)rnd.NextDouble() * 2f - 1f, 0.006f + (float)rnd.NextDouble() * 0.012f);

            var top = new Color(0.06f, 0.13f, 0.36f);
            var bottom = new Color(0.01f, 0.02f, 0.09f);
            var glow = new Color(0.15f, 0.6f, 1f);

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    var uv = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f);

                    // Fondo: degradado + resplandor detrás de la nave
                    Color c = Color.Lerp(bottom, top, (uv.y + 1f) / 2f);
                    float d = (uv - center).magnitude;
                    c = Over(c, glow, Mathf.Pow(Mathf.Clamp01(1f - d / 0.95f), 2.2f) * 0.75f);

                    foreach (var s in stars)
                    {
                        float sd = Vector2.Distance(uv, new Vector2(s.x, s.y));
                        if (sd < s.z * 2.5f) c = Over(c, Color.white, Mathf.Pow(Mathf.Clamp01(1f - sd / (s.z * 2.5f)), 2f) * 0.9f);
                    }

                    // Llama del motor
                    float fd = Vector2.Distance(uv, center + new Vector2(0, -0.55f));
                    c = Over(c, new Color(1f, 0.55f, 0.1f), Mathf.Pow(Mathf.Clamp01(1f - fd / 0.3f), 2f) * 0.6f);
                    c = Over(c, new Color(1f, 0.5f, 0.1f), Coverage(uv, pixel, flame));
                    c = Over(c, new Color(1f, 0.95f, 0.6f), Coverage(uv, pixel, flameCore));

                    // Nave: blanco azulado con sombreado hacia los bordes
                    float cov = Coverage(uv, pixel, ship);
                    if (cov > 0f)
                    {
                        float local = Mathf.Abs(uv.x - center.x) / 0.45f;
                        float shade = Mathf.Lerp(1f, 0.55f, Mathf.Clamp01(local)) + (uv.y - center.y) * 0.25f;
                        var body = Color.Lerp(new Color(0.25f, 0.75f, 1f), new Color(0.92f, 0.98f, 1f), Mathf.Clamp01(shade));
                        c = Over(c, body, cov);
                    }

                    float cc = Coverage(uv, pixel, cockpit);
                    if (cc > 0f)
                        c = Over(c, Color.Lerp(new Color(0.05f, 0.15f, 0.4f), new Color(0.5f, 0.9f, 1f), Mathf.InverseLerp(center.y - 0.05f, center.y + 0.2f, uv.y)), cc);

                    px[y * size + x] = c;
                }

            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }
    }
}
