using UnityEngine;

namespace AeroBlitz
{
    /// <summary>Aspecto de un mundo: nombre, colores del fondo y si tiene un planeta lejano.</summary>
    public class WorldTheme
    {
        public string nameKey;
        public Color background;   // color de fondo de la cámara (siempre oscuro para que se lean los obstáculos)
        public Color stars;        // tinte de las estrellas
        public Color nebulaA;      // nubes de color del fondo
        public Color nebulaB;
        public float nebulaAlpha = 0.22f;
        public bool planet;
        public Color planetColor;
        public string Name => L.T(nameKey);
    }

    /// <summary>
    /// Los 70 niveles se dividen en 10 mundos de 7 niveles. Cada mundo tiene su fondo, su nombre
    /// y su tema musical (ver MusicPlayer: hay un tema por mundo, en el mismo orden).
    /// </summary>
    public static class WorldCatalog
    {
        public const int LevelsPerWorld = 7;

        public static readonly WorldTheme[] Worlds =
        {
            // 1-7
            new WorldTheme { nameKey = "lvl.w1", background = new Color(0.02f, 0.03f, 0.09f), stars = new Color(0.7f, 0.8f, 1f),
                nebulaA = new Color(0.15f, 0.25f, 0.6f), nebulaB = new Color(0.3f, 0.2f, 0.55f), planet = true, planetColor = new Color(0.35f, 0.45f, 0.7f) },
            // 8-14
            new WorldTheme { nameKey = "lvl.w2", background = new Color(0.01f, 0.05f, 0.09f), stars = new Color(0.75f, 0.95f, 1f),
                nebulaA = new Color(0.2f, 0.6f, 0.75f), nebulaB = new Color(0.55f, 0.8f, 1f), nebulaAlpha = 0.18f, planet = true, planetColor = new Color(0.55f, 0.8f, 0.95f) },
            // 15-21
            new WorldTheme { nameKey = "lvl.w3", background = new Color(0.06f, 0.01f, 0.04f), stars = new Color(1f, 0.8f, 0.8f),
                nebulaA = new Color(0.6f, 0.08f, 0.15f), nebulaB = new Color(0.45f, 0.05f, 0.35f), nebulaAlpha = 0.26f },
            // 22-28
            new WorldTheme { nameKey = "lvl.w4", background = new Color(0.03f, 0.035f, 0.015f), stars = new Color(1f, 0.95f, 0.7f),
                nebulaA = new Color(0.45f, 0.45f, 0.1f), nebulaB = new Color(0.25f, 0.35f, 0.12f), nebulaAlpha = 0.22f, planet = true, planetColor = new Color(0.75f, 0.75f, 0.55f) },
            // 29-35
            new WorldTheme { nameKey = "lvl.w5", background = new Color(0.07f, 0.025f, 0f), stars = new Color(1f, 0.85f, 0.6f),
                nebulaA = new Color(0.85f, 0.35f, 0.05f), nebulaB = new Color(0.7f, 0.15f, 0.02f), nebulaAlpha = 0.2f, planet = true, planetColor = new Color(1f, 0.78f, 0.3f) },
            // 36-42
            new WorldTheme { nameKey = "lvl.w6", background = new Color(0.03f, 0.035f, 0.045f), stars = new Color(0.8f, 0.85f, 0.9f),
                nebulaA = new Color(0.3f, 0.38f, 0.48f), nebulaB = new Color(0.2f, 0.3f, 0.32f), nebulaAlpha = 0.3f, planet = true, planetColor = new Color(0.55f, 0.6f, 0.68f) },
            // 43-49
            new WorldTheme { nameKey = "lvl.w7", background = new Color(0.01f, 0.045f, 0.035f), stars = new Color(0.7f, 1f, 0.85f),
                nebulaA = new Color(0.05f, 0.6f, 0.42f), nebulaB = new Color(0.1f, 0.4f, 0.65f), nebulaAlpha = 0.3f },
            // 50-56
            new WorldTheme { nameKey = "lvl.w8", background = new Color(0.04f, 0.01f, 0.08f), stars = new Color(0.9f, 0.75f, 1f),
                nebulaA = new Color(0.5f, 0.1f, 0.7f), nebulaB = new Color(0.15f, 0.3f, 0.8f), planet = true, planetColor = new Color(0.65f, 0.35f, 0.9f) },
            // 57-63
            new WorldTheme { nameKey = "lvl.w9", background = new Color(0.008f, 0.008f, 0.015f), stars = new Color(0.6f, 0.6f, 0.72f),
                nebulaA = new Color(0.3f, 0.18f, 0.5f), nebulaB = new Color(0.12f, 0.12f, 0.25f), nebulaAlpha = 0.32f },
            // 64-70
            new WorldTheme { nameKey = "lvl.w10", background = new Color(0.05f, 0.015f, 0.05f), stars = new Color(1f, 0.9f, 0.95f),
                nebulaA = new Color(0.8f, 0.2f, 0.55f), nebulaB = new Color(1f, 0.7f, 0.25f), nebulaAlpha = 0.3f },
        };

        public static int WorldForLevel(int level) => Mathf.Clamp((level - 1) / LevelsPerWorld, 0, Worlds.Length - 1);
        public static WorldTheme ForLevel(int level) => Worlds[WorldForLevel(level)];
    }
}
