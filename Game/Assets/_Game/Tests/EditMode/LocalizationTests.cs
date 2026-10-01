using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace SweetBazaar.Core.Tests
{
    public class LocalizerTests
    {
        private static LocalizationTable Table(string language, params string[] keyValues)
        {
            var strings = new Dictionary<string, string>();
            for (int i = 0; i < keyValues.Length; i += 2)
                strings[keyValues[i]] = keyValues[i + 1];
            return new LocalizationTable(language, language.ToUpperInvariant(), strings);
        }

        private static Localizer EnglishAndTurkish(string language = "en") => new Localizer(
            new[]
            {
                Table("en", "hello", "Hello", "level", "Level {0}", "onlyEnglish", "English only"),
                Table("tr", "hello", "Merhaba", "level", "Bölüm {0}"),
            },
            language);

        [Test]
        public void Get_ReturnsTheTextInTheCurrentLanguage()
        {
            Assert.AreEqual("Merhaba", EnglishAndTurkish("tr").Get("hello"));
            Assert.AreEqual("Hello", EnglishAndTurkish("en").Get("hello"));
        }

        [Test]
        public void Get_FallsBackToEnglishWhenTheCurrentLanguageLacksTheKey()
        {
            Assert.AreEqual("English only", EnglishAndTurkish("tr").Get("onlyEnglish"));
        }

        [Test]
        public void Get_ShowsTheKeyInBracketsWhenNoLanguageHasIt()
        {
            Assert.AreEqual("[nope]", EnglishAndTurkish().Get("nope"));
        }

        [Test]
        public void Format_FillsPlaceholders()
        {
            Assert.AreEqual("Bölüm 12", EnglishAndTurkish("tr").Format("level", 12));
        }

        [Test]
        public void UnknownStartLanguage_UsesEnglish()
        {
            Assert.AreEqual("en", EnglishAndTurkish("xx").Language);
        }

        [Test]
        public void SetLanguage_SwitchesAndRaisesTheEventOnlyOnChange()
        {
            var localizer = EnglishAndTurkish("en");
            int raised = 0;
            localizer.LanguageChanged += () => raised++;

            Assert.IsTrue(localizer.SetLanguage("tr"));
            Assert.IsTrue(localizer.SetLanguage("tr"));

            Assert.AreEqual("tr", localizer.Language);
            Assert.AreEqual(1, raised);
        }

        [Test]
        public void SetLanguage_RejectsAnUnknownLanguage()
        {
            var localizer = EnglishAndTurkish("en");

            Assert.IsFalse(localizer.SetLanguage("de"));
            Assert.AreEqual("en", localizer.Language);
        }

        [Test]
        public void NoLanguages_IsRejected()
        {
            Assert.Throws<ArgumentException>(() => new Localizer(new LocalizationTable[0], "en"));
        }

        // Available languages: en, tr.
        [TestCase("tr", "en", "tr", Description = "the saved choice wins over the phone language")]
        [TestCase(null, "tr", "tr", Description = "the phone language is used on first launch")]
        [TestCase(null, "de", "en", Description = "an unsupported phone language falls back to English")]
        [TestCase("de", "tr", "tr", Description = "an unavailable saved choice is ignored")]
        [TestCase(null, null, "en", Description = "nothing known falls back to English")]
        public void ChooseLanguage_FollowsTheRules(string saved, string system, string expected)
        {
            string chosen = Localizer.ChooseLanguage(new[] { "en", "tr" }, saved, system);

            Assert.AreEqual(expected, chosen);
        }

        [Test]
        public void ChooseLanguage_WithoutEnglish_TakesTheFirstAvailable()
        {
            Assert.AreEqual("tr", Localizer.ChooseLanguage(new[] { "tr", "de" }, null, "fr"));
        }
    }

    public class LocalizationTableTests
    {
        private const string Valid =
            "{\"formatVersion\":1,\"language\":\"tr\",\"name\":\"Türkçe\",\"strings\":{\"a\":\"Merhaba\"}}";

        [Test]
        public void FromJson_ReadsTheTable()
        {
            var table = LocalizationTable.FromJson(Valid);

            Assert.AreEqual("tr", table.Language);
            Assert.AreEqual("Türkçe", table.Name);
            Assert.AreEqual("Merhaba", table.Strings["a"]);
        }

        [TestCase("not json")]
        [TestCase("[]")]
        [TestCase("{\"formatVersion\":2,\"language\":\"tr\",\"strings\":{}}")]
        [TestCase("{\"formatVersion\":1,\"strings\":{}}")]
        [TestCase("{\"formatVersion\":1,\"language\":\"tr\"}")]
        [TestCase("{\"formatVersion\":1,\"language\":\"tr\",\"strings\":{\"a\":5}}")]
        public void FromJson_RejectsInvalidTables(string json)
        {
            Assert.Throws<FormatException>(() => LocalizationTable.FromJson(json));
        }

        [Test]
        public void FromJson_NameDefaultsToTheLanguageCode()
        {
            var table = LocalizationTable.FromJson("{\"formatVersion\":1,\"language\":\"de\",\"strings\":{}}");

            Assert.AreEqual("de", table.Name);
        }
    }

    // Checks the real language files that ship with the game.
    public class ShippedLocalizationTests
    {
        private static readonly string Folder =
            Path.Combine(Application.dataPath, "_Game", "Resources", "Localization");

        private static List<LocalizationTable> LoadAll() => Directory.GetFiles(Folder, "*.json")
            .Select(file => LocalizationTable.FromJson(File.ReadAllText(file)))
            .OrderBy(table => table.Language)
            .ToList();

        private static IEnumerable<string> AllKeys() => typeof(LocKeys)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral)
            .Select(field => (string)field.GetRawConstantValue());

        [Test]
        public void EnglishAndTurkish_Exist()
        {
            var languages = LoadAll().Select(table => table.Language).ToList();

            CollectionAssert.Contains(languages, "en");
            CollectionAssert.Contains(languages, "tr");
        }

        [Test]
        public void EveryKeyTheGameUses_ExistsInEveryLanguage()
        {
            foreach (var table in LoadAll())
            {
                foreach (string key in AllKeys())
                {
                    Assert.IsTrue(table.Strings.ContainsKey(key), $"{table.Language} is missing \"{key}\"");
                    Assert.IsNotEmpty(table.Strings[key].Trim(), $"{table.Language} \"{key}\" is empty");
                }
            }
        }

        [Test]
        public void NoLanguageHasKeysTheGameDoesNotUse()
        {
            var used = new HashSet<string>(AllKeys());

            foreach (var table in LoadAll())
            {
                foreach (string key in table.Strings.Keys)
                    Assert.IsTrue(used.Contains(key), $"{table.Language} has the unused key \"{key}\"");
            }
        }

        [Test]
        public void PlaceholdersMatchAcrossLanguages()
        {
            var tables = LoadAll();
            var english = tables.First(table => table.Language == "en");

            foreach (var table in tables)
            {
                foreach (var pair in english.Strings)
                {
                    Assert.AreEqual(Placeholders(pair.Value), Placeholders(table.Strings[pair.Key]),
                        $"{table.Language} \"{pair.Key}\"");
                }
            }
        }

        [Test]
        public void ResourcesLoadTheLanguageFilesTheWayTheGameWillLoadThem()
        {
            var assets = Resources.LoadAll<TextAsset>("Localization");

            Assert.GreaterOrEqual(assets.Length, 2);
            foreach (var asset in assets)
                Assert.DoesNotThrow(() => LocalizationTable.FromJson(asset.text), asset.name);
        }

        private static string Placeholders(string text) =>
            string.Join(",", Regex.Matches(text, @"\{\d+\}").Cast<Match>().Select(m => m.Value).OrderBy(v => v));
    }
}
