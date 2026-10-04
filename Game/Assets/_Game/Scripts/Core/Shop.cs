using System;
using System.Collections.Generic;
using System.Globalization;

namespace SweetBazaar.Core
{
    // The places of the candy shop. Each place is built once and has several looks (styles) to choose from.
    public enum ShopPlace
    {
        Counter = 0,
        Display = 1,
        Sign = 2,
        TeaCorner = 3,
        Facade = 4,
        Decor = 5,
    }

    public static class ShopCatalog
    {
        public const int PlaceCount = 6;
        public const int StylesPerPlace = 3;

        // Gold for the 1st, 2nd ... purchase of a new place. The price depends on HOW MANY places are built already,
        // not on which one the player picks, so there is no "best order" to get wrong.
        // Paced against the gold the shipped levels pay (about 14 gold per level at the start, 24-30 later; see
        // ShopRules.GoldForLevel): purchases come after roughly levels 4, 11, 31, 70 and 150, so the shop keeps giving the
        // player something to save for far into the game. Gold is also spent on extra helps (ShopRules.UndoPrice ...),
        // so it never loses its use once the shop is complete.
        private static readonly int[] PurchaseCosts = { 50, 160, 420, 1000, 2200 };

        // The counter is there from the start; every other place has to be bought.
        public static ShopPlace StartingPlace => ShopPlace.Counter;

        public static int PurchaseCount => PurchaseCosts.Length;

        // purchaseNumber starts at 1.
        public static int CostOfPurchase(int purchaseNumber)
        {
            if (purchaseNumber < 1 || purchaseNumber > PurchaseCosts.Length)
                throw new ArgumentOutOfRangeException(nameof(purchaseNumber));
            return PurchaseCosts[purchaseNumber - 1];
        }
    }

    // The candy shop the player grows with the gold earned from levels (docs/TASARIM.md section 6).
    // The player decides WHAT to build next and in WHICH style; nothing is built automatically.
    public sealed class Shop
    {
        public const int NotBuilt = -1;

        private readonly int[] _styles = new int[ShopCatalog.PlaceCount];

        // styles: the saved style of each place (NotBuilt or 0 .. StylesPerPlace - 1); missing or invalid entries mean "not built".
        public Shop(int gold = 0, IReadOnlyList<int> styles = null)
        {
            Gold = Math.Max(0, gold);

            for (int i = 0; i < _styles.Length; i++)
            {
                int style = styles != null && i < styles.Count ? styles[i] : NotBuilt;
                _styles[i] = style >= 0 && style < ShopCatalog.StylesPerPlace ? style : NotBuilt;
            }

            // the shop always starts with its counter
            if (_styles[(int)ShopCatalog.StartingPlace] == NotBuilt)
                _styles[(int)ShopCatalog.StartingPlace] = 0;
        }

        public int Gold { get; private set; }

        public int BuiltCount
        {
            get
            {
                int count = 0;
                foreach (int style in _styles)
                {
                    if (style != NotBuilt)
                        count++;
                }
                return count;
            }
        }

        public bool IsFullyBuilt => BuiltCount >= ShopCatalog.PlaceCount;

        public bool IsBuilt(ShopPlace place) => _styles[(int)place] != NotBuilt;

        // The chosen style of a place, or NotBuilt.
        public int StyleOf(ShopPlace place) => _styles[(int)place];

        // A copy of all styles (index = place), e.g. to draw the shop.
        public int[] Styles => (int[])_styles.Clone();

        // Gold the next new place costs (0 when everything is built).
        public int NextCost => IsFullyBuilt ? 0 : ShopCatalog.CostOfPurchase(BuiltCount);

        public bool CanBuild(ShopPlace place) => !IsBuilt(place) && !IsFullyBuilt && Gold >= NextCost;

        // Gold still missing for the next new place (0 if affordable or fully built).
        public int GoldMissing => IsFullyBuilt ? 0 : Math.Max(0, NextCost - Gold);

        public void AddGold(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            Gold += amount;
        }

        // Pays gold for something else (an extra help). False (nothing changes) if there is not enough gold.
        public bool TrySpend(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (Gold < amount)
                return false;

            Gold -= amount;
            return true;
        }

        // Builds a new place in the chosen style and pays for it. False (nothing changes) if the place is already built,
        // the style does not exist or there is not enough gold.
        public bool TryBuild(ShopPlace place, int style)
        {
            if (!IsValidStyle(style) || !CanBuild(place))
                return false;

            Gold -= NextCost;
            _styles[(int)place] = style;
            return true;
        }

        // Changes the style of a place that is already built. Free, any time.
        public bool TrySetStyle(ShopPlace place, int style)
        {
            if (!IsBuilt(place) || !IsValidStyle(style))
                return false;

            _styles[(int)place] = style;
            return true;
        }

        // Saved form of the styles ("0,-1,2,-1,-1,-1").
        public string SerializeStyles() => string.Join(",", _styles);

        // Reads what SerializeStyles wrote; anything unreadable gives the starting shop.
        public static int[] ParseStyles(string text)
        {
            var result = new int[ShopCatalog.PlaceCount];
            for (int i = 0; i < result.Length; i++)
                result[i] = NotBuilt;

            if (string.IsNullOrEmpty(text))
                return result;

            var parts = text.Split(',');
            for (int i = 0; i < result.Length && i < parts.Length; i++)
            {
                if (int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out int style))
                    result[i] = style;
            }
            return result;
        }

        private static bool IsValidStyle(int style) => style >= 0 && style < ShopCatalog.StylesPerPlace;
    }

    public static class ShopRules
    {
        public const int BaseGold = 10;
        public const int GoldPerCandyType = 2;
        public const int CleanSolveBonus = 5;

        // Gold for one extra help when the level's free ones have run out: about half a level's pay for an undo,
        // a bit more than one level's pay for an extra box.
        public const int UndoPrice = 12;
        public const int ExtraBoxPrice = 30;

        // Gold for finishing a level: more for harder levels (more candy types) and a bonus for a clean solve,
        // i.e. without using undo or the extra box.
        public static int GoldForLevel(int candyTypes, bool withoutHelp)
        {
            if (candyTypes < 0)
                throw new ArgumentOutOfRangeException(nameof(candyTypes));
            return BaseGold + GoldPerCandyType * candyTypes + (withoutHelp ? CleanSolveBonus : 0);
        }
    }
}
