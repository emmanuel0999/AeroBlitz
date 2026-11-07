using UnityEngine;
using UnityEngine.UI;

namespace AeroBlitz
{
    /// <summary>Ranking: filtro Mundial (por defecto) y filtro por país.</summary>
    public class RankingScreen : UIScreen
    {
        TopBar _bar;
        Button _globalTab, _countryTab, _countryBtn;
        Text _status, _mine;
        RectTransform _content;
        ScrollRect _scroll;
        bool _byCountry;
        string _country;
        int _requestId;

        protected override void Build()
        {
            UIFactory.Image(Root, Palette.Bg, false, "Bg").rectTransform.Stretch();
            _bar = TopBar.Build(Root, L.T("menu.ranking"), () => UIManager.Instance.ShowScreen<MainMenuScreen>());

            _globalTab = UIFactory.Button(Root, L.T("rank.world"), Palette.Cyan, () => { _byCountry = false; Load(); }, 40);
            _globalTab.GetComponent<RectTransform>().Place(0.03f, 0.85f, 0.495f, 0.915f);
            _countryTab = UIFactory.Button(Root, L.T("rank.by_country"), Palette.PanelLight, () => { _byCountry = true; Load(); }, 40);
            _countryTab.GetComponent<RectTransform>().Place(0.505f, 0.85f, 0.97f, 0.915f);

            _countryBtn = UIFactory.Button(Root, "", Palette.PanelLight, PickCountry, 36);
            _countryBtn.GetComponent<RectTransform>().Place(0.03f, 0.785f, 0.97f, 0.84f);

            _status = UIFactory.Label(Root, "", 28, Palette.TextDim);
            _status.rectTransform.Place(0.03f, 0.755f, 0.97f, 0.785f);

            var header = UIFactory.Image(Root, Palette.PanelLight, true, "Header");
            header.rectTransform.Place(0.03f, 0.705f, 0.97f, 0.75f);
            Cols(header.rectTransform, "#", L.T("rank.col_pilot"), L.T("rank.col_country"), L.T("rank.col_points"), Palette.Gold, true);

            var listBg = UIFactory.Image(Root, new Color(0, 0, 0, 0.2f), true, "ListBg");
            listBg.rectTransform.Place(0.03f, 0.1f, 0.97f, 0.7f);
            _content = UIFactory.ScrollList(listBg.transform, out _scroll, 6, 8);
            ((RectTransform)_scroll.transform).Stretch();
            UIFactory.AddVerticalScrollbar(_scroll);

            var mine = UIFactory.Image(Root, new Color(0.3f, 0.22f, 0.05f), true, "Mine");
            mine.rectTransform.Place(0.03f, 0.02f, 0.97f, 0.09f);
            _mine = UIFactory.Label(mine.transform, "", 36, Palette.Gold, TextAnchor.MiddleCenter, true);
            _mine.rectTransform.Stretch();
        }

        static void Cols(RectTransform row, string a, string b, string c, string d, Color color, bool bold)
        {
            UIFactory.Label(row, a, 34, color, TextAnchor.MiddleCenter, bold).rectTransform.Place(0, 0, 0.11f, 1);
            var pilot = UIFactory.Label(row, b, 34, color, TextAnchor.MiddleLeft, bold);
            pilot.rectTransform.Place(0.11f, 0, 0.45f, 1, 10, 0, 10, 0);
            Fit(pilot, 34, 22);
            // País: columna ancha con margen a ambos lados; los nombres largos bajan a dos líneas o reducen la letra.
            var country = UIFactory.Label(row, c, 30, color, TextAnchor.MiddleLeft, bold);
            country.rectTransform.Place(0.45f, 0, 0.76f, 1, 14, 6, 14, 6);
            Fit(country, 30, 18);
            UIFactory.Label(row, d, 34, color, TextAnchor.MiddleRight, bold).rectTransform.Place(0.76f, 0, 1, 1, 0, 0, 20, 0);
        }

        /// <summary>Ajusta el tamaño de la letra para que el texto quepa entero en su celda, sin cortarse.</summary>
        static void Fit(Text t, int max, int min)
        {
            t.resizeTextForBestFit = true;
            t.resizeTextMaxSize = max;
            t.resizeTextMinSize = min;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
        }

        public override void OnShow()
        {
            AdsManager.Instance.HideBanner();
            if (_country == null) _country = GameManager.Instance.Data.countryCode;
            _byCountry = false; // Mundial es el filtro por defecto
            Load();
        }

        public override void Refresh() => _bar.Refresh(GameManager.Instance.Data);

        void PickCountry()
        {
            UIManager.Instance.Dialog(L.T("rank.choose_country"), null, (p, close) =>
            {
                var area = UIFactory.Rect("Area", p);
                area.Pref(1000);
                CountryPicker.Build(area, _country, code => { _country = code; _byCountry = true; close(); Load(); });
                UIManager.DialogButton(p, L.T("close"), Palette.Grey, null, close);
            });
        }

        void Load()
        {
            _globalTab.SetColor(_byCountry ? Palette.PanelLight : Palette.Cyan);
            _countryTab.SetColor(_byCountry ? Palette.Cyan : Palette.PanelLight);
            _countryBtn.gameObject.SetActive(_byCountry);
            _countryBtn.SetLabel(L.T("rank.country_btn_fmt", Countries.NameOf(_country)));
            _status.text = L.T("rank.loading");
            UIFactory.Clear(_content);
            int req = ++_requestId;
            LeaderboardService.Instance.Fetch(_byCountry ? _country : null, (list, online) =>
            {
                if (this == null || req != _requestId) return;
                _status.text = online ? L.T("rank.top_online_fmt", GameConfig.RankingTopCount, list.Count) : L.T("rank.offline");
                // Tu posición se busca en la lista completa, aunque estés fuera del top 50
                RankingEntry me = list.Find(x => x.isMe);
                int shown = Mathf.Min(list.Count, GameConfig.RankingTopCount);
                for (int i = 0; i < shown; i++)
                {
                    var e = list[i];
                    var bg = UIFactory.Image(_content, e.isMe ? new Color(0.3f, 0.22f, 0.05f) : Palette.Panel, true, "Row");
                    bg.Pref(84);
                    Color c = e.rank == 1 ? Palette.Gold : e.rank == 2 ? new Color(0.8f, 0.85f, 0.95f) : e.rank == 3 ? new Color(0.85f, 0.55f, 0.3f) : Palette.Text;
                    Cols(bg.rectTransform, e.rank.ToString(), e.name, Countries.NameOf(e.country), UIFactory.Fmt(e.points), e.isMe ? Palette.Gold : c, e.rank <= 3 || e.isMe);
                }
                if (list.Count == 0) UIFactory.Label(_content, L.T("rank.empty_country"), 34, Palette.TextDim).Pref(120);
                _scroll.verticalNormalizedPosition = 1f; // empezar arriba (puesto #1)
                var d = GameManager.Instance.Data;
                _mine.text = me != null ? L.T("rank.mine_fmt", me.rank, UIFactory.Fmt(d.totalPoints))
                                        : L.T("rank.mine_out_fmt", UIFactory.Fmt(d.totalPoints));
            });
        }
    }
}
