using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SweetBazaar.Core
{
    // Looks texts up by key in the current language, falling back to English (the default language).
    // No text in the game is hard-coded: everything player-visible comes from here (see LocKeys).
    public sealed class Localizer
    {
        public const string DefaultLanguage = "en";

        private readonly List<LocalizationTable> _tables;
        private readonly LocalizationTable _fallback;
        private LocalizationTable _current;

        public Localizer(IEnumerable<LocalizationTable> tables, string language)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            _tables = tables.ToList();
            if (_tables.Count == 0)
                throw new ArgumentException("At least one language is required.", nameof(tables));

            _fallback = Find(DefaultLanguage) ?? _tables[0];
            _current = Find(language) ?? _fallback;
        }

        // Raised after the current language changed, so views can refresh their texts.
        public event Action LanguageChanged;

        public string Language => _current.Language;

        public IReadOnlyList<LocalizationTable> Languages => _tables;

        // Returns false (and changes nothing) if the language is not available.
        public bool SetLanguage(string language)
        {
            var table = Find(language);
            if (table == null)
                return false;

            if (table != _current)
            {
                _current = table;
                LanguageChanged?.Invoke();
            }
            return true;
        }

        // Current language, then English, otherwise "[key]" so a missing text is visible instead of blank.
        public string Get(string key)
        {
            if (_current.Strings.TryGetValue(key, out string text))
                return text;
            if (_fallback.Strings.TryGetValue(key, out text))
                return text;
            return "[" + key + "]";
        }

        public string Format(string key, params object[] args) =>
            string.Format(CultureInfo.InvariantCulture, Get(key), args);

        // First launch: the player's saved choice, else the phone's language, else English.
        public static string ChooseLanguage(IEnumerable<string> available, string savedLanguage, string systemLanguage)
        {
            var codes = available.ToList();
            if (savedLanguage != null && codes.Contains(savedLanguage))
                return savedLanguage;
            if (systemLanguage != null && codes.Contains(systemLanguage))
                return systemLanguage;
            if (codes.Contains(DefaultLanguage))
                return DefaultLanguage;
            return codes.Count > 0 ? codes[0] : DefaultLanguage;
        }

        private LocalizationTable Find(string language) =>
            _tables.FirstOrDefault(table => table.Language == language);
    }
}
