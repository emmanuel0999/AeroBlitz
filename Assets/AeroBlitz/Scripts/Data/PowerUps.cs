using UnityEngine;

namespace AeroBlitz
{
    /// <summary>
    /// Mejoras temporales que aparecen durante los niveles (desde GameConfig.PickupsFromLevel).
    /// No se guardan en el perfil: duran unos segundos o unos disparos y terminan al acabar el nivel.
    /// </summary>
    public enum PowerUpType { Shield, Overcharge, Magnet, Repair }

    public static class PowerUps
    {
        public static string Name(PowerUpType t) => L.T("pu." + t.ToString().ToLowerInvariant());

        public static Color ColorOf(PowerUpType t)
        {
            switch (t)
            {
                case PowerUpType.Shield: return new Color(0.3f, 0.7f, 1f);
                case PowerUpType.Overcharge: return new Color(1f, 0.82f, 0.2f);
                case PowerUpType.Magnet: return new Color(1f, 0.38f, 0.38f);
                default: return new Color(0.35f, 1f, 0.45f); // Repair
            }
        }
    }
}
