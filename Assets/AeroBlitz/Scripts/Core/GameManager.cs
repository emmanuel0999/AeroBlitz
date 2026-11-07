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
        public void NotifyChanged() => DataChanged?.Invoke();

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
        public void PlayCurrentLevel()
        {
            UpdateLifeTimers();
            if (Data.lives <= 0) { UIManager.Instance.ShowLivesPanel(true); return; }
            AdsManager.Instance.HideBanner();
            UIManager.Instance.ShowScreen<HUDScreen>();
            Gameplay.Begin(Data.currentLevel, ShipStats.Compute(Data.SelectedShipState), Data.SelectedShipState.shipId);
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

            Data.totalPoints += points;
            Data.coins += coins;
            _lastEarnedCoins = coins;

            // Misiones que dependen de superar el nivel
            var missionsDone = new List<string>();
            if (TrackMissions)
            {
                var done = new List<MissionDefinition>();
                done.AddRange(Missions.Add(Data, MissionType.CollectCoins, r.coinPickups));
                if (!r.tookDamage) done.AddRange(Missions.Add(Data, MissionType.NoDamageLevels, 1));
                done.AddRange(Missions.Add(Data, MissionType.CompleteLevels, 1));
                foreach (var m in done) missionsDone.Add(m.Title);
            }

            bool campaignDone = r.level >= GameConfig.TotalLevels;
            if (!campaignDone && r.level == Data.currentLevel)
            {
                Data.currentLevel = r.level + 1;
                Data.highestUnlocked = Mathf.Max(Data.highestUnlocked, Data.currentLevel);
            }
            Save();
#if UNITY_EDITOR
            if (!TrailerCheats.Active)
#endif
            LeaderboardService.Instance.SubmitScore(Data.totalPoints, Data.countryCode);
            NotifyChanged();
            AudioFX.Instance.Play(AudioFX.Sfx.Win);

            UIManager.Instance.ShowLevelComplete(r.level, points, coins, r.destroyed, campaignDone, r.pickupCoins, r.trialProductId, missionsDone);
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

        /// <summary>Revisa la recarga de 5 h y el período de vidas por video (también funciona con el juego cerrado).</summary>
        public void UpdateLifeTimers()
        {
            if (Data == null) return;
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

        void LoseLife()
        {
            Data.lives = Mathf.Max(0, Data.lives - 1);
            // Sin vidas: arranca la espera (LifeRefillHours) para recargar las 3 vidas
            if (Data.lives == 0 && !RefillRunning)
                Data.livesRefillAtTicks = NowTicks + Hours(GameConfig.LifeRefillHours);
        }

        /// <summary>Cada video llena 33,3% de la barra; con 3 videos se gana 1 vida (máx. 2 por período de 5 h).</summary>
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

        // ---------------- MISIONES ----------------
        /// <summary>Grabar el tráiler (solo en el Editor) no cuenta para las misiones.</summary>
        static bool TrackMissions
        {
            get
            {
#if UNITY_EDITOR
                if (TrailerCheats.Active) return false;
#endif
                return true;
            }
        }

        /// <summary>Progreso que se suma durante la partida (drones y asteroides). Avisa si se completa una misión.</summary>
        public void MissionEvent(MissionType type, int amount)
        {
            if (!TrackMissions) return;
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
            Save(); NotifyChanged();
            AudioFX.Instance.Play(AudioFX.Sfx.Win);
            if (def.reward == MissionReward.Chest || def.reward == MissionReward.WeeklyPaint)
            {
                string title = L.T(def.reward == MissionReward.Chest ? "chest.title" : "mis.weekly_done_title");
                UIManager.Instance.Dialog(title, string.Join("\n", lines), (p, close) =>
                    UIManager.DialogButton(p, L.T("mis.great"), Palette.Green, after, close));
            }
            else
            {
                UIManager.Instance.Toast(string.Join("  ", lines));
                after?.Invoke();
            }
        }

        public bool CanClaimDaily => !Data.dailyRewardClaimed || (Data.vip && !Data.vipLivesClaimed);

        public void ClaimDaily()
        {
            Data.RefreshDaily();
            string msg = "";
            if (!Data.dailyRewardClaimed)
            {
                Data.dailyRewardClaimed = true;
                Data.coins += GameConfig.DailyRewardCoins;
                msg = L.T("toast.coins_fmt", GameConfig.DailyRewardCoins);
            }
            if (Data.vip && !Data.vipLivesClaimed)
            {
                Data.vipLivesClaimed = true;
                Data.lives = Mathf.Min(GameConfig.MaxLives, Data.lives + GameConfig.VipDailyLives);
                msg += "  " + L.T("toast.vip_lives_fmt", GameConfig.VipDailyLives);
            }
            Save(); NotifyChanged();
            if (msg.Length > 0) UIManager.Instance.Toast(L.T("toast.daily_fmt", msg.Trim()));
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
