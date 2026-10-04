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
    }

    public class ShopTests
    {
        [Test]
        public void TheShopStartsAsALittleCounterWithoutGold()
        {
            var shop = new Shop();

            Assert.AreEqual(0, shop.Gold);
            Assert.AreEqual(0, shop.Stage);
            Assert.IsFalse(shop.IsFullyBuilt);
            Assert.AreEqual(ShopStages.CostOf(1), shop.NextCost);
        }

        [Test]
        public void TheStagesGetMoreExpensive()
        {
            for (int stage = 2; stage < ShopStages.Count; stage++)
                Assert.Greater(ShopStages.CostOf(stage), ShopStages.CostOf(stage - 1), $"stage {stage}");

            Assert.AreEqual(0, ShopStages.CostOf(0));
            Assert.AreEqual(6, ShopStages.Count);
        }

        [Test]
        public void ItCannotBeUpgradedWithoutEnoughGold()
        {
            var shop = new Shop(gold: ShopStages.CostOf(1) - 1);

            Assert.IsFalse(shop.CanUpgrade);
            Assert.IsFalse(shop.TryUpgrade());
            Assert.AreEqual(0, shop.Stage);
            Assert.AreEqual(1, shop.GoldMissing);
        }

        [Test]
        public void UpgradingCostsGoldAndBuildsTheNextStage()
        {
            var shop = new Shop(gold: ShopStages.CostOf(1) + 7);

            Assert.IsTrue(shop.TryUpgrade());

            Assert.AreEqual(1, shop.Stage);
            Assert.AreEqual(7, shop.Gold);
            Assert.AreEqual(ShopStages.CostOf(2), shop.NextCost);
        }

        [Test]
        public void AfterTheLastStage_NothingMoreCanBeBought()
        {
            var shop = new Shop(gold: 1000000, stage: ShopStages.Count - 1);

            Assert.IsTrue(shop.IsFullyBuilt);
            Assert.IsFalse(shop.CanUpgrade);
            Assert.IsFalse(shop.TryUpgrade());
            Assert.AreEqual(0, shop.NextCost);
            Assert.AreEqual(1000000, shop.Gold);
        }

        [Test]
        public void SavedValuesOutsideTheRange_AreClamped()
        {
            var shop = new Shop(gold: -5, stage: 99);

            Assert.AreEqual(0, shop.Gold);
            Assert.AreEqual(ShopStages.Count - 1, shop.Stage);
            Assert.AreEqual(0, new Shop(0, -3).Stage);
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
        public void ThePacing_FirstUpgradeAfterAboutThreeEasyLevels()
        {
            // Easy early levels (2-3 candy types, clean solves) should buy the first upgrade within a handful of levels.
            int perLevel = ShopRules.GoldForLevel(3, withoutHelp: true);

            int levels = (int)Math.Ceiling(ShopStages.CostOf(1) / (double)perLevel);

            Assert.That(levels, Is.InRange(2, 5));
        }

        [Test]
        public void ThePacing_TheWholeShopCanBeBuiltWithinTheShippedLevels()
        {
            // Over 200 levels with the gold the levels actually pay, the last stage must be reachable but not trivially.
            var pack = LevelPackJson.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(
                UnityEngine.Application.dataPath, "_Game", "Resources", "Levels", "levels.json")));

            int gold = 0;
            int levelsNeeded = -1;
            foreach (var record in pack.Levels)
            {
                gold += ShopRules.GoldForLevel(record.Definition.CountCandyTypes(), withoutHelp: false);
                if (levelsNeeded < 0 && gold >= TotalCost())
                    levelsNeeded = record.Number;
            }

            Assert.Greater(levelsNeeded, 60, "the last stage should not come too early");
            Assert.LessOrEqual(levelsNeeded, pack.Count, "the last stage must be reachable within the shipped levels");
        }

        private static int TotalCost()
        {
            int total = 0;
            for (int stage = 1; stage < ShopStages.Count; stage++)
                total += ShopStages.CostOf(stage);
            return total;
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
