using System.Collections.Generic;

namespace SweetBazaar.Core
{
    // Plain data describing a level's starting position. Reading it from JSON happens outside Core.
    public sealed class LevelDefinition
    {
        public int BoxCapacity { get; set; } = 4;

        // One entry per box, candies listed bottom -> top; an empty array is an empty box.
        public List<int[]> Boxes { get; set; } = new List<int[]>();

        // Returns the problems found; an empty list means the definition is valid.
        // Whether the level is actually solvable is the solver's job, not checked here.
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();

            if (BoxCapacity < 1)
            {
                errors.Add("BoxCapacity must be at least 1.");
                return errors;
            }
            if (Boxes == null || Boxes.Count == 0)
            {
                errors.Add("A level needs at least one box.");
                return errors;
            }

            var counts = new SortedDictionary<int, int>();
            for (int i = 0; i < Boxes.Count; i++)
            {
                var box = Boxes[i];
                if (box == null)
                {
                    errors.Add($"Box {i} is null.");
                    continue;
                }
                if (box.Length > BoxCapacity)
                    errors.Add($"Box {i} holds {box.Length} candies but the capacity is {BoxCapacity}.");

                foreach (var candy in box)
                {
                    if (candy < 0)
                    {
                        errors.Add($"Box {i} contains the invalid candy id {candy}.");
                        continue;
                    }
                    counts.TryGetValue(candy, out int seen);
                    counts[candy] = seen + 1;
                }
            }

            // Every candy type must be able to fill whole boxes, otherwise the level cannot be won.
            foreach (var pair in counts)
            {
                if (pair.Value % BoxCapacity != 0)
                    errors.Add($"Candy {pair.Key} appears {pair.Value} times; it must be a multiple of {BoxCapacity}.");
            }

            return errors;
        }
    }
}
