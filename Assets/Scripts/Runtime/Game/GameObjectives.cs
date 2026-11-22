using System;
using Fungiiiii.Champimaison;
using Fungiiiii.Inventory;

namespace Fungiiiii.Game
{
    /// <summary>
    /// Follows the game to tell the objective (<see cref="ObjectiveRules"/>): the seed in
    /// the inventory, the planted Champimaison and its stage, the net. Raised again only
    /// when the objective changes.
    /// </summary>
    internal sealed class GameObjectives : IDisposable
    {
        private readonly PlayerInventory inventory;
        private readonly MoleHoles moleHoles;
        private readonly ItemDefinition net;
        private FungalHole champimaison;
        private bool hasHadNet;

        public GameObjectives(PlayerInventory playerInventory, MoleHoles holes, ItemDefinition netItem)
        {
            inventory = playerInventory ?? throw new ArgumentNullException(nameof(playerInventory));
            moleHoles = holes != null ? holes : throw new ArgumentNullException(nameof(holes));
            net = netItem != null ? netItem : throw new ArgumentNullException(nameof(netItem));
            inventory.Changed += Evaluate;
            moleHoles.Planted += OnPlanted;
            Current = Work();
        }

        public event Action Changed;

        public Objective Current { get; private set; }

        public void Dispose()
        {
            inventory.Changed -= Evaluate;
            moleHoles.Planted -= OnPlanted;
            if (champimaison != null)
            {
                champimaison.Network.Changed -= Evaluate;
            }
        }

        private void OnPlanted(FungalHole planted)
        {
            champimaison = planted;
            champimaison.Network.Changed += Evaluate;
            Evaluate();
        }

        private void Evaluate()
        {
            var next = Work();
            if (next != Current)
            {
                Current = next;
                Changed?.Invoke();
            }
        }

        private Objective Work()
        {
            hasHadNet |= inventory.Count(net.Id) > 0;
            var stage = champimaison != null ? champimaison.Network.Stage : 0;
            var canEnter = champimaison != null && champimaison.CanBeEntered;
            return ObjectiveRules.For(new ObjectiveState(
                inventory.Count(moleHoles.Seed.Id) > 0,
                moleHoles.IsPlanted,
                stage,
                hasHadNet,
                canEnter));
        }
    }
}
