using System.IO;
using UnityEditor;
using UnityEngine;

namespace AeroBlitz.EditorTools
{
    /// <summary>
    /// Genera una imagen de cada uno de los 10 mundos (carpeta WorldPreview) con la nave y algunos
    /// obstáculos encima, para revisar que el fondo se ve bien y no tapa nada.
    /// </summary>
    public static class WorldPreview
    {
        const string Folder = "WorldPreview";
        const int W = 540, H = 1200;

        [MenuItem("Aero Blitz/Generar vista previa de los mundos")]
        public static void Generate()
        {
            Directory.CreateDirectory(Folder);
            Random.InitState(7);
            var root = new GameObject("WorldPreviewRoot");
            try
            {
                var cam = new GameObject("Cam").AddComponent<Camera>();
                cam.transform.SetParent(root.transform);
                cam.orthographic = true;
                cam.orthographicSize = 8f;
                cam.transform.position = new Vector3(0, 0, -10);
                cam.clearFlags = CameraClearFlags.SolidColor;

                var backdrop = new GameObject("Backdrop").AddComponent<WorldBackdrop>();
                backdrop.transform.SetParent(root.transform);
                backdrop.Build(cam);
                backdrop.Tick(2f, 1f, 1f); // un poco de movimiento para que no quede todo en su sitio inicial

                // Elementos del juego encima, para juzgar el contraste
                Put(root, SpriteFactory.Ship(ShipCatalog.Get(ShipCatalog.StarterShipId)), new Vector3(0, -5.5f, 0), 1f, new Color(0.85f, 0.9f, 1f), 10);
                Put(root, SpriteFactory.Asteroid(0), new Vector3(-2.5f, 3f, 0), 1.2f, Color.white, 5);
                Put(root, SpriteFactory.Asteroid(5), new Vector3(2.2f, 0.5f, 0), 1.6f, Color.white, 5);
                Put(root, SpriteFactory.Mine, new Vector3(-1.5f, -1.5f, 0), 0.9f, Color.white, 5);
                Put(root, SpriteFactory.Drone, new Vector3(2.5f, 5f, 0), 1f, Color.white, 5);
                Put(root, SpriteFactory.Wall, new Vector3(0, 1.8f, 0), 1f, Color.white, 5);
                // Moneda y mejoras temporales (aparecen desde el nivel 16)
                Put(root, SpriteFactory.Coin, new Vector3(-3.2f, -2.8f, 0), 0.6f, Color.white, 3);
                Put(root, SpriteFactory.Coin, new Vector3(-2.4f, -3.4f, 0), 0.6f, Color.white, 3);
                float px = -3.6f;
                foreach (PowerUpType t in System.Enum.GetValues(typeof(PowerUpType)))
                {
                    Put(root, SpriteFactory.PowerUp(t), new Vector3(px, -7f, 0), 0.95f, Color.white, 3);
                    px += 1.8f;
                }

                var rt = new RenderTexture(W, H, 24);
                cam.targetTexture = rt;
                var tex = new Texture2D(W, H, TextureFormat.RGB24, false);
                for (int w = 0; w < WorldCatalog.Worlds.Length; w++)
                {
                    backdrop.Apply(w);
                    cam.Render();
                    RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
                    tex.Apply();
                    int from = w * WorldCatalog.LevelsPerWorld + 1;
                    string path = $"{Folder}/mundo_{w + 1:00}_niveles_{from}-{from + WorldCatalog.LevelsPerWorld - 1}.png";
                    File.WriteAllBytes(path, tex.EncodeToPNG());
                    Debug.Log("[Aero Blitz] Vista previa: " + path);
                }
                RenderTexture.active = null;
                cam.targetTexture = null;
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(tex);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        public static void GenerateBatch()
        {
            Generate();
            EditorApplication.Exit(0);
        }

        static void Put(GameObject root, Sprite sprite, Vector3 pos, float scale, Color color, int order)
        {
            var go = new GameObject("Item");
            go.transform.SetParent(root.transform);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
        }
    }
}
