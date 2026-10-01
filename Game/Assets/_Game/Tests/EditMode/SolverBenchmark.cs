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
