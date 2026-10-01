namespace SweetBazaar.Core
{
    public sealed class GeneratedLevel
    {
        public GeneratedLevel(LevelDefinition level, int seed, int minMoves, int attempts)
        {
            Level = level;
            Seed = seed;
            MinMoves = minMoves;
            Attempts = attempts;
        }

        public LevelDefinition Level { get; }

        public int Seed { get; }

        // Length of the shortest solution, as proven by the solver.
        public int MinMoves { get; }

        // How many candidates were tried before this one was accepted (1 = the first).
        public int Attempts { get; }
    }
}
