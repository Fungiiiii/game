using System;
using Fungiiiii.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class PlayerInventoryTests
    {
        private static InventoryItemDefinition CreateItem(int maxStackSize = 5)
        {
            return new InventoryItemDefinition("test_item", Color.magenta, maxStackSize);
        }

        [Test]
        public void Add_FillsExistingStackBeforeOpeningAnotherSlot()
        {
            var inventory = new PlayerInventory(3);
            InventoryItemDefinition item = CreateItem();

            Assert.That(inventory.Add(item, 7), Is.EqualTo(7));
            Assert.That(inventory.GetSlot(0).Quantity, Is.EqualTo(5));
            Assert.That(inventory.GetSlot(1).Quantity, Is.EqualTo(2));
            Assert.That(inventory.GetSlot(2).IsEmpty, Is.True);
        }

        [Test]
        public void Add_WhenFull_ReturnsOnlyQuantityThatFits()
        {
            var inventory = new PlayerInventory(1);
            InventoryItemDefinition item = CreateItem(2);

            Assert.That(inventory.Add(item, 5), Is.EqualTo(2));
            Assert.That(inventory.Count(item.Id), Is.EqualTo(2));
        }

        [Test]
        public void Remove_ReleasesLastStackFirst()
        {
            var inventory = new PlayerInventory(3);
            InventoryItemDefinition item = CreateItem();
            inventory.Add(item, 7);

            Assert.That(inventory.Remove(item.Id, 2), Is.EqualTo(2));
            Assert.That(inventory.GetSlot(0).Quantity, Is.EqualTo(5));
            Assert.That(inventory.GetSlot(1).IsEmpty, Is.True);
        }

        [Test]
        public void MoveOrSwap_MovesToEmptySlotAndSwapsOccupiedSlots()
        {
            var inventory = new PlayerInventory(3);
            InventoryItemDefinition item = CreateItem();
            InventoryItemDefinition other = new InventoryItemDefinition("other_item", Color.cyan, 5);
            inventory.Add(item, 2);
            inventory.Add(other, 1);

            Assert.That(inventory.MoveOrSwap(0, 2), Is.True);
            Assert.That(inventory.GetSlot(0).IsEmpty, Is.True);
            Assert.That(inventory.GetSlot(2).ItemId, Is.EqualTo(item.Id));

            Assert.That(inventory.MoveOrSwap(1, 2), Is.True);
            Assert.That(inventory.GetSlot(1).ItemId, Is.EqualTo(item.Id));
            Assert.That(inventory.GetSlot(2).ItemId, Is.EqualTo(other.Id));
        }

        [Test]
        public void TakeAndAddToSlot_SupportSplitStackInteractions()
        {
            var inventory = new PlayerInventory(3);
            InventoryItemDefinition item = CreateItem();
            inventory.Add(item, 5);

            int taken = inventory.TakeFromSlot(0, 3, out InventoryItemDefinition carriedItem);
            int placed = inventory.AddToSlot(1, carriedItem, 1);

            Assert.That(taken, Is.EqualTo(3));
            Assert.That(placed, Is.EqualTo(1));
            Assert.That(inventory.GetSlot(0).Quantity, Is.EqualTo(2));
            Assert.That(inventory.GetSlot(1).Quantity, Is.EqualTo(1));
            Assert.That(inventory.Count(item.Id), Is.EqualTo(3));
        }

        [Test]
        public void StackAll_ConsolidatesCompatibleStacksIntoThePreferredSlot()
        {
            var inventory = new PlayerInventory(3);
            InventoryItemDefinition item = CreateItem();
            inventory.Add(item, 5);
            inventory.TakeFromSlot(0, 2, out InventoryItemDefinition carriedItem);
            inventory.AddToSlot(1, carriedItem, 2);

            Assert.That(inventory.StackAll(item.Id, 1), Is.True);
            Assert.That(inventory.GetSlot(0).IsEmpty, Is.True);
            Assert.That(inventory.GetSlot(1).Quantity, Is.EqualTo(5));
        }

        [Test]
        public void Changed_IsRaisedOncePerSuccessfulOperation()
        {
            var inventory = new PlayerInventory(2);
            InventoryItemDefinition item = CreateItem();
            int changes = 0;
            inventory.Changed += () => changes++;

            inventory.Add(item, 2);
            inventory.Remove(item.Id, 1);

            Assert.That(changes, Is.EqualTo(2));
        }

        [Test]
        public void InvalidArguments_AreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerInventory(0));
            Assert.Throws<System.ArgumentException>(() => new InventoryItemDefinition(string.Empty, Color.white, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateItem(0));
        }
    }
}
