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
            var game = Bootstrap;

            game.SetInventoryOpen(true);
            yield return null;

            var panel = game.Crafting;
            Assert.That(panel.isActiveAndEnabled, Is.True, "The crafting panel does not open with the inventory.");
            Assert.That(panel.Rows.Count, Is.EqualTo(panel.Book.Recipes.Count));
            Assert.That(panel.OpenedRecipe, Is.Null);
            Assert.That(panel.AreDetailsShown, Is.False);
            Assert.That(panel.Rows.All(row => !row.CanBeCrafted), Is.True, "A recipe can be made from an empty inventory.");

            // Both keys always there, craft on the left, track on the right, greyed while nothing is open.
            Assert.That(panel.Hints.Select(hint => hint.LabelKey), Is.EqualTo(new[] { UiText.HintCraft, UiText.HintTrack }));
            Assert.That(panel.Hints.All(hint => !hint.IsAvailable), Is.True, "A key is lit while no recipe is open.");
        }

        [UnityTest]
        public IEnumerator AClick_OpensARecipe_ASecondClickMakesIt_InASecond_IntoTheQuickBar()
        {
            yield return Load();
            var game = Bootstrap;
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
            Assert.That(panel.Hints[0].LabelKey, Is.EqualTo(UiText.HintCraft), "The craft key is not first.");
            Assert.That(panel.Hints[0].IsAvailable, Is.True, "The craft key is greyed for a recipe that can be made.");

            panel.Click(slingshot);
            yield return null;

            // Since the playtest of 22/11, an item takes a moment to make, as in other games.
            Assert.That(panel.IsMaking, Is.True, "The second click does not start making the item.");
            Assert.That(game.Inventory.Count("slingshot"), Is.Zero, "The item came at once.");
            yield return Wait(0.5f);
            Assert.That(panel.MakingProgress, Is.InRange(0.2f, 0.8f), "The bar does not fill with time.");
            panel.Click(slingshot);
            Assert.That(panel.MakingProgress, Is.GreaterThan(0.2f), "A click while making started it again.");
            yield return WaitUntil(() => !panel.IsMaking, 2f);

            Assert.That(game.Inventory.Count("slingshot"), Is.EqualTo(1));
            Assert.That(game.Inventory.Count(Wood(game).Id), Is.EqualTo(1));
            Assert.That(game.Inventory.Count(Pebble(game).Id), Is.Zero);
            Assert.That(game.Inventory.GetSlot(Fungiiiii.Inventory.InventoryLayout.Standard.QuickBarFirstSlot).ItemId, Is.EqualTo("slingshot"));
            Assert.That(game.Pickups.Lines.Count, Is.EqualTo(1), "Crafting was not shown as collected.");
        }

        [UnityTest]
        public IEnumerator OpeningAnotherRecipe_OrClosingTheInventory_StopsTheMaking_AndTakesNothing()
        {
            yield return Load();
            var game = Bootstrap;
            game.Inventory.AddToSlot(0, Wood(game), 3);
            game.Inventory.AddToSlot(1, Pebble(game), 1);
            game.SetInventoryOpen(true);
            yield return null;
            var panel = game.Crafting;
            var slingshot = panel.Rows.Select((row, index) => (row, index)).First(pair => pair.row.Recipe.Result.Id == "slingshot").index;
            var net = panel.Rows.Select((row, index) => (row, index)).First(pair => pair.row.Recipe.Result.Id == "net").index;

            panel.Click(slingshot);
            panel.Click(slingshot);
            yield return Wait(0.3f);
            panel.Click(net);
            Assert.That(panel.IsMaking, Is.False, "Opening another recipe did not stop the making.");
            yield return Wait(1.2f);
            Assert.That(game.Inventory.Count("slingshot"), Is.Zero);
            Assert.That(game.Inventory.Count(Wood(game).Id), Is.EqualTo(3), "Something was taken for an item not made.");

            panel.Click(slingshot);
            panel.Click(slingshot);
            yield return Wait(0.3f);
            game.SetInventoryOpen(false);
            yield return Wait(1.2f);
            Assert.That(game.Inventory.Count("slingshot"), Is.Zero, "The item was made with the inventory closed.");
            Assert.That(game.Inventory.Count(Pebble(game).Id), Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator AMissingIngredient_ShowsWhereItComesFrom_AndTheCraftKeyIsGreyed()
        {
            yield return Load();
            var game = Bootstrap;
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
            Assert.That(panel.Hints[0].LabelKey, Is.EqualTo(UiText.HintCraft), "The craft key left its place.");
            Assert.That(panel.Hints[0].IsAvailable, Is.False, "The craft key is lit for a recipe that cannot be made.");
            Assert.That(panel.Hints[1].LabelKey, Is.EqualTo(UiText.HintTrack));
            Assert.That(panel.Hints[1].IsAvailable, Is.True);
        }

        [UnityTest]
        public IEnumerator ATrackedRecipe_ShowsTopRight_UntilItIsCrafted()
        {
            yield return Load();
            var game = Bootstrap;
            var netRecipe = game.Crafting.Book.Recipes.First(recipe => recipe.Result.Id == "net");

            game.Tracker.Toggle(netRecipe);
            yield return null;
            var hud = game.ObjectiveHud;
            Assert.That(hud.TrackedViews.Count(view => view.isActiveAndEnabled), Is.EqualTo(1));
            // Since the playtest of 23/11: a line and a heading set it apart from the quest.
            Assert.That(hud.ShowsTrackedHeader, Is.True, "Nothing sets the tracked recipe apart from the quest.");
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
            yield return WaitUntil(() => !game.Crafting.IsMaking, 2f);

            Assert.That(game.Inventory.Count("net"), Is.EqualTo(1));
            Assert.That(game.Tracker.Tracked, Is.Empty, "A crafted recipe is still tracked.");
            Assert.That(hud.TrackedViews.Any(view => view.isActiveAndEnabled), Is.False);
            Assert.That(hud.ShowsTrackedHeader, Is.False, "The heading stays with no recipe tracked.");
        }
    }
}
