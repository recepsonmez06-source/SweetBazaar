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

                Assert.That(profile.CandyTypes, Is.InRange(2, 3), $"level {level}");
                Assert.AreEqual(3, profile.EmptyBoxes, $"level {level}");
            }
        }

        [Test]
        public void EveryProfile_IsValid()
        {
            for (int level = 1; level <= 300; level++)
                Assert.IsEmpty(LevelCurve.GetProfile(level).Validate(), $"level {level}");
        }

        [Test]
        public void NoLevel_UsesMoreCandyTypesThanTheArtBudget()
        {
            for (int level = 1; level <= 1000; level++)
                Assert.LessOrEqual(LevelCurve.GetProfile(level).CandyTypes, LevelCurve.MaxCandyTypes, $"level {level}");

            Assert.AreEqual(LevelCurve.MaxCandyTypes, LevelCurve.GetProfile(1000).CandyTypes);
        }

        [Test]
        public void HardLevel_IsFollowedByABreather()
        {
            for (int hard = 10; hard <= 200; hard += 5)
            {
                Assert.Greater(
                    LevelCurve.GetProfile(hard).CandyTypes,
                    LevelCurve.GetProfile(hard + 1).CandyTypes,
                    $"level {hard}");
            }
        }

        [Test]
        public void HardLevel_AsksForMoreMovesPerCandyTypeThanANormalOne()
        {
            var hard = LevelCurve.GetProfile(10);
            var normal = LevelCurve.GetProfile(12);

            Assert.Greater((double)hard.MinMoves / hard.CandyTypes, (double)normal.MinMoves / normal.CandyTypes);
        }

        [Test]
        public void Difficulty_GrowsOverTheCourseOfTheGame()
        {
            Assert.Less(LevelCurve.GetProfile(2).CandyTypes, LevelCurve.GetProfile(30).CandyTypes);
            Assert.Less(LevelCurve.GetProfile(30).CandyTypes, LevelCurve.GetProfile(120).CandyTypes);
        }

        [Test]
        public void LevelNumbersBelowOne_AreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => LevelCurve.GetProfile(0));
        }

        [Test]
        public void TheFirstLevels_CanAllBeGenerated()
        {
            for (int level = 1; level <= 30; level++)
            {
                var generated = LevelGenerator.GenerateForLevel(level);

                var profile = LevelCurve.GetProfile(level);
                Assert.That(generated.MinMoves, Is.InRange(profile.MinMoves, profile.MaxMoves), $"level {level}");
            }
        }
    }
}
