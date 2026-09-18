using Fungiiiii.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using System.Collections;

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class InventoryDemoControllerPlayModeTests
    {
        [UnityTest]
        public IEnumerator Controller_CreatesThreeByFourInventoryAndQuickBar()
        {
            var subject = new GameObject(nameof(Controller_CreatesThreeByFourInventoryAndQuickBar));
            InventoryDemoController controller = subject.AddComponent<InventoryDemoController>();

            yield return null;

            Assert.That(controller.Inventory.SlotCount, Is.EqualTo(InventoryDemoController.TotalSlotCount));
            Assert.That(controller.Inventory.Count("demo_mushroom"), Is.EqualTo(6));
            Assert.That(subject.transform.Find("InventoryCanvas/InventoryPanel/InventoryGrid_3x4"), Is.Not.Null);
            Assert.That(subject.transform.Find("InventoryCanvas/InventoryPanel/Hotbar_3Slots"), Is.Not.Null);
            Assert.That(subject.GetComponentInChildren<UnityEngine.UI.Image>().sprite, Is.Not.Null);

            UnityEngine.UI.Button[] controls = subject.GetComponentsInChildren<UnityEngine.UI.Button>();
            Assert.That(controls, Is.Empty);

            Assert.That(controller.Inventory.GetSlot(InventoryDemoController.MainSlotCount).ItemId, Is.EqualTo("demo_mushroom"));
            Assert.That(controller.Inventory.GetSlot(InventoryDemoController.MainSlotCount).Quantity, Is.EqualTo(5));
            Assert.That(controller.Inventory.GetSlot(InventoryDemoController.MainSlotCount + 1).ItemId, Is.EqualTo("demo_herb"));
            Assert.That(controller.Inventory.GetSlot(InventoryDemoController.MainSlotCount + 2).ItemId, Is.EqualTo("demo_spore"));

            Assert.That(controller.Inventory.MoveOrSwap(InventoryDemoController.MainSlotCount, 0), Is.True);
            Assert.That(controller.Inventory.GetSlot(0).ItemId, Is.EqualTo("demo_mushroom"));

            Object.Destroy(subject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SampleScene_BootstrapsInventoryController()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("SampleScene");
            yield return load;

            InventoryDemoController controller = Object.FindFirstObjectByType<InventoryDemoController>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.Inventory.Count("demo_mushroom"), Is.EqualTo(6));
            Assert.That(controller.Inventory.SlotCount, Is.EqualTo(InventoryDemoController.TotalSlotCount));
        }
    }
}
