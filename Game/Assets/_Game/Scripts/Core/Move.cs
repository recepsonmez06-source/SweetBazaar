using System;

namespace SweetBazaar.Core
{
    // "Move Count candies from box From to box To". Box numbers are indexes into Board.Boxes.
    public readonly struct Move : IEquatable<Move>
    {
        public Move(int from, int to, int count)
        {
            From = from;
            To = to;
            Count = count;
        }

        public int From { get; }
        public int To { get; }
        public int Count { get; }

        public bool Equals(Move other) => From == other.From && To == other.To && Count == other.Count;

        public override bool Equals(object obj) => obj is Move other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(From, To, Count);

        public override string ToString() => $"{From}->{To} x{Count}";
    }

    // What happened when a move was applied; the view uses it to pick animations (e.g. packing).
    public readonly struct MoveOutcome
    {
        public MoveOutcome(Move move, bool targetClosed)
        {
            Move = move;
            TargetClosed = targetClosed;
        }

        public Move Move { get; }

        // True if this move filled the target box with a single type, so it closed.
        public bool TargetClosed { get; }
    }
}
