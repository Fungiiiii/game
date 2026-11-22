using Fungiiiii.Champimaison;
using Fungiiiii.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class FungalNetworkTests
    {
        private static readonly ItemDefinition Water = ItemDefinition.Create("water", Color.blue, 16);
        private static readonly ItemDefinition Spore = ItemDefinition.Create("spore", Color.magenta, 16);
        private static readonly ItemDefinition Wood = ItemDefinition.Create("wood", Color.yellow, 16);

        private static FungalNetwork NewNetwork()
        {
            return new FungalNetwork(new[]
            {
                new ChampimaisonTier(new[] { new ItemAmount(Water, 1), new ItemAmount(Spore, 3) }, TierUnlock.MyceliumThread),
                new ChampimaisonTier(new[] { new ItemAmount(Water, 5), new ItemAmount(Spore, 5), new ItemAmount(Wood, 5) }, TierUnlock.Entrance),
            });
        }

        private static PlayerInventory InventoryWith(int water, int spores, int wood)
        {
            var inventory = new PlayerInventory(15);
            if (water > 0) inventory.Add(Water, water);
            if (spores > 0) inventory.Add(Spore, spores);
            if (wood > 0) inventory.Add(Wood, wood);
            return inventory;
        }

        [Test]
        public void ANewNetwork_IsASeedThatHasNotWokenUp()
        {
            var network = NewNetwork();

            Assert.That(network.Stage, Is.Zero);
            Assert.That(network.IsComplete, Is.False);
            Assert.That(network.Progress.Required(0).Item, Is.SameAs(Water));
            Assert.That(network.Progress.Required(1).Amount, Is.EqualTo(3));
            Assert.That(network.HasUnlocked(TierUnlock.MyceliumThread), Is.False);
        }

        [Test]
        public void APartialDeposit_DoesNotGrowTheNetwork()
        {
            var network = NewNetwork();
            var grew = false;
            network.Grew += _ => grew = true;

            Assert.That(network.Deposit(InventoryWith(water: 0, spores: 2, wood: 0)), Is.EqualTo(2));

            Assert.That(network.Stage, Is.Zero);
            Assert.That(grew, Is.False);
            Assert.That(network.Progress.Deposited(1), Is.EqualTo(2));
        }

        [Test]
        public void TheLastDeposit_GrowsTheNetwork_AndStartsTheNextStageEmpty()
        {
            var network = NewNetwork();
            var grewTo = 0;
            network.Grew += stage => grewTo = stage;
            network.Deposit(InventoryWith(water: 0, spores: 3, wood: 0));

            var inventory = InventoryWith(water: 4, spores: 0, wood: 2);
            network.Deposit(inventory);

            Assert.That(grewTo, Is.EqualTo(1));
            Assert.That(network.Stage, Is.EqualTo(1));
            Assert.That(network.HasUnlocked(TierUnlock.MyceliumThread), Is.True);
            Assert.That(inventory.Count("water"), Is.EqualTo(3), "The rest of the water went into the next stage at once.");
            Assert.That(network.Progress.Deposited(0), Is.Zero);
            Assert.That(network.Progress.Missing(2), Is.EqualTo(5));
        }

        [Test]
        public void TheLastStage_OpensTheEntrance_AndCompletesTheNetwork()
        {
            var network = NewNetwork();
            network.Deposit(InventoryWith(water: 1, spores: 3, wood: 0));

            network.Deposit(InventoryWith(water: 5, spores: 5, wood: 5));

            Assert.That(network.IsComplete, Is.True);
            Assert.That(network.Progress, Is.Null);
            Assert.That(network.HasUnlocked(TierUnlock.Entrance), Is.True);
            Assert.That(network.Deposit(InventoryWith(water: 9, spores: 9, wood: 9)), Is.Zero);
        }

        [Test]
        public void Changed_IsRaisedOnEveryDepositThatTakesSomething()
        {
            var network = NewNetwork();
            var changes = 0;
            network.Changed += () => changes++;

            network.Deposit(InventoryWith(water: 0, spores: 0, wood: 3));
            Assert.That(changes, Is.Zero, "Wood is not wanted for the first stage.");

            network.Deposit(InventoryWith(water: 1, spores: 0, wood: 0));
            Assert.That(changes, Is.EqualTo(1));
        }
    }
}
