using SweetBazaar.Game;
using UnityEditor;
using UnityEngine;

namespace SweetBazaar.EditorTools
{
    // Handy while testing: forget the saved level so the game starts at level 1 again.
    // (To jump straight to any level instead, select the "Game" object in the scene and type a number into
    // "Debug Start Level" in the Inspector; that does not touch the saved progress.)
    public static class ProgressMenu
    {
        [MenuItem("Sweet Bazaar/Reset Saved Level")]
        public static void ResetSavedLevel()
        {
            GameBootstrap.ResetSavedProgress();
            Debug.Log("Saved level forgotten: the game starts at level 1 next time.");
        }
    }
}
