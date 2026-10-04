using System.Collections.Generic;
using System.Linq;
using SweetBazaar.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace SweetBazaar.Game
{
    // The only component the scene needs: put it on an empty GameObject and press Play.
    // It creates the camera settings, the board, the HUD and the controller, then loads the saved level.
    public sealed class GameBootstrap : MonoBehaviour
    {
        private static readonly Color BackgroundColor = new Color32(0xFF, 0xF1, 0xD6, 255);

        // Tests and preview tools switch this off so they do not overwrite the player's saved progress.
        public bool PersistProgress { get; set; } = true;

        // Testing aid: select the "Game" object and type a level number here to start on that level instead of the
        // saved one. While it is set (> 0), nothing is saved, so the real saved level and language stay untouched.
        [SerializeField] private int debugStartLevel;

        // Set before Initialize to start on a specific level instead of the saved one.
        public int StartLevel { get; set; }

        // Forgets the saved level, so the next start is level 1.
        public static void ResetSavedProgress() => GamePrefs.ResetProgress();

        public GameController Controller { get; private set; }

        private void Start()
        {
            if (Controller == null)
                Initialize();
        }

        public GameController Initialize()
        {
            if (debugStartLevel > 0)
            {
                StartLevel = debugStartLevel;
                PersistProgress = false;
            }

            if (Application.isPlaying)
            {
                Application.targetFrameRate = 60;
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            var camera = SetUpCamera();
            EnsureEventSystem();

            var localizer = LoadLocalizer();
            var pack = LoadLevelPack();

            BackgroundArt.Create(transform, camera);

            var boardObject = new GameObject("Board");
            boardObject.transform.SetParent(transform, false);
            var boardView = boardObject.AddComponent<BoardView>();

            var hud = new GameHud(transform, camera, localizer);

            Controller = gameObject.AddComponent<GameController>();
            Controller.Initialize(pack, localizer, boardView, hud, camera, PersistProgress);
            Controller.LoadLevel(StartLevel > 0 ? StartLevel : (PersistProgress ? GamePrefs.CurrentLevel : 1));
            return Controller;
        }

        private static Camera SetUpCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                camera = go.AddComponent<Camera>();
            }

            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            return camera;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null || Object.FindFirstObjectByType<EventSystem>() != null)
                return;

            var go = new GameObject("EventSystem", typeof(EventSystem));
            var module = go.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        private Localizer LoadLocalizer()
        {
            var tables = new List<LocalizationTable>();
            foreach (var asset in Resources.LoadAll<TextAsset>("Localization"))
                tables.Add(LocalizationTable.FromJson(asset.text));

            // Stable order so the language button always cycles the same way.
            tables = tables.OrderBy(table => table.Language == Localizer.DefaultLanguage ? 0 : 1)
                           .ThenBy(table => table.Language).ToList();

            string saved = PersistProgress ? GamePrefs.Language : null;
            string system = SystemLanguageCodes.Code(Application.systemLanguage);
            string language = Localizer.ChooseLanguage(tables.Select(table => table.Language), saved, system);
            return new Localizer(tables, language);
        }

        private static LevelPack LoadLevelPack()
        {
            var asset = Resources.Load<TextAsset>("Levels/levels");
            if (asset == null)
                throw new System.InvalidOperationException("Resources/Levels/levels.json is missing; build it with tools\\build-levels.ps1.");
            return LevelPackJson.Parse(asset.text);
        }
    }
}
