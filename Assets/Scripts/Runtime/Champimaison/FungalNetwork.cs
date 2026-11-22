using System;
using System.Collections.Generic;
using Fungiiiii.Inventory;

namespace Fungiiiii.Champimaison
{
    /// <summary>
    /// The fungal network of a Champimaison (RG_CH_01, RG_CH_04): its stages are paid by
    /// dropping items into it, a little at a time if need be. What was dropped is kept; the
    /// last item of a stage's cost grows the network to that stage.
    /// </summary>
    public sealed class FungalNetwork
    {
        private readonly ChampimaisonTier[] tiers;

        public FungalNetwork(IReadOnlyList<ChampimaisonTier> tiers)
        {
            if (tiers == null || tiers.Count == 0)
            {
                throw new ArgumentException("The network needs at least one stage.", nameof(tiers));
            }

            this.tiers = new ChampimaisonTier[tiers.Count];
            for (var index = 0; index < tiers.Count; index++)
            {
                this.tiers[index] = tiers[index];
            }

            Progress = new RequirementProgress(this.tiers[0].Cost);
        }

        /// <summary>Raised with the new stage each time the network grows.</summary>
        public event Action<int> Grew;

        /// <summary>Raised after every deposit that took something, and after a growth.</summary>
        public event Action Changed;

        /// <summary>Stages reached: 0 for a seed that has not woken up yet.</summary>
        public int Stage { get; private set; }

        public int StageCount => tiers.Length;

        public bool IsComplete => Stage >= tiers.Length;

        /// <summary>What has been dropped towards the next stage; null once the network is complete.</summary>
        public RequirementProgress Progress { get; private set; }

        /// <summary>Whether a stage reached so far gives <paramref name="unlock"/>.</summary>
        public bool HasUnlocked(TierUnlock unlock)
        {
            for (var index = 0; index < Stage; index++)
            {
                if (tiers[index].Unlock == unlock)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Drops into the network what the next stage still lacks and the inventory holds,
        /// and grows the network if that completes the stage.
        /// </summary>
        /// <returns>How many items were taken.</returns>
        public int Deposit(PlayerInventory inventory)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            if (IsComplete)
            {
                return 0;
            }

            var taken = Progress.Deposit(inventory);
            if (Progress.IsComplete)
            {
                Stage++;
                Progress = IsComplete ? null : new RequirementProgress(tiers[Stage].Cost);
                Grew?.Invoke(Stage);
                Changed?.Invoke();
            }
            else if (taken > 0)
            {
                Changed?.Invoke();
            }

            return taken;
        }
    }
}
