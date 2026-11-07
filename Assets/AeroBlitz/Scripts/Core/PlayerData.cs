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

        public int currentLevel = 1;     // Nivel que el jugador debe jugar ahora
        public int highestUnlocked = 1;
        public long totalPoints;
        public int coins;
        public int lives = GameConfig.DefaultLives;

        public string selectedShip = ShipCatalog.StarterShipId;
        public List<ShipState> ships = new List<ShipState>();
        public List<string> ownedEquipment = new List<string>();
        public List<string> ownedPaints = new List<string> { "default", "red", "green" };
        public List<string> ownedProducts = new List<string>(); // no consumibles (remove ads, starter pack...)

        // Transacciones de compra ya entregadas (evita entregar dos veces si la tienda reenvía un pedido)
        public List<string> deliveredTransactions = new List<string>();

        // Último nivel tras el que se pidió valorar el juego (para no repetir)
        public int lastReviewPromptLevel;

        public bool removeAds;
        public bool vip;

        // Vidas: recarga por tiempo y vidas por video (fechas en ticks UTC; 0 = sin temporizador)
        public long livesRefillAtTicks;          // cuándo se recargan las 3 vidas
        public int lifeVideoProgress;            // videos vistos hacia la siguiente vida (0..VideosPerLife-1)
        public long videoLifeWindowStartTicks;   // inicio del período de 5 h de vidas por video
        public int videoLivesInWindow;           // vidas ganadas con video en el período actual

        // Límites diarios
        public string dailyDate = "";
        public int rewardedUpgradesToday;
        public int rewardedCoinsToday;
        public bool dailyRewardClaimed;
        public bool vipLivesClaimed;

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
