using NUnit.Framework;
using static SweetBazaar.Core.Tests.TestBoards;

namespace SweetBazaar.Core.Tests
{
    public class GameSessionTests
    {
        [Test]
        public void TryMove_LegalMove_ChangesTheBoardAndCanBeUndone()
        {
            var session = new GameSession(Cap4(new[] { 0, 1 }, None));

            Assert.IsTrue(session.TryMove(0, 1, out var outcome));

            Assert.AreEqual(new Move(0, 1, 1), outcome.Move);
            Assert.IsTrue(session.CanUndo);
            CollectionAssert.AreEqual(new[] { 1 }, session.Board.Boxes[1].Candies);
        }

        [Test]
        public void TryMove_IllegalMove_ChangesNothing()
        {
            var session = new GameSession(Cap4(new[] { 0, 1 }, new[] { 0 }));
            string before = session.Board.GetStateKey();

            Assert.IsFalse(session.TryMove(0, 1, out _));

            Assert.AreEqual(before, session.Board.GetStateKey());
            Assert.IsFalse(session.CanUndo);
        }

        [Test]
        public void Undo_RestoresThePreviousPosition()
        {
            var session = new GameSession(Cap4(new[] { 0, 1 }, new[] { 1 }));
            string before = session.Board.GetStateKey();
            session.TryMove(0, 1, out _);

            Assert.IsTrue(session.Undo());

            Assert.AreEqual(before, session.Board.GetStateKey());
            Assert.IsFalse(session.CanUndo);
        }

        [Test]
        public void Undo_ReopensABoxThatTheMoveHadClosed()
        {
            var session = new GameSession(Cap4(new[] { 0, 1 }, new[] { 1, 1, 1 }));
            session.TryMove(0, 1, out var outcome);
            Assert.IsTrue(outcome.TargetClosed);

            session.Undo();

            Assert.IsFalse(session.Board.Boxes[1].IsClosed);
        }

        [Test]
        public void Undo_WithNothingToUndo_ReturnsFalse()
        {
            var session = new GameSession(Cap4(new[] { 0 }, None));

            Assert.IsFalse(session.Undo());
        }

        [Test]
        public void AddEmptyBox_AddsABoxOfStandardCapacityAndCanBeUndone()
        {
            var session = new GameSession(Cap4(new[] { 0 }));

            int index = session.AddEmptyBox();

            Assert.AreEqual(1, index);
            Assert.AreEqual(4, session.Board.Boxes[1].Capacity);

            session.Undo();

            Assert.AreEqual(1, session.Board.Boxes.Count);
        }

        [Test]
        public void Undo_UnwindsMovesAndAddedBoxesInOrder()
        {
            var session = new GameSession(Cap4(new[] { 0, 1 }, new[] { 0 }));
            string start = session.Board.GetStateKey();

            int extra = session.AddEmptyBox();
            session.TryMove(0, extra, out _);

            Assert.IsTrue(session.Undo());
            Assert.IsTrue(session.Undo());
            Assert.AreEqual(start, session.Board.GetStateKey());
            Assert.IsFalse(session.CanUndo);
        }

        [Test]
        public void TryUndo_ReportsTheMoveThatWasTakenBack()
        {
            var session = new GameSession(Cap4(new[] { 0, 1 }, None));
            session.TryMove(0, 1, out var made);

            Assert.IsTrue(session.TryUndo(out var info));

            Assert.IsFalse(info.WasAddedBox);
            Assert.AreEqual(made.Move, info.UndoneMove.Move);
        }

        [Test]
        public void TryUndo_ReportsTheAddedBoxThatWasTakenBack()
        {
            var session = new GameSession(Cap4(new[] { 0 }));
            int index = session.AddEmptyBox();

            Assert.IsTrue(session.TryUndo(out var info));

            Assert.IsTrue(info.WasAddedBox);
            Assert.AreEqual(index, info.RemovedBoxIndex);
        }

        [Test]
        public void NextUndoIsAddedBox_TellsWhatTheNextUndoWouldTakeBack()
        {
            var session = new GameSession(Cap4(new[] { 0, 1 }, None));
            Assert.IsFalse(session.NextUndoIsAddedBox, "nothing to undo yet");

            session.TryMove(0, 1, out _);
            Assert.IsFalse(session.NextUndoIsAddedBox, "the last thing was a move");

            session.AddEmptyBox();
            Assert.IsTrue(session.NextUndoIsAddedBox, "the last thing was an added box");

            session.Undo();
            Assert.IsFalse(session.NextUndoIsAddedBox, "back to the move");
        }

        [Test]
        public void TryUndo_WithNothingToUndo_ReturnsFalse()
        {
            var session = new GameSession(Cap4(new[] { 0 }));

            Assert.IsFalse(session.TryUndo(out var info));
            Assert.IsFalse(info.WasAddedBox);
        }

        [Test]
        public void ASmallLevel_CanBePlayedToAWin()
        {
            // Capacity 2: [0,1] [1,0] [] -> three moves pack both types.
            var session = new GameSession(Cap2(new[] { 0, 1 }, new[] { 1, 0 }, None));

            Assert.IsTrue(session.TryMove(0, 2, out _));
            Assert.IsFalse(session.Board.IsWon);

            Assert.IsTrue(session.TryMove(1, 0, out var packFirst));
            Assert.IsTrue(packFirst.TargetClosed);

            Assert.IsTrue(session.TryMove(2, 1, out var packSecond));
            Assert.IsTrue(packSecond.TargetClosed);

            Assert.IsTrue(session.Board.IsWon);
            Assert.IsFalse(session.Board.IsStuck);
        }
    }
}
