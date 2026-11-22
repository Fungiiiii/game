namespace Fungiiiii.Champimaison
{
    /// <summary>What a mole hole shows over it when aimed at.</summary>
    public enum PlantingPrompt
    {
        /// <summary>The seed is planted elsewhere: the hole does nothing any more.</summary>
        None,

        /// <summary>No seed: "Needs a seed".</summary>
        NeedsSeed,

        /// <summary>The seed is in the backpack, not in hand: "Hold the seed in hand".</summary>
        HoldSeed,

        /// <summary>The seed is in hand: the interact key, "Plant".</summary>
        Plant,
    }

    /// <summary>A tool is used from the hand: the seed is planted only when it is the item held.</summary>
    public static class PlantingRules
    {
        public static PlantingPrompt PromptFor(bool seedPlanted, bool seedInHand, bool seedInInventory)
        {
            if (seedPlanted)
            {
                return PlantingPrompt.None;
            }

            if (seedInHand)
            {
                return PlantingPrompt.Plant;
            }

            return seedInInventory ? PlantingPrompt.HoldSeed : PlantingPrompt.NeedsSeed;
        }
    }
}
