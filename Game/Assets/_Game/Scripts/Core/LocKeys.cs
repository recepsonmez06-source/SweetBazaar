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
        public const string ShopGold = "shop.gold";          // {0} = gold the player has
        public const string ShopUpgrade = "shop.upgrade";
        public const string ShopNext = "shop.next";          // {0} = name of the next stage, {1} = its cost
        public const string ShopMissing = "shop.missing";    // {0} = gold still missing
        public const string ShopMaxed = "shop.maxed";
        public const string ShopClose = "shop.close";
        public const string ShopBuilt = "shop.built";        // shown right after an upgrade

        public const string ShopStage0Name = "shop.stage.0.name";
        public const string ShopStage1Name = "shop.stage.1.name";
        public const string ShopStage2Name = "shop.stage.2.name";
        public const string ShopStage3Name = "shop.stage.3.name";
        public const string ShopStage4Name = "shop.stage.4.name";
        public const string ShopStage5Name = "shop.stage.5.name";

        public const string ShopStage0Text = "shop.stage.0.text";
        public const string ShopStage1Text = "shop.stage.1.text";
        public const string ShopStage2Text = "shop.stage.2.text";
        public const string ShopStage3Text = "shop.stage.3.text";
        public const string ShopStage4Text = "shop.stage.4.text";
        public const string ShopStage5Text = "shop.stage.5.text";

        // Name and description keys of a shop stage (0 .. ShopStages.Count - 1).
        public static string ShopStageName(int stage) => "shop.stage." + CheckStage(stage) + ".name";

        public static string ShopStageText(int stage) => "shop.stage." + CheckStage(stage) + ".text";

        private static int CheckStage(int stage)
        {
            if (stage < 0 || stage >= ShopStages.Count)
                throw new ArgumentOutOfRangeException(nameof(stage));
            return stage;
        }
    }
}
