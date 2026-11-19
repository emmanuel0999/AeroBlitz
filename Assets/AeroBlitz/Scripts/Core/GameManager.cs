using System;
using System.Collections.Generic;
using UnityEngine;

namespace AeroBlitz
{
    public struct LevelResult
    {
        public int level;
        public int destroyed;
        public int killPoints;
        public float healthPercent;
        public int pickupCoins;     // monedas recogidas durante el nivel
        public string trialProductId; // arma de pago que se probó en el nivel (o null)
        public int coinPickups;       // cantidad de monedas recogidas (misiones)
        public bool tookDamage;       // perdió vida durante el nivel (misiones)
        public bool died;             // la nave explotó y revivió con un video (rompe la racha de "Sobreviviente")
        public bool bossDefeated;     // el nivel era de jefe y se venció
    }

    /// <summary>
    /// Punto central del juego: datos del jugador, progreso, economía y conexión entre sistemas.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public PlayerData Data { get; private set; }
        public event Action DataChanged;

        public GameplayController Gameplay { get; private set; }

        int _lastEarnedCoins;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoCreate()
        {
            if (Instance == null && FindAnyObjectByType<GameManager>() == null)
                new GameObject("AeroBlitz").AddComponent<GameManager>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Data = SaveSystem.Load() ?? PlayerData.CreateNew();
            if (Data.ships.Count == 0) Data.AddShip(ShipCatalog.StarterShipId);
            MigrateLives(Data);
            Data.RefreshDaily();
            L.Set(Data.language >= 0 ? (Language)Data.language : L.FromSystem());

            gameObject.AddComponent<AudioFX>();
            gameObject.AddComponent<MusicPlayer>();
            gameObject.AddComponent<AdsManager>();
            gameObject.AddComponent<IAPManager>();
            gameObject.AddComponent<LeaderboardService>();
            gameObject.AddComponent<AccountService>();
            Gameplay = new GameObject("Gameplay").AddComponent<GameplayController>();
            Gameplay.transform.SetParent(transform);
            new GameObject("UI").AddComponent<UIManager>().transform.SetParent(transform);
        }

        async void Start()
        {
            if (!Data.registered) UIManager.Instance.ShowScreen<RegistrationScreen>();
            else UIManager.Instance.ShowScreen<MainMenuScreen>();
            UpdateLifeTimers(); // aplica la recarga si pasaron las horas de espera con el juego cerrado
            CheckTermsAccepted();

            AdsManager.Instance.Initialize();
            await LeaderboardService.Instance.InitializeAsync(); // también inicializa Unity Services
            IAPManager.Instance.Initialize();
            NotifyChanged();
            // El menú se mostró antes de conectar: ahora que hay conexión, se actualiza el ranking.
            var menu = UIManager.Instance.Get<MainMenuScreen>();
            if (menu != null && menu.gameObject.activeInHierarchy) menu.LoadRanking();
        }

        // ---------------- TÉRMINOS Y CONDICIONES ----------------
        public bool TermsAccepted => Data.acceptedTermsVersion >= GameConfig.TermsVersion;

        public void AcceptTerms()
        {
            Data.acceptedTermsVersion = GameConfig.TermsVersion;
            Data.acceptedTermsUtc = DateTime.UtcNow.ToString("o");
            Save();
        }

        /// <summary>Jugadores ya registrados deben aceptar los términos nuevos si cambió la versión.</summary>
        void CheckTermsAccepted()
        {
            if (Data.registered && !TermsAccepted) UIManager.Instance.ShowTermsUpdate();
        }

        /// <summary>Reemplaza todo el progreso (al recuperar desde la nube).</summary>
        public void ReplaceData(PlayerData data)
        {
            Gameplay.Stop();
            Data = data;
            if (Data.ships.Count == 0) Data.AddShip(ShipCatalog.StarterShipId);
            MigrateLives(Data);
            Data.RefreshDaily();
            SaveSystem.Save(Data);
            UpdateLifeTimers();
            NotifyChanged();
            UIManager.Instance.ShowFreshScreen(Data.registered ? typeof(MainMenuScreen) : typeof(RegistrationScreen));
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) { Save(); Gameplay.PauseIfRunning(); }
            else if (Data != null) { Data.RefreshDaily(); UpdateLifeTimers(); NotifyChanged(); }
        }

        void OnApplicationQuit() => Save();

        public void Save()
        {
#if UNITY_EDITOR
            if (TrailerCheats.Active) return; // grabar el tráiler no altera la partida guardada
#endif
            Data.savedAtUtcTicks = DateTime.UtcNow.Ticks;
            SaveSystem.Save(Data);
            AccountService.Instance?.ScheduleUpload(); // respaldo en la nube si inició sesión con Google
        }
        public void NotifyChanged()
        {
            CheckAchievements(true); // naves compradas, misiones reclamadas, progreso recuperado de la nube...
            DataChanged?.Invoke();
        }

        // ---------------- IDIOMA ----------------
        public void SetLanguage(Language lang)
        {
            Data.language = (int)lang;
            Save();
            L.Set(lang);
            UIManager.Instance.RebuildForLanguage();
        }

        // ---------------- REGISTRO ----------------
        public void Register(string username, string countryCode)
        {
            Data.username = username;
            Data.countryCode = countryCode;
            Data.registered = true;
            Save();
            _ = LeaderboardService.Instance.SyncProfileAsync(username, countryCode, Data.totalPoints);
            NotifyChanged();
        }

        // ---------------- FLUJO DE NIVELES ----------------
        public void PlayCurrentLevel() => PlayLevel(Data.currentLevel);

        /// <summary>Juega un nivel concreto: el actual, o el jefe de un nivel que el jugador ya había pasado (reto opcional).</summary>
        public void PlayLevel(int level)
        {
            UpdateLifeTimers();
            if (Data.lives <= 0) { UIManager.Instance.ShowLivesPanel(true); return; }
            AdsManager.Instance.HideBanner();
            UIManager.Instance.ShowScreen<HUDScreen>();
            Gameplay.Begin(level, ShipStats.Compute(Data.SelectedShipState), Data.SelectedShipState.shipId);
        }

        public void ReturnToMenu()
        {
            Gameplay.Stop();
            UIManager.Instance.ShowScreen<MainMenuScreen>();
        }

        /// <summary>El nivel se superó: se suman puntos y dinero y se guarda el progreso.</summary>
        public void OnLevelCompleted(LevelResult r)
        {
            var cfg = LevelDatabase.Get(r.level);
            int healthBonus = Mathf.RoundToInt(cfg.basePoints * 0.5f * r.healthPercent);
            int points = cfg.basePoints + r.killPoints + healthBonus;
            int coins = Mathf.RoundToInt(cfg.baseCoins * (Data.vip ? GameConfig.VipCoinMultiplier : 1f)) + r.pickupCoins;

            // Un jefe da puntos y premio solo la primera vez (así retarlo no infla el ranking ni se puede repetir para sacar monedas)
            bool bossFirst = r.bossDefeated && !BossProgress.IsBeaten(Data, r.level);
            if (r.bossDefeated && !bossFirst) { points = 0; coins = r.pickupCoins; }
            // Reto opcional: jefe de un nivel que el jugador ya había pasado (no es un nivel nuevo)
            bool challenge = r.bossDefeated && r.level < Data.currentLevel;
            if (r.bossDefeated) BossProgress.MarkBeaten(Data, r.level);

            Data.totalPoints += points;
            Data.coins += coins;
            _lastEarnedCoins = coins;

            // Misiones, logros y cofres que dependen de superar el nivel
            var notes = new List<string>();
            bool chestEarned = false;
            if (TrackProgress)
            {
                var done = new List<MissionDefinition>();
                done.AddRange(Missions.Add(Data, MissionType.CollectCoins, r.coinPickups));
                if (!r.tookDamage) done.AddRange(Missions.Add(Data, MissionType.NoDamageLevels, 1));
                if (!challenge) done.AddRange(Missions.Add(Data, MissionType.CompleteLevels, 1));
                foreach (var m in done) notes.Add(L.T("mis.level_done_fmt", m.Title));

                Data.statCoinsPicked += r.coinPickups;
                if (!r.tookDamage) Data.statNoDamageLevels++;
                if (r.level % 10 == 0 && (!r.bossDefeated || bossFirst)) Data.statStormsCleared++;
                if (r.level >= GameConfig.TotalLevels) Data.campaignCompleted = true;
                if (r.died) Data.statStreak = 0;
                else if (!challenge) { Data.statStreak++; Data.statBestStreak = Mathf.Max(Data.statBestStreak, Data.statStreak); }

                // Jefe vencido por primera vez: monedas extra y un cofre raro
                if (bossFirst)
                {
                    int bonus = GameConfig.BossBonusCoinsPerTier * (r.level / 10);
                    Data.coins += bonus;
                    coins += bonus;
                    _lastEarnedCoins = coins;
                    Chests.Add(Data, ChestTier.Rare);
                    chestEarned = true;
                    notes.Add(L.T("boss.reward_fmt", bonus));
                }

                // Cofre raro cada N niveles superados (el reto de un jefe saltado no cuenta como nivel nuevo)
                if (!challenge) Data.statLevelsCompleted++;
                if (!challenge && Data.statLevelsCompleted % GameConfig.RareChestEveryLevels == 0)
                {
                    Chests.Add(Data, ChestTier.Rare);
                    chestEarned = true;
                    notes.Add(L.T("chest.level_earned_fmt", Chests.Name(ChestTier.Rare), GameConfig.RareChestEveryLevels));
                }
            }

            bool campaignDone = r.level >= GameConfig.TotalLevels;
            if (!campaignDone && r.level == Data.currentLevel)
            {
                Data.currentLevel = r.level + 1;
                Data.highestUnlocked = Mathf.Max(Data.highestUnlocked, Data.currentLevel);
            }
            // Se anuncian en la pantalla de nivel completado (sin aviso flotante)
            foreach (var a in CheckAchievements(false)) notes.Add(L.T("ach.level_done_fmt", a.Name));
            Save();
#if UNITY_EDITOR
            if (!TrailerCheats.Active)
#endif
            LeaderboardService.Instance.SubmitScore(Data.totalPoints, Data.countryCode);
            NotifyChanged();
            AudioFX.Instance.Play(AudioFX.Sfx.Win);

            UIManager.Instance.ShowLevelComplete(r.level, points, coins, r.destroyed, campaignDone, r.pickupCoins, r.trialProductId, notes, chestEarned);
            // Momento positivo: se pide valorar (solo en ciertos niveles y una vez en cada uno)
            PlatformServices.MaybeRequestReview(r.level);
        }

        /// <summary>Recompensa extra: ver un video duplica las monedas del nivel.</summary>
        public void DoubleLastCoins(Action onDone)
        {
            AdsManager.Instance.ShowRewarded(ok =>
            {
                if (ok) { Data.coins += _lastEarnedCoins; Save(); NotifyChanged(); UIManager.Instance.Toast(L.T("toast.coins_fmt", _lastEarnedCoins)); }
                onDone?.Invoke();
            });
        }

        /// <summary>La nave fue destruida o el jugador abandonó: se pierde una vida.</summary>
        public void OnLevelFailed(int level)
        {
            if (TrackProgress) Data.statStreak = 0; // perder un nivel rompe la racha de "Sobreviviente"
            LoseLife();
            Save();
            NotifyChanged();
            AudioFX.Instance.Play(AudioFX.Sfx.Lose);
            UIManager.Instance.ShowLevelFailed(level);
        }

        public void AfterRunInterstitial(int level, Action next)
        {
            AdsManager.Instance.MaybeShowInterstitial(level, () => { Save(); next?.Invoke(); });
        }

        // ---------------- ECONOMÍA ----------------
        public bool SpendCoins(int amount)
        {
            if (Data.coins < amount) { UIManager.Instance.Toast(L.T("toast.no_coins")); return false; }
            Data.coins -= amount;
            Save();
            NotifyChanged();
            return true;
        }

        public void BuyShipWithCoins(ShipDefinition ship)
        {
            if (Data.OwnsShip(ship.id)) return;
            if (!SpendCoins(ship.coinPrice)) return;
            Data.AddShip(ship.id);
            Data.selectedShip = ship.id;
            Save(); NotifyChanged();
            UIManager.Instance.Toast(L.T("toast.ship_unlocked_fmt", ship.Name));
        }

        public void BuyUpgradeWithCoins(ShipState ship, UpgradeStat stat)
        {
            var def = UpgradeCatalog.GetStat(stat);
            int lvl = ship.GetUpgrade(stat);
            if (lvl >= def.MaxLevel) return;
            if (!SpendCoins(def.coinCosts[lvl])) return;
            ship.SetUpgrade(stat, lvl + 1);
            Save(); NotifyChanged();
            AudioFX.Instance.Play(AudioFX.Sfx.Upgrade);
        }

        /// <summary>Compra un estilo (skin) para una nave con monedas y se lo pone. La nave de prueba no puede comprar estilos.</summary>
        public void BuySkinWithCoins(ShipState ship, ShipSkin skin)
        {
            var def = ShipCatalog.Get(ship.shipId);
            if (!ShipSkins.Available(def, skin)) return;
            if (ShipSkins.Owns(ship, def, skin)) { ship.skin = ShipSkins.Id(skin); Save(); NotifyChanged(); return; }
            if (Data.IsTempShip(ship.shipId)) { UIManager.Instance.Toast(L.T("toast.skin_temp_ship")); return; }
            if (!SpendCoins(ShipSkins.Price(def, skin))) return;
            if (ship.ownedSkins == null) ship.ownedSkins = new List<string>();
            ship.ownedSkins.Add(ShipSkins.Id(skin));
            ship.skin = ShipSkins.Id(skin);
            Save(); NotifyChanged();
            AudioFX.Instance.Play(AudioFX.Sfx.Upgrade);
            UIManager.Instance.Toast(L.T("toast.skin_bought_fmt", ShipSkins.Name(skin), def.Name));
        }

        public void BuyPaintWithCoins(PaintDefinition paint)
        {
            if (Data.ownedPaints.Contains(paint.id)) return;
            if (!SpendCoins(paint.coinPrice)) return;
            Data.ownedPaints.Add(paint.id);
            Save(); NotifyChanged();
        }

        /// <summary>Compra equipamiento básico con monedas y lo deja equipado en la nave indicada.</summary>
        public void BuyEquipmentWithCoins(EquipmentDefinition eq, ShipState ship)
        {
            if (eq == null || !eq.IsCoin || Data.ownedEquipment.Contains(eq.id)) return;
            if (!SpendCoins(eq.coinPrice)) return;
            Data.ownedEquipment.Add(eq.id);
            if (ship != null && !ship.equipped.Contains(eq.id)) ship.equipped.Add(eq.id);
            Save(); NotifyChanged();
            AudioFX.Instance.Play(AudioFX.Sfx.Upgrade);
            UIManager.Instance.Toast(L.T("toast.equipment_bought_fmt", eq.Name));
        }

        public void BuyProduct(string productId)
        {
            IAPManager.Instance.Buy(productId, (ok, msg) =>
            {
                UIManager.Instance.Toast(msg);
                if (ok) AudioFX.Instance.Play(AudioFX.Sfx.Upgrade);
            });
        }

        // ---------------- VIDAS: RECARGA POR TIEMPO Y VIDAS POR VIDEO ----------------
        static long NowTicks => DateTime.UtcNow.Ticks;
        static long Hours(float h) => TimeSpan.FromHours(h).Ticks;

        float _timerTick;

        void Update()
        {
            _timerTick -= Time.unscaledDeltaTime;
            if (_timerTick > 0f) return;
            _timerTick = 1f;
            UpdateLifeTimers();
        }

        public bool RefillRunning => Data.livesRefillAtTicks > 0;
        public TimeSpan RefillRemaining => TimeSpan.FromTicks(Math.Max(0, Data.livesRefillAtTicks - NowTicks));

        public bool VideoLifeLimitReached => Data.videoLivesInWindow >= GameConfig.MaxVideoLivesPerWindow;
        public TimeSpan VideoWindowRemaining =>
            TimeSpan.FromTicks(Math.Max(0, Data.videoLifeWindowStartTicks + Hours(GameConfig.VideoLifeWindowHours) - NowTicks));

        /// <summary>Se puede ver video por vida si faltan vidas y no se alcanzó el límite del período.</summary>
        public bool CanWatchForLife => Data.lives < GameConfig.DefaultLives && !VideoLifeLimitReached;

        public static string FormatTime(TimeSpan t) => $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}";

        /// <summary>Revisa la recarga completa de vidas y el período de vidas por video (también funciona con el juego cerrado).</summary>
        public void UpdateLifeTimers()
        {
            if (Data == null) return;
            CheckTempShip();
            bool changed = false;

            if (RefillRunning && (NowTicks >= Data.livesRefillAtTicks || Data.lives >= GameConfig.DefaultLives))
            {
                bool refilled = Data.lives < GameConfig.DefaultLives;
                Data.lives = Mathf.Max(Data.lives, GameConfig.DefaultLives);
                Data.livesRefillAtTicks = 0;
                changed = true;
                if (refilled) UIManager.Instance?.Toast(L.T("toast.lives_refilled"));
            }

            if (Data.videoLifeWindowStartTicks > 0 && NowTicks - Data.videoLifeWindowStartTicks >= Hours(GameConfig.VideoLifeWindowHours))
            {
                Data.videoLifeWindowStartTicks = 0;
                Data.videoLivesInWindow = 0;
                changed = true;
            }

            if (changed) { Save(); NotifyChanged(); }
        }

        /// <summary>Al pasar de 3 a 5 vidas, quien ya jugaba recibe las 5 vidas una sola vez y el progreso de videos de antes se reinicia.</summary>
        static void MigrateLives(PlayerData d)
        {
            if (d.livesV5) return;
            d.livesV5 = true;
            if (d.lives < GameConfig.DefaultLives) d.lives = GameConfig.DefaultLives;
            d.livesRefillAtTicks = 0;
            d.lifeVideoProgress = 0;
        }

        void LoseLife()
        {
            Data.lives = Mathf.Max(0, Data.lives - 1);
            // Sin vidas: arranca la espera (LifeRefillHours) para recargar todas las vidas
            if (Data.lives == 0 && !RefillRunning)
                Data.livesRefillAtTicks = NowTicks + Hours(GameConfig.LifeRefillHours);
        }

        /// <summary>Cada video da 1 vida (VideosPerLife), con un máximo de MaxVideoLivesPerWindow por período.</summary>
        public void WatchAdForLife(Action onDone = null)
        {
            UpdateLifeTimers();
            if (Data.lives >= GameConfig.DefaultLives) { UIManager.Instance.Toast(L.T("lives.full")); return; }
            if (VideoLifeLimitReached) { UIManager.Instance.Toast(L.T("lives.window_limit_fmt", FormatTime(VideoWindowRemaining))); return; }
            AdsManager.Instance.ShowRewarded(ok =>
            {
                if (ok)
                {
                    Data.lifeVideoProgress++;
                    if (Data.lifeVideoProgress >= GameConfig.VideosPerLife)
                    {
                        Data.lifeVideoProgress = 0;
                        Data.lives = Mathf.Min(GameConfig.MaxLives, Data.lives + 1);
                        if (Data.videoLivesInWindow == 0) Data.videoLifeWindowStartTicks = NowTicks;
                        Data.videoLivesInWindow++;
                        AudioFX.Instance.Play(AudioFX.Sfx.Upgrade);
                        UIManager.Instance.Toast(L.T("toast.plus_life"));
                    }
                    else UIManager.Instance.Toast(L.T("toast.life_video_fmt", Data.lifeVideoProgress, GameConfig.VideosPerLife));
                    Save(); NotifyChanged();
                }
                onDone?.Invoke();
            });
        }

        // ---------------- RECOMPENSAS POR VIDEO ----------------
        public int RewardedUpgradesLeft => GameConfig.MaxRewardedUpgradesPerDay - Data.rewardedUpgradesToday;
        public int RewardedCoinsLeft => GameConfig.MaxRewardedCoinsPerDay - Data.rewardedCoinsToday;

        /// <summary>Cada mejora por video requiere 3 videos (máx. 2 mejoras por video al día).</summary>
        public void WatchAdForUpgrade(ShipState ship, UpgradeStat stat)
        {
            Data.RefreshDaily();
            var def = UpgradeCatalog.GetStat(stat);
            if (ship.GetUpgrade(stat) >= def.MaxLevel) return;
            if (RewardedUpgradesLeft <= 0) { UIManager.Instance.Toast(L.T("toast.upgrade_limit")); return; }
            AdsManager.Instance.ShowRewarded(ok =>
            {
                if (!ok) return;
                int progress = ship.GetVideoProgress(stat) + 1;
                if (progress >= GameConfig.VideosPerUpgrade)
                {
                    ship.SetVideoProgress(stat, 0);
                    Data.rewardedUpgradesToday++;
                    ship.SetUpgrade(stat, ship.GetUpgrade(stat) + 1);
                    AudioFX.Instance.Play(AudioFX.Sfx.Upgrade);
                    UIManager.Instance.Toast(L.T("toast.upgraded_fmt", def.Name));
                }
                else
                {
                    ship.SetVideoProgress(stat, progress);
                    UIManager.Instance.Toast(L.T("toast.upgrade_video_fmt", def.Name, progress, GameConfig.VideosPerUpgrade));
                }
                Save(); NotifyChanged();
            });
        }

        public void WatchAdForCoins()
        {
            Data.RefreshDaily();
            if (RewardedCoinsLeft <= 0) { UIManager.Instance.Toast(L.T("toast.daily_limit")); return; }
            AdsManager.Instance.ShowRewarded(ok =>
            {
                if (!ok) return;
                Data.rewardedCoinsToday++;
                Data.coins += GameConfig.RewardedCoinsAmount;
                Save(); NotifyChanged();
                UIManager.Instance.Toast(L.T("toast.coins_fmt", GameConfig.RewardedCoinsAmount));
            });
        }

        // ---------------- MISIONES Y LOGROS ----------------
        /// <summary>Grabar el tráiler (solo en el Editor) no cuenta para misiones ni logros.</summary>
        static bool TrackProgress
        {
            get
            {
#if UNITY_EDITOR
                if (TrailerCheats.Active) return false;
#endif
                return true;
            }
        }

        /// <summary>Un obstáculo destruido en la partida: suma a estadísticas, misiones y logros al momento.</summary>
        public void ObstacleDestroyed(ObstacleType t)
        {
            if (!TrackProgress) return;
            if (t == ObstacleType.Drone) { Data.statDrones++; MissionEvent(MissionType.DestroyDrones, 1); }
            else if (t == ObstacleType.SmallAsteroid || t == ObstacleType.BigAsteroid) { Data.statAsteroids++; MissionEvent(MissionType.DestroyAsteroids, 1); }
            else return;
            CheckAchievements(true);
        }

        /// <summary>Progreso que se suma durante la partida (drones y asteroides). Avisa si se completa una misión.</summary>
        void MissionEvent(MissionType type, int amount)
        {
            if (!TrackProgress) return;
            foreach (var m in Missions.Add(Data, type, amount))
            {
                UIManager.Instance.Toast(L.T("mis.done_fmt", m.Title));
                AudioFX.Instance.Play(AudioFX.Sfx.Upgrade);
            }
        }

        public bool HasMissionReward => Missions.HasClaimable(Data);

        /// <summary>Reclama el premio de una misión. El cofre y la semanal muestran lo ganado; after se llama al cerrar.</summary>
        public void ClaimMission(MissionState s, Action after)
        {
            var def = Missions.Get(s.id);
            var lines = Missions.Claim(Data, s);
            if (def == null || lines.Count == 0) { after?.Invoke(); return; }
            Data.statMissionsClaimed++;
            Save(); NotifyChanged();
            if (def.reward == MissionReward.Chest)
            {
                UIManager.Instance.ShowChestOpen(ChestTier.Common, after); // el cofre se abre con animación
            }
            else if (def.reward == MissionReward.WeeklyPaint)
            {
                AudioFX.Instance.Play(AudioFX.Sfx.Win);
                UIManager.Instance.IconDialog("icon_gift", L.T("mis.weekly_done_title"), string.Join("\n", lines), (p, close) =>
                    UIManager.DialogButton(p, L.T("chest.open_fmt", Chests.Name(ChestTier.Epic).ToUpper(L.Culture)), Palette.Purple,
                        () => UIManager.Instance.ShowChestOpen(ChestTier.Epic, after), close, true, "icon_chest"));
            }
            else
            {
                UIManager.Instance.Toast(string.Join("  ", lines));
                after?.Invoke();
            }
        }

        public bool HasAchievementReward => Achievements.HasClaimable(Data);

        /// <summary>Desbloquea los logros cumplidos. Con toast=true avisa en pantalla (durante la partida, compras, etc.).</summary>
        List<AchievementDefinition> CheckAchievements(bool toast)
        {
            if (Data == null || !TrackProgress) return new List<AchievementDefinition>();
            var list = Achievements.CheckUnlocks(Data);
            if (list.Count == 0) return list;
            Save();
            if (toast && UIManager.Instance != null)
            {
                UIManager.Instance.Toast(list.Count == 1 ? L.T("ach.unlocked_fmt", list[0].Name) : L.T("ach.unlocked_many_fmt", list.Count));
                AudioFX.Instance?.Play(AudioFX.Sfx.Win);
            }
            return list;
        }

        public void ClaimAchievement(AchievementDefinition a, Action after)
        {
            int coins = Achievements.Claim(Data, a);
            if (coins > 0)
            {
                Save(); NotifyChanged();
                AudioFX.Instance.Play(AudioFX.Sfx.Upgrade);
                UIManager.Instance.Toast(L.T("toast.coins_fmt", UIFactory.Fmt(coins)));
            }
            after?.Invoke();
        }

        public bool CanClaimDaily => DailyStreak.Available(Data) || (Data.vip && !Data.vipLivesClaimed);

        /// <summary>Regalo de la racha diaria (y vidas VIP). Los premios grandes se muestran en una ventana.</summary>
        public void ClaimDaily()
        {
            Data.RefreshDaily();
            var lines = new List<string>();
            var vipLines = new List<string>();
            bool big = false, chest = false;
            int day = DailyStreak.CurrentDay(Data);
            if (DailyStreak.Available(Data))
            {
                lines.AddRange(DailyStreak.Claim(Data, out var type));
                big = type != StreakRewardType.Coins;
                chest = type == StreakRewardType.Chest;
            }
            if (Data.vip && !Data.vipLivesClaimed)
            {
                Data.vipLivesClaimed = true;
                Data.lives = Mathf.Min(GameConfig.MaxLives, Data.lives + GameConfig.VipDailyLives);
                vipLines.Add(L.T("toast.vip_lives_fmt", GameConfig.VipDailyLives));
                lines.AddRange(vipLines);
            }
            if (lines.Count == 0) return;
            Save(); NotifyChanged();
            if (chest)
            {
                if (vipLines.Count > 0) UIManager.Instance.Toast(string.Join("  ", vipLines));
                UIManager.Instance.ShowChestOpen(ChestTier.Common); // día 4: cofre con animación
                return;
            }
            AudioFX.Instance.Play(big ? AudioFX.Sfx.Win : AudioFX.Sfx.Upgrade);
            if (big)
                UIManager.Instance.IconDialog("icon_flame", L.T("streak.day_title_fmt", day), string.Join("\n", lines), (p, close) =>
                    UIManager.DialogButton(p, L.T("mis.great"), Palette.Green, null, close));
            else
                UIManager.Instance.Toast(L.T("toast.daily_fmt", string.Join("  ", lines)));
        }

        /// <summary>Abre el mejor cofre que tenga guardado (botón de cofres del menú).</summary>
        public void OpenNextChest()
        {
            var t = Chests.Next(Data);
            if (t.HasValue) UIManager.Instance.ShowChestOpen(t.Value);
        }

        /// <summary>Se acabó el tiempo de la nave o del arma de prueba (nunca en mitad de un nivel).</summary>
        void CheckTempShip()
        {
            if (string.IsNullOrEmpty(Data.tempShipId) && string.IsNullOrEmpty(Data.tempEquipId)) return;
            var st = Gameplay != null ? Gameplay.State : GameplayController.RunState.Idle;
            if (st != GameplayController.RunState.Idle && st != GameplayController.RunState.Ended) return;

            bool hadShip = !string.IsNullOrEmpty(Data.tempShipId), hadEquip = !string.IsNullOrEmpty(Data.tempEquipId);
            string removedShip = DailyStreak.ExpireTempShip(Data);
            string removedEquip = Chests.ExpireTempEquip(Data);
            bool changed = (hadShip && string.IsNullOrEmpty(Data.tempShipId)) || (hadEquip && string.IsNullOrEmpty(Data.tempEquipId));
            if (!changed) return; // aún les queda tiempo
            Save(); NotifyChanged();
            if (removedShip != null) UIManager.Instance?.Toast(L.T("streak.ship_ended_fmt", ShipCatalog.Get(removedShip).Name));
            if (removedEquip != null) UIManager.Instance?.Toast(L.T("chest.weapon_ended_fmt", UpgradeCatalog.GetEquipment(removedEquip).Name));
        }

        public void ResetProgress()
        {
            SaveSystem.DeleteAll();
            Data = PlayerData.CreateNew();
            Data.language = (int)L.Current; // conserva el idioma
            Data.RefreshDaily();
            Save();
            NotifyChanged();
            RegistrationScreen.ClearPending();
            UIManager.Instance.ShowFreshScreen(typeof(RegistrationScreen)); // formulario vacío, casilla sin marcar
        }
    }
}
