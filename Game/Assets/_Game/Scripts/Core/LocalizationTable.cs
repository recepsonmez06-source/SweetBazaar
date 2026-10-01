using System;
using System.Collections.Generic;

namespace SweetBazaar.Core
{
    // The texts of one language. JSON format (one file per language):
    // { "formatVersion": 1, "language": "tr", "name": "Türkçe", "strings": { "hud.level": "Bölüm {0}", ... } }
    // "name" is the language's own name, shown in the language picker.
    public sealed class LocalizationTable
    {
        public const int FormatVersion = 1;

        public LocalizationTable(string language, string name, IReadOnlyDictionary<string, string> strings)
        {
            if (string.IsNullOrWhiteSpace(language))
                throw new ArgumentException("A language code is required.", nameof(language));

            Language = language;
            Name = string.IsNullOrWhiteSpace(name) ? language : name;
            Strings = strings ?? throw new ArgumentNullException(nameof(strings));
        }

        // Lower-case code such as "en" or "tr".
        public string Language { get; }

        public string Name { get; }

        public IReadOnlyDictionary<string, string> Strings { get; }

        // Throws FormatException if the text is not valid JSON or not a language table.
        public static LocalizationTable FromJson(string json)
        {
            var root = MiniJson.Parse(json) as Dictionary<string, object>
                ?? throw new FormatException("The JSON root must be an object.");

            if (!(Read(root, "formatVersion") is double version) || version != FormatVersion)
                throw new FormatException($"Unsupported localization format version (expected {FormatVersion}).");

            string language = Read(root, "language") as string
                ?? throw new FormatException("Missing or invalid \"language\".");
            string name = Read(root, "name") as string;

            var rawStrings = Read(root, "strings") as Dictionary<string, object>
                ?? throw new FormatException("Missing or invalid \"strings\".");

            var strings = new Dictionary<string, string>(rawStrings.Count);
            foreach (var pair in rawStrings)
            {
                strings[pair.Key] = pair.Value as string
                    ?? throw new FormatException($"The text for \"{pair.Key}\" must be a string.");
            }

            return new LocalizationTable(language, name, strings);
        }

        private static object Read(Dictionary<string, object> obj, string name)
        {
            obj.TryGetValue(name, out object value);
            return value;
        }
    }
}
