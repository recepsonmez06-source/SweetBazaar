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

        // ---- tutorial levels have no counters ----

        [UnityTest]
        public IEnumerator TheTutorialLevelsHaveUnlimitedHelpsWithoutCounters()
        {
            var tutorial = StartGame(3);
            yield return null;
            Assert.IsFalse(tutorial.Hud.RightsVisible, "no counters in the tutorial");

            var move = Solution(tutorial)[0];
            for (int i = 0; i < Allowance.DefaultUndos + 2; i++)
            {
                yield return PlayMove(tutorial, move);
                tutorial.Undo();
                yield return WaitUntilIdle(tutorial);
            }
            Assert.AreEqual(0, tutorial.MovesMade, "every undo worked, there was no limit");
            Assert.IsFalse(tutorial.Hud.UndoInteractable, "nothing left to undo: the board is back at the start");

            tutorial.LoadLevel(LevelCurve.TutorialLevels + 1);
            yield return null;
            Assert.IsTrue(tutorial.Hud.RightsVisible, "the counters appear after the tutorial");
            Assert.AreEqual("5", tutorial.Hud.UndoBadgeText);
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
        public IEnumerator TheShopOpensOnAPlaceThatStillHasToBeBuilt()
        {
            var controller = StartGame(6);
            yield return null;

            controller.OpenShop();

            Assert.IsFalse(controller.Shop.IsBuilt(controller.Hud.SelectedShopPlace));
        }

        [UnityTest]
        public IEnumerator NothingCanBeBuiltWithoutGold()
        {
            var controller = StartGame(6);
            yield return null;

            controller.OpenShop();
            controller.Hud.SelectPlace(ShopPlace.Sign);
            Assert.IsFalse(controller.Hud.ShopActionInteractable, "the build button is disabled without enough gold");

            controller.ShopAction(ShopPlace.Sign, 0);

            Assert.IsFalse(controller.Shop.IsBuilt(ShopPlace.Sign));
            Assert.AreEqual(1, controller.Shop.BuiltCount);
        }

        [UnityTest]
        public IEnumerator APlaceIsBuiltInTheStyleThePlayerPicksAndPaidFor()
        {
            var controller = StartGame(6);
            yield return null;
            controller.Shop.AddGold(ShopCatalog.CostOfPurchase(1) + 15);

            controller.OpenShop();
            controller.Hud.SelectPlace(ShopPlace.Sign);
            controller.Hud.SelectStyle(2);
            Assert.IsTrue(controller.Hud.ShopActionInteractable);
            controller.ShopAction(controller.Hud.SelectedShopPlace, controller.Hud.PreviewedShopStyle);

            Assert.IsTrue(controller.Shop.IsBuilt(ShopPlace.Sign));
            Assert.AreEqual(2, controller.Shop.StyleOf(ShopPlace.Sign), "the style the player picked, not a default");
            Assert.AreEqual("15", controller.Hud.GoldText);
            Assert.IsTrue(controller.Hud.ShopVisible, "the shop stays open to show the result");
        }

        [UnityTest]
        public IEnumerator PreviewingAStyleChangesNothingUntilThePlayerConfirms()
        {
            var controller = StartGame(6);
            yield return null;
            controller.Shop.AddGold(1000);

            controller.OpenShop();
            controller.Hud.SelectPlace(ShopPlace.Display);
            controller.Hud.SelectStyle(1);
            controller.Hud.SelectStyle(2);
            controller.Hud.SelectPlace(ShopPlace.Facade);

            Assert.AreEqual(1, controller.Shop.BuiltCount, "looking around builds nothing");
            Assert.AreEqual(1000, controller.Shop.Gold, "and costs nothing");
        }

        [UnityTest]
        public IEnumerator TheStyleOfABuiltPlaceChangesForFree()
        {
            var controller = StartGame(6);
            yield return null;
            controller.Shop.AddGold(ShopCatalog.CostOfPurchase(1));
            controller.OpenShop();
            controller.ShopAction(ShopPlace.Sign, 0);
            Assert.AreEqual(0, controller.Shop.Gold);

            controller.Hud.SelectPlace(ShopPlace.Sign);
            controller.Hud.SelectStyle(1);
            Assert.IsTrue(controller.Hud.ShopActionInteractable);
            controller.ShopAction(ShopPlace.Sign, 1);

            Assert.AreEqual(1, controller.Shop.StyleOf(ShopPlace.Sign));
            Assert.AreEqual(0, controller.Shop.Gold, "changing the style costs nothing");
        }

        [UnityTest]
        public IEnumerator WinningALevelNeverBuildsAnythingOnItsOwn()
        {
            var controller = StartGame(1);
            yield return null;
            foreach (var move in Solution(controller))
                yield return PlayMove(controller, move);

            Assert.IsTrue(controller.Hud.WinVisible);
            Assert.Greater(controller.Shop.Gold, 0);
            Assert.AreEqual(1, controller.Shop.BuiltCount, "the gold is kept until the player decides what to build");
        }

        [UnityTest]
        public IEnumerator TheShopIsRememberedBetweenGames()
        {
            // the test touches the real saved values, so it puts them back afterwards
            string[] keys = { "shop.gold", "shop.styles", "level.current" };
            var hadKey = keys.ToDictionary(k => k, PlayerPrefs.HasKey);
            int savedGold = PlayerPrefs.GetInt("shop.gold", 0);
            string savedStyles = PlayerPrefs.GetString("shop.styles", "");
            int savedLevel = PlayerPrefs.GetInt("level.current", 1);

            try
            {
                PlayerPrefs.DeleteKey("shop.gold");
                PlayerPrefs.DeleteKey("shop.styles");
                PlayerPrefs.SetInt("level.current", 1);

                var first = StartGame(0, persist: true);
                yield return null;
                first.Shop.AddGold(ShopCatalog.CostOfPurchase(1) + 5);
                first.OpenShop();
                first.ShopAction(ShopPlace.Facade, 1);
                Object.Destroy(_bootstrap.gameObject);
                foreach (var camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                    Object.Destroy(camera.gameObject);
                yield return null;

                var second = StartGame(0, persist: true);
                yield return null;

                Assert.IsTrue(second.Shop.IsBuilt(ShopPlace.Facade));
                Assert.AreEqual(1, second.Shop.StyleOf(ShopPlace.Facade));
                Assert.AreEqual(5, second.Shop.Gold);
            }
            finally
            {
                if (hadKey["shop.gold"]) PlayerPrefs.SetInt("shop.gold", savedGold); else PlayerPrefs.DeleteKey("shop.gold");
                if (hadKey["shop.styles"]) PlayerPrefs.SetString("shop.styles", savedStyles); else PlayerPrefs.DeleteKey("shop.styles");
                if (hadKey["level.current"]) PlayerPrefs.SetInt("level.current", savedLevel); else PlayerPrefs.DeleteKey("level.current");
                PlayerPrefs.Save();
            }
        }
    }
}