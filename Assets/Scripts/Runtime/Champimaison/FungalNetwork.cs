using System;
using System.Collections.Generic;
using Fungiiiii.Inventory;

namespace Fungiiiii.Champimaison
{
    /// <summary>Resources dropped into the Champimaison hole to grow the network one stage.</summary>
    public readonly struct NetworkStageCost
    {
        public NetworkStageCost(int wood, int spores)
        {
            if (wood < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(wood));
            }

            if (spores < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(spores));
            }

            Wood = wood;
            Spores = spores;
        }

        public int Wood { get; }
        public int Spores { get; }
    }

    /// <summary>
    /// The fungal network around the Champimaison hole (RG_CH_01, RG_CH_04): it grows one
    /// stage each time the player drops the next stage's wood and spores into the hole.
    /// A stage is paid in full or not at all.
    /// </summary>
    public sealed class FungalNetwork
    {
        private readonly NetworkStageCost[] stageCosts;
        private readonly string woodItemId;
        private readonly string sporeItemId;

        public FungalNetwork(IReadOnlyList<NetworkStageCost> stageCosts, string woodItemId, string sporeItemId)
        {
            if (stageCosts == null || stageCosts.Count == 0)
            {
                throw new ArgumentException("The network needs at least one stage.", nameof(stageCosts));
            }

            if (string.IsNullOrWhiteSpace(woodItemId))
            {
                throw new ArgumentException("A wood item id is required.", nameof(woodItemId));
            }

            if (string.IsNullOrWhiteSpace(sporeItemId))
            {
                throw new ArgumentException("A spore item id is required.", nameof(sporeItemId));
            }

            this.stageCosts = new NetworkStageCost[stageCosts.Count];
            for (var index = 0; index < stageCosts.Count; index++)
            {
                this.stageCosts[index] = stageCosts[index];
            }

            this.woodItemId = woodItemId;
            this.sporeItemId = sporeItemId;
        }

        /// <summary>Raised with the new stage each time the network grows.</summary>
        public event Action<int> Grew;

        /// <summary>Stages reached: 0 before anything was dropped in the hole.</summary>
        public int Stage { get; private set; }

        public int StageCount => stageCosts.Length;

        public bool IsComplete => Stage >= stageCosts.Length;

        /// <summary>Cost of the next stage. Meaningless once the network is complete.</summary>
        public NetworkStageCost NextCost => IsComplete ? default : stageCosts[Stage];

        /// <summary>
        /// Takes the next stage's wood and spores from <paramref name="inventory"/> and grows
        /// the network. Refused, taking nothing, if the inventory lacks either or the network
        /// is complete.
        /// </summary>
        public bool TryGrow(PlayerInventory inventory)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            if (IsComplete)
            {
                return false;
            }

            var cost = stageCosts[Stage];
            if (inventory.Count(woodItemId) < cost.Wood || inventory.Count(sporeItemId) < cost.Spores)
            {
                return false;
            }

            if (cost.Wood > 0)
            {
                inventory.Remove(woodItemId, cost.Wood);
            }

            if (cost.Spores > 0)
            {
                inventory.Remove(sporeItemId, cost.Spores);
            }

            Stage++;
            Grew?.Invoke(Stage);
            return true;
        }

        /// <summary>Back to an empty hole, for a reset.</summary>
        public void Reset()
        {
            Stage = 0;
        }
    }
}
