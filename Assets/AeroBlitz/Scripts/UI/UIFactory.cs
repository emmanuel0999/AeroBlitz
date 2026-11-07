using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AeroBlitz
{
    public static class Palette
    {
        public static readonly Color Bg = new Color(0.03f, 0.05f, 0.12f, 0.96f);
        public static readonly Color Panel = new Color(0.08f, 0.11f, 0.24f, 0.97f);
        public static readonly Color PanelLight = new Color(0.13f, 0.17f, 0.34f, 1f);
        public static readonly Color Cyan = new Color(0.15f, 0.75f, 1f);
        public static readonly Color Orange = new Color(1f, 0.55f, 0.15f);
        public static readonly Color Green = new Color(0.2f, 0.78f, 0.4f);
        public static readonly Color Red = new Color(0.9f, 0.25f, 0.3f);
        public static readonly Color Gold = new Color(1f, 0.8f, 0.2f);
        public static readonly Color Purple = new Color(0.58f, 0.35f, 1f);
        public static readonly Color Grey = new Color(0.35f, 0.38f, 0.48f);
        public static readonly Color Text = new Color(0.93f, 0.96f, 1f);
        public static readonly Color TextDim = new Color(0.65f, 0.72f, 0.85f);
    }

    /// <summary>Helpers para construir la interfaz (uGUI) desde código.</summary>
    public static class UIFactory
    {
        static Font _font;
        public static Font Font
        {
            get
            {
                if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return _font;
            }
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5; // UI
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        /// <summary>Anclas en proporción (0..1) del padre, con márgenes en píxeles de referencia.</summary>
        public static RectTransform Place(this RectTransform rt, float minX, float minY, float maxX, float maxY, float padL = 0, float padB = 0, float padR = 0, float padT = 0)
        {
            rt.anchorMin = new Vector2(minX, minY);
            rt.anchorMax = new Vector2(maxX, maxY);
            rt.offsetMin = new Vector2(padL, padB);
            rt.offsetMax = new Vector2(-padR, -padT);
            return rt;
        }

        public static RectTransform Stretch(this RectTransform rt, float pad = 0) => rt.Place(0, 0, 1, 1, pad, pad, pad, pad);

        /// <summary>Tamaño fijo, anclado a un punto del padre.</summary>
        public static RectTransform Fixed(this RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static T Pref<T>(this T c, float height, float width = -1) where T : Component
        {
            if (!c.TryGetComponent<LayoutElement>(out var le)) le = c.gameObject.AddComponent<LayoutElement>();
            if (height >= 0) le.preferredHeight = le.minHeight = height;
            if (width >= 0) le.preferredWidth = width;
            return c;
        }

        public static Image Image(Transform parent, Color color, bool rounded = true, string name = "Image")
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = rounded ? SpriteFactory.Rounded : SpriteFactory.White;
            img.type = rounded ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static Text Label(Transform parent, string text, int size, Color color, TextAnchor align = TextAnchor.MiddleCenter, bool bold = false)
        {
            var rt = Rect("Text", parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            t.supportRichText = true;
            return t;
        }

        public static Button Button(Transform parent, string label, Color color, UnityAction onClick, int fontSize = 40)
        {
            var img = Image(parent, color, true, "Button");
            img.raycastTarget = true;
            var btn = img.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            btn.colors = colors;
            var txt = Label(img.transform, label, fontSize, Color.white, TextAnchor.MiddleCenter, true);
            txt.rectTransform.Stretch(8);
            txt.resizeTextForBestFit = true;
            txt.resizeTextMaxSize = fontSize;
            txt.resizeTextMinSize = 14;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
            btn.onClick.AddListener(() => AudioFX.Instance?.Play(AudioFX.Sfx.Click));
            if (onClick != null) btn.onClick.AddListener(onClick);
            return btn;
        }

        public static void SetLabel(this Button b, string text) => b.GetComponentInChildren<Text>().text = text;
        public static void SetColor(this Button b, Color c) => b.GetComponent<Image>().color = c;

        public static ProgressBar Bar(Transform parent, Color bg, Color fill)
        {
            var back = Image(parent, bg, true, "Bar");
            var fillImg = Image(back.transform, fill, true, "Fill");
            fillImg.rectTransform.Place(0, 0, 1, 1, 4, 4, 4, 4);
            var label = Label(back.transform, "", 28, Color.white, TextAnchor.MiddleCenter, true);
            label.rectTransform.Stretch();
            var pb = back.gameObject.AddComponent<ProgressBar>();
            pb.fill = fillImg;
            pb.label = label;
            return pb;
        }

        public static InputField Input(Transform parent, string placeholder, int fontSize = 42)
        {
            var bg = Image(parent, new Color(1, 1, 1, 0.95f), true, "Input");
            bg.raycastTarget = true;
            var input = bg.gameObject.AddComponent<InputField>();
            var ph = Label(bg.transform, placeholder, fontSize, new Color(0.4f, 0.4f, 0.5f), TextAnchor.MiddleLeft);
            ph.fontStyle = FontStyle.Italic;
            ph.rectTransform.Place(0, 0, 1, 1, 24, 0, 24, 0);
            var txt = Label(bg.transform, "", fontSize, new Color(0.08f, 0.1f, 0.2f), TextAnchor.MiddleLeft);
            txt.rectTransform.Place(0, 0, 1, 1, 24, 0, 24, 0);
            txt.supportRichText = false;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            input.textComponent = txt;
            input.placeholder = ph;
            input.targetGraphic = bg;
            return input;
        }

        /// <summary>Lista con scroll vertical. Devuelve el contenedor donde añadir filas.</summary>
        public static RectTransform ScrollList(Transform parent, out ScrollRect scroll, float spacing = 16, int padding = 12)
        {
            var root = Rect("Scroll", parent);
            scroll = root.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40;
            var viewport = Rect("Viewport", root).Stretch();
            viewport.gameObject.AddComponent<RectMask2D>();
            var hit = viewport.gameObject.AddComponent<Image>();
            hit.color = new Color(0, 0, 0, 0);
            var content = Rect("Content", viewport);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.offsetMin = content.offsetMax = Vector2.zero;
            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = spacing;
            vlg.padding = new RectOffset(padding, padding, padding, padding);
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport;
            scroll.content = content;
            return content;
        }

        /// <summary>Casilla de verificación con texto (tocar el texto también la marca).</summary>
        public static Toggle Checkbox(Transform parent, string label, int fontSize, Action<bool> onChanged)
        {
            var root = Rect("Checkbox", parent);
            var toggle = root.gameObject.AddComponent<Toggle>();
            var box = Image(root, Color.white, true, "Box");
            box.raycastTarget = true;
            box.rectTransform.anchorMin = box.rectTransform.anchorMax = new Vector2(0, 0.5f);
            box.rectTransform.pivot = new Vector2(0, 0.5f);
            box.rectTransform.sizeDelta = new Vector2(72, 72);
            box.rectTransform.anchoredPosition = Vector2.zero;
            var check = Image(box.transform, Palette.Green, true, "Check");
            check.rectTransform.Stretch(12);
            var text = Label(root, label, fontSize, Palette.Text, TextAnchor.MiddleLeft);
            text.rectTransform.Place(0, 0, 1, 1, 92, 0, 0, 0);
            text.raycastTarget = true;
            toggle.targetGraphic = box;
            toggle.graphic = check;
            toggle.isOn = false;
            toggle.onValueChanged.AddListener(v => { AudioFX.Instance?.Play(AudioFX.Sfx.Click); onChanged?.Invoke(v); });
            return toggle;
        }

        /// <summary>Barra de scroll vertical visible a la derecha (deja espacio al contenido).</summary>
        public static Scrollbar AddVerticalScrollbar(ScrollRect scroll, float width = 18)
        {
            ((RectTransform)scroll.viewport).offsetMax = new Vector2(-(width + 8), 0);
            var track = Image(scroll.transform, new Color(1, 1, 1, 0.08f), false, "Scrollbar");
            track.raycastTarget = true;
            track.rectTransform.anchorMin = new Vector2(1, 0);
            track.rectTransform.anchorMax = new Vector2(1, 1);
            track.rectTransform.pivot = new Vector2(1, 0.5f);
            track.rectTransform.offsetMin = new Vector2(-width, 6);
            track.rectTransform.offsetMax = new Vector2(-2, -6);
            var area = Rect("Sliding Area", track.transform).Stretch();
            var handle = Image(area, Palette.Cyan, false, "Handle");
            handle.raycastTarget = true;
            handle.rectTransform.Stretch();
            var bar = track.gameObject.AddComponent<Scrollbar>();
            bar.direction = Scrollbar.Direction.BottomToTop;
            bar.handleRect = handle.rectTransform;
            bar.targetGraphic = handle;
            scroll.verticalScrollbar = bar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            return bar;
        }

        public static HorizontalLayoutGroup HRow(Transform parent, float height, float spacing = 12)
        {
            var rt = Rect("Row", parent);
            var h = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing;
            h.childControlHeight = true;
            h.childControlWidth = true;
            h.childForceExpandHeight = true;
            h.childForceExpandWidth = true;
            h.childAlignment = TextAnchor.MiddleCenter;
            rt.Pref(height);
            return h;
        }

        public static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
        }

        public static string Fmt(long n) => n.ToString("N0", L.Culture);
    }

    public class ProgressBar : MonoBehaviour
    {
        public Image fill;
        public Text label;
        float _value = 1f;

        public void Set(float value01, string text = null)
        {
            _value = Mathf.Clamp01(value01);
            fill.rectTransform.anchorMax = new Vector2(_value, 1);
            fill.enabled = _value > 0.001f;
            if (text != null) label.text = text;
        }

        public void SetColor(Color c) => fill.color = c;
    }
}
