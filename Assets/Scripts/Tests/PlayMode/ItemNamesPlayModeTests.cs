using System.Collections;
using System.Linq;
using Fungiiiii.Game;
using Fungiiiii.Inventory;
using Fungiiiii.Runtime.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>Item names on the HUD: the item in hand above the quick bar, the plate in the inventory, and the pickup feed bottom left.</summary>
    public sealed class ItemNamesPlayModeTests
    {
        private const string ScenePath = "Assets/Scenes/Game/Game.unity";
        private const float LoadTimeoutSeconds = 60f;

        private GameBootstrap game;

        private IEnumerator LoadGame()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            var elapsed = 0f;
            game = null;
            while (elapsed < LoadTimeoutSeconds && (game == null || !game.IsReady))
            {
                game = Object.FindFirstObjectByType<GameBootstrap>();
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(game != null && game.IsReady, Is.True, "The game was not ready in time.");
            game.Intro.Skip();
        }

        private static IEnumerator Wait(float seconds)
        {
            var elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private InventoryLayout Layout => Object.FindFirstObjectByType<PlayerMotor>().GetComponent<Backpack>().Layout;

        private ItemDefinition Champonge => game.Companions.HeldBehaviours.Select(held => held.Item).First();

        [UnityTest]
        public IEnumerator AnotherItemInHand_ShowsItsNameAboveTheQuickBar_ThenFades()
        {
            yield return LoadGame();
            var heldName = game.Hotbar.HeldItemName;
            Assert.That(heldName, Is.Not.Null, "The quick bar has no name for the item in hand.");
            Assert.That(heldName.IsVisible, Is.False, "A name shows before anything changed in hand.");

            game.Inventory.AddToSlot(Layout.QuickBarSlot(1), Champonge, 1);
            yield return null;
            Assert.That(heldName.IsVisible, Is.False, "A name shows though the item in hand did not change.");

            game.Hotbar.Select(1);
            yield return null;
            Assert.That(heldName.IsVisible, Is.True, "The name of the item taken in hand does not show.");
            Assert.That(heldName.NameKey, Is.EqualTo(Champonge.NameKey));

            yield return Wait(2.6f);
            Assert.That(heldName.IsVisible, Is.False, "The name does not fade out.");

            // The player starts empty-handed: the net is put in the first slot, out of hand.
            game.Inventory.AddToSlot(Layout.QuickBarFirstSlot, game.NetItem, 1);
            yield return null;
            game.Hotbar.Select(0);
            yield return null;
            Assert.That(heldName.IsVisible, Is.True);
            game.Hotbar.Select(2);
            yield return null;
            Assert.That(heldName.IsVisible, Is.False, "Empty hands still show a name.");
        }

        [UnityTest]
        public IEnumerator InTheInventory_TheSlotInFocus_ShowsItsItemsName_ExceptEmptyOrWithAnItemInHand()
        {
            yield return LoadGame();
            var netSlot = Layout.QuickBarFirstSlot;
            // The player starts empty-handed: the net is put in the slot for the test.
            game.Inventory.AddToSlot(netSlot, game.NetItem, 1);
            var emptySlot = 0;
            var screen = game.InventoryView;
            game.SetInventoryOpen(true);
            yield return null;

            screen.Focus(netSlot, true);
            yield return null;
            Assert.That(screen.NamePlate.IsVisible, Is.True, "No name by the slot in focus.");
            Assert.That(screen.NamePlate.NameKey, Is.EqualTo(game.Inventory.GetSlot(netSlot).Item.NameKey));
            // Since the playtest of 22/11, what the item is for, under its name.
            Assert.That(screen.NamePlate.DescriptionKey, Is.EqualTo(game.NetItem.DescriptionKey), "No description under the name.");
            Assert.That(screen.NamePlate.SourceKey, Is.Empty, "A crafted item says where it is found.");

            screen.Focus(netSlot, false);
            screen.Focus(emptySlot, true);
            yield return null;
            Assert.That(screen.NamePlate.IsVisible, Is.False, "An empty slot shows a name.");

            screen.Focus(emptySlot, false);
            screen.Focus(netSlot, true);
            screen.PressSlot(netSlot, UnityEngine.EventSystems.PointerEventData.InputButton.Left, screen.ScreenPointOf(netSlot));
            yield return null;
            Assert.That(screen.Hand.IsHolding, Is.True);
            Assert.That(screen.NamePlate.IsVisible, Is.False, "The name shows over the item in hand.");

            game.SetInventoryOpen(false);
            yield return null;
            Assert.That(screen.NamePlate.IsVisible, Is.False, "The name stays once the inventory is closed.");
        }

        [UnityTest]
        public IEnumerator WhatIsCollected_ShowsLeftOfTheQuickBar_NotAboveIt()
        {
            yield return LoadGame();
            game.Inventory.Collect(Champonge, 1);
            yield return null;
            Canvas.ForceUpdateCanvases();

            Assert.That(game.Pickups.Lines.Count, Is.EqualTo(1));
            var line = WorldCorners((RectTransform)game.Pickups.Lines[0].transform);
            var bar = WorldCorners(game.Hotbar.Slots);

            // Beside the bar, not above it where the held item's name shows.
            Assert.That(line[2].x, Is.LessThan(bar[0].x), "The pickup line is not left of the quick bar.");
            Assert.That(bar[0].x - line[2].x, Is.LessThan(Screen.height * 0.05f), "The pickup line is far from the quick bar.");
            Assert.That(line[0].y, Is.EqualTo(bar[0].y).Within(Screen.height * 0.01f), "The pickup line does not sit on the quick bar's bottom.");

            // Worded by the pickup.line entry, with the amount and the item's name from the table.
            var pickup = game.Pickups.Lines[0];
            yield return Wait(1f);
            Assert.That(pickup.Text, Does.StartWith("+1 "), "The line has no name after its amount.");
            Assert.That(pickup.Text.Length, Is.GreaterThan(3));
            Assert.That(pickup.Text, Does.Not.Contain("No translation").And.Not.Contain("{"), "The pickup.line entry is missing or not filled.");
        }

        private static Vector3[] WorldCorners(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return corners;
        }
    }
}
