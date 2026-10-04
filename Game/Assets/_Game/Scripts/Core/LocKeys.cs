using System;

namespace SweetBazaar.Core
{
    // Every text key the game uses. A test checks that each key exists in every language file
    // (Game/Assets/_Game/Resources/Localization/*.json); {0}-style placeholders must match across languages.
    public static class LocKeys
    {
        public const string HudLevel = "hud.level";          // {0} = level number
        public const string HudMoves = "hud.moves";          // {0} = moves made
        public const string HudUndo = "hud.undo";
        public const string HudRestart = "hud.restart";
        public const string HudAddBox = "hud.addBox";
        public const string HudShop = "hud.shop";

        public const string WinTitle = "win.title";
        public const string WinNext = "win.next";
        public const string WinAllDone = "win.allDone";
        public const string WinCheer = "win.cheer";          // what the candy maker says
        public const string WinGold = "win.gold";            // {0} = gold earned
        public const string WinShop = "win.shop";

        public const string StuckTitle = "stuck.title";
        public const string StuckHint = "stuck.hint";

        public const string ShopTitle = "shop.title";
        public const string ShopGold = "shop.gold";                // {0} = gold the player has
        public const string ShopClose = "shop.close";
        public const string ShopHint = "shop.hint";
        public const string ShopBuild = "shop.build";              // {0} = cost in gold
        public const string ShopUse = "shop.use";
        public const string ShopCurrent = "shop.current";
        public const string ShopNotBuilt = "shop.notBuilt";
        public const string ShopMissing = "shop.missing";          // {0} = gold still missing
        public const string ShopMaxed = "shop.maxed";
        public const string ShopJustBuilt = "shop.justBuilt";
        public const string ShopFreeChange = "shop.freeChange";
        public const string ShopBuiltCount = "shop.builtCount";    // {0} = places built, {1} = places in total

        public const string ShopPlaceCounter = "shop.place.counter";
        public const string ShopPlaceDisplay = "shop.place.display";
        public const string ShopPlaceSign = "shop.place.sign";
        public const string ShopPlaceTeaCorner = "shop.place.teaCorner";
        public const string ShopPlaceFacade = "shop.place.facade";
        public const string ShopPlaceDecor = "shop.place.decor";

        public const string ShopStyleCounter0 = "shop.style.counter.0";
        public const string ShopStyleCounter1 = "shop.style.counter.1";
        public const string ShopStyleCounter2 = "shop.style.counter.2";
        public const string ShopStyleDisplay0 = "shop.style.display.0";
        public const string ShopStyleDisplay1 = "shop.style.display.1";
        public const string ShopStyleDisplay2 = "shop.style.display.2";
        public const string ShopStyleSign0 = "shop.style.sign.0";
        public const string ShopStyleSign1 = "shop.style.sign.1";
        public const string ShopStyleSign2 = "shop.style.sign.2";
        public const string ShopStyleTeaCorner0 = "shop.style.teaCorner.0";
        public const string ShopStyleTeaCorner1 = "shop.style.teaCorner.1";
        public const string ShopStyleTeaCorner2 = "shop.style.teaCorner.2";
        public const string ShopStyleFacade0 = "shop.style.facade.0";
        public const string ShopStyleFacade1 = "shop.style.facade.1";
        public const string ShopStyleFacade2 = "shop.style.facade.2";
        public const string ShopStyleDecor0 = "shop.style.decor.0";
        public const string ShopStyleDecor1 = "shop.style.decor.1";
        public const string ShopStyleDecor2 = "shop.style.decor.2";

        // The short word used in keys for a place ("counter", "display" ...).
        public static string ShopPlaceId(ShopPlace place)
        {
            switch (place)
            {
                case ShopPlace.Counter: return "counter";
                case ShopPlace.Display: return "display";
                case ShopPlace.Sign: return "sign";
                case ShopPlace.TeaCorner: return "teaCorner";
                case ShopPlace.Facade: return "facade";
                case ShopPlace.Decor: return "decor";
                default: throw new ArgumentOutOfRangeException(nameof(place));
            }
        }

        public static string ShopPlaceName(ShopPlace place) => "shop.place." + ShopPlaceId(place);

        public static string ShopStyleName(ShopPlace place, int style)
        {
            if (style < 0 || style >= ShopCatalog.StylesPerPlace)
                throw new ArgumentOutOfRangeException(nameof(style));
            return "shop.style." + ShopPlaceId(place) + "." + style;
        }
    }
}
