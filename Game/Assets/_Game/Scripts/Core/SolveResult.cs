using System;
using System.Collections.Generic;

namespace SweetBazaar.Core
{
    public enum SolveStatus
    {
        // A solution was found; Moves holds a shortest one.
        Solved,

        // The whole reachable space was searched and no position is won.
        Unsolvable,

        // The search hit the state limit before it could decide.
        Unknown
    }

    public sealed class SolveResult
    {
        private static readonly IReadOnlyList<Move> NoMoves = new Move[0];

        private SolveResult(SolveStatus status, IReadOnlyList<Move> moves, int statesExplored)
        {
            Status = status;
            Moves = moves;
            StatesExplored = statesExplored;
        }

        public SolveStatus Status { get; }

        // The shortest solution, in order; empty unless Status is Solved (and empty for a board that is already won).
        public IReadOnlyList<Move> Moves { get; }

        // Number of distinct positions the search discovered.
        public int StatesExplored { get; }

        internal static SolveResult Solved(IReadOnlyList<Move> moves, int statesExplored) =>
            new SolveResult(SolveStatus.Solved, moves ?? throw new ArgumentNullException(nameof(moves)), statesExplored);

        internal static SolveResult Unsolvable(int statesExplored) =>
            new SolveResult(SolveStatus.Unsolvable, NoMoves, statesExplored);

        internal static SolveResult Unknown(int statesExplored) =>
            new SolveResult(SolveStatus.Unknown, NoMoves, statesExplored);
    }
}
