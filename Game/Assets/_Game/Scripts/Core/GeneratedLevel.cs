namespace SweetBazaar.Core
{
    public sealed class GeneratedLevel
    {
        public GeneratedLevel(LevelDefinition level, int seed, int minMoves, int attempts, int winRate = -1)
        {
            Level = level;
            Seed = seed;
            MinMoves = minMoves;
            Attempts = attempts;
            WinRate = winRate;
        }

        public LevelDefinition Level { get; }

        public int Seed { get; }

        // Length of the shortest solution, as proven by the solver.
        public int MinMoves { get; }

        // How many candidates were tried before this one was accepted (1 = the first).
        public int Attempts { get; }

        // Percent of play-throughs the CasualBot wins (0 hard .. 100 easy); -1 if the profile did not measure it.
        public int WinRate { get; }
    }
}
