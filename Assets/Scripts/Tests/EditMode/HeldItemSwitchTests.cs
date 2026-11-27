using System.Collections.Generic;
using Fungiiiii.Inventory;
using Fungiiiii.Items;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class HeldItemSwitchTests
    {
        private readonly List<Object> created = new List<Object>();
        private ItemDefinition net;
        private ItemDefinition axe;
        private ItemDefinition wood;
        private FakeHeldItem netInHand;
        private FakeHeldItem axeInHand;
        private int prefabsMade;
        private HeldItemSwitch hands;

        [SetUp]
        public void SetUp()
        {
            net = Item("net");
            axe = Item("axe");
            wood = Item("wood");
            netInHand = new FakeHeldItem();
            axeInHand = new FakeHeldItem();
            prefabsMade = 0;
            hands = new HeldItemSwitch(item =>
            {
                prefabsMade++;
                return item == axe ? axeInHand : null;
            });
            hands.Register(net, netInHand);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var asset in created)
            {
                Object.DestroyImmediate(asset);
            }

            created.Clear();
        }

        [Test]
        public void HoldingARegisteredItem_EquipsIt()
        {
            hands.Hold(net);

            Assert.That(hands.Current, Is.SameAs(netInHand));
            Assert.That(hands.CurrentItem, Is.SameAs(net));
            Assert.That(netInHand.Calls, Is.EqualTo(new[] { "Equip" }));
        }

        [Test]
        public void HoldingAnotherItem_PutsTheFirstAway()
        {
            hands.Hold(net);

            hands.Hold(wood);

            Assert.That(netInHand.Calls, Is.EqualTo(new[] { "Equip", "Unequip" }));
            Assert.That(hands.CurrentItem, Is.SameAs(wood));
            Assert.That(hands.Current, Is.Null, "Wood does nothing in hand.");
        }

        [Test]
        public void HoldingTheSameItemAgain_DoesNotEquipItTwice()
        {
            hands.Hold(net);

            hands.Hold(net);

            Assert.That(netInHand.Calls, Is.EqualTo(new[] { "Equip" }));
        }

        [Test]
        public void EmptyHands_HoldNothing()
        {
            hands.Hold(net);

            hands.Hold(null);

            Assert.That(hands.Current, Is.Null);
            Assert.That(hands.CurrentItem, Is.Null);
            Assert.That(netInHand.Calls, Is.EqualTo(new[] { "Equip", "Unequip" }));
        }

        [Test]
        public void TheControls_GoToTheItemInHandOnly()
        {
            hands.Hold(net);

            hands.UsePrimary();
            hands.HoldSecondary(true);
            hands.Cancel();

            Assert.That(netInHand.Calls, Is.EqualTo(new[] { "Equip", "UsePrimary", "HoldSecondary:True", "Cancel" }));
            Assert.That(axeInHand.Calls, Is.Empty);
        }

        [Test]
        public void TheControls_DoNothing_WithEmptyHands()
        {
            Assert.DoesNotThrow(() =>
            {
                hands.UsePrimary();
                hands.HoldSecondary(true);
                hands.Cancel();
            });
        }

        [Test]
        public void AnItemWithAHeldPrefab_IsMadeFromIt_OnlyOnce()
        {
            var axeModel = new GameObject("Axe Model");
            created.Add(axeModel);
            SetHeldPrefab(axe, axeModel);

            hands.Hold(axe);
            hands.Hold(net);
            hands.Hold(axe);

            Assert.That(prefabsMade, Is.EqualTo(1));
            Assert.That(axeInHand.Calls, Is.EqualTo(new[] { "Equip", "Unequip", "Equip" }));
        }

        [Test]
        public void AnItemWithoutHeldPrefab_IsNeverMade()
        {
            hands.Hold(wood);

            Assert.That(prefabsMade, Is.EqualTo(0));
        }

        private ItemDefinition Item(string id)
        {
            var item = ItemDefinition.Create(id, Color.white, 1);
            created.Add(item);
            return item;
        }

        private static void SetHeldPrefab(ItemDefinition item, GameObject prefab)
        {
            var serialized = new UnityEditor.SerializedObject(item);
            serialized.FindProperty("heldPrefab").objectReferenceValue = prefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private sealed class FakeHeldItem : IHeldItem
        {
            public List<string> Calls { get; } = new List<string>();

            public void Equip() => Calls.Add("Equip");

            public void Unequip() => Calls.Add("Unequip");

            public void UsePrimary() => Calls.Add("UsePrimary");

            public void HoldSecondary(bool held) => Calls.Add($"HoldSecondary:{held}");

            public void Cancel() => Calls.Add("Cancel");
        }
    }
}
