using System;

namespace SweetBazaar.Core
{
    // The candy shop the player grows with the gold earned from levels (docs/TASARIM.md section 6).
    // Stage 0 is the starting little counter; each upgrade builds the next stage.
    public static class ShopStages
    {
        // Gold needed to build each stage (index = stage). Stage 0 is free: it is where the shop starts.
        // Paced so the first upgrade comes after about 3 levels and the last one after about 100.
        private static readonly int[] Costs = { 0, 60, 150, 350, 800, 1800 };

        public static int Count => Costs.Length;

        public static int CostOf(int stage)
        {
            if (stage < 0 || stage >= Costs.Length)
                throw new ArgumentOutOfRangeException(nameof(stage));
            return Costs[stage];
        }
    }

    public sealed class Shop
    {
        public Shop(int gold = 0, int stage = 0)
        {
            Gold = Math.Max(0, gold);
            Stage = Math.Min(Math.Max(0, stage), ShopStages.Count - 1);
        }

        public int Gold { get; private set; }

        // Index of the stage that is built right now (0 .. ShopStages.Count - 1).
        public int Stage { get; private set; }

        public bool IsFullyBuilt => Stage >= ShopStages.Count - 1;

        // Gold the next stage costs (0 when everything is built).
        public int NextCost => IsFullyBuilt ? 0 : ShopStages.CostOf(Stage + 1);

        public bool CanUpgrade => !IsFullyBuilt && Gold >= NextCost;

        // Gold still missing for the next stage (0 if affordable or fully built).
        public int GoldMissing => IsFullyBuilt ? 0 : Math.Max(0, NextCost - Gold);

        public void AddGold(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            Gold += amount;
        }

        // Builds the next stage and pays for it; false (and nothing changes) if it is not affordable.
        public bool TryUpgrade()
        {
            if (!CanUpgrade)
                return false;

            Gold -= NextCost;
            Stage++;
            return true;
        }
    }

    public static class ShopRules
    {
        public const int BaseGold = 10;
        public const int GoldPerCandyType = 2;
        public const int CleanSolveBonus = 5;

        // Gold for finishing a level: more for harder levels (more candy types) and a bonus for a clean solve,
        // i.e. without using undo or the extra box.
        public static int GoldForLevel(int candyTypes, bool withoutHelp)
        {
            if (candyTypes < 0)
                throw new ArgumentOutOfRangeException(nameof(candyTypes));
            return BaseGold + GoldPerCandyType * candyTypes + (withoutHelp ? CleanSolveBonus : 0);
        }
    }
}
