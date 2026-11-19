using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AeroBlitz
{
    /// <summary>
    /// Ruta Galáctica: al pulsar JUGAR se ve el camino por los 10 mundos (de abajo hacia arriba), con tu nave en el mundo
    /// actual, los mundos completados marcados, los bloqueados apagados y los jefes señalados.
    /// Al tocar un mundo aparece su detalle y el botón para iniciar la misión (el nivel que toca jugar).
    /// </summary>
    public class GalaxyRouteScreen : UIScreen
    {
        TopBar _bar;
        RectTransform _list;
        ScrollRect _scroll;
        Text _dName, _dLevels, _dBoss, _dReward, _startLabel;
        Image _startBg;
        Outline _startBorder;
        Image _startIcon;
        System.Action _startAction;
        Image _bossIcon;
        int _selected;
        bool _scrollPending;
        readonly List<Image> _rings = new List<Image>();

        const int RowHeight = 250;

        protected override void Build()
        {
            var bg = UIFactory.Image(Root, Palette.Bg, false, "Bg");
            bg.rectTransform.Stretch();
            var art = UIKit.Get("menu_bg");
            if (art != null)
            {
                bg.sprite = art; bg.color = Color.white;
                var fit = bg.gameObject.AddComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio = art.rect.width / art.rect.height;
            }
            _bar = TopBar.Build(Root, L.T("route.title"), () => UIManager.Instance.ShowScreen<MainMenuScreen>());

            // Camino con scroll
            var area = UIFactory.Image(Root, new Color(0, 0, 0, 0.18f), true, "Map");
            area.rectTransform.Place(0.02f, 0.335f, 0.98f, 0.915f);
            _list = UIFactory.ScrollList(area.transform, out _scroll, 0, 6);
            ((RectTransform)_scroll.transform).Stretch();

            // Detalle del mundo elegido
            var panel = UIKit.Panel(Root, UIKit.PanelDark, UIKit.Border, "Detail");
            panel.rectTransform.Place(0.04f, 0.115f, 0.96f, 0.325f);
            _dName = UIFactory.Label(panel.transform, "", 42, Palette.Gold, TextAnchor.MiddleLeft, true);
            _dName.rectTransform.Place(0.04f, 0.72f, 0.96f, 0.97f);
            Fit(_dName, 22);
            _dLevels = UIFactory.Label(panel.transform, "", 30, Palette.Text, TextAnchor.MiddleLeft);
            _dLevels.rectTransform.Place(0.04f, 0.5f, 0.96f, 0.72f);
            Fit(_dLevels, 16);
            _bossIcon = UIKit.Icon(panel.transform, "icon_crown");
            _bossIcon.rectTransform.Place(0.04f, 0.27f, 0.12f, 0.5f);
            _dBoss = UIFactory.Label(panel.transform, "", 30, new Color(1f, 0.62f, 0.3f), TextAnchor.MiddleLeft, true);
            _dBoss.rectTransform.Place(0.13f, 0.27f, 0.96f, 0.5f);
            Fit(_dBoss, 16);
            _dReward = UIFactory.Label(panel.transform, "", 28, Palette.TextDim, TextAnchor.MiddleLeft);
            _dReward.rectTransform.Place(0.04f, 0.04f, 0.96f, 0.27f);
            Fit(_dReward, 15);

            // Iniciar misión
            var start = UIKit.Tile(Root, new Color(1f, 0.52f, 0.1f), new Color(1f, 0.8f, 0.35f, 0.9f), () => _startAction?.Invoke());
            start.GetComponent<RectTransform>().Place(0.06f, 0.02f, 0.94f, 0.105f);
            _startBg = start.GetComponent<Image>();
            _startBorder = start.GetComponent<Outline>();
            _startIcon = UIKit.Icon(start.transform, "icon_play");
            _startIcon.rectTransform.Place(0.08f, 0.2f, 0.2f, 0.8f);
            _startLabel = UIFactory.Label(start.transform, "", 48, Color.white, TextAnchor.MiddleCenter, true);
            _startLabel.rectTransform.Place(0.2f, 0, 0.97f, 1);
            Fit(_startLabel, 24);
            _startLabel.gameObject.AddComponent<Shadow>().effectColor = new Color(0.45f, 0.18f, 0f, 0.8f);
        }

        static void Fit(Text t, int min)
        {
            t.resizeTextForBestFit = true; t.resizeTextMinSize = min; t.resizeTextMaxSize = t.fontSize;
            t.verticalOverflow = VerticalWrapMode.Truncate;
        }

        public override void OnShow()
        {
            var d = GameManager.Instance.Data;
            _selected = WorldCatalog.WorldForLevel(d.currentLevel);
            Rebuild();
            _scrollPending = true;
        }

        public override void Refresh()
        {
            if (_bar != null) _bar.Refresh(GameManager.Instance.Data);
            if (_list != null && _list.childCount > 0) Rebuild();
        }

        void LateUpdate()
        {
            if (!_scrollPending) return;
            _scrollPending = false;
            StartCoroutine(ScrollToSelected());
        }

        IEnumerator ScrollToSelected()
        {
            yield return null; // espera a que se calcule el tamaño de la lista
            LayoutRebuilder.ForceRebuildLayoutImmediate(_list);
            int worlds = WorldCatalog.Worlds.Length;
            int fromTop = worlds - 1 - _selected; // el mundo 1 está abajo del todo
            float content = _list.rect.height, view = ((RectTransform)_scroll.viewport).rect.height;
            if (content <= view) { _scroll.verticalNormalizedPosition = 1f; yield break; }
            float center = (fromTop + 0.5f) * RowHeight + 6f;               // distancia desde arriba al centro del mundo
            float top = Mathf.Clamp(center - view / 2f, 0f, content - view);  // arriba de la ventana
            _scroll.verticalNormalizedPosition = 1f - top / (content - view);
        }

        // ---------------- LISTA DE MUNDOS ----------------
        void Rebuild()
        {
            var d = GameManager.Instance.Data;
            int cleared = d.campaignCompleted ? GameConfig.TotalLevels : Mathf.Clamp(d.highestUnlocked - 1, 0, GameConfig.TotalLevels);
            int currentWorld = WorldCatalog.WorldForLevel(d.currentLevel);
            UIFactory.Clear(_list);
            _rings.Clear();
            // El último mundo arriba y el primero abajo: se avanza hacia arriba
            for (int w = WorldCatalog.Worlds.Length - 1; w >= 0; w--) BuildWorld(w, cleared, currentWorld, d);
            ShowDetail();
        }

        void BuildWorld(int w, int cleared, int currentWorld, PlayerData d)
        {
            var theme = WorldCatalog.Worlds[w];
            int first = w * WorldCatalog.LevelsPerWorld + 1, last = first + WorldCatalog.LevelsPerWorld - 1;
            bool done = cleared >= last;
            bool locked = !d.campaignCompleted && first > d.highestUnlocked;
            bool current = w == currentWorld && !d.campaignCompleted;
            bool left = w % 2 == 0;
            float px = left ? 0.3f : 0.7f;
            Color planetCol = theme.planet ? theme.planetColor : Color.Lerp(theme.nebulaA, Color.white, 0.35f);
            Color glowCol = Color.Lerp(theme.nebulaA, theme.nebulaB, 0.4f);
            int index = w;

            var row = UIFactory.Rect("World" + (w + 1), _list);
            row.Pref(RowHeight);

            // Ruta luminosa que une los mundos
            var line = UIFactory.Image(row, locked ? new Color(0.3f, 0.4f, 0.65f, 0.25f) : new Color(0.45f, 0.8f, 1f, done ? 0.85f : 0.5f), false, "Route");
            line.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            line.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            line.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            line.rectTransform.sizeDelta = new Vector2(8, 0);

            var glow = UIFactory.Image(row, new Color(glowCol.r, glowCol.g, glowCol.b, locked ? 0.12f : 0.5f), false, "Glow");
            glow.sprite = SpriteFactory.Glow;
            Center(glow.rectTransform, px, 300);

            // Anillo de selección
            var ring = UIFactory.Image(row, new Color(1f, 0.85f, 0.3f, 0f), false, "Ring");
            ring.sprite = SpriteFactory.Ring;
            Center(ring.rectTransform, px, 210);
            _rings.Add(ring);

            var planet = UIFactory.Image(row, locked ? new Color(0.28f, 0.3f, 0.4f) : planetCol, false, "Planet");
            planet.sprite = SpriteFactory.Planet;
            planet.raycastTarget = true;
            Center(planet.rectTransform, px, 160);
            var btn = planet.gameObject.AddComponent<Button>();
            btn.targetGraphic = planet;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => { _selected = index; AudioFX.Instance?.Play(AudioFX.Sfx.Click); ShowDetail(); });

            if (locked) Mark(row, "icon_lock", px, 84, Color.white);
            else if (done) Mark(row, "icon_check", px + (left ? 0.075f : -0.075f), 60, new Color(0.45f, 1f, 0.55f), 38f);

            // Tu nave sobre el mundo actual
            if (current)
            {
                var ship = UIKit.Icon(row, "icon_rocket");
                Center(ship.rectTransform, px + (left ? -0.115f : 0.115f), 86);
                ship.rectTransform.anchoredPosition = new Vector2(0, 40);
                ship.gameObject.AddComponent<Bobbing>().amplitude = 8;
            }

            // Texto del mundo al lado opuesto
            float tx0 = left ? 0.55f : 0.03f, tx1 = left ? 0.97f : 0.45f; // el texto no pisa la línea de la ruta
            var name = UIFactory.Label(row, theme.Name, 36, locked ? Palette.TextDim : Palette.Text, left ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight, true);
            name.rectTransform.Place(tx0, 0.55f, tx1, 0.85f);
            Fit(name, 18);
            int worldBoss = 0;
            for (int l = first; l <= last; l++) if (BossCatalog.IsBossLevel(l)) worldBoss = l;
            bool bossPending = worldBoss > 0 && BossProgress.IsPending(d, worldBoss);
            string state = locked ? L.T("route.locked") : bossPending ? L.T("route.boss_pending") : done ? L.T("route.done") : current ? L.T("route.current") : "";
            var sub = UIFactory.Label(row, $"{L.T("route.world_fmt", w + 1)}" + (state.Length > 0 ? "  ·  " + state : ""), 26,
                bossPending ? new Color(1f, 0.55f, 0.3f) : done ? new Color(0.5f, 1f, 0.6f) : current ? Palette.Gold : Palette.TextDim, left ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight);
            sub.rectTransform.Place(tx0, 0.38f, tx1, 0.56f);
            Fit(sub, 14);

            // Un punto por nivel: azul = superado, dorado = el que toca, naranja grande = jefe
            var dots = UIFactory.Rect("Levels", row).Place(tx0, 0.12f, tx1, 0.37f);
            var h = dots.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 10;
            h.childAlignment = left ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            h.childControlWidth = h.childControlHeight = false;
            h.childForceExpandWidth = h.childForceExpandHeight = false;
            for (int l = first; l <= last; l++)
            {
                bool boss = BossCatalog.IsBossLevel(l);
                float size = boss ? 42f : 28f;
                Color c = l <= cleared ? new Color(0.35f, 0.85f, 1f)
                        : l == d.currentLevel && !d.campaignCompleted ? Palette.Gold
                        : locked ? new Color(0.15f, 0.2f, 0.35f, 0.7f) : new Color(0.18f, 0.28f, 0.5f, 0.9f);
                if (boss) c = BossProgress.IsBeaten(d, l) ? new Color(1f, 0.65f, 0.25f) : new Color(0.85f, 0.3f, 0.2f, locked ? 0.55f : 1f);
                var dot = UIFactory.Image(dots, c, false, boss ? "Boss" : "Level");
                dot.sprite = SpriteFactory.Circle;
                dot.rectTransform.sizeDelta = new Vector2(size, size);
                if (boss) UIKit.Icon(dot.transform, "icon_crown").rectTransform.Stretch(7);
            }
        }

        static void Center(RectTransform rt, float x, float size)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(x, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(size, size);
        }

        static void Mark(RectTransform row, string icon, float x, float size, Color tint, float y = 0f)
        {
            var img = UIKit.Icon(row, icon, tint);
            Center(img.rectTransform, x, size);
            img.rectTransform.anchoredPosition = new Vector2(0, y);
        }

        // ---------------- DETALLE ----------------
        void ShowDetail()
        {
            var d = GameManager.Instance.Data;
            int w = Mathf.Clamp(_selected, 0, WorldCatalog.Worlds.Length - 1);
            var theme = WorldCatalog.Worlds[w];
            int first = w * WorldCatalog.LevelsPerWorld + 1, last = first + WorldCatalog.LevelsPerWorld - 1;
            int cleared = d.campaignCompleted ? GameConfig.TotalLevels : Mathf.Clamp(d.highestUnlocked - 1, 0, GameConfig.TotalLevels);
            int doneHere = Mathf.Clamp(cleared - (first - 1), 0, WorldCatalog.LevelsPerWorld);

            for (int i = 0; i < _rings.Count; i++)
            {
                int world = WorldCatalog.Worlds.Length - 1 - i; // las filas están de arriba abajo
                var c = _rings[i].color; c.a = world == w ? 0.95f : 0f; _rings[i].color = c;
            }

            _dName.text = $"{L.T("route.world_fmt", w + 1)}  ·  {theme.Name}";
            _dLevels.text = L.T("route.levels_fmt", first, last) + "   ·   " + L.T("route.progress_fmt", doneHere, WorldCatalog.LevelsPerWorld);

            int bossLevel = 0;
            for (int l = first; l <= last; l++) if (BossCatalog.IsBossLevel(l)) bossLevel = l;
            if (bossLevel > 0)
            {
                var boss = BossCatalog.ForLevel(bossLevel);
                bool beaten = BossProgress.IsBeaten(d, bossLevel);
                bool pending = BossProgress.IsPending(d, bossLevel);
                _bossIcon.gameObject.SetActive(true);
                _dBoss.text = L.T("route.boss_fmt", boss.Name, bossLevel) + (beaten ? "  ·  " + L.T("route.done") : pending ? "  ·  " + L.T("route.boss_pending") : "");
                _dReward.text = beaten ? L.T("route.boss_beaten") : L.T("route.reward_fmt", GameConfig.BossBonusCoinsPerTier * (bossLevel / 10));
            }
            else
            {
                _bossIcon.gameObject.SetActive(false);
                _dBoss.text = L.T("route.no_boss");
                _dReward.text = L.T("route.next_boss_hint");
            }

            // Botón principal: reto del jefe pendiente del mundo elegido, o la misión que toca jugar
            bool hasLives = d.lives > 0;
            int challengeLevel = 0;
            for (int l = first; l <= last; l++) if (BossProgress.IsPending(d, l)) challengeLevel = l;
            if (challengeLevel > 0)
            {
                _startBg.color = new Color(0.78f, 0.16f, 0.26f);
                _startBorder.effectColor = new Color(1f, 0.55f, 0.6f, 0.9f);
                _startLabel.text = hasLives ? L.T("route.challenge_fmt", BossCatalog.ForLevel(challengeLevel).Name) : L.T("menu.no_lives_big");
                int lv = challengeLevel;
                SpriteFactory.PrepareBoss(BossCatalog.ForLevel(lv));
                _startAction = () => GameManager.Instance.PlayLevel(lv);
            }
            else
            {
                _startBg.color = new Color(1f, 0.52f, 0.1f);
                _startBorder.effectColor = new Color(1f, 0.8f, 0.35f, 0.9f);
                _startLabel.text = hasLives ? L.T("route.start_fmt", d.currentLevel) : L.T("menu.no_lives_big");
                _startAction = () => GameManager.Instance.PlayCurrentLevel();
            }
        }
    }
}
