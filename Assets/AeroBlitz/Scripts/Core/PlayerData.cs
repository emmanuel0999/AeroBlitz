using System;
using System.Collections.Generic;

namespace AeroBlitz
{
    [Serializable]
    public class ShipState
    {
        public string shipId;
        public int armorLevel;
        public int engineLevel;
        public int magazineLevel;
        public int chargerLevel;
        public string paintId = "default";
        public string trailId = "default";       // estela del motor (sale en cofres)
        public string skin = "";                  // estilo puesto: "cartoon", "metal" (solo premium), "neon"; "" = el de serie
        public List<string> ownedSkins = new List<string>(); // estilos comprados para esta nave (el de serie no hace falta)
        public List<string> equipped = new List<string>();

        public int GetUpgrade(UpgradeStat stat)
        {
            switch (stat)
            {
                case UpgradeStat.Armor: return armorLevel;
                case UpgradeStat.Engine: return engineLevel;
                case UpgradeStat.Magazine: return magazineLevel;
                default: return chargerLevel;
            }
        }

        public void SetUpgrade(UpgradeStat stat, int value)
        {
            switch (stat)
            {
                case UpgradeStat.Armor: armorLevel = value; break;
                case UpgradeStat.Engine: engineLevel = value; break;
                case UpgradeStat.Magazine: magazineLevel = value; break;
                default: chargerLevel = value; break;
            }
        }

        /// <summary>Videos vistos hacia el siguiente nivel de cada mejora (Blindaje, Motor, Cargador, Condensador).</summary>
        public int[] videoProgress = new int[4];

        public int GetVideoProgress(UpgradeStat stat)
        {
            if (videoProgress == null || videoProgress.Length < 4) videoProgress = new int[4];
            return videoProgress[(int)stat];
        }

        public void SetVideoProgress(UpgradeStat stat, int value)
        {
            GetVideoProgress(stat);
            videoProgress[(int)stat] = value;
        }
    }

    [Serializable]
    public class PlayerData
    {
        public string playerId;          // ID local único (se usa también como fallback en el ranking)
        public string username;
        public string countryCode;       // ISO 3166-1 alpha-2
        public bool registered;

        // Cuenta y respaldo en la nube
        public bool googleLinked;        // progreso respaldado en la nube con Google Play Juegos
        public long savedAtUtcTicks;     // última vez que se guardó (para comparar con la nube)

        // Términos y condiciones
        public int acceptedTermsVersion; // 0 = nunca aceptó
        public string acceptedTermsUtc;  // fecha/hora de aceptación (ISO 8601, UTC)

        public int bossesBeaten;         // jefes vencidos (un bit por jefe: nivel 10 = bit 0 ... nivel 70 = bit 6)
        public bool tutorialSeen;        // ya vio el tutorial corto del nivel 1
        public int currentLevel = 1;     // Nivel que el jugador debe jugar ahora
        public int highestUnlocked = 1;
        public long totalPoints;
        public int coins;
        public int lives = GameConfig.DefaultLives;

        public string selectedShip = ShipCatalog.StarterShipId;
        public List<ShipState> ships = new List<ShipState>();
        public List<string> ownedEquipment = new List<string>();
        public List<string> ownedPaints = new List<string> { "default" }; // las partidas antiguas conservan las que ya tenían
        public List<string> ownedProducts = new List<string>(); // no consumibles (remove ads, starter pack...)

        // Transacciones de compra ya entregadas (evita entregar dos veces si la tienda reenvía un pedido)
        public List<string> deliveredTransactions = new List<string>();

        // Último nivel tras el que se pidió valorar el juego (para no repetir)
        public int lastReviewPromptLevel;

        public bool removeAds;
        public bool vip;

        // Vidas: recarga por tiempo y vidas por video (fechas en ticks UTC; 0 = sin temporizador)
        public long livesRefillAtTicks;          // cuándo se recargan todas las vidas
        public int lifeVideoProgress;            // videos vistos hacia la siguiente vida (0..VideosPerLife-1)
        public bool livesV5;                     // ya recibió el paso de 3 a 5 vidas (actualización 1.0.3)
        public long videoLifeWindowStartTicks;   // inicio del período de 5 h de vidas por video
        public int videoLivesInWindow;           // vidas ganadas con video en el período actual

        // Límites diarios
        public string dailyDate = "";
        public int rewardedUpgradesToday;
        public int rewardedCoinsToday;
        public bool dailyRewardClaimed;
        public bool vipLivesClaimed;

        // Misiones: 3 diarias (se renuevan a medianoche) y 1 semanal (cada lunes). Ver Core/Missions.cs
        public string missionsDate = "";
        public List<MissionState> dailyMissions = new List<MissionState>();
        public string weeklyKey = "";
        public MissionState weeklyMission = new MissionState();

        // Racha diaria (Core/DailyStreak.cs)
        public int streakDay;                   // último día de la racha reclamado (1..7; 0 = nunca)
        public string streakLastDate = "";      // fecha del último regalo de racha (yyyy-MM-dd, hora local)
        public string tempShipId = "";          // nave premium de prueba del día 7 ("" = ninguna)
        public long tempShipUntilTicks;         // hasta cuándo se puede usar (ticks UTC)
        public string tempShipPrevSelected = ""; // nave que tenía seleccionada antes de la prueba

        // Cofres (Core/Chests.cs): los que tiene sin abrir, estelas y arma premium de prueba
        public int chestCommon, chestRare, chestEpic;
        public List<string> ownedTrails = new List<string> { "default" };
        public string tempEquipId = "";         // arma premium de prueba del cofre épico ("" = ninguna)
        public long tempEquipUntilTicks;        // hasta cuándo (ticks UTC)
        public int statLevelsCompleted;         // niveles superados desde esta versión (cofre raro cada 5)

        public bool IsTempShip(string id) => !string.IsNullOrEmpty(tempShipId) && tempShipId == id;
        /// <summary>Naves propias (sin contar la de prueba).</summary>
        public int PermanentShipCount => ships.Count - (string.IsNullOrEmpty(tempShipId) ? 0 : 1);

        // Logros (Core/Achievements.cs) y estadísticas que los alimentan
        public List<string> achUnlocked = new List<string>();
        public List<string> achClaimed = new List<string>();
        public bool campaignCompleted;          // superó el nivel 70 alguna vez
        public int statAsteroids;               // asteroides destruidos (total)
        public int statDrones;                  // drones destruidos (total)
        public int statCoinsPicked;             // monedas recogidas en niveles superados
        public int statNoDamageLevels;          // niveles superados sin perder vida
        public int statStormsCleared;           // niveles de tormenta superados (10, 20, 30...)
        public int statMissionsClaimed;         // premios de misiones reclamados
        public int statStreak;                  // niveles seguidos superados sin morir (ahora)
        public int statBestStreak;              // mejor racha

        // Ajustes
        public int language = -1;       // -1 = usar idioma del teléfono
        public bool soundOn = true;
        public bool musicOn = true;
        public bool vibrationOn = true;

        public int levelsPlayedSinceInterstitial;

        public ShipState GetShip(string id)
        {
            return ships.Find(s => s.shipId == id);
        }

        public bool OwnsShip(string id) => GetShip(id) != null;

        public ShipState SelectedShipState
        {
            get
            {
                var s = GetShip(selectedShip);
                if (s == null && ships.Count > 0) { selectedShip = ships[0].shipId; s = ships[0]; }
                return s;
            }
        }

        public ShipState AddShip(string id)
        {
            var existing = GetShip(id);
            if (existing != null) return existing;
            var s = new ShipState { shipId = id };
            ships.Add(s);
            return s;
        }

        public static PlayerData CreateNew()
        {
            var d = new PlayerData { playerId = Guid.NewGuid().ToString("N") };
            d.AddShip(ShipCatalog.StarterShipId);
            return d;
        }

        /// <summary>Reinicia contadores diarios si cambió el día.</summary>
        public void RefreshDaily()
        {
            Missions.Ensure(this); // va antes: las misiones se crean aunque los demás contadores ya sean de hoy
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            if (dailyDate == today) return;
            dailyDate = today;
            rewardedUpgradesToday = 0;
            rewardedCoinsToday = 0;
            dailyRewardClaimed = false;
            vipLivesClaimed = false;
        }
    }
}
