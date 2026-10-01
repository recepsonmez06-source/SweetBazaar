using System;
using NUnit.Framework;
using static SweetBazaar.Core.Tests.TestBoards;

namespace SweetBazaar.Core.Tests
{
    public class SolverTests
    {
        [Test]
        public void AlreadyWonBoard_NeedsNoMoves()
        {
            var board = Cap2(new[] { 0, 0 }, new[] { 1, 1 }, None);

            var result = Solver.Solve(board);

            Assert.AreEqual(SolveStatus.Solved, result.Status);
            Assert.IsEmpty(result.Moves);
        }

        [Test]
        public void OneMoveFromWinning_ReturnsThatMove()
        {
            var board = Cap2(new[] { 0 }, new[] { 0 });

            var result = Solver.Solve(board);

            Assert.AreEqual(SolveStatus.Solved, result.Status);
            Assert.AreEqual(1, result.Moves.Count);
        }

        [Test]
        public void FindsTheShortestSolution()
        {
            // Worked out by hand: two moves cannot pack both types, three can.
            var board = Cap2(new[] { 0, 1 }, new[] { 1, 0 }, None);

            var result = Solver.Solve(board);

            Assert.AreEqual(SolveStatus.Solved, result.Status);
            Assert.AreEqual(3, result.Moves.Count);
        }

        [Test]
        public void ReturnedMoves_WinWhenPlayedInOrder()
        {
            var board = Board.Create(3, new[] { new[] { 0, 1, 0 }, new[] { 1, 0, 1 }, None });

            var result = Solver.Solve(board);

            Assert.AreEqual(SolveStatus.Solved, result.Status);
            // A five-move solution exists (0->2, 1->0, 1->2, 0->1, 0->2), so the shortest is at most that.
            Assert.LessOrEqual(result.Moves.Count, 5);

            var replay = board.Clone();
            foreach (var move in result.Moves)
                replay.Apply(move);
            Assert.IsTrue(replay.IsWon);
        }

        [Test]
        public void BoardWithNoLegalMove_IsUnsolvable()
        {
            var board = Cap2(new[] { 0, 1 }, new[] { 1, 0 });

            var result = Solver.Solve(board);

            Assert.AreEqual(SolveStatus.Unsolvable, result.Status);
            Assert.IsEmpty(result.Moves);
            Assert.AreEqual(1, result.StatesExplored);
        }

        [Test]
        public void CandyTypeThatCannotFillABox_IsUnsolvable()
        {
            // Only 3 candies of type 0 and 2 of type 1 exist, so no box can ever close,
            // even though plenty of moves are possible.
            var board = Cap4(new[] { 0, 0, 1 }, new[] { 1, 0 }, None);

            var result = Solver.Solve(board);

            Assert.AreEqual(SolveStatus.Unsolvable, result.Status);
            Assert.Greater(result.StatesExplored, 1);
        }

        [Test]
        public void DoesNotChangeTheBoardItIsGiven()
        {
            var board = Cap2(new[] { 0, 1 }, new[] { 1, 0 }, None);
            string before = board.GetStateKey();

            Solver.Solve(board);

            Assert.AreEqual(before, board.GetStateKey());
        }

        [Test]
        public void StateLimit_GivesUnknownInsteadOfAWrongAnswer()
        {
            var board = Cap2(new[] { 0, 1 }, new[] { 1, 0 }, None);

            var result = Solver.Solve(board, new SolverOptions { MaxStates = 2 });

            Assert.AreEqual(SolveStatus.Unknown, result.Status);
            Assert.IsEmpty(result.Moves);
        }

        [Test]
        public void MaxStatesBelowOne_IsRejected()
        {
            var board = Cap2(new[] { 0 }, new[] { 0 });

            Assert.Throws<ArgumentOutOfRangeException>(() => Solver.Solve(board, new SolverOptions { MaxStates = 0 }));
        }

        [Test]
        public void NullBoard_IsRejected()
        {
            Assert.Throws<ArgumentNullException>(() => Solver.Solve(null));
        }
    }
}
