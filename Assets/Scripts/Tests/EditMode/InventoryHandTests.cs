using Fungiiiii.Inventory;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class InventoryHandTests
    {
        private static readonly ItemDefinition Wood = ItemDefinition.Create("wood", Color.red, 16);
        private static readonly ItemDefinition Spore = ItemDefinition.Create("spore", Color.blue, 16);

        private PlayerInventory inventory;
        private InventoryHand hand;

        [SetUp]
        public void SetUp()
        {
            inventory = new PlayerInventory(15);
            hand = new InventoryHand(inventory);
        }

        private int QuantityIn(int slot) => inventory.GetSlot(slot).Quantity;

        [Test]
        public void ALeftPress_TakesTheWholeStack_AtOnce()
        {
            inventory.AddToSlot(3, Wood, 9);

            hand.PressLeft(3, 0f);

            Assert.That(hand.Quantity, Is.EqualTo(9));
            Assert.That(hand.Item, Is.EqualTo(Wood));
            Assert.That(inventory.GetSlot(3).IsEmpty, Is.True);
        }

        [Test]
        public void ALeftPress_OnAnEmptySlot_PutsEverythingDown()
        {
            inventory.AddToSlot(0, Wood, 9);
            hand.PressLeft(0, 0f);

            hand.PressLeft(5, 1f);

            Assert.That(hand.IsHolding, Is.False);
            Assert.That(QuantityIn(5), Is.EqualTo(9));
        }

        [Test]
        public void ALeftPress_OnTheSameItem_TopsItUp_AndKeepsTheRestInHand()
        {
            inventory.AddToSlot(0, Wood, 10);
            inventory.AddToSlot(1, Wood, 12);
            hand.PressLeft(0, 0f);

            hand.PressLeft(1, 1f);

            Assert.That(QuantityIn(1), Is.EqualTo(16));
            Assert.That(hand.Quantity, Is.EqualTo(6));
        }

        [Test]
        public void ALeftPress_OnAnotherItem_Swaps()
        {
            inventory.AddToSlot(0, Wood, 4);
            inventory.AddToSlot(1, Spore, 7);
            hand.PressLeft(0, 0f);

            hand.PressLeft(1, 1f);

            Assert.That(inventory.GetSlot(1).ItemId, Is.EqualTo("wood"));
            Assert.That(QuantityIn(1), Is.EqualTo(4));
            Assert.That(hand.Item, Is.EqualTo(Spore));
            Assert.That(hand.Quantity, Is.EqualTo(7));
        }

        [Test]
        public void ADoublePress_GathersEveryStackOntoTheMouse_IncompleteOnesFirst()
        {
            inventory.AddToSlot(0, Wood, 16);
            inventory.AddToSlot(2, Wood, 3);
            inventory.AddToSlot(4, Wood, 5);
            inventory.AddToSlot(6, Spore, 5);
            inventory.AddToSlot(8, Wood, 4);

            hand.PressLeft(4, 10f);
            hand.PressLeft(4, 10.2f);

            // 5 in hand, then the 3 and the 4 of the incomplete stacks, then 4 from the full one.
            Assert.That(hand.Quantity, Is.EqualTo(16));
            Assert.That(QuantityIn(2), Is.Zero);
            Assert.That(QuantityIn(8), Is.Zero);
            Assert.That(QuantityIn(0), Is.EqualTo(12));
            Assert.That(QuantityIn(6), Is.EqualTo(5), "Another item was gathered.");
        }

        [Test]
        public void ASecondPressAfterTheWindow_PutsTheItemBack()
        {
            inventory.AddToSlot(4, Wood, 5);
            inventory.AddToSlot(8, Wood, 4);

            hand.PressLeft(4, 10f);
            hand.PressLeft(4, 10.3f);

            Assert.That(hand.IsHolding, Is.False);
            Assert.That(QuantityIn(4), Is.EqualTo(5));
            Assert.That(QuantityIn(8), Is.EqualTo(4));
        }

        [Test]
        public void ADoublePressOnAnotherSlot_IsTwoPresses()
        {
            inventory.AddToSlot(4, Wood, 5);
            inventory.AddToSlot(8, Wood, 4);

            hand.PressLeft(4, 10f);
            hand.PressLeft(5, 10.1f);

            Assert.That(hand.IsHolding, Is.False);
            Assert.That(QuantityIn(5), Is.EqualTo(5));
        }

        [Test]
        public void AThirdQuickPress_IsNotASecondDoubleClick()
        {
            inventory.AddToSlot(4, Wood, 5);
            inventory.AddToSlot(8, Wood, 4);

            hand.PressLeft(4, 10f);
            hand.PressLeft(4, 10.1f);
            hand.PressLeft(4, 10.2f);

            Assert.That(hand.IsHolding, Is.False, "The third press did not put the gathered stack down.");
            Assert.That(QuantityIn(4), Is.EqualTo(9));
        }

        [Test]
        public void GatherAll_WithEmptyHands_TakesTheSlot_ThenEveryOtherStackOfIt()
        {
            inventory.AddToSlot(0, Wood, 3);
            inventory.AddToSlot(4, Wood, 5);
            inventory.AddToSlot(6, Spore, 2);

            hand.GatherAll(4);

            Assert.That(hand.Item, Is.EqualTo(Wood));
            Assert.That(hand.Quantity, Is.EqualTo(8));
            Assert.That(inventory.GetSlot(0).IsEmpty, Is.True);
            Assert.That(inventory.GetSlot(4).IsEmpty, Is.True);
            Assert.That(QuantityIn(6), Is.EqualTo(2), "Another item was gathered.");
        }

        [Test]
        public void GatherAll_WithSomethingInHand_GathersMoreOfIt_UpToAFullStack()
        {
            inventory.AddToSlot(0, Wood, 10);
            inventory.AddToSlot(1, Wood, 10);
            hand.PressLeft(0, 0f);

            hand.GatherAll(5);

            Assert.That(hand.Quantity, Is.EqualTo(16));
            Assert.That(QuantityIn(1), Is.EqualTo(4));
        }

        [Test]
        public void GatherAll_OnAnEmptySlot_WithEmptyHands_DoesNothing()
        {
            inventory.AddToSlot(0, Wood, 3);

            hand.GatherAll(5);

            Assert.That(hand.IsHolding, Is.False);
            Assert.That(QuantityIn(0), Is.EqualTo(3));
        }

        [Test]
        public void ARightPress_TakesHalf_RoundedUp_ThenPutsDownOne()
        {
            inventory.AddToSlot(0, Wood, 7);

            hand.PressRight(0);
            Assert.That(hand.Quantity, Is.EqualTo(4));
            Assert.That(QuantityIn(0), Is.EqualTo(3));

            hand.PressRight(5);
            Assert.That(QuantityIn(5), Is.EqualTo(1));
            Assert.That(hand.Quantity, Is.EqualTo(3));

            hand.PressRight(0);
            Assert.That(QuantityIn(0), Is.EqualTo(4));
        }

        [Test]
        public void ARightPress_OnAnotherItem_DoesNothing()
        {
            inventory.AddToSlot(0, Wood, 2);
            inventory.AddToSlot(1, Spore, 2);
            hand.PressRight(0);

            hand.PressRight(1);

            Assert.That(hand.Quantity, Is.EqualTo(1));
            Assert.That(QuantityIn(1), Is.EqualTo(2));
        }

        [Test]
        public void PuttingBack_ReturnsTheItemToItsSlot()
        {
            inventory.AddToSlot(6, Wood, 5);
            hand.PressLeft(6, 0f);

            hand.PutBack();

            Assert.That(hand.IsHolding, Is.False);
            Assert.That(QuantityIn(6), Is.EqualTo(5));
        }

        [Test]
        public void PuttingBack_UsesAnotherSlot_WhenItsOwnIsTaken()
        {
            inventory.AddToSlot(6, Wood, 5);
            hand.PressLeft(6, 0f);
            inventory.AddToSlot(6, Spore, 1);

            hand.PutBack();

            Assert.That(hand.IsHolding, Is.False);
            Assert.That(inventory.Count("wood"), Is.EqualTo(5));
        }

        [Test]
        public void PuttingBack_KeepsInHandWhatFitsNowhere()
        {
            var tiny = new PlayerInventory(1);
            var tinyHand = new InventoryHand(tiny);
            tiny.AddToSlot(0, Wood, 5);
            tinyHand.PressLeft(0, 0f);
            tiny.AddToSlot(0, Spore, 1);

            tinyHand.PutBack();

            Assert.That(tinyHand.Quantity, Is.EqualTo(5));
        }
    }
}
