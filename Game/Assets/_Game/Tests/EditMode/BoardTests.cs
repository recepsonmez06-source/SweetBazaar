using System;
using NUnit.Framework;
using static SweetBazaar.Core.Tests.TestBoards;

namespace SweetBazaar.Core.Tests
{
    public class BoardTests
    {
        [Test]
        public void Apply_MovesTheRunAndKeepsOrder()
        {
            var board = Cap4(new[] { 0, 1, 1 }, new[] { 1 });

            board.Apply(new Move(0, 1, 2));

            CollectionAssert.AreEqual(new[] { 0 }, board.Boxes[0].Candies);
            CollectionAssert.AreEqual(new[] { 1, 1, 1 }, board.Boxes[1].Candies);
        }

        [Test]
        public void Apply_ClosesTheTargetWhenItFillsWithOneType()
        {
            var board = Cap4(new[] { 0, 1 }, new[] { 1, 1, 1 });

            var outcome = board.Apply(new Move(0, 1, 1));

            Assert.IsTrue(outcome.TargetClosed);
            Assert.IsTrue(board.Boxes[1].IsClosed);
        }

        [Test]
        public void Apply_ReportsNoClosingWhenTargetStaysOpen()
        {
            var board = Cap4(new[] { 0, 1 }, new[] { 1 });

            var outcome = board.Apply(new Move(0, 1, 1));

            Assert.IsFalse(outcome.TargetClosed);
        }

        [Test]
        public void Apply_PartialMoveLeavesTheRestBehind()
        {
            var board = Cap4(new[] { 0, 1, 1, 1 }, new[] { 1, 1 });

            var outcome = board.Apply(new Move(0, 1, 2));

            CollectionAssert.AreEqual(new[] { 0, 1 }, board.Boxes[0].Candies);
            Assert.IsTrue(board.Boxes[1].IsClosed);
            Assert.IsTrue(outcome.TargetClosed);
        }

        [Test]
        public void Apply_IllegalMove_Throws()
        {
            var board = Cap4(new[] { 0, 1 }, new[] { 0 });

            Assert.Throws<ArgumentException>(() => board.Apply(new Move(0, 1, 1)));
        }

        [Test]
        public void Apply_MoveWithWrongCount_Throws()
        {
            var board = Cap4(new[] { 0, 1 }, None);

            Assert.Throws<ArgumentException>(() => board.Apply(new Move(0, 1, 5)));
        }

        [Test]
        public void Revert_RestoresTheBoardAndReopensAClosedBox()
        {
            var board = Cap4(new[] { 0, 1 }, new[] { 1, 1, 1 });
            string before = board.GetStateKey();
            var outcome = board.Apply(new Move(0, 1, 1));

            board.Revert(outcome);

            Assert.AreEqual(before, board.GetStateKey());
            Assert.IsFalse(board.Boxes[1].IsClosed);
            CollectionAssert.AreEqual(new[] { 0, 1 }, board.Boxes[0].Candies);
        }

        [Test]
        public void Create_ClosesBoxesThatStartFullAndSingleTyped()
        {
            var board = Cap4(new[] { 0, 0, 0, 0 }, new[] { 1, 1 });

            Assert.IsTrue(board.Boxes[0].IsClosed);
            Assert.IsFalse(board.Boxes[1].IsClosed);
        }

        [Test]
        public void IsWon_WhenEveryBoxIsEmptyOrClosed()
        {
            var board = Cap4(new[] { 0, 0, 0, 0 }, new[] { 1, 1, 1, 1 }, None);

            Assert.IsTrue(board.IsWon);
            Assert.IsFalse(board.IsStuck);
        }

        [Test]
        public void IsWon_IsFalseWhileACandyIsStillUnpacked()
        {
            var board = Cap4(new[] { 0, 0, 0 }, new[] { 0 }, None);

            Assert.IsFalse(board.IsWon);
        }

        [Test]
        public void IsStuck_WhenNotWonAndNoLegalMoveRemains()
        {
            var board = Cap4(new[] { 0, 1, 0, 1 }, new[] { 1, 0, 1, 0 });

            Assert.IsFalse(board.IsWon);
            Assert.IsTrue(board.IsStuck);
        }

        [Test]
        public void IsStuck_IsFalseWhileAMoveExists()
        {
            var board = Cap4(new[] { 0, 1, 0, 1 }, new[] { 1, 0, 1, 0 }, None);

            Assert.IsFalse(board.IsStuck);
        }

        [Test]
        public void AddBox_AppendsAnEmptyBoxAndRemoveLastBoxTakesItBack()
        {
            var board = Cap4(new[] { 0 });

            int index = board.AddBox(board.StandardCapacity);

            Assert.AreEqual(1, index);
            Assert.AreEqual(2, board.Boxes.Count);
            Assert.IsTrue(board.Boxes[1].IsEmpty);

            board.RemoveLastBox();

            Assert.AreEqual(1, board.Boxes.Count);
        }

        [Test]
        public void RemoveLastBox_RefusesANonEmptyBox()
        {
            var board = Cap4(new[] { 0 });

            Assert.Throws<InvalidOperationException>(() => board.RemoveLastBox());
        }

        [Test]
        public void Clone_IsIndependentOfTheOriginal()
        {
            var board = Cap4(new[] { 0, 1 }, None);
            var clone = board.Clone();

            board.Apply(new Move(0, 1, 1));

            CollectionAssert.AreEqual(new[] { 0, 1 }, clone.Boxes[0].Candies);
            Assert.IsTrue(clone.Boxes[1].IsEmpty);
        }

        [Test]
        public void StateKey_IgnoresBoxOrder()
        {
            var a = Cap4(new[] { 0, 1 }, new[] { 2 });
            var b = Cap4(new[] { 2 }, new[] { 0, 1 });

            Assert.AreEqual(a.GetStateKey(), b.GetStateKey());
        }

        [Test]
        public void StateKey_DiffersWhenCandyOrderInABoxDiffers()
        {
            var a = Cap4(new[] { 0, 1 }, new[] { 2 });
            var b = Cap4(new[] { 1, 0 }, new[] { 2 });

            Assert.AreNotEqual(a.GetStateKey(), b.GetStateKey());
        }
    }
}
