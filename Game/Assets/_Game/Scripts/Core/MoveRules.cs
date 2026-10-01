using System;
using System.Collections.Generic;

namespace SweetBazaar.Core
{
    public static class MoveRules
    {
        // The rules of docs/TASARIM.md section 3:
        // - the source must be non-empty and not closed; closed boxes are frozen
        // - the target must have room, and be empty or have the same type on top
        // - the whole run of identical candies on top of the source moves together,
        //   but only as many as fit (a partial move when the target has less room)
        public static bool TryCreateMove(Board board, int from, int to, out Move move)
        {
            move = default;

            int boxCount = board.Boxes.Count;
            if (from == to || from < 0 || to < 0 || from >= boxCount || to >= boxCount)
                return false;

            var source = board.Boxes[from];
            var target = board.Boxes[to];

            // A closed target is full, so IsFull covers it.
            if (source.IsEmpty || source.IsClosed || target.IsFull)
                return false;
            if (!target.IsEmpty && target.Top != source.Top)
                return false;

            move = new Move(from, to, Math.Min(source.TopRunLength, target.FreeSlots));
            return true;
        }

        public static bool HasAnyLegalMove(Board board)
        {
            int boxCount = board.Boxes.Count;
            for (int from = 0; from < boxCount; from++)
            {
                for (int to = 0; to < boxCount; to++)
                {
                    if (TryCreateMove(board, from, to, out _))
                        return true;
                }
            }
            return false;
        }

        public static List<Move> GetLegalMoves(Board board)
        {
            var results = new List<Move>();
            GetLegalMoves(board, results);
            return results;
        }

        // Allocation-free variant for the solver: clears and fills the given list.
        public static void GetLegalMoves(Board board, List<Move> results)
        {
            results.Clear();

            int boxCount = board.Boxes.Count;
            for (int from = 0; from < boxCount; from++)
            {
                for (int to = 0; to < boxCount; to++)
                {
                    if (TryCreateMove(board, from, to, out var move))
                        results.Add(move);
                }
            }
        }
    }
}
