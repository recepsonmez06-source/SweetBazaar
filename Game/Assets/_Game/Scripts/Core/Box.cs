using System;
using System.Collections.Generic;

namespace SweetBazaar.Core
{
    // One candy box. Candies are stored bottom -> top and are plain int ids (>= 0).
    // A box that is full and holds a single candy type is "closed" (packed): it is frozen and counts as done.
    // Boxes are changed only through Board, so the public surface here is read-only.
    public sealed class Box
    {
        // Value of Top for an empty box.
        public const int NoCandy = -1;

        private readonly List<int> _candies;

        public Box(int capacity, IEnumerable<int> candies = null)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be at least 1.");

            Capacity = capacity;
            _candies = new List<int>(capacity);

            if (candies == null)
                return;

            foreach (var candy in candies)
            {
                if (candy < 0)
                    throw new ArgumentException("Candy ids must be >= 0.", nameof(candies));
                if (_candies.Count == capacity)
                    throw new ArgumentException("More candies than the box capacity.", nameof(candies));
                _candies.Add(candy);
            }
        }

        public int Capacity { get; }
        public int Count => _candies.Count;
        public bool IsEmpty => _candies.Count == 0;
        public bool IsFull => _candies.Count == Capacity;
        public int FreeSlots => Capacity - _candies.Count;
        public IReadOnlyList<int> Candies => _candies;

        public int Top => _candies.Count == 0 ? NoCandy : _candies[_candies.Count - 1];

        public bool IsClosed
        {
            get
            {
                if (!IsFull)
                    return false;
                for (int i = 1; i < _candies.Count; i++)
                {
                    if (_candies[i] != _candies[0])
                        return false;
                }
                return true;
            }
        }

        // How many identical candies sit on top of the box (0 for an empty box).
        public int TopRunLength
        {
            get
            {
                if (_candies.Count == 0)
                    return 0;

                int top = Top;
                int length = 0;
                for (int i = _candies.Count - 1; i >= 0 && _candies[i] == top; i--)
                    length++;
                return length;
            }
        }

        public Box Clone() => new Box(Capacity, _candies);

        internal void Push(int candy) => _candies.Add(candy);

        internal void Pop() => _candies.RemoveAt(_candies.Count - 1);
    }
}
