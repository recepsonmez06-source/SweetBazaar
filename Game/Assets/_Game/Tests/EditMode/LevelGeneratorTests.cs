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
        public void WithATargetWinRate_TheAcceptedLevelLandsInsideTheBand()
        {
            var profile = new DifficultyProfile
            {
                CandyTypes = 5, EmptyBoxes = 2, TargetWinRate = 70, WinRateTolerance = 6, MaxAttempts = 2000,
            };

            var generated = LevelGenerator.Generate(profile, seed: 21);

            Assert.That(generated.WinRate, Is.InRange(64, 76));
            // the stored number is exactly what measuring the level again gives
            var board = Board.FromLevel(generated.Level);
            Assert.AreEqual(generated.WinRate, LevelGenerator.MeasureWinRate(board, generated.Seed, profile.BotTrials));
        }

        [Test]
        public void WithoutATargetWinRate_TheDifficultyIsNotMeasured()
        {
            var generated = LevelGenerator.Generate(Profile(4), seed: 2);

            Assert.AreEqual(-1, generated.WinRate);
        }

        [Test]
        public void ATargetNoLevelCanMeet_ThrowsAfterTheAttemptsRunOut()
        {
            // 2 candy types with 3 empty boxes are won by the bot every time; asking for 0% is impossible.
            var profile = new DifficultyProfile
            {
                CandyTypes = 2, EmptyBoxes = 3, TargetWinRate = 0, WinRateTolerance = 0, MaxAttempts = 5,
            };

            Assert.Throws<InvalidOperationException>(() => LevelGenerator.Generate(profile, seed: 1));
        }

        [Test]
        public void ProfileValidation_ChecksTheDifficultySettings()
        {
            Assert.IsNotEmpty(new DifficultyProfile { TargetWinRate = 101 }.Validate());
            Assert.IsNotEmpty(new DifficultyProfile { WinRateTolerance = -1 }.Validate());
            Assert.IsNotEmpty(new DifficultyProfile { BotTrials = 10, BotPrefilterTrials = 20 }.Validate());
            Assert.IsEmpty(new DifficultyProfile { TargetWinRate = 50 }.Validate());
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
