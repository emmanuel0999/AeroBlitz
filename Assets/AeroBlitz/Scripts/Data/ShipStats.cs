using UnityEngine;

namespace AeroBlitz
{
    /// <summary>Estadísticas finales de una nave: base + mejoras + equipamiento.</summary>
    public struct ShipStats
    {
        public float maxHealth;
        public float speed;
        public int shots;
        public float chargeTime;
        public float fireWindow;
        public int shieldHits;
        public float damageReduction;
        public bool doubleCannon;
        public int cannonTier;     // 0 normal, 1 doble, 2 triple, 3 plasma
        public bool missiles;
        public int shipLevel;
        public Color color;

        public static ShipStats Compute(ShipState state)
        {
            var def = ShipCatalog.Get(state.shipId);
            var s = new ShipStats
            {
                maxHealth = def.maxHealth * (1f + 0.10f * state.armorLevel),
                speed = def.speed * (1f + 0.06f * state.engineLevel),
                shots = def.shotsPerCharge + state.magazineLevel,
                chargeTime = def.chargeTime * (1f - 0.06f * state.chargerLevel),
                fireWindow = def.fireWindow,
                color = UpgradeCatalog.GetPaint(state.paintId).color,
            };

            foreach (var id in state.equipped)
            {
                var eq = UpgradeCatalog.GetEquipment(id);
                if (eq == null) continue;
                s.maxHealth *= eq.healthMultiplier;
                s.damageReduction = Mathf.Max(s.damageReduction, eq.damageReduction);
                s.shieldHits += eq.shieldHits;
                s.shots += eq.extraShots;
                s.doubleCannon |= eq.doubleCannon;
                s.cannonTier = Mathf.Max(s.cannonTier, eq.cannonTier);
                s.missiles |= eq.missiles;
                s.chargeTime *= eq.chargeTimeMultiplier;
                s.fireWindow += eq.fireWindowBonus;
                s.speed *= eq.speedMultiplier;
            }

            // Regla de diseño: el tiempo de carga SIEMPRE dura más que el tiempo cargado.
            s.fireWindow = Mathf.Min(s.fireWindow, s.chargeTime * 0.65f);

            // "Nivel de nave": tier + mejoras + equipo. Se muestra en el hangar.
            s.shipLevel = def.tier * 3 - 2 + state.armorLevel + state.engineLevel + state.magazineLevel + state.chargerLevel + state.equipped.Count * 2;
            return s;
        }
    }
}
