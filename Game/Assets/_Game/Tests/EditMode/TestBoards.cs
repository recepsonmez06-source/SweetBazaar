namespace SweetBazaar.Core.Tests
{
    // Helpers for building small boards in tests. Candy ids are arbitrary ints (0, 1, 2 ...).
    internal static class TestBoards
    {
        public static readonly int[] None = new int[0];

        // Board with the standard capacity of 4. Each argument is one box, bottom -> top.
        public static Board Cap4(params int[][] boxes) => Board.Create(4, boxes);

        // Board with capacity 2, handy for short winning sequences.
        public static Board Cap2(params int[][] boxes) => Board.Create(2, boxes);
    }
}
