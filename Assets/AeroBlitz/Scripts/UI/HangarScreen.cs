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

        protected override void Build()
        {
            UIFactory.Image(Root, Palette.Bg, false, "Bg").rectTransform.Stretch();
            _bar = TopBar.Build(Root, L.T("menu.hangar"), () => UIManager.Instance.ShowScreen<MainMenuScreen>());

            var carousel = UIFactory.Image(Root, Palette.Panel, true, "Carousel");
            carousel.rectTransform.Place(0.03f, 0.56f, 0.97f, 0.915f);
            var prev = UIFactory.Button(carousel.transform, "<", Palette.PanelLight, () => Move(-1), 70);
            prev.GetComponent<RectTransform>().Place(0, 0.55f, 0.14f, 0.85f, 16, 0, 0, 0);
            var next = UIFactory.Button(carousel.transform, ">", Palette.PanelLight, () => Move(1), 70);
            next.GetComponent<RectTransform>().Place(0.86f, 0.55f, 1f, 0.85f, 0, 0, 16, 0);
            var holder = UIFactory.Rect("Holder", carousel.transform).Place(0.2f, 0.45f, 0.8f, 0.98f);
            _shipImg = ShipUI.ShipImage(holder, null, 250);
            _shipImg.rectTransform.anchorMin = _shipImg.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _shipImg.gameObject.AddComponent<Bobbing>().amplitude = 10;
            _name = UIFactory.Label(carousel.transform, "", 50, Palette.Text, TextAnchor.MiddleCenter, true);
            _name.rectTransform.Place(0, 0.36f, 1, 0.46f);
            _desc = UIFactory.Label(carousel.transform, "", 32, Palette.TextDim);
            _desc.rectTransform.Place(0.04f, 0.29f, 0.96f, 0.36f);
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
            _name.text = def.Name + (def.priceType == PriceType.RealMoney ? "  <color=#B48CFF>" + L.T("hangar.premium") + "</color>" : "");
            _desc.text = def.Description;
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

        void Section(string title)
        {
            UIFactory.Label(_content, title, 38, Palette.Gold, TextAnchor.MiddleLeft, true).Pref(64);
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
            if (state == null)
            {
                UIFactory.Label(_content, L.T("hangar.buy_first"), 36, Palette.TextDim).Pref(120);
                return;
            }

            Section(L.T("hangar.upgrades_fmt", gm.RewardedUpgradesLeft));
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
                // Mini barra: cada video llena un tercio
                var mini = UIFactory.Bar(row, new Color(0, 0, 0, 0.5f), Palette.Green);
                mini.GetComponent<RectTransform>().Place(0.75f, 0.03f, 0.98f, 0.13f, 6, 0, 10, 0);
                mini.label.fontSize = 0;
                mini.Set(vids / (float)GameConfig.VideosPerUpgrade);
                ad.GetComponent<RectTransform>().Place(0.75f, 0.15f, 0.98f, 0.85f, 6, 0, 10, 0);
                ad.interactable = gm.RewardedUpgradesLeft > 0;
            }

            Section(L.T("hangar.basic_eq"));
            EquipmentRows(state, false);
            Section(L.T("hangar.premium_eq"));
            EquipmentRows(state, true);

            Section(L.T("hangar.paint"));
            BuildPaints(state);
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
                    : p.priceType == PriceType.Coins ? $"$ {p.coinPrice}" : IAPManager.Instance.GetPrice(p.productId);
                var b = UIFactory.Button(grid, label, p.color * 0.8f + new Color(0, 0, 0, 1), () =>
                {
                    if (d.ownedPaints.Contains(paint.id)) { state.paintId = paint.id; gm.Save(); gm.NotifyChanged(); }
                    else if (paint.priceType == PriceType.Coins) gm.BuyPaintWithCoins(paint);
                    else gm.BuyProduct(paint.productId);
                }, 30);
                if (current) b.gameObject.AddComponent<Outline>().effectDistance = new Vector2(5, 5);
            }
        }
    }
}
