using System;
using System.Collections.Generic;
using UnityEngine;

namespace AeroBlitz
{
    public enum MissionType { DestroyDrones, NoDamageLevels, CollectCoins, DestroyAsteroids, CompleteLevels }
    public enum MissionReward { Coins, Life, Chest, WeeklyPaint }

    /// <summary>Estado guardado de una misión: progreso y si ya se reclamó el premio.</summary>
    [Serializable]
    public class MissionState
    {
        public string id;
        public int progress;
        public bool claimed;
    }

    public class MissionDefinition
    {
        public string id;
        public MissionType type;
        public int target;
        public MissionReward reward;
        public int amount; // monedas (Coins y WeeklyPaint) o vidas (Life)

        public string Title
        {
            get
            {
                switch (type)
                {
                    case MissionType.DestroyDrones: return L.T("mis.drones_fmt", target);
                    case MissionType.NoDamageLevels: return L.T("mis.nodamage_fmt", target);
                    case MissionType.CollectCoins: return L.T("mis.coins_fmt", target);
                    case MissionType.DestroyAsteroids: return L.T("mis.asteroids_fmt", target);
                    default: return L.T("mis.levels_fmt", target);
                }
            }
        }
    }

    /// <summary>
    /// Misiones diarias (3, se renuevan a medianoche con la hora del teléfono) y una semanal (se renueva cada lunes).
    /// El progreso de destruir se suma en el momento; el de monedas, niveles sin daño y niveles completados, al superar el nivel
    /// (las monedas recogidas solo cuentan si se completa el nivel, igual que en el juego).
    /// </summary>
    public static class Missions
    {
        public static readonly MissionDefinition Drones = new MissionDefinition
            { id = "drones", type = MissionType.DestroyDrones, target = 30, reward = MissionReward.Coins, amount = 200 };
        public static readonly MissionDefinition NoDamage = new MissionDefinition
            { id = "nodamage", type = MissionType.NoDamageLevels, target = 2, reward = MissionReward.Life, amount = 1 };
        public static readonly MissionDefinition Coins = new MissionDefinition
            { id = "coins", type = MissionType.CollectCoins, target = 50, reward = MissionReward.Chest };
        /// <summary>Sustituye a la de monedas para quien aún no llega al nivel 16 (antes no aparecen monedas).</summary>
        public static readonly MissionDefinition Asteroids = new MissionDefinition
            { id = "asteroids", type = MissionType.DestroyAsteroids, target = 40, reward = MissionReward.Chest };
        public static readonly MissionDefinition WeeklyLevels = new MissionDefinition
            { id = "weekly_levels", type = MissionType.CompleteLevels, target = 15, reward = MissionReward.WeeklyPaint, amount = 1000 };

        static readonly MissionDefinition[] All = { Drones, NoDamage, Coins, Asteroids, WeeklyLevels };

        /// <summary>Pinturas que solo se ganan con la misión semanal, en este orden.</summary>
        public static readonly string[] ExclusivePaints = { "neon", "toxic", "midnight", "ruby" };

        public static MissionDefinition Get(string id) => Array.Find(All, m => m.id == id);

        // ---------------- FECHAS ----------------
        static string TodayKey => DateTime.Now.ToString("yyyy-MM-dd");
        static DateTime WeekStart
        {
            get
            {
                var d = DateTime.Now.Date;
                return d.AddDays(-(((int)d.DayOfWeek + 6) % 7)); // lunes de esta semana
            }
        }
        static string WeekKey => WeekStart.ToString("yyyy-MM-dd");
        public static TimeSpan DailyRemaining => DateTime.Now.Date.AddDays(1) - DateTime.Now;
        public static TimeSpan WeeklyRemaining => WeekStart.AddDays(7) - DateTime.Now;

        /// <summary>Crea las misiones del día y de la semana si cambió la fecha (o si aún no existían).</summary>
        public static void Ensure(PlayerData d)
        {
            if (d.dailyMissions == null) d.dailyMissions = new List<MissionState>();
            if (d.missionsDate != TodayKey || d.dailyMissions.Count == 0)
            {
                d.missionsDate = TodayKey;
                d.dailyMissions.Clear();
                var third = d.highestUnlocked >= GameConfig.PickupsFromLevel ? Coins : Asteroids;
                foreach (var m in new[] { Drones, NoDamage, third }) d.dailyMissions.Add(new MissionState { id = m.id });
            }
            if (d.weeklyMission == null || string.IsNullOrEmpty(d.weeklyMission.id) || d.weeklyKey != WeekKey)
            {
                d.weeklyKey = WeekKey;
                d.weeklyMission = new MissionState { id = WeeklyLevels.id };
            }
        }

        static IEnumerable<MissionState> States(PlayerData d)
        {
            foreach (var s in d.dailyMissions) yield return s;
            yield return d.weeklyMission;
        }

        /// <summary>Suma progreso a las misiones de ese tipo. Devuelve las que se acaban de completar.</summary>
        public static List<MissionDefinition> Add(PlayerData d, MissionType type, int amount)
        {
            var done = new List<MissionDefinition>();
            if (amount <= 0) return done;
            Ensure(d);
            foreach (var s in States(d))
            {
                var def = Get(s.id);
                if (def == null || def.type != type || s.claimed || s.progress >= def.target) continue;
                s.progress = Mathf.Min(def.target, s.progress + amount);
                if (s.progress >= def.target) done.Add(def);
            }
            return done;
        }

        public static bool IsComplete(MissionState s)
        {
            var def = Get(s.id);
            return def != null && s.progress >= def.target;
        }

        public static bool HasClaimable(PlayerData d)
        {
            Ensure(d);
            foreach (var s in States(d)) if (!s.claimed && IsComplete(s)) return true;
            return false;
        }

        public static PaintDefinition NextExclusivePaint(PlayerData d)
        {
            foreach (var id in ExclusivePaints)
                if (!d.ownedPaints.Contains(id)) return UpgradeCatalog.GetPaint(id);
            return null;
        }

        public static bool IsExclusivePaint(string paintId) => Array.IndexOf(ExclusivePaints, paintId) >= 0;

        /// <summary>Texto del premio para la lista de misiones.</summary>
        public static string RewardText(MissionDefinition def, PlayerData d)
        {
            switch (def.reward)
            {
                case MissionReward.Coins: return L.T("mis.rw_coins_fmt", def.amount);
                case MissionReward.Life: return L.T("mis.rw_life");
                case MissionReward.Chest: return L.T("mis.rw_chest");
                default:
                    var paint = NextExclusivePaint(d);
                    return paint != null
                        ? L.T("mis.rw_weekly_fmt", paint.Name, UIFactory.Fmt(def.amount))
                        : L.T("mis.rw_coins_fmt", UIFactory.Fmt(def.amount + GameConfig.WeeklyAllPaintsBonus));
            }
        }

        /// <summary>Entrega el premio y marca la misión como reclamada. Devuelve lo que se ganó (vacío si no se podía).</summary>
        public static List<string> Claim(PlayerData d, MissionState s)
        {
            var lines = new List<string>();
            var def = Get(s.id);
            if (def == null || s.claimed || s.progress < def.target) return lines;
            s.claimed = true;
            switch (def.reward)
            {
                case MissionReward.Coins:
                    d.coins += def.amount;
                    lines.Add(L.T("toast.coins_fmt", def.amount));
                    break;
                case MissionReward.Life:
                    d.lives = Mathf.Min(GameConfig.MaxLives, d.lives + def.amount);
                    lines.Add(L.T("toast.plus_life"));
                    break;
                case MissionReward.Chest:
                    lines.AddRange(OpenChest(d));
                    break;
                case MissionReward.WeeklyPaint:
                    var paint = NextExclusivePaint(d);
                    int coins = def.amount + (paint == null ? GameConfig.WeeklyAllPaintsBonus : 0);
                    if (paint != null)
                    {
                        d.ownedPaints.Add(paint.id);
                        lines.Add(L.T("mis.paint_unlocked_fmt", paint.Name));
                    }
                    d.coins += coins;
                    lines.Add(L.T("toast.coins_fmt", UIFactory.Fmt(coins)));
                    break;
            }
            return lines;
        }

        /// <summary>Cofre: siempre monedas, y con suerte una vida y/o una mejora gratis para la nave en uso.</summary>
        static List<string> OpenChest(PlayerData d)
        {
            var lines = new List<string>();
            int coins = Mathf.RoundToInt(UnityEngine.Random.Range(GameConfig.ChestCoinsMin, GameConfig.ChestCoinsMax + 1) / 10f) * 10;
            d.coins += coins;
            lines.Add(L.T("toast.coins_fmt", coins));

            if (UnityEngine.Random.value < GameConfig.ChestLifeChance)
            {
                d.lives = Mathf.Min(GameConfig.MaxLives, d.lives + 1);
                lines.Add(L.T("toast.plus_life"));
            }

            if (UnityEngine.Random.value < GameConfig.ChestUpgradeChance)
            {
                var ship = d.SelectedShipState;
                var options = UpgradeCatalog.Stats.FindAll(st => ship.GetUpgrade(st.stat) < st.MaxLevel);
                if (options.Count > 0)
                {
                    var st = options[UnityEngine.Random.Range(0, options.Count)];
                    ship.SetUpgrade(st.stat, ship.GetUpgrade(st.stat) + 1);
                    lines.Add(L.T("chest.upgrade_fmt", st.Name, ShipCatalog.Get(ship.shipId).Name));
                }
                else
                {
                    d.coins += GameConfig.ChestUpgradeFallbackCoins; // la nave ya tiene todo al máximo
                    lines.Add(L.T("toast.coins_fmt", GameConfig.ChestUpgradeFallbackCoins));
                }
            }
            return lines;
        }
    }
}
