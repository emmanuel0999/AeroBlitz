using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AeroBlitz
{
    /// <summary>Barra superior con botón volver, título, monedas y vidas.</summary>
    public class TopBar
    {
        Text _coins, _lives;

        public static TopBar Build(RectTransform parent, string title, Action onBack)
        {
            var bar = new TopBar();
            var bg = UIFactory.Image(parent, Palette.Panel, false, "TopBar");
            bg.rectTransform.Place(0, 0.925f, 1, 1);
            if (onBack != null)
            {
                var back = UIFactory.Button(bg.transform, "", Palette.PanelLight, () => onBack(), 56);
                back.GetComponent<RectTransform>().Place(0, 0, 0, 1, 16, 14, 0, 14).sizeDelta = new Vector2(120, -28);
                back.GetComponent<RectTransform>().anchoredPosition = new Vector2(76, 0);
                UIKit.Icon(back.transform, "icon_chevron_left").rectTransform.Stretch(22);
            }
            var t = UIFactory.Label(bg.transform, title, 50, Palette.Text, TextAnchor.MiddleLeft, true);
            t.rectTransform.Place(0, 0, 0.47f, 1, onBack != null ? 160 : 30, 0, 0, 0);
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 26; t.resizeTextMaxSize = 50;
            // Monedas y vidas con sus iconos, como en el menú principal
            bar._coins = Chip(bg.transform, "icon_coin", 0.48f, 0.76f, Palette.Gold);
            bar._lives = Chip(bg.transform, "icon_heart", 0.78f, 0.98f, new Color(1f, 0.6f, 0.65f));
            return bar;
        }

        static Text Chip(Transform parent, string icon, float x0, float x1, Color color)
        {
            var chip = UIKit.Panel(parent, UIKit.PanelDark, UIKit.Border, "Chip");
            chip.rectTransform.Place(x0, 0.16f, x1, 0.84f);
            UIKit.Icon(chip.transform, icon).rectTransform.Place(0.04f, 0.12f, 0.3f, 0.88f);
            var t = UIFactory.Label(chip.transform, "", 38, color, TextAnchor.MiddleCenter, true);
            t.rectTransform.Place(0.3f, 0, 1, 1, 4, 0, 10, 0);
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 20; t.resizeTextMaxSize = 38;
            return t;
        }

        public void Refresh(PlayerData d)
        {
            _coins.text = UIFactory.Fmt(d.coins);
            _lives.text = d.lives.ToString();
        }
    }

    /// <summary>Buscador + lista de países.</summary>
    public class CountryPicker
    {
        RectTransform _content;
        string _filter = "";
        public string Selected { get; private set; }
        Action<string> _onSelect;

        public static CountryPicker Build(RectTransform area, string initial, Action<string> onSelect)
        {
            var p = new CountryPicker { Selected = initial, _onSelect = onSelect };
            var search = UIFactory.Input(area, L.T("reg.search"), 38);
            search.GetComponent<RectTransform>().Place(0, 1, 1, 1).sizeDelta = new Vector2(0, 100);
            search.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
            search.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            search.onValueChanged.AddListener(v => { p._filter = v.Trim().ToLowerInvariant(); p.Rebuild(); });

            var listBg = UIFactory.Image(area, new Color(0, 0, 0, 0.3f), true, "ListBg");
            listBg.rectTransform.Place(0, 0, 1, 1, 0, 0, 0, 115);
            p._content = UIFactory.ScrollList(listBg.transform, out var scroll, 8, 10);
            ((RectTransform)scroll.transform).Stretch();
            p.Rebuild();
            return p;
        }

        void Rebuild()
        {
            UIFactory.Clear(_content);
            foreach (var c in Countries.All)
            {
                if (_filter.Length > 0 && !c.Name.ToLowerInvariant().Contains(_filter) && !c.code.ToLowerInvariant().Contains(_filter)) continue;
                var code = c.code;
                bool sel = code == Selected;
                var b = UIFactory.Button(_content, $"{c.Name}  ({c.code})", sel ? Palette.Cyan : Palette.PanelLight, () =>
                {
                    Selected = code;
                    _onSelect?.Invoke(code);
                    Rebuild();
                }, 36);
                b.Pref(90);
                b.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleLeft;
                b.GetComponentInChildren<Text>().rectTransform.Place(0, 0, 1, 1, 30, 0, 20, 0);
            }
        }
    }

    /// <summary>Botón que responde al tocar (no al soltar) – mejor para disparar.</summary>
    public class PressButton : MonoBehaviour, IPointerDownHandler
    {
        public Action onPress;
        public void OnPointerDown(PointerEventData e) => onPress?.Invoke();
    }

    /// <summary>Texto que se recalcula cada medio segundo (cuentas regresivas).</summary>
    public class LiveText : MonoBehaviour
    {
        public Func<string> source;
        Text _text;
        float _t;

        void Update()
        {
            _t -= Time.unscaledDeltaTime;
            if (_t > 0f || source == null) return;
            _t = 0.5f;
            if (_text == null) _text = GetComponent<Text>();
            _text.text = source();
        }
    }

    /// <summary>Animación de flotación para la nave del menú.</summary>
    public class Bobbing : MonoBehaviour
    {
        public float amplitude = 18f;
        Vector2 _base;
        bool _init;
        void Update()
        {
            var rt = (RectTransform)transform;
            if (!_init) { _base = rt.anchoredPosition; _init = true; }
            rt.anchoredPosition = _base + Vector2.up * Mathf.Sin(Time.unscaledTime * 2f) * amplitude;
            rt.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.unscaledTime * 1.3f) * 3f);
        }
    }

    public static class ShipUI
    {
        /// <summary>Imagen de la nave: casco (teñido), detalles o brillo neón, y cabina.</summary>
        public static Image ShipImage(Transform parent, ShipState state, float size)
        {
            var def = ShipCatalog.Get(state != null ? state.shipId : ShipCatalog.StarterShipId);
            var img = UIFactory.Image(parent, Color.white, false, "Ship");
            img.preserveAspect = true;
            img.rectTransform.sizeDelta = new Vector2(size, size);
            var detail = UIFactory.Image(img.transform, Color.white, false, "Detail");
            detail.preserveAspect = true;
            detail.rectTransform.Stretch();
            var cockpit = UIFactory.Image(img.transform, Color.white, false, "Cockpit");
            cockpit.sprite = SpriteFactory.Cockpit;
            cockpit.preserveAspect = true;
            cockpit.rectTransform.anchorMin = cockpit.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            cockpit.rectTransform.sizeDelta = new Vector2(size * 0.172f, size * 0.172f); // 0,22 de 1,28 unidades
            UpdateShipImage(img, state, def);
            return img;
        }

        /// <summary>state null = nave que aún no tiene: se muestra con su aspecto de serie (las premium, metálicas).</summary>
        public static void UpdateShipImage(Image img, ShipState state, ShipDefinition def)
        {
            var look = ShipLook.Of(def, state);
            img.sprite = look.hull;
            img.color = look.hullColor;
            var detail = img.transform.Find("Detail")?.GetComponent<Image>();
            if (detail != null) { detail.sprite = look.detail; detail.color = look.detailColor; }
            var cockpit = img.transform.Find("Cockpit") as RectTransform;
            if (cockpit != null)
            {
                float size = img.rectTransform.sizeDelta.x;
                cockpit.anchoredPosition = new Vector2(0, def.cockpitY * SpriteFactory.ShipScale * size / 2f);
            }
        }
    }
}
