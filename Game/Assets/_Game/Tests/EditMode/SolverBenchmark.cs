using System;
using System.Diagnostics;
using NUnit.Framework;

namespace SweetBazaar.Core.Tests
{
    // Not part of the normal test run. Prints how the solver scales on random boards of growing size:
    //   powershell -File tools\run-tests.ps1 -Filter SolverBenchmark   (then read the Output of the result file)
    public class SolverBenchmark
    {
        private const int Capacity = 4;
        private const int EmptyBoxes = 2;

        [Test, Explicit("Prints timings only; run on demand.")]
        public void PrintSolveTimes()
        {
            foreach (int types in new[] { 4, 5, 6, 7, 8, 10 })
            {
                for (int seed = 1; seed <= 3; seed++)
                {
                    var board = RandomBoard(types, seed);
                    var watch = Stopwatch.StartNew();
                    var result = Solver.Solve(board, new SolverOptions { MaxStates = 300000 });
                    watch.Stop();

                    TestContext.WriteLine(
                        $"types={types,2} seed={seed} status={result.Status,-10} moves={result.Moves.Count,3} " +
                        $"states={result.StatesExplored,8} time={watch.ElapsedMilliseconds,6} ms");
                }
            }
        }

        [Test, Explicit("Prints timings only; run on demand.")]
        public void PrintLevelCurveGenerationTimes()
        {
            var total = Stopwatch.StartNew();
            for (int level = 1; level <= 120; level++)
            {
                var watch = Stopwatch.StartNew();
                var profile = LevelCurve.GetProfile(level);
                try
                {
                    var generated = LevelGenerator.GenerateForLevel(level);
                    TestContext.WriteLine(
                        $"level={level,3} types={profile.CandyTypes,2} empty={profile.EmptyBoxes} " +
                        $"minMoves={generated.MinMoves,3} attempts={generated.Attempts,3} time={watch.ElapsedMilliseconds,6} ms");
                }
                catch (InvalidOperationException e)
                {
                    TestContext.WriteLine($"level={level,3} types={profile.CandyTypes,2} FAILED: {e.Message} time={watch.ElapsedMilliseconds} ms");
                }
            }
            TestContext.WriteLine($"total: {total.Elapsed.TotalSeconds:F1} s");
        }

        [Test, Explicit("Prints timings only; run on demand.")]
        public void PrintHardProfileGenerationTimes()
        {
            // One empty box: most random deals are unsolvable, so many attempts are needed.
            foreach (int types in new[] { 6, 8, 10, 12 })
            {
                for (int seed = 1; seed <= 2; seed++)
                {
                    var profile = new DifficultyProfile
                    {
                        CandyTypes = types, EmptyBoxes = 1, MaxAttempts = 3000, SolverStateLimit = 300000
                    };
                    var watch = Stopwatch.StartNew();
                    try
                    {
                        var generated = LevelGenerator.Generate(profile, seed);
                        TestContext.WriteLine(
                            $"1 empty box: types={types,2} seed={seed} minMoves={generated.MinMoves,3} " +
                            $"attempts={generated.Attempts} time={watch.ElapsedMilliseconds,6} ms");
                    }
                    catch (InvalidOperationException)
                    {
                        TestContext.WriteLine(
                            $"1 empty box: types={types,2} seed={seed} no level accepted, time={watch.ElapsedMilliseconds,6} ms");
                    }
                }
            }
        }

        private static Board RandomBoard(int types, int seed)
        {
            var random = new Random(seed);
            var candies = new int[types * Capacity];
            for (int i = 0; i < candies.Length; i++)
                candies[i] = i / Capacity;

            for (int i = candies.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (candies[i], candies[j]) = (candies[j], candies[i]);
            }

            var boxes = new int[types + EmptyBoxes][];
            for (int b = 0; b < types; b++)
            {
                boxes[b] = new int[Capacity];
                Array.Copy(candies, b * Capacity, boxes[b], 0, Capacity);
            }
            for (int b = types; b < boxes.Length; b++)
                boxes[b] = new int[0];

            return Board.Create(Capacity, boxes);
        }
    }
}
