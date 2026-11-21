using System.Collections.Generic;
using Fungiiiii.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class ItemCatalogTests
    {
        private readonly List<Object> created = new List<Object>();

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
        public void AnItem_IsFoundByItsId()
        {
            var wood = Item("wood");
            var catalog = Catalog(Item("spore"), wood);

            Assert.That(catalog.Get("wood"), Is.SameAs(wood));
            Assert.That(catalog.TryGet("wood", out var found), Is.True);
            Assert.That(found, Is.SameAs(wood));
        }

        [Test]
        public void AnUnknownId_IsNotFound()
        {
            var catalog = Catalog(Item("wood"));

            Assert.That(catalog.TryGet("stone", out var found), Is.False);
            Assert.That(found, Is.Null);
            Assert.Throws<KeyNotFoundException>(() => catalog.Get("stone"));
        }

        [Test]
        public void ASoundCatalog_HasNoProblem()
        {
            Assert.That(Catalog(Item("wood"), Item("spore")).FindProblems(), Is.Empty);
        }

        [Test]
        public void ARepeatedId_IsAProblem()
        {
            var problems = Catalog(Item("wood"), Item("wood")).FindProblems();

            Assert.That(problems, Has.Count.EqualTo(1));
            Assert.That(problems[0], Does.Contain("wood"));
        }

        [Test]
        public void AnEmptyEntry_IsAProblem()
        {
            Assert.That(Catalog(Item("wood"), null).FindProblems(), Has.Count.EqualTo(1));
        }

        [Test]
        public void AnItemWithoutId_CannotBeCreated()
        {
            Assert.Throws<System.ArgumentException>(() => ItemDefinition.Create(" ", Color.white, 1));
        }

        private ItemDefinition Item(string id)
        {
            var item = ItemDefinition.Create(id, Color.white, 4);
            created.Add(item);
            return item;
        }

        private ItemCatalog Catalog(params ItemDefinition[] items)
        {
            var catalog = ItemCatalog.Create(items);
            created.Add(catalog);
            return catalog;
        }
    }
}
