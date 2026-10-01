using System.Collections.Generic;

namespace SweetBazaar.Core
{
    // What a generated level should look like. All numbers are data: tune them, not the generator.
    public sealed class DifficultyProfile
    {
        public int CandyTypes { get; set; } = 3;

        // Empty boxes next to the CandyTypes filled ones. Fewer empty boxes make a level harder.
        public int EmptyBoxes { get; set; } = 2;

        public int BoxCapacity { get; set; } = 4;

        // Accepted range for the shortest solution (see Solver). Used as the difficulty measure.
        public int MinMoves { get; set; }
        public int MaxMoves { get; set; } = int.MaxValue;

        // Solver limit per candidate; a candidate the solver cannot decide within it is discarded.
        public int SolverStateLimit { get; set; } = 300000;

        // Candidates tried before generation gives up. With a single empty box only about one random deal in
        // eight is solvable (and fewer with many candy types), so such profiles need far more attempts.
        public int MaxAttempts { get; set; } = 200;

        // Returns the problems found; an empty list means the profile is usable.
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();

            if (CandyTypes < 1)
                errors.Add("CandyTypes must be at least 1.");
            if (EmptyBoxes < 1)
                errors.Add("EmptyBoxes must be at least 1, otherwise the board starts without any move.");
            if (BoxCapacity < 2)
                errors.Add("BoxCapacity must be at least 2.");
            if (MinMoves < 0)
                errors.Add("MinMoves must not be negative.");
            if (MaxMoves < MinMoves)
                errors.Add("MaxMoves must not be below MinMoves.");
            if (SolverStateLimit < 1)
                errors.Add("SolverStateLimit must be at least 1.");
            if (MaxAttempts < 1)
                errors.Add("MaxAttempts must be at least 1.");

            return errors;
        }
    }
}
