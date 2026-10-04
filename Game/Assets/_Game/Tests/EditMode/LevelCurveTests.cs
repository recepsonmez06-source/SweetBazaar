using System;
using NUnit.Framework;

namespace SweetBazaar.Core.Tests
{
    public class LevelCurveTests
    {
        [Test]
        public void FirstLevels_AreTheTutorial()
        {
            for (int level = 1; level <= 5; level++)
            {
                var profile = LevelCurve.GetProfile(level);

                Assert.That(profile.CandyTypes, Is.InRange(2, 4), $"level {level}");
                Assert.AreEqual(3, profile.EmptyBoxes, $"level {level}");
                Assert.AreEqual(100, profile.TargetWinRate, $"level {level}");
            }
        }

        [Test]
        public void EveryProfile_IsValid()
        {
            for (int level = 1; level <= 600; level++)
                Assert.IsEmpty(LevelCurve.GetProfile(level).Validate(), $"level {level}");
        }

        [Test]
        public void TheTargetNeverGoesUp()
        {
            // The core promise: no level is meant to be easier than the one before it.
            for (int level = 1; level < 2000; level++)
            {
                Assert.LessOrEqual(LevelCurve.TargetWinRate(level + 1), LevelCurve.TargetWinRate(level) + 1e-9,
                    $"level {level} -> {level + 1}");
            }
        }

        [Test]
        public void TheTargetFallsFromVeryEasyToVeryHard()
        {
            Assert.AreEqual(100, LevelCurve.TargetWinRate(1), 1e-9);
            Assert.That(LevelCurve.TargetWinRate(60), Is.InRange(30.0, 50.0));
            Assert.LessOrEqual(LevelCurve.TargetWinRate(10), 90.0, "the game must clearly get harder soon after the 5-level tutorial");
            Assert.LessOrEqual(LevelCurve.TargetWinRate(200), 8.0);
            Assert.Greater(LevelCurve.TargetWinRate(2000), 0.0);
        }

        [Test]
        public void TheSettingsNeverGetEasier()
        {
            // Empty boxes only go down and, with the same number of empty boxes, candy types only go up.
            for (int level = 1; level < 1000; level++)
            {
                var now = LevelCurve.GetProfile(level);
                var next = LevelCurve.GetProfile(level + 1);

                Assert.LessOrEqual(next.EmptyBoxes, now.EmptyBoxes, $"level {level} -> {level + 1}");
                if (next.EmptyBoxes == now.EmptyBoxes)
                    Assert.GreaterOrEqual(next.CandyTypes, now.CandyTypes, $"level {level} -> {level + 1}");
            }
        }

        [Test]
        public void NoLevel_UsesMoreCandyTypesThanTheArtBudget()
        {
            for (int level = 1; level <= 1000; level++)
                Assert.LessOrEqual(LevelCurve.GetProfile(level).CandyTypes, LevelCurve.MaxCandyTypes, $"level {level}");
        }

        [Test]
        public void LateLevelsHaveASingleEmptyBox()
        {
            Assert.AreEqual(3, LevelCurve.GetProfile(1).EmptyBoxes);
            Assert.AreEqual(2, LevelCurve.GetProfile(30).EmptyBoxes);
            Assert.AreEqual(1, LevelCurve.GetProfile(200).EmptyBoxes);
        }

        [Test]
        public void LevelNumbersBelowOne_AreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => LevelCurve.GetProfile(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => LevelCurve.TargetWinRate(0));
        }

        [Test]
        public void GeneratedLevels_LandNearTheirTarget()
        {
            foreach (int level in new[] { 1, 5, 15, 25 })
            {
                var profile = LevelCurve.GetProfile(level);

                var generated = LevelGenerator.GenerateForLevel(level);

                Assert.That(generated.WinRate, Is.InRange(
                    profile.TargetWinRate - profile.WinRateTolerance, profile.TargetWinRate + profile.WinRateTolerance), $"level {level}");
                Assert.AreEqual(profile.CandyTypes + profile.EmptyBoxes, generated.Level.Boxes.Count, $"level {level}");
            }
        }
    }
}
