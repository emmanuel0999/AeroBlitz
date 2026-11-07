using UnityEngine;
using UnityEngine.UI;

namespace AeroBlitz
{
    /// <summary>Pantalla de inicio: jugar, hangar, tienda y apartado de ranking (top 5 + ver completo).</summary>
    public class MainMenuScreen : UIScreen
    {
        Text _player, _coins, _lives, _playLabel, _shipName, _vip;
        Image _shipImg;
        Button _daily;
        GameObject _missionBadge;
        RectTransform _rankList, _content;
        Text _rankStatus;
        float _appliedInset = -1f;

        protected override void Build()
        {
            UIFactory.Image(Root, Palette.Bg, false, "Bg").rectTransform.Stretch();
            // Todo el contenido va en un contenedor que se encoge por abajo cuando hay banner.
            _content = UIFactory.Rect("Content", Root).Stretch();

            // Barra superior
            var top = UIFactory.Image(_content, Palette.Panel, false, "Top");
            top.rectTransform.Place(0, 0.94f, 1, 1);
            _player = UIFactory.Label(top.transform, "", 36, Palette.Text, TextAnchor.MiddleLeft, true);
            _player.rectTransform.Place(0, 0, 0.42f, 1, 28, 0, 0, 0);
            _coins = UIFactory.Label(top.transform, "", 38, Palette.Gold, TextAnchor.MiddleRight, true);
            _coins.rectTransform.Place(0.42f, 0, 0.62f, 1);
            _lives = UIFactory.Label(top.transform, "", 38, new Color(1f, 0.45f, 0.5f), TextAnchor.MiddleRight, true);
            _lives.rectTransform.Place(0.62f, 0, 1, 1, 0, 0, 24, 0);
            _lives.gameObject.AddComponent<LiveText>().source = LivesText;

            var title = UIFactory.Label(_content, "AERO BLITZ", 120, Palette.Cyan, TextAnchor.MiddleCenter, true);
            title.rectTransform.Place(0, 0.85f, 1, 0.93f);
            title.gameObject.AddComponent<Outline>().effectColor = new Color(0, 0.2f, 0.5f);
            _vip = UIFactory.Label(_content, "", 34, Palette.Gold, TextAnchor.MiddleCenter, true);
            _vip.rectTransform.Place(0, 0.825f, 1, 0.85f);

            var shipHolder = UIFactory.Rect("ShipHolder", _content).Place(0.3f, 0.67f, 0.7f, 0.82f);
            _shipImg = ShipUI.ShipImage(shipHolder, GameManager.Instance.Data.SelectedShipState, 260);
            _shipImg.rectTransform.anchorMin = _shipImg.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _shipImg.gameObject.AddComponent<Bobbing>();
            _shipName = UIFactory.Label(_content, "", 36, Palette.TextDim);
            _shipName.rectTransform.Place(0, 0.635f, 1, 0.665f);

            var play = UIFactory.Button(_content, "", Palette.Orange, () => GameManager.Instance.PlayCurrentLevel(), 64);
            play.GetComponent<RectTransform>().Place(0.1f, 0.55f, 0.9f, 0.625f);
            _playLabel = play.GetComponentInChildren<Text>();
            _playLabel.gameObject.AddComponent<LiveText>().source = PlayText;

            var row = UIFactory.Rect("Row", _content).Place(0.05f, 0.47f, 0.95f, 0.535f);
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 18; h.childForceExpandWidth = true; h.childForceExpandHeight = true; h.childControlWidth = true; h.childControlHeight = true;
            UIFactory.Button(row, L.T("menu.hangar"), Palette.Cyan, () => UIManager.Instance.ShowScreen<HangarScreen>(), 42);
            UIFactory.Button(row, L.T("menu.store"), Palette.Purple, () => UIManager.Instance.ShowScreen<StoreScreen>(), 42);
            UIFactory.Button(row, L.T("menu.ranking"), Palette.Green, () => UIManager.Instance.ShowScreen<RankingScreen>(), 42);
            var missions = UIFactory.Button(row, L.T("menu.missions"), Palette.Gold, () => UIManager.Instance.ShowMissions(), 42);
            // Aviso rojo "!" cuando hay un premio de misión sin reclamar
            var badge = UIFactory.Image(missions.transform, Palette.Red, true, "Badge");
            ((RectTransform)badge.transform).Fixed(new Vector2(1, 1), new Vector2(14, 14), new Vector2(56, 56));
            UIFactory.Label(badge.transform, "!", 40, Color.white, TextAnchor.MiddleCenter, true).rectTransform.Stretch();
            _missionBadge = badge.gameObject;

            // Apartado de ranking en la pantalla de inicio
            var rankPanel = UIFactory.Image(_content, Palette.Panel, true, "RankPanel");
            rankPanel.rectTransform.Place(0.05f, 0.155f, 0.95f, 0.455f);
            var rt = UIFactory.Label(rankPanel.transform, L.T("menu.top_world"), 42, Palette.Gold, TextAnchor.MiddleLeft, true);
            rt.rectTransform.Place(0, 0.84f, 0.6f, 1, 30, 0, 0, 0);
            _rankStatus = UIFactory.Label(rankPanel.transform, "", 28, Palette.TextDim, TextAnchor.MiddleRight);
            _rankStatus.rectTransform.Place(0.5f, 0.84f, 1, 1, 0, 0, 30, 0);
            _rankList = UIFactory.Rect("List", rankPanel.transform).Place(0, 0.2f, 1, 0.84f, 24, 0, 24, 0);
            var vlg = _rankList.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6; vlg.childControlHeight = true; vlg.childControlWidth = true; vlg.childForceExpandHeight = false;
            var all = UIFactory.Button(rankPanel.transform, L.T("menu.see_ranking"), Palette.PanelLight, () => UIManager.Instance.ShowScreen<RankingScreen>(), 34);
            all.GetComponent<RectTransform>().Place(0.05f, 0.03f, 0.95f, 0.18f);

            // Fila inferior
            var bottom = UIFactory.Rect("Bottom", _content).Place(0.05f, 0.075f, 0.95f, 0.14f);
            var hb = bottom.gameObject.AddComponent<HorizontalLayoutGroup>();
            hb.spacing = 14; hb.childForceExpandWidth = true; hb.childForceExpandHeight = true; hb.childControlWidth = true; hb.childControlHeight = true;
            _daily = UIFactory.Button(bottom, L.T("menu.daily"), Palette.Gold, () => GameManager.Instance.ClaimDaily(), 32);
            UIFactory.Button(bottom, L.T("menu.free_life"), Palette.Green, () => UIManager.Instance.ShowLivesPanel(), 32);
            UIFactory.Button(bottom, L.T("menu.settings"), Palette.Grey, ShowSettings, 32);
            // El 7.5% inferior queda libre para el banner publicitario; si el banner es más alto, ver ApplyBannerInset.
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
            var d = GameManager.Instance.Data;
            _player.text = $"{d.username}  [{d.countryCode}]";
            _coins.text = "$ " + UIFactory.Fmt(d.coins);
            _lives.text = LivesText();
            _playLabel.text = PlayText();
            var state = d.SelectedShipState;
            var def = ShipCatalog.Get(state.shipId);
            ShipUI.UpdateShipImage(_shipImg, state, def);
            _shipName.text = L.T("menu.ship_fmt", def.Name, ShipStats.Compute(state).shipLevel, UIFactory.Fmt(d.totalPoints));
            _vip.text = d.vip ? L.T("menu.vip") : "";
            _daily.interactable = GameManager.Instance.CanClaimDaily;
            _daily.SetLabel(L.T(GameManager.Instance.CanClaimDaily ? "menu.daily" : "menu.daily_tomorrow"));
            _missionBadge.SetActive(GameManager.Instance.HasMissionReward);
        }

        /// <summary>"Vidas 2", o con cuenta regresiva de la recarga cuando está corriendo.</summary>
        static string LivesText()
        {
            var gm = GameManager.Instance;
            return gm.RefillRunning
                ? L.T("menu.lives_timer_fmt", gm.Data.lives, GameManager.FormatTime(gm.RefillRemaining))
                : L.T("lives_fmt", gm.Data.lives);
        }

        static string PlayText()
        {
            var gm = GameManager.Instance;
            if (gm.Data.lives > 0) return L.T("menu.play_fmt", gm.Data.currentLevel, GameConfig.TotalLevels);
            return gm.RefillRunning ? L.T("menu.no_lives_timer_fmt", GameManager.FormatTime(gm.RefillRemaining)) : L.T("menu.no_lives");
        }

        public void LoadRanking()
        {
            _rankStatus.text = L.T("loading");
            LeaderboardService.Instance.Fetch(null, (list, online) =>
            {
                if (this == null) return;
                _rankStatus.text = L.T(online ? "online" : "offline");
                UIFactory.Clear(_rankList);
                for (int i = 0; i < Mathf.Min(5, list.Count); i++)
                {
                    var e = list[i];
                    var row = UIFactory.Label(_rankList, $"{e.rank}.  {e.name}  <color=#9FB0D0>({e.country})</color>", 34, e.isMe ? Palette.Gold : Palette.Text, TextAnchor.MiddleLeft, e.isMe);
                    row.Pref(52);
                    var pts = UIFactory.Label(row.transform, UIFactory.Fmt(e.points), 34, Palette.Cyan, TextAnchor.MiddleRight, true);
                    pts.rectTransform.Stretch();
                }
                if (list.Count == 0) UIFactory.Label(_rankList, L.T("menu.no_pilots"), 32, Palette.TextDim).Pref(52);
            });
        }

        void ShowSettings()
        {
            var gm = GameManager.Instance;
            var d = gm.Data;
            UIManager.Instance.Dialog(L.T("menu.settings"), L.T("set.pilot_fmt", d.username, Countries.NameOf(d.countryCode)), (p, close) =>
            {
                UIManager.DialogButton(p, L.T("set.language_fmt", L.NativeName(L.Current)), Palette.Cyan,
                    UIManager.Instance.ShowLanguagePicker, close);
                Button snd = null, vib = null;
                snd = UIManager.DialogButton(p, L.T(d.soundOn ? "set.sound_on" : "set.sound_off"), Palette.PanelLight, () =>
                { d.soundOn = !d.soundOn; gm.Save(); snd.SetLabel(L.T(d.soundOn ? "set.sound_on" : "set.sound_off")); }, close, false);
                Button mus = null;
                mus = UIManager.DialogButton(p, L.T(d.musicOn ? "set.music_on" : "set.music_off"), Palette.PanelLight, () =>
                { d.musicOn = !d.musicOn; gm.Save(); mus.SetLabel(L.T(d.musicOn ? "set.music_on" : "set.music_off")); }, close, false);
                vib = UIManager.DialogButton(p, L.T(d.vibrationOn ? "set.vib_on" : "set.vib_off"), Palette.PanelLight, () =>
                { d.vibrationOn = !d.vibrationOn; gm.Save(); vib.SetLabel(L.T(d.vibrationOn ? "set.vib_on" : "set.vib_off")); }, close, false);
                // Cuenta de Google: respaldo del progreso en la nube
                var acc = AccountService.Instance;
                if (acc.IsLinked)
                {
                    var linked = UIManager.DialogButton(p, L.T("acc.linked_status"), Palette.Green, null, close, false);
                    linked.interactable = false;
                }
                else
                {
                    UIManager.DialogButton(p, L.T("acc.google_btn_backup"), Color.white,
                        () => acc.SignInWithGoogle((ok, msg) => UIManager.Instance.Toast(msg)), close)
                        .GetComponentInChildren<Text>().color = new Color(0.2f, 0.2f, 0.25f);
                }
                UIManager.DialogButton(p, L.T("set.restore"), Palette.Purple, () =>
                    IAPManager.Instance.RestorePurchases((ok, msg) => UIManager.Instance.Toast(msg)), close);

                var legal = UIFactory.HRow(p, 110, 16);
                UIFactory.Button(legal.transform, L.T("legal.terms"), Palette.PanelLight, () => UIManager.Instance.ShowLegal("terms"), 30);
                UIFactory.Button(legal.transform, L.T("legal.privacy"), Palette.PanelLight, () => UIManager.Instance.ShowLegal("privacy"), 30);

                UIManager.DialogButton(p, L.T("acc.delete_btn"), Palette.Red, () =>
                    UIManager.Instance.Confirm(L.T("acc.delete_title"), L.T(acc.IsLinked ? "acc.delete_body_linked" : "acc.delete_body_local"),
                        L.T("acc.delete_yes"), () => acc.DeleteAccountAndData((ok, msg) => UIManager.Instance.Toast(msg))), close);
                UIManager.DialogButton(p, L.T("close"), Palette.Grey, null, close);
            });
        }
    }
}
