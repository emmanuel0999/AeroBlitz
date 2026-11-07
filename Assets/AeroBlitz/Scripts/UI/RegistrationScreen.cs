using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace AeroBlitz
{
    /// <summary>Primera vez: idioma, nombre de usuario y país.</summary>
    public class RegistrationScreen : UIScreen
    {
        // Se conservan al cambiar de idioma (la pantalla se reconstruye)
        static string _pendingName = "";
        static string _country;

        InputField _name;
        Text _selected;
        Text _error;

        protected override void Build()
        {
            UIFactory.Image(Root, Palette.Bg, false, "Bg").rectTransform.Stretch();

            // Selector de idioma
            var langRow = UIFactory.Rect("Languages", Root).Place(0.06f, 0.945f, 0.94f, 0.99f);
            var h = langRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 14; h.childControlWidth = true; h.childControlHeight = true; h.childForceExpandWidth = true; h.childForceExpandHeight = true;
            foreach (var lang in L.All)
            {
                var l = lang;
                UIFactory.Button(langRow, L.NativeName(l), l == L.Current ? Palette.Cyan : Palette.PanelLight, () =>
                {
                    _pendingName = _name.text;
                    GameManager.Instance.SetLanguage(l);
                }, 34);
            }

            var title = UIFactory.Label(Root, "AERO BLITZ", 110, Palette.Cyan, TextAnchor.MiddleCenter, true);
            title.rectTransform.Place(0, 0.855f, 1, 0.94f);
            title.gameObject.AddComponent<Outline>().effectColor = new Color(0, 0.2f, 0.5f);
            var sub = UIFactory.Label(Root, L.T("reg.subtitle"), 44, Palette.TextDim);
            sub.rectTransform.Place(0, 0.82f, 1, 0.855f);

            var l1 = UIFactory.Label(Root, L.T("reg.username"), 38, Palette.Text, TextAnchor.LowerLeft, true);
            l1.rectTransform.Place(0.06f, 0.78f, 0.94f, 0.815f);
            _name = UIFactory.Input(Root, L.T("reg.name_ph"), 44);
            _name.characterLimit = 16;
            _name.text = _pendingName;
            _name.GetComponent<RectTransform>().Place(0.06f, 0.715f, 0.94f, 0.775f);

            var l2 = UIFactory.Label(Root, L.T("reg.country"), 38, Palette.Text, TextAnchor.LowerLeft, true);
            l2.rectTransform.Place(0.06f, 0.67f, 0.94f, 0.705f);
            var area = UIFactory.Rect("CountryArea", Root).Place(0.06f, 0.315f, 0.94f, 0.665f);
            CountryPicker.Build(area, _country, code => { _country = code; UpdateSelected(); });

            _selected = UIFactory.Label(Root, "", 36, Palette.Gold, TextAnchor.MiddleCenter, true);
            _selected.rectTransform.Place(0.06f, 0.28f, 0.94f, 0.312f);

            // Aceptación obligatoria de términos y privacidad
            _terms = UIFactory.Checkbox(Root, L.T("legal.checkbox"), 30, v => { _termsChecked = v; if (v) _error.text = ""; });
            _terms.GetComponent<RectTransform>().Place(0.06f, 0.215f, 0.94f, 0.275f);
            _terms.isOn = _termsChecked;
            var links = UIFactory.Rect("LegalLinks", Root).Place(0.06f, 0.175f, 0.94f, 0.21f);
            var lh = links.gameObject.AddComponent<HorizontalLayoutGroup>();
            lh.spacing = 16; lh.childControlWidth = true; lh.childControlHeight = true; lh.childForceExpandWidth = true; lh.childForceExpandHeight = true;
            UIFactory.Button(links, L.T("legal.read_terms"), Palette.PanelLight, () => UIManager.Instance.ShowLegal("terms"), 28);
            UIFactory.Button(links, L.T("legal.read_privacy"), Palette.PanelLight, () => UIManager.Instance.ShowLegal("privacy"), 28);

            _error = UIFactory.Label(Root, "", 32, Palette.Red);
            _error.rectTransform.Place(0.06f, 0.14f, 0.94f, 0.172f);

            var go = UIFactory.Button(Root, L.T("reg.start"), Palette.Orange, Submit, 56);
            go.GetComponent<RectTransform>().Place(0.1f, 0.07f, 0.9f, 0.135f);

            // Recuperar progreso de una instalación anterior u otro teléfono (opcional)
            var google = UIFactory.Button(Root, L.T("acc.recover_btn"), Color.white, SignInWithGoogle, 32);
            google.GetComponent<RectTransform>().Place(0.1f, 0.012f, 0.9f, 0.06f);
            google.GetComponentInChildren<Text>().color = new Color(0.2f, 0.2f, 0.25f);
            UpdateSelected();
        }

        static bool _termsChecked;
        Toggle _terms;

        /// <summary>Vacía nombre, país y aceptación (al borrar la cuenta se empieza de cero).</summary>
        public static void ClearPending()
        {
            _pendingName = "";
            _country = null;
            _termsChecked = false;
        }

        void SignInWithGoogle()
        {
            if (!_termsChecked) { _error.text = L.T("legal.must_accept"); return; }
            _error.text = "";
            var gm = GameManager.Instance;
            gm.AcceptTerms();
            if (gm.Data.language < 0) gm.Data.language = (int)L.Current;
            _pendingName = _name.text;
            AccountService.Instance.SignInWithGoogle((ok, msg) =>
            {
                UIManager.Instance.Toast(msg);
                // Si se recuperó un progreso registrado, ReplaceData ya abrió el menú principal.
            });
        }

        void UpdateSelected()
        {
            _selected.text = _country == null ? L.T("reg.none") : L.T("reg.country_fmt", Countries.NameOf(_country));
        }

        void Submit()
        {
            string n = _name.text.Trim();
            if (n.Length < 3) { _error.text = L.T("reg.err_short"); return; }
            if (!Regex.IsMatch(n, @"^[\p{L}0-9_ ]+$")) { _error.text = L.T("reg.err_chars"); return; }
            if (_country == null) { _error.text = L.T("reg.err_country"); return; }
            if (!_termsChecked) { _error.text = L.T("legal.must_accept"); return; }
            _error.text = "";
            var gm = GameManager.Instance;
            gm.AcceptTerms(); // guarda versión y fecha de aceptación
            if (gm.Data.language < 0) { gm.Data.language = (int)L.Current; } // fija el idioma elegido
            gm.Register(n, _country);
            _pendingName = "";
            _termsChecked = false; // si luego borra su progreso, deberá aceptar otra vez
            UIManager.Instance.ShowScreen<MainMenuScreen>();
            UIManager.Instance.Toast(L.T("reg.welcome_fmt", n));
        }
    }
}
