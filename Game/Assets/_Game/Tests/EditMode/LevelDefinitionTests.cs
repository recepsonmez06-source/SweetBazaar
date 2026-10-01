using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SweetBazaar.Core.Tests
{
    public class LevelDefinitionTests
    {
        private static LevelDefinition Level(int capacity, params int[][] boxes) =>
            new LevelDefinition { BoxCapacity = capacity, Boxes = new List<int[]>(boxes) };

        [Test]
        public void ValidLevel_HasNoErrors()
        {
            var level = Level(2, new[] { 0, 1 }, new[] { 1, 0 }, new int[0]);

            Assert.IsEmpty(level.Validate());
        }

        [Test]
        public void CandyCountNotAMultipleOfCapacity_IsReported()
        {
            // Three candies of type 0 can never fill a box of 4.
            var level = Level(4, new[] { 0, 0, 0 }, new int[0]);

            Assert.That(level.Validate(), Has.Some.Contains("Candy 0"));
        }

        [Test]
        public void OverfilledBox_IsReported()
        {
            var level = Level(2, new[] { 0, 0, 0 }, new int[0]);

            Assert.That(level.Validate(), Has.Some.Contains("Box 0"));
        }

        [Test]
        public void NegativeCandyId_IsReported()
        {
            var level = Level(2, new[] { 0, -1 }, new int[0]);

            Assert.That(level.Validate(), Has.Some.Contains("invalid candy id"));
        }

        [Test]
        public void LevelWithoutBoxes_IsReported()
        {
            Assert.IsNotEmpty(Level(4).Validate());
        }

        [Test]
        public void InvalidCapacity_IsReported()
        {
            Assert.IsNotEmpty(Level(0, new[] { 0 }).Validate());
        }

        [Test]
        public void FromLevel_BuildsTheBoardFromAValidLevel()
        {
            var level = Level(2, new[] { 0, 1 }, new[] { 1, 0 }, new int[0]);

            var board = Board.FromLevel(level);

            Assert.AreEqual(3, board.Boxes.Count);
            Assert.AreEqual(2, board.StandardCapacity);
            CollectionAssert.AreEqual(new[] { 1, 0 }, board.Boxes[1].Candies);
        }

        [Test]
        public void FromLevel_ThrowsForAnInvalidLevel()
        {
            var level = Level(4, new[] { 0, 0, 0 }, new int[0]);

            Assert.Throws<ArgumentException>(() => Board.FromLevel(level));
        }
    }
}
