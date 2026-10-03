using System;
using System.IO;
using System.Text;
using SweetBazaar.Core;
using UnityEditor;
using UnityEngine;

namespace SweetBazaar.EditorTools
{
    // Generates the shipped levels once and stores them as JSON. Append-only by default: levels already in the
    // file are kept untouched (players must keep seeing the same level N), only missing numbers are generated.
    //   Menu:         Sweet Bazaar > Build Level Pack
    //   Command line: tools\build-levels.ps1 [-Count 200] [-Rebuild]
    public static class LevelPackBuilder
    {
        // Loaded at runtime with Resources.Load<TextAsset>("Levels/levels").
        public const string PackAssetPath = "Assets/_Game/Resources/Levels/levels.json";

        private const int DefaultLevelCount = 200;

        [MenuItem("Sweet Bazaar/Build Level Pack")]
        public static void BuildFromMenu() => Build(DefaultLevelCount, rebuild: false);

        public static void BuildFromCommandLine()
        {
            int count = DefaultLevelCount;
            bool rebuild = false;

            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-levelCount" && i + 1 < args.Length)
                    count = int.Parse(args[i + 1]);
                else if (args[i] == "-rebuild")
                    rebuild = true;
            }

            try
            {
                Build(count, rebuild);
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogError("Level pack build failed: " + e);
                EditorApplication.Exit(1);
            }
        }

        public static void Build(int levelCount, bool rebuild)
        {
            string path = Path.GetFullPath(PackAssetPath);

            var pack = new LevelPack();
            if (!rebuild && File.Exists(path))
                pack = LevelPackJson.Parse(File.ReadAllText(path));

            int kept = pack.Count;
            for (int number = pack.Count + 1; number <= levelCount; number++)
            {
                var generated = LevelGenerator.GenerateForLevel(number);
                pack.Add(new LevelRecord(number, generated.Seed, generated.MinMoves, generated.Level, generated.WinRate));
                Debug.Log($"Level {number}: {generated.Level.Boxes.Count} boxes, {generated.MinMoves} moves, " +
                          $"bot wins {generated.WinRate}%, {generated.Attempts} attempt(s)");
            }

            var errors = pack.Validate();
            if (errors.Count > 0)
                throw new InvalidOperationException("Invalid level pack: " + string.Join("; ", errors));

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, LevelPackJson.Serialize(pack), new UTF8Encoding(false));
            AssetDatabase.Refresh();

            Debug.Log($"Level pack written: {pack.Count} levels ({kept} kept, {pack.Count - kept} generated) -> {PackAssetPath}");
        }
    }
}
