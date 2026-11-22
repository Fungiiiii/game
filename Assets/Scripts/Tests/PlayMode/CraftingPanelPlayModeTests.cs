using System.Collections;
using System.Linq;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Fungiiiii.Tests.PlayMode.PlaytestLoopGame;

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>The crafting panel next to the inventory, and the recipes tracked top right.</summary>
    public sealed class CraftingPanelPlayModeTests
    {
        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator ThePanel_OpensWithTheInventory_EveryRecipeListed_NoneOpen()
        {
            yield return Load();
            var game = Game;

            game.SetInventoryOpen(true);
            yield return null;

            var panel = game.Crafting;
            Assert.That(panel.isActiveAndEnabled, Is.True, "The crafting panel does not open with the inventory.");
            Assert.That(panel.Rows.Count, Is.EqualTo(panel.Book.Recipes.Count));
            Assert.That(panel.OpenedRecipe, Is.Null);
            Assert.That(panel.AreDetailsShown, Is.False);
            Assert.That(panel.Rows.All(row => !row.CanBeCrafted), Is.True, "A recipe can be made from an empty inventory.");
        }

        [UnityTest]
        public IEnumerator AClick_OpensARecipe_ASecondClickCraftsIt_IntoTheQuickBar()
        {
            yield return Load();
            var game = Game;
            game.Inventory.AddToSlot(0, Wood(game), 3);
            game.Inventory.AddToSlot(1, Pebble(game), 1);
            game.SetInventoryOpen(true);
            yield return null;
            var panel = game.Crafting;
            var slingshot = panel.Rows.Select((row, index) => (row, index)).First(pair => pair.row.Recipe.Result.Id == "slingshot").index;

            panel.Click(slingshot);
            Assert.That(panel.OpenedRecipe, Is.SameAs(panel.Rows[slingshot].Recipe));
            Assert.That(panel.AreDetailsShown, Is.True);
            Assert.That(panel.Rows[slingshot].CanBeCrafted, Is.True);
            Assert.That(panel.Hints.Any(hint => hint.LabelKey == UiText.HintCraft), Is.True, "No key to craft.");

            panel.Click(slingshot);
            yield return null;

            Assert.That(game.Inventory.Count("slingshot"), Is.EqualTo(1));
            Assert.That(game.Inventory.Count(Wood(game).Id), Is.EqualTo(1));
            Assert.That(game.Inventory.Count(Pebble(game).Id), Is.Zero);
            Assert.That(game.Inventory.GetSlot(Fungiiiii.Inventory.InventoryLayout.Standard.QuickBarFirstSlot).ItemId, Is.EqualTo("slingshot"));
            Assert.That(game.Pickups.Lines.Count, Is.EqualTo(1), "Crafting was not shown as collected.");
        }

        [UnityTest]
        public IEnumerator AMissingIngredient_ShowsWhereItComesFrom_AndTheCraftKeyIsGone()
        {
            yield return Load();
            var game = Game;
            game.SetInventoryOpen(true);
            yield return null;
            var panel = game.Crafting;
            var net = panel.Rows.Select((row, index) => (row, index)).First(pair => pair.row.Recipe.Result.Id == "net").index;

            panel.Click(net);
            yield return null;

            var thread = panel.IngredientRows[0];
            Assert.That(thread.IsMissing, Is.True);
            Assert.That(thread.ShowsSource, Is.True, "Where the thread comes from is not shown.");
            Assert.That(thread.CountText, Is.EqualTo("0/3"));
            Assert.That(panel.Hints.Any(hint => hint.LabelKey == UiText.HintCraft), Is.False, "The craft key shows for a recipe that cannot be made.");
            Assert.That(panel.Hints.Any(hint => hint.LabelKey == UiText.HintTrack), Is.True);
        }

        [UnityTest]
        public IEnumerator ATrackedRecipe_ShowsTopRight_UntilItIsCrafted()
        {
            yield return Load();
            var game = Game;
            var netRecipe = game.Crafting.Book.Recipes.First(recipe => recipe.Result.Id == "net");

            game.Tracker.Toggle(netRecipe);
            yield return null;
            var hud = game.ObjectiveHud;
            Assert.That(hud.TrackedViews.Count(view => view.isActiveAndEnabled), Is.EqualTo(1));
            Assert.That(hud.TrackedViews[0].Recipe, Is.SameAs(netRecipe));
            Assert.That(hud.TrackedViews[0].Lines[0].CountText, Is.EqualTo("0/3"));

            game.Inventory.Add(Thread(game), 3);
            game.Inventory.Add(Wood(game), 2);
            yield return null;
            Assert.That(hud.TrackedViews[0].Lines[0].CountText, Is.EqualTo("3/3"), "The tracked recipe did not follow the inventory.");

            game.SetInventoryOpen(true);
            yield return null;
            var net = game.Crafting.Rows.Select((row, index) => (row, index)).First(pair => pair.row.Recipe == netRecipe).index;
            game.Crafting.Click(net);
            game.Crafting.Click(net);
            yield return null;

            Assert.That(game.Inventory.Count("net"), Is.EqualTo(1));
            Assert.That(game.Tracker.Tracked, Is.Empty, "A crafted recipe is still tracked.");
            Assert.That(hud.TrackedViews.Any(view => view.isActiveAndEnabled), Is.False);
        }
    }
}
