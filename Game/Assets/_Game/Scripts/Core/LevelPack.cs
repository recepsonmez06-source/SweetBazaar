using System;
using System.Collections.Generic;

namespace SweetBazaar.Core
{
    // One shipped level: its number, how it was made and its starting position.
    public sealed class LevelRecord
    {
        public LevelRecord(int number, int seed, int minMoves, LevelDefinition definition, int winRate = -1)
        {
            Number = number;
            Seed = seed;
            MinMoves = minMoves;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            WinRate = winRate;
        }

        public int Number { get; }
        public int Seed { get; }

        // Length of the shortest solution when the level was made.
        public int MinMoves { get; }

        // How easy the level was for the CasualBot when it was made: percent of play-throughs won
        // (100 easy .. 0 hard), or -1 if it was not measured.
        public int WinRate { get; }

        public LevelDefinition Definition { get; }
    }

    // All levels of the game, numbered 1..Count without gaps. Stored as JSON (see LevelPackJson);
    // the file, not the generator, is the source of truth once a level has shipped.
    public sealed class LevelPack
    {
        private readonly List<LevelRecord> _levels = new List<LevelRecord>();

        public IReadOnlyList<LevelRecord> Levels => _levels;

        public int Count => _levels.Count;

        // Level numbers start at 1.
        public LevelRecord Get(int number)
        {
            if (number < 1 || number > _levels.Count)
                throw new ArgumentOutOfRangeException(nameof(number), $"No level {number}; the pack has {_levels.Count}.");

            return _levels[number - 1];
        }

        // Appends the next level; its number must be Count + 1 so the pack stays gap-free.
        public void Add(LevelRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));
            if (record.Number != _levels.Count + 1)
                throw new ArgumentException($"Expected level {_levels.Count + 1} but got {record.Number}.", nameof(record));

            _levels.Add(record);
        }

        // Returns the problems found; an empty list means every level is a valid definition.
        // (Solvability is not checked here; see Solver.)
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            foreach (var record in _levels)
            {
                foreach (var error in record.Definition.Validate())
                    errors.Add($"Level {record.Number}: {error}");

                if (record.MinMoves < 0)
                    errors.Add($"Level {record.Number}: MinMoves must not be negative.");
                if (record.WinRate < -1 || record.WinRate > 100)
                    errors.Add($"Level {record.Number}: WinRate must be -1 (not measured) or between 0 and 100.");
            }
            return errors;
        }
    }
}
