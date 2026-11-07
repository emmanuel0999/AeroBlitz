namespace AeroBlitz
{
    /// <summary>
    /// Configuración central del juego. Todo lo que necesitas cambiar para publicar
    /// (IDs de anuncios, IDs de productos, límites diarios) está aquí.
    /// </summary>
    public static class GameConfig
    {
        public const string GameName = "Aero Blitz";
        public const int TotalLevels = 70;
        public const int EasyLevels = 5;
        public const int DefaultLives = 3;
        public const int MaxLives = 99;

        // ---------------- ANUNCIOS (Unity LevelPlay) ----------------
        // Datos del panel de LevelPlay (https://platform.ironsrc.com): "App Key" de cada plataforma
        // y el "Ad Unit ID" de cada formato. iOS queda pendiente hasta que se publique en la App Store.
        public const string AndroidAppKey = "2863b9aa5";
        public const string IosAppKey = "REEMPLAZAR_IOS_APP_KEY";
        public const string RewardedAndroid = "ivda4ufacc4abczy";     // Rewarded_Android
        public const string RewardedIos = "REEMPLAZAR_REWARDED_IOS";
        public const string InterstitialAndroid = "x6wpyrqxq2oh3ved"; // Interstitial_Android
        public const string InterstitialIos = "REEMPLAZAR_INTERSTITIAL_IOS";
        public const string BannerAndroid = "89fe6vto996k6ctu";       // Banner_Android
        public const string BannerIos = "REEMPLAZAR_BANNER_IOS";
        /// <summary>
        /// Protección de tu cuenta de anuncios: en builds de DESARROLLO (los APK de prueba) los anuncios
        /// son simulados y nunca se cargan anuncios reales. Ponlo en true solo para probar anuncios reales
        /// en un teléfono registrado como "dispositivo de prueba" en el panel de LevelPlay.
        /// La versión de Google Play (no desarrollo) siempre usa anuncios reales.
        /// </summary>
        public const bool RealAdsInDevelopmentBuilds = false;

        /// <summary>Mostrar un intersticial cada N niveles completados/fallados.</summary>
        public const int InterstitialEveryNRuns = 3;
        /// <summary>Segundos mínimos entre intersticiales (no molestar demasiado al jugador).</summary>
        public const float InterstitialMinSeconds = 90f;
        /// <summary>Los primeros niveles no muestran intersticiales para no espantar a jugadores nuevos.</summary>
        public const int NoInterstitialBeforeLevel = 4;

        // ---------------- CUENTA Y GUARDADO EN LA NUBE ----------------
        /// <summary>Clave de Unity Cloud Save donde se guarda todo el progreso del jugador.</summary>
        public const string CloudSaveKey = "progress";
        /// <summary>Segundos de espera tras un cambio antes de subir el progreso (agrupa muchos guardados seguidos).</summary>
        public const float CloudUploadDelay = 3f;

        // ---------------- LEGAL ----------------
        /// <summary>Sube este número cada vez que cambies los términos: se pedirá aceptarlos otra vez.</summary>
        public const int TermsVersion = 1;
        /// <summary>URLs públicas de tus documentos (Google Play exige la de privacidad).</summary>
        public const string TermsUrl = "https://sites.google.com/view/aeroblitz/terms";
        public const string PrivacyUrl = "https://sites.google.com/view/aeroblitz";

        // ---------------- DAÑO ----------------
        /// <summary>Chocar un obstáculo indestructible quita este % de la vida máxima (0.5 = 50%).</summary>
        public const float IndestructibleDamagePercent = 0.5f;
        /// <summary>Desde este nivel los indestructibles quitan menos, porque hay muchos más obstáculos en pantalla.</summary>
        public const int IndestructibleReducedFromLevel = 25;
        public const float IndestructibleDamagePercentLate = 0.25f;

        // ---------------- OBJETOS DURANTE EL NIVEL ----------------
        /// <summary>Desde este nivel aparecen monedas y mejoras temporales que se recogen al pasar por encima.</summary>
        public const int PickupsFromLevel = 16;
        public const int CoinsPerLevelMin = 8, CoinsPerLevelMax = 15;   // del nivel 16 al 70
        public const int CoinValueMin = 5, CoinValueMax = 10;           // valor de cada moneda
        public const float PowerUpInterval = 25f;                       // segundos entre mejoras (aprox.)
        public const float PowerUpShieldSeconds = 13f;
        public const float PowerUpOverchargeSeconds = 11f;
        public const float PowerUpOverchargeMultiplier = 2f;            // la carga va al doble (con tope, ver GameplayController)
        public const float PowerUpMagnetSeconds = 13f;
        public const float PowerUpRepairPercent = 0.25f;

        // ---------------- ARMAS NUEVAS ----------------
        public const int MissilesPerLoad = 4;          // misiles por carga
        public const int MissileLoadsPerLevel = 2;     // cargas por nivel (4 + 4 = 8 misiles)
        public const float MissileReloadSeconds = 20f; // espera entre la primera y la segunda carga
        public const int PlasmaPierce = 3;             // obstáculos rompibles que atraviesa cada disparo de plasma
        /// <summary>Desde este nivel puede caer una prueba de un arma de pago (solo dura ese nivel).</summary>
        public const int TrialsFromLevel = 25;
        public const float TrialChancePerLevel = 0.6f;

        public static float IndestructibleDamageFor(int level) =>
            level >= IndestructibleReducedFromLevel ? IndestructibleDamagePercentLate : IndestructibleDamagePercent;

        // ---------------- VIDAS ----------------
        /// <summary>El contador empieza al perder las 3 vidas; a las N horas se recargan las 3 vidas
        /// automáticamente (también con la aplicación cerrada).</summary>
        public const float LifeRefillHours = 8f;
        /// <summary>Videos necesarios para ganar 1 vida (cada video = 33,3% de la barra).</summary>
        public const int VideosPerLife = 3;
        /// <summary>Máximo de vidas que se pueden ganar con videos dentro de cada período.</summary>
        public const int MaxVideoLivesPerWindow = 2;
        public const float VideoLifeWindowHours = 5f;

        // ---------------- MEJORAS POR VIDEO ----------------
        /// <summary>Videos necesarios para subir 1 nivel de mejora.</summary>
        public const int VideosPerUpgrade = 3;
        /// <summary>Máximo de mejoras que se pueden completar con videos por día.</summary>
        public const int MaxRewardedUpgradesPerDay = 2;

        // Límites diarios de otras recompensas por video (protegen el valor de las compras reales)
        public const int MaxRewardedCoinsPerDay = 5;
        public const int RewardedCoinsAmount = 150;

        // ---------------- RANKING (Unity Gaming Services Leaderboards) ----------------
        public const string LeaderboardId = "aeroblitz_global";
        public const int LeaderboardFetchLimit = 1000;
        /// <summary>Nombre que se pone a un jugador al eliminar su cuenta; esas entradas no se muestran en el ranking.</summary>
        public const string DeletedPlayerName = "cuenta_eliminada";
        /// <summary>Cantidad de pilotos que se muestran en la pantalla de ranking.</summary>
        public const int RankingTopCount = 50;

        // ---------------- ECONOMÍA ----------------
        public const int DailyRewardCoins = 100;
        public const int VipDailyLives = 3;
        public const float VipCoinMultiplier = 2f;
        public const float ReviveHealthPercent = 0.5f;

        // ---------------- MISIONES ----------------
        // Objetivos y premios de cada misión: Core/Missions.cs
        public const int ChestCoinsMin = 150, ChestCoinsMax = 400;   // el cofre siempre da monedas
        public const float ChestLifeChance = 0.35f;                   // probabilidad de +1 vida
        public const float ChestUpgradeChance = 0.15f;                // probabilidad de 1 nivel de mejora gratis
        public const int ChestUpgradeFallbackCoins = 300;             // si la nave ya tiene todas las mejoras
        public const int WeeklyAllPaintsBonus = 500;                  // extra si ya tiene todas las pinturas exclusivas
    }
}
