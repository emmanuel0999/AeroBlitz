using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AeroBlitz
{
    /// <summary>Imágenes del menú (logo, fondo, iconos) que están en Resources/UI, y piezas de interfaz con ese estilo.</summary>
    public static class UIKit
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        /// <summary>Sprite de Resources/UI (si se importó como textura normal, se crea el sprite al vuelo).</summary>
        public static Sprite Get(string name)
        {
            if (Cache.TryGetValue(name, out var s) && s != null) return s;
            s = Resources.Load<Sprite>("UI/" + name);
            if (s == null)
            {
                var t = Resources.Load<Texture2D>("UI/" + name);
                if (t != null) s = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100f);
            }
            Cache[name] = s;
            return s;
        }

        public static readonly Color PanelDark = new Color(0.04f, 0.07f, 0.18f, 0.88f);
        public static readonly Color Border = new Color(0.35f, 0.58f, 1f, 0.55f);

        /// <summary>Panel oscuro redondeado con borde azul suave.</summary>
        public static Image Panel(Transform parent, Color fill, Color border, string name = "Panel")
        {
            var img = UIFactory.Image(parent, fill, true, name);
            var o = img.gameObject.AddComponent<Outline>();
            o.effectColor = border;
            o.effectDistance = new Vector2(3, 3);
            return img;
        }

        public static Image Icon(Transform parent, string name, Color? tint = null)
        {
            var img = UIFactory.Image(parent, tint ?? Color.white, false, "Icon_" + name);
            img.sprite = Get(name);
            img.preserveAspect = true;
            return img;
        }

        /// <summary>Botón con fondo de color, borde más claro y sin texto propio (se le añaden iconos y textos).</summary>
        public static Button Tile(Transform parent, Color fill, Color border, UnityEngine.Events.UnityAction onClick)
        {
            var b = UIFactory.Button(parent, "", fill, onClick, 30);
            b.GetComponentInChildren<Text>().gameObject.SetActive(false);
            var o = b.gameObject.AddComponent<Outline>();
            o.effectColor = border;
            o.effectDistance = new Vector2(3, 3);
            // Brillo en la mitad superior
            var shine = UIFactory.Image(b.transform, new Color(1, 1, 1, 0.08f), true, "Shine");
            shine.rectTransform.Place(0.03f, 0.52f, 0.97f, 0.96f);
            return b;
        }

        /// <summary>
        /// Pone (o quita, con null) un icono a la izquierda del texto de un botón. El texto se aparta para dejarle sitio.
        /// Se puede llamar otra vez (por ejemplo al cambiar de "Sonido: SÍ" a "Sonido: NO") sin duplicar nada.
        /// </summary>
        public static Image SetIcon(this Button b, string icon, Color? tint = null, bool auto = false)
        {
            var label = b.GetComponentInChildren<Text>(true);
            if (label == null) return null;
            var rt = label.rectTransform;
            var mark = label.GetComponent<ButtonIconMark>();
            var existing = label.transform.Find("BtnIcon");
            if (string.IsNullOrEmpty(icon))
            {
                if (mark != null)
                {
                    if (Mathf.Approximately(rt.offsetMin.x, mark.appliedLeft)) rt.offsetMin -= new Vector2(mark.pad, 0);
                    Object.Destroy(mark);
                }
                if (existing != null) Object.Destroy(existing.gameObject);
                return null;
            }
            float s = Mathf.Clamp(label.fontSize * 1.15f, 28f, 64f);
            float pad = s + 14f;
            if (mark == null) mark = label.gameObject.AddComponent<ButtonIconMark>();
            // Si alguien recolocó el texto después de ponerle el icono, se vuelve a apartar
            if (!Mathf.Approximately(rt.offsetMin.x, mark.appliedLeft) || mark.pad <= 0f)
            {
                rt.offsetMin += new Vector2(pad, 0);
                mark.appliedLeft = rt.offsetMin.x;
            }
            mark.pad = pad; mark.auto = auto; mark.icon = icon;
            var img = existing != null ? existing.GetComponent<Image>() : UIFactory.Image(label.transform, Color.white, false, "BtnIcon");
            img.sprite = Get(icon);
            img.color = tint ?? Color.white;
            img.preserveAspect = true;
            var irt = img.rectTransform;
            irt.anchorMin = irt.anchorMax = new Vector2(0, 0.5f);
            irt.pivot = new Vector2(0, 0.5f);
            irt.anchoredPosition = new Vector2(-pad + 2f, 0);
            irt.sizeDelta = new Vector2(s, s);
            return img;
        }

        /// <summary>Vuelve a colocar el icono de un botón cuyo texto se recolocó después (Place).</summary>
        public static void RefreshIcon(this Button b)
        {
            var label = b.GetComponentInChildren<Text>(true);
            var mark = label != null ? label.GetComponent<ButtonIconMark>() : null;
            var icon = label != null ? label.transform.Find("BtnIcon") : null;
            if (mark == null || icon == null) return;
            b.SetIcon(mark.icon, icon.GetComponent<Image>().color, mark.auto);
        }

        /// <summary>Los precios en monedas se escriben "$ 4.000": se cambia el "$" por el icono de la moneda.</summary>
        public static string ApplyCoinIcon(Button b, string text)
        {
            if (text == null) return null;
            var label = b.GetComponentInChildren<Text>(true);
            var mark = label != null ? label.GetComponent<ButtonIconMark>() : null;
            if (text.Contains("$ "))
            {
                b.SetIcon("icon_coin", null, true);
                return text.Replace("$ ", "");
            }
            if (mark != null && mark.auto) b.SetIcon(null);
            return text;
        }

        /// <summary>Icono a la izquierda de una etiqueta (títulos de sección, filas).</summary>
        public static Image LabelIcon(Text label, string icon, float size = 0f)
        {
            float s = size > 0 ? size : Mathf.Clamp(label.fontSize * 1.2f, 28f, 72f);
            var img = UIFactory.Image(label.transform, Color.white, false, "LabelIcon");
            img.sprite = Get(icon);
            img.preserveAspect = true;
            var irt = img.rectTransform;
            irt.anchorMin = irt.anchorMax = new Vector2(0, 0.5f);
            irt.pivot = new Vector2(0, 0.5f);
            irt.sizeDelta = new Vector2(s, s);
            label.rectTransform.offsetMin += new Vector2(s + 14f, 0);
            irt.anchoredPosition = new Vector2(-(s + 14f), 0);
            return img;
        }

        /// <summary>Círculo rojo con un número o "!" en la esquina de un botón.</summary>
        public static Text Badge(Transform parent, float size = 54)
        {
            var badge = UIFactory.Image(parent, Palette.Red, true, "Badge");
            ((RectTransform)badge.transform).Fixed(new Vector2(1, 1), new Vector2(12, 12), new Vector2(size, size));
            var o = badge.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0.4f, 0.02f, 0.05f, 0.9f);
            var t = UIFactory.Label(badge.transform, "!", (int)(size * 0.62f), Color.white, TextAnchor.MiddleCenter, true);
            t.rectTransform.Stretch();
            return t;
        }
    }

    /// <summary>Marca en el texto de un botón que tiene icono (cuánto se apartó el texto y si lo puso el precio en monedas).</summary>
    public class ButtonIconMark : MonoBehaviour
    {
        public float pad, appliedLeft;
        public bool auto;
        public string icon;
    }

    /// <summary>Fuego del motor en el menú: tiembla y, con la estela arcoíris, cambia de color.</summary>
    public class UIFlameFlicker : MonoBehaviour
    {
        public bool rainbow;
        Image _img;
        void Update()
        {
            transform.localScale = new Vector3(1f, 0.85f + Random.value * 0.4f, 1f);
            if (!rainbow) return;
            if (_img == null) _img = GetComponent<Image>();
            _img.color = Color.HSVToRGB(Mathf.Repeat(Time.unscaledTime * 0.6f, 1f), 0.75f, 1f);
        }
    }
}
