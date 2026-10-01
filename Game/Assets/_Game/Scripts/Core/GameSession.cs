using System;
using System.Collections.Generic;

namespace SweetBazaar.Core
{
    // What an undo took back: either a move or an added box.
    public readonly struct UndoInfo
    {
        public UndoInfo(MoveOutcome undoneMove, bool wasAddedBox, int removedBoxIndex)
        {
            UndoneMove = undoneMove;
            WasAddedBox = wasAddedBox;
            RemovedBoxIndex = removedBoxIndex;
        }

        // The move that was reverted (meaningful when WasAddedBox is false).
        public MoveOutcome UndoneMove { get; }

        public bool WasAddedBox { get; }

        // Index the removed extra box had (meaningful when WasAddedBox is true).
        public int RemovedBoxIndex { get; }
    }

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
        public bool Undo() => TryUndo(out _);

        // Like Undo, and tells what was undone so a view can animate it.
        public bool TryUndo(out UndoInfo info)
        {
            info = default;
            if (_history.Count == 0)
                return false;

            var entry = _history.Pop();
            if (entry.AddedBox)
            {
                int index = Board.Boxes.Count - 1;
                Board.RemoveLastBox();
                info = new UndoInfo(default, wasAddedBox: true, removedBoxIndex: index);
            }
            else
            {
                Board.Revert(entry.Outcome);
                info = new UndoInfo(entry.Outcome, wasAddedBox: false, removedBoxIndex: -1);
            }
            return true;
        }
    }
}
