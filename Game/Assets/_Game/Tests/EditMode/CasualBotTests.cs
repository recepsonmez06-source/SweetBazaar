using System;
using NUnit.Framework;
using static SweetBazaar.Core.Tests.TestBoards;

namespace SweetBazaar.Core.Tests
{
    public class CasualBotTests
    {
        [Test]
        public void ABoardThatIsAlreadyWon_IsWon()
        {
            var board = Cap4(new[] { 0, 0, 0, 0 }, new[] { 1, 1, 1, 1 }, None);

            Assert.IsTrue(CasualBot.Wins(board, 1, 0));
            Assert.AreEqual(100, CasualBot.WinRatePercent(board, 10, 1));
        }

        [Test]
        public void ABoardWithoutAnyLegalMove_IsNeverWon()
        {
            var board = Cap4(new[] { 0, 1, 0, 1 }, new[] { 1, 0, 1, 0 });

            Assert.IsFalse(CasualBot.Wins(board, 1, 0));
            Assert.AreEqual(0, CasualBot.WinRatePercent(board, 10, 1));
        }

        [Test]
        public void AnEasyBoard_IsWonAlmostAlways()
        {
            // Two candy types and three empty boxes: even a casual player gets there.
            var board = Cap4(new[] { 0, 1, 0, 1 }, new[] { 1, 0, 1, 0 }, None, None, None);

            Assert.GreaterOrEqual(CasualBot.WinRatePercent(board, 50, 7), 90);
        }

        [Test]
        public void TheSameBoardAndSeed_GiveTheSameResult()
        {
            var board = Cap4(new[] { 0, 1, 2, 0 }, new[] { 1, 2, 0, 1 }, new[] { 2, 0, 1, 2 }, None, None);

            int first = CasualBot.WinRatePercent(board, 40, 99);
            int second = CasualBot.WinRatePercent(board, 40, 99);

            Assert.AreEqual(first, second);
        }

        [Test]
        public void ThePlayThroughs_DoNotChangeTheBoard()
        {
            var board = Cap4(new[] { 0, 1, 2, 0 }, new[] { 1, 2, 0, 1 }, new[] { 2, 0, 1, 2 }, None, None);
            string before = board.GetStateKey();

            CasualBot.WinRatePercent(board, 20, 5);

            Assert.AreEqual(before, board.GetStateKey());
        }

        [Test]
        public void LevelsWithOneEmptyBox_AreHarderForTheBotThanLevelsWithTwo()
        {
            // The whole difficulty system rests on this: taking away an empty box makes levels harder.
            int single = 0, double_ = 0;
            for (int seed = 1; seed <= 6; seed++)
            {
                var oneEmpty = new DifficultyProfile { CandyTypes = 6, EmptyBoxes = 1, MaxAttempts = 3000 };
                var twoEmpty = new DifficultyProfile { CandyTypes = 6, EmptyBoxes = 2, MaxAttempts = 3000 };

                single += LevelGenerator.MeasureWinRate(Board.FromLevel(LevelGenerator.Generate(oneEmpty, seed).Level), seed, 40);
                double_ += LevelGenerator.MeasureWinRate(Board.FromLevel(LevelGenerator.Generate(twoEmpty, seed).Level), seed, 40);
            }

            Assert.Less(single, double_);
        }

        [Test]
        public void AtLeastOneTrial_IsRequired()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CasualBot.WinRatePercent(Cap4(new[] { 0 }), 0, 1));
        }

        [Test]
        public void TheBotDoesNotChangeWhatTheGeneratorMeasures()
        {
            // MeasureWinRate is what levels.json stores; it must be a pure function of board, seed and trials.
            var board = Cap4(new[] { 0, 1, 2, 0 }, new[] { 1, 2, 0, 1 }, new[] { 2, 0, 1, 2 }, None, None);

            Assert.AreEqual(LevelGenerator.MeasureWinRate(board, 12, 30), LevelGenerator.MeasureWinRate(board, 12, 30));
        }
    }
}
