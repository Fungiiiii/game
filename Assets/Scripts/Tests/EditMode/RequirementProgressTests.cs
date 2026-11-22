using System;
using Fungiiiii.Champimaison;
using Fungiiiii.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class RequirementProgressTests
    {
        private static readonly ItemDefinition Water = ItemDefinition.Create("water", Color.blue, 16);
        private static readonly ItemDefinition Spore = ItemDefinition.Create("spore", Color.magenta, 16);
        private static readonly ItemDefinition Wood = ItemDefinition.Create("wood", Color.yellow, 16);

        private static RequirementProgress WaterAndSpores()
        {
            return new RequirementProgress(new[] { new ItemAmount(Water, 1), new ItemAmount(Spore, 3) });
        }

        private static PlayerInventory InventoryWith(int water, int spores, int wood = 0)
        {
            var inventory = new PlayerInventory(15);
            if (water > 0) inventory.Add(Water, water);
            if (spores > 0) inventory.Add(Spore, spores);
            if (wood > 0) inventory.Add(Wood, wood);
            return inventory;
        }

        [Test]
        public void ANewProgress_HasNothingDeposited()
        {
            var progress = WaterAndSpores();

            Assert.That(progress.LineCount, Is.EqualTo(2));
            Assert.That(progress.Deposited(0), Is.Zero);
            Assert.That(progress.Missing(1), Is.EqualTo(3));
            Assert.That(progress.IsComplete, Is.False);
        }

        [Test]
        public void Deposit_NeverTakesMoreThanWhatIsMissing()
        {
            var progress = WaterAndSpores();
            var inventory = InventoryWith(water: 4, spores: 10);

            Assert.That(progress.Deposit(inventory), Is.EqualTo(4));

            Assert.That(inventory.Count("water"), Is.EqualTo(3));
            Assert.That(inventory.Count("spore"), Is.EqualTo(7));
            Assert.That(progress.IsComplete, Is.True);
        }

        [Test]
        public void APartialDeposit_IsKept_AndTheNextOneCompletesIt()
        {
            var progress = WaterAndSpores();

            progress.Deposit(InventoryWith(water: 0, spores: 2));
            Assert.That(progress.Deposited(1), Is.EqualTo(2));
            Assert.That(progress.IsComplete, Is.False);

            progress.Deposit(InventoryWith(water: 1, spores: 5));
            Assert.That(progress.Deposited(0), Is.EqualTo(1));
            Assert.That(progress.Deposited(1), Is.EqualTo(3));
            Assert.That(progress.IsComplete, Is.True);
        }

        [Test]
        public void ACompleteLine_TakesNothingMore()
        {
            var progress = WaterAndSpores();
            progress.Deposit(InventoryWith(water: 1, spores: 0));
            var inventory = InventoryWith(water: 2, spores: 0);

            Assert.That(progress.Deposit(inventory), Is.Zero);
            Assert.That(inventory.Count("water"), Is.EqualTo(2));
        }

        [Test]
        public void WhatIsNotRequired_IsLeftInTheInventory()
        {
            var progress = WaterAndSpores();
            var inventory = InventoryWith(water: 0, spores: 0, wood: 5);

            Assert.That(progress.CanTakeFrom(inventory), Is.False);
            Assert.That(progress.Deposit(inventory), Is.Zero);
            Assert.That(inventory.Count("wood"), Is.EqualTo(5));
        }

        [Test]
        public void Changed_IsRaisedOnlyWhenSomethingIsTaken()
        {
            var progress = WaterAndSpores();
            var changes = 0;
            progress.Changed += () => changes++;

            progress.Deposit(InventoryWith(water: 0, spores: 0));
            Assert.That(changes, Is.Zero);

            progress.Deposit(InventoryWith(water: 0, spores: 1));
            Assert.That(changes, Is.EqualTo(1));
        }

        [Test]
        public void WouldComplete_TellsWhetherTheInventoryCoversWhatIsMissing()
        {
            var progress = WaterAndSpores();
            progress.Deposit(InventoryWith(water: 0, spores: 2));

            Assert.That(progress.WouldComplete(InventoryWith(water: 1, spores: 1)), Is.True);
            Assert.That(progress.WouldComplete(InventoryWith(water: 0, spores: 1)), Is.False);
            Assert.That(progress.CanTakeFrom(InventoryWith(water: 0, spores: 1)), Is.True);
        }

        [Test]
        public void AnItemRequiredTwice_IsRefused()
        {
            Assert.Throws<ArgumentException>(() => new RequirementProgress(new[] { new ItemAmount(Water, 1), new ItemAmount(Water, 2) }));
            Assert.Throws<ArgumentException>(() => new RequirementProgress(new ItemAmount[0]));
        }
    }
}
