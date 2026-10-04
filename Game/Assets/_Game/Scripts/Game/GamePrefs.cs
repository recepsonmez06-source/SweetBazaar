using UnityEngine;

namespace SweetBazaar.Game
{
    // What the game remembers between launches: the level the player is on and the chosen language.
    internal static class GamePrefs
    {
        private const string LevelKey = "level.current";
        private const string LanguageKey = "language";

        public static int CurrentLevel
        {
            get => Mathf.Max(1, PlayerPrefs.GetInt(LevelKey, 1));
            set
            {
                PlayerPrefs.SetInt(LevelKey, value);
                PlayerPrefs.Save();
            }
        }

        private const string GoldKey = "shop.gold";
        private const string ShopStylesKey = "shop.styles";

        public static int ShopGold
        {
            get => Mathf.Max(0, PlayerPrefs.GetInt(GoldKey, 0));
            set => PlayerPrefs.SetInt(GoldKey, value);
        }

        // The chosen style of every shop place, e.g. "0,-1,2,-1,-1,-1" (see Shop.SerializeStyles).
        public static string ShopStyles
        {
            get => PlayerPrefs.GetString(ShopStylesKey, "");
            set => PlayerPrefs.SetString(ShopStylesKey, value);
        }

        public static void SaveNow() => PlayerPrefs.Save();

        // Forgets the saved level (the next start is level 1). The chosen language and the shop stay.
        public static void ResetProgress()
        {
            PlayerPrefs.DeleteKey(LevelKey);
            PlayerPrefs.Save();
        }

        // Null until the player has picked a language.
        public static string Language
        {
            get => PlayerPrefs.HasKey(LanguageKey) ? PlayerPrefs.GetString(LanguageKey) : null;
            set
            {
                PlayerPrefs.SetString(LanguageKey, value);
                PlayerPrefs.Save();
            }
        }
    }

    internal static class SystemLanguageCodes
    {
        // Language codes of the phone's language, for the languages the game plans to support.
        public static string Code(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Turkish: return "tr";
                case SystemLanguage.English: return "en";
                case SystemLanguage.German: return "de";
                case SystemLanguage.Spanish: return "es";
                case SystemLanguage.Portuguese: return "pt";
                case SystemLanguage.French: return "fr";
                case SystemLanguage.Japanese: return "ja";
                default: return null;
            }
        }
    }
}
