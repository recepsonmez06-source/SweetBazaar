using System.Collections;
using System.Linq;
using NUnit.Framework;
using SweetBazaar.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace SweetBazaar.Game.PlayTests
{
    // The per-level helps (undo, extra box), the gold paid for won levels and the candy shop.
    public class RightsAndShopTests
    {
        private GameBootstrap _bootstrap;

        private GameController StartGame(int level, bool persist = false)
        {
            var root = new GameObject("Game");
            _bootstrap = root.AddComponent<GameBootstrap>();
            _bootstrap.PersistProgress = persist;
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
                if (waited > 15f)
                    Assert.Fail("The animation never finished.");
                yield return null;
            }
        }

        private static IEnumerator PlayMove(GameController controller, Move move)
        {
            controller.TapBox(move.From);
            yield return WaitUntilIdle(controller);
            controller.TapBox(move.To);
            yield return WaitUntilIdle(controller);
        }

        private static Move[] Solution(GameController controller) =>
            Solver.Solve(controller.Session.Board.Clone()).Moves.ToArray();

        private static int CandyTypesOfLevel(int number) =>
            LevelPackJson.Parse(Resources.Load<TextAsset>("Levels/levels").text).Get(number).Definition.CountCandyTypes();

        // ---- the helps of a level ----

        [UnityTest]
        public IEnumerator TheButtonsShowHowManyUsesAreLeft()
        {
            var controller = StartGame(6);
            yield return null;

            Assert.AreEqual("5", controller.Hud.UndoBadgeText);
            Assert.AreEqual("1", controller.Hud.AddBoxBadgeText);

            yield return PlayMove(controller, Solution(controller)[0]);
            controller.Undo();
            yield return WaitUntilIdle(controller);
            Assert.AreEqual("4", controller.Hud.UndoBadgeText, "an undo costs one");

            controller.AddExtraBox();
            yield return WaitUntilIdle(controller);
            Assert.AreEqual("0", controller.Hud.AddBoxBadgeText);
        }

        [UnityTest]
        public IEnumerator UndoStopsWorkingWhenTheUndosRunOut()
        {
            var controller = StartGame(6);
            yield return null;
            var move = Solution(controller)[0];

            for (int i = 0; i < Allowance.DefaultUndos; i++)
            {
                yield return PlayMove(controller, move);
                controller.Undo();
                yield return WaitUntilIdle(controller);
            }
            Assert.AreEqual(0, controller.Allowance.UndosLeft);

            yield return PlayMove(controller, move);
            controller.Undo();
            yield return WaitUntilIdle(controller);

            Assert.AreEqual(1, controller.MovesMade, "the move stays: there was no undo left");
            Assert.IsFalse(controller.Hud.UndoInteractable);
            Assert.AreEqual("0", controller.Hud.UndoBadgeText);
        }

        [UnityTest]
        public IEnumerator UndoingAnExtraBoxIsFreeAndGivesTheBoxBack()
        {
            var controller = StartGame(6);
            yield return null;

            controller.AddExtraBox();
            yield return WaitUntilIdle(controller);
            controller.Undo();
            yield return WaitUntilIdle(controller);

            Assert.AreEqual(Allowance.DefaultUndos, controller.Allowance.UndosLeft, "no undo was spent");
            Assert.AreEqual(1, controller.Allowance.ExtraBoxesLeft);
        }

        [UnityTest]
        public IEnumerator RestartingGivesAFreshAllowance()
        {
            var controller = StartGame(6);
            yield return null;
            yield return PlayMove(controller, Solution(controller)[0]);
            controller.Undo();
            yield return WaitUntilIdle(controller);
            controller.AddExtraBox();
            yield return WaitUntilIdle(controller);

            controller.Restart();
            yield return null;

            Assert.AreEqual(Allowance.DefaultUndos, controller.Allowance.UndosLeft);
            Assert.AreEqual(Allowance.DefaultExtraBoxes, controller.Allowance.ExtraBoxesLeft);
            Assert.AreEqual("5", controller.Hud.UndoBadgeText);
        }

        // ---- gold ----

        [UnityTest]
        public IEnumerator WinningALevelPaysGoldWithACleanSolveBonusAndShowsTheCandyMaker()
        {
            var controller = StartGame(1);
            yield return null;

            foreach (var move in Solution(controller))
                yield return PlayMove(controller, move);

            int expected = ShopRules.GoldForLevel(CandyTypesOfLevel(1), withoutHelp: true);
            Assert.IsTrue(controller.Hud.WinVisible);
            Assert.AreEqual(expected, controller.Shop.Gold);
            Assert.AreEqual(expected.ToString(), controller.Hud.GoldText);
        }

        [UnityTest]
        public IEnumerator UsingAnUndoRemovesTheCleanSolveBonus()
        {
            var controller = StartGame(1);
            yield return null;
            var solution = Solution(controller);

            yield return PlayMove(controller, solution[0]);
            controller.Undo();
            yield return WaitUntilIdle(controller);
            foreach (var move in Solution(controller))
                yield return PlayMove(controller, move);

            Assert.AreEqual(ShopRules.GoldForLevel(CandyTypesOfLevel(1), withoutHelp: false), controller.Shop.Gold);
        }

        // ---- the candy shop ----

        [UnityTest]
        public IEnumerator TheShopOpensBlocksTheBoardAndCloses()
        {
            var controller = StartGame(6);
            yield return null;

            controller.OpenShop();
            Assert.IsTrue(controller.Hud.ShopVisible);
            controller.TapBox(0);
            Assert.AreEqual(-1, controller.SelectedBox, "the board ignores taps while the shop is open");

            controller.CloseShop();
            Assert.IsFalse(controller.Hud.ShopVisible);
            controller.TapBox(controller.Session.Board.Boxes.ToList().FindIndex(b => !b.IsEmpty && !b.IsClosed));
            Assert.GreaterOrEqual(controller.SelectedBox, 0);
        }

        [UnityTest]
        public IEnumerator TheShopCannotBeUpgradedWithoutGold()
        {
            var controller = StartGame(6);
            yield return null;

            controller.OpenShop();
            controller.UpgradeShop();

            Assert.AreEqual(0, controller.Shop.Stage);
            Assert.IsFalse(controller.Hud.UpgradeInteractable);
        }

        [UnityTest]
        public IEnumerator TheShopGrowsWhenThereIsEnoughGold()
        {
            var controller = StartGame(6);
            yield return null;
            controller.Shop.AddGold(ShopStages.CostOf(1) + 15);

            controller.OpenShop();
            Assert.IsTrue(controller.Hud.UpgradeInteractable);
            controller.UpgradeShop();

            Assert.AreEqual(1, controller.Shop.Stage);
            Assert.AreEqual(15, controller.Shop.Gold);
            Assert.AreEqual("15", controller.Hud.GoldText);
            Assert.IsTrue(controller.Hud.ShopVisible, "the shop stays open to show the new stage");
        }

        [UnityTest]
        public IEnumerator TheShopStageAndGoldAreRememberedBetweenGames()
        {
            // the test touches the real saved values, so it puts them back afterwards
            string[] keys = { "shop.gold", "shop.stage", "level.current" };
            var hadKey = keys.ToDictionary(k => k, PlayerPrefs.HasKey);
            int savedGold = PlayerPrefs.GetInt("shop.gold", 0), savedStage = PlayerPrefs.GetInt("shop.stage", 0);
            int savedLevel = PlayerPrefs.GetInt("level.current", 1);

            try
            {
                PlayerPrefs.DeleteKey("shop.gold");
                PlayerPrefs.DeleteKey("shop.stage");
                PlayerPrefs.SetInt("level.current", 1);

                var first = StartGame(0, persist: true);
                yield return null;
                first.Shop.AddGold(ShopStages.CostOf(1) + 5);
                first.OpenShop();
                first.UpgradeShop();
                Object.Destroy(_bootstrap.gameObject);
                foreach (var camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                    Object.Destroy(camera.gameObject);
                yield return null;

                var second = StartGame(0, persist: true);
                yield return null;

                Assert.AreEqual(1, second.Shop.Stage);
                Assert.AreEqual(5, second.Shop.Gold);
            }
            finally
            {
                if (hadKey["shop.gold"]) PlayerPrefs.SetInt("shop.gold", savedGold); else PlayerPrefs.DeleteKey("shop.gold");
                if (hadKey["shop.stage"]) PlayerPrefs.SetInt("shop.stage", savedStage); else PlayerPrefs.DeleteKey("shop.stage");
                if (hadKey["level.current"]) PlayerPrefs.SetInt("level.current", savedLevel); else PlayerPrefs.DeleteKey("level.current");
                PlayerPrefs.Save();
            }
        }
    }
}
