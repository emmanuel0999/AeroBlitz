using System;
using System.Collections.Generic;
using UnityEngine;

namespace AeroBlitz
{
    public enum StreakRewardType { Coins, Chest, Paint, TempShip }

    public struct StreakReward
    {
        public StreakRewardType type;
        public int amount; // monedas (Coins)
    }

    /// <summary>
    /// Racha diaria de 7 días: cada día seguido que entras el regalo es mejor. Si faltas un día, vuelve al día 1;
    /// después del día 7 empieza otra vez en el 1. Las fechas usan la hora del teléfono, igual que el resto de límites diarios.
    /// </summary>
    public static class DailyStreak
    {
        public static readonly StreakReward[] Days =
        {
            new StreakReward { type = StreakRewardType.Coins, amount = 100 },
            new StreakReward { type = StreakRewardType.Coins, amount = 200 },
            new StreakReward { type = StreakRewardType.Coins, amount = 300 },
            new StreakReward { type = StreakRewardType.Chest },
            new StreakReward { type = StreakRewardType.Paint },
            new StreakReward { type = StreakRewardType.Coins, amount = 1000 },
            new StreakReward { type = StreakRewardType.TempShip },
        };

        /// <summary>Pinturas que solo se ganan el día 5 de la racha, en este orden.</summary>
        public static readonly string[] StreakPaints = { "carbon", "sakura", "mint" };

        static string Today => DateTime.Now.ToString("yyyy-MM-dd");
        static string Yesterday => DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        public static TimeSpan UntilTomorrow => DateTime.Now.Date.AddDays(1) - DateTime.Now;

        public static bool ClaimedToday(PlayerData d) => d.streakLastDate == Today;

        /// <summary>Hoy se puede reclamar (dailyRewardClaimed cubre el regalo antiguo ya cobrado el día de la actualización).</summary>
        public static bool Available(PlayerData d) => !ClaimedToday(d) && !d.dailyRewardClaimed;

        /// <summary>Día de la racha (1..7) que toca hoy, o el que ya se reclamó hoy.</summary>
        public static int CurrentDay(PlayerData d)
        {
            if (ClaimedToday(d)) return Mathf.Clamp(d.streakDay, 1, Days.Length);
            if (d.streakLastDate == Yesterday) return d.streakDay % Days.Length + 1;
            return 1;
        }

        /// <summary>Tenía una racha y faltó al menos un día: se mostrará el aviso de racha perdida.</summary>
        public static bool StreakLost(PlayerData d) =>
            !ClaimedToday(d) && d.streakDay > 1 && !string.IsNullOrEmpty(d.streakLastDate) && d.streakLastDate != Yesterday;

        public static bool IsStreakPaint(string paintId) => Array.IndexOf(StreakPaints, paintId) >= 0;

        static PaintDefinition NextPaint(PlayerData d)
        {
            foreach (var id in StreakPaints) if (!d.ownedPaints.Contains(id)) return UpgradeCatalog.GetPaint(id);
            return null;
        }

        /// <summary>La mejor nave premium que el jugador no tiene (Nova, luego Titán, luego Fénix).</summary>
        static ShipDefinition TrialShipCandidate(PlayerData d)
        {
            for (int i = ShipCatalog.All.Count - 1; i >= 0; i--)
            {
                var s = ShipCatalog.All[i];
                if (s.priceType == PriceType.RealMoney && !d.OwnsShip(s.id)) return s;
            }
            return null;
        }

        /// <summary>Texto corto del premio para las casillas de la racha.</summary>
        public static string CellText(int dayIndex)
        {
            var r = Days[dayIndex];
            switch (r.type)
            {
                case StreakRewardType.Coins: return L.T("streak.rw_coins_fmt", UIFactory.Fmt(r.amount));
                case StreakRewardType.Chest: return L.T("streak.rw_chest");
                case StreakRewardType.Paint: return L.T("streak.rw_paint");
                default: return L.T("streak.rw_ship_fmt", GameConfig.TempShipHours);
            }
        }

        /// <summary>Entrega el regalo de hoy y avanza la racha. Devuelve lo ganado (vacío si ya lo reclamó).</summary>
        public static List<string> Claim(PlayerData d, out StreakRewardType type)
        {
            var lines = new List<string>();
            type = StreakRewardType.Coins;
            if (!Available(d)) return lines;
            int day = CurrentDay(d);
            var r = Days[day - 1];
            type = r.type;
            switch (r.type)
            {
                case StreakRewardType.Coins:
                    d.coins += r.amount;
                    lines.Add(L.T("toast.coins_fmt", UIFactory.Fmt(r.amount)));
                    break;
                case StreakRewardType.Chest:
                    Chests.Add(d, ChestTier.Common); // se abre con animación al reclamar (GameManager.ClaimDaily)
                    lines.Add(L.T("chest.earned_fmt", Chests.Name(ChestTier.Common)));
                    break;
                case StreakRewardType.Paint:
                    var paint = NextPaint(d);
                    if (paint != null)
                    {
                        d.ownedPaints.Add(paint.id);
                        lines.Add(L.T("streak.paint_fmt", paint.Name));
                    }
                    else
                    {
                        d.coins += GameConfig.StreakPaintFallbackCoins; // ya tiene todas las de la racha
                        lines.Add(L.T("toast.coins_fmt", UIFactory.Fmt(GameConfig.StreakPaintFallbackCoins)));
                    }
                    break;
                case StreakRewardType.TempShip:
                    var ship = GrantTempShip(d);
                    if (ship != null) lines.Add(L.T("streak.ship_fmt", ship.Name, GameConfig.TempShipHours));
                    else
                    {
                        d.coins += GameConfig.StreakShipFallbackCoins; // ya tiene todas las naves premium
                        lines.Add(L.T("toast.coins_fmt", UIFactory.Fmt(GameConfig.StreakShipFallbackCoins)));
                    }
                    break;
            }
            d.streakDay = day;
            d.streakLastDate = Today;
            d.dailyRewardClaimed = true;
            return lines;
        }

        /// <summary>Presta 24 h la mejor nave premium que no tiene y la deja seleccionada (día 7 y cofre épico). Null si ya las tiene todas.</summary>
        public static ShipDefinition GrantTempShip(PlayerData d)
        {
            string prevSelected = d.IsTempShip(d.selectedShip) ? d.tempShipPrevSelected : d.selectedShip;
            ExpireTempShip(d, true); // por si quedaba otra prueba
            var ship = TrialShipCandidate(d);
            if (ship == null) return null;
            d.AddShip(ship.id);
            d.tempShipId = ship.id;
            d.tempShipUntilTicks = DateTime.UtcNow.AddHours(GameConfig.TempShipHours).Ticks;
            d.tempShipPrevSelected = d.OwnsShip(prevSelected) ? prevSelected : d.selectedShip;
            d.selectedShip = ship.id; // queda lista para usarla en el próximo nivel
            return ship;
        }

        public static TimeSpan TempShipRemaining(PlayerData d) =>
            TimeSpan.FromTicks(Math.Max(0, d.tempShipUntilTicks - DateTime.UtcNow.Ticks));

        /// <summary>
        /// Quita la nave de prueba cuando se acaba el tiempo (o ya, con force). Si la compró mientras tanto, se la queda.
        /// Devuelve el id de la nave que se quitó, o null.
        /// </summary>
        public static string ExpireTempShip(PlayerData d, bool force = false)
        {
            if (string.IsNullOrEmpty(d.tempShipId)) return null;
            if (!force && DateTime.UtcNow.Ticks < d.tempShipUntilTicks) return null;
            string id = d.tempShipId;
            var def = ShipCatalog.Get(id);
            bool bought = def != null && !string.IsNullOrEmpty(def.productId) && ProductCatalog.IsOwned(d, def.productId);
            if (!bought)
            {
                d.ships.RemoveAll(s => s.shipId == id);
                if (d.ships.Count == 0) d.AddShip(ShipCatalog.StarterShipId);
                if (d.selectedShip == id)
                    d.selectedShip = d.OwnsShip(d.tempShipPrevSelected) ? d.tempShipPrevSelected : d.ships[0].shipId;
            }
            d.tempShipId = "";
            d.tempShipUntilTicks = 0;
            d.tempShipPrevSelected = "";
            return bought ? null : id;
        }
    }
}
