using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace SweetBazaar.Core.Tests
{
    // Checks the real levels.json that ships with the game (built with tools\build-levels.ps1).
    public class ShippedLevelPackTests
    {
        private const int ExpectedMinimumLevelCount = 200;

        private static string PackPath =>
            Path.Combine(Application.dataPath, "_Game", "Resources", "Levels", "levels.json");

        private static LevelPack Load() => LevelPackJson.Parse(File.ReadAllText(PackPath));

        [Test]
        public void PackFile_ParsesAndEveryLevelIsValid()
        {
            var pack = Load();

            Assert.GreaterOrEqual(pack.Count, ExpectedMinimumLevelCount);
            Assert.IsEmpty(pack.Validate());
        }

        [Test]
        public void PackCanBeLoadedTheWayTheGameWillLoadIt()
        {
            var asset = Resources.Load<TextAsset>("Levels/levels");

            Assert.IsNotNull(asset, "Resources.Load could not find Levels/levels");
            Assert.AreEqual(Load().Count, LevelPackJson.Parse(asset.text).Count);
        }

        [Test]
        public void NoLevelStartsWithAPackedBoxOrIsAlreadyWon()
        {
            foreach (var record in Load().Levels)
            {
                var board = Board.FromLevel(record.Definition);

                Assert.IsFalse(board.IsWon, $"level {record.Number}");
                Assert.IsFalse(board.Boxes.Any(box => box.IsClosed), $"level {record.Number}");
            }
        }

        [Test]
        public void NoLevelUsesMoreCandyTypesThanTheArtBudget()
        {
            foreach (var record in Load().Levels)
            {
                int types = record.Definition.Boxes.SelectMany(box => box).Distinct().Count();

                Assert.LessOrEqual(types, LevelCurve.MaxCandyTypes, $"level {record.Number}");
            }
        }

        [Test]
        public void SampledLevels_AreSolvableWithTheStatedShortestSolution()
        {
            var pack = Load();

            foreach (int number in new[] { 1, 2, 3, 5, 10, 20, 50, 100, 150, 200 })
                AssertSolvesAsStated(pack.Get(number));
        }

        // The full check takes a few minutes: powershell -File tools\run-tests.ps1 -Filter AllLevels_AreSolvable
        [Test, Explicit("Solves every shipped level; takes a few minutes.")]
        public void AllLevels_AreSolvableWithTheStatedShortestSolution()
        {
            foreach (var record in Load().Levels)
                AssertSolvesAsStated(record);
        }

        private static void AssertSolvesAsStated(LevelRecord record)
        {
            var result = Solver.Solve(Board.FromLevel(record.Definition), new SolverOptions { MaxStates = 2000000 });

            Assert.AreEqual(SolveStatus.Solved, result.Status, $"level {record.Number}");
            Assert.AreEqual(record.MinMoves, result.Moves.Count, $"level {record.Number}");
        }
    }
}
