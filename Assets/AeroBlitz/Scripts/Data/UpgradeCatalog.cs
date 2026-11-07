using System.Collections.Generic;
using UnityEngine;

namespace AeroBlitz
{
    public enum UpgradeStat { Armor, Engine, Magazine, Charger }

    /// <summary>Mejora básica de estadística (se compra con monedas del juego o viendo videos).</summary>
    public class StatUpgradeDefinition
    {
        public UpgradeStat stat;
        public string Name => L.T("up." + stat);
        public string Description => L.T("up." + stat + ".desc");
        public int[] coinCosts;   // costo por nivel (1..MaxLevel)
        public int MaxLevel => coinCosts.Length;
    }

    /// <summary>
    /// Equipamiento. El premium (dinero real) tiene las mejores piezas; el básico se compra con
    /// monedas del juego y es deliberadamente más modesto.
    /// </summary>
    public class EquipmentDefinition
    {
        public string id;
        public string Name => L.T("eq." + id);
        public string Description => L.T("eq." + id + ".desc");
        public string productId;           // dinero real (null si se compra con monedas)
        public int coinPrice;              // monedas del juego (0 si es de dinero real)
        public bool IsCoin => coinPrice > 0;
        public float healthMultiplier = 1f;
        public float damageReduction;      // 0..1
        public int shieldHits;
        public int extraShots;
        public bool doubleCannon;
        public float chargeTimeMultiplier = 1f;
        public float fireWindowBonus;
        public float speedMultiplier = 1f;
        public int cannonTier;             // 1 doble, 2 triple, 3 plasma (solo cuenta el mejor equipado)
        public bool missiles;              // habilita el botón de misiles
    }

    public class PaintDefinition
    {
        public string id;
        public string Name => L.T("paint." + id);
        public Color color;
        public PriceType priceType;
        public int coinPrice;
        public string productId;
    }

    public static class UpgradeCatalog
    {
        public static readonly List<StatUpgradeDefinition> Stats = new List<StatUpgradeDefinition>
        {
            new StatUpgradeDefinition { stat = UpgradeStat.Armor, coinCosts = new[] { 400, 900, 1600 } },
            new StatUpgradeDefinition { stat = UpgradeStat.Engine, coinCosts = new[] { 350, 800, 1400 } },
            new StatUpgradeDefinition { stat = UpgradeStat.Magazine, coinCosts = new[] { 500, 1100, 2000 } },
            new StatUpgradeDefinition { stat = UpgradeStat.Charger, coinCosts = new[] { 450, 1000, 1800 } },
        };

        public static StatUpgradeDefinition GetStat(UpgradeStat s) => Stats.Find(x => x.stat == s);

        public static readonly List<EquipmentDefinition> Equipment = new List<EquipmentDefinition>
        {
            new EquipmentDefinition { id = "eq_shield", productId = ProductIds.EqPlasmaShield, shieldHits = 2 },
            new EquipmentDefinition { id = "eq_twin", productId = ProductIds.EqTwinCannon, doubleCannon = true, cannonTier = 1, extraShots = 2 },
            new EquipmentDefinition { id = "eq_quantum", productId = ProductIds.EqQuantumCore, chargeTimeMultiplier = 0.75f, fireWindowBonus = 1f },
            new EquipmentDefinition { id = "eq_titanium", productId = ProductIds.EqTitaniumArmor, healthMultiplier = 1.5f, damageReduction = 0.2f },
            new EquipmentDefinition { id = "eq_thrusters", productId = ProductIds.EqIonThrusters, speedMultiplier = 1.2f },
            new EquipmentDefinition { id = "eq_triple", productId = ProductIds.EqTripleCannon, cannonTier = 2 },
            new EquipmentDefinition { id = "eq_plasma", productId = ProductIds.EqPlasmaCannon, cannonTier = 3 },
            new EquipmentDefinition { id = "eq_missiles", productId = ProductIds.EqMissiles, missiles = true },
            // Básico: se compra con monedas. Efectos menores que sus equivalentes premium.
            new EquipmentDefinition { id = "eq_plates", coinPrice = 3000, healthMultiplier = 1.2f },
            new EquipmentDefinition { id = "eq_magazine", coinPrice = 3500, extraShots = 2 },
        };

        public static EquipmentDefinition GetEquipment(string id) => Equipment.Find(x => x.id == id);
        public static EquipmentDefinition EquipmentByProduct(string pid) =>
            string.IsNullOrEmpty(pid) ? null : Equipment.Find(x => x.productId == pid);

        public static readonly List<PaintDefinition> Paints = new List<PaintDefinition>
        {
            new PaintDefinition { id = "default", color = new Color(0.85f, 0.9f, 1f), priceType = PriceType.Free },
            new PaintDefinition { id = "red", color = new Color(1f, 0.35f, 0.35f), priceType = PriceType.Free },
            new PaintDefinition { id = "green", color = new Color(0.4f, 1f, 0.5f), priceType = PriceType.Free },
            new PaintDefinition { id = "cyan", color = new Color(0.3f, 0.95f, 1f), priceType = PriceType.Coins, coinPrice = 300 },
            new PaintDefinition { id = "orange", color = new Color(1f, 0.6f, 0.2f), priceType = PriceType.Coins, coinPrice = 300 },
            new PaintDefinition { id = "purple", color = new Color(0.7f, 0.45f, 1f), priceType = PriceType.Coins, coinPrice = 500 },
            new PaintDefinition { id = "gold", color = new Color(1f, 0.84f, 0.25f), priceType = PriceType.RealMoney, productId = ProductIds.PaintGold },
        };

        public static PaintDefinition GetPaint(string id) => Paints.Find(x => x.id == id) ?? Paints[0];
        public static PaintDefinition PaintByProduct(string pid) => Paints.Find(x => x.productId == pid);
    }
}
