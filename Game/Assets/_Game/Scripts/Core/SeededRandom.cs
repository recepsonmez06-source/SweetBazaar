namespace SweetBazaar.Core
{
    // Small deterministic random generator (SplitMix64). Unlike System.Random it is fixed by this code,
    // so a given seed yields the same levels on every device and engine version.
    internal sealed class SeededRandom
    {
        private ulong _state;

        public SeededRandom(ulong seed)
        {
            _state = seed;
        }

        public ulong NextUInt64()
        {
            unchecked
            {
                _state += 0x9E3779B97F4A7C15UL;
                ulong z = _state;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }

        // Uniform-enough integer in [0, maxExclusive); the tiny modulo bias is irrelevant for shuffling candies.
        public int Next(int maxExclusive) => (int)(NextUInt64() % (ulong)maxExclusive);
    }
}
