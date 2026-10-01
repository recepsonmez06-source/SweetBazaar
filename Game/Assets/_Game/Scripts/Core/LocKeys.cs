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

        public const string WinTitle = "win.title";
        public const string WinNext = "win.next";
        public const string WinAllDone = "win.allDone";

        public const string StuckTitle = "stuck.title";
        public const string StuckHint = "stuck.hint";
    }
}
