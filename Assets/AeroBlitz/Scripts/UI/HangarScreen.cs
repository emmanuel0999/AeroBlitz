using UnityEngine;
using UnityEngine.UI;

namespace AeroBlitz
{
    /// <summary>Hangar: ver todas las naves, seleccionar, comprar, mejorar, equipar y pintar.</summary>
    public class HangarScreen : UIScreen
    {
        TopBar _bar;
        int _index;
        Image _shipImg;
        Text _name, _desc, _stats;
        Button _action;
        RectTransform _content;
        Outline _goldFrame;

        protected override void Build()
        {
            UIFactory.Image(Root, Palette.Bg, false, "Bg").rectTransform.Stretch();
            _bar = TopBar.Build(Root, L.T("menu.hangar"), () => UIManager.Instance.ShowScreen<MainMenuScreen>());

            var carousel = UIFactory.Image(Root, Palette.Panel, true, "Carousel");
            carousel.rectTransform.Place(0.03f, 0.56f, 0.97f, 0.915f);
            _goldFrame = carousel.gameObject.AddComponent<Outline>(); // marco dorado para las naves premium
            _goldFrame.effectColor = new Color(1f, 0.8f, 0.25f, 0.95f);
            _goldFrame.effectDistance = new Vector2(7, 7);
            _goldFrame.enabled = false;
            var prev = UIFactory.Button(carousel.transform, "", Palette.PanelLight, () => Move(-1), 70);
            UIKit.Icon(prev.transform, "icon_chevron_left").rectTransform.Stretch(18);
            prev.GetComponent<RectTransform>().Place(0, 0.55f, 0.14f, 0.85f, 16, 0, 0, 0);
            var next = UIFactory.Button(carousel.transform, "", Palette.PanelLight, () => Move(1), 70);
            UIKit.Icon(next.transform, "icon_chevron_right").rectTransform.Stretch(18);
            next.GetComponent<RectTransform>().Place(0.86f, 0.55f, 1f, 0.85f, 0, 0, 16, 0);
            var holder = UIFactory.Rect("Holder", carousel.transform).Place(0.2f, 0.45f, 0.8f, 0.98f);
            _shipImg = ShipUI.ShipImage(holder, null, 250);
            _shipImg.rectTransform.anchorMin = _shipImg.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _shipImg.gameObject.AddComponent<Bobbing>().amplitude = 10;
            _name = UIFactory.Label(carousel.transform, "", 50, Palette.Text, TextAnchor.MiddleCenter, true);
            _name.rectTransform.Place(0, 0.36f, 1, 0.46f);
            _desc = UIFactory.Label(carousel.transform, "", 32, Palette.TextDim);
            _desc.rectTransform.Place(0.04f, 0.29f, 0.96f, 0.36f);
            _desc.gameObject.AddComponent<LiveText>().source = DescText; // cuenta atrás si es la nave de prueba
            _stats = UIFactory.Label(carousel.transform, "", 30, Palette.Cyan);
            _stats.rectTransform.Place(0.04f, 0.16f, 0.96f, 0.29f);
            _action = UIFactory.Button(carousel.transform, "", Palette.Orange, OnAction, 42);
            _action.GetComponent<RectTransform>().Place(0.15f, 0.025f, 0.85f, 0.15f);

            var listBg = UIFactory.Image(Root, new Color(0, 0, 0, 0.2f), true, "ListBg");
            listBg.rectTransform.Place(0.03f, 0.02f, 0.97f, 0.55f);
            _content = UIFactory.ScrollList(listBg.transform, out var scroll, 14, 16);
            ((RectTransform)scroll.transform).Stretch();
        }

        public override void OnShow()
        {
            AdsManager.Instance.HideBanner();
            var d = GameManager.Instance.Data;
            _index = Mathf.Max(0, ShipCatalog.All.FindIndex(s => s.id == d.selectedShip));
        }

        void Move(int dir)
        {
            _index = (_index + dir + ShipCatalog.All.Count) % ShipCatalog.All.Count;
            Refresh();
        }

        ShipDefinition Current => ShipCatalog.All[_index];

        string DescText()
        {
            var d = GameManager.Instance.Data;
            return d.IsTempShip(Current.id)
                ? "<color=#7CFFB2>" + L.T("hangar.temp_fmt", GameManager.FormatTime(DailyStreak.TempShipRemaining(d))) + "</color>"
                : Current.Description;
        }

        void OnAction()
        {
            var gm = GameManager.Instance;
            var def = Current;
            if (gm.Data.OwnsShip(def.id))
            {
                gm.Data.selectedShip = def.id;
                gm.Save(); gm.NotifyChanged();
            }
            else if (def.priceType == PriceType.Coins) gm.BuyShipWithCoins(def);
            else if (def.priceType == PriceType.RealMoney) gm.BuyProduct(def.productId);
        }

        public override void Refresh()
        {
            var gm = GameManager.Instance;
            var d = gm.Data;
            _bar.Refresh(d);
            var def = Current;
            var state = d.GetShip(def.id);
            bool owned = state != null;
            var preview = state ?? new ShipState { shipId = def.id };
            var st = ShipStats.Compute(preview);

            ShipUI.UpdateShipImage(_shipImg, owned ? state : null, def);
            _goldFrame.enabled = def.IsPremium;
            _name.text = def.Name + (def.priceType == PriceType.RealMoney ? "  <color=#B48CFF>" + L.T("hangar.premium") + "</color>" : "");
            _desc.text = DescText();
            _stats.text = L.T("hangar.stats_fmt", st.shipLevel, Mathf.RoundToInt(st.maxHealth), st.speed, st.shots, st.chargeTime, st.fireWindow);

            if (owned)
            {
                bool sel = d.selectedShip == def.id;
                _action.SetLabel(L.T(sel ? "hangar.selected" : "hangar.select"));
                _action.SetColor(sel ? Palette.Green : Palette.Cyan);
                _action.interactable = !sel;
            }
            else
            {
                _action.interactable = true;
                _action.SetLabel(L.T("hangar.buy_fmt", def.priceType == PriceType.Coins ? "$ " + UIFactory.Fmt(def.coinPrice) : IAPManager.Instance.GetPrice(def.productId)));
                _action.SetColor(def.priceType == PriceType.Coins ? Palette.Orange : Palette.Purple);
            }

            BuildList(state);
        }

        void Section(string title, string icon = null)
        {
            if (icon == null) { UIFactory.Label(_content, title, 38, Palette.Gold, TextAnchor.MiddleLeft, true).Pref(64); return; }
            var row = UIFactory.Rect("Section", _content);
            row.Pref(64);
            var t = UIFactory.Label(row, title, 38, Palette.Gold, TextAnchor.MiddleLeft, true);
            t.rectTransform.Stretch();
            UIKit.LabelIcon(t, icon, 48);
        }

        RectTransform Row(float h = 150)
        {
            var bg = UIFactory.Image(_content, Palette.Panel, true, "Row");
            bg.Pref(h);
            return bg.rectTransform;
        }

        void BuildList(ShipState state)
        {
            UIFactory.Clear(_content);
            var gm = GameManager.Instance;
            var d = gm.Data;
            // Premium: sus habilidades se ven siempre, también antes de comprarla
            if (Current.IsPremium) BuildAbilities(Current);
            if (state == null)
            {
                UIFactory.Label(_content, L.T("hangar.buy_first"), 36, Palette.TextDim).Pref(120);
                return;
            }

            // Nave de prueba de la racha: se puede usar y equipar, pero no mejorar (se perdería al terminar la prueba)
            if (d.IsTempShip(state.shipId))
            {
                var info = Row(230);
                var t = UIFactory.Label(info, L.T("hangar.temp_info"), 32, Palette.Text, TextAnchor.MiddleLeft);
                t.rectTransform.Place(0, 0, 0.62f, 1, 28, 0, 0, 0);
                var def = ShipCatalog.Get(state.shipId);
                var buy = UIFactory.Button(info, L.T("hangar.buy_fmt", IAPManager.Instance.GetPrice(def.productId)), Palette.Purple, () => gm.BuyProduct(def.productId), 32);
                buy.GetComponent<RectTransform>().Place(0.64f, 0.2f, 0.98f, 0.8f, 6, 0, 10, 0);
            }
            else
            {
                BuildUpgrades(state);
            }

            Section(L.T("hangar.basic_eq"), "icon_bolt");
            EquipmentRows(state, false);
            Section(L.T("hangar.premium_eq"), "icon_crown");
            EquipmentRows(state, true);

            Section(L.T("hangar.skin"), "icon_star");
            BuildSkins(state);

            Section(L.T("hangar.paint"), "icon_brush");
            BuildPaints(state);

            Section(L.T("hangar.trail"), "icon_flame");
            BuildTrails(state);
        }

        /// <summary>Habilidad pasiva y habilidad con botón de una nave premium.</summary>
        void BuildAbilities(ShipDefinition def)
        {
            Section("<color=#FFD24A>" + L.T("hangar.abilities") + "</color>", "icon_crown");
            foreach (var (key, kind) in new[] { ("ab." + def.passive.ToString().ToLower(), L.T("ab.passive")), ("ab." + def.active.ToString().ToLower(), L.T("ab.active")) })
            {
                var row = Row(140);
                var t = UIFactory.Label(row, $"<b>{L.T(key)}</b>  <size=26><color=#FFD24A>{kind}</color></size>\n<size=28><color=#A9B7D6>{L.T(key + ".desc")}</color></size>",
                    34, Palette.Text, TextAnchor.MiddleLeft);
                t.rectTransform.Place(0, 0, 1, 1, 28, 0, 20, 0);
            }
        }

        /// <summary>Estilos de la nave: cartoon y neón para todas, metálico solo premium. Se compran con monedas.</summary>
        void BuildSkins(ShipState state)
        {
            var gm = GameManager.Instance;
            var d = gm.Data;
            var def = ShipCatalog.Get(state.shipId);
            var grid = UIFactory.Rect("Skins", _content);
            var gl = grid.gameObject.AddComponent<GridLayoutGroup>();
            gl.cellSize = new Vector2(240, 200);
            gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gl.constraintCount = 3;
            gl.spacing = new Vector2(14, 14);
            gl.childAlignment = TextAnchor.UpperCenter;
            grid.Pref(214);
            var current = ShipSkins.Current(state, def);
            foreach (var sk in ShipSkins.All)
            {
                var skin = sk;
                bool available = ShipSkins.Available(def, skin);
                bool owned = ShipSkins.Owns(state, def, skin);
                bool inUse = owned && current == skin;
                string label = !available ? L.T("hangar.skin_premium_only")
                    : inUse ? L.T("hangar.in_use")
                    : owned ? ShipSkins.Name(skin)
                    : "$ " + UIFactory.Fmt(ShipSkins.Price(def, skin));
                var b = UIFactory.Button(grid, label, !available ? new Color(0.18f, 0.18f, 0.24f) : owned ? Palette.PanelLight : Palette.Orange, () =>
                {
                    if (!ShipSkins.Available(def, skin)) { UIManager.Instance.Toast(L.T("toast.metal_premium_only")); return; }
                    gm.BuySkinWithCoins(state, skin);
                }, 28);
                b.interactable = available && !inUse && (owned || d.coins >= ShipSkins.Price(def, skin));
                if (!available) b.interactable = true; // permite tocarla para ver el aviso
                // Vista previa de la nave con ese estilo
                var look = ShipLook.Of(def, state, skin);
                var mini = UIFactory.Image(b.transform, look.hullColor, false, "Preview");
                mini.sprite = look.hull;
                mini.preserveAspect = true;
                mini.rectTransform.Place(0.2f, 0.36f, 0.8f, 0.97f);
                var det = UIFactory.Image(mini.transform, look.detailColor, false, "Detail");
                det.sprite = look.detail;
                det.preserveAspect = true;
                det.rectTransform.Stretch();
                if (!available) { mini.color = new Color(0.3f, 0.3f, 0.35f); det.color = new Color(0.4f, 0.4f, 0.45f, 0.6f); }
                var name = UIFactory.Label(b.transform, ShipSkins.Name(skin), 24, Palette.TextDim, TextAnchor.MiddleCenter, true);
                name.rectTransform.Place(0, 0.2f, 1, 0.38f);
                b.GetComponentInChildren<Text>().rectTransform.Place(0, 0, 1, 0.22f, 6, 4, 6, 0);
                b.RefreshIcon(); // la moneda del precio se recoloca junto al texto
                if (inUse) b.gameObject.AddComponent<Outline>().effectDistance = new Vector2(5, 5);
            }
        }

        /// <summary>Estelas del motor: las que tiene se equipan; las demás indican de qué cofre salen.</summary>
        void BuildTrails(ShipState state)
        {
            var gm = GameManager.Instance;
            var d = gm.Data;
            if (d.ownedTrails == null) d.ownedTrails = new System.Collections.Generic.List<string> { "default" };
            var grid = UIFactory.Rect("Trails", _content);
            var gl = grid.gameObject.AddComponent<GridLayoutGroup>();
            gl.cellSize = new Vector2(240, 140);
            gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gl.constraintCount = 3;
            gl.spacing = new Vector2(14, 14);
            gl.childAlignment = TextAnchor.UpperCenter;
            grid.Pref(Mathf.CeilToInt(UpgradeCatalog.Trails.Count / 3f) * 154);
            foreach (var tr in UpgradeCatalog.Trails)
            {
                var trail = tr;
                bool owned = trail.id == "default" || d.ownedTrails.Contains(trail.id);
                bool current = (string.IsNullOrEmpty(state.trailId) ? "default" : state.trailId) == trail.id;
                string label = current ? L.T("hangar.in_use") : owned ? trail.Name : Chests.Name(trail.rare ? ChestTier.Epic : ChestTier.Rare);
                Color bg = trail.id == "default" ? new Color(0.85f, 0.45f, 0.15f) : trail.color * 0.75f + new Color(0, 0, 0, 1);
                var b = UIFactory.Button(grid, label, owned ? bg : bg * 0.45f + new Color(0, 0, 0, 1), () =>
                {
                    if (trail.id == "default" || d.ownedTrails.Contains(trail.id)) { state.trailId = trail.id; gm.Save(); gm.NotifyChanged(); }
                    else UIManager.Instance.Toast(L.T("toast.trail_hint_fmt", Chests.Name(trail.rare ? ChestTier.Epic : ChestTier.Rare)));
                }, 28);
                // Llama de muestra con el color de la estela
                var flame = UIFactory.Image(b.transform, trail.id == "default" ? Color.white : trail.color, false, "Flame");
                flame.sprite = trail.id == "default" ? SpriteFactory.Flame : SpriteFactory.FlameMask;
                flame.preserveAspect = true;
                flame.rectTransform.Place(0.38f, 0.52f, 0.62f, 0.95f);
                b.GetComponentInChildren<Text>().rectTransform.Place(0, 0, 1, 0.52f, 6, 4, 6, 0);
                if (current) b.gameObject.AddComponent<Outline>().effectDistance = new Vector2(5, 5);
            }
        }

        void BuildUpgrades(ShipState state)
        {
            var gm = GameManager.Instance;
            var d = gm.Data;
            Section(L.T("hangar.upgrades_fmt", gm.RewardedUpgradesLeft), "icon_up");
            foreach (var up in UpgradeCatalog.Stats)
            {
                var stat = up.stat;
                int lvl = state.GetUpgrade(stat);
                var row = Row();
                var t = UIFactory.Label(row, $"<b>{up.Name}</b>  {L.T("hangar.lv_fmt", lvl, up.MaxLevel)}\n<size=28><color=#A9B7D6>{up.Description}</color></size>", 36, Palette.Text, TextAnchor.MiddleLeft);
                t.rectTransform.Place(0, 0, 0.5f, 1, 28, 0, 0, 0);
                if (lvl >= up.MaxLevel)
                {
                    var max = UIFactory.Label(row, L.T("hangar.max"), 36, Palette.Green, TextAnchor.MiddleCenter, true);
                    max.rectTransform.Place(0.5f, 0, 1, 1);
                    continue;
                }
                var buy = UIFactory.Button(row, "$ " + UIFactory.Fmt(up.coinCosts[lvl]), Palette.Orange, () => gm.BuyUpgradeWithCoins(state, stat), 34);
                buy.GetComponent<RectTransform>().Place(0.5f, 0.15f, 0.74f, 0.85f, 6, 0, 6, 0);
                buy.interactable = d.coins >= up.coinCosts[lvl];
                int vids = state.GetVideoProgress(stat);
                var ad = UIFactory.Button(row, L.T("hangar.video_fmt", vids, GameConfig.VideosPerUpgrade), Palette.Green, () => gm.WatchAdForUpgrade(state, stat), 28);
                ad.SetIcon("icon_video");
                // Mini barra: cada video llena un tercio
                // (separada del botón y con fondo claro para que se lea como barra y no como una sombra)
                var mini = UIFactory.Bar(row, new Color(1f, 1f, 1f, 0.16f), new Color(0.45f, 1f, 0.55f));
                mini.GetComponent<RectTransform>().Place(0.77f, 0.05f, 0.96f, 0.11f, 6, 0, 10, 0);
                mini.label.fontSize = 0;
                mini.Set(vids / (float)GameConfig.VideosPerUpgrade);
                ad.GetComponent<RectTransform>().Place(0.75f, 0.2f, 0.98f, 0.88f, 6, 0, 10, 0);
                ad.interactable = gm.RewardedUpgradesLeft > 0;
            }
        }

        void EquipmentRows(ShipState state, bool premium)
        {
            var gm = GameManager.Instance;
            var d = gm.Data;
            foreach (var eq in UpgradeCatalog.Equipment)
            {
                if (eq.IsCoin == premium) continue;
                var def = eq;
                var id = eq.id;
                var row = Row();
                var icon = UIFactory.Image(row, Color.white, false, "Icon");
                icon.sprite = SpriteFactory.Icon("eq:" + eq.id);
                icon.preserveAspect = true;
                icon.rectTransform.Place(0.01f, 0.1f, 0.15f, 0.9f);
                var t = UIFactory.Label(row, $"<b>{eq.Name}</b>\n<size=28><color=#A9B7D6>{eq.Description}</color></size>", 34, Palette.Text, TextAnchor.MiddleLeft);
                t.rectTransform.Place(0.15f, 0, 0.64f, 1, 16, 0, 0, 0);
                Button b;
                if (d.ownedEquipment.Contains(id))
                {
                    bool on = state.equipped.Contains(id);
                    b = UIFactory.Button(row, L.T(on ? "hangar.unequip" : "hangar.equip"), on ? Palette.Grey : Palette.Cyan, () =>
                    {
                        if (state.equipped.Contains(id)) state.equipped.Remove(id); else state.equipped.Add(id);
                        gm.Save(); gm.NotifyChanged();
                    }, 34);
                }
                else if (def.IsCoin)
                {
                    b = UIFactory.Button(row, "$ " + UIFactory.Fmt(def.coinPrice), Palette.Orange, () => gm.BuyEquipmentWithCoins(def, state), 34);
                    b.interactable = d.coins >= def.coinPrice;
                }
                else b = UIFactory.Button(row, IAPManager.Instance.GetPrice(eq.productId), Palette.Purple, () => gm.BuyProduct(eq.productId), 34);
                b.GetComponent<RectTransform>().Place(0.65f, 0.15f, 0.98f, 0.85f, 6, 0, 10, 0);

                // Arma premium de prueba del cofre épico: cuenta atrás (ya está activa en todas las naves)
                if (!d.ownedEquipment.Contains(id) && d.tempEquipId == id && Chests.TempEquipActive(d))
                {
                    t.rectTransform.Place(0.15f, 0.3f, 0.64f, 1, 16, 0, 0, 0);
                    var timer = UIFactory.Label(row, "", 26, new Color(0.49f, 1f, 0.7f), TextAnchor.MiddleLeft, true);
                    timer.rectTransform.Place(0.15f, 0, 0.64f, 0.32f, 16, 4, 0, 0);
                    timer.gameObject.AddComponent<LiveText>().source = () =>
                        L.T("hangar.temp_eq_fmt", GameManager.FormatTime(Chests.TempEquipRemaining(d)));
                }
            }
        }

        void BuildPaints(ShipState state)
        {
            var gm = GameManager.Instance;
            var d = gm.Data;
            var grid = UIFactory.Rect("Paints", _content);
            var gl = grid.gameObject.AddComponent<GridLayoutGroup>();
            gl.cellSize = new Vector2(240, 140);
            gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gl.constraintCount = 3;
            gl.spacing = new Vector2(14, 14);
            gl.childAlignment = TextAnchor.UpperCenter;
            grid.Pref(Mathf.CeilToInt(UpgradeCatalog.Paints.Count / 3f) * 154);
            foreach (var p in UpgradeCatalog.Paints)
            {
                var paint = p;
                bool ownedPaint = d.ownedPaints.Contains(p.id);
                bool current = state.paintId == p.id;
                string label = current ? L.T("hangar.in_use") : ownedPaint ? p.Name
                    : p.priceType == PriceType.Coins ? $"$ {p.coinPrice}"
                    : p.priceType == PriceType.Mission ? L.T("hangar.weekly_paint")
                    : p.priceType == PriceType.Streak ? L.T("hangar.streak_paint")
                    : IAPManager.Instance.GetPrice(p.productId);
                var b = UIFactory.Button(grid, label, p.color * 0.8f + new Color(0, 0, 0, 1), () =>
                {
                    if (d.ownedPaints.Contains(paint.id)) { state.paintId = paint.id; gm.Save(); gm.NotifyChanged(); }
                    else if (paint.priceType == PriceType.Coins) gm.BuyPaintWithCoins(paint);
                    else if (paint.priceType == PriceType.Mission) UIManager.Instance.Toast(L.T("toast.weekly_paint_hint"));
                    else if (paint.priceType == PriceType.Streak) UIManager.Instance.Toast(L.T("toast.streak_paint_hint"));
                    else gm.BuyProduct(paint.productId);
                }, 30);
                if (current) b.gameObject.AddComponent<Outline>().effectDistance = new Vector2(5, 5);
            }
        }
    }
}
