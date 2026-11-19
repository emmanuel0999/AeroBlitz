using System;
using System.Collections.Generic;

namespace AeroBlitz
{
    public class AchievementDefinition
    {
        public string id;
        public int target;
        public int coins;                    // premio al reclamarlo
        public Func<PlayerData, int> progress;

        public string Name => L.T("ach." + id);
        public string Description => L.T("ach." + id + ".desc", UIFactory.Fmt(target));
    }

    /// <summary>
    /// Logros permanentes. Se desbloquean solos al cumplir la condición y sus monedas se reclaman en la pantalla de logros.
    /// Los de progreso (mundos, naves) también se dan a quien ya los había cumplido antes de esta versión.
    /// </summary>
    public static class Achievements
    {
        /// <summary>Niveles superados: 70 solo si terminó la campaña (el nivel actual no pasa de 70).</summary>
        static int LevelsCleared(PlayerData d) => d.campaignCompleted ? GameConfig.TotalLevels : Math.Max(0, d.highestUnlocked - 1);

        public static readonly List<AchievementDefinition> All = new List<AchievementDefinition>
        {
            new AchievementDefinition { id = "first_flight", target = 1, coins = 100, progress = LevelsCleared },
            new AchievementDefinition { id = "world1", target = 7, coins = 300, progress = LevelsCleared },
            new AchievementDefinition { id = "storm", target = 1, coins = 500, progress = d => d.statStormsCleared },
            new AchievementDefinition { id = "survivor", target = 10, coins = 1000, progress = d => d.statBestStreak },
            new AchievementDefinition { id = "untouchable", target = 10, coins = 800, progress = d => d.statNoDamageLevels },
            new AchievementDefinition { id = "collector", target = 500, coins = 600, progress = d => d.statCoinsPicked },
            new AchievementDefinition { id = "drone_hunter", target = 250, coins = 800, progress = d => d.statDrones },
            new AchievementDefinition { id = "asteroid_hunter", target = 1000, coins = 1000, progress = d => d.statAsteroids },
            new AchievementDefinition { id = "fleet", target = 3, coins = 1000, progress = d => d.PermanentShipCount }, // la nave de prueba no cuenta
            new AchievementDefinition { id = "agent", target = 20, coins = 1000, progress = d => d.statMissionsClaimed },
            new AchievementDefinition { id = "world5", target = 35, coins = 1500, progress = LevelsCleared },
            new AchievementDefinition { id = "master", target = GameConfig.TotalLevels, coins = 5000, progress = LevelsCleared },
        };

        public static AchievementDefinition Get(string id) => All.Find(a => a.id == id);

        static void Fix(PlayerData d)
        {
            if (d.achUnlocked == null) d.achUnlocked = new List<string>();
            if (d.achClaimed == null) d.achClaimed = new List<string>();
        }

        public static int Progress(PlayerData d, AchievementDefinition a) => Math.Min(a.target, Math.Max(0, a.progress(d)));
        public static bool IsUnlocked(PlayerData d, AchievementDefinition a) { Fix(d); return d.achUnlocked.Contains(a.id); }
        public static bool IsClaimed(PlayerData d, AchievementDefinition a) { Fix(d); return d.achClaimed.Contains(a.id); }

        public static int UnlockedCount(PlayerData d)
        {
            int n = 0;
            foreach (var a in All) if (IsUnlocked(d, a)) n++;
            return n;
        }

        /// <summary>Desbloquea los logros cuya condición ya se cumple. Devuelve los nuevos.</summary>
        public static List<AchievementDefinition> CheckUnlocks(PlayerData d)
        {
            Fix(d);
            var list = new List<AchievementDefinition>();
            foreach (var a in All)
            {
                if (d.achUnlocked.Contains(a.id) || a.progress(d) < a.target) continue;
                d.achUnlocked.Add(a.id);
                list.Add(a);
            }
            return list;
        }

        public static bool HasClaimable(PlayerData d)
        {
            Fix(d);
            foreach (var a in All) if (d.achUnlocked.Contains(a.id) && !d.achClaimed.Contains(a.id)) return true;
            return false;
        }

        /// <summary>Entrega las monedas del logro. Devuelve cuántas (0 si no se podía reclamar).</summary>
        public static int Claim(PlayerData d, AchievementDefinition a)
        {
            Fix(d);
            if (!d.achUnlocked.Contains(a.id) || d.achClaimed.Contains(a.id)) return 0;
            d.achClaimed.Add(a.id);
            d.coins += a.coins;
            return a.coins;
        }
    }
}
