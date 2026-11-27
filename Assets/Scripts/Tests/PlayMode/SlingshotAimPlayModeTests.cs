using System.Collections;
using Fungiiiii.Items;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using static Fungiiiii.Tests.PlayMode.PlaytestLoopGame;

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>Aiming the slingshot with the right button, through the player's hands.</summary>
    public sealed class SlingshotAimPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator HoldingTheRightButton_WithPebbles_NarrowsTheView_EvenAfterTheInventory()
        {
            var mouse = InputSystem.AddDevice<Mouse>();
            InputSystem.AddDevice<Keyboard>();
            yield return Load();
            var game = Bootstrap;
            var zoom = game.ViewCamera.GetComponent<AimZoom>();
            game.Inventory.AddToSlot(1, Pebble(game), 5);
            PutInHand(game, Slingshot(game));
            yield return null;

            Press(mouse.rightButton);
            yield return Wait(0.4f);
            Assert.That(game.Slingshot.IsAiming, Is.True, "Holding the right button does not aim.");
            Assert.That(game.ViewCamera.fieldOfView, Is.LessThan(zoom.BaseFieldOfView - 1f), "The view does not narrow while aiming.");

            Release(mouse.rightButton);
            game.SetInventoryOpen(true);
            yield return null;
            game.SetInventoryOpen(false);
            yield return null;

            Press(mouse.rightButton);
            yield return Wait(0.4f);
            Assert.That(game.Slingshot.IsAiming, Is.True, "After the inventory, the right button no longer aims.");
            Assert.That(game.ViewCamera.fieldOfView, Is.LessThan(zoom.BaseFieldOfView - 1f));
            Release(mouse.rightButton);
        }
    }
}
