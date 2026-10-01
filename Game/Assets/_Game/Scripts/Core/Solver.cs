using System;
using System.Collections.Generic;

namespace SweetBazaar.Core
{
    public static class Solver
    {
        // Breadth-first search over positions. Every move costs one, so the first winning position
        // found is reached by a shortest solution. A position is visited once (box order ignored).
        // The given board is not changed.
        public static SolveResult Solve(Board board, SolverOptions options = null)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            options = options ?? new SolverOptions();
            if (options.MaxStates < 1)
                throw new ArgumentOutOfRangeException(nameof(options), "MaxStates must be at least 1.");

            var start = board.Clone();
            if (start.IsWon)
                return SolveResult.Solved(new Move[0], 1);

            var visited = new HashSet<string> { start.GetStateKey() };

            // Search tree for rebuilding the path: node i was reached from parents[i] by movesTo[i].
            var parents = new List<int> { -1 };
            var movesTo = new List<Move> { default };

            var queue = new Queue<(Board board, int node)>();
            queue.Enqueue((start, 0));

            var legalMoves = new List<Move>();

            while (queue.Count > 0)
            {
                var (current, node) = queue.Dequeue();
                MoveRules.GetLegalMoves(current, legalMoves);

                foreach (var move in legalMoves)
                {
                    if (IsRelocation(current, move))
                        continue;

                    // Try the move on the board itself and undo it, so only new positions get cloned.
                    var outcome = current.Apply(move);

                    if (!visited.Add(current.GetStateKey()))
                    {
                        current.Revert(outcome);
                        continue;
                    }

                    if (visited.Count > options.MaxStates)
                        return SolveResult.Unknown(visited.Count - 1);

                    int child = parents.Count;
                    parents.Add(node);
                    movesTo.Add(move);

                    if (current.IsWon)
                        return SolveResult.Solved(BuildPath(parents, movesTo, child), visited.Count);

                    queue.Enqueue((current.Clone(), child));
                    current.Revert(outcome);
                }
            }

            return SolveResult.Unsolvable(visited.Count);
        }

        // Moving a whole box into an empty box of the same capacity only relocates it: the result is
        // the same position up to box order, so there is no point in trying it.
        private static bool IsRelocation(Board board, Move move)
        {
            var source = board.Boxes[move.From];
            var target = board.Boxes[move.To];
            return target.IsEmpty && source.Capacity == target.Capacity && move.Count == source.Count;
        }

        private static List<Move> BuildPath(List<int> parents, List<Move> movesTo, int node)
        {
            var path = new List<Move>();
            for (int i = node; parents[i] >= 0; i = parents[i])
                path.Add(movesTo[i]);

            path.Reverse();
            return path;
        }
    }
}
