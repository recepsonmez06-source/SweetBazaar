using System;
using System.Collections.Generic;

namespace SweetBazaar.Core
{
    // The whole counter: all boxes and the rules to change them.
    // Game code should normally go through GameSession, which adds undo history on top of this.
    public sealed class Board
    {
        private readonly List<Box> _boxes;

        private Board(int standardCapacity, List<Box> boxes)
        {
            StandardCapacity = standardCapacity;
            _boxes = boxes;
        }

        // Capacity used for boxes added later (e.g. the extra empty box).
        public int StandardCapacity { get; }

        public IReadOnlyList<Box> Boxes => _boxes;

        // Every box is either empty or closed.
        public bool IsWon
        {
            get
            {
                foreach (var box in _boxes)
                {
                    if (!box.IsEmpty && !box.IsClosed)
                        return false;
                }
                return true;
            }
        }

        // No legal move left on a board that is not won.
        public bool IsStuck => !IsWon && !MoveRules.HasAnyLegalMove(this);

        // boxContents: candies of each box, bottom -> top. Boxes that start full and
        // single-typed are closed from the start.
        public static Board Create(int capacity, IEnumerable<int[]> boxContents)
        {
            if (boxContents == null)
                throw new ArgumentNullException(nameof(boxContents));

            var boxes = new List<Box>();
            foreach (var contents in boxContents)
                boxes.Add(new Box(capacity, contents));
            return new Board(capacity, boxes);
        }

        public static Board FromLevel(LevelDefinition level)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));

            var errors = level.Validate();
            if (errors.Count > 0)
                throw new ArgumentException("Invalid level: " + string.Join("; ", errors), nameof(level));

            return Create(level.BoxCapacity, level.Boxes);
        }

        // Throws ArgumentException if the move is not exactly what MoveRules would produce.
        public MoveOutcome Apply(Move move)
        {
            if (!MoveRules.TryCreateMove(this, move.From, move.To, out var legal) || !legal.Equals(move))
                throw new ArgumentException($"Illegal move: {move}", nameof(move));

            var source = _boxes[move.From];
            var target = _boxes[move.To];
            int candy = source.Top;

            for (int i = 0; i < move.Count; i++)
            {
                source.Pop();
                target.Push(candy);
            }

            return new MoveOutcome(move, target.IsClosed);
        }

        // Undoes a move. Moves must be reverted in reverse order of how they were applied.
        public void Revert(MoveOutcome outcome)
        {
            var move = outcome.Move;
            var source = _boxes[move.From];
            var target = _boxes[move.To];

            if (target.Count < move.Count || source.FreeSlots < move.Count)
                throw new InvalidOperationException($"Cannot revert {move}: the board changed since it was applied.");

            int candy = target.Top;
            for (int i = 0; i < move.Count; i++)
            {
                target.Pop();
                source.Push(candy);
            }
        }

        // Adds an empty box at the end and returns its index.
        public int AddBox(int capacity)
        {
            _boxes.Add(new Box(capacity));
            return _boxes.Count - 1;
        }

        public void RemoveLastBox()
        {
            if (_boxes.Count == 0 || !_boxes[_boxes.Count - 1].IsEmpty)
                throw new InvalidOperationException("Only an empty last box can be removed.");

            _boxes.RemoveAt(_boxes.Count - 1);
        }

        public Board Clone()
        {
            var boxes = new List<Box>(_boxes.Count);
            foreach (var box in _boxes)
                boxes.Add(box.Clone());
            return new Board(StandardCapacity, boxes);
        }

        // Identifies a position independently of box order: boards that differ only by
        // which box sits where get the same key. The solver uses it to spot repeated positions.
        public string GetStateKey()
        {
            var parts = new string[_boxes.Count];
            for (int i = 0; i < parts.Length; i++)
                parts[i] = _boxes[i].Capacity + ":" + string.Join(",", _boxes[i].Candies);

            Array.Sort(parts, StringComparer.Ordinal);
            return string.Join("|", parts);
        }
    }
}
