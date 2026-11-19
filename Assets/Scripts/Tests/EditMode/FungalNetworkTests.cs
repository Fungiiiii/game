using Fungiiiii.Champimaison;
using Fungiiiii.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class FungalNetworkTests
    {
        private const string WoodId = "wood";
        private const string SporeId = "spore";

        private static readonly InventoryItemDefinition Wood = new InventoryItemDefinition(WoodId, Color.yellow, 20);
        private static readonly InventoryItemDefinition Spore = new InventoryItemDefinition(SporeId, Color.cyan, 20);

        private static FungalNetwork NewNetwork()
        {
            return new FungalNetwork(
                new[] { new NetworkStageCost(wood: 2, spores: 2), new NetworkStageCost(wood: 4, spores: 4) },
                WoodId,
                SporeId);
        }

        private static PlayerInventory InventoryWith(int wood, int spores)
        {
            var inventory = new PlayerInventory(15);
            if (wood > 0)
            {
                inventory.Add(Wood, wood);
            }

            if (spores > 0)
            {
                inventory.Add(Spore, spores);
            }

            return inventory;
        }

        [Test]
        public void ANewNetwork_IsAnEmptyHole()
        {
            var network = NewNetwork();

            Assert.That(network.Stage, Is.EqualTo(0));
            Assert.That(network.IsComplete, Is.False);
            Assert.That(network.NextCost.Wood, Is.EqualTo(2));
            Assert.That(network.NextCost.Spores, Is.EqualTo(2));
        }

        [Test]
        public void StageOne_TakesTwoWoodAndTwoSpores()
        {
            var network = NewNetwork();
            var inventory = InventoryWith(wood: 3, spores: 5);
            var grewTo = 0;
            network.Grew += stage => grewTo = stage;

            Assert.That(network.TryGrow(inventory), Is.True);
            Assert.That(network.Stage, Is.EqualTo(1));
            Assert.That(grewTo, Is.EqualTo(1));
            Assert.That(inventory.Count(WoodId), Is.EqualTo(1));
            Assert.That(inventory.Count(SporeId), Is.EqualTo(3));
        }

        [Test]
        public void WithoutEnoughOfEither_ItIsRefused_AndTakesNothing()
        {
            var network = NewNetwork();
            var inventory = InventoryWith(wood: 5, spores: 1);

            Assert.That(network.TryGrow(inventory), Is.False);
            Assert.That(network.Stage, Is.EqualTo(0));
            Assert.That(inventory.Count(WoodId), Is.EqualTo(5));
            Assert.That(inventory.Count(SporeId), Is.EqualTo(1));
        }

        [Test]
        public void WithAnEmptyInventory_ItIsRefused()
        {
            var network = NewNetwork();

            Assert.That(network.TryGrow(new PlayerInventory(15)), Is.False);
            Assert.That(network.Stage, Is.EqualTo(0));
        }

        [Test]
        public void StageTwo_TakesFourWoodAndFourSpores_OnTopOfStageOne()
        {
            var network = NewNetwork();
            var inventory = InventoryWith(wood: 6, spores: 6);

            Assert.That(network.TryGrow(inventory), Is.True);
            Assert.That(network.TryGrow(inventory), Is.True);

            Assert.That(network.Stage, Is.EqualTo(2));
            Assert.That(network.IsComplete, Is.True);
            Assert.That(inventory.Count(WoodId), Is.EqualTo(0));
            Assert.That(inventory.Count(SporeId), Is.EqualTo(0));
        }

        [Test]
        public void StageTwo_WithOnlyStageOneResourcesLeft_IsRefused()
        {
            var network = NewNetwork();
            var inventory = InventoryWith(wood: 4, spores: 4);
            network.TryGrow(inventory);

            Assert.That(network.TryGrow(inventory), Is.False);
            Assert.That(network.Stage, Is.EqualTo(1));
            Assert.That(inventory.Count(WoodId), Is.EqualTo(2));
            Assert.That(inventory.Count(SporeId), Is.EqualTo(2));
        }

        [Test]
        public void ACompleteNetwork_TakesNothingMore()
        {
            var network = NewNetwork();
            var inventory = InventoryWith(wood: 10, spores: 10);
            network.TryGrow(inventory);
            network.TryGrow(inventory);

            Assert.That(network.TryGrow(inventory), Is.False);
            Assert.That(inventory.Count(WoodId), Is.EqualTo(4));
            Assert.That(inventory.Count(SporeId), Is.EqualTo(4));
        }

        [Test]
        public void Reset_EmptiesTheHole()
        {
            var network = NewNetwork();
            network.TryGrow(InventoryWith(wood: 2, spores: 2));

            network.Reset();

            Assert.That(network.Stage, Is.EqualTo(0));
        }
    }
}
