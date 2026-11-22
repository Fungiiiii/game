using Fungiiiii.Crafting;
using Fungiiiii.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class CrafterTests
    {
        private static readonly ItemDefinition Wood = ItemDefinition.Create("wood", Color.yellow, 16);
        private static readonly ItemDefinition Pebble = ItemDefinition.Create("pebble", Color.gray, 16);
        private static readonly ItemDefinition Slingshot = ItemDefinition.Create("slingshot", Color.red, 1);

        private static CraftingRecipe SlingshotRecipe()
        {
            return CraftingRecipe.Create(Slingshot, new ItemAmount(Wood, 2), new ItemAmount(Pebble, 1));
        }

        private static PlayerInventory StandardInventory()
        {
            var layout = InventoryLayout.Standard;
            return new PlayerInventory(layout.SlotCount, layout.FillOrder());
        }

        [Test]
        public void ACraft_TakesEveryIngredient_AndGivesTheResult()
        {
            var inventory = StandardInventory();
            inventory.Add(Wood, 3);
            inventory.Add(Pebble, 4);

            Assert.That(Crafter.TryCraft(inventory, SlingshotRecipe()), Is.True);

            Assert.That(inventory.Count("wood"), Is.EqualTo(1));
            Assert.That(inventory.Count("pebble"), Is.EqualTo(3));
            Assert.That(inventory.Count("slingshot"), Is.EqualTo(1));
        }

        [Test]
        public void AMissingIngredient_TakesNothing()
        {
            var inventory = StandardInventory();
            inventory.Add(Wood, 1);
            inventory.Add(Pebble, 4);

            Assert.That(Crafter.CanCraft(inventory, SlingshotRecipe()), Is.False);
            Assert.That(Crafter.TryCraft(inventory, SlingshotRecipe()), Is.False);

            Assert.That(inventory.Count("wood"), Is.EqualTo(1));
            Assert.That(inventory.Count("pebble"), Is.EqualTo(4));
        }

        [Test]
        public void AResultThatCannotFit_GivesTheIngredientsBack()
        {
            // Two slots, both full of what the recipe does not take entirely.
            var inventory = new PlayerInventory(2);
            inventory.Add(Wood, 16);
            inventory.Add(Pebble, 16);

            Assert.That(Crafter.TryCraft(inventory, SlingshotRecipe()), Is.False);

            Assert.That(inventory.Count("wood"), Is.EqualTo(16));
            Assert.That(inventory.Count("pebble"), Is.EqualTo(16));
            Assert.That(inventory.Count("slingshot"), Is.Zero);
        }

        [Test]
        public void TheResult_GoesToTheQuickBarFirst_AndIsShownAsCollected()
        {
            var inventory = StandardInventory();
            inventory.AddToSlot(0, Wood, 2);
            inventory.AddToSlot(1, Pebble, 1);
            ItemDefinition collected = null;
            inventory.Collected += (item, _) => collected = item;

            Crafter.TryCraft(inventory, SlingshotRecipe());

            var firstQuickBarSlot = InventoryLayout.Standard.QuickBarFirstSlot;
            Assert.That(inventory.GetSlot(firstQuickBarSlot).ItemId, Is.EqualTo("slingshot"));
            Assert.That(collected, Is.SameAs(Slingshot));
        }
    }
}
