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
        public string trailId;     // estela del motor

        public static ShipStats Compute(ShipState state)
        {
            var def = ShipCatalog.Get(state.shipId);
            var look = ShipLook.Of(def, state);
            var s = new ShipStats
            {
                maxHealth = def.maxHealth * (1f + 0.10f * state.armorLevel),
                speed = def.speed * (1f + 0.06f * state.engineLevel),
                shots = def.shotsPerCharge + state.magazineLevel,
                chargeTime = def.chargeTime * (1f - 0.06f * state.chargerLevel),
                fireWindow = def.fireWindow,
                color = look.neon ? look.detailColor : look.hullColor, // color de la nave (explosiones, efectos)
                trailId = string.IsNullOrEmpty(state.trailId) ? "default" : state.trailId,
            };

            foreach (var id in state.equipped) Apply(ref s, UpgradeCatalog.GetEquipment(id));

            // Arma premium de prueba (cofre épico): vale para todas las naves mientras dure
            var pd = GameManager.Instance != null ? GameManager.Instance.Data : null;
            if (pd != null && Chests.TempEquipActive(pd) && !state.equipped.Contains(pd.tempEquipId))
                Apply(ref s, UpgradeCatalog.GetEquipment(pd.tempEquipId));

            // Regla de diseño: el tiempo de carga SIEMPRE dura más que el tiempo cargado.
            s.fireWindow = Mathf.Min(s.fireWindow, s.chargeTime * 0.65f);

            // "Nivel de nave": tier + mejoras + equipo. Se muestra en el hangar.
            s.shipLevel = def.tier * 3 - 2 + state.armorLevel + state.engineLevel + state.magazineLevel + state.chargerLevel + state.equipped.Count * 2;
            return s;
        }

        static void Apply(ref ShipStats s, EquipmentDefinition eq)
        {
            if (eq == null) return;
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
    }
}
