using Fungiiiii.UI;

namespace Fungiiiii.Game
{
    /// <summary>The steps of the playtest's loop, in order.</summary>
    public enum Objective
    {
        FindSeed,
        PlantSeed,
        AwakenSeed,
        CraftNet,
        GrowChampimaison,
        EnterChampimaison,
    }

    /// <summary>Where the player stands in the loop, as the objective reads it.</summary>
    public readonly struct ObjectiveState
    {
        public ObjectiveState(bool hasSeed, bool seedPlanted, int champimaisonStage, bool hasHadNet, bool canEnter)
        {
            HasSeed = hasSeed;
            SeedPlanted = seedPlanted;
            ChampimaisonStage = champimaisonStage;
            HasHadNet = hasHadNet;
            CanEnter = canEnter;
        }

        public bool HasSeed { get; }

        public bool SeedPlanted { get; }

        public int ChampimaisonStage { get; }

        /// <summary>The net was in the inventory at least once: losing it later does not ask for it again.</summary>
        public bool HasHadNet { get; }

        public bool CanEnter { get; }
    }

    /// <summary>The objective shown top right, worked out from the state of the game rather than from the order of events.</summary>
    public static class ObjectiveRules
    {
        public static Objective For(ObjectiveState state)
        {
            if (!state.SeedPlanted)
            {
                return state.HasSeed ? Objective.PlantSeed : Objective.FindSeed;
            }

            if (state.CanEnter)
            {
                return Objective.EnterChampimaison;
            }

            if (state.ChampimaisonStage == 0)
            {
                return Objective.AwakenSeed;
            }

            return state.HasHadNet ? Objective.GrowChampimaison : Objective.CraftNet;
        }

        /// <summary>The objective's text, a key of the UI string table.</summary>
        public static string KeyFor(Objective objective)
        {
            switch (objective)
            {
                case Objective.FindSeed: return UiText.ObjectiveFindSeed;
                case Objective.PlantSeed: return UiText.ObjectivePlantSeed;
                case Objective.AwakenSeed: return UiText.ObjectiveAwakenSeed;
                case Objective.CraftNet: return UiText.ObjectiveCraftNet;
                case Objective.GrowChampimaison: return UiText.ObjectiveGrow;
                default: return UiText.ObjectiveEnter;
            }
        }
    }
}
