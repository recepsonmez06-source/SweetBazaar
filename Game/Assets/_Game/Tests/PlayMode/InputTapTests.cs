using System.Collections;
using System.Linq;
using NUnit.Framework;
using SweetBazaar.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace SweetBazaar.Game.PlayTests
{
    // The other flow tests call GameController.TapBox directly. These go through the real input path
    // (Input System pointer -> screen position -> world position -> box under it), like a finger or a mouse click.
    public class InputTapTests : InputTestFixture
    {
        private GameObject _root;

        [TearDown]
        public void DestroyGame()
        {
            if (_root != null)
                Object.Destroy(_root);
            foreach (var camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                Object.Destroy(camera.gameObject);
            foreach (var eventSystem in Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
                Object.Destroy(eventSystem.gameObject);
        }

        private GameController StartGame(int level)
        {
            _root = new GameObject("Game");
            var bootstrap = _root.AddComponent<GameBootstrap>();
            bootstrap.PersistProgress = false;
            bootstrap.StartLevel = level;
            return bootstrap.Initialize();
        }

        private static Vector2 ScreenPointOfBox(BoxView box) =>
            Camera.main.WorldToScreenPoint(box.transform.position + Vector3.up * (box.Height * 0.5f));

        private IEnumerator Click(Mouse mouse, Vector2 screenPoint)
        {
            Set(mouse.position, screenPoint);
            Press(mouse.leftButton);
            yield return null;
            Release(mouse.leftButton);
            yield return null;
        }

        private static IEnumerator WaitUntilIdle(GameController controller)
        {
            float waited = 0f;
            while (controller.IsBusy)
            {
                waited += Time.deltaTime;
                if (waited > 15f)
                    Assert.Fail("The animation never finished.");
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator ClickingABoxSelectsItAndClickingAnotherOneMovesTheCandies()
        {
            var mouse = InputSystem.AddDevice<Mouse>();
            var controller = StartGame(6);
            yield return null;

            var view = _root.GetComponentInChildren<BoardView>();
            var move = Solver.Solve(controller.Session.Board.Clone()).Moves[0];

            yield return Click(mouse, ScreenPointOfBox(view.Boxes[move.From]));
            Assert.AreEqual(move.From, controller.SelectedBox, "the click should pick up the box under the pointer");

            yield return Click(mouse, ScreenPointOfBox(view.Boxes[move.To]));
            yield return WaitUntilIdle(controller);

            Assert.AreEqual(1, controller.MovesMade, "the second click should have made the move");
            Assert.AreEqual(-1, controller.SelectedBox);
        }

        [UnityTest]
        public IEnumerator ClickingEmptySpaceDropsTheSelection()
        {
            var mouse = InputSystem.AddDevice<Mouse>();
            var controller = StartGame(6);
            yield return null;

            var view = _root.GetComponentInChildren<BoardView>();
            int box = controller.Session.Board.Boxes.ToList().FindIndex(b => !b.IsEmpty && !b.IsClosed);
            yield return Click(mouse, ScreenPointOfBox(view.Boxes[box]));
            Assert.AreEqual(box, controller.SelectedBox);

            yield return Click(mouse, new Vector2(2f, 2f));

            Assert.AreEqual(-1, controller.SelectedBox);
        }

        [UnityTest]
        public IEnumerator HoldingThePointerDownDoesNotTapEveryFrame()
        {
            var mouse = InputSystem.AddDevice<Mouse>();
            var controller = StartGame(6);
            yield return null;

            var view = _root.GetComponentInChildren<BoardView>();
            int box = controller.Session.Board.Boxes.ToList().FindIndex(b => !b.IsEmpty && !b.IsClosed);

            // Press once and keep holding for several frames: it must count as ONE tap (select), not select/deselect.
            Set(mouse.position, ScreenPointOfBox(view.Boxes[box]));
            Press(mouse.leftButton);
            for (int frame = 0; frame < 8; frame++)
                yield return null;

            Assert.AreEqual(box, controller.SelectedBox);
            Release(mouse.leftButton);
        }

        [UnityTest]
        public IEnumerator TheGameKeepsWorkingWhenItsRuntimeStateIsLost()
        {
            // After scripts are recompiled during play mode in the editor, the controller keeps existing but its
            // private runtime fields are gone. It must stay quiet instead of throwing every frame.
            var mouse = InputSystem.AddDevice<Mouse>();
            var controller = StartGame(6);
            yield return null;

            var fields = typeof(GameController).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            foreach (var name in new[] { "_hud", "_boardView", "_camera", "_session" })
                fields.First(f => f.Name == name).SetValue(controller, null);

            Set(mouse.position, new Vector2(100f, 100f));
            Press(mouse.leftButton);
            yield return null;
            yield return null;
            Release(mouse.leftButton);

            LogAssert.NoUnexpectedReceived();
        }
    }
}
