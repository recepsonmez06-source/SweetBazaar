using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SweetBazaar.EditorTools
{
    // Builds an installable APK for trying the game on a phone. It is signed with Unity's debug key, which is fine
    // for testing; the store build (AAB with the real keystore) comes later and its secrets never enter the repo.
    //   Menu:         Sweet Bazaar > Build Android APK
    //   Command line: tools\build-apk.ps1
    public static class AndroidBuilder
    {
        public const string ApkPath = "Builds/Android/SweetBazaar-debug.apk";

        [MenuItem("Sweet Bazaar/Build Android APK")]
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
                Debug.LogError("APK build failed: " + e);
                EditorApplication.Exit(1);
            }
        }

        public static void Build()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                throw new InvalidOperationException(
                    "The active build target is not Android. Start Unity with -buildTarget Android or use File > Build Profiles > Android > Switch Platform.");

            string output = Path.GetFullPath(ApkPath);
            Directory.CreateDirectory(Path.GetDirectoryName(output));

            EditorUserBuildSettings.buildAppBundle = false;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { SceneBuilder.MainScenePath },
                locationPathName = output,
                target = BuildTarget.Android,
                options = BuildOptions.None,
            };

            BuildSummary summary = BuildPipeline.BuildPlayer(options).summary;
            if (summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException($"Build {summary.result}: {summary.totalErrors} error(s).");

            Debug.Log($"APK built: {output} ({summary.totalSize / (1024 * 1024)} MB, {summary.totalTime.TotalSeconds:F0} s)");
        }
    }
}
