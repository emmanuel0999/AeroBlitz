using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if AEROBLITZ_ADS
using Unity.Services.LevelPlay;
#endif

namespace AeroBlitz
{
    /// <summary>
    /// Anuncios con Unity LevelPlay (mediación: Unity Ads + otras redes compiten por cada anuncio).
    /// Recompensados (vidas, mejoras, monedas x2, revivir), intersticiales entre niveles y banner en el menú.
    /// Si no hay IDs configurados o la plataforma no es Android/iOS, en el Editor / builds de desarrollo
    /// se simula el anuncio para poder probar el flujo.
    /// </summary>
    public class AdsManager : MonoBehaviour
    {
        public static AdsManager Instance { get; private set; }

        bool _bannerWanted;
        float _lastInterstitialTime = -999f;
        Action<bool> _rewardCallback;
        Action _interstitialCallback;

        // Los eventos de LevelPlay pueden llegar desde otro hilo: se encolan y se ejecutan en Update.
        readonly Queue<Action> _mainThread = new Queue<Action>();

        static bool SimulationAllowed => Application.isEditor || Debug.isDebugBuild;
        static bool IsIos => Application.platform == RuntimePlatform.IPhonePlayer;
        static bool PlatformSupported => Application.platform == RuntimePlatform.Android || IsIos;

        static string AppKey => IsIos ? GameConfig.IosAppKey : GameConfig.AndroidAppKey;
        static string RewardedId => IsIos ? GameConfig.RewardedIos : GameConfig.RewardedAndroid;
        static string InterstitialId => IsIos ? GameConfig.InterstitialIos : GameConfig.InterstitialAndroid;
        static string BannerId => IsIos ? GameConfig.BannerIos : GameConfig.BannerAndroid;

        static bool IdsConfigured => !AppKey.StartsWith("REEMPLAZAR");

#if AEROBLITZ_ADS
        bool _initialized;
        bool _rewardEarned;
        LevelPlayRewardedAd _rewarded;
        LevelPlayInterstitialAd _interstitial;
        LevelPlayBannerAd _banner;
        float _bannerPixels, _pxPerDp = 2.75f;
        bool _bannerLoaded, _bannerLoading;
        float _bannerRetryDelay;
#endif

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            lock (_mainThread)
                while (_mainThread.Count > 0) _mainThread.Dequeue()?.Invoke();
        }

        void OnMain(Action a)
        {
            lock (_mainThread) _mainThread.Enqueue(a);
        }

        public void Initialize()
        {
#if AEROBLITZ_ADS
            if (Debug.isDebugBuild && !GameConfig.RealAdsInDevelopmentBuilds)
            {
                Debug.Log("[Ads] Build de desarrollo: anuncios simulados (no se cargan anuncios reales).");
                return;
            }
            if (!PlatformSupported || !IdsConfigured)
            {
                Debug.Log("[Ads] LevelPlay no configurado o plataforma sin anuncios: se usarán anuncios simulados (solo Editor/Dev).");
                return;
            }
            LevelPlay.OnInitSuccess += cfg => OnMain(OnInitSuccess);
            LevelPlay.OnInitFailed += err => OnMain(() => Debug.LogWarning("[Ads] Falló inicialización de LevelPlay: " + err));
            LevelPlay.Init(AppKey);
#else
            Debug.Log("[Ads] Paquete com.unity.services.levelplay no instalado: anuncios simulados.");
#endif
        }

        public bool AdsRemoved
        {
            get
            {
                var d = GameManager.Instance != null ? GameManager.Instance.Data : null;
                return d != null && (d.removeAds || d.vip);
            }
        }

        public bool IsRewardedAvailable
        {
            get
            {
#if AEROBLITZ_ADS
                if (_initialized && _rewarded != null && _rewarded.IsAdReady()) return true;
#endif
                return SimulationAllowed;
            }
        }

        // ---------------- RECOMPENSADOS ----------------
        public void ShowRewarded(Action<bool> onDone)
        {
#if AEROBLITZ_ADS
            if (_initialized && _rewarded != null && _rewarded.IsAdReady())
            {
                _rewardCallback = onDone;
                _rewardEarned = false;
                _rewarded.ShowAd();
                return;
            }
#endif
            if (SimulationAllowed)
            {
                UIManager.Instance.ShowSimulatedAd(true, () => onDone?.Invoke(true));
                return;
            }
            UIManager.Instance.Toast(L.T("ad.unavailable"));
            LoadRewarded();
            onDone?.Invoke(false);
        }

        // ---------------- INTERSTICIALES ----------------
        /// <summary>Se llama al terminar un nivel. Muestra intersticial respetando frecuencia y compras.</summary>
        public void MaybeShowInterstitial(int level, Action onClosed)
        {
            var d = GameManager.Instance.Data;
            d.levelsPlayedSinceInterstitial++;
            bool shouldShow = !AdsRemoved
                              && level >= GameConfig.NoInterstitialBeforeLevel
                              && d.levelsPlayedSinceInterstitial >= GameConfig.InterstitialEveryNRuns
                              && Time.realtimeSinceStartup - _lastInterstitialTime > GameConfig.InterstitialMinSeconds;
            if (!shouldShow) { onClosed?.Invoke(); return; }

            d.levelsPlayedSinceInterstitial = 0;
            _lastInterstitialTime = Time.realtimeSinceStartup;
#if AEROBLITZ_ADS
            if (_initialized && _interstitial != null && _interstitial.IsAdReady())
            {
                _interstitialCallback = onClosed;
                _interstitial.ShowAd();
                return;
            }
#endif
            if (SimulationAllowed) { UIManager.Instance.ShowSimulatedAd(false, onClosed); return; }
            onClosed?.Invoke();
        }

        // ---------------- BANNER ----------------
        /// <summary>
        /// Alto en píxeles de pantalla que ocupa el banner desde el borde inferior (0 si no hay banner).
        /// Las pantallas lo usan para dejar ese espacio libre y que el anuncio no tape botones.
        /// </summary>
        public float BannerReservePixels { get; private set; }

        public void ShowBanner()
        {
            _bannerWanted = true;
            if (AdsRemoved) { HideBanner(); return; }
#if AEROBLITZ_ADS
            if (!_initialized) return;
            if (_banner == null)
            {
                var size = LevelPlayAdSize.CreateAdaptiveAdSize() ?? LevelPlayAdSize.BANNER;
                _pxPerDp = PixelsPerDp();
                // El tamaño viene en dp; se pasa a píxeles de pantalla con la densidad real del dispositivo.
                _bannerPixels = (size.Height > 0 ? size.Height : 60) * _pxPerDp;
                var config = new LevelPlayBannerAd.Config.Builder()
                    .SetSize(size)
                    .SetPosition(LevelPlayBannerPosition.BottomCenter)
                    .SetDisplayOnLoad(true)
                    // Pegado al borde inferior real de la pantalla: así su alto es predecible.
                    .SetRespectSafeArea(false)
                    .Build();
                _banner = new LevelPlayBannerAd(BannerId, config);
                _banner.OnAdLoaded += info => OnMain(() => OnBannerLoaded(info));
                _banner.OnAdLoadFailed += err => OnMain(() => OnBannerLoadFailed(err));
                LoadBanner();
            }
            else if (_bannerLoaded) _banner.ShowAd();
            else LoadBanner(); // la carga anterior falló o sigue pendiente: se vuelve a intentar al entrar al menú
            // Solo se reserva espacio cuando hay un anuncio cargado; sin anuncio el menú usa toda la pantalla.
            BannerReservePixels = _bannerLoaded ? _bannerPixels : 0;
#endif
        }

#if AEROBLITZ_ADS
        void LoadBanner()
        {
            if (_banner == null || _bannerLoading || _bannerLoaded) return;
            _bannerLoading = true;
            _banner.LoadAd();
        }

        /// <summary>
        /// Si no hay anuncio disponible (algo frecuente con pocos jugadores), se reintenta cada vez más
        /// espaciado: 20 s, 40 s, 80 s y luego cada 2 minutos, mientras el jugador siga en el menú.
        /// </summary>
        void OnBannerLoadFailed(LevelPlayAdError err)
        {
            _bannerLoading = false;
            Debug.LogWarning("[Ads] Banner error: " + err);
            if (_bannerLoaded) return; // ya había un anuncio: LevelPlay sigue renovándolo solo
            _bannerRetryDelay = Mathf.Clamp(_bannerRetryDelay <= 0f ? 20f : _bannerRetryDelay * 2f, 20f, 120f);
            CancelInvoke(nameof(RetryBanner));
            Invoke(nameof(RetryBanner), _bannerRetryDelay);
        }

        void RetryBanner()
        {
            if (!_bannerWanted || AdsRemoved) return; // fuera del menú: se reintentará al volver
            LoadBanner();
        }

        void OnBannerLoaded(LevelPlayAdInfo info)
        {
            _bannerLoaded = true;
            _bannerLoading = false;
            _bannerRetryDelay = 0f;
            // Si el anuncio llega cuando el jugador ya salió del menú, no debe quedar encima del juego.
            if (!_bannerWanted || AdsRemoved) _banner?.HideAd();
            // Si la red sirve un anuncio más alto que el previsto, manda el tamaño real.
            var real = info != null ? info.AdSize : null;
            if (real != null && real.Height > 0)
                _bannerPixels = Mathf.Max(_bannerPixels, real.Height * _pxPerDp);
            if (_bannerWanted && !AdsRemoved) BannerReservePixels = _bannerPixels;
            Debug.Log($"[Ads] Banner cargado: reserva {Mathf.RoundToInt(_bannerPixels)} px (densidad {_pxPerDp:0.##}, pantalla {Screen.width}x{Screen.height}).");
        }

        /// <summary>Píxeles por dp según la densidad que usa Android para dibujar el anuncio.</summary>
        static float PixelsPerDp()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var res = activity.Call<AndroidJavaObject>("getResources"))
                using (var metrics = res.Call<AndroidJavaObject>("getDisplayMetrics"))
                {
                    float density = metrics.Get<float>("density");
                    if (density > 0) return density;
                }
            }
            catch (Exception e) { Debug.LogWarning("[Ads] No se pudo leer la densidad de pantalla: " + e.Message); }
#endif
            return Screen.dpi > 0 ? Screen.dpi / 160f : 2.75f;
        }
#endif

        public void HideBanner()
        {
            _bannerWanted = false;
            BannerReservePixels = 0;
#if AEROBLITZ_ADS
            _banner?.HideAd();
#endif
        }

        void LoadRewarded()
        {
#if AEROBLITZ_ADS
            if (_initialized) _rewarded?.LoadAd();
#endif
        }

        void LoadInterstitial()
        {
#if AEROBLITZ_ADS
            if (_initialized && !AdsRemoved) _interstitial?.LoadAd();
#endif
        }

#if AEROBLITZ_ADS
        void OnInitSuccess()
        {
            _initialized = true;
            Debug.Log("[Ads] LevelPlay inicializado.");

            _rewarded = new LevelPlayRewardedAd(RewardedId);
            _rewarded.OnAdRewarded += (info, reward) => OnMain(() => _rewardEarned = true);
            _rewarded.OnAdClosed += info => OnMain(() => StartCoroutine(FinishRewardedSoon()));
            _rewarded.OnAdDisplayFailed += (info, err) => OnMain(() => { _rewardEarned = false; FinishRewarded(); });
            _rewarded.OnAdLoadFailed += err => OnMain(() => Invoke(nameof(LoadRewarded), 15f));
            _rewarded.LoadAd();

            _interstitial = new LevelPlayInterstitialAd(InterstitialId);
            _interstitial.OnAdClosed += info => OnMain(FinishInterstitial);
            _interstitial.OnAdDisplayFailed += (info, err) => OnMain(FinishInterstitial);
            _interstitial.OnAdLoadFailed += err => OnMain(() => Invoke(nameof(LoadInterstitial), 15f));
            LoadInterstitial();

            if (_bannerWanted) ShowBanner();
        }

        /// <summary>Algunas redes envían la recompensa justo después de cerrar el anuncio: se espera un instante.</summary>
        IEnumerator FinishRewardedSoon()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            FinishRewarded();
        }

        void FinishRewarded()
        {
            var cb = _rewardCallback; _rewardCallback = null;
            cb?.Invoke(_rewardEarned);
            _rewardEarned = false;
            LoadRewarded();
        }

        void FinishInterstitial()
        {
            var cb = _interstitialCallback; _interstitialCallback = null;
            cb?.Invoke();
            LoadInterstitial();
        }

        void OnDestroy()
        {
            _rewarded?.DestroyAd();
            _interstitial?.DestroyAd();
            _banner?.DestroyAd();
        }
#endif
    }
}
