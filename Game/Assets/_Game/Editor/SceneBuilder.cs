using System;
using System.IO;
using SweetBazaar.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SweetBazaar.EditorTools
{
    // Creates the game's one scene: a camera and a GameBootstrap object (the bootstrap builds everything else).
    //   Menu:         Sweet Bazaar > Create Main Scene
    //   Command line: tools\run-editor-method.ps1 -Method SweetBazaar.EditorTools.SceneBuilder.BuildFromCommandLine
    public static class SceneBuilder
    {
        public const string MainScenePath = "Assets/_Game/Scenes/Main.unity";

        [MenuItem("Sweet Bazaar/Create Main Scene")]
        public static void BuildFromMenu() => Build();

        public static void BuildFromCommandLine()
        {
            try
            {
                Build();
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogError("Scene build failed: " + e);
                EditorApplication.Exit(1);
            }
        }

        public static void Build()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(MainScenePath)));

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(0xFF, 0xF1, 0xD6, 255);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            new GameObject("Game").AddComponent<GameBootstrap>();

            if (!EditorSceneManager.SaveScene(scene, MainScenePath))
                throw new InvalidOperationException("Could not save " + MainScenePath);

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MainScenePath, true) };
            Debug.Log("Main scene created: " + MainScenePath);
        }
    }
}
