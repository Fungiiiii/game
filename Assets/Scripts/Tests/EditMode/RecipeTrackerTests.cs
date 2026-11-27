using Fungiiiii.Crafting;
using Fungiiiii.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class RecipeTrackerTests
    {
        private static readonly ItemDefinition Wood = ItemDefinition.Create("wood", Color.yellow, 16);

        private static CraftingRecipe Recipe(string id)
        {
            return CraftingRecipe.Create(ItemDefinition.Create(id, Color.white, 1), new ItemAmount(Wood, 1));
        }

        [Test]
        public void Toggle_TracksARecipe_ThenLetsItGo()
        {
            var tracker = new RecipeTracker();
            var net = Recipe("net");
            var changes = 0;
            tracker.Changed += () => changes++;

            tracker.Toggle(net);
            Assert.That(tracker.IsTracked(net), Is.True);

            tracker.Toggle(net);
            Assert.That(tracker.IsTracked(net), Is.False);
            Assert.That(changes, Is.EqualTo(2));
        }

        [Test]
        public void SeveralRecipes_AreTracked_InTheOrderTheyWereTracked()
        {
            var tracker = new RecipeTracker();
            var net = Recipe("net");
            var slingshot = Recipe("slingshot");

            tracker.Toggle(net);
            tracker.Toggle(slingshot);

            Assert.That(tracker.Tracked, Is.EqualTo(new[] { net, slingshot }));
        }

        [Test]
        public void ACraftedRecipe_IsNoLongerTracked()
        {
            var tracker = new RecipeTracker();
            var net = Recipe("net");
            tracker.Toggle(net);

            tracker.Crafted(net);

            Assert.That(tracker.Tracked, Is.Empty);
        }
    }
}
