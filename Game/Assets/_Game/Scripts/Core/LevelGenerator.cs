using System;
using System.Collections.Generic;

namespace SweetBazaar.Core
{
    public static class LevelGenerator
    {
        // Shuffles the candies into boxes from a seed, lets the solver check the result and keeps the first
        // candidate that is solvable and falls inside the profile's move range. The same profile and seed
        // always give the same level. Throws InvalidOperationException if no candidate is accepted.
        public static GeneratedLevel Generate(DifficultyProfile profile, int seed)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            var errors = profile.Validate();
            if (errors.Count > 0)
                throw new ArgumentException("Invalid profile: " + string.Join("; ", errors), nameof(profile));

            var solverOptions = new SolverOptions { MaxStates = profile.SolverStateLimit };

            for (int attempt = 0; attempt < profile.MaxAttempts; attempt++)
            {
                var random = new SeededRandom(Mix(seed, attempt));
                var level = RandomLevel(profile, random);
                var board = Board.FromLevel(level);

                if (HasClosedBox(board))
                    continue;

                // Cheap difficulty pre-check first: it rejects most candidates for a fraction of the solver's cost.
                if (profile.TargetWinRate >= 0 && !PassesPrefilter(board, profile, seed))
                    continue;

                var result = Solver.Solve(board, solverOptions);
                if (result.Status != SolveStatus.Solved)
                    continue;

                int moves = result.Moves.Count;
                if (moves < profile.MinMoves || moves > profile.MaxMoves)
                    continue;

                int winRate = -1;
                if (profile.TargetWinRate >= 0)
                {
                    winRate = CasualBot.WinRatePercent(board, profile.BotTrials, BotSeed(seed));
                    if (Math.Abs(winRate - profile.TargetWinRate) > profile.WinRateTolerance)
                        continue;
                }

                return new GeneratedLevel(level, seed, moves, attempt + 1, winRate);
            }

            throw new InvalidOperationException(
                $"No level accepted for seed {seed} within {profile.MaxAttempts} attempts.");
        }

        // The level for a level number, following LevelCurve. The level number is the seed.
        public static GeneratedLevel GenerateForLevel(int levelNumber) =>
            Generate(LevelCurve.GetProfile(levelNumber), levelNumber);

        // How easy a level is for the CasualBot: percent of play-throughs won. The bot plays the same way every time
        // for a given level seed, so the number is stable and can be checked again later.
        public static int MeasureWinRate(Board board, int seed, int trials) =>
            CasualBot.WinRatePercent(board, trials, BotSeed(seed));

        private static ulong BotSeed(int seed) => 0xB07UL * 1000003UL + (ulong)(uint)seed;

        // With few trials the measured rate is noisy (up to about 11 points of standard deviation for 20 trials),
        // so the pre-check only rejects candidates that are far outside the wanted band.
        private static bool PassesPrefilter(Board board, DifficultyProfile profile, int seed)
        {
            int quick = CasualBot.WinRatePercent(board, profile.BotPrefilterTrials, BotSeed(seed));
            int slack = (int)(125.0 / Math.Sqrt(profile.BotPrefilterTrials));
            return Math.Abs(quick - profile.TargetWinRate) <= profile.WinRateTolerance + slack;
        }

        private static ulong Mix(int seed, int attempt)
        {
            unchecked
            {
                return (ulong)(uint)seed * 0x9E3779B97F4A7C15UL + (ulong)attempt * 0xD1B54A32D192ED03UL;
            }
        }

        private static LevelDefinition RandomLevel(DifficultyProfile profile, SeededRandom random)
        {
            int capacity = profile.BoxCapacity;
            var candies = new int[profile.CandyTypes * capacity];
            for (int i = 0; i < candies.Length; i++)
                candies[i] = i / capacity;

            // Fisher-Yates shuffle
            for (int i = candies.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int swap = candies[i];
                candies[i] = candies[j];
                candies[j] = swap;
            }

            var boxes = new List<int[]>();
            for (int b = 0; b < profile.CandyTypes; b++)
            {
                var box = new int[capacity];
                Array.Copy(candies, b * capacity, box, 0, capacity);
                boxes.Add(box);
            }
            for (int e = 0; e < profile.EmptyBoxes; e++)
                boxes.Add(new int[0]);

            return new LevelDefinition { BoxCapacity = capacity, Boxes = boxes };
        }

        // A level must not start with a box that is already packed.
        private static bool HasClosedBox(Board board)
        {
            foreach (var box in board.Boxes)
            {
                if (box.IsClosed)
                    return true;
            }
            return false;
        }
    }
}
