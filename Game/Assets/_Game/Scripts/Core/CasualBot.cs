using System;
using System.Collections.Generic;

namespace SweetBazaar.Core
{
    // A simulated casual player used to measure how forgiving a level is. It has no lookahead, only a few sensible
    // habits (stack onto the same type, finish boxes, do not waste empty boxes, never undo its last move), and it
    // plays on until it wins, gets stuck or runs out of moves. The share of play-throughs it wins is a proxy for
    // difficulty that "shortest solution" alone does not give: a level can have a short solution and still be full
    // of traps. Everything is integer arithmetic on a fixed random generator, so results are identical everywhere.
    public static class CasualBot
    {
        public const int DefaultMaxMoves = 400;

        // True if one play-through (determined by seed) wins.
        public static bool Wins(Board board, ulong seed, int streamIndex, int maxMoves = DefaultMaxMoves)
        {
            var random = SeededRandom.ForStream(seed, streamIndex);
            var play = board.Clone();
            var legal = new List<Move>();
            var best = new List<Move>();
            Move last = default;
            bool hasLast = false;

            for (int step = 0; step < maxMoves; step++)
            {
                if (play.IsWon)
                    return true;

                MoveRules.GetLegalMoves(play, legal);
                if (legal.Count == 0)
                    return false;

                int bestScore = int.MinValue;
                best.Clear();
                foreach (var move in legal)
                {
                    int score = Score(play, move, hasLast, last) + random.Next(4);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        best.Clear();
                    }
                    if (score == bestScore)
                        best.Add(move);
                }

                var chosen = best[random.Next(best.Count)];
                play.Apply(chosen);
                last = chosen;
                hasLast = true;
            }
            return play.IsWon;
        }

        // Share of play-throughs won, as a whole percentage 0..100. The same board, seed and trial count always
        // give the same number.
        public static int WinRatePercent(Board board, int trials, ulong seed)
        {
            if (trials < 1)
                throw new ArgumentOutOfRangeException(nameof(trials), "At least one trial is needed.");

            int wins = 0;
            for (int trial = 0; trial < trials; trial++)
            {
                if (Wins(board, seed, trial))
                    wins++;
            }
            return (wins * 100 + trials / 2) / trials;
        }

        private static int Score(Board board, Move move, bool hasLast, Move last)
        {
            var source = board.Boxes[move.From];
            var target = board.Boxes[move.To];
            int score = 2 * move.Count;

            if (hasLast && move.From == last.To && move.To == last.From)
                score -= 200;                       // undoing the previous move
            if (move.Count == source.Count && target.IsEmpty)
                score -= 100;                       // only relocating a whole box
            else if (target.IsEmpty)
                score -= 20;                        // spending an empty box
            else
                score += 30;                        // stacking onto the same type

            if (move.Count == source.Count)
                score += 15;                        // empties the source box

            if (target.Count > 0 && target.Count + move.Count == target.Capacity && IsSingleType(target))
                score += 100;                       // completes a box
            return score;
        }

        private static bool IsSingleType(Box box)
        {
            for (int i = 1; i < box.Count; i++)
            {
                if (box.Candies[i] != box.Candies[0])
                    return false;
            }
            return true;
        }
    }
}
