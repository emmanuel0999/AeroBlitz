using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AeroBlitz
{
    /// <summary>
    /// Tienda: artículos con dinero real (ofertas, vidas, monedas, naves y equipo premium),
    /// artículos sencillos con monedas del juego y recompensas gratis por video.
    /// </summary>
    public class StoreScreen : UIScreen
    {
        public const string TabFree = "free";
        public const string TabCoinShop = "coinshop";

        static readonly string[] Tabs =
        {
            ProductCatalog.CatOffers, ProductCatalog.CatLives, ProductCatalog.CatCoins, TabFree,
            ProductCatalog.CatShips, ProductCatalog.CatEquipment, TabCoinShop
        };

        TopBar _bar;
        string _tab = ProductCatalog.CatOffers;
        readonly Dictionary<string, Button> _tabButtons = new Dictionary<string, Button>();
        RectTransform _content;
        ScrollRect _scroll;

        protected override void Build()
        {
            UIFactory.Image(Root, Palette.Bg, false, "Bg").rectTransform.Stretch();
            _bar = TopBar.Build(Root, L.T("menu.store"), () => UIManager.Instance.ShowScreen<MainMenuScreen>());

            var tabs = UIFactory.Rect("Tabs", Root).Place(0.02f, 0.8f, 0.98f, 0.92f, 0, 0, 0, 10);
            var grid = tabs.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            grid.cellSize = new Vector2(205, 95);
            grid.spacing = new Vector2(10, 10);
            grid.childAlignment = TextAnchor.MiddleCenter;
            foreach (var t in Tabs)
            {
                var tab = t;
                _tabButtons[t] = UIFactory.Button(tabs, L.T("tab." + t), Palette.PanelLight, () => SelectTab(tab), 30);
            }

            var listBg = UIFactory.Image(Root, new Color(0, 0, 0, 0.2f), true, "ListBg");
            listBg.rectTransform.Place(0.03f, 0.1f, 0.97f, 0.795f);
            _content = UIFactory.ScrollList(listBg.transform, out _scroll, 14, 16);
            ((RectTransform)_scroll.transform).Stretch();

            var restore = UIFactory.Button(Root, L.T("set.restore"), Palette.Grey, () =>
                IAPManager.Instance.RestorePurchases((ok, msg) => UIManager.Instance.Toast(msg)), 32);
            restore.GetComponent<RectTransform>().Place(0.25f, 0.025f, 0.75f, 0.085f);
        }

        public override void OnShow() => AdsManager.Instance.HideBanner();

        public void SelectTab(string tab)
        {
            _tab = tab;
            Refresh();
            _scroll.verticalNormalizedPosition = 1;
        }

        public override void Refresh()
        {
            var gm = GameManager.Instance;
            var d = gm.Data;
            _bar.Refresh(d);
            foreach (var kv in _tabButtons) kv.Value.SetColor(kv.Key == _tab ? Palette.Cyan : Palette.PanelLight);

            UIFactory.Clear(_content);
            if (_tab == TabFree) { BuildFree(); return; }
            if (_tab == TabCoinShop) { BuildCoinShop(); return; }

            foreach (var p in ProductCatalog.All)
            {
                if (p.category != _tab) continue;
                var id = p.id;
                bool owned = p.kind != ProductKind.Consumable && (ProductCatalog.IsOwned(d, id) || (p.kind == ProductKind.Subscription && d.vip));
                bool highlight = id == ProductIds.StarterPack || id == ProductIds.VipMonthly;
                ProductRow(p.Title, p.Description, owned ? L.T("store.owned") : IAPManager.Instance.GetPrice(id),
                    owned ? Palette.Grey : Palette.Purple, !owned, () => gm.BuyProduct(id), highlight, IconPainter.KeyForProduct(id));
            }
            if (_tab == ProductCatalog.CatOffers)
                UIFactory.Label(_content, L.T("store.sub_note"), 26, Palette.TextDim).Pref(90);
        }

        void BuildFree()
        {
            var gm = GameManager.Instance;
            var d = gm.Data;
            ProductRow(L.T("store.free_life"),
                L.T("store.free_life_desc_fmt", d.lifeVideoProgress, GameConfig.VideosPerLife, d.videoLivesInWindow, GameConfig.MaxVideoLivesPerWindow, GameConfig.VideoLifeWindowHours),
                L.T("store.open"), Palette.Green, true, () => UIManager.Instance.ShowLivesPanel(), false, "free_life");
            ProductRow(L.T("store.coins_fmt", GameConfig.RewardedCoinsAmount), L.T("store.watch_left_fmt", gm.RewardedCoinsLeft), L.T("store.watch"), Palette.Green,
                gm.RewardedCoinsLeft > 0, gm.WatchAdForCoins, false, "free_coins");
            ProductRow(L.T("store.free_upgrade"), L.T("store.free_upgrade_desc_fmt", GameConfig.VideosPerUpgrade, gm.RewardedUpgradesLeft), L.T("store.go_hangar"), Palette.Cyan,
                true, () => UIManager.Instance.ShowScreen<HangarScreen>(), false, "free_upgrade");
            ProductRow(L.T("menu.daily"), gm.Data.vip ? L.T("store.daily_vip") : L.T("store.daily_fmt", GameConfig.DailyRewardCoins),
                L.T(gm.CanClaimDaily ? "store.claim" : "store.tomorrow"), Palette.Gold, gm.CanClaimDaily, gm.ClaimDaily, false, "daily");
        }

        void BuildCoinShop()
        {
            var gm = GameManager.Instance;
            var d = gm.Data;
            UIFactory.Label(_content, L.T("store.coinshop_header"), 32, Palette.TextDim).Pref(60);
            foreach (var ship in ShipCatalog.All)
            {
                if (ship.priceType != PriceType.Coins) continue;
                var s = ship;
                bool owned = d.OwnsShip(s.id);
                ProductRow(L.T("store.ship_fmt", s.Name), s.Description, owned ? L.T("store.owned") : "$ " + UIFactory.Fmt(s.coinPrice),
                    owned ? Palette.Grey : Palette.Orange, !owned && d.coins >= s.coinPrice, () => gm.BuyShipWithCoins(s), false, "ship:" + s.id);
            }
            foreach (var paint in UpgradeCatalog.Paints)
            {
                if (paint.priceType != PriceType.Coins) continue;
                var p = paint;
                bool owned = d.ownedPaints.Contains(p.id);
                ProductRow(L.T("store.paint_fmt", p.Name), L.T("store.paint_desc"), owned ? L.T("store.owned") : "$ " + UIFactory.Fmt(p.coinPrice),
                    owned ? Palette.Grey : Palette.Orange, !owned && d.coins >= p.coinPrice, () => gm.BuyPaintWithCoins(p), false, "paint:" + p.id);
            }
            var state = d.SelectedShipState;
            string shipName = ShipCatalog.Get(state.shipId).Name;
            foreach (var up in UpgradeCatalog.Stats)
            {
                var stat = up.stat;
                int lvl = state.GetUpgrade(stat);
                bool max = lvl >= up.MaxLevel;
                ProductRow($"{up.Name} ({shipName}) {L.T("hangar.lv_fmt", lvl, up.MaxLevel)}", up.Description,
                    max ? L.T("hangar.max") : "$ " + UIFactory.Fmt(up.coinCosts[lvl]), max ? Palette.Grey : Palette.Orange,
                    !max && d.coins >= up.coinCosts[lvl], () => gm.BuyUpgradeWithCoins(state, stat), false, "up:" + stat);
            }
        }

        void ProductRow(string title, string desc, string price, Color color, bool interactable, UnityEngine.Events.UnityAction onBuy, bool highlight = false, string icon = null)
        {
            var bg = UIFactory.Image(_content, highlight ? new Color(0.22f, 0.14f, 0.38f) : Palette.Panel, true, "Product");
            bg.Pref(170);
            if (highlight) bg.gameObject.AddComponent<Outline>().effectColor = Palette.Gold;
            float textLeft = 0f;
            if (icon != null)
            {
                var img = UIFactory.Image(bg.transform, Color.white, false, "Icon");
                img.sprite = SpriteFactory.Icon(icon);
                img.preserveAspect = true;
                img.rectTransform.Place(0.01f, 0.1f, 0.16f, 0.9f);
                textLeft = 0.16f;
            }
            var t = UIFactory.Label(bg.transform, $"<b>{title}</b>\n<size=28><color=#A9B7D6>{desc}</color></size>", 36, Palette.Text, TextAnchor.MiddleLeft);
            t.rectTransform.Place(textLeft, 0, 0.64f, 1, 20, 8, 0, 8);
            var b = UIFactory.Button(bg.transform, price, color, onBuy, 36);
            b.GetComponent<RectTransform>().Place(0.66f, 0.18f, 0.98f, 0.82f, 0, 0, 12, 0);
            b.interactable = interactable;
        }
    }
}
