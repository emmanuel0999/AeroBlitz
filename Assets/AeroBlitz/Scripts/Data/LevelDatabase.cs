using UnityEngine;

namespace AeroBlitz
{
    public enum ObstacleType { SmallAsteroid, BigAsteroid, Drone, MetalWall, Mine, LaserGate }

    public class LevelConfig
    {
        public int level;
        public float duration;            // segundos que hay que sobrevivir
        public float obstacleSpeed;       // unidades/segundo
        public float spawnInterval;       // segundos entre oleadas
        public float destructibleRatio;   // prob. de que el obstáculo sea rompible
        public float damageMultiplier;
        public int maxPerWave;
        public bool allowDrones, allowMines, allowLaserGates, allowBigAsteroids;
        public int basePoints;
        public int baseCoins;
        public string titleKey;
        public string Title => L.T(titleKey, level);
    }

    /// <summary>
    /// Genera los 70 niveles de forma determinista.
    /// Niveles 1-5: tutorial/sencillos. A partir del 6 la dificultad sube de forma continua,
    /// con "picos" cada 10 niveles (niveles tormenta).
    /// </summary>
    public static class LevelDatabase
    {
        public static LevelConfig Get(int level)
        {
            level = Mathf.Clamp(level, 1, GameConfig.TotalLevels);
            var c = new LevelConfig { level = level };

            if (level <= GameConfig.EasyLevels)
            {
                // Accesibles pero con reto: más obstáculos, más rápidos y más daño que antes.
                // El nivel 5 queda casi igual de exigente que el 6 para que no haya un salto brusco.
                float e = (level - 1) / 4f;
                c.duration = Mathf.Lerp(35f, 42f, e);
                c.obstacleSpeed = Mathf.Lerp(3.2f, 3.8f, e);
                c.spawnInterval = Mathf.Lerp(1.45f, 1.2f, e);
                c.destructibleRatio = Mathf.Lerp(0.7f, 0.65f, e);
                c.damageMultiplier = Mathf.Lerp(0.8f, 0.95f, e);
                c.maxPerWave = level <= 2 ? 1 : 2;
                c.allowBigAsteroids = level >= 2;
                c.allowDrones = level >= 4;
                c.allowMines = false;
                c.allowLaserGates = false;
                c.titleKey = "lvl.training_fmt";
            }
            else
            {
                float t = (level - GameConfig.EasyLevels) / (float)(GameConfig.TotalLevels - GameConfig.EasyLevels); // 0..1
                bool storm = level % 10 == 0;
                c.duration = Mathf.Lerp(42f, 75f, t) + (storm ? 10f : 0f);
                c.obstacleSpeed = Mathf.Lerp(3.8f, 8.2f, t) * (storm ? 1.1f : 1f);
                c.spawnInterval = Mathf.Lerp(1.25f, 0.42f, Mathf.Pow(t, 0.8f)) * (storm ? 0.85f : 1f);
                c.destructibleRatio = Mathf.Lerp(0.65f, 0.45f, t);
                c.damageMultiplier = Mathf.Lerp(1f, 1.7f, t);
                c.maxPerWave = level < 15 ? 2 : level < 35 ? 3 : 4;
                c.allowBigAsteroids = true;
                c.allowDrones = level >= 8;
                c.allowMines = level >= 12;
                c.allowLaserGates = level >= 18;
                c.titleKey = storm ? "lvl.storm" : Sector(level);
            }

            c.basePoints = 100 + level * 25 + (level > GameConfig.EasyLevels ? level * level : 0);
            c.baseCoins = 40 + level * 8;
            return c;
        }

        /// <summary>Nombre del mundo al que pertenece el nivel (10 mundos de 7 niveles).</summary>
        static string Sector(int level) => WorldCatalog.ForLevel(level).nameKey;

        // Estadísticas base por tipo de obstáculo
        public static bool IsDestructible(ObstacleType t) =>
            t == ObstacleType.SmallAsteroid || t == ObstacleType.BigAsteroid || t == ObstacleType.Drone;

        public static float BaseDamage(ObstacleType t)
        {
            switch (t)
            {
                case ObstacleType.SmallAsteroid: return 10f;
                case ObstacleType.BigAsteroid: return 22f;
                case ObstacleType.Drone: return 15f;
                case ObstacleType.MetalWall: return 28f;
                case ObstacleType.Mine: return 40f;
                case ObstacleType.LaserGate: return 32f;
            }
            return 10f;
        }

        public static int BaseHitPoints(ObstacleType t)
        {
            switch (t)
            {
                case ObstacleType.SmallAsteroid: return 1;
                case ObstacleType.BigAsteroid: return 3;
                case ObstacleType.Drone: return 2;
            }
            return int.MaxValue;
        }

        public static int PointsForDestroying(ObstacleType t)
        {
            switch (t)
            {
                case ObstacleType.SmallAsteroid: return 10;
                case ObstacleType.BigAsteroid: return 30;
                case ObstacleType.Drone: return 25;
            }
            return 0;
        }
    }
}
