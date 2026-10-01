using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SweetBazaar.Core;

namespace CoreBench
{
    // Quick checks of the engine-independent game logic without starting Unity.
    internal static class Program
    {
        private static int Main(string[] args)
        {
            string command = args.Length > 0 ? args[0] : "help";
            switch (command)
            {
                case "curve":
                    CurveStatistics(args.Length > 1 ? int.Parse(args[1]) : 1, args.Length > 2 ? int.Parse(args[2]) : 120);
                    return 0;
                case "level":
                    ShowLevel(int.Parse(args[1]));
                    return 0;
                case "oneempty":
                    OneEmptyStatistics();
                    return 0;
                case "build-levels":
                    return BuildLevels(args);
                case "verify-levels":
                    return VerifyLevels(args);
                default:
                    Console.WriteLine("Usage: curve [from to] | level N | oneempty | build-levels <levels.json> <count> [rebuild] | verify-levels <levels.json>");
                    return 1;
            }
        }

        // Levels with a single empty box: how many random deals are needed and how long the solutions are.
        private static void OneEmptyStatistics()
        {
            foreach (int types in new[] { 4, 6, 8, 10 })
            {
                var moves = new List<int>();
                var attempts = new List<int>();
                var watch = Stopwatch.StartNew();
                for (int seed = 1; seed <= 12; seed++)
                {
                    var profile = new DifficultyProfile { CandyTypes = types, EmptyBoxes = 1, MaxAttempts = 3000, SolverStateLimit = 400000 };
                    try
                    {
                        var generated = LevelGenerator.Generate(profile, seed);
                        moves.Add(generated.MinMoves);
                        attempts.Add(generated.Attempts);
                    }
                    catch (InvalidOperationException) { }
                }
                moves.Sort();
                attempts.Sort();
                Console.WriteLine(
                    $"1 empty: types={types,2} generated={moves.Count,2}/12 " +
                    (moves.Count > 0
                        ? $"minMoves={moves[0]}-{moves[moves.Count - 1]} median={moves[moves.Count / 2]} ({(double)moves[moves.Count / 2] / types:F1}/type) " +
                          $"attempts median={attempts[attempts.Count / 2]} max={attempts[attempts.Count - 1]} "
                        : "") +
                    $"time={watch.ElapsedMilliseconds} ms");
            }
        }

        private static void CurveStatistics(int from, int to)
        {
            var total = Stopwatch.StartNew();
            for (int level = from; level <= to; level++)
            {
                var profile = LevelCurve.GetProfile(level);
                var watch = Stopwatch.StartNew();
                try
                {
                    var generated = LevelGenerator.GenerateForLevel(level);
                    Console.WriteLine(
                        $"level={level,3} types={profile.CandyTypes,2} empty={profile.EmptyBoxes} " +
                        $"minMoves={generated.MinMoves,3} ({(double)generated.MinMoves / profile.CandyTypes:F1}/type) " +
                        $"attempts={generated.Attempts,4} time={watch.ElapsedMilliseconds,6} ms");
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine($"level={level,3} types={profile.CandyTypes,2} empty={profile.EmptyBoxes} FAILED: {e.Message} ({watch.ElapsedMilliseconds} ms)");
                }
            }
            Console.WriteLine($"total: {total.Elapsed.TotalSeconds:F1} s");
        }

        private static void ShowLevel(int number)
        {
            var generated = LevelGenerator.GenerateForLevel(number);
            Console.WriteLine($"level {number}: minMoves={generated.MinMoves}, attempts={generated.Attempts}");
            foreach (var box in generated.Level.Boxes)
                Console.WriteLine("  [" + string.Join(",", box) + "]");
        }

        // Solves every level of a pack again and checks the stored shortest-solution length, then prints a summary.
        private static int VerifyLevels(string[] args)
        {
            var pack = LevelPackJson.Parse(File.ReadAllText(args[1]));
            var errors = pack.Validate();
            foreach (var error in errors)
                Console.WriteLine("INVALID: " + error);

            int bad = errors.Count;
            var watch = Stopwatch.StartNew();
            foreach (var record in pack.Levels)
            {
                var board = Board.FromLevel(record.Definition);
                var result = Solver.Solve(board, new SolverOptions { MaxStates = 2000000 });
                bool closed = board.Boxes.Any(box => box.IsClosed);
                if (result.Status != SolveStatus.Solved || result.Moves.Count != record.MinMoves || closed)
                {
                    bad++;
                    Console.WriteLine($"PROBLEM level {record.Number}: status={result.Status}, solver={result.Moves.Count}, stored={record.MinMoves}, packedAtStart={closed}");
                }
            }
            Console.WriteLine($"verified {pack.Count} levels in {watch.Elapsed.TotalSeconds:F0} s: {(bad == 0 ? "all OK" : bad + " problem(s)")}");

            // summary per range
            foreach (var range in new[] { (1, 5), (6, 24), (25, 59), (60, 100), (101, 200) })
            {
                var levels = pack.Levels.Where(l => l.Number >= range.Item1 && l.Number <= range.Item2).ToList();
                if (levels.Count == 0)
                    continue;
                int Types(LevelRecord l) => l.Definition.Boxes.SelectMany(b => b).Distinct().Count();
                int Empties(LevelRecord l) => l.Definition.Boxes.Count(b => b.Length == 0);
                Console.WriteLine(
                    $"levels {range.Item1,3}-{range.Item2,3}: types {levels.Min(Types)}-{levels.Max(Types)}, " +
                    $"shortest solution {levels.Min(l => l.MinMoves)}-{levels.Max(l => l.MinMoves)} (avg {levels.Average(l => l.MinMoves):F1}), " +
                    $"levels with 1 empty box: {levels.Count(l => Empties(l) == 1)} of {levels.Count}");
            }
            return bad == 0 ? 0 : 1;
        }

        // Same as tools\build-levels.ps1 (which needs Unity), but without Unity: writes the level pack JSON.
        // Append-only unless "rebuild" is given, exactly like the Unity tool.
        private static int BuildLevels(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: build-levels <levels.json> <count> [rebuild]");
                return 1;
            }

            string path = args[1];
            int count = int.Parse(args[2]);
            bool rebuild = args.Length > 3 && args[3] == "rebuild";

            var pack = new LevelPack();
            if (!rebuild && File.Exists(path))
                pack = LevelPackJson.Parse(File.ReadAllText(path));

            int kept = pack.Count;
            var watch = Stopwatch.StartNew();
            for (int number = pack.Count + 1; number <= count; number++)
            {
                var generated = LevelGenerator.GenerateForLevel(number);
                pack.Add(new LevelRecord(number, generated.Seed, generated.MinMoves, generated.Level));
                Console.WriteLine($"level {number,3}: {generated.Level.Boxes.Count} boxes, {generated.MinMoves} moves, {generated.Attempts} attempt(s)");
            }

            var errors = pack.Validate();
            if (errors.Count > 0)
            {
                Console.WriteLine("Invalid level pack: " + string.Join("; ", errors));
                return 1;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            File.WriteAllText(path, LevelPackJson.Serialize(pack), new UTF8Encoding(false));
            Console.WriteLine($"Level pack written: {pack.Count} levels ({kept} kept, {pack.Count - kept} generated) in {watch.Elapsed.TotalSeconds:F0} s -> {path}");
            return 0;
        }
    }
}
