using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AeroBlitz
{
    /// <summary>
    /// Pantalla de inicio: barra superior (piloto, monedas, vidas, ajustes), logo, nave con flechas para cambiarla,
    /// datos de la nave, progreso de la campaña, botón JUGAR, accesos (hangar, tienda, misiones, ranking),
    /// top mundial y fila inferior (racha, vida gratis, logros).
    /// </summary>
    public class MainMenuScreen : UIScreen
    {
        const float ShipSize = 330f;

        Text _player, _coins, _lives, _vip, _shipName, _shipLevel, _points, _levelText, _levelPct, _playTitle, _playSub, _rankStatus, _streakDay;
        Text _missionBadge, _achBadge, _dailyBadge, _chestCount;
        RectTransform _content, _shipGroup, _rankList;
        Image _shipImg, _glow, _flame, _playBg, _playIcon;
        Outline _playBorder;
        GameObject _prevShip, _nextShip, _chests;
        readonly List<Image> _segments = new List<Image>();
        float _appliedInset = -1f;

        protected override void Build()
        {
            // Fondo espacial (cubre toda la pantalla sin deformarse)
            var bg = UIFactory.Image(Root, Palette.Bg, false, "Bg");
            bg.rectTransform.Stretch();
            var bgArt = UIKit.Get("menu_bg");
            if (bgArt != null)
            {
                bg.sprite = bgArt; bg.color = Color.white;
                var fit = bg.gameObject.AddComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio = bgArt.rect.width / bgArt.rect.height;
            }
            // Todo el contenido va en un contenedor que se encoge por abajo cuando hay banner.
            _content = UIFactory.Rect("Content", Root).Stretch();

            BuildTopBar();

            var logo = UIFactory.Image(_content, Color.white, false, "Logo");
            logo.sprite = UIKit.Get("logo");
            logo.preserveAspect = true;
            logo.rectTransform.Place(0.1f, 0.795f, 0.9f, 0.935f);

            BuildShipArea();
            BuildShipInfo();
            BuildProgress();
            BuildPlayButton();
            BuildTiles();
            BuildRanking();
            BuildBottomRow();
            // El 7.5 % inferior queda libre para el banner publicitario; si el banner es más alto, ver ApplyBannerInset.
        }

        // ---------------- BARRA SUPERIOR ----------------
        void BuildTopBar()
        {
            var gm = GameManager.Instance;
            var pl = UIKit.Panel(_content, UIKit.PanelDark, UIKit.Border, "Player");
            pl.rectTransform.Place(0.03f, 0.948f, 0.37f, 0.99f);
            UIKit.Icon(pl.transform, "icon_user").rectTransform.Place(0.04f, 0.18f, 0.2f, 0.82f);
            _player = UIFactory.Label(pl.transform, "", 28, Palette.Text, TextAnchor.MiddleLeft, true);
            _player.rectTransform.Place(0.22f, 0, 1, 1, 0, 0, 10, 0);
            FitText(_player, 16);
            _vip = UIFactory.Label(_content, "", 24, Palette.Gold, TextAnchor.MiddleLeft, true);
            _vip.rectTransform.Place(0.04f, 0.928f, 0.4f, 0.948f);

            var coins = UIKit.Panel(_content, UIKit.PanelDark, UIKit.Border, "Coins");
            coins.rectTransform.Place(0.39f, 0.948f, 0.66f, 0.99f);
            UIKit.Icon(coins.transform, "icon_coin").rectTransform.Place(0.03f, 0.14f, 0.24f, 0.86f);
            _coins = UIFactory.Label(coins.transform, "", 30, Palette.Text, TextAnchor.MiddleLeft, true);
            _coins.rectTransform.Place(0.26f, 0, 0.74f, 1);
            FitText(_coins, 16);
            PlusButton(coins.transform, () => UIManager.Instance.ShowScreen<StoreScreen>().SelectTab(ProductCatalog.CatCoins));

            var lives = UIKit.Panel(_content, UIKit.PanelDark, UIKit.Border, "Lives");
            lives.rectTransform.Place(0.68f, 0.948f, 0.87f, 0.99f);
            UIKit.Icon(lives.transform, "icon_heart").rectTransform.Place(0.04f, 0.16f, 0.32f, 0.84f);
            _lives = UIFactory.Label(lives.transform, "", 30, Palette.Text, TextAnchor.MiddleLeft, true);
            _lives.rectTransform.Place(0.34f, 0, 0.68f, 1);
            FitText(_lives, 14);
            _lives.gameObject.AddComponent<LiveText>().source = LivesText;
            PlusButton(lives.transform, () => UIManager.Instance.ShowLivesPanel());

            var gear = UIFactory.Button(_content, "", new Color(0, 0, 0, 0), ShowSettings, 20);
            gear.GetComponent<RectTransform>().Place(0.885f, 0.948f, 0.975f, 0.99f);
            UIKit.Icon(gear.transform, "icon_gear").rectTransform.Stretch(4);
        }

        static void PlusButton(Transform chip, UnityEngine.Events.UnityAction onClick)
        {
            var plus = UIFactory.Button(chip, "+", new Color(0.18f, 0.72f, 0.3f), onClick, 36);
            plus.GetComponent<RectTransform>().Place(0.76f, 0.14f, 0.97f, 0.86f);
        }

        static void FitText(Text t, int min)
        {
            t.resizeTextForBestFit = true; t.resizeTextMinSize = min; t.resizeTextMaxSize = t.fontSize;
            t.verticalOverflow = VerticalWrapMode.Truncate;
        }

        // ---------------- NAVE ----------------
        void BuildShipArea()
        {
            _shipGroup = UIFactory.Rect("ShipGroup", _content).Place(0.2f, 0.6f, 0.8f, 0.795f);
            _shipGroup.gameObject.AddComponent<Bobbing>().amplitude = 10;
            _glow = UIFactory.Image(_shipGroup, new Color(0.3f, 0.6f, 1f, 0.35f), false, "Glow");
            _glow.sprite = SpriteFactory.Glow;
            ((RectTransform)_glow.transform).Fixed(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(ShipSize * 1.5f, ShipSize * 1.5f));
            _glow.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _flame = UIFactory.Image(_shipGroup, Color.white, false, "Flame");
            _flame.preserveAspect = true;
            _flame.rectTransform.anchorMin = _flame.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _flame.rectTransform.pivot = new Vector2(0.5f, 1f);
            _flame.rectTransform.sizeDelta = new Vector2(ShipSize * 0.2f, ShipSize * 0.34f);
            _flame.gameObject.AddComponent<UIFlameFlicker>();
            _shipImg = ShipUI.ShipImage(_shipGroup, GameManager.Instance.Data.SelectedShipState, ShipSize);
            _shipImg.rectTransform.anchorMin = _shipImg.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            _prevShip = Arrow("icon_chevron_left", 0.03f, -1);
            _nextShip = Arrow("icon_chevron_right", 0.85f, 1);

            // Cofres guardados sin abrir (solo se ve si hay alguno)
            var chest = UIFactory.Button(_content, "", new Color(0, 0, 0, 0), () => GameManager.Instance.OpenNextChest(), 20);
            chest.GetComponent<RectTransform>().Place(0.84f, 0.745f, 0.97f, 0.8f);
            UIKit.Icon(chest.transform, "icon_chest").rectTransform.Stretch(2);
            chest.gameObject.AddComponent<Bobbing>().amplitude = 5;
            _chestCount = UIKit.Badge(chest.transform, 48);
            _chests = chest.gameObject;
        }

        GameObject Arrow(string icon, float x, int dir)
        {
            var b = UIFactory.Button(_content, "", new Color(0.05f, 0.1f, 0.25f, 0.55f), () => CycleShip(dir), 20);
            b.GetComponent<RectTransform>().Place(x, 0.665f, x + 0.12f, 0.73f);
            var o = b.gameObject.AddComponent<Outline>();
            o.effectColor = UIKit.Border; o.effectDistance = new Vector2(3, 3);
            UIKit.Icon(b.transform, icon).rectTransform.Stretch(18);
            return b.gameObject;
        }

        /// <summary>Cambia a la siguiente nave que tiene (las flechas solo aparecen si tiene más de una).</summary>
        void CycleShip(int dir)
        {
            var gm = GameManager.Instance;
            var owned = ShipCatalog.All.FindAll(s => gm.Data.OwnsShip(s.id));
            if (owned.Count < 2) return;
            int i = Mathf.Max(0, owned.FindIndex(s => s.id == gm.Data.selectedShip));
            gm.Data.selectedShip = owned[(i + dir + owned.Count) % owned.Count].id;
            gm.Save(); gm.NotifyChanged();
        }

        void BuildShipInfo()
        {
            var bar = UIKit.Panel(_content, UIKit.PanelDark, UIKit.Border, "ShipInfo");
            bar.rectTransform.Place(0.06f, 0.553f, 0.94f, 0.596f);
            _shipName = UIFactory.Label(bar.transform, "", 30, Palette.Text, TextAnchor.MiddleCenter, true);
            _shipName.rectTransform.Place(0, 0, 0.34f, 1);
            FitText(_shipName, 16);
            Divider(bar.transform, 0.34f);
            UIKit.Icon(bar.transform, "icon_star").rectTransform.Place(0.37f, 0.18f, 0.44f, 0.82f);
            _shipLevel = UIFactory.Label(bar.transform, "", 28, Palette.Text, TextAnchor.MiddleLeft);
            _shipLevel.rectTransform.Place(0.45f, 0, 0.64f, 1);
            FitText(_shipLevel, 14);
            Divider(bar.transform, 0.645f);
            UIKit.Icon(bar.transform, "icon_trophy").rectTransform.Place(0.67f, 0.16f, 0.74f, 0.84f);
            _points = UIFactory.Label(bar.transform, "", 28, Palette.Text, TextAnchor.MiddleLeft, true);
            _points.rectTransform.Place(0.75f, 0, 0.99f, 1);
            FitText(_points, 14);
        }

        static void Divider(Transform parent, float x)
        {
            var d = UIFactory.Image(parent, new Color(0.4f, 0.55f, 0.9f, 0.4f), false, "Divider");
            d.rectTransform.Place(x, 0.2f, x, 0.8f, -1, 0, -1, 0);
        }

        void BuildProgress()
        {
            _levelText = UIFactory.Label(_content, "", 28, Palette.Text, TextAnchor.MiddleLeft, true);
            _levelText.rectTransform.Place(0.07f, 0.527f, 0.7f, 0.55f);
            _levelPct = UIFactory.Label(_content, "", 28, Palette.Text, TextAnchor.MiddleRight, true);
            _levelPct.rectTransform.Place(0.7f, 0.527f, 0.93f, 0.55f);
            var bar = UIFactory.Rect("Segments", _content).Place(0.06f, 0.507f, 0.94f, 0.523f);
            var h = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 6; h.childControlWidth = true; h.childControlHeight = true; h.childForceExpandWidth = true; h.childForceExpandHeight = true;
            for (int i = 0; i < 14; i++) _segments.Add(UIFactory.Image(bar, Color.white, true, "Seg"));
        }

        void BuildPlayButton()
        {
            var play = UIKit.Tile(_content, new Color(1f, 0.52f, 0.1f), new Color(1f, 0.8f, 0.35f, 0.9f), () =>
            {
                // Con vidas se abre la Ruta Galáctica; sin vidas, el aviso de vidas de siempre
                if (GameManager.Instance.Data.lives > 0) UIManager.Instance.ShowScreen<GalaxyRouteScreen>();
                else GameManager.Instance.PlayCurrentLevel();
            });
            play.GetComponent<RectTransform>().Place(0.06f, 0.405f, 0.94f, 0.495f);
            _playBg = play.GetComponent<Image>();
            _playBorder = play.GetComponent<Outline>();
            _playIcon = UIKit.Icon(play.transform, "icon_play");
            _playIcon.rectTransform.Place(0.2f, 0.42f, 0.33f, 0.9f);
            _playTitle = UIFactory.Label(play.transform, "", 82, Color.white, TextAnchor.MiddleLeft, true);
            _playTitle.rectTransform.Place(0.35f, 0.36f, 0.95f, 0.98f);
            FitText(_playTitle, 40);
            _playTitle.gameObject.AddComponent<Shadow>().effectColor = new Color(0.45f, 0.18f, 0f, 0.8f);
            _playSub = UIFactory.Label(play.transform, "", 30, new Color(1f, 0.95f, 0.85f), TextAnchor.MiddleCenter, true);
            _playSub.rectTransform.Place(0, 0.06f, 1, 0.38f);
            _playSub.gameObject.AddComponent<LiveText>().source = PlaySubText;
        }

        void BuildTiles()
        {
            var row = UIFactory.Rect("Tiles", _content).Place(0.06f, 0.3f, 0.94f, 0.395f);
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 16; h.childControlWidth = true; h.childControlHeight = true; h.childForceExpandWidth = true; h.childForceExpandHeight = true;
            var blue = new Color(0.08f, 0.24f, 0.55f); var blueB = new Color(0.4f, 0.68f, 1f, 0.9f);
            Tile(row, "icon_rocket", L.T("menu.hangar"), blue, blueB, () => UIManager.Instance.ShowScreen<HangarScreen>());
            Tile(row, "icon_cart", L.T("menu.store"), new Color(0.24f, 0.14f, 0.52f), new Color(0.7f, 0.5f, 1f, 0.9f), () => UIManager.Instance.ShowScreen<StoreScreen>());
            var mis = Tile(row, "icon_missions", L.T("menu.missions"), new Color(0.45f, 0.3f, 0.05f), new Color(1f, 0.8f, 0.3f, 0.95f), () => UIManager.Instance.ShowMissions());
            _missionBadge = UIKit.Badge(mis.transform);
            Tile(row, "icon_bars", L.T("menu.ranking"), blue, blueB, () => UIManager.Instance.ShowScreen<RankingScreen>());
        }

        static Button Tile(Transform row, string icon, string label, Color fill, Color border, UnityEngine.Events.UnityAction onClick)
        {
            var b = UIKit.Tile(row, fill, border, onClick);
            UIKit.Icon(b.transform, icon).rectTransform.Place(0.22f, 0.36f, 0.78f, 0.9f);
            var t = UIFactory.Label(b.transform, label, 26, Color.white, TextAnchor.MiddleCenter, true);
            t.rectTransform.Place(0.02f, 0.05f, 0.98f, 0.34f);
            FitText(t, 14);
            return b;
        }

        // ---------------- RANKING ----------------
        void BuildRanking()
        {
            var panel = UIKit.Panel(_content, UIKit.PanelDark, UIKit.Border, "RankPanel");
            panel.rectTransform.Place(0.06f, 0.137f, 0.94f, 0.29f);
            UIKit.Icon(panel.transform, "icon_globe").rectTransform.Place(0.025f, 0.79f, 0.095f, 0.97f);
            var title = UIFactory.Label(panel.transform, L.T("menu.top_world"), 30, Palette.Gold, TextAnchor.MiddleLeft, true);
            title.rectTransform.Place(0.11f, 0.79f, 0.6f, 0.97f);
            _rankStatus = UIFactory.Label(panel.transform, "", 24, Palette.TextDim, TextAnchor.MiddleRight);
            _rankStatus.rectTransform.Place(0.55f, 0.79f, 0.97f, 0.97f);
            _rankList = UIFactory.Rect("List", panel.transform).Place(0.025f, 0.25f, 0.88f, 0.78f);
            var vlg = _rankList.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2; vlg.childControlHeight = true; vlg.childControlWidth = true; vlg.childForceExpandHeight = true; vlg.childForceExpandWidth = true;
            var more = UIFactory.Button(panel.transform, "", new Color(0, 0, 0, 0), () => UIManager.Instance.ShowScreen<RankingScreen>(), 20);
            more.GetComponent<RectTransform>().Place(0.89f, 0.33f, 0.98f, 0.72f);
            UIKit.Icon(more.transform, "icon_chevron_right").rectTransform.Stretch(6);
            var all = UIKit.Tile(panel.transform, new Color(0.08f, 0.14f, 0.32f), UIKit.Border, () => UIManager.Instance.ShowScreen<RankingScreen>());
            all.GetComponent<RectTransform>().Place(0.24f, 0.04f, 0.76f, 0.22f);
            var allT = UIFactory.Label(all.transform, L.T("menu.see_full_ranking"), 24, Palette.Text, TextAnchor.MiddleCenter, true);
            allT.rectTransform.Stretch(4);
            FitText(allT, 14);
        }

        public void LoadRanking()
        {
            _rankStatus.text = L.T("loading");
            LeaderboardService.Instance.Fetch(null, (list, online) =>
            {
                if (this == null) return;
                _rankStatus.text = L.T(online ? "online" : "offline");
                UIFactory.Clear(_rankList);
                for (int i = 0; i < Mathf.Min(3, list.Count); i++)
                {
                    var e = list[i];
                    var row = UIFactory.Rect("Row", _rankList);
                    var medal = UIKit.Icon(row, "medal_" + Mathf.Clamp(e.rank, 1, 3));
                    medal.rectTransform.Place(0, 0.04f, 0.09f, 0.96f);
                    var name = UIFactory.Label(row, $"{e.name}  <color=#9FB0D0>({e.country})</color>", 27, e.isMe ? Palette.Gold : Palette.Text, TextAnchor.MiddleLeft, e.isMe);
                    name.rectTransform.Place(0.11f, 0, 0.72f, 1);
                    FitText(name, 14);
                    var pts = UIFactory.Label(row, UIFactory.Fmt(e.points), 27, Palette.Cyan, TextAnchor.MiddleRight, true);
                    pts.rectTransform.Place(0.72f, 0, 1, 1);
                }
                if (list.Count == 0) UIFactory.Label(_rankList, L.T("menu.no_pilots"), 26, Palette.TextDim);
            });
        }

        // ---------------- FILA INFERIOR ----------------
        void BuildBottomRow()
        {
            var row = UIFactory.Rect("Bottom", _content).Place(0.06f, 0.077f, 0.94f, 0.127f);
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 14; h.childControlWidth = true; h.childControlHeight = true; h.childForceExpandWidth = true; h.childForceExpandHeight = true;

            var streak = UIKit.Tile(row, new Color(0.42f, 0.2f, 0.07f), new Color(1f, 0.6f, 0.25f, 0.9f), () => UIManager.Instance.ShowDailyStreak());
            UIKit.Icon(streak.transform, "icon_flame").rectTransform.Place(0.04f, 0.12f, 0.3f, 0.88f);
            var st = UIFactory.Label(streak.transform, L.T("menu.streak_btn"), 26, Color.white, TextAnchor.MiddleLeft, true);
            st.rectTransform.Place(0.32f, 0.48f, 0.98f, 0.96f); FitText(st, 14);
            _streakDay = UIFactory.Label(streak.transform, "", 22, new Color(1f, 0.85f, 0.7f), TextAnchor.MiddleLeft);
            _streakDay.rectTransform.Place(0.32f, 0.04f, 0.98f, 0.5f); FitText(_streakDay, 12);
            _dailyBadge = UIKit.Badge(streak.transform, 46);

            var life = UIKit.Tile(row, new Color(0.08f, 0.4f, 0.18f), new Color(0.4f, 0.95f, 0.5f, 0.9f), () => UIManager.Instance.ShowLivesPanel());
            UIKit.Icon(life.transform, "icon_heart").rectTransform.Place(0.04f, 0.14f, 0.27f, 0.86f);
            var lt = UIFactory.Label(life.transform, L.T("menu.free_life_btn"), 26, Color.white, TextAnchor.MiddleLeft, true);
            lt.rectTransform.Place(0.29f, 0.48f, 0.98f, 0.96f); FitText(lt, 14);
            var lv = UIFactory.Label(life.transform, L.T("menu.watch_video"), 22, new Color(0.8f, 1f, 0.85f), TextAnchor.MiddleLeft);
            lv.rectTransform.Place(0.29f, 0.04f, 0.8f, 0.5f); FitText(lv, 12);
            UIKit.Icon(life.transform, "icon_video").rectTransform.Place(0.8f, 0.08f, 0.96f, 0.46f);

            var ach = UIKit.Tile(row, new Color(0.28f, 0.14f, 0.52f), new Color(0.72f, 0.5f, 1f, 0.9f), () => UIManager.Instance.ShowAchievements());
            UIKit.Icon(ach.transform, "icon_trophy").rectTransform.Place(0.06f, 0.12f, 0.32f, 0.88f);
            var at = UIFactory.Label(ach.transform, L.T("menu.achievements"), 28, Color.white, TextAnchor.MiddleLeft, true);
            at.rectTransform.Place(0.35f, 0, 0.98f, 1); FitText(at, 14);
            _achBadge = UIKit.Badge(ach.transform, 46);
        }

        /// <summary>Sube el borde inferior del contenido lo necesario para que el banner no tape la fila de botones.</summary>
        void ApplyBannerInset()
        {
            const float FreeFraction = 0.075f; // hueco que el diseño ya deja abajo
            float h = Root.rect.height;
            float pxPerUnit = Root.lossyScale.y;
            if (h <= 0 || pxPerUnit <= 0) return;
            float reservePx = AdsManager.Instance != null ? AdsManager.Instance.BannerReservePixels : 0f;
            float extra = 0f;
            if (reservePx > 0)
            {
                // El banner se mide desde el borde de la pantalla; el contenido empieza sobre el área segura.
                float needed = Mathf.Max(0f, reservePx - Screen.safeArea.yMin) / pxPerUnit + 14f;
                float free = FreeFraction * h;
                if (needed > free) extra = (needed - free) / (1f - FreeFraction);
            }
            if (Mathf.Approximately(extra, _appliedInset)) return;
            _appliedInset = extra;
            _content.offsetMin = new Vector2(0, extra);
        }

        void Update() => ApplyBannerInset();

        public override void OnShow()
        {
            GameManager.Instance.Data.RefreshDaily(); // si pasó la medianoche con el juego abierto, misiones nuevas
            AdsManager.Instance.ShowBanner();
            LoadRanking();
        }

        public override void Refresh()
        {
            var gm = GameManager.Instance;
            var d = gm.Data;
            _player.text = $"{d.username} [{d.countryCode}]";
            _vip.text = d.vip ? L.T("menu.vip") : "";
            _coins.text = UIFactory.Fmt(d.coins);
            _lives.text = LivesText();

            // Nave seleccionada: aspecto, brillo (más intenso en las premium) y fuego con su estela
            var state = d.SelectedShipState;
            var def = ShipCatalog.Get(state.shipId);
            var stats = ShipStats.Compute(state);
            ShipUI.UpdateShipImage(_shipImg, state, def);
            var acc = def.accent;
            _glow.color = new Color(acc.r, acc.g, acc.b, def.IsPremium ? 0.55f : 0.3f);
            var trail = UpgradeCatalog.GetTrail(stats.trailId);
            _flame.sprite = trail.id == "default" ? SpriteFactory.Flame : SpriteFactory.FlameMask;
            _flame.color = trail.color;
            _flame.GetComponent<UIFlameFlicker>().rainbow = trail.animated;
            _flame.rectTransform.anchoredPosition = new Vector2(0, def.flameY * SpriteFactory.ShipScale * ShipSize / 2f + ShipSize * 0.03f);
            bool several = ShipCatalog.All.FindAll(s => d.OwnsShip(s.id)).Count > 1;
            _prevShip.SetActive(several); _nextShip.SetActive(several);
            _shipName.text = def.IsPremium ? $"<color=#FFD24A>{def.Name}</color>" : def.Name;
            _shipLevel.text = L.T("menu.ship_level_fmt", stats.shipLevel);
            _points.text = L.T("menu.points_fmt", UIFactory.Fmt(d.totalPoints));

            // Progreso de la campaña
            int cleared = d.campaignCompleted ? GameConfig.TotalLevels : Mathf.Clamp(d.highestUnlocked - 1, 0, GameConfig.TotalLevels);
            float pct = cleared / (float)GameConfig.TotalLevels;
            _levelText.text = L.T("menu.level_progress_fmt", d.currentLevel, GameConfig.TotalLevels);
            _levelPct.text = Mathf.RoundToInt(pct * 100f) + "%";
            int lit = Mathf.RoundToInt(pct * _segments.Count);
            for (int i = 0; i < _segments.Count; i++)
                _segments[i].color = i < lit ? Color.Lerp(new Color(0.2f, 0.6f, 1f), new Color(0.35f, 0.9f, 1f), i / (float)_segments.Count) : new Color(0.1f, 0.15f, 0.32f, 0.9f);

            bool hasLives = d.lives > 0;
            _playTitle.text = L.T(hasLives ? "menu.play_big" : "menu.no_lives_big");
            // Sin vidas: botón rojo con corazón (abre el panel de vidas)
            _playBg.color = hasLives ? new Color(1f, 0.52f, 0.1f) : new Color(0.78f, 0.16f, 0.26f);
            _playBorder.effectColor = hasLives ? new Color(1f, 0.8f, 0.35f, 0.9f) : new Color(1f, 0.55f, 0.6f, 0.9f);
            _playIcon.sprite = UIKit.Get(hasLives ? "icon_play" : "icon_heart");
            _playSub.text = PlaySubText();

            // Avisos
            int missions = Missions.ClaimableCount(d);
            _missionBadge.transform.parent.gameObject.SetActive(missions > 0);
            _missionBadge.text = missions.ToString();
            _achBadge.transform.parent.gameObject.SetActive(gm.HasAchievementReward);
            bool canClaim = gm.CanClaimDaily;
            _dailyBadge.transform.parent.gameObject.SetActive(canClaim);
            _streakDay.text = L.T("menu.streak_day_fmt", DailyStreak.CurrentDay(d));
            int chests = Chests.Total(d);
            _chests.SetActive(chests > 0);
            _chestCount.text = chests.ToString();
        }

        /// <summary>"2", o con la cuenta atrás de la recarga cuando está corriendo.</summary>
        static string LivesText()
        {
            var gm = GameManager.Instance;
            return gm.RefillRunning && gm.Data.lives < GameConfig.DefaultLives
                ? gm.Data.lives + " <size=20>" + GameManager.FormatTime(gm.RefillRemaining) + "</size>"
                : gm.Data.lives.ToString();
        }

        static string PlaySubText()
        {
            var gm = GameManager.Instance;
            if (gm.Data.lives > 0) return L.T("menu.play_sub_fmt", gm.Data.currentLevel);
            return gm.RefillRunning ? L.T("menu.no_lives_sub_fmt", GameManager.FormatTime(gm.RefillRemaining)) : L.T("menu.no_lives_sub");
        }

        void ShowSettings()
        {
            var gm = GameManager.Instance;
            var d = gm.Data;
            UIManager.Instance.IconDialog("icon_gear", L.T("menu.settings"), L.T("set.pilot_fmt", d.username, Countries.NameOf(d.countryCode)), (p, close) =>
            {
                UIManager.DialogButton(p, L.T("set.language_fmt", L.NativeName(L.Current)), Palette.Cyan,
                    UIManager.Instance.ShowLanguagePicker, close, true, "icon_globe");
                Button snd = null, vib = null;
                snd = UIManager.DialogButton(p, L.T(d.soundOn ? "set.sound_on" : "set.sound_off"), Palette.PanelLight, () =>
                { d.soundOn = !d.soundOn; gm.Save(); snd.SetLabel(L.T(d.soundOn ? "set.sound_on" : "set.sound_off")); snd.SetIcon(d.soundOn ? "icon_sound" : "icon_sound_off"); }, close, false, d.soundOn ? "icon_sound" : "icon_sound_off");
                Button mus = null;
                mus = UIManager.DialogButton(p, L.T(d.musicOn ? "set.music_on" : "set.music_off"), Palette.PanelLight, () =>
                { d.musicOn = !d.musicOn; gm.Save(); mus.SetLabel(L.T(d.musicOn ? "set.music_on" : "set.music_off")); mus.SetIcon(d.musicOn ? "icon_music" : "icon_music_off"); }, close, false, d.musicOn ? "icon_music" : "icon_music_off");
                vib = UIManager.DialogButton(p, L.T(d.vibrationOn ? "set.vib_on" : "set.vib_off"), Palette.PanelLight, () =>
                { d.vibrationOn = !d.vibrationOn; gm.Save(); vib.SetLabel(L.T(d.vibrationOn ? "set.vib_on" : "set.vib_off")); vib.SetIcon(d.vibrationOn ? "icon_vibrate" : "icon_vibrate_off"); }, close, false, d.vibrationOn ? "icon_vibrate" : "icon_vibrate_off");
                UIManager.DialogButton(p, L.T("set.rate"), Palette.Gold, PlatformServices.OpenStorePage, close, false, "icon_star");
                // Cuenta de Google: respaldo del progreso en la nube
                var acc = AccountService.Instance;
                if (acc.IsLinked)
                {
                    var linked = UIManager.DialogButton(p, L.T("acc.linked_status"), Palette.Green, null, close, false, "icon_check");
                    linked.interactable = false;
                }
                else
                {
                    var google = UIManager.DialogButton(p, L.T("acc.google_btn_backup"), Color.white,
                        () => acc.SignInWithGoogle((ok, msg) => UIManager.Instance.Toast(msg)), close);
                    google.GetComponentInChildren<Text>().color = new Color(0.2f, 0.2f, 0.25f);
                    google.SetIcon("icon_cloud", new Color(0.2f, 0.45f, 0.95f));
                }
                UIManager.DialogButton(p, L.T("set.restore"), Palette.Purple, () =>
                    IAPManager.Instance.RestorePurchases((ok, msg) => UIManager.Instance.Toast(msg)), close, true, "icon_restore");

                var legal = UIFactory.HRow(p, 110, 16);
                UIFactory.Button(legal.transform, L.T("legal.terms"), Palette.PanelLight, () => UIManager.Instance.ShowLegal("terms"), 30).SetIcon("icon_doc");
                UIFactory.Button(legal.transform, L.T("legal.privacy"), Palette.PanelLight, () => UIManager.Instance.ShowLegal("privacy"), 30).SetIcon("icon_shield");

                UIManager.DialogButton(p, L.T("acc.delete_btn"), Palette.Red, () =>
                    UIManager.Instance.Confirm(L.T("acc.delete_title"), L.T(acc.IsLinked ? "acc.delete_body_linked" : "acc.delete_body_local"),
                        L.T("acc.delete_yes"), () => acc.DeleteAccountAndData((ok, msg) => UIManager.Instance.Toast(msg))), close, true, "icon_trash");
                UIManager.DialogButton(p, L.T("close"), Palette.Grey, null, close);
            });
        }
    }
}
