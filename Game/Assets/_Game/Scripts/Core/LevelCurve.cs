using System;

namespace SweetBazaar.Core
{
    // Maps a level number to a DifficultyProfile. This file is the one place to tune difficulty.
    //
    // Difficulty is measured with the CasualBot: the share of play-throughs it wins (100 = very easy, 0 = very hard).
    // Each level gets a TARGET win rate that only ever goes down as the level number grows, and the generator keeps
    // only levels that land close to it. That is what makes the game get steadily harder without "very easy" levels
    // popping up between hard ones: random levels of one setting vary a lot (e.g. 8 types / 2 empty boxes: from
    // 7% to 90%), the target band removes that variation.
    public static class LevelCurve
    {
        // Wanted CasualBot win rate (percent) at some level numbers; straight lines in between, flat after the last.
        private static readonly (int level, double winRate)[] TargetPoints =
        {
            (1, 100), (10, 100), (20, 96), (30, 86), (45, 66), (65, 46), (90, 28), (120, 16), (160, 9), (200, 5.5), (400, 3),
        };

        // Settings from easy to hard, with how hard random levels of that setting typically are: the mean CasualBot
        // win rate over 120 random solvable levels (measured with `tools\CoreBench cells`). The order is monotone:
        // empty boxes only go down and, with the same number of empty boxes, candy types only go up.
        private static readonly (int types, int emptyBoxes, double meanWinRate)[] Ladder =
        {
            (4, 2, 99.6), (5, 2, 94.1), (6, 2, 79.7), (7, 2, 61.5), (8, 2, 44.1), (9, 2, 25.0), (10, 2, 15.7),
            (6, 1, 12.1), (7, 1, 6.9), (8, 1, 3.5), (9, 1, 2.9), (10, 1, 1.4),
        };

        // Tutorial levels (target at or above this) use 3 empty boxes and 2..4 candy types.
        private const double TutorialTarget = 99.5;
        private const int TutorialEmptyBoxes = 3;
        private const int TutorialMaxMovesPerType = 4;

        // Art budget: no level uses more candy types than this. Beyond it, difficulty is meant to come from
        // new mechanics (closed / locked boxes, docs/TASARIM.md section 5), not from ever more candy types.
        public const int MaxCandyTypes = 10;

        // Candidates tried per level. One empty box with many types needs thousands (most random deals are unsolvable).
        private const int MaxAttempts = 8000;

        // Accepted distance (percentage points) between a level's measured and its target win rate.
        private const int Tolerance = 4;

        // Wanted CasualBot win rate (percent) for a level.
        public static double TargetWinRate(int levelNumber)
        {
            if (levelNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(levelNumber), "Level numbers start at 1.");

            for (int i = 1; i < TargetPoints.Length; i++)
            {
                if (levelNumber > TargetPoints[i].level)
                    continue;

                var from = TargetPoints[i - 1];
                var to = TargetPoints[i];
                double progress = (double)(levelNumber - from.level) / (to.level - from.level);
                return from.winRate + (to.winRate - from.winRate) * progress;
            }
            return TargetPoints[TargetPoints.Length - 1].winRate;
        }

        public static DifficultyProfile GetProfile(int levelNumber)
        {
            double target = TargetWinRate(levelNumber);

            int types, emptyBoxes, maxMoves;
            if (target >= TutorialTarget)
            {
                types = Math.Min(4, 2 + (levelNumber - 1) / 2);
                emptyBoxes = TutorialEmptyBoxes;
                maxMoves = types * TutorialMaxMovesPerType;
            }
            else
            {
                var setting = Ladder[0];
                foreach (var candidate in Ladder)
                {
                    if (Math.Abs(candidate.meanWinRate - target) < Math.Abs(setting.meanWinRate - target))
                        setting = candidate;
                }
                types = setting.types;
                emptyBoxes = setting.emptyBoxes;
                maxMoves = int.MaxValue;
            }

            return new DifficultyProfile
            {
                CandyTypes = Math.Min(types, MaxCandyTypes),
                EmptyBoxes = emptyBoxes,
                MinMoves = types * 2,               // only a sanity floor; difficulty is steered by the win rate
                MaxMoves = maxMoves,
                TargetWinRate = (int)Math.Round(target),
                WinRateTolerance = Tolerance,
                MaxAttempts = MaxAttempts,
            };
        }
    }
}
