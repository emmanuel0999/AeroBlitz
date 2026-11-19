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
        public const int DefaultLives = 5;
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
        public const float IndestructibleDamagePercentLate = 0.18f;
        /// <summary>Titán (pasiva Coraza): % de la vida que le quitan los indestructibles (niveles 1-24 / desde el 25).</summary>
        public const float IndestructibleArmorPercent = 0.25f;
        public const float IndestructibleArmorPercentLate = 0.10f;

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

        // ---------------- JEFES (niveles 10, 20 ... 70) ----------------
        /// <summary>Un disparo de jefe quita este % de la vida máxima (sube un poco con el nivel).</summary>
        public const float BossShotDamageMin = 0.09f, BossShotDamageMax = 0.14f;
        public const float BossShotArmorFactor = 0.6f;       // Titán (Coraza)
        /// <summary>Durante la pelea el arma carga más rápido: con 3 tiros cada 9 s no se podría vencer a un jefe.</summary>
        public const float BossChargeBoost = 2f;
        /// <summary>Fracción de la vida del jefe que quita cada misil (8 misiles por nivel).</summary>
        public const float BossMissileDamage01 = 0.045f;
        /// <summary>Monedas extra al vencer a un jefe por primera vez (por cada nivel de jefe: 10 → x1, 70 → x7).</summary>
        public const int BossBonusCoinsPerTier = 300;

        public static float BossShotDamageFor(int level) =>
            UnityEngine.Mathf.Lerp(BossShotDamageMin, BossShotDamageMax, UnityEngine.Mathf.InverseLerp(10f, 70f, level));

        public static float IndestructibleDamageFor(int level, bool armor = false) =>
            level >= IndestructibleReducedFromLevel
                ? (armor ? IndestructibleArmorPercentLate : IndestructibleDamagePercentLate)
                : (armor ? IndestructibleArmorPercent : IndestructibleDamagePercent);

        // ---------------- VIDAS ----------------
        /// <summary>El contador empieza al perder la última vida; a las N horas se recargan todas las vidas
        /// (DefaultLives) automáticamente (también con la aplicación cerrada).</summary>
        public const float LifeRefillHours = 4f;
        /// <summary>Videos necesarios para ganar 1 vida (1 = cada video da una vida; los textos dicen "1 video = 1 vida").</summary>
        public const int VideosPerLife = 1;
        /// <summary>Máximo de vidas que se pueden ganar con videos dentro de cada período.</summary>
        public const int MaxVideoLivesPerWindow = 3;
        public const float VideoLifeWindowHours = 4f;

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
        public const int VipDailyLives = 3;
        public const float VipCoinMultiplier = 2f;
        public const float ReviveHealthPercent = 0.5f;

        // ---------------- MISIONES ----------------
        // Objetivos y premios de cada misión: Core/Missions.cs
        public const int WeeklyAllPaintsBonus = 500;                  // extra si ya tiene todas las pinturas exclusivas

        // ---------------- COFRES ----------------
        // Solo se ganan jugando (nunca se venden). Contenido: Core/Chests.cs
        // Común: misión diaria y día 4 de la racha. Raro: cada N niveles. Épico: misión semanal.
        public const int RareChestEveryLevels = 5;
        public const float CommonCoinsChance = 0.80f, CommonLifeChance = 0.15f;   // el 5 % restante: pintura
        public const int CommonCoinsMin = 150, CommonCoinsMax = 400;
        public const float RareCoinsChance = 0.60f, RareItemChance = 0.30f;       // el 10 % restante: pintura o estela
        public const int RareCoinsMin = 500, RareCoinsMax = 1000;
        public const float EpicWeaponChance = 0.40f, EpicShipChance = 0.30f;      // el 30 % restante: estela rara
        public const int TempWeaponHours = 24;                                    // arma premium de prueba
        // Si el premio que tocó ya no está disponible (lo tiene todo), se dan monedas:
        public const int CommonChestFallbackCoins = 300, RareChestFallbackCoins = 800, EpicChestFallbackCoins = 2000;

        // ---------------- NAVES ----------------
        // Estilos (skins) por nave, con monedas. El de serie es gratis: cartoon en las normales, metálico en las premium.
        public const int NeonSkinPrice = 3000;                 // neón: cualquier nave
        public const int CartoonSkinPrice = 1500;              // cartoon para una nave premium
        public const int ChestPaintMaxPrice = 1000;            // los cofres no regalan pinturas más caras (la Oro se compra)
        // Habilidades premium (pasivas en GameplayController.HitPlayer / SpawnVolley)
        public const float RebirthHealthPercent = 0.4f;        // Fénix: renace con este % de vida (1 vez por nivel)
        public const int PiercePassiveExtra = 1;               // Nova: obstáculos extra que atraviesa cada bala
        public const float FlareCooldown = 30f;                // Fénix: Llamarada
        public const float TotalShieldCooldown = 25f, TotalShieldSeconds = 4f;   // Titán: Escudo total
        public const float OverdriveCooldown = 25f, OverdriveSeconds = 3f;       // Nova: Hipervelocidad
        public const float OverdriveFireInterval = 0.14f;
        public const float AbilityFirstCooldown = 6f;          // espera al empezar cada nivel

        // ---------------- RACHA DIARIA ----------------
        // Premios de cada día: Core/DailyStreak.cs
        public const int TempShipHours = 24;                          // día 7: nave premium de prueba
        public const int StreakPaintFallbackCoins = 500;              // día 5 si ya tiene todas las pinturas de la racha
        public const int StreakShipFallbackCoins = 1500;              // día 7 si ya tiene todas las naves premium
    }
}
