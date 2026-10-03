using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                case "cells":
                    CellStatistics();
                    return 0;
                case "build-levels":
                    return BuildLevels(args);
                case "verify-levels":
                    return VerifyLevels(args);
                case "difficulty":
                    return DifficultyChart(args);
                default:
                    Console.WriteLine(
                        "Usage:\n" +
                        "  curve [from to]                         generate levels from the curve and print statistics\n" +
                        "  level N                                 show one generated level\n" +
                        "  cells                                   how hard are random levels of each (types, empty boxes) setting\n" +
                        "  build-levels <levels.json> <count> [rebuild]   write the level pack (in parallel)\n" +
                        "  verify-levels <levels.json>             solve and measure every level again\n" +
                        "  difficulty <levels.json>                chart of the measured difficulty over the levels");
                    return 1;
            }
        }

        // For every (candy types, empty boxes) setting: how hard are random solvable levels for the casual bot?
        // The numbers go into the Ladder table of LevelCurve.
        private static void CellStatistics()
        {
            Console.WriteLine("types empty | CasualBot win rate over 120 random solvable levels: mean  p10  median  p90 | shortest solution median");
            foreach (int empties in new[] { 3, 2, 1 })
            {
                // 3 empty boxes with many types make the solver's search space explode; the game never needs that.
                foreach (int types in empties == 3 ? new[] { 2, 3, 4, 5, 6 } : new[] { 3, 4, 5, 6, 7, 8, 9, 10 })
                {
                    var rates = new ConcurrentBag<int>();
                    var moves = new ConcurrentBag<int>();
                    Parallel.For(1, 121, seed =>
                    {
                        var profile = new DifficultyProfile { CandyTypes = types, EmptyBoxes = empties, MaxAttempts = 8000, SolverStateLimit = 400000 };
                        try
                        {
                            var generated = LevelGenerator.Generate(profile, seed);
                            var board = Board.FromLevel(generated.Level);
                            rates.Add(LevelGenerator.MeasureWinRate(board, seed, 60));
                            moves.Add(generated.MinMoves);
                        }
                        catch (InvalidOperationException) { }
                    });
                    var sorted = rates.OrderBy(r => r).ToList();
                    var sortedMoves = moves.OrderBy(m => m).ToList();
                    if (sorted.Count == 0)
                    {
                        Console.WriteLine($"{types,5} {empties,5} | no level generated");
                        continue;
                    }
                    Console.WriteLine(
                        $"{types,5} {empties,5} | n={sorted.Count,3}  {sorted.Average(),5:F1}  {sorted[sorted.Count / 10],4}  {sorted[sorted.Count / 2],5}  {sorted[sorted.Count * 9 / 10],4}" +
                        $"  | {sortedMoves[sortedMoves.Count / 2]}");
                }
            }
        }

        private static void CurveStatistics(int from, int to)
        {
            var total = Stopwatch.StartNew();
            var lines = new string[to - from + 1];
            Parallel.For(from, to + 1, level =>
            {
                var profile = LevelCurve.GetProfile(level);
                var watch = Stopwatch.StartNew();
                try
                {
                    var generated = LevelGenerator.GenerateForLevel(level);
                    lines[level - from] =
                        $"level={level,3} types={profile.CandyTypes,2} empty={profile.EmptyBoxes} target={profile.TargetWinRate,3}% " +
                        $"botWins={generated.WinRate,3}% minMoves={generated.MinMoves,3} attempts={generated.Attempts,5} time={watch.ElapsedMilliseconds,6} ms";
                }
                catch (InvalidOperationException e)
                {
                    lines[level - from] = $"level={level,3} types={profile.CandyTypes,2} empty={profile.EmptyBoxes} target={profile.TargetWinRate}% FAILED: {e.Message} ({watch.ElapsedMilliseconds} ms)";
                }
            });
            foreach (var line in lines)
                Console.WriteLine(line);
            Console.WriteLine($"total: {total.Elapsed.TotalSeconds:F1} s");
        }

        private static void ShowLevel(int number)
        {
            var generated = LevelGenerator.GenerateForLevel(number);
            Console.WriteLine($"level {number}: minMoves={generated.MinMoves}, botWins={generated.WinRate}%, attempts={generated.Attempts}");
            foreach (var box in generated.Level.Boxes)
                Console.WriteLine("  [" + string.Join(",", box) + "]");
        }

        // Writes the level pack JSON without Unity. Append-only unless "rebuild" is given, exactly like the Unity tool.
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
            int first = pack.Count + 1;
            var watch = Stopwatch.StartNew();
            var made = new GeneratedLevel[Math.Max(0, count - first + 1)];
            var failures = new ConcurrentBag<string>();

            Parallel.For(first, count + 1, number =>
            {
                try
                {
                    made[number - first] = LevelGenerator.GenerateForLevel(number);
                }
                catch (InvalidOperationException e)
                {
                    failures.Add($"level {number}: {e.Message}");
                }
            });

            if (!failures.IsEmpty)
            {
                foreach (var failure in failures.OrderBy(f => f))
                    Console.WriteLine("FAILED " + failure);
                return 1;
            }

            for (int i = 0; i < made.Length; i++)
            {
                int number = first + i;
                var generated = made[i];
                pack.Add(new LevelRecord(number, generated.Seed, generated.MinMoves, generated.Level, generated.WinRate));
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

        // Solves every level again and checks: solvable, stored shortest solution, stored bot win rate, nothing packed at start.
        private static int VerifyLevels(string[] args)
        {
            var pack = LevelPackJson.Parse(File.ReadAllText(args[1]));
            var problems = new ConcurrentBag<string>();
            foreach (var error in pack.Validate())
                problems.Add("INVALID: " + error);

            var watch = Stopwatch.StartNew();
            Parallel.ForEach(pack.Levels, record =>
            {
                var board = Board.FromLevel(record.Definition);
                var result = Solver.Solve(board, new SolverOptions { MaxStates = 2000000 });
                if (result.Status != SolveStatus.Solved)
                    problems.Add($"level {record.Number}: not solvable ({result.Status})");
                else if (result.Moves.Count != record.MinMoves)
                    problems.Add($"level {record.Number}: shortest solution is {result.Moves.Count}, stored {record.MinMoves}");
                if (board.Boxes.Any(box => box.IsClosed))
                    problems.Add($"level {record.Number}: a box is already packed at the start");
                if (record.WinRate >= 0)
                {
                    int measured = LevelGenerator.MeasureWinRate(board, record.Seed, 100);
                    if (measured != record.WinRate)
                        problems.Add($"level {record.Number}: bot win rate is {measured}%, stored {record.WinRate}%");
                }
            });

            foreach (var problem in problems.OrderBy(p => p))
                Console.WriteLine(problem);
            Console.WriteLine($"verified {pack.Count} levels in {watch.Elapsed.TotalSeconds:F0} s: {(problems.IsEmpty ? "all OK" : problems.Count + " problem(s)")}");
            return problems.IsEmpty ? 0 : 1;
        }

        // Measured difficulty over the levels: per window, the average / lowest / highest bot win rate and the settings used.
        private static int DifficultyChart(string[] args)
        {
            var pack = LevelPackJson.Parse(File.ReadAllText(args[1]));
            int window = args.Length > 2 ? int.Parse(args[2]) : 10;
            Console.WriteLine($"window of {window} levels: bot win rate avg / min / max | candy types | levels with 1 empty box | shortest solution avg");

            for (int from = 1; from <= pack.Count; from += window)
            {
                var levels = pack.Levels.Where(l => l.Number >= from && l.Number < from + window).ToList();
                int Types(LevelRecord l) => l.Definition.Boxes.SelectMany(b => b).Distinct().Count();
                int Empties(LevelRecord l) => l.Definition.Boxes.Count(b => b.Length == 0);
                var rates = levels.Where(l => l.WinRate >= 0).Select(l => l.WinRate).ToList();
                Console.WriteLine(
                    $"  {from,3}-{from + levels.Count - 1,3}: " +
                    (rates.Count > 0 ? $"{rates.Average(),5:F1}% / {rates.Min(),3}% / {rates.Max(),3}%" : "      n/a       ") +
                    $" | types {levels.Min(Types)}-{levels.Max(Types)} | 1-empty: {levels.Count(l => Empties(l) == 1),2} | {levels.Average(l => l.MinMoves),4:F1} moves");
            }
            return 0;
        }
    }
}
