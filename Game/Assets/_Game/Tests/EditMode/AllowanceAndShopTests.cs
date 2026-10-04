using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SweetBazaar.Core.Tests
{
    public class AllowanceTests
    {
        [Test]
        public void ANewLevel_StartsWithTheDefaultHelps()
        {
            var allowance = new Allowance();

            Assert.AreEqual(5, allowance.UndosLeft);
            Assert.AreEqual(1, allowance.ExtraBoxesLeft);
            Assert.IsFalse(allowance.AnyHelpUsed);
            Assert.IsFalse(allowance.IsUnlimited);
        }

        [Test]
        public void UsingAnUndo_CostsOneAndMarksTheLevelAsHelped()
        {
            var allowance = new Allowance(undos: 2);

            Assert.IsTrue(allowance.TryUseUndo());

            Assert.AreEqual(1, allowance.UndosLeft);
            Assert.IsTrue(allowance.AnyHelpUsed);
        }

        [Test]
        public void WhenNoUndosAreLeft_TheNextOneIsRefused()
        {
            var allowance = new Allowance(undos: 1);
            allowance.TryUseUndo();

            Assert.IsFalse(allowance.CanUndo);
            Assert.IsFalse(allowance.TryUseUndo());
            Assert.AreEqual(0, allowance.UndosLeft);
        }

        [Test]
        public void TheExtraBox_CanBeUsedOnceByDefault()
        {
            var allowance = new Allowance();

            Assert.IsTrue(allowance.TryUseExtraBox());
            Assert.IsFalse(allowance.CanAddExtraBox);
            Assert.IsFalse(allowance.TryUseExtraBox());
        }

        [Test]
        public void GivingTheExtraBoxBack_RestoresIt()
        {
            var allowance = new Allowance();
            allowance.TryUseExtraBox();

            allowance.GiveExtraBoxBack();

            Assert.AreEqual(1, allowance.ExtraBoxesLeft);
        }

        [Test]
        public void Grant_AddsHelps()
        {
            var allowance = new Allowance(undos: 0, extraBoxes: 0);

            allowance.Grant(undos: 3, extraBoxes: 1);

            Assert.AreEqual(3, allowance.UndosLeft);
            Assert.AreEqual(1, allowance.ExtraBoxesLeft);
        }

        [Test]
        public void NegativeAmounts_AreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Allowance(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Allowance(0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Allowance().Grant(-1, 0));
        }

        [Test]
        public void UnlimitedHelps_NeverRunOutButStillCountAsHelp()
        {
            var allowance = Allowance.Unlimited();

            for (int i = 0; i < 50; i++)
            {
                Assert.IsTrue(allowance.TryUseUndo());
                Assert.IsTrue(allowance.TryUseExtraBox());
            }

            Assert.IsTrue(allowance.IsUnlimited);
            Assert.IsTrue(allowance.CanUndo);
            Assert.IsTrue(allowance.CanAddExtraBox);
            Assert.IsTrue(allowance.AnyHelpUsed);
        }

        [Test]
        public void TheTutorialLevels_HaveUnlimitedHelps_TheOthersDoNot()
        {
            for (int level = 1; level <= LevelCurve.TutorialLevels; level++)
                Assert.IsTrue(Allowance.ForLevel(level).IsUnlimited, $"level {level}");

            Assert.IsFalse(Allowance.ForLevel(LevelCurve.TutorialLevels + 1).IsUnlimited);
            Assert.AreEqual(Allowance.DefaultUndos, Allowance.ForLevel(50).UndosLeft);
        }
    }

    public class ShopTests
    {
        [Test]
        public void TheShopStartsWithOnlyTheCounterAndNoGold()
        {
            var shop = new Shop();

            Assert.AreEqual(0, shop.Gold);
            Assert.AreEqual(1, shop.BuiltCount);
            Assert.IsTrue(shop.IsBuilt(ShopPlace.Counter));
            Assert.AreEqual(0, shop.StyleOf(ShopPlace.Counter));
            foreach (var place in new[] { ShopPlace.Display, ShopPlace.Sign, ShopPlace.TeaCorner, ShopPlace.Facade, ShopPlace.Decor })
            {
                Assert.IsFalse(shop.IsBuilt(place), place.ToString());
                Assert.AreEqual(Shop.NotBuilt, shop.StyleOf(place));
            }
        }

        [Test]
        public void ThePurchasesGetMoreExpensive()
        {
            for (int n = 2; n <= ShopCatalog.PurchaseCount; n++)
                Assert.Greater(ShopCatalog.CostOfPurchase(n), ShopCatalog.CostOfPurchase(n - 1), $"purchase {n}");

            Assert.AreEqual(ShopCatalog.PlaceCount - 1, ShopCatalog.PurchaseCount, "every place but the counter is bought");
        }

        [Test]
        public void APlaceIsBuiltInTheStyleThePlayerChooses_AndPaidFor()
        {
            var shop = new Shop(gold: ShopCatalog.CostOfPurchase(1) + 7);

            Assert.IsTrue(shop.CanBuild(ShopPlace.Sign));
            Assert.IsTrue(shop.TryBuild(ShopPlace.Sign, 2));

            Assert.IsTrue(shop.IsBuilt(ShopPlace.Sign));
            Assert.AreEqual(2, shop.StyleOf(ShopPlace.Sign));
            Assert.AreEqual(7, shop.Gold);
            Assert.AreEqual(2, shop.BuiltCount);
        }

        [Test]
        public void NothingIsBuiltAutomatically()
        {
            var shop = new Shop(gold: 100000);

            // plenty of gold, but only what the player asks for gets built
            Assert.AreEqual(1, shop.BuiltCount);
            shop.TryBuild(ShopPlace.Decor, 1);
            Assert.AreEqual(2, shop.BuiltCount);
            Assert.IsFalse(shop.IsBuilt(ShopPlace.Sign));
        }

        [Test]
        public void ThePlayerCanBuildInAnyOrder()
        {
            var shop = new Shop(gold: 100000);

            Assert.IsTrue(shop.TryBuild(ShopPlace.Facade, 0));
            Assert.IsTrue(shop.TryBuild(ShopPlace.TeaCorner, 1));
            Assert.IsTrue(shop.TryBuild(ShopPlace.Display, 2));

            Assert.AreEqual(4, shop.BuiltCount);
        }

        [Test]
        public void ThePriceDependsOnHowManyPlacesAreBuilt_NotOnWhichOne()
        {
            var first = new Shop(gold: 100000);
            var second = new Shop(gold: 100000);
            int before = first.Gold;

            first.TryBuild(ShopPlace.Facade, 0);
            second.TryBuild(ShopPlace.Decor, 0);

            Assert.AreEqual(before - first.Gold, 100000 - second.Gold);
            Assert.AreEqual(ShopCatalog.CostOfPurchase(2), first.NextCost);
        }

        [Test]
        public void WithoutEnoughGold_NothingIsBuilt()
        {
            var shop = new Shop(gold: ShopCatalog.CostOfPurchase(1) - 1);

            Assert.IsFalse(shop.CanBuild(ShopPlace.Sign));
            Assert.IsFalse(shop.TryBuild(ShopPlace.Sign, 0));
            Assert.IsFalse(shop.IsBuilt(ShopPlace.Sign));
            Assert.AreEqual(1, shop.GoldMissing);
            Assert.AreEqual(ShopCatalog.CostOfPurchase(1) - 1, shop.Gold);
        }

        [Test]
        public void APlaceCannotBeBuiltTwice_AndAnInvalidStyleIsRefused()
        {
            var shop = new Shop(gold: 100000);
            shop.TryBuild(ShopPlace.Sign, 0);

            Assert.IsFalse(shop.TryBuild(ShopPlace.Sign, 1), "already built");
            Assert.IsFalse(shop.TryBuild(ShopPlace.Display, 3), "there are only 3 styles");
            Assert.IsFalse(shop.TryBuild(ShopPlace.Display, -1));
            Assert.AreEqual(0, shop.StyleOf(ShopPlace.Sign));
        }

        [Test]
        public void TheStyleOfABuiltPlaceCanBeChangedForFree()
        {
            var shop = new Shop(gold: 100000);
            shop.TryBuild(ShopPlace.Sign, 0);
            int goldBefore = shop.Gold;

            Assert.IsTrue(shop.TrySetStyle(ShopPlace.Sign, 2));
            Assert.IsTrue(shop.TrySetStyle(ShopPlace.Counter, 1));

            Assert.AreEqual(2, shop.StyleOf(ShopPlace.Sign));
            Assert.AreEqual(1, shop.StyleOf(ShopPlace.Counter));
            Assert.AreEqual(goldBefore, shop.Gold);
        }

        [Test]
        public void AStyleCannotBeSetOnAPlaceThatIsNotBuilt()
        {
            var shop = new Shop(gold: 100000);

            Assert.IsFalse(shop.TrySetStyle(ShopPlace.Decor, 1));
            Assert.IsFalse(shop.IsBuilt(ShopPlace.Decor));
        }

        [Test]
        public void WhenEverythingIsBuilt_NothingMoreCanBeBought()
        {
            var shop = new Shop(gold: 1000000);
            foreach (ShopPlace place in Enum.GetValues(typeof(ShopPlace)))
                shop.TryBuild(place, 0);

            Assert.IsTrue(shop.IsFullyBuilt);
            Assert.AreEqual(0, shop.NextCost);
            Assert.IsFalse(shop.CanBuild(ShopPlace.Sign));
        }

        [Test]
        public void SavedStyles_AreReadBack_AndGarbageGivesTheStartingShop()
        {
            var shop = new Shop(gold: 100000);
            shop.TryBuild(ShopPlace.TeaCorner, 2);
            shop.TrySetStyle(ShopPlace.Counter, 1);

            var restored = new Shop(shop.Gold, Shop.ParseStyles(shop.SerializeStyles()));

            CollectionAssert.AreEqual(shop.Styles, restored.Styles);
            Assert.AreEqual(1, new Shop(0, Shop.ParseStyles("nonsense")).BuiltCount);
            Assert.AreEqual(1, new Shop(0, Shop.ParseStyles(null)).BuiltCount);
            Assert.AreEqual(1, new Shop(0, Shop.ParseStyles("")).BuiltCount);
        }

        [Test]
        public void ValuesOutsideTheRange_AreIgnored()
        {
            var shop = new Shop(gold: -5, styles: new[] { 7, -9, 2, 99, 1, 0 });

            Assert.AreEqual(0, shop.Gold);
            Assert.AreEqual(0, shop.StyleOf(ShopPlace.Counter), "the counter always exists");
            Assert.IsFalse(shop.IsBuilt(ShopPlace.Display));
            Assert.AreEqual(2, shop.StyleOf(ShopPlace.Sign));
            Assert.IsFalse(shop.IsBuilt(ShopPlace.TeaCorner));
            Assert.AreEqual(1, shop.StyleOf(ShopPlace.Facade));
            Assert.AreEqual(0, shop.StyleOf(ShopPlace.Decor));
        }

        [Test]
        public void AddGold_RejectsNegativeAmounts()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Shop().AddGold(-1));
        }

        [Test]
        public void HarderLevelsAndCleanSolves_PayMoreGold()
        {
            Assert.AreEqual(ShopRules.GoldForLevel(5, false) + ShopRules.CleanSolveBonus, ShopRules.GoldForLevel(5, true));
            Assert.Greater(ShopRules.GoldForLevel(9, false), ShopRules.GoldForLevel(3, false));
        }

        [Test]
        public void ThePacing_FirstPurchaseAfterAboutThreeEasyLevels()
        {
            int perLevel = ShopRules.GoldForLevel(3, withoutHelp: true);

            int levels = (int)Math.Ceiling(ShopCatalog.CostOfPurchase(1) / (double)perLevel);

            Assert.That(levels, Is.InRange(2, 5));
        }

        [Test]
        public void ThePacing_TheWholeShopCanBeBuiltWithinTheShippedLevels()
        {
            var pack = LevelPackJson.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(
                UnityEngine.Application.dataPath, "_Game", "Resources", "Levels", "levels.json")));

            int total = 0;
            for (int n = 1; n <= ShopCatalog.PurchaseCount; n++)
                total += ShopCatalog.CostOfPurchase(n);

            int gold = 0;
            int levelsNeeded = -1;
            foreach (var record in pack.Levels)
            {
                gold += ShopRules.GoldForLevel(record.Definition.CountCandyTypes(), withoutHelp: false);
                if (levelsNeeded < 0 && gold >= total)
                    levelsNeeded = record.Number;
            }

            Assert.Greater(levelsNeeded, 120, "the shop must stay a goal for a long time (it was finished by level 110 once)");
            Assert.LessOrEqual(levelsNeeded, pack.Count, "the last place must be reachable within the shipped levels");
        }

        [Test]
        public void ThePacing_EachPurchaseTakesLongerThanThePreviousOne()
        {
            var pack = LevelPackJson.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(
                UnityEngine.Application.dataPath, "_Game", "Resources", "Levels", "levels.json")));

            int gold = 0, purchase = 1, previousLevel = 0, previousGap = 0, cost = ShopCatalog.CostOfPurchase(1);
            foreach (var record in pack.Levels)
            {
                gold += ShopRules.GoldForLevel(record.Definition.CountCandyTypes(), withoutHelp: false);
                if (gold < cost)
                    continue;

                int gap = record.Number - previousLevel;
                Assert.GreaterOrEqual(gap, previousGap, $"purchase {purchase} should not come sooner after the last one than the one before did");
                previousGap = gap;
                previousLevel = record.Number;
                gold -= cost;

                purchase++;
                if (purchase > ShopCatalog.PurchaseCount)
                    break;
                cost = ShopCatalog.CostOfPurchase(purchase);
            }
            Assert.Greater(purchase, ShopCatalog.PurchaseCount, "every purchase must be reachable");
        }

        [Test]
        public void SpendingGold_TakesItOrChangesNothing()
        {
            var shop = new Shop(gold: 20);

            Assert.IsFalse(shop.TrySpend(21));
            Assert.AreEqual(20, shop.Gold);

            Assert.IsTrue(shop.TrySpend(ShopRules.UndoPrice));
            Assert.AreEqual(20 - ShopRules.UndoPrice, shop.Gold);
            Assert.Throws<ArgumentOutOfRangeException>(() => shop.TrySpend(-1));
        }

        [Test]
        public void ExtraHelpsCostLessThanAWonLevelPays_ButNotNothing()
        {
            int perLevel = ShopRules.GoldForLevel(5, withoutHelp: false);
            Assert.Greater(ShopRules.UndoPrice, 0);
            Assert.Less(ShopRules.UndoPrice, perLevel);
            Assert.Greater(ShopRules.ExtraBoxPrice, ShopRules.UndoPrice);
            Assert.LessOrEqual(ShopRules.ExtraBoxPrice, perLevel * 2);
        }

        [Test]
        public void EveryPlaceHasANameAndEveryStyleHasAName()
        {
            foreach (ShopPlace place in Enum.GetValues(typeof(ShopPlace)))
            {
                Assert.IsNotEmpty(LocKeys.ShopPlaceName(place));
                for (int style = 0; style < ShopCatalog.StylesPerPlace; style++)
                    Assert.IsNotEmpty(LocKeys.ShopStyleName(place, style), $"{place} {style}");
            }
            Assert.AreEqual(ShopCatalog.PlaceCount, Enum.GetValues(typeof(ShopPlace)).Length);
        }
    }

    public class LevelDefinitionTypeCountTests
    {
        [Test]
        public void CountCandyTypes_CountsDistinctTypes()
        {
            var level = new LevelDefinition
            {
                BoxCapacity = 4,
                Boxes = new List<int[]> { new[] { 0, 1, 1, 0 }, new[] { 2, 2, 2, 2 }, new int[0] },
            };

            Assert.AreEqual(3, level.CountCandyTypes());
        }
    }
}
