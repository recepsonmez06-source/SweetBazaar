using System;
using System.Collections.Generic;

namespace SweetBazaar.Core
{
    // A board plus the history needed for undo. Limits on undo and the extra box
    // (rewarded ads, free uses) are policy for the game layer, not enforced here.
    public sealed class GameSession
    {
        private readonly struct HistoryEntry
        {
            public HistoryEntry(MoveOutcome outcome, bool addedBox)
            {
                Outcome = outcome;
                AddedBox = addedBox;
            }

            public MoveOutcome Outcome { get; }
            public bool AddedBox { get; }
        }

        private readonly Stack<HistoryEntry> _history = new Stack<HistoryEntry>();

        public GameSession(Board board)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
        }

        public Board Board { get; }

        public bool CanUndo => _history.Count > 0;

        // Makes the move if the rules allow it; returns false and changes nothing otherwise.
        public bool TryMove(int from, int to, out MoveOutcome outcome)
        {
            outcome = default;
            if (!MoveRules.TryCreateMove(Board, from, to, out var move))
                return false;

            outcome = Board.Apply(move);
            _history.Push(new HistoryEntry(outcome, addedBox: false));
            return true;
        }

        // Adds an empty box of the standard capacity and returns its index. Can be undone.
        public int AddEmptyBox()
        {
            int index = Board.AddBox(Board.StandardCapacity);
            _history.Push(new HistoryEntry(default, addedBox: true));
            return index;
        }

        // Reverts the latest move or added box; returns false if there is nothing to undo.
        public bool Undo()
        {
            if (_history.Count == 0)
                return false;

            var entry = _history.Pop();
            if (entry.AddedBox)
                Board.RemoveLastBox();
            else
                Board.Revert(entry.Outcome);
            return true;
        }
    }
}
