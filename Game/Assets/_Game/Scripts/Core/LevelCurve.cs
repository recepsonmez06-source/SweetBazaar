using System;

namespace SweetBazaar.Core
{
    // Maps a level number to a DifficultyProfile. This table is the one place to tune difficulty.
    // First draft (docs/TASARIM.md section 5): tutorial, then a gradual ramp with a wave on top:
    // every 5th level is harder, the level after it is a breather.
    public static class LevelCurve
    {
        private readonly struct Stage
        {
            public Stage(int firstLevel, int minTypes, int maxTypes, int rampLevels, int emptyBoxes, int maxMovesPerType)
            {
                FirstLevel = firstLevel;
                MinTypes = minTypes;
                MaxTypes = maxTypes;
                RampLevels = rampLevels;
                EmptyBoxes = emptyBoxes;
                MaxMovesPerType = maxMovesPerType;
            }

            public int FirstLevel { get; }

            // Candy types grow from MinTypes to MaxTypes over RampLevels levels, then stay at MaxTypes.
            public int MinTypes { get; }
            public int MaxTypes { get; }
            public int RampLevels { get; }

            public int EmptyBoxes { get; }

            // Upper bound on the shortest solution per candy type (0 = no bound); keeps tutorial levels short.
            public int MaxMovesPerType { get; }
        }

        // Ordered by FirstLevel. A level uses the last stage that has started.
        private static readonly Stage[] Stages =
        {
            new Stage(firstLevel: 1,  minTypes: 2, maxTypes: 3,  rampLevels: 5,  emptyBoxes: 3, maxMovesPerType: 4),
            new Stage(firstLevel: 6,  minTypes: 4, maxTypes: 6,  rampLevels: 15, emptyBoxes: 2, maxMovesPerType: 0),
            new Stage(firstLevel: 21, minTypes: 6, maxTypes: 9,  rampLevels: 30, emptyBoxes: 2, maxMovesPerType: 0),
            new Stage(firstLevel: 51, minTypes: 8, maxTypes: 12, rampLevels: 50, emptyBoxes: 2, maxMovesPerType: 0),
        };

        private const int TutorialLevels = 5;
        private const int WavePeriod = 5;
        private const int MinTypesAfterBreather = 2;

        // Shortest solution should take at least this many moves per candy type; hard levels ask for more.
        private const int MinMovesPerType = 2;
        private const int MinMovesPerTypeHard = 3;

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
                types += (stage.MaxTypes - stage.MinTypes) * into / stage.RampLevels;

            int minMovesPerType = MinMovesPerType;
            if (levelNumber > TutorialLevels)
            {
                if (levelNumber % WavePeriod == 0)
                {
                    types++;
                    minMovesPerType = MinMovesPerTypeHard;
                }
                else if (levelNumber % WavePeriod == 1)
                {
                    types = Math.Max(MinTypesAfterBreather, types - 1);
                }
            }

            return new DifficultyProfile
            {
                CandyTypes = types,
                EmptyBoxes = stage.EmptyBoxes,
                MinMoves = types * minMovesPerType,
                MaxMoves = stage.MaxMovesPerType > 0 ? types * stage.MaxMovesPerType : int.MaxValue,
            };
        }
    }
}
