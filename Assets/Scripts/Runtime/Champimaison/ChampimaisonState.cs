using System;

namespace Fungiiiii.Champimaison
{
    /// <summary>
    /// The two buildable tiers of a Champimaison prototype plot.
    /// </summary>
    public enum ChampimaisonTier
    {
        Empty = -1,
        Tier0 = 0,
        Tier1 = 1
    }

    /// <summary>
    /// A small value object used by the prototype's fictional economy.
    /// </summary>
    public readonly struct ChampimaisonResourceCost
    {
        public ChampimaisonResourceCost(int spores, int dew)
        {
            Spores = spores;
            Dew = dew;
        }

        public int Spores { get; }

        public int Dew { get; }
    }

    /// <summary>
    /// Deterministic state machine for the Champimaison planting and upgrade flow.
    /// It intentionally has no Unity dependency so the rules can be tested in EditMode.
    /// </summary>
    public sealed class ChampimaisonState
    {
        public static readonly ChampimaisonResourceCost PlantSeedCost = new ChampimaisonResourceCost(1, 0);
        public static readonly ChampimaisonResourceCost Tier1Cost = new ChampimaisonResourceCost(2, 1);

        public ChampimaisonState(int spores = 3, int dew = 2)
        {
            StartingSpores = spores;
            StartingDew = dew;
            Reset();
        }

        public event Action Changed;

        public int StartingSpores { get; }

        public int StartingDew { get; }

        public int Spores { get; private set; }

        public int Dew { get; private set; }

        public ChampimaisonTier Tier { get; private set; }

        public bool IsPlanted => Tier != ChampimaisonTier.Empty;

        public bool CanPlantSeed => Tier == ChampimaisonTier.Empty && CanAfford(PlantSeedCost);

        public bool CanUpgradeToTier1 => Tier == ChampimaisonTier.Tier0 && CanAfford(Tier1Cost);

        public bool TryPlantSeed()
        {
            if (!CanPlantSeed)
            {
                return false;
            }

            Pay(PlantSeedCost);
            Tier = ChampimaisonTier.Tier0;
            Changed?.Invoke();
            return true;
        }

        public bool TryUpgradeToTier1()
        {
            if (!CanUpgradeToTier1)
            {
                return false;
            }

            Pay(Tier1Cost);
            Tier = ChampimaisonTier.Tier1;
            Changed?.Invoke();
            return true;
        }

        public void Reset()
        {
            Spores = StartingSpores;
            Dew = StartingDew;
            Tier = ChampimaisonTier.Empty;
            Changed?.Invoke();
        }

        private bool CanAfford(ChampimaisonResourceCost cost)
        {
            return Spores >= cost.Spores && Dew >= cost.Dew;
        }

        private void Pay(ChampimaisonResourceCost cost)
        {
            Spores -= cost.Spores;
            Dew -= cost.Dew;
        }
    }
}
