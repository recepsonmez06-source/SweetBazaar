using System;
using System.Collections.Generic;
using System.IO;
using SweetBazaar.Core;
using SweetBazaar.Game;
using UnityEditor;
using UnityEngine;

namespace SweetBazaar.EditorTools
{
    // Renders the game screen in a few situations to PNG files, so the look can be checked without playing.
    //   Menu:         Sweet Bazaar > Capture Preview Screenshots
    //   Command line: tools\preview.ps1
    public static class PreviewScreenshots
    {
        private const int Width = 1080;
        private const int Height = 1920;

        private sealed class Shot
        {
            public string Name;
            public int Level;
            public string Language = "en";
            public Action<GameController> Setup;
        }

        private static readonly Shot[] Shots =
        {
            new Shot { Name = "level-001", Level = 1 },
            new Shot { Name = "level-006", Level = 6 },
            new Shot { Name = "level-020", Level = 20 },
            new Shot { Name = "level-060", Level = 60 },
            new Shot { Name = "level-150-tr", Level = 150, Language = "tr" },
            new Shot { Name = "selected", Level = 6, Setup = c => c.TapBox(0) },
            new Shot { Name = "packed", Level = 6, Setup = c => PlayAlmostToTheEnd(c, keepLast: 2) },
            new Shot { Name = "win", Level = 6, Setup = c => { PlayAlmostToTheEnd(c, keepLast: 0); c.Hud.ShowWin(false, 27); } },
            new Shot { Name = "win-tr", Level = 6, Language = "tr", Setup = c => { PlayAlmostToTheEnd(c, keepLast: 0); c.Hud.ShowWin(false, 27); } },
            new Shot { Name = "stuck-tr", Level = 12, Language = "tr", Setup = c => c.Hud.ShowStuck() },
            new Shot { Name = "shop-0", Level = 3, Setup = c => c.Hud.ShowShop(new Shop(gold: 40)) },
            new Shot { Name = "shop-pick-sign", Level = 3, Setup = c => { c.Hud.ShowShop(new Shop(gold: 200)); c.Hud.SelectPlace(ShopPlace.Sign); c.Hud.SelectStyle(2); } },
            new Shot { Name = "shop-built-tr", Level = 3, Language = "tr", Setup = c => c.Hud.ShowShop(new Shop(12, new[] { 0, -1, -1, -1, 1, -1 }), justBuilt: true) },
            new Shot { Name = "shop-mid-tr", Level = 3, Language = "tr", Setup = c => c.Hud.ShowShop(new Shop(120, new[] { 1, 0, 2, -1, 1, -1 })) },
            new Shot { Name = "shop-full-0", Level = 3, Setup = c => c.Hud.ShowShop(new Shop(2000, new[] { 0, 0, 0, 0, 0, 0 })) },
            new Shot { Name = "shop-full-1", Level = 3, Setup = c => c.Hud.ShowShop(new Shop(2000, new[] { 1, 1, 1, 1, 1, 1 })) },
            new Shot { Name = "shop-full-2", Level = 3, Setup = c => c.Hud.ShowShop(new Shop(2000, new[] { 2, 2, 2, 2, 2, 2 })) },
            new Shot { Name = "tutorial-no-counters", Level = 2 },
            new Shot { Name = "help-prices", Level = 6, Setup = c => { c.Hud.SetGold(120); c.Hud.SetRights(0, 0, false, ShopRules.UndoPrice, ShopRules.ExtraBoxPrice); } },
            new Shot { Name = "shop-bunting", Level = 3, Setup = c => c.Hud.ShowShop(new Shop(300, new[] { 0, -1, 1, -1, -1, 0 })) },
        };

        [MenuItem("Sweet Bazaar/Capture Preview Screenshots")]
        public static void CaptureFromMenu()
        {
            string dir = Path.Combine(Path.GetTempPath(), "SweetBazaar-preview");
            Capture(dir);
            EditorUtility.RevealInFinder(dir);
        }

        public static void CaptureFromCommandLine()
        {
            string dir = Path.Combine(Path.GetTempPath(), "SweetBazaar-preview");
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-shotDir")
                    dir = args[i + 1];
            }

            try
            {
                Capture(dir);
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogError("Preview capture failed: " + e);
                EditorApplication.Exit(1);
            }
        }

        public static void Capture(string directory)
        {
            Directory.CreateDirectory(directory);

            // The very first render of a session can draw freshly generated textures wrongly; render once and discard.
            Render(Shots[0], Path.Combine(directory, "warm-up.png"));

            foreach (var shot in Shots)
                Render(shot, Path.Combine(directory, shot.Name + ".png"));

            Debug.Log($"Preview screenshots written: {Shots.Length} -> {directory}");
        }

        // Plays the solver's solution up to the last keepLast moves, so some boxes are already packed.
        private static void PlayAlmostToTheEnd(GameController controller, int keepLast)
        {
            var solution = Solver.Solve(controller.Session.Board.Clone());
            if (solution.Status != SolveStatus.Solved)
                throw new InvalidOperationException("Preview level is not solvable.");

            var moves = new List<Move>(solution.Moves);
            moves.RemoveRange(moves.Count - keepLast, keepLast);
            controller.ApplyMovesInstantly(moves);
        }

        private static void Render(Shot shot, string path)
        {
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.targetTexture = target;

            var root = new GameObject("PreviewRoot");
            try
            {
                var bootstrap = root.AddComponent<GameBootstrap>();
                bootstrap.PersistProgress = false;
                bootstrap.StartLevel = shot.Level;
                var controller = bootstrap.Initialize();

                controller.SetLanguage(shot.Language);
                shot.Setup?.Invoke(controller);

                Canvas.ForceUpdateCanvases();
                camera.Render();

                var previous = RenderTexture.active;
                RenderTexture.active = target;
                var image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                image.Apply();
                RenderTexture.active = previous;

                File.WriteAllBytes(path, image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image);
            }
            finally
            {
                camera.targetTexture = null;
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(cameraObject);

                var eventSystem = UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
                if (eventSystem != null)
                    UnityEngine.Object.DestroyImmediate(eventSystem.gameObject);
            }
        }
    }
}
