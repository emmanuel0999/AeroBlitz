using UnityEngine;
using UnityEngine.UI;

namespace AeroBlitz
{
    /// <summary>Interfaz durante el juego: vida, progreso del nivel, barra de carga del arma y botón de disparo.</summary>
    public class HUDScreen : UIScreen
    {
        Text _level, _shield, _shots, _coins, _bonus;
        ProgressBar _health, _progress, _charge;
        Image _fireImg, _missileImg;
        Text _missileLabel;
        Text _fireLabel;

        protected override void Build()
        {
            var pause = UIFactory.Button(Root, "II", Palette.PanelLight, () => GameManager.Instance.Gameplay.Pause(), 50);
            pause.GetComponent<RectTransform>().Fixed(new Vector2(0, 1), new Vector2(24, -24), new Vector2(120, 120));

            _level = UIFactory.Label(Root, "", 38, Palette.Text, TextAnchor.MiddleCenter, true);
            _level.rectTransform.Place(0.15f, 0.955f, 0.85f, 0.99f);
            _progress = UIFactory.Bar(Root, new Color(0, 0, 0, 0.5f), Palette.Cyan);
            _progress.GetComponent<RectTransform>().Place(0.17f, 0.935f, 0.83f, 0.952f);
            _progress.label.fontSize = 0;

            var hl = UIFactory.Label(Root, L.T("hud.health"), 26, Palette.TextDim, TextAnchor.MiddleLeft, true);
            hl.rectTransform.Place(0.17f, 0.9f, 0.3f, 0.93f);
            hl.horizontalOverflow = HorizontalWrapMode.Overflow; // "HEALTH" no debe partirse en dos líneas
            _health = UIFactory.Bar(Root, new Color(0, 0, 0, 0.55f), Palette.Green);
            _health.GetComponent<RectTransform>().Place(0.3f, 0.9f, 0.97f, 0.93f);
            _health.label.fontSize = 28;
            _shield = UIFactory.Label(Root, "", 30, new Color(0.5f, 0.85f, 1f), TextAnchor.MiddleLeft, true);
            _shield.rectTransform.Place(0.3f, 0.87f, 0.97f, 0.9f);
            // Monedas recogidas en el nivel y mejoras activas con su tiempo restante
            _coins = UIFactory.Label(Root, "", 30, Palette.Gold, TextAnchor.MiddleLeft, true);
            _coins.rectTransform.Place(0.03f, 0.865f, 0.3f, 0.895f);
            _bonus = UIFactory.Label(Root, "", 28, new Color(0.9f, 0.92f, 1f), TextAnchor.MiddleLeft, true);
            _bonus.rectTransform.Place(0.3f, 0.84f, 0.97f, 0.87f);
            _bonus.resizeTextForBestFit = true;
            _bonus.resizeTextMaxSize = 28;
            _bonus.resizeTextMinSize = 18;

            // Arma
            var wl = UIFactory.Label(Root, L.T("hud.weapon"), 30, Palette.TextDim, TextAnchor.MiddleLeft, true);
            wl.rectTransform.Place(0.04f, 0.075f, 0.65f, 0.1f);
            _charge = UIFactory.Bar(Root, new Color(0, 0, 0, 0.55f), Palette.Orange);
            _charge.GetComponent<RectTransform>().Place(0.04f, 0.035f, 0.65f, 0.072f);
            _charge.label.fontSize = 28;
            _shots = UIFactory.Label(Root, "", 30, Palette.Text, TextAnchor.MiddleLeft);
            _shots.rectTransform.Place(0.04f, 0.0f, 0.65f, 0.035f);

            _fireImg = UIFactory.Image(Root, Palette.Grey, false, "Fire");
            _fireImg.sprite = SpriteFactory.Circle;
            _fireImg.raycastTarget = true;
            _fireImg.rectTransform.Fixed(new Vector2(1, 0), new Vector2(-30, 30), new Vector2(270, 270));
            _fireImg.gameObject.AddComponent<PressButton>().onPress = () => GameManager.Instance.Gameplay.FirePressed();
            _fireLabel = UIFactory.Label(_fireImg.transform, "", 36, Color.white, TextAnchor.MiddleCenter, true);
            _fireLabel.rectTransform.Stretch(6);

            // Botón de misiles: encima del de disparo, solo visible si hay misiles
            _missileImg = UIFactory.Image(Root, Palette.Grey, false, "Missiles");
            _missileImg.sprite = SpriteFactory.Circle;
            _missileImg.raycastTarget = true;
            _missileImg.rectTransform.Fixed(new Vector2(1, 0), new Vector2(-70, 320), new Vector2(190, 190));
            _missileImg.gameObject.AddComponent<PressButton>().onPress = () => GameManager.Instance.Gameplay.MissilePressed();
            _missileLabel = UIFactory.Label(_missileImg.transform, "", 28, Color.white, TextAnchor.MiddleCenter, true);
            _missileLabel.rectTransform.Stretch(6);
            _missileLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            _missileImg.gameObject.SetActive(false);
            _fireLabel.horizontalOverflow = HorizontalWrapMode.Overflow; // "CARGANDO" no debe partirse en dos líneas
        }

        public override void Refresh() { }

        static string BonusText(GameplayController gp)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (gp.TempShieldTime > 0) parts.Add(Timed(PowerUpType.Shield, gp.TempShieldTime));
            if (gp.OverchargeTime > 0) parts.Add(Timed(PowerUpType.Overcharge, gp.OverchargeTime));
            if (gp.MagnetTime > 0) parts.Add(Timed(PowerUpType.Magnet, gp.MagnetTime));
            if (gp.TrialProductId != null)
            {
                var def = ProductCatalog.Get(gp.TrialProductId);
                if (def != null) parts.Add("<color=#FFD24A>" + L.T("hud.trial_fmt", def.Title) + "</color>");
            }
            return string.Join("   ", parts);
        }

        void UpdateMissileButton(GameplayController gp)
        {
            bool show = gp.MissilesEnabled;
            if (_missileImg.gameObject.activeSelf != show) _missileImg.gameObject.SetActive(show);
            if (!show) return;
            if (gp.MissilesInLoad > 0)
            {
                _missileImg.color = new Color(1f, 0.45f, 0.18f);
                _missileLabel.text = L.T("hud.missiles_fmt", gp.MissilesInLoad);
            }
            else if (gp.MissileReloadTime > 0f)
            {
                _missileImg.color = Palette.Grey;
                _missileLabel.text = L.T("hud.missiles_reload_fmt", Mathf.CeilToInt(gp.MissileReloadTime));
            }
            else
            {
                _missileImg.color = new Color(0.25f, 0.25f, 0.3f);
                _missileLabel.text = L.T("hud.missiles_empty");
            }
        }

        static string Timed(PowerUpType t, float seconds) =>
            Colored(t, L.T("hud.pu_time_fmt", PowerUps.Name(t), Mathf.CeilToInt(seconds)));

        static string Colored(PowerUpType t, string text) =>
            "<color=#" + ColorUtility.ToHtmlStringRGB(PowerUps.ColorOf(t)) + ">" + text + "</color>";

        void Update()
        {
            var gp = GameManager.Instance.Gameplay;
            if (gp.State == GameplayController.RunState.Idle) return;
            _level.text = L.T("hud.level_fmt", gp.Level, LevelDatabase.Get(gp.Level).Title);
            _progress.Set(gp.Progress01);

            float h = gp.MaxHealth > 0 ? gp.Health / gp.MaxHealth : 0;
            _health.Set(h, $"{Mathf.CeilToInt(gp.Health)} / {Mathf.CeilToInt(gp.MaxHealth)}");
            _health.SetColor(h > 0.5f ? Palette.Green : h > 0.25f ? Palette.Gold : Palette.Red);
            _shield.text = gp.ShieldHits > 0 ? L.T("hud.shield_fmt", gp.ShieldHits) : "";
            _coins.text = gp.PickupsEnabled ? L.T("hud.run_coins_fmt", gp.RunCoins) : "";
            _bonus.text = BonusText(gp);
            UpdateMissileButton(gp);

            var w = gp.Weapon;
            switch (w.Current)
            {
                case WeaponSystem.State.Charging:
                    _charge.Set(w.Bar01, $"{Mathf.FloorToInt(w.Bar01 * 100)}%");
                    _charge.SetColor(Palette.Orange);
                    _fireImg.color = Palette.Grey;
                    _fireLabel.text = L.T("hud.charging");
                    _shots.text = L.T("hud.shots_per_fmt", w.MaxShots);
                    break;
                case WeaponSystem.State.Ready:
                    _charge.Set(1, L.T("hud.ready"));
                    _charge.SetColor(Palette.Gold);
                    float pulse = 0.75f + Mathf.Sin(Time.time * 8f) * 0.25f;
                    _fireImg.color = Palette.Red * pulse + new Color(0, 0, 0, 1);
                    _fireLabel.text = L.T("hud.fire");
                    _shots.text = L.T("hud.tap_fire_fmt", w.MaxShots);
                    break;
                case WeaponSystem.State.Firing:
                    _charge.Set(w.Bar01, $"{w.ShotsLeft} / {w.MaxShots}");
                    _charge.SetColor(Palette.Cyan);
                    _fireImg.color = Palette.Red;
                    _fireLabel.text = L.T("hud.fire_n_fmt", w.ShotsLeft);
                    _shots.text = L.T("hud.hurry");
                    break;
            }
        }
    }
}
