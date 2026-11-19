using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AeroBlitz
{
    public enum ChestTier { Common, Rare, Epic }

    /// <summary>Lo que salió al abrir un cofre: el premio principal (en grande) y su detalle.</summary>
    public class ChestResult
    {
        public ChestTier tier;
        public string headline;
        public string detail = "";
        public Sprite icon;
        public Color iconColor = Color.white;
    }

    /// <summary>
    /// Cofres con premio aleatorio. Solo se ganan jugando (nunca se venden con dinero real):
    /// común (misión diaria, día 4 de la racha), raro (cada 5 niveles) y épico (misión semanal).
    /// Nunca regalan para siempre lo que se vende: las armas y naves premium salen solo como prueba de 24 h.
    /// </summary>
    public static class Chests
    {
        static readonly string[] PremiumWeapons = { ProductIds.EqTripleCannon, ProductIds.EqPlasmaCannon, ProductIds.EqMissiles };

        public static string Name(ChestTier t) => L.T(t == ChestTier.Common ? "chest.tier_common" : t == ChestTier.Rare ? "chest.tier_rare" : "chest.tier_epic");

        public static Color TierColor(ChestTier t) =>
            t == ChestTier.Common ? new Color(0.55f, 0.65f, 0.8f) : t == ChestTier.Rare ? new Color(0.2f, 0.55f, 1f) : new Color(0.72f, 0.3f, 1f);

        public static int Count(PlayerData d, ChestTier t) => t == ChestTier.Common ? d.chestCommon : t == ChestTier.Rare ? d.chestRare : d.chestEpic;
        public static int Total(PlayerData d) => d.chestCommon + d.chestRare + d.chestEpic;

        public static void Add(PlayerData d, ChestTier t, int n = 1)
        {
            if (t == ChestTier.Common) d.chestCommon = Mathf.Max(0, d.chestCommon + n);
            else if (t == ChestTier.Rare) d.chestRare = Mathf.Max(0, d.chestRare + n);
            else d.chestEpic = Mathf.Max(0, d.chestEpic + n);
        }

        /// <summary>El mejor cofre que tiene sin abrir (épico, luego raro, luego común).</summary>
        public static ChestTier? Next(PlayerData d) =>
            d.chestEpic > 0 ? ChestTier.Epic : d.chestRare > 0 ? ChestTier.Rare : d.chestCommon > 0 ? ChestTier.Common : (ChestTier?)null;

        /// <summary>Texto de probabilidades (botón "i").</summary>
        public static string OddsText(ChestTier t)
        {
            int P(float f) => Mathf.RoundToInt(f * 100f);
            switch (t)
            {
                case ChestTier.Common:
                    return L.T("chest.odds_common_fmt", P(GameConfig.CommonCoinsChance), GameConfig.CommonCoinsMin, GameConfig.CommonCoinsMax,
                        P(GameConfig.CommonLifeChance), P(1f - GameConfig.CommonCoinsChance - GameConfig.CommonLifeChance));
                case ChestTier.Rare:
                    return L.T("chest.odds_rare_fmt", P(GameConfig.RareCoinsChance), GameConfig.RareCoinsMin, GameConfig.RareCoinsMax,
                        P(GameConfig.RareItemChance), P(1f - GameConfig.RareCoinsChance - GameConfig.RareItemChance));
                default:
                    return L.T("chest.odds_epic_fmt", P(GameConfig.EpicWeaponChance), GameConfig.TempWeaponHours,
                        P(GameConfig.EpicShipChance), P(1f - GameConfig.EpicWeaponChance - GameConfig.EpicShipChance));
            }
        }

        /// <summary>Abre un cofre de ese tipo (lo gasta) y entrega el premio. Null si no tenía ninguno.</summary>
        public static ChestResult Open(PlayerData d, ChestTier t)
        {
            if (Count(d, t) <= 0) return null;
            Add(d, t, -1);
            var r = new ChestResult { tier = t };
            float roll = Random.value;
            switch (t)
            {
                case ChestTier.Common:
                    if (roll < GameConfig.CommonCoinsChance) Coins(d, r, Random.Range(GameConfig.CommonCoinsMin, GameConfig.CommonCoinsMax + 1));
                    else if (roll < GameConfig.CommonCoinsChance + GameConfig.CommonLifeChance) Life(d, r);
                    else if (!Paint(d, r)) Coins(d, r, GameConfig.CommonChestFallbackCoins);
                    break;
                case ChestTier.Rare:
                    if (roll < GameConfig.RareCoinsChance) Coins(d, r, Random.Range(GameConfig.RareCoinsMin, GameConfig.RareCoinsMax + 1));
                    else if (roll < GameConfig.RareCoinsChance + GameConfig.RareItemChance)
                    {
                        if (!CoinEquipment(d, r) && !FreeUpgrade(d, r)) Coins(d, r, GameConfig.RareChestFallbackCoins);
                    }
                    else
                    {
                        bool paintFirst = Random.value < 0.5f;
                        bool ok = paintFirst ? (Paint(d, r) || Trail(d, r, false)) : (Trail(d, r, false) || Paint(d, r));
                        if (!ok) Coins(d, r, GameConfig.RareChestFallbackCoins);
                    }
                    break;
                default:
                    if (roll < GameConfig.EpicWeaponChance)
                    {
                        if (!TempWeapon(d, r) && !TempShip(d, r)) Coins(d, r, GameConfig.EpicChestFallbackCoins);
                    }
                    else if (roll < GameConfig.EpicWeaponChance + GameConfig.EpicShipChance)
                    {
                        if (!TempShip(d, r) && !TempWeapon(d, r)) Coins(d, r, GameConfig.EpicChestFallbackCoins);
                    }
                    else if (!Trail(d, r, true) && !Trail(d, r, false)) Coins(d, r, GameConfig.EpicChestFallbackCoins);
                    break;
            }
            return r;
        }

        // ---------------- PREMIOS ----------------
        static void Coins(PlayerData d, ChestResult r, int amount)
        {
            amount = Mathf.Max(10, Mathf.RoundToInt(amount / 10f) * 10);
            d.coins += amount;
            r.headline = L.T("chest.h_coins_fmt", UIFactory.Fmt(amount));
            r.icon = SpriteFactory.Coin;
        }

        static void Life(PlayerData d, ChestResult r)
        {
            d.lives = Mathf.Min(GameConfig.MaxLives, d.lives + 1);
            r.headline = L.T("chest.h_life");
            r.icon = SpriteFactory.Icon("lives1");
        }

        /// <summary>Pintura de las que se compran con monedas y aún no tiene.</summary>
        static bool Paint(PlayerData d, ChestResult r)
        {
            var pool = UpgradeCatalog.Paints.FindAll(p => p.priceType == PriceType.Coins && p.coinPrice <= GameConfig.ChestPaintMaxPrice && !d.ownedPaints.Contains(p.id));
            if (pool.Count == 0) return false;
            var p = pool[Random.Range(0, pool.Count)];
            d.ownedPaints.Add(p.id);
            r.headline = L.T("chest.h_paint_fmt", p.Name);
            r.detail = L.T("chest.d_paint");
            r.icon = SpriteFactory.Rounded;
            r.iconColor = p.color;
            return true;
        }

        static bool Trail(PlayerData d, ChestResult r, bool rare)
        {
            if (d.ownedTrails == null) d.ownedTrails = new List<string> { "default" };
            var pool = UpgradeCatalog.Trails.FindAll(x => x.id != "default" && x.rare == rare && !d.ownedTrails.Contains(x.id));
            if (pool.Count == 0) return false;
            var tr = pool[Random.Range(0, pool.Count)];
            d.ownedTrails.Add(tr.id);
            r.headline = L.T("chest.h_trail_fmt", tr.Name);
            r.detail = L.T("chest.d_trail");
            r.icon = SpriteFactory.FlameMask;
            r.iconColor = tr.color;
            return true;
        }

        /// <summary>Equipamiento de los que se compran con monedas (placas, cargador); queda equipado en la nave actual.</summary>
        static bool CoinEquipment(PlayerData d, ChestResult r)
        {
            var pool = UpgradeCatalog.Equipment.FindAll(e => e.IsCoin && !d.ownedEquipment.Contains(e.id));
            if (pool.Count == 0) return false;
            var eq = pool[Random.Range(0, pool.Count)];
            d.ownedEquipment.Add(eq.id);
            var ship = d.SelectedShipState;
            if (ship != null && !ship.equipped.Contains(eq.id)) ship.equipped.Add(eq.id);
            r.headline = eq.Name;
            r.detail = L.T("chest.d_equip");
            r.icon = SpriteFactory.Icon("eq:" + eq.id);
            return true;
        }

        /// <summary>Un nivel de mejora gratis en la nave propia que esté usando (no en la de prueba).</summary>
        static bool FreeUpgrade(PlayerData d, ChestResult r)
        {
            var ship = d.SelectedShipState;
            if (ship == null || d.IsTempShip(ship.shipId)) ship = d.GetShip(d.tempShipPrevSelected) ?? d.ships[0];
            var options = UpgradeCatalog.Stats.FindAll(st => ship.GetUpgrade(st.stat) < st.MaxLevel);
            if (options.Count == 0) return false;
            var st = options[Random.Range(0, options.Count)];
            ship.SetUpgrade(st.stat, ship.GetUpgrade(st.stat) + 1);
            r.headline = L.T("chest.h_upgrade_fmt", st.Name);
            r.detail = L.T("chest.d_upgrade_fmt", ShipCatalog.Get(ship.shipId).Name, ship.GetUpgrade(st.stat));
            r.icon = SpriteFactory.Icon("free_upgrade");
            return true;
        }

        static bool TempWeapon(PlayerData d, ChestResult r)
        {
            var pool = new List<EquipmentDefinition>();
            foreach (var pid in PremiumWeapons)
            {
                var eq = UpgradeCatalog.EquipmentByProduct(pid);
                if (eq != null && !d.ownedEquipment.Contains(eq.id) && eq.id != d.tempEquipId) pool.Add(eq);
            }
            if (pool.Count == 0) return false;
            var pick = pool[Random.Range(0, pool.Count)];
            d.tempEquipId = pick.id;
            d.tempEquipUntilTicks = DateTime.UtcNow.AddHours(GameConfig.TempWeaponHours).Ticks;
            r.headline = L.T("chest.h_temp_fmt", pick.Name, GameConfig.TempWeaponHours);
            r.detail = L.T("chest.d_temp_weapon_fmt", GameConfig.TempWeaponHours);
            r.icon = SpriteFactory.Icon("eq:" + pick.id);
            return true;
        }

        static bool TempShip(PlayerData d, ChestResult r)
        {
            var ship = DailyStreak.GrantTempShip(d);
            if (ship == null) return false;
            r.headline = L.T("chest.h_temp_fmt", ship.Name, GameConfig.TempShipHours);
            r.detail = L.T("streak.ship_fmt", ship.Name, GameConfig.TempShipHours);
            r.icon = SpriteFactory.Ship(ship);
            r.iconColor = ShipLook.Of(ship, null).hullColor; // las premium se ven metálicas
            return true;
        }

        // ---------------- ARMA PREMIUM DE PRUEBA ----------------
        public static bool TempEquipActive(PlayerData d) =>
            !string.IsNullOrEmpty(d.tempEquipId) && DateTime.UtcNow.Ticks < d.tempEquipUntilTicks && !d.ownedEquipment.Contains(d.tempEquipId);

        public static TimeSpan TempEquipRemaining(PlayerData d) =>
            TimeSpan.FromTicks(Math.Max(0, d.tempEquipUntilTicks - DateTime.UtcNow.Ticks));

        /// <summary>Quita el arma de prueba al terminar el tiempo (o si ya la compró). Devuelve su id si terminó la prueba sin comprarla.</summary>
        public static string ExpireTempEquip(PlayerData d)
        {
            if (string.IsNullOrEmpty(d.tempEquipId)) return null;
            bool bought = d.ownedEquipment.Contains(d.tempEquipId);
            if (!bought && DateTime.UtcNow.Ticks < d.tempEquipUntilTicks) return null;
            string id = d.tempEquipId;
            d.tempEquipId = "";
            d.tempEquipUntilTicks = 0;
            return bought ? null : id;
        }
    }
}
