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
        public IEnumerator Controller_CreatesInteractiveStackUi()
        {
            var subject = new GameObject(nameof(Controller_CreatesInteractiveStackUi));
            InventoryDemoController controller = subject.AddComponent<InventoryDemoController>();

            yield return null;

            Assert.That(controller.Inventory.Count("demo_mushroom"), Is.EqualTo(1));
            Assert.That(subject.transform.Find("InventoryCanvas/InventoryPanel/InventorySlots"), Is.Not.Null);
            Assert.That(subject.GetComponentInChildren<UnityEngine.UI.Image>().sprite, Is.Not.Null);

            UnityEngine.UI.Button[] controls = subject.GetComponentsInChildren<UnityEngine.UI.Button>();
            Assert.That(controls, Has.Length.EqualTo(2));

            for (int i = 0; i < 5; i++)
            {
                controls[0].onClick.Invoke();
            }

            Assert.That(controller.Inventory.GetSlot(0).Quantity, Is.EqualTo(5));
            Assert.That(controller.Inventory.GetSlot(1).Quantity, Is.EqualTo(1));

            controls[1].onClick.Invoke();
            Assert.That(controller.Inventory.Count("demo_mushroom"), Is.EqualTo(5));

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
            Assert.That(controller.Inventory.Count("demo_mushroom"), Is.EqualTo(1));
        }
    }
}
