using System;
using System.Linq;
using NUnit.Framework;

namespace SweetBazaar.Core.Tests
{
    public class LevelGeneratorTests
    {
        private static DifficultyProfile Profile(int types, int minMoves = 0, int maxMoves = int.MaxValue) =>
            new DifficultyProfile { CandyTypes = types, EmptyBoxes = 2, MinMoves = minMoves, MaxMoves = maxMoves };

        private static string Describe(LevelDefinition level) =>
            string.Join("|", level.Boxes.Select(box => string.Join(",", box)));

        [Test]
        public void SameProfileAndSeed_GiveTheSameLevel()
        {
            var first = LevelGenerator.Generate(Profile(5), seed: 7);
            var second = LevelGenerator.Generate(Profile(5), seed: 7);

            Assert.AreEqual(Describe(first.Level), Describe(second.Level));
            Assert.AreEqual(first.MinMoves, second.MinMoves);
        }

        [Test]
        public void DifferentSeeds_GiveDifferentLevels()
        {
            var descriptions = Enumerable.Range(1, 6)
                .Select(seed => Describe(LevelGenerator.Generate(Profile(5), seed).Level))
                .Distinct();

            Assert.Greater(descriptions.Count(), 1);
        }

        [Test]
        public void GeneratedLevel_IsValidAndHasTheStatedShortestSolution()
        {
            var profile = Profile(6);

            var generated = LevelGenerator.Generate(profile, seed: 3);

            Assert.IsEmpty(generated.Level.Validate());
            Assert.AreEqual(profile.CandyTypes + profile.EmptyBoxes, generated.Level.Boxes.Count);

            var solved = Solver.Solve(Board.FromLevel(generated.Level));
            Assert.AreEqual(SolveStatus.Solved, solved.Status);
            Assert.AreEqual(generated.MinMoves, solved.Moves.Count);
        }

        [Test]
        public void GeneratedLevel_RespectsTheMoveRange()
        {
            var profile = Profile(6, minMoves: 18, maxMoves: 24);

            var generated = LevelGenerator.Generate(profile, seed: 11);

            Assert.GreaterOrEqual(generated.MinMoves, 18);
            Assert.LessOrEqual(generated.MinMoves, 24);
        }

        [Test]
        public void GeneratedLevel_NeverStartsWithAPackedBox()
        {
            // Two types make a pre-packed box likely in a random shuffle, so this exercises the rejection.
            for (int seed = 1; seed <= 30; seed++)
            {
                var generated = LevelGenerator.Generate(Profile(2), seed);

                var board = Board.FromLevel(generated.Level);
                Assert.IsFalse(board.Boxes.Any(box => box.IsClosed), $"seed {seed}");
            }
        }

        [Test]
        public void ImpossibleMoveRange_ThrowsAfterTheAttemptsRunOut()
        {
            var profile = Profile(4, minMoves: 1000);
            profile.MaxAttempts = 5;

            Assert.Throws<InvalidOperationException>(() => LevelGenerator.Generate(profile, seed: 1));
        }

        [Test]
        public void InvalidProfile_IsRejected()
        {
            Assert.Throws<ArgumentException>(() => LevelGenerator.Generate(Profile(0), seed: 1));
            Assert.Throws<ArgumentException>(() => LevelGenerator.Generate(
                new DifficultyProfile { CandyTypes = 3, EmptyBoxes = 0 }, seed: 1));
            Assert.Throws<ArgumentException>(() => LevelGenerator.Generate(
                new DifficultyProfile { CandyTypes = 3, MinMoves = 10, MaxMoves = 5 }, seed: 1));
            Assert.Throws<ArgumentNullException>(() => LevelGenerator.Generate(null, seed: 1));
        }

        [Test]
        public void ProfileValidation_ListsTheProblems()
        {
            var profile = new DifficultyProfile { CandyTypes = 0, EmptyBoxes = 0 };

            Assert.GreaterOrEqual(profile.Validate().Count, 2);
            Assert.IsEmpty(new DifficultyProfile().Validate());
        }
    }
}
