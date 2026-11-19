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
        /// <summary>Diálogo con un icono junto al título.</summary>
        public RectTransform IconDialog(string icon, string title, string body, Action<RectTransform, Action> builder = null) => Dialog(title, body, builder, icon);

        public RectTransform Dialog(string title, string body, Action<RectTransform, Action> builder = null, string icon = null)
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

            if (string.IsNullOrEmpty(icon))
            {
                var t = UIFactory.Label(panel.transform, title, 60, Palette.Gold, TextAnchor.MiddleCenter, true);
                t.Pref(90);
            }
            else
            {
                // Icono + título centrados como un grupo
                var row = UIFactory.Rect("TitleRow", panel.transform);
                row.Pref(90);
                var t = UIFactory.Label(row, title, 60, Palette.Gold, TextAnchor.MiddleCenter, true);
                float tw = Mathf.Min(t.preferredWidth, 700f);
                t.rectTransform.Stretch();
                t.rectTransform.offsetMin = new Vector2(46, 0); t.rectTransform.offsetMax = new Vector2(46, 0);
                var ic = UIKit.Icon(row, icon);
                ic.rectTransform.anchorMin = ic.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                ic.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                ic.rectTransform.sizeDelta = new Vector2(76, 76);
                ic.rectTransform.anchoredPosition = new Vector2(-tw / 2f - 8f, 0);
            }
            if (!string.IsNullOrEmpty(body))
            {
                var b = UIFactory.Label(panel.transform, body, 38, Palette.Text);
                b.gameObject.AddComponent<LayoutElement>().minHeight = 60;
            }
            Action close = () => { if (dim != null) Destroy(dim.gameObject); };
            builder?.Invoke((RectTransform)panel.transform, close);
            return (RectTransform)panel.transform;
        }

        public static Button DialogButton(RectTransform panel, string label, Color color, Action onClick, Action close, bool closeOnClick = true, string icon = null)
        {
            var b = UIFactory.Button(panel, label, color, () => { if (closeOnClick) close(); onClick?.Invoke(); }, 42);
            b.Pref(120);
            if (icon == null && (label == L.T("close") || label == L.T("cancel"))) icon = "icon_close";
            if (icon != null) b.SetIcon(icon);
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
            IconDialog("icon_globe", L.T("lang.choose"), null, (p, close) =>
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
            IconDialog(doc == "terms" ? "icon_doc" : "icon_shield", title, null, (p, close) =>
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
            IconDialog("icon_doc", L.T("legal.update_title"), L.T("legal.update_body"), (p, close) =>
            {
                DialogButton(p, L.T("legal.read_terms"), Palette.PanelLight, () => ShowLegal("terms"), close, false, "icon_doc");
                DialogButton(p, L.T("legal.read_privacy"), Palette.PanelLight, () => ShowLegal("privacy"), close, false, "icon_shield");
                DialogButton(p, L.T("legal.accept"), Palette.Green, GameManager.Instance.AcceptTerms, close, true, "icon_check");
            });
        }

        // ---------------- CUENTA ----------------
        /// <summary>La cuenta de Google ya tenía progreso y este teléfono también: el jugador elige.</summary>
        public void ShowProgressConflict(PlayerData cloud, PlayerData local, Action useCloud, Action useLocal)
        {
            string Summary(PlayerData d) => L.T("acc.summary_fmt", d.currentLevel, UIFactory.Fmt(d.totalPoints), UIFactory.Fmt(d.coins));
            string body = L.T("acc.conflict_body_fmt", Summary(cloud), Summary(local));
            IconDialog("icon_cloud", L.T("acc.conflict_title"), body, (p, close) =>
            {
                DialogButton(p, L.T("acc.use_cloud"), Palette.Green, useCloud, close, true, "icon_cloud");
                DialogButton(p, L.T("acc.use_local"), Palette.Orange, useLocal, close, true, "icon_user");
            });
        }

        /// <summary>Tras una compra real, si no hay cuenta: invita a respaldar el progreso (una vez por sesión).</summary>
        public void ShowProtectPurchasesPrompt()
        {
            if (_protectPromptShown || AccountService.Instance == null || AccountService.Instance.IsLinked) return;
            _protectPromptShown = true;
            IconDialog("icon_shield", L.T("acc.protect_title"), L.T("acc.protect_body"), (p, close) =>
            {
                DialogButton(p, L.T("acc.google_btn"), Palette.Cyan,
                    () => AccountService.Instance.SignInWithGoogle((ok, msg) => Toast(msg)), close, true, "icon_cloud");
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
            IconDialog("icon_heart", title, body, (p, close) =>
            {
                // Cuenta regresiva de la recarga completa
                var refill = UIFactory.Label(p, "", 36, Palette.Gold, TextAnchor.MiddleCenter, true);
                refill.Pref(50);
                refill.gameObject.AddComponent<LiveText>().source = () =>
                    gm.RefillRunning ? L.T("lives.refill_fmt", GameManager.FormatTime(gm.RefillRemaining)) : "";

                // Vidas por video (con más de un video por vida, barra de progreso)
                bool oneVideo = GameConfig.VideosPerLife <= 1;
                var info = UIFactory.Label(p, d.lives >= GameConfig.DefaultLives ? L.T("lives.full")
                    : oneVideo ? L.T("lives.video_info_one_fmt", d.videoLivesInWindow, GameConfig.MaxVideoLivesPerWindow)
                    : L.T("lives.video_info_fmt", GameConfig.VideosPerLife, d.videoLivesInWindow, GameConfig.MaxVideoLivesPerWindow), 32, Palette.TextDim);
                info.Pref(90);
                if (!oneVideo)
                {
                    var bar = UIFactory.Bar(p, new Color(0, 0, 0, 0.5f), Palette.Green);
                    bar.Pref(70);
                    float pct = d.lifeVideoProgress / (float)GameConfig.VideosPerLife;
                    bar.Set(pct, L.T("lives.bar_fmt", d.lifeVideoProgress, GameConfig.VideosPerLife, pct * 100f));
                }

                var limit = UIFactory.Label(p, "", 32, Palette.Orange, TextAnchor.MiddleCenter, true);
                limit.Pref(44);
                limit.gameObject.AddComponent<LiveText>().source = () =>
                    gm.VideoLifeLimitReached ? L.T("lives.window_limit_fmt", GameManager.FormatTime(gm.VideoWindowRemaining)) : "";

                var watch = DialogButton(p, oneVideo ? L.T("lives.watch_btn_one") : L.T("lives.watch_btn_fmt", d.lifeVideoProgress + 1, GameConfig.VideosPerLife), Palette.Green,
                    () => gm.WatchAdForLife(() => ShowLivesPanel(outOfLives)), close, true, "icon_video");
                watch.interactable = gm.CanWatchForLife;

                DialogButton(p, L.T("dlg.lives_buy_fmt", 5, IAPManager.Instance.GetPrice(ProductIds.Lives5)), Palette.Purple, () => gm.BuyProduct(ProductIds.Lives5), close, true, "icon_heart");
                DialogButton(p, L.T("dlg.lives_buy_fmt", 15, IAPManager.Instance.GetPrice(ProductIds.Lives15)), Palette.Purple, () => gm.BuyProduct(ProductIds.Lives15), close, true, "icon_heart");
                DialogButton(p, L.T("dlg.go_store"), Palette.Orange, () => ShowScreen<StoreScreen>().SelectTab(ProductCatalog.CatLives), close, true, "icon_cart");
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
            IconDialog("icon_missions", L.T("mis.title"), null, (p, close) =>
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
            RowIcon(row.transform, MissionIcon(def.type), Color.white);
            var title = FitLabel(row.transform, def.Title, 34, Palette.Text, true);
            title.rectTransform.Place(0, 0.6f, 0.7f, 1, RowTextLeft, 0, 0, 10);
            var reward = FitLabel(row.transform, L.T("mis.reward_fmt", Missions.RewardText(def, gm.Data)), 28, Palette.Gold, false);
            reward.rectTransform.Place(0, 0.35f, 0.7f, 0.6f, RowTextLeft, 0, 0, 0);
            var bar = UIFactory.Bar(row.transform, new Color(0, 0, 0, 0.5f), complete ? Palette.Green : Palette.Cyan);
            ((RectTransform)bar.transform).Place(0, 0.08f, 0.7f, 0.32f, RowTextLeft, 0, 0, 0);
            bar.Set(s.progress / (float)def.target, L.T("mis.progress_fmt", s.progress, def.target));

            string label = s.claimed ? L.T("mis.claimed") : complete ? L.T("mis.claim") : L.T("mis.in_progress");
            var btn = UIFactory.Button(row.transform, label, complete && !s.claimed ? Palette.Green : Palette.Grey, () =>
            {
                close();
                gm.ClaimMission(s, ShowMissions);
            }, 34);
            ((RectTransform)btn.transform).Place(0.72f, 0.22f, 1, 0.78f, 0, 0, 22, 0);
            btn.interactable = complete && !s.claimed;
            if (s.claimed) btn.SetIcon("icon_check");
        }

        const float RowTextLeft = 128f;

        /// <summary>Icono cuadrado a la izquierda de una fila de lista (misiones, logros).</summary>
        static void RowIcon(Transform row, Sprite sprite, Color tint)
        {
            var ic = UIFactory.Image(row, tint, false, "RowIcon");
            ic.sprite = sprite;
            ic.preserveAspect = true;
            var rt = ic.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = new Vector2(20, 0);
            rt.sizeDelta = new Vector2(92, 92);
        }

        /// <summary>Lo que hay que hacer en cada misión, con el mismo dibujo que en el juego cuando se puede.</summary>
        static Sprite MissionIcon(MissionType t)
        {
            switch (t)
            {
                case MissionType.DestroyDrones: return SpriteFactory.Drone;
                case MissionType.DestroyAsteroids: return SpriteFactory.Asteroid(0);
                case MissionType.CollectCoins: return UIKit.Get("icon_coin");
                case MissionType.NoDamageLevels: return UIKit.Get("icon_shield");
                default: return UIKit.Get("icon_flag");
            }
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

        // ---------------- COFRES ----------------
        /// <summary>Abre un cofre de ese tipo con animación: tiembla, se abre con un destello y aparece el premio.</summary>
        public void ShowChestOpen(ChestTier tier, Action after = null)
        {
            var gm = GameManager.Instance;
            var res = Chests.Open(gm.Data, tier);
            if (res == null) { after?.Invoke(); return; }
            gm.Save(); gm.NotifyChanged();
            StartCoroutine(ChestRoutine(res, after));
        }

        IEnumerator ChestRoutine(ChestResult res, Action after)
        {
            Color tc = Chests.TierColor(res.tier);
            var dim = UIFactory.Image(_dialogs, new Color(0, 0, 0, 0.88f), false, "ChestOpen");
            dim.raycastTarget = true;
            dim.rectTransform.Stretch();

            var title = UIFactory.Label(dim.transform, Chests.Name(res.tier).ToUpper(L.Culture), 70, tc, TextAnchor.MiddleCenter, true);
            title.rectTransform.Place(0, 0.8f, 1, 0.88f);
            title.gameObject.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.8f);
            var info = UIFactory.Button(dim.transform, "i", Palette.PanelLight, () => ShowChestOdds(res.tier), 50);
            ((RectTransform)info.transform).Fixed(new Vector2(1, 1), new Vector2(-36, -36), new Vector2(100, 100));

            var glow = UIFactory.Image(dim.transform, new Color(tc.r, tc.g, tc.b, 0f), false, "Glow");
            glow.sprite = SpriteFactory.Glow;
            ((RectTransform)glow.transform).Fixed(new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(820, 820));
            glow.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Cofre dibujado con piezas: cuerpo, banda dorada, tapa y cerradura
            var chest = UIFactory.Rect("Chest", dim.transform).Fixed(new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(380, 320));
            chest.pivot = new Vector2(0.5f, 0.5f);
            var body = UIFactory.Image(chest, tc * 0.7f + new Color(0, 0, 0, 1), true, "Body");
            body.rectTransform.Place(0, 0, 1, 0.62f);
            UIFactory.Image(body.transform, Palette.Gold, true, "Band").rectTransform.Place(0.44f, 0, 0.56f, 1);
            var lid = UIFactory.Image(chest, tc, true, "Lid");
            lid.rectTransform.Place(-0.04f, 0.58f, 1.04f, 0.92f);
            UIFactory.Image(lid.transform, Palette.Gold, true, "LidBand").rectTransform.Place(0.44f, 0, 0.56f, 1);
            var lockImg = UIFactory.Image(chest, Palette.Gold, true, "Lock");
            lockImg.rectTransform.Place(0.42f, 0.46f, 0.58f, 0.68f);

            // 1) Tiembla cada vez más
            AudioFX.Instance.Play(AudioFX.Sfx.PowerUp);
            for (float t = 0; t < 1.1f; t += Time.unscaledDeltaTime)
            {
                if (dim == null) yield break;
                float k = t / 1.1f;
                chest.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 42f) * 7f * k);
                chest.localScale = Vector3.one * (1f + 0.06f * k + 0.03f * Mathf.Sin(t * 30f));
                glow.color = new Color(tc.r, tc.g, tc.b, 0.25f * k);
                yield return null;
            }

            // 2) Se abre: destello, la tapa sale volando y el brillo crece
            AudioFX.Instance.Play(AudioFX.Sfx.Win);
            var flash = UIFactory.Image(dim.transform, new Color(1, 1, 1, 0.9f), false, "Flash");
            flash.rectTransform.Stretch();
            Vector2 lidStart = lid.rectTransform.anchoredPosition;
            chest.localRotation = Quaternion.identity;
            for (float t = 0; t < 0.5f; t += Time.unscaledDeltaTime)
            {
                if (dim == null) yield break;
                float p = t / 0.5f;
                lid.rectTransform.anchoredPosition = lidStart + new Vector2(0, 320f * p);
                lid.color = new Color(tc.r, tc.g, tc.b, 1f - p);
                lockImg.color = new Color(1, 0.8f, 0.2f, 1f - p);
                flash.color = new Color(1, 1, 1, 0.9f * (1f - p));
                glow.color = new Color(tc.r, tc.g, tc.b, 0.25f + 0.65f * p);
                glow.rectTransform.localScale = Vector3.one * (0.8f + 0.5f * p);
                body.color = new Color(body.color.r, body.color.g, body.color.b, 1f - p);
                yield return null;
            }
            Destroy(flash.gameObject);
            chest.gameObject.SetActive(false);

            // 3) Aparece el premio
            var reward = UIFactory.Rect("Reward", dim.transform).Place(0.06f, 0.2f, 0.94f, 0.78f);
            if (res.icon != null)
            {
                var icon = UIFactory.Image(reward, res.iconColor, false, "Icon");
                icon.sprite = res.icon;
                icon.preserveAspect = true;
                icon.rectTransform.Place(0.3f, 0.5f, 0.7f, 0.95f);
            }
            var head = UIFactory.Label(reward, res.headline, 60, Palette.Gold, TextAnchor.MiddleCenter, true);
            head.rectTransform.Place(0, 0.3f, 1, 0.48f);
            head.gameObject.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.8f);
            head.resizeTextForBestFit = true; head.resizeTextMaxSize = 60; head.resizeTextMinSize = 28;
            head.verticalOverflow = VerticalWrapMode.Truncate;
            if (!string.IsNullOrEmpty(res.detail))
                UIFactory.Label(reward, res.detail, 34, Palette.Text).rectTransform.Place(0, 0.12f, 1, 0.3f);
            var ok = UIFactory.Button(reward, L.T("mis.great"), Palette.Green, () =>
            {
                if (dim != null) Destroy(dim.gameObject);
                after?.Invoke();
            }, 48);
            ((RectTransform)ok.transform).Place(0.2f, -0.1f, 0.8f, 0.06f);

            for (float t = 0; t < 0.35f; t += Time.unscaledDeltaTime)
            {
                if (dim == null) yield break;
                float p = t / 0.35f;
                reward.localScale = Vector3.one * (p < 0.7f ? Mathf.Lerp(0.2f, 1.12f, p / 0.7f) : Mathf.Lerp(1.12f, 1f, (p - 0.7f) / 0.3f));
                yield return null;
            }
            reward.localScale = Vector3.one;

            // El brillo sigue latiendo hasta que se cierra
            while (dim != null)
            {
                glow.rectTransform.localScale = Vector3.one * (1.3f + Mathf.Sin(Time.unscaledTime * 3f) * 0.06f);
                yield return null;
            }
        }

        /// <summary>Probabilidades de cada cofre (por transparencia: los cofres no se venden).</summary>
        public void ShowChestOdds(ChestTier tier)
        {
            IconDialog("icon_chest", L.T("chest.odds_title_fmt", Chests.Name(tier)), Chests.OddsText(tier) + "\n\n<size=30>" + L.T("chest.odds_note") + "</size>", (p, close) =>
                DialogButton(p, L.T("close"), Palette.Grey, null, close));
        }

        // ---------------- RACHA DIARIA ----------------
        /// <summary>Los 7 días de la racha: reclamados (verde), hoy (dorado) y próximos (gris), con el botón para reclamar.</summary>
        public void ShowDailyStreak()
        {
            var gm = GameManager.Instance;
            var d = gm.Data;
            d.RefreshDaily();
            bool available = DailyStreak.Available(d);
            int day = DailyStreak.CurrentDay(d);
            string body = L.T("streak.body");
            if (DailyStreak.StreakLost(d)) body = "<color=#FF8A80>" + L.T("streak.lost_fmt", d.streakDay) + "</color>\n" + body;

            IconDialog("icon_flame", L.T("streak.title"), body, (p, close) =>
            {
                var grid = UIFactory.Rect("Days", p);
                var gl = grid.gameObject.AddComponent<GridLayoutGroup>();
                gl.cellSize = new Vector2(150, 190);
                gl.spacing = new Vector2(12, 12);
                gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gl.constraintCount = 4;
                gl.childAlignment = TextAnchor.UpperCenter;
                grid.Pref(392);
                for (int i = 0; i < DailyStreak.Days.Length; i++)
                {
                    int n = i + 1;
                    bool done = n < day || (n == day && !available);   // ya reclamado en esta racha
                    bool today = n == day && available;
                    var cell = UIFactory.Image(grid, done ? new Color(0.12f, 0.4f, 0.22f) : today ? Palette.Gold : Palette.PanelLight, true, "Day");
                    var title = UIFactory.Label(cell.transform, L.T("streak.day_fmt", n), 28, today ? new Color(0.25f, 0.15f, 0f) : Palette.Text, TextAnchor.MiddleCenter, true);
                    title.rectTransform.Place(0, 0.66f, 1, 1, 4, 0, 4, 6);
                    var ic = UIKit.Icon(cell.transform, done ? "icon_check" : StreakIcon(DailyStreak.Days[i].type));
                    ic.rectTransform.Place(0.25f, 0.34f, 0.75f, 0.68f);
                    var rw = UIFactory.Label(cell.transform, done ? L.T("streak.done") : DailyStreak.CellText(i), 26,
                        today ? new Color(0.25f, 0.15f, 0f) : done ? new Color(0.75f, 1f, 0.8f) : Palette.TextDim, TextAnchor.MiddleCenter, true);
                    rw.rectTransform.Place(0, 0, 1, 0.36f, 6, 6, 6, 0);
                    rw.resizeTextForBestFit = true; rw.resizeTextMinSize = 16; rw.resizeTextMaxSize = 26;
                    rw.verticalOverflow = VerticalWrapMode.Truncate;
                    if (today) cell.gameObject.AddComponent<Outline>().effectDistance = new Vector2(4, 4);
                }

                if (available)
                    DialogButton(p, L.T("streak.claim_fmt", day), Palette.Green, gm.ClaimDaily, close, true, "icon_gift");
                else if (d.vip && !d.vipLivesClaimed)
                    DialogButton(p, L.T("streak.vip_claim"), Palette.Gold, gm.ClaimDaily, close, true, "icon_crown");
                else
                {
                    var wait = DialogButton(p, "", Palette.Grey, null, close, false);
                    wait.interactable = false;
                    wait.GetComponentInChildren<Text>().gameObject.AddComponent<LiveText>().source = () =>
                        L.T("streak.come_back_fmt", GameManager.FormatTime(DailyStreak.UntilTomorrow));
                }
                DialogButton(p, L.T("close"), Palette.Grey, null, close);
            });
        }

        static string StreakIcon(StreakRewardType t) =>
            t == StreakRewardType.Chest ? "icon_chest" : t == StreakRewardType.Paint ? "icon_brush" : t == StreakRewardType.TempShip ? "icon_rocket" : "icon_coin";

        // ---------------- LOGROS ----------------
        /// <summary>Lista de logros (con scroll): primero los que se pueden reclamar, luego los pendientes y al final los reclamados.</summary>
        public void ShowAchievements()
        {
            var d = GameManager.Instance.Data;
            var ordered = new List<AchievementDefinition>();
            ordered.AddRange(Achievements.All.FindAll(a => Achievements.IsUnlocked(d, a) && !Achievements.IsClaimed(d, a)));
            ordered.AddRange(Achievements.All.FindAll(a => !Achievements.IsUnlocked(d, a)));
            ordered.AddRange(Achievements.All.FindAll(a => Achievements.IsClaimed(d, a)));

            IconDialog("icon_trophy", L.T("ach.title_fmt", Achievements.UnlockedCount(d), Achievements.All.Count), null, (p, close) =>
            {
                var area = UIFactory.Image(p, new Color(0, 0, 0, 0.25f), true, "AchArea");
                area.Pref(1180);
                var content = UIFactory.ScrollList(area.transform, out var scroll, 14, 14);
                ((RectTransform)scroll.transform).Stretch();
                UIFactory.AddVerticalScrollbar(scroll);
                foreach (var a in ordered) AchievementRow(content, a, close);
                DialogButton(p, L.T("close"), Palette.Grey, null, close);
            });
        }

        void AchievementRow(RectTransform parent, AchievementDefinition a, Action close)
        {
            var gm = GameManager.Instance;
            var d = gm.Data;
            bool unlocked = Achievements.IsUnlocked(d, a), claimed = Achievements.IsClaimed(d, a);
            int prog = Achievements.Progress(d, a);

            var row = UIFactory.Image(parent, unlocked ? new Color(0.22f, 0.18f, 0.07f) : Palette.PanelLight, true, "Achievement");
            row.Pref(210);
            RowIcon(row.transform, UIKit.Get(claimed ? "icon_check" : unlocked ? "icon_trophy" : "icon_lock"), unlocked || claimed ? Color.white : new Color(0.6f, 0.65f, 0.78f));
            var name = FitLabel(row.transform, a.Name, 34, unlocked ? Palette.Gold : Palette.Text, true);
            name.rectTransform.Place(0, 0.62f, 0.68f, 1, RowTextLeft, 0, 0, 8);
            var desc = FitLabel(row.transform, a.Description, 26, Palette.TextDim, false);
            desc.rectTransform.Place(0, 0.36f, 0.68f, 0.62f, RowTextLeft, 0, 0, 0);
            var bar = UIFactory.Bar(row.transform, new Color(0, 0, 0, 0.5f), unlocked ? Palette.Green : Palette.Cyan);
            ((RectTransform)bar.transform).Place(0, 0.08f, 0.68f, 0.32f, RowTextLeft, 0, 0, 0);
            bar.Set(prog / (float)a.target, UIFactory.Fmt(prog) + " / " + UIFactory.Fmt(a.target));

            var reward = UIFactory.Label(row.transform, "+" + UIFactory.Fmt(a.coins), 30, Palette.Gold, TextAnchor.MiddleCenter, true);
            reward.rectTransform.Place(0.7f, 0.62f, 1, 0.95f, 0, 0, 20, 0);
            UIKit.LabelIcon(reward, "icon_coin", 36);
            string label = claimed ? L.T("mis.claimed") : unlocked ? L.T("mis.claim") : L.T("ach.locked");
            var btn = UIFactory.Button(row.transform, label, unlocked && !claimed ? Palette.Green : Palette.Grey, () =>
            {
                close();
                gm.ClaimAchievement(a, ShowAchievements);
            }, 32);
            ((RectTransform)btn.transform).Place(0.7f, 0.1f, 1, 0.58f, 0, 0, 20, 0);
            btn.interactable = unlocked && !claimed;
        }

        public void ShowLevelComplete(int level, int points, int coins, int destroyed, bool campaignDone, int pickupCoins = 0, string trialProductId = null, List<string> notes = null, bool chestEarned = false)
        {
            var gm = GameManager.Instance;
            string body = L.T("dlg.complete_body_fmt", UIFactory.Fmt(points), UIFactory.Fmt(coins), destroyed);
            if (pickupCoins > 0) body += "\n" + L.T("dlg.pickup_coins_fmt", UIFactory.Fmt(pickupCoins));
            if (notes != null)
                foreach (var n in notes) body += "\n<color=#FFCC33>" + n + "</color>"; // misiones y logros conseguidos
            if (campaignDone) body += "\n\n" + L.T("dlg.campaign_done");
            IconDialog("icon_star", L.T("dlg.complete_title_fmt", level), body, (p, close) =>
            {
                // Cofre raro de cada 5 niveles: se abre aquí mismo (si no, queda guardado en el menú)
                if (chestEarned && gm.Data.chestRare > 0)
                {
                    Button open = null;
                    open = DialogButton(p, L.T("chest.open_fmt", Chests.Name(ChestTier.Rare).ToUpper(L.Culture)), Palette.Purple, () =>
                    {
                        open.interactable = false;
                        ShowChestOpen(ChestTier.Rare);
                    }, close, false, "icon_chest");
                }
                // Si probó un arma de pago en este nivel, se le ofrece comprarla
                if (trialProductId != null && !ProductCatalog.IsOwned(gm.Data, trialProductId))
                {
                    var def = ProductCatalog.Get(trialProductId);
                    if (def != null)
                        DialogButton(p, L.T("dlg.trial_buy_fmt", def.Title, IAPManager.Instance.GetPrice(trialProductId)), Palette.Purple,
                            () => gm.BuyProduct(trialProductId), close, false, "icon_cart");
                }
                Button dbl = null;
                dbl = DialogButton(p, L.T("dlg.double_coins"), Palette.Green, () =>
                {
                    dbl.interactable = false;
                    gm.DoubleLastCoins(null);
                }, close, false, "icon_video");
                DialogButton(p, L.T("dlg.share"), Palette.Purple, () =>
                    PlatformServices.ShareText(L.T("share.level_fmt", level, UIFactory.Fmt(gm.Data.totalPoints), PlatformServices.StoreUrl)), close, false, "icon_share");
                if (!campaignDone)
                    DialogButton(p, L.T("dlg.next_fmt", gm.Data.currentLevel), Palette.Cyan, () => gm.AfterRunInterstitial(level, gm.PlayCurrentLevel), close, true, "icon_play");
                else
                    DialogButton(p, L.T("dlg.replay_last"), Palette.Cyan, () => gm.AfterRunInterstitial(level, gm.PlayCurrentLevel), close, true, "icon_restore");
                DialogButton(p, L.T("dlg.main_menu"), Palette.Grey, () => gm.AfterRunInterstitial(level, gm.ReturnToMenu), close, true, "icon_home");
            });
        }

        public void ShowLevelFailed(int level)
        {
            var gm = GameManager.Instance;
            int lives = gm.Data.lives;
            string body = lives > 0 ? L.T("dlg.failed_body_fmt", lives, level) : L.T("dlg.failed_last_fmt", GameConfig.LifeRefillHours, GameConfig.DefaultLives);
            IconDialog("icon_heart", L.T("dlg.failed_title"), body, (p, close) =>
            {
                if (lives > 0)
                    DialogButton(p, L.T("dlg.retry"), Palette.Cyan, () => gm.AfterRunInterstitial(level, () => gm.PlayLevel(level)), close, true, "icon_restore");
                else
                    DialogButton(p, L.T("dlg.get_lives"), Palette.Green, () => { gm.ReturnToMenu(); ShowLivesPanel(true); }, close, true, "icon_heart");
                DialogButton(p, L.T("dlg.main_menu"), Palette.Grey, () => gm.AfterRunInterstitial(level, gm.ReturnToMenu), close, true, "icon_home");
            });
        }

        public void ShowRevive(bool canRevive, int level)
        {
            var gp = GameManager.Instance.Gameplay;
            if (!canRevive || !AdsManager.Instance.IsRewardedAvailable) { gp.DeclineRevive(); return; }
            IconDialog("icon_heart", L.T("dlg.revive_title"), L.T("dlg.revive_body"), (p, close) =>
            {
                DialogButton(p, L.T("dlg.revive_btn"), Palette.Green, () =>
                    AdsManager.Instance.ShowRewarded(ok => { if (ok) gp.Revive(); else gp.DeclineRevive(); }), close, true, "icon_video");
                DialogButton(p, L.T("dlg.no_thanks"), Palette.Grey, gp.DeclineRevive, close, true, "icon_close");
            });
        }

        public void ShowPause()
        {
            var gm = GameManager.Instance;
            IconDialog("icon_pause", L.T("dlg.pause"), L.T("dlg.level_fmt", gm.Gameplay.Level), (p, close) =>
            {
                DialogButton(p, L.T("dlg.continue"), Palette.Cyan, gm.Gameplay.Resume, close, true, "icon_play");
                Button snd = null;
                snd = DialogButton(p, L.T(gm.Data.soundOn ? "set.sound_on" : "set.sound_off"), Palette.Grey, () =>
                {
                    gm.Data.soundOn = !gm.Data.soundOn; gm.Save();
                    snd.SetLabel(L.T(gm.Data.soundOn ? "set.sound_on" : "set.sound_off"));
                    snd.SetIcon(gm.Data.soundOn ? "icon_sound" : "icon_sound_off");
                }, close, false, gm.Data.soundOn ? "icon_sound" : "icon_sound_off");
                Button mus = null;
                mus = DialogButton(p, L.T(gm.Data.musicOn ? "set.music_on" : "set.music_off"), Palette.Grey, () =>
                {
                    gm.Data.musicOn = !gm.Data.musicOn; gm.Save();
                    mus.SetLabel(L.T(gm.Data.musicOn ? "set.music_on" : "set.music_off"));
                    mus.SetIcon(gm.Data.musicOn ? "icon_music" : "icon_music_off");
                }, close, false, gm.Data.musicOn ? "icon_music" : "icon_music_off");
                DialogButton(p, L.T("dlg.abandon"), Palette.Red, () =>
                    Confirm(L.T("dlg.abandon_title"), L.T("dlg.abandon_body"), L.T("dlg.abandon_yes"), gm.Gameplay.Abandon), close, true, "icon_home");
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
