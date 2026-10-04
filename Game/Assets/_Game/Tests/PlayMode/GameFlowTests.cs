using System.Collections;
using System.Linq;
using NUnit.Framework;
using SweetBazaar.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace SweetBazaar.Game.PlayTests
{
    // Plays the real game objects the way a player does: taps on boxes, with the animations running.
    public class GameFlowTests
    {
        private const float AnimationTimeoutSeconds = 15f;

        private GameBootstrap _bootstrap;

        private GameController StartGame(int level)
        {
            var root = new GameObject("Game");
            _bootstrap = root.AddComponent<GameBootstrap>();
            _bootstrap.PersistProgress = false;
            _bootstrap.StartLevel = level;
            return _bootstrap.Initialize();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_bootstrap != null)
                Object.Destroy(_bootstrap.gameObject);
            foreach (var camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                Object.Destroy(camera.gameObject);
            foreach (var eventSystem in Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
                Object.Destroy(eventSystem.gameObject);

            _bootstrap = null;
            yield return null;
        }

        private static IEnumerator WaitUntilIdle(GameController controller)
        {
            float waited = 0f;
            while (controller.IsBusy)
            {
                waited += Time.deltaTime;
                if (waited > AnimationTimeoutSeconds)
                    Assert.Fail("The animation never finished.");
                yield return null;
            }
        }

        private static IEnumerator Tap(GameController controller, int box)
        {
            controller.TapBox(box);
            yield return WaitUntilIdle(controller);
        }

        private static IEnumerator PlayMove(GameController controller, Move move)
        {
            yield return Tap(controller, move.From);
            yield return Tap(controller, move.To);
        }

        private static Move[] Solution(GameController controller)
        {
            var result = Solver.Solve(controller.Session.Board.Clone());
            Assert.AreEqual(SolveStatus.Solved, result.Status);
            return result.Moves.ToArray();
        }

        private BoardView View => _bootstrap.GetComponentInChildren<BoardView>();

        [UnityTest]
        public IEnumerator StartsOnTheRequestedLevelWithTheBoardShown()
        {
            var controller = StartGame(6);
            yield return null;

            Assert.AreEqual(6, controller.LevelNumber);
            Assert.AreEqual(controller.Session.Board.Boxes.Count, View.Boxes.Count);
            Assert.IsFalse(controller.Hud.WinVisible);
            Assert.IsFalse(controller.Hud.UndoInteractable, "nothing to undo yet");
            Assert.IsTrue(controller.Hud.AddBoxInteractable);
        }

        [UnityTest]
        public IEnumerator TappingABoxLiftsItsTopCandiesAndTappingAgainPutsThemBack()
        {
            var controller = StartGame(6);
            yield return null;

            int box = FirstPickableBox(controller);
            float restingY = View.Boxes[box].Candies.Last().transform.localPosition.y;

            controller.TapBox(box);
            yield return new WaitForSeconds(0.25f);
            Assert.AreEqual(box, controller.SelectedBox);
            Assert.Greater(View.Boxes[box].Candies.Last().transform.localPosition.y, restingY + 0.2f);

            controller.TapBox(box);
            yield return new WaitForSeconds(0.25f);
            Assert.AreEqual(-1, controller.SelectedBox);
            Assert.AreEqual(restingY, View.Boxes[box].Candies.Last().transform.localPosition.y, 0.01f);
        }

        [UnityTest]
        public IEnumerator ALegalMoveMovesTheCandiesAndKeepsTheViewInSyncWithTheModel()
        {
            var controller = StartGame(6);
            yield return null;
            var move = Solution(controller)[0];

            yield return PlayMove(controller, move);

            Assert.AreEqual(1, controller.MovesMade);
            AssertViewMatchesModel(controller);
        }

        [UnityTest]
        public IEnumerator AnIllegalMoveChangesNothingAndMovesTheSelectionToTheTappedBox()
        {
            var controller = StartGame(6);
            yield return null;

            var board = controller.Session.Board;
            int from = -1, to = -1;
            for (int a = 0; a < board.Boxes.Count && from < 0; a++)
            {
                for (int b = 0; b < board.Boxes.Count; b++)
                {
                    if (a != b && !board.Boxes[a].IsEmpty && !board.Boxes[b].IsEmpty
                        && !MoveRules.TryCreateMove(board, a, b, out _))
                    {
                        from = a;
                        to = b;
                        break;
                    }
                }
            }
            Assert.GreaterOrEqual(from, 0, "the level should have a pair of boxes that cannot be combined");
            string before = board.GetStateKey();

            yield return Tap(controller, from);
            yield return Tap(controller, to);

            Assert.AreEqual(before, board.GetStateKey());
            Assert.AreEqual(0, controller.MovesMade);
            Assert.AreEqual(to, controller.SelectedBox);
        }

        [UnityTest]
        public IEnumerator PlayingTheSolutionShowsPackagesAndTheWinPanel()
        {
            var controller = StartGame(6);
            yield return null;
            var solution = Solution(controller);

            foreach (var move in solution)
                yield return PlayMove(controller, move);

            Assert.IsTrue(controller.Session.Board.IsWon);
            Assert.IsTrue(controller.Hud.WinVisible);
            Assert.AreEqual(solution.Length, controller.MovesMade);
            AssertViewMatchesModel(controller);

            int parcels = View.Boxes.Count(box => box.ParcelVisible);
            int closedBoxes = controller.Session.Board.Boxes.Count(box => box.IsClosed);
            Assert.AreEqual(closedBoxes, parcels);
            Assert.Greater(parcels, 0);
        }

        [UnityTest]
        public IEnumerator NextLevelAfterAWinLoadsTheNextLevel()
        {
            var controller = StartGame(1);
            yield return null;
            foreach (var move in Solution(controller))
                yield return PlayMove(controller, move);
            Assert.IsTrue(controller.Hud.WinVisible);

            controller.NextLevel();
            yield return null;

            Assert.AreEqual(2, controller.LevelNumber);
            Assert.IsFalse(controller.Hud.WinVisible);
            Assert.AreEqual(0, controller.MovesMade);
            AssertViewMatchesModel(controller);
        }

        [UnityTest]
        public IEnumerator TheDebugStartLevelStartsThereAndLeavesTheSavedProgressAlone()
        {
            const string levelKey = "level.current";
            bool hadLevel = PlayerPrefs.HasKey(levelKey);
            int level = PlayerPrefs.GetInt(levelKey, 1);

            try
            {
                PlayerPrefs.SetInt(levelKey, 7);

                var root = new GameObject("Game");
                _bootstrap = root.AddComponent<GameBootstrap>();
                typeof(GameBootstrap).GetField("debugStartLevel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(_bootstrap, 42);
                var controller = _bootstrap.Initialize();
                yield return null;

                Assert.AreEqual(42, controller.LevelNumber, "the debug level wins over the saved one");
                Assert.AreEqual(7, PlayerPrefs.GetInt(levelKey, 0), "and the saved progress is not overwritten");
            }
            finally
            {
                if (hadLevel) PlayerPrefs.SetInt(levelKey, level); else PlayerPrefs.DeleteKey(levelKey);
                PlayerPrefs.Save();
            }
        }

        [UnityTest]
        public IEnumerator ResettingTheSavedLevelMakesTheNextStartLevelOne()
        {
            const string levelKey = "level.current";
            bool hadLevel = PlayerPrefs.HasKey(levelKey);
            int level = PlayerPrefs.GetInt(levelKey, 1);

            try
            {
                PlayerPrefs.SetInt(levelKey, 12);
                GameBootstrap.ResetSavedProgress();

                var root = new GameObject("Game");
                _bootstrap = root.AddComponent<GameBootstrap>();
                var controller = _bootstrap.Initialize();   // persisting on, no start level given
                yield return null;

                Assert.AreEqual(1, controller.LevelNumber);
            }
            finally
            {
                if (hadLevel) PlayerPrefs.SetInt(levelKey, level); else PlayerPrefs.DeleteKey(levelKey);
                PlayerPrefs.Save();
            }
        }

        [UnityTest]
        public IEnumerator TheLastLevelWrapsAroundToTheFirst()
        {
            var controller = StartGame(1);
            yield return null;
            int last = LevelPackJson.Parse(Resources.Load<TextAsset>("Levels/levels").text).Count;

            controller.LoadLevel(last);
            controller.NextLevel();

            Assert.AreEqual(1, controller.LevelNumber);
        }

        [UnityTest]
        public IEnumerator UndoTakesTheMoveBackAndReopensAClosedBox()
        {
            // a short level: undoing every move up to the first packed box must stay within the 5 undos of a level
            var controller = StartGame(1);
            yield return null;
            string start = controller.Session.Board.GetStateKey();
            var solution = Solution(controller);

            // up to and including the first move that packs a box
            int played = 0;
            foreach (var move in solution)
            {
                yield return PlayMove(controller, move);
                played++;
                if (controller.Session.Board.Boxes.Any(box => box.IsClosed))
                    break;
            }
            Assert.IsTrue(controller.Session.Board.Boxes.Any(box => box.IsClosed), "a box should have been packed");

            for (int i = 0; i < played; i++)
            {
                controller.Undo();
                yield return WaitUntilIdle(controller);
            }

            Assert.AreEqual(start, controller.Session.Board.GetStateKey());
            Assert.AreEqual(0, controller.MovesMade);
            Assert.IsFalse(controller.Hud.UndoInteractable);
            AssertViewMatchesModel(controller);
            Assert.IsFalse(View.Boxes.Any(box => box.ParcelVisible));
        }

        [UnityTest]
        public IEnumerator TheExtraBoxCanBeAddedOnceAndUndone()
        {
            var controller = StartGame(6);
            yield return null;
            int boxes = controller.Session.Board.Boxes.Count;

            controller.AddExtraBox();
            yield return WaitUntilIdle(controller);
            controller.AddExtraBox();
            yield return WaitUntilIdle(controller);

            Assert.AreEqual(boxes + 1, controller.Session.Board.Boxes.Count, "only one extra box per level");
            Assert.AreEqual(boxes + 1, View.Boxes.Count);
            Assert.IsFalse(controller.Hud.AddBoxInteractable);

            controller.Undo();
            yield return WaitUntilIdle(controller);

            Assert.AreEqual(boxes, controller.Session.Board.Boxes.Count);
            Assert.AreEqual(boxes, View.Boxes.Count);
            Assert.IsTrue(controller.Hud.AddBoxInteractable, "undoing the box gives the use back");
        }

        [UnityTest]
        public IEnumerator RestartResetsTheLevel()
        {
            var controller = StartGame(6);
            yield return null;
            string start = controller.Session.Board.GetStateKey();
            yield return PlayMove(controller, Solution(controller)[0]);

            controller.Restart();
            yield return null;

            Assert.AreEqual(start, controller.Session.Board.GetStateKey());
            Assert.AreEqual(0, controller.MovesMade);
            Assert.AreEqual(6, controller.LevelNumber);
            AssertViewMatchesModel(controller);
        }

        [UnityTest]
        public IEnumerator TappingEmptySpaceOrAClosedBoxSelectsNothing()
        {
            var controller = StartGame(6);
            yield return null;

            controller.TapBox(-1);
            Assert.AreEqual(-1, controller.SelectedBox);

            int emptyBox = controller.Session.Board.Boxes.ToList().FindIndex(box => box.IsEmpty);
            controller.TapBox(emptyBox);
            Assert.AreEqual(-1, controller.SelectedBox, "an empty box cannot be picked up");

            // play until a box is packed, then try to pick it up
            foreach (var move in Solution(controller))
            {
                yield return PlayMove(controller, move);
                if (controller.Session.Board.Boxes.Any(box => box.IsClosed))
                    break;
            }
            int closedBox = controller.Session.Board.Boxes.ToList().FindIndex(box => box.IsClosed);
            Assert.GreaterOrEqual(closedBox, 0);

            controller.TapBox(closedBox);
            Assert.AreEqual(-1, controller.SelectedBox, "a packed box is frozen");
        }

        [UnityTest]
        public IEnumerator HitTestFindsTheBoxUnderAWorldPosition()
        {
            var controller = StartGame(20);
            yield return null;

            for (int i = 0; i < View.Boxes.Count; i++)
            {
                var box = View.Boxes[i];
                Vector2 middle = box.transform.position + Vector3.up * (box.Height * 0.5f);
                Assert.AreEqual(i, View.HitTest(middle));
            }
            Assert.AreEqual(-1, View.HitTest(new Vector2(500f, 500f)));
        }

        [UnityTest]
        public IEnumerator EveryBoxFitsOnTheScreenForEveryKindOfLevel()
        {
            var controller = StartGame(1);
            yield return null;
            var camera = Camera.main;

            // A normal phone, a tall phone and a tablet, all in portrait.
            foreach (float aspect in new[] { 9f / 16f, 9f / 20f, 3f / 4f })
            {
                camera.aspect = aspect;

                foreach (int level in new[] { 1, 6, 20, 60, 150, 200 })
                {
                    controller.LoadLevel(level);
                    yield return null;

                    foreach (var box in View.Boxes)
                    {
                        Vector3 origin = box.transform.position;
                        var bottom = camera.WorldToViewportPoint(origin);
                        var topRight = camera.WorldToViewportPoint(origin + new Vector3(0.5f, box.Height + BoxView.LiftHeight, 0f));
                        var topLeft = camera.WorldToViewportPoint(origin + new Vector3(-0.5f, box.Height + BoxView.LiftHeight, 0f));
                        string where = $"aspect {aspect:F2}, level {level}";

                        Assert.GreaterOrEqual(topLeft.x, 0f, where);
                        Assert.LessOrEqual(topRight.x, 1f, where);
                        Assert.GreaterOrEqual(bottom.y, GameHud.BottomReserve - 0.001f, where + " (bottom)");
                        Assert.LessOrEqual(topRight.y, 1f - GameHud.TopReserve + 0.001f, where + " (top)");
                    }
                }
            }
        }

#if UNITY_EDITOR
        // What happens when the player opens the scene and presses Play. The game saves progress and language,
        // so the test puts the real saved values back afterwards.
        [UnityTest]
        public IEnumerator TheMainSceneStartsTheGameByItself()
        {
            const string levelKey = "level.current", languageKey = "language";
            bool hadLevel = PlayerPrefs.HasKey(levelKey), hadLanguage = PlayerPrefs.HasKey(languageKey);
            int level = PlayerPrefs.GetInt(levelKey, 1);
            string language = PlayerPrefs.GetString(languageKey, "");

            try
            {
                yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                    "Assets/_Game/Scenes/Main.unity", new UnityEngine.SceneManagement.LoadSceneParameters());
                yield return null;
                yield return null;

                var bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
                Assert.IsNotNull(bootstrap, "the scene needs a GameBootstrap");
                Assert.IsNotNull(bootstrap.Controller, "the bootstrap should have started the game");
                Assert.GreaterOrEqual(bootstrap.Controller.LevelNumber, 1);
                Assert.IsNotNull(Camera.main);
                Assert.IsNotNull(Object.FindFirstObjectByType<EventSystem>(), "buttons need an EventSystem");

                _bootstrap = bootstrap;
            }
            finally
            {
                if (hadLevel) PlayerPrefs.SetInt(levelKey, level); else PlayerPrefs.DeleteKey(levelKey);
                if (hadLanguage) PlayerPrefs.SetString(languageKey, language); else PlayerPrefs.DeleteKey(languageKey);
                PlayerPrefs.Save();
            }
        }
#endif

        [Test]
        public void TheArtHasALookForEveryCandyTypeTheLevelsCanUse()
        {
            Assert.GreaterOrEqual(CandyArt.TypeCount, LevelCurve.MaxCandyTypes);
            for (int type = 0; type < CandyArt.TypeCount; type++)
                Assert.IsNotNull(CandyArt.Candy(type), $"type {type}");
        }

        // ---- helpers ----

        private static int FirstPickableBox(GameController controller) =>
            controller.Session.Board.Boxes.ToList().FindIndex(box => !box.IsEmpty && !box.IsClosed);

        // The view must show exactly what the model says: candy types per box, in order, and parcels on closed boxes.
        private void AssertViewMatchesModel(GameController controller)
        {
            var board = controller.Session.Board;
            Assert.AreEqual(board.Boxes.Count, View.Boxes.Count);

            for (int i = 0; i < board.Boxes.Count; i++)
            {
                CollectionAssert.AreEqual(board.Boxes[i].Candies.ToArray(),
                    View.Boxes[i].Candies.Select(candy => candy.Type).ToArray(), $"box {i}");
                Assert.AreEqual(board.Boxes[i].IsClosed, View.Boxes[i].ParcelVisible, $"box {i} parcel");

                // every candy sits in its slot
                for (int slot = 0; slot < View.Boxes[i].Candies.Count; slot++)
                {
                    var candy = View.Boxes[i].Candies[slot];
                    Assert.AreEqual(View.Boxes[i].transform, candy.transform.parent, $"box {i} slot {slot}");
                    Assert.AreEqual(0f, Vector3.Distance(View.Boxes[i].SlotLocal(slot), candy.transform.localPosition), 0.02f, $"box {i} slot {slot}");
                }
            }
        }
    }
}
