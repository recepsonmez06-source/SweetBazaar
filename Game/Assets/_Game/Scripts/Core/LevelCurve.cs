using System;

namespace SweetBazaar.Core
{
    // Maps a level number to a DifficultyProfile. This table is the one place to tune difficulty.
    // Shape (docs/TASARIM.md section 5): a short tutorial, then a gradual climb with a wave on top: every 5th level
    // is a hard one and the level after it a breather. Later stages make hard levels harder by taking away empty boxes,
    // which is the classic way sort puzzles get difficult.
    public static class LevelCurve
    {
        private readonly struct Stage
        {
            public Stage(int firstLevel, int minTypes, int maxTypes, int rampLevels, int emptyBoxes,
                int maxMovesPerType, int hardEmptyBoxes, bool alsoHardOnThirdLevel)
            {
                FirstLevel = firstLevel;
                MinTypes = minTypes;
                MaxTypes = maxTypes;
                RampLevels = rampLevels;
                EmptyBoxes = emptyBoxes;
                MaxMovesPerType = maxMovesPerType;
                HardEmptyBoxes = hardEmptyBoxes;
                AlsoHardOnThirdLevel = alsoHardOnThirdLevel;
            }

            public int FirstLevel { get; }

            // Candy types grow from MinTypes to MaxTypes over RampLevels levels, then stay at MaxTypes.
            public int MinTypes { get; }
            public int MaxTypes { get; }
            public int RampLevels { get; }

            public int EmptyBoxes { get; }

            // Upper bound on the shortest solution per candy type (0 = no bound); keeps tutorial levels short.
            public int MaxMovesPerType { get; }

            // Empty boxes on hard levels. Fewer than EmptyBoxes means hard levels are made harder by taking boxes
            // away (instead of adding a candy type); 0 means "same as EmptyBoxes" (hard = one more type).
            public int HardEmptyBoxes { get; }

            // Also treat every level that is the 3rd of a group of 5 as hard (late stages only).
            public bool AlsoHardOnThirdLevel { get; }
        }

        // Ordered by FirstLevel. A level uses the last stage that has started.
        private static readonly Stage[] Stages =
        {
            new Stage(firstLevel: 1,  minTypes: 2, maxTypes: 3,  rampLevels: 4,  emptyBoxes: 3, maxMovesPerType: 4, hardEmptyBoxes: 0, alsoHardOnThirdLevel: false),
            new Stage(firstLevel: 6,  minTypes: 3, maxTypes: 6,  rampLevels: 18, emptyBoxes: 2, maxMovesPerType: 0, hardEmptyBoxes: 0, alsoHardOnThirdLevel: false),
            new Stage(firstLevel: 25, minTypes: 6, maxTypes: 9,  rampLevels: 35, emptyBoxes: 2, maxMovesPerType: 0, hardEmptyBoxes: 1, alsoHardOnThirdLevel: false),
            new Stage(firstLevel: 60, minTypes: 8, maxTypes: 10, rampLevels: 40, emptyBoxes: 2, maxMovesPerType: 0, hardEmptyBoxes: 1, alsoHardOnThirdLevel: true),
        };

        // Art budget: no level uses more candy types than this. Beyond it, difficulty is meant to come from
        // new mechanics (closed / locked boxes, docs/TASARIM.md section 5), not from ever more candy types.
        public const int MaxCandyTypes = 10;

        private const int SingleEmptyBoxAttempts = 8000;

        private const int TutorialLevels = 5;
        private const int WavePeriod = 5;
        // A breather never drops below the tutorial's last level, so the game does not get easier right after it.
        private const int MinTypesAfterBreather = 3;

        // The shortest solution should take at least this many moves per candy type, in tenths (25 = 2.5).
        // A random deal of a normal level typically needs about 3 per type, so 2.5 only rejects the easiest deals.
        private const int TutorialMinMovesTenths = 20;
        private const int NormalMinMovesTenths = 25;
        private const int HardMinMovesTenths = 33;
        private const int SingleEmptyBoxMinMovesTenths = 28;

        public static DifficultyProfile GetProfile(int levelNumber)
        {
            if (levelNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(levelNumber), "Level numbers start at 1.");

            var stage = Stages[0];
            foreach (var candidate in Stages)
            {
                if (levelNumber >= candidate.FirstLevel)
                    stage = candidate;
            }

            int into = Math.Min(levelNumber - stage.FirstLevel, stage.RampLevels);
            int types = stage.MinTypes;
            if (stage.RampLevels > 0)
            {
                // rounded, so a 2 -> 3 ramp switches half way instead of at the very end
                types += ((stage.MaxTypes - stage.MinTypes) * into * 2 / stage.RampLevels + 1) / 2;
            }

            bool inWave = levelNumber > TutorialLevels;
            bool hard = inWave && (levelNumber % WavePeriod == 0
                || (stage.AlsoHardOnThirdLevel && levelNumber % WavePeriod == 3));
            bool breather = inWave && levelNumber % WavePeriod == 1;

            int emptyBoxes = stage.EmptyBoxes;
            int minMovesTenths = inWave ? NormalMinMovesTenths : TutorialMinMovesTenths;

            if (hard)
            {
                minMovesTenths = HardMinMovesTenths;
                if (stage.HardEmptyBoxes > 0 && stage.HardEmptyBoxes < stage.EmptyBoxes)
                {
                    // The difficulty comes from the missing empty box, so the solution length needs not be extreme;
                    // asking for 3.3 moves per type would almost never be met at 10 types (measured median is 3.0).
                    emptyBoxes = stage.HardEmptyBoxes;
                    minMovesTenths = SingleEmptyBoxMinMovesTenths;
                }
                else
                {
                    types++;
                }
            }
            else if (breather)
            {
                types = Math.Max(MinTypesAfterBreather, types - 1);
            }

            types = Math.Min(types, MaxCandyTypes);

            return new DifficultyProfile
            {
                CandyTypes = types,
                EmptyBoxes = emptyBoxes,
                MinMoves = types * minMovesTenths / 10,
                MaxMoves = stage.MaxMovesPerType > 0 ? types * stage.MaxMovesPerType : int.MaxValue,

                // Measured: with one empty box roughly 1 deal in 8 is solvable at 6 types and 1 in 135 at 10 types.
                MaxAttempts = emptyBoxes < 2 ? SingleEmptyBoxAttempts : 200,
            };
        }
    }
}
