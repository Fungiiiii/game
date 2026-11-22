using Fungiiiii.Champimaison;
using Fungiiiii.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class ThreadProductionTests
    {
        private static readonly ItemDefinition Thread = ItemDefinition.Create("mycelium_thread", Color.white, 16);

        [Test]
        public void NothingIsProduced_BeforeTheProductionStarts()
        {
            var production = new ThreadProduction(10f, 3);

            production.Tick(100f);

            Assert.That(production.Stored, Is.Zero);
        }

        [Test]
        public void OneThread_IsProducedEveryPeriod()
        {
            var production = new ThreadProduction(10f, 3);
            production.Start();

            production.Tick(9f);
            Assert.That(production.Stored, Is.Zero);

            production.Tick(1f);
            Assert.That(production.Stored, Is.EqualTo(1));

            production.Tick(10f);
            Assert.That(production.Stored, Is.EqualTo(2));
        }

        [Test]
        public void AFullStock_Waits()
        {
            var production = new ThreadProduction(10f, 2);
            production.Start();

            production.Tick(1000f);
            Assert.That(production.Stored, Is.EqualTo(2));

            production.Collect(new PlayerInventory(15), Thread);
            production.Tick(9f);
            Assert.That(production.Stored, Is.Zero, "Time spent full counted towards the next thread.");
        }

        [Test]
        public void Collect_GivesTheStock_AndKeepsWhatDoesNotFit()
        {
            var production = new ThreadProduction(1f, 3);
            production.Start();
            production.Tick(3f);
            var full = new PlayerInventory(1);
            full.Add(ItemDefinition.Create("other", Color.red, 1), 1);

            Assert.That(production.Collect(full, Thread), Is.Zero);
            Assert.That(production.Stored, Is.EqualTo(3));

            var inventory = new PlayerInventory(15);
            Assert.That(production.Collect(inventory, Thread), Is.EqualTo(3));
            Assert.That(inventory.Count("mycelium_thread"), Is.EqualTo(3));
            Assert.That(production.Stored, Is.Zero);
        }
    }
}
