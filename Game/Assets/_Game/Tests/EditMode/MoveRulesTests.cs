using NUnit.Framework;
using static SweetBazaar.Core.Tests.TestBoards;

namespace SweetBazaar.Core.Tests
{
    public class MoveRulesTests
    {
        [Test]
        public void Move_ToEmptyBox_IsAllowed()
        {
            var board = Cap4(new[] { 0, 1 }, None);

            Assert.IsTrue(MoveRules.TryCreateMove(board, 0, 1, out var move));
            Assert.AreEqual(new Move(0, 1, 1), move);
        }

        [Test]
        public void Move_OntoSameTypeOnTop_IsAllowed()
        {
            var board = Cap4(new[] { 0, 1 }, new[] { 1 });

            Assert.IsTrue(MoveRules.TryCreateMove(board, 0, 1, out var move));
            Assert.AreEqual(new Move(0, 1, 1), move);
        }

        [Test]
        public void Move_OntoDifferentType_IsRejected()
        {
            var board = Cap4(new[] { 0, 1 }, new[] { 0 });

            Assert.IsFalse(MoveRules.TryCreateMove(board, 0, 1, out _));
        }

        [Test]
        public void Move_TakesTheWholeRunOfIdenticalCandiesOnTop()
        {
            var board = Cap4(new[] { 0, 1, 1 }, None);

            Assert.IsTrue(MoveRules.TryCreateMove(board, 0, 1, out var move));
            Assert.AreEqual(2, move.Count);
        }

        [Test]
        public void Move_IsPartialWhenTargetHasLessRoomThanTheRun()
        {
            // Run of 3 on top of box 0, but box 1 only has 2 free slots.
            var board = Cap4(new[] { 0, 1, 1, 1 }, new[] { 1, 1 });

            Assert.IsTrue(MoveRules.TryCreateMove(board, 0, 1, out var move));
            Assert.AreEqual(new Move(0, 1, 2), move);
        }

        [Test]
        public void Move_FromEmptyBox_IsRejected()
        {
            var board = Cap4(None, new[] { 0 });

            Assert.IsFalse(MoveRules.TryCreateMove(board, 0, 1, out _));
        }

        [Test]
        public void Move_ToFullBox_IsRejectedEvenWithMatchingTop()
        {
            var board = Cap4(new[] { 1 }, new[] { 0, 1, 0, 1 });

            Assert.IsFalse(MoveRules.TryCreateMove(board, 0, 1, out _));
        }

        [Test]
        public void Move_ToSameBox_IsRejected()
        {
            var board = Cap4(new[] { 0, 1 }, None);

            Assert.IsFalse(MoveRules.TryCreateMove(board, 0, 0, out _));
        }

        [Test]
        public void Move_WithInvalidBoxNumbers_IsRejected()
        {
            var board = Cap4(new[] { 0 }, None);

            Assert.IsFalse(MoveRules.TryCreateMove(board, -1, 1, out _));
            Assert.IsFalse(MoveRules.TryCreateMove(board, 0, -1, out _));
            Assert.IsFalse(MoveRules.TryCreateMove(board, 0, 2, out _));
            Assert.IsFalse(MoveRules.TryCreateMove(board, 5, 0, out _));
        }

        [Test]
        public void Move_FromClosedBox_IsRejected()
        {
            var board = Cap4(new[] { 0, 0, 0, 0 }, None);

            Assert.IsFalse(MoveRules.TryCreateMove(board, 0, 1, out _));
        }

        [Test]
        public void Move_ToClosedBox_IsRejected()
        {
            var board = Cap4(new[] { 0 }, new[] { 0, 0, 0, 0 });

            Assert.IsFalse(MoveRules.TryCreateMove(board, 0, 1, out _));
        }

        [Test]
        public void GetLegalMoves_ListsEveryAllowedMove()
        {
            // 0 and 1 can each go to the empty box 2; they cannot go onto each other.
            var board = Cap4(new[] { 0 }, new[] { 1 }, None);

            var moves = MoveRules.GetLegalMoves(board);

            CollectionAssert.AreEquivalent(new[] { new Move(0, 2, 1), new Move(1, 2, 1) }, moves);
        }

        [Test]
        public void HasAnyLegalMove_IsFalseWhenNothingIsAllowed()
        {
            var board = Cap4(new[] { 0, 1, 0, 1 }, new[] { 1, 0, 1, 0 });

            Assert.IsFalse(MoveRules.HasAnyLegalMove(board));
            Assert.IsEmpty(MoveRules.GetLegalMoves(board));
        }
    }
}
