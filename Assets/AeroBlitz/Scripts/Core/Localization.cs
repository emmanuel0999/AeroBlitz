using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace AeroBlitz
{
    public enum Language { Spanish = 0, English = 1, French = 2 }

    /// <summary>
    /// Sistema de idiomas. Uso: L.T("clave") o L.T("clave_fmt", arg0, arg1...).
    /// Para añadir un idioma: agrega un valor al enum, una columna en LocalizationTable y en Countries.
    /// </summary>
    public static class L
    {
        public static Language Current { get; private set; } = Language.Spanish;
        public static event Action Changed;

        public static readonly Language[] All = { Language.Spanish, Language.English, Language.French };

        static Dictionary<string, string[]> _table;

        public static void Set(Language lang)
        {
            if (Current == lang && _table != null) return;
            Current = lang;
            Changed?.Invoke();
        }

        /// <summary>Idioma del teléfono; si no es uno soportado, inglés.</summary>
        public static Language FromSystem()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Spanish: return Language.Spanish;
                case SystemLanguage.French: return Language.French;
                default: return Language.English;
            }
        }

        /// <summary>Código corto del idioma actual ("es", "en", "fr").</summary>
        public static string Code => Current == Language.English ? "en" : Current == Language.French ? "fr" : "es";

        public static string NativeName(Language l)
        {
            switch (l)
            {
                case Language.English: return "English";
                case Language.French: return "Français";
                default: return "Español";
            }
        }

        public static CultureInfo Culture
        {
            get
            {
                switch (Current)
                {
                    case Language.English: return CultureInfo.GetCultureInfo("en-US");
                    case Language.French: return CultureInfo.GetCultureInfo("fr-FR");
                    default: return CultureInfo.GetCultureInfo("es-ES");
                }
            }
        }

        public static string T(string key)
        {
            if (_table == null) _table = LocalizationTable.Build();
            if (_table.TryGetValue(key, out var row))
            {
                string s = row[(int)Current];
                return string.IsNullOrEmpty(s) ? row[0] : s;
            }
            Debug.LogWarning("[L] Falta traducción: " + key);
            return key;
        }

        public static string T(string key, params object[] args)
        {
            try { return string.Format(Culture, T(key), args); }
            catch (FormatException) { return T(key); }
        }

        /// <summary>Verifica (en el Editor) que todas las claves tengan los 3 idiomas.</summary>
        public static List<string> MissingTranslations()
        {
            var missing = new List<string>();
            foreach (var kv in LocalizationTable.Build())
                for (int i = 0; i < kv.Value.Length; i++)
                    if (string.IsNullOrEmpty(kv.Value[i])) missing.Add($"{kv.Key} [{(Language)i}]");
            return missing;
        }
    }
}
