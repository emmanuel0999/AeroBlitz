using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AeroBlitz
{
    public abstract class UIScreen : MonoBehaviour
    {
        bool _built;
        public RectTransform Root => (RectTransform)transform;

        public void EnsureBuilt()
        {
            if (_built) return;
            _built = true;
            Build();
        }

        protected abstract void Build();
        public virtual void OnShow() { }
        public virtual void Refresh() { }
    }

    /// <summary>Gestiona pantallas, diálogos, avisos y el anuncio simulado.</summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        public RectTransform App { get; private set; }
        RectTransform _canvasRt, _screens, _dialogs, _toasts;
        Image _damageFlash;
        readonly Dictionary<Type, UIScreen> _cache = new Dictionary<Type, UIScreen>();
        UIScreen _current;
        Vector2 _lastCanvasSize;
        Rect _lastSafe;

        void Awake()
        {
            Instance = this;

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                es.transform.SetParent(transform);
            }

            var canvasGo = new GameObject("Canvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            canvasGo.AddComponent<GraphicRaycaster>();
            _canvasRt = (RectTransform)canvasGo.transform;

            App = UIFactory.Rect("App", _canvasRt);
            _screens = UIFactory.Rect("Screens", App).Stretch();
            _damageFlash = UIFactory.Image(App, new Color(1, 0, 0, 0), false, "DamageFlash");
            _damageFlash.rectTransform.Stretch();
            _dialogs = UIFactory.Rect("Dialogs", App).Stretch();
            _toasts = UIFactory.Rect("Toasts", App).Stretch();

            GameManager.Instance.DataChanged += () => { if (_current != null && _current.gameObject.activeSelf) _current.Refresh(); };
        }

        void LateUpdate()
        {
            // Área jugable centrada (en PC/tablet horizontal) y respeto del notch (safe area)
            var size = _canvasRt.rect.size;
            if (size == _lastCanvasSize && Screen.safeArea == _lastSafe) return;
            _lastCanvasSize = size; _lastSafe = Screen.safeArea;
            float w = Mathf.Min(size.x, size.y * 0.62f);
            float scale = size.y / Mathf.Max(1, Screen.height);
            float top = (Screen.height - Screen.safeArea.yMax) * scale;
            float bottom = Screen.safeArea.yMin * scale;
            App.anchorMin = new Vector2(0.5f, 0);
            App.anchorMax = new Vector2(0.5f, 1);
            App.pivot = new Vector2(0.5f, 0.5f);
            App.offsetMin = new Vector2(-w / 2f, bottom);
            App.offsetMax = new Vector2(w / 2f, -top);
            if (_current != null) _current.Refresh();
        }

        // ---------------- PANTALLAS ----------------
        public T ShowScreen<T>() where T : UIScreen => (T)ShowScreen(typeof(T));

        public UIScreen ShowScreen(Type type)
        {
            CloseAllDialogs();
            if (!_cache.TryGetValue(type, out var screen))
            {
                var rt = UIFactory.Rect(type.Name, _screens).Stretch();
                screen = (UIScreen)rt.gameObject.AddComponent(type);
                _cache[type] = screen;
            }
            foreach (var s in _cache.Values) s.gameObject.SetActive(s == screen);
            _current = screen;
            screen.EnsureBuilt();
            screen.OnShow();
            screen.Refresh();
            return screen;
        }

        public T Get<T>() where T : UIScreen => _cache.TryGetValue(typeof(T), out var s) ? (T)s : null;

        /// <summary>Destruye todas las pantallas y las vuelve a construir en el nuevo idioma.</summary>
        public void RebuildForLanguage()
        {
            var type = _current != null ? _current.GetType() : typeof(MainMenuScreen);
            ShowFreshScreen(type);
        }

        /// <summary>Destruye todas las pantallas y muestra una recién construida (sin datos anteriores).</summary>
        public UIScreen ShowFreshScreen(Type type)
        {
            foreach (var s in _cache.Values) Destroy(s.gameObject);
            _cache.Clear();
            _current = null;
            return ShowScreen(type);
        }

        // ---------------- DIÁLOGOS ----------------
        public void CloseAllDialogs() => UIFactory.Clear(_dialogs);

        /// <summary>Crea un diálogo modal. El builder añade contenido al panel (layout vertical).</summary>
        public RectTransform Dialog(string title, string body, Action<RectTransform, Action> builder = null)
        {
            var dim = UIFactory.Image(_dialogs, new Color(0, 0, 0, 0.75f), false, "Dialog");
            dim.raycastTarget = true;
            dim.rectTransform.Stretch();
            var panel = UIFactory.Image(dim.transform, Palette.Panel, true, "Panel");
            panel.rectTransform.anchorMin = new Vector2(0.06f, 0.5f);
            panel.rectTransform.anchorMax = new Vector2(0.94f, 0.5f);
            panel.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            var vlg = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(40, 40, 40, 40);
            vlg.spacing = 20;
            vlg.childControlHeight = true; vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = true;
            var fit = panel.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var t = UIFactory.Label(panel.transform, title, 60, Palette.Gold, TextAnchor.MiddleCenter, true);
            t.Pref(90);
            if (!string.IsNullOrEmpty(body))
            {
                var b = UIFactory.Label(panel.transform, body, 38, Palette.Text);
                b.gameObject.AddComponent<LayoutElement>().minHeight = 60;
            }
            Action close = () => { if (dim != null) Destroy(dim.gameObject); };
            builder?.Invoke((RectTransform)panel.transform, close);
            return (RectTransform)panel.transform;
        }

        public static Button DialogButton(RectTransform panel, string label, Color color, Action onClick, Action close, bool closeOnClick = true)
        {
            var b = UIFactory.Button(panel, label, color, () => { if (closeOnClick) close(); onClick?.Invoke(); }, 42);
            b.Pref(120);
            return b;
        }

        public void Confirm(string title, string body, string yes, Action onYes)
        {
            Dialog(title, body, (p, close) =>
            {
                DialogButton(p, yes, Palette.Red, onYes, close);
                DialogButton(p, L.T("cancel"), Palette.Grey, null, close);
            });
        }

        /// <summary>Selector de idioma (Español / English / Français).</summary>
        public void ShowLanguagePicker()
        {
            Dialog(L.T("lang.choose"), null, (p, close) =>
            {
                foreach (var lang in L.All)
                {
                    var l = lang;
                    DialogButton(p, L.NativeName(l), l == L.Current ? Palette.Cyan : Palette.PanelLight,
                        () => GameManager.Instance.SetLanguage(l), close);
                }
                DialogButton(p, L.T("close"), Palette.Grey, null, close);
            });
        }

        // ---------------- LEGAL ----------------
        /// <summary>Muestra "terms" o "privacy" en el idioma actual (texto en Resources/Legal).</summary>
        public void ShowLegal(string doc)
        {
            var asset = Resources.Load<TextAsset>($"Legal/{doc}_{L.Code}") ?? Resources.Load<TextAsset>($"Legal/{doc}_es");
            string title = L.T(doc == "terms" ? "legal.terms" : "legal.privacy");
            Dialog(title, null, (p, close) =>
            {
                var area = UIFactory.Image(p, new Color(0, 0, 0, 0.3f), true, "LegalArea");
                area.Pref(1250);
                var content = UIFactory.ScrollList(area.transform, out var scroll, 18, 20);
                ((RectTransform)scroll.transform).Stretch();
                UIFactory.AddVerticalScrollbar(scroll);
                // Un Text por párrafo: un solo Text muy largo supera el límite de vértices de Unity
                foreach (var para in (asset != null ? asset.text : "—").Replace("\r", "").Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    bool heading = para.StartsWith("#");
                    var t = UIFactory.Label(content, para.TrimStart('#', ' '), heading ? 34 : 28,
                        heading ? Palette.Gold : Palette.Text, TextAnchor.UpperLeft, heading);
                    t.gameObject.AddComponent<LayoutElement>();
                }
                DialogButton(p, L.T("close"), Palette.Grey, null, close);
            });
        }

        /// <summary>Jugadores existentes: aceptar la nueva versión de los términos para seguir jugando.</summary>
        public void ShowTermsUpdate()
        {
            Dialog(L.T("legal.update_title"), L.T("legal.update_body"), (p, close) =>
            {
                DialogButton(p, L.T("legal.read_terms"), Palette.PanelLight, () => ShowLegal("terms"), close, false);
                DialogButton(p, L.T("legal.read_privacy"), Palette.PanelLight, () => ShowLegal("privacy"), close, false);
                DialogButton(p, L.T("legal.accept"), Palette.Green, GameManager.Instance.AcceptTerms, close);
            });
        }

        // ---------------- CUENTA ----------------
        /// <summary>La cuenta de Google ya tenía progreso y este teléfono también: el jugador elige.</summary>
        public void ShowProgressConflict(PlayerData cloud, PlayerData local, Action useCloud, Action useLocal)
        {
            string Summary(PlayerData d) => L.T("acc.summary_fmt", d.currentLevel, UIFactory.Fmt(d.totalPoints), UIFactory.Fmt(d.coins));
            string body = L.T("acc.conflict_body_fmt", Summary(cloud), Summary(local));
            Dialog(L.T("acc.conflict_title"), body, (p, close) =>
            {
                DialogButton(p, L.T("acc.use_cloud"), Palette.Green, useCloud, close);
                DialogButton(p, L.T("acc.use_local"), Palette.Orange, useLocal, close);
            });
        }

        /// <summary>Tras una compra real, si no hay cuenta: invita a respaldar el progreso (una vez por sesión).</summary>
        public void ShowProtectPurchasesPrompt()
        {
            if (_protectPromptShown || AccountService.Instance == null || AccountService.Instance.IsLinked) return;
            _protectPromptShown = true;
            Dialog(L.T("acc.protect_title"), L.T("acc.protect_body"), (p, close) =>
            {
                DialogButton(p, L.T("acc.google_btn"), Palette.Cyan,
                    () => AccountService.Instance.SignInWithGoogle((ok, msg) => Toast(msg)), close);
                DialogButton(p, L.T("acc.later"), Palette.Grey, null, close);
            });
        }
        bool _protectPromptShown;

        /// <summary>
        /// Panel de vidas: cuenta regresiva de la recarga de 5 h, barra de videos (cada video = 33,3%)
        /// y opciones de compra. outOfLives = se abrió porque el jugador se quedó sin vidas.
        /// </summary>
        public void ShowLivesPanel(bool outOfLives = false)
        {
            var gm = GameManager.Instance;
            var d = gm.Data;
            gm.UpdateLifeTimers();
            string title = outOfLives && d.lives <= 0 ? L.T("dlg.no_lives_title") : L.T("lives.title");
            string body = L.T("lives.count_fmt", d.lives, GameConfig.DefaultLives);
            Dialog(title, body, (p, close) =>
            {
                // Cuenta regresiva de la recarga completa
                var refill = UIFactory.Label(p, "", 36, Palette.Gold, TextAnchor.MiddleCenter, true);
                refill.Pref(50);
                refill.gameObject.AddComponent<LiveText>().source = () =>
                    gm.RefillRunning ? L.T("lives.refill_fmt", GameManager.FormatTime(gm.RefillRemaining)) : "";

                // Barra de progreso de videos
                var info = UIFactory.Label(p, d.lives >= GameConfig.DefaultLives ? L.T("lives.full")
                    : L.T("lives.video_info_fmt", GameConfig.VideosPerLife, d.videoLivesInWindow, GameConfig.MaxVideoLivesPerWindow), 32, Palette.TextDim);
                info.Pref(90);
                var bar = UIFactory.Bar(p, new Color(0, 0, 0, 0.5f), Palette.Green);
                bar.Pref(70);
                float pct = d.lifeVideoProgress / (float)GameConfig.VideosPerLife;
                bar.Set(pct, L.T("lives.bar_fmt", d.lifeVideoProgress, GameConfig.VideosPerLife, pct * 100f));

                var limit = UIFactory.Label(p, "", 32, Palette.Orange, TextAnchor.MiddleCenter, true);
                limit.Pref(44);
                limit.gameObject.AddComponent<LiveText>().source = () =>
                    gm.VideoLifeLimitReached ? L.T("lives.window_limit_fmt", GameManager.FormatTime(gm.VideoWindowRemaining)) : "";

                var watch = DialogButton(p, L.T("lives.watch_btn_fmt", d.lifeVideoProgress + 1, GameConfig.VideosPerLife), Palette.Green,
                    () => gm.WatchAdForLife(() => ShowLivesPanel(outOfLives)), close);
                watch.interactable = gm.CanWatchForLife;

                DialogButton(p, L.T("dlg.lives_buy_fmt", 5, IAPManager.Instance.GetPrice(ProductIds.Lives5)), Palette.Purple, () => gm.BuyProduct(ProductIds.Lives5), close);
                DialogButton(p, L.T("dlg.lives_buy_fmt", 15, IAPManager.Instance.GetPrice(ProductIds.Lives15)), Palette.Purple, () => gm.BuyProduct(ProductIds.Lives15), close);
                DialogButton(p, L.T("dlg.go_store"), Palette.Orange, () => ShowScreen<StoreScreen>().SelectTab(ProductCatalog.CatLives), close);
                DialogButton(p, L.T("close"), Palette.Grey, null, close);
            });
        }

        // ---------------- MISIONES ----------------
        /// <summary>Lista de misiones diarias y semanal con su progreso y el botón para reclamar el premio.</summary>
        public void ShowMissions()
        {
            var gm = GameManager.Instance;
            var d = gm.Data;
            d.RefreshDaily();
            Dialog(L.T("mis.title"), null, (p, close) =>
            {
                var daily = UIFactory.Label(p, "", 34, Palette.Cyan, TextAnchor.MiddleLeft, true);
                daily.Pref(50);
                daily.gameObject.AddComponent<LiveText>().source = () => L.T("mis.daily_fmt", FormatCountdown(Missions.DailyRemaining));
                foreach (var s in d.dailyMissions) MissionRow(p, s, close);

                var weekly = UIFactory.Label(p, "", 34, Palette.Purple, TextAnchor.MiddleLeft, true);
                weekly.Pref(50);
                weekly.gameObject.AddComponent<LiveText>().source = () => L.T("mis.weekly_fmt", FormatCountdown(Missions.WeeklyRemaining));
                MissionRow(p, d.weeklyMission, close);

                DialogButton(p, L.T("close"), Palette.Grey, null, close);
            });
        }

        void MissionRow(RectTransform parent, MissionState s, Action close)
        {
            var gm = GameManager.Instance;
            var def = Missions.Get(s.id);
            if (def == null) return;
            bool complete = Missions.IsComplete(s);

            var row = UIFactory.Image(parent, Palette.PanelLight, true, "Mission");
            row.Pref(250);
            var title = FitLabel(row.transform, def.Title, 34, Palette.Text, true);
            title.rectTransform.Place(0, 0.6f, 0.7f, 1, 26, 0, 0, 10);
            var reward = FitLabel(row.transform, L.T("mis.reward_fmt", Missions.RewardText(def, gm.Data)), 28, Palette.Gold, false);
            reward.rectTransform.Place(0, 0.35f, 0.7f, 0.6f, 26, 0, 0, 0);
            var bar = UIFactory.Bar(row.transform, new Color(0, 0, 0, 0.5f), complete ? Palette.Green : Palette.Cyan);
            ((RectTransform)bar.transform).Place(0, 0.08f, 0.7f, 0.32f, 26, 0, 0, 0);
            bar.Set(s.progress / (float)def.target, L.T("mis.progress_fmt", s.progress, def.target));

            string label = s.claimed ? L.T("mis.claimed") : complete ? L.T("mis.claim") : L.T("mis.in_progress");
            var btn = UIFactory.Button(row.transform, label, complete && !s.claimed ? Palette.Green : Palette.Grey, () =>
            {
                close();
                gm.ClaimMission(s, ShowMissions);
            }, 34);
            ((RectTransform)btn.transform).Place(0.72f, 0.22f, 1, 0.78f, 0, 0, 22, 0);
            btn.interactable = complete && !s.claimed;
        }

        /// <summary>Texto que se encoge si no cabe (textos largos en francés o el premio semanal).</summary>
        static Text FitLabel(Transform parent, string text, int size, Color color, bool bold)
        {
            var t = UIFactory.Label(parent, text, size, color, TextAnchor.MiddleLeft, bold);
            t.resizeTextForBestFit = true;
            t.resizeTextMaxSize = size;
            t.resizeTextMinSize = 18;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            return t;
        }

        /// <summary>"5:32:10", o "3 d 4 h" si falta más de un día.</summary>
        static string FormatCountdown(TimeSpan t) =>
            t.TotalHours >= 24 ? L.T("time.days_fmt", (int)t.TotalDays, t.Hours) : GameManager.FormatTime(t);

        public void ShowLevelComplete(int level, int points, int coins, int destroyed, bool campaignDone, int pickupCoins = 0, string trialProductId = null, List<string> missionsDone = null)
        {
            var gm = GameManager.Instance;
            string body = L.T("dlg.complete_body_fmt", UIFactory.Fmt(points), UIFactory.Fmt(coins), destroyed);
            if (pickupCoins > 0) body += "\n" + L.T("dlg.pickup_coins_fmt", UIFactory.Fmt(pickupCoins));
            if (missionsDone != null)
                foreach (var m in missionsDone) body += "\n<color=#FFCC33>" + L.T("mis.level_done_fmt", m) + "</color>";
            if (campaignDone) body += "\n\n" + L.T("dlg.campaign_done");
            Dialog(L.T("dlg.complete_title_fmt", level), body, (p, close) =>
            {
                // Si probó un arma de pago en este nivel, se le ofrece comprarla
                if (trialProductId != null && !ProductCatalog.IsOwned(gm.Data, trialProductId))
                {
                    var def = ProductCatalog.Get(trialProductId);
                    if (def != null)
                        DialogButton(p, L.T("dlg.trial_buy_fmt", def.Title, IAPManager.Instance.GetPrice(trialProductId)), Palette.Purple,
                            () => gm.BuyProduct(trialProductId), close, false);
                }
                Button dbl = null;
                dbl = DialogButton(p, L.T("dlg.double_coins"), Palette.Green, () =>
                {
                    dbl.interactable = false;
                    gm.DoubleLastCoins(null);
                }, close, false);
                DialogButton(p, L.T("dlg.share"), Palette.Purple, () =>
                    PlatformServices.ShareText(L.T("share.level_fmt", level, UIFactory.Fmt(gm.Data.totalPoints), PlatformServices.StoreUrl)), close, false);
                if (!campaignDone)
                    DialogButton(p, L.T("dlg.next_fmt", gm.Data.currentLevel), Palette.Cyan, () => gm.AfterRunInterstitial(level, gm.PlayCurrentLevel), close);
                else
                    DialogButton(p, L.T("dlg.replay_last"), Palette.Cyan, () => gm.AfterRunInterstitial(level, gm.PlayCurrentLevel), close);
                DialogButton(p, L.T("dlg.main_menu"), Palette.Grey, () => gm.AfterRunInterstitial(level, gm.ReturnToMenu), close);
            });
        }

        public void ShowLevelFailed(int level)
        {
            var gm = GameManager.Instance;
            int lives = gm.Data.lives;
            string body = lives > 0 ? L.T("dlg.failed_body_fmt", lives, level) : L.T("dlg.failed_last_fmt", GameConfig.LifeRefillHours);
            Dialog(L.T("dlg.failed_title"), body, (p, close) =>
            {
                if (lives > 0)
                    DialogButton(p, L.T("dlg.retry"), Palette.Cyan, () => gm.AfterRunInterstitial(level, gm.PlayCurrentLevel), close);
                else
                    DialogButton(p, L.T("dlg.get_lives"), Palette.Green, () => { gm.ReturnToMenu(); ShowLivesPanel(true); }, close);
                DialogButton(p, L.T("dlg.main_menu"), Palette.Grey, () => gm.AfterRunInterstitial(level, gm.ReturnToMenu), close);
            });
        }

        public void ShowRevive(bool canRevive, int level)
        {
            var gp = GameManager.Instance.Gameplay;
            if (!canRevive || !AdsManager.Instance.IsRewardedAvailable) { gp.DeclineRevive(); return; }
            Dialog(L.T("dlg.revive_title"), L.T("dlg.revive_body"), (p, close) =>
            {
                DialogButton(p, L.T("dlg.revive_btn"), Palette.Green, () =>
                    AdsManager.Instance.ShowRewarded(ok => { if (ok) gp.Revive(); else gp.DeclineRevive(); }), close);
                DialogButton(p, L.T("dlg.no_thanks"), Palette.Grey, gp.DeclineRevive, close);
            });
        }

        public void ShowPause()
        {
            var gm = GameManager.Instance;
            Dialog(L.T("dlg.pause"), L.T("dlg.level_fmt", gm.Gameplay.Level), (p, close) =>
            {
                DialogButton(p, L.T("dlg.continue"), Palette.Cyan, gm.Gameplay.Resume, close);
                Button snd = null;
                snd = DialogButton(p, L.T(gm.Data.soundOn ? "set.sound_on" : "set.sound_off"), Palette.Grey, () =>
                {
                    gm.Data.soundOn = !gm.Data.soundOn; gm.Save();
                    snd.SetLabel(L.T(gm.Data.soundOn ? "set.sound_on" : "set.sound_off"));
                }, close, false);
                Button mus = null;
                mus = DialogButton(p, L.T(gm.Data.musicOn ? "set.music_on" : "set.music_off"), Palette.Grey, () =>
                {
                    gm.Data.musicOn = !gm.Data.musicOn; gm.Save();
                    mus.SetLabel(L.T(gm.Data.musicOn ? "set.music_on" : "set.music_off"));
                }, close, false);
                DialogButton(p, L.T("dlg.abandon"), Palette.Red, () =>
                    Confirm(L.T("dlg.abandon_title"), L.T("dlg.abandon_body"), L.T("dlg.abandon_yes"), gm.Gameplay.Abandon), close);
            });
        }

        // ---------------- AVISOS / EFECTOS ----------------
        public void Toast(string msg)
        {
            var bg = UIFactory.Image(_toasts, new Color(0.05f, 0.07f, 0.15f, 0.95f), true, "Toast");
            bg.rectTransform.anchorMin = new Vector2(0.08f, 0.8f);
            bg.rectTransform.anchorMax = new Vector2(0.92f, 0.8f);
            bg.rectTransform.sizeDelta = new Vector2(0, 110);
            var t = UIFactory.Label(bg.transform, msg, 38, Color.white, TextAnchor.MiddleCenter, true);
            t.rectTransform.Stretch(12);
            StartCoroutine(FadeToast(bg));
        }

        IEnumerator FadeToast(Image bg)
        {
            float t = 0;
            var cg = bg.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            while (t < 2.2f)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = t < 1.7f ? 1f : 1f - (t - 1.7f) / 0.5f;
                bg.rectTransform.anchoredPosition = new Vector2(0, t * 20f);
                yield return null;
            }
            Destroy(bg.gameObject);
        }

        public void FlashDamage() { StopCoroutine(nameof(Flash)); StartCoroutine(nameof(Flash)); }

        IEnumerator Flash()
        {
            for (float t = 0.35f; t > 0; t -= Time.deltaTime)
            {
                _damageFlash.color = new Color(1, 0, 0, t);
                yield return null;
            }
            _damageFlash.color = new Color(1, 0, 0, 0);
        }

        /// <summary>Anuncio simulado para Editor/pruebas (no se usa en producción con IDs reales).</summary>
        public void ShowSimulatedAd(bool rewarded, Action onDone)
        {
            var bg = UIFactory.Image(_toasts, Color.black, false, "SimAd");
            bg.raycastTarget = true;
            bg.rectTransform.Stretch();
            var t = UIFactory.Label(bg.transform, "", 50, Color.white, TextAnchor.MiddleCenter, true);
            t.rectTransform.Stretch(60);
            StartCoroutine(SimAd(bg, t, rewarded, onDone));
        }

        IEnumerator SimAd(Image bg, Text t, bool rewarded, Action onDone)
        {
            for (float s = rewarded ? 3f : 2f; s > 0; s -= Time.unscaledDeltaTime)
            {
                t.text = L.T("ad.test_fmt", L.T(rewarded ? "ad.rewarded" : "ad.interstitial"), Mathf.CeilToInt(s));
                yield return null;
            }
            Destroy(bg.gameObject);
            onDone?.Invoke();
        }
    }
}
