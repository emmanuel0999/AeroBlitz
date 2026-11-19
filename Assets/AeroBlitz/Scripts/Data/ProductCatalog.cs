using System.Collections.Generic;

namespace AeroBlitz
{
    /// <summary>
    /// IDs de productos de compra real. Deben crearse EXACTAMENTE igual en
    /// Google Play Console y App Store Connect.
    /// </summary>
    public static class ProductIds
    {
        public const string Lives5 = "aeroblitz.lives5";
        public const string Lives15 = "aeroblitz.lives15";
        public const string CoinsSmall = "aeroblitz.coins_small";
        public const string CoinsMedium = "aeroblitz.coins_medium";
        public const string CoinsLarge = "aeroblitz.coins_large";
        public const string StarterPack = "aeroblitz.starter_pack";
        public const string RemoveAds = "aeroblitz.remove_ads";
        public const string VipMonthly = "aeroblitz.vip_monthly";
        public const string ShipPhoenix = "aeroblitz.ship_phoenix";
        public const string ShipTitan = "aeroblitz.ship_titan";
        public const string ShipNova = "aeroblitz.ship_nova";
        public const string EqPlasmaShield = "aeroblitz.eq_plasma_shield";
        public const string EqTwinCannon = "aeroblitz.eq_twin_cannon";
        public const string EqQuantumCore = "aeroblitz.eq_quantum_core";
        public const string EqTitaniumArmor = "aeroblitz.eq_titanium_armor";
        public const string EqIonThrusters = "aeroblitz.eq_ion_thrusters";
        public const string EqTripleCannon = "aeroblitz.eq_triple_cannon";
        public const string EqPlasmaCannon = "aeroblitz.eq_plasma_cannon";
        public const string EqMissiles = "aeroblitz.eq_missiles";
        public const string PaintGold = "aeroblitz.paint_gold";
    }

    public enum ProductKind { Consumable, NonConsumable, Subscription }

    public class ProductDefinition
    {
        public string id;
        public ProductKind kind;
        string Key => id.StartsWith("aeroblitz.") ? id.Substring(10) : id;
        public string Title => L.T("prod." + Key);
        public string Description => L.T("prod." + Key + ".desc");
        public string fallbackPrice; // se muestra si la tienda aún no devolvió el precio localizado
        public string category;
        public bool hidden;          // ya no se vende (sigue registrado para restaurar compras antiguas)
    }

    public static class ProductCatalog
    {
        public const string CatOffers = "offers";
        public const string CatLives = "lives";
        public const string CatCoins = "coins";
        public const string CatShips = "ships";
        public const string CatEquipment = "equipment";

        public static readonly List<ProductDefinition> All = new List<ProductDefinition>
        {
            new ProductDefinition { id = ProductIds.StarterPack, kind = ProductKind.NonConsumable, category = CatOffers, fallbackPrice = "US$1.99" },
            new ProductDefinition { id = ProductIds.VipMonthly, kind = ProductKind.Subscription, category = CatOffers, fallbackPrice = "US$4.99/mes" },
            new ProductDefinition { id = ProductIds.RemoveAds, kind = ProductKind.NonConsumable, category = CatOffers, fallbackPrice = "US$2.99" },
            new ProductDefinition { id = ProductIds.Lives5, kind = ProductKind.Consumable, category = CatLives, fallbackPrice = "US$0.99" },
            new ProductDefinition { id = ProductIds.Lives15, kind = ProductKind.Consumable, category = CatLives, fallbackPrice = "US$1.99" },
            new ProductDefinition { id = ProductIds.CoinsSmall, kind = ProductKind.Consumable, category = CatCoins, fallbackPrice = "US$0.99" },
            new ProductDefinition { id = ProductIds.CoinsMedium, kind = ProductKind.Consumable, category = CatCoins, fallbackPrice = "US$2.99" },
            new ProductDefinition { id = ProductIds.CoinsLarge, kind = ProductKind.Consumable, category = CatCoins, fallbackPrice = "US$4.99" },
            new ProductDefinition { id = ProductIds.ShipPhoenix, kind = ProductKind.NonConsumable, category = CatShips, fallbackPrice = "US$2.99" },
            new ProductDefinition { id = ProductIds.ShipTitan, kind = ProductKind.NonConsumable, category = CatShips, fallbackPrice = "US$4.99" },
            new ProductDefinition { id = ProductIds.ShipNova, kind = ProductKind.NonConsumable, category = CatShips, fallbackPrice = "US$6.99" },
            new ProductDefinition { id = ProductIds.EqPlasmaShield, kind = ProductKind.NonConsumable, category = CatEquipment, fallbackPrice = "US$1.99" },
            new ProductDefinition { id = ProductIds.EqTwinCannon, kind = ProductKind.NonConsumable, category = CatEquipment, fallbackPrice = "US$1.99" },
            new ProductDefinition { id = ProductIds.EqQuantumCore, kind = ProductKind.NonConsumable, category = CatEquipment, fallbackPrice = "US$2.99" },
            new ProductDefinition { id = ProductIds.EqTitaniumArmor, kind = ProductKind.NonConsumable, category = CatEquipment, fallbackPrice = "US$2.99" },
            new ProductDefinition { id = ProductIds.EqIonThrusters, kind = ProductKind.NonConsumable, category = CatEquipment, fallbackPrice = "US$1.49" },
            new ProductDefinition { id = ProductIds.EqTripleCannon, kind = ProductKind.NonConsumable, category = CatEquipment, fallbackPrice = "US$2.99" },
            new ProductDefinition { id = ProductIds.EqPlasmaCannon, kind = ProductKind.NonConsumable, category = CatEquipment, fallbackPrice = "US$3.99" },
            new ProductDefinition { id = ProductIds.EqMissiles, kind = ProductKind.NonConsumable, category = CatEquipment, fallbackPrice = "US$3.99" },
            // La pintura Oro ahora se compra con monedas: el producto queda oculto solo para restaurar compras antiguas
            new ProductDefinition { id = ProductIds.PaintGold, kind = ProductKind.NonConsumable, category = CatEquipment, fallbackPrice = "US$0.99", hidden = true },
        };

        public static ProductDefinition Get(string id) => All.Find(p => p.id == id);

        /// <summary>Entrega el contenido de una compra ya validada por la tienda.</summary>
        public static void Grant(PlayerData d, string productId)
        {
            switch (productId)
            {
                case ProductIds.Lives5: d.lives = System.Math.Min(GameConfig.MaxLives, d.lives + 5); break;
                case ProductIds.Lives15: d.lives = System.Math.Min(GameConfig.MaxLives, d.lives + 15); break;
                case ProductIds.CoinsSmall: d.coins += 2000; break;
                case ProductIds.CoinsMedium: d.coins += 7000; break;
                case ProductIds.CoinsLarge: d.coins += 15000; break;
                case ProductIds.StarterPack:
                    if (d.ownedProducts.Contains(productId)) break;
                    d.lives = System.Math.Min(GameConfig.MaxLives, d.lives + 5);
                    d.coins += 3000;
                    if (!d.ownedEquipment.Contains("eq_shield")) d.ownedEquipment.Add("eq_shield");
                    break;
                case ProductIds.RemoveAds: d.removeAds = true; break;
                case ProductIds.VipMonthly: d.vip = true; break;
                default:
                    var ship = ShipCatalog.ByProduct(productId);
                    if (ship != null)
                    {
                        d.AddShip(ship.id);
                        if (d.IsTempShip(ship.id)) { d.tempShipId = ""; d.tempShipUntilTicks = 0; d.tempShipPrevSelected = ""; } // compró la de prueba: ya es suya
                        break;
                    }
                    var eq = UpgradeCatalog.EquipmentByProduct(productId);
                    if (eq != null) { if (!d.ownedEquipment.Contains(eq.id)) d.ownedEquipment.Add(eq.id); break; }
                    var paint = UpgradeCatalog.PaintByProduct(productId);
                    if (paint != null && !d.ownedPaints.Contains(paint.id)) d.ownedPaints.Add(paint.id);
                    break;
            }

            var def = Get(productId);
            if (def != null && def.kind != ProductKind.Consumable && !d.ownedProducts.Contains(productId))
                d.ownedProducts.Add(productId);
        }

        public static bool IsOwned(PlayerData d, string productId) => d.ownedProducts.Contains(productId);
    }
}
