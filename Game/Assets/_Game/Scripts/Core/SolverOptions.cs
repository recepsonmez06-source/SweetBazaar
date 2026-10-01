namespace SweetBazaar.Core
{
    public sealed class SolverOptions
    {
        // The search gives up (SolveStatus.Unknown) after discovering this many distinct positions.
        // It bounds memory and time; the right value depends on the board size.
        public int MaxStates { get; set; } = 1000000;
    }
}
