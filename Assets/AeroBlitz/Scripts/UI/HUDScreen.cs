using UnityEngine;
using UnityEngine.UI;

namespace AeroBlitz
{
    /// <summary>Interfaz durante el juego: vida, progreso del nivel, barra de carga del arma y botón de disparo.</summary>
    public class HUDScreen : UIScreen
    {
        Text _level, _shield, _shots, _coins, _bonus;
        ProgressBar _health, _progress;
        Image _fireImg, _missileImg, _abilityImg, _ring;
        bool _laidMissiles, _laidAbility;
        Text _missileLabel, _abilityLabel;
        Text _fireLabel;
        Image _tutBox;
        Text _tutText;
        const float TutStepSeconds = 7f;
        const int TutSteps = 4;

        protected override void Build()
        {
            var pause = UIFactory.Button(Root, "", Palette.PanelLight, () => GameManager.Instance.Gameplay.Pause(), 50);
            pause.GetComponent<RectTransform>().Fixed(new Vector2(0, 1), new Vector2(24, -24), new Vector2(120, 120));
            UIKit.Icon(pause.transform, "icon_pause").rectTransform.Stretch(28);

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

            // Todos los botones van en una fila pegada al borde inferior para no tapar la zona de juego:
            // disparo a la derecha (con la carga del arma en un aro) y misiles + habilidad a la izquierda.
            var fire = UIFactory.Rect("Fire", Root).Fixed(new Vector2(1, 0), new Vector2(-Margin, Margin), new Vector2(FireSize, FireSize));
            var track = UIFactory.Image(fire, new Color(0, 0, 0, 0.55f), false, "Track");
            track.sprite = SpriteFactory.Ring; track.rectTransform.Stretch();
            _ring = UIFactory.Image(fire, Palette.Orange, false, "Charge");
            _ring.sprite = SpriteFactory.Ring; _ring.rectTransform.Stretch();
            _ring.type = Image.Type.Filled; _ring.fillMethod = Image.FillMethod.Radial360;
            _ring.fillOrigin = (int)Image.Origin360.Top; _ring.fillClockwise = true;
            _fireImg = UIFactory.Image(fire, Palette.Grey, false, "Button");
            _fireImg.sprite = SpriteFactory.Circle;
            _fireImg.raycastTarget = true;
            _fireImg.rectTransform.Stretch(26);
            _fireImg.gameObject.AddComponent<PressButton>().onPress = () => GameManager.Instance.Gameplay.FirePressed();
            _fireLabel = UIFactory.Label(_fireImg.transform, "", 36, Color.white, TextAnchor.MiddleCenter, true);
            _fireLabel.rectTransform.Stretch(6);
            _fireLabel.horizontalOverflow = HorizontalWrapMode.Overflow; // "CARGANDO" no debe partirse en dos líneas

            // Misiles (solo si los tiene) y habilidad de las naves premium: abajo a la izquierda
            _missileImg = SideButton("Missiles", () => GameManager.Instance.Gameplay.MissilePressed(), out _missileLabel);
            _missileLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            _abilityImg = SideButton("Ability", () => GameManager.Instance.Gameplay.AbilityPressed(), out _abilityLabel);
            _abilityLabel.rectTransform.Stretch(4);
            _abilityLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            _abilityLabel.verticalOverflow = VerticalWrapMode.Overflow;

            // Aviso corto del arma ("6 disparos por carga", "¡Dispara antes de que se acabe!") en el hueco de abajo
            _shots = UIFactory.Label(Root, "", 28, Palette.TextDim, TextAnchor.MiddleCenter, true);
            _shots.resizeTextForBestFit = true; _shots.resizeTextMinSize = 16; _shots.resizeTextMaxSize = 28;
            _shots.rectTransform.anchorMin = new Vector2(0, 0); _shots.rectTransform.anchorMax = new Vector2(1, 0);
            _shots.rectTransform.pivot = new Vector2(0.5f, 0);
            LayoutBottom(false, false);

            // Tutorial corto del nivel 1 (solo jugadores nuevos): un consejo cada pocos segundos, sin detener el juego
            _tutBox = UIKit.Panel(Root, new Color(0.03f, 0.08f, 0.22f, 0.72f), new Color(1f, 0.82f, 0.3f, 0.85f), "Tutorial");
            _tutBox.rectTransform.Place(0.08f, 0.7f, 0.92f, 0.785f);
            _tutText = UIFactory.Label(_tutBox.transform, "", 40, Color.white, TextAnchor.MiddleCenter, true);
            _tutText.rectTransform.Stretch(14);
            _tutText.resizeTextForBestFit = true; _tutText.resizeTextMinSize = 20; _tutText.resizeTextMaxSize = 36;
            _tutBox.gameObject.SetActive(false);
        }

        void UpdateTutorial(GameplayController gp)
        {
            var d = GameManager.Instance.Data;
            bool on = gp.Level == 1 && !d.tutorialSeen && gp.State == GameplayController.RunState.Running;
            if (_tutBox.gameObject.activeSelf != on) _tutBox.gameObject.SetActive(on);
            if (!on) return;
            int step = Mathf.FloorToInt(gp.RunTime / TutStepSeconds);
            if (step >= TutSteps)
            {
                d.tutorialSeen = true;
                GameManager.Instance.Save();
                _tutBox.gameObject.SetActive(false);
                return;
            }
            _tutText.text = L.T("tut.step" + (step + 1));
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 5f) * 0.03f;
            _tutBox.rectTransform.localScale = Vector3.one * pulse;
        }

        const float Margin = 24f, FireSize = 240f, SideSize = 172f, SideGap = 16f;
        /// <summary>Alto de la fila de botones respecto al alto de la pantalla (la nave no baja de ahí).</summary>
        public static float BottomReserve01 => (Margin + FireSize) / 1920f;

        Image SideButton(string name, System.Action onPress, out Text label)
        {
            var img = UIFactory.Image(Root, Palette.Grey, false, name);
            img.sprite = SpriteFactory.Circle;
            img.raycastTarget = true;
            img.gameObject.AddComponent<PressButton>().onPress = onPress;
            label = UIFactory.Label(img.transform, "", 28, Color.white, TextAnchor.MiddleCenter, true);
            label.rectTransform.Stretch(6);
            img.gameObject.SetActive(false);
            return img;
        }

        /// <summary>Coloca misiles y habilidad uno al lado del otro desde la esquina izquierda, y el aviso del arma en el hueco que queda.</summary>
        void LayoutBottom(bool missiles, bool ability)
        {
            _laidMissiles = missiles; _laidAbility = ability;
            float x = Margin, cy = Margin + FireSize / 2f;
            foreach (var (img, show) in new[] { (_missileImg, missiles), (_abilityImg, ability) })
            {
                img.gameObject.SetActive(show);
                if (!show) continue;
                img.rectTransform.Fixed(new Vector2(0, 0), new Vector2(x, cy - SideSize / 2f), new Vector2(SideSize, SideSize));
                x += SideSize + SideGap;
            }
            _shots.rectTransform.offsetMin = new Vector2(x + 6f, Margin + 20f);
            _shots.rectTransform.offsetMax = new Vector2(-(Margin + FireSize + 12f), Margin + FireSize - 20f);
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
            if (!gp.MissilesEnabled) return;
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

        void UpdateAbilityButton(GameplayController gp)
        {
            if (gp.Ability == ActiveAbility.None) return;
            string name = L.T("ab." + gp.Ability.ToString().ToLower()).ToUpper(L.Culture);
            // El nombre se ajusta a su largo ("HIPERVELOCIDAD" no cabe al tamaño de "FLARE") y los segundos van grandes
            int nameSize = Mathf.Clamp(Mathf.RoundToInt(270f / Mathf.Max(1, name.Length)), 17, 30);
            if (gp.AbilityActiveTime > 0f)
            {
                _abilityImg.color = Palette.Gold;
                _abilityLabel.text = $"<size={Mathf.Min(nameSize, 22)}>{name}</size>\n<size=46>{Mathf.CeilToInt(gp.AbilityActiveTime)}</size>";
            }
            else if (gp.AbilityReady)
            {
                float pulse = 0.8f + Mathf.Sin(Time.time * 6f) * 0.2f;
                var acc = ShipCatalog.Get(GameManager.Instance.Data.selectedShip).accent;
                _abilityImg.color = new Color(acc.r * pulse, acc.g * pulse, acc.b * pulse, 1f);
                _abilityLabel.text = $"<size={nameSize}>{name}</size>";
            }
            else
            {
                _abilityImg.color = new Color(0.25f, 0.25f, 0.32f);
                _abilityLabel.text = Mathf.CeilToInt(gp.AbilityCooldown) + " s";
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
            UpdateTutorial(gp);
            if (gp.IsBossLevel)
            {
                _progress.Set(gp.BossHp01);
                _progress.SetColor(gp.BossPhaseIndex >= 2 ? Palette.Red : Palette.Orange);
            }
            else
            {
                _progress.Set(gp.Progress01);
                _progress.SetColor(Palette.Cyan);
            }

            float h = gp.MaxHealth > 0 ? gp.Health / gp.MaxHealth : 0;
            _health.Set(h, $"{Mathf.CeilToInt(gp.Health)} / {Mathf.CeilToInt(gp.MaxHealth)}");
            _health.SetColor(h > 0.5f ? Palette.Green : h > 0.25f ? Palette.Gold : Palette.Red);
            _shield.text = gp.ShieldHits > 0 ? L.T("hud.shield_fmt", gp.ShieldHits) : "";
            _coins.text = gp.PickupsEnabled ? L.T("hud.run_coins_fmt", gp.RunCoins) : "";
            _bonus.text = BonusText(gp);
            bool missiles = gp.MissilesEnabled, ability = gp.Ability != ActiveAbility.None;
            if (missiles != _laidMissiles || ability != _laidAbility) LayoutBottom(missiles, ability);
            UpdateMissileButton(gp);
            UpdateAbilityButton(gp);

            // Carga del arma: el aro se llena alrededor del botón de disparo
            var w = gp.Weapon;
            switch (w.Current)
            {
                case WeaponSystem.State.Charging:
                    _ring.fillAmount = w.Bar01;
                    _ring.color = Palette.Orange;
                    _fireImg.color = Palette.Grey;
                    _fireLabel.text = $"<size=26>{L.T("hud.charging")}</size>\n{Mathf.FloorToInt(w.Bar01 * 100)}%";
                    _shots.text = L.T("hud.shots_per_fmt", w.MaxShots);
                    break;
                case WeaponSystem.State.Ready:
                    float pulse = 0.75f + Mathf.Sin(Time.time * 8f) * 0.25f;
                    _ring.fillAmount = 1f;
                    _ring.color = Palette.Gold;
                    _fireImg.color = Palette.Red * pulse + new Color(0, 0, 0, 1);
                    _fireLabel.text = L.T("hud.fire");
                    _shots.text = L.T("hud.tap_fire_fmt", w.MaxShots);
                    break;
                case WeaponSystem.State.Firing:
                    _ring.fillAmount = w.Bar01;
                    _ring.color = Palette.Cyan;
                    _fireImg.color = Palette.Red;
                    _fireLabel.text = L.T("hud.fire_n_fmt", w.ShotsLeft);
                    _shots.text = L.T("hud.hurry");
                    break;
            }
        }
    }
}
