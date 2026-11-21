using System.Collections;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class InventoryDemoControllerPlayModeTests
    {
        private const float TextTimeoutSeconds = 5f;

        // The sample items: 5 mushrooms (a full stack) in the quick bar's first slot and
        // 1 more in slot 1, 8 spores in the quick bar's third slot.
        private const int FullMushrooms = InventoryDemoController.MainSlotCount;
        private const int LoneMushroom = 1;
        private const int Spores = InventoryDemoController.MainSlotCount + 2;

        private GameObject subject;
        private InventoryDemoController controller;

        [UnitySetUp]
        public IEnumerator CreateTheScreen()
        {
            subject = new GameObject("Inventory Under Test");
            controller = subject.AddComponent<InventoryDemoController>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator DestroyTheScreen()
        {
            if (subject != null)
            {
                Object.Destroy(subject);
            }

            yield return null;
        }

        private void Press(int slot, PointerEventData.InputButton button)
        {
            var slotObject = subject.transform.Find(
                slot < InventoryDemoController.MainSlotCount
                    ? $"InventoryCanvas/InventoryPanel/InventoryGrid_3x4/InventorySlot_{slot}"
                    : $"InventoryCanvas/InventoryPanel/Hotbar_3Slots/InventorySlot_{slot}").gameObject;
            var press = new PointerEventData(EventSystem.current) { button = button, position = new Vector2(100f, 100f) };
            ExecuteEvents.Execute(slotObject, press, ExecuteEvents.pointerDownHandler);
        }

        private int QuantityIn(int slot) => controller.Inventory.GetSlot(slot).Quantity;

        private GameObject SlotObject(int slot)
        {
            return subject.transform.Find(
                slot < InventoryDemoController.MainSlotCount
                    ? $"InventoryCanvas/InventoryPanel/InventoryGrid_3x4/InventorySlot_{slot}"
                    : $"InventoryCanvas/InventoryPanel/Hotbar_3Slots/InventorySlot_{slot}").gameObject;
        }

        private UnityEngine.UI.Selectable SelectableOf(int slot) => SlotObject(slot).GetComponent<UnityEngine.UI.Selectable>();

        [UnityTest]
        public IEnumerator TheSlots_AreLinkedForTheGamepad_TheGridThenTheQuickBar()
        {
            yield return null;
            var first = SelectableOf(0).navigation;
            Assert.That(first.selectOnRight, Is.SameAs(SelectableOf(1)));
            Assert.That(first.selectOnDown, Is.SameAs(SelectableOf(3)));
            Assert.That(first.selectOnLeft, Is.Null);
            Assert.That(first.selectOnUp, Is.Null);

            var bottomLeft = SelectableOf(9).navigation;
            Assert.That(bottomLeft.selectOnDown, Is.SameAs(SelectableOf(InventoryDemoController.MainSlotCount)), "The grid does not lead to the quick bar.");

            var quickBar = SelectableOf(InventoryDemoController.MainSlotCount + 2).navigation;
            Assert.That(quickBar.selectOnUp, Is.SameAs(SelectableOf(11)));
            Assert.That(quickBar.selectOnRight, Is.Null);
            Assert.That(quickBar.selectOnDown, Is.Null);
        }

        [UnityTest]
        public IEnumerator ASelectedSlot_HasTheFocus_AndClosingTheScreen_ClearsIt()
        {
            EventSystem.current.SetSelectedGameObject(SlotObject(4));
            yield return null;
            Assert.That(controller.FocusedSlot, Is.EqualTo(4));

            subject.SetActive(false);
            subject.SetActive(true);
            yield return null;

            Assert.That(controller.FocusedSlot, Is.EqualTo(-1), "The slot kept the focus after the screen closed.");
            Assert.That(EventSystem.current.currentSelectedGameObject, Is.Not.SameAs(SlotObject(4)));
        }

        [UnityTest]
        public IEnumerator Submit_OnTheSelectedSlot_TakesItsStack_AsALeftClick()
        {
            EventSystem.current.SetSelectedGameObject(SlotObject(Spores));
            ExecuteEvents.Execute(SlotObject(Spores), new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
            yield return null;

            Assert.That(controller.Hand.Quantity, Is.EqualTo(8));
            Assert.That(controller.Inventory.GetSlot(Spores).IsEmpty, Is.True);
        }

        [UnityTest]
        public IEnumerator X_OnTheFocusedSlot_TakesHalf()
        {
            EventSystem.current.SetSelectedGameObject(SlotObject(Spores));
            yield return null;

            controller.TakeHalfOnFocusedSlot();

            Assert.That(controller.Hand.Quantity, Is.EqualTo(4));
            Assert.That(QuantityIn(Spores), Is.EqualTo(4));
        }

        [UnityTest]
        public IEnumerator Y_OnTheFocusedSlot_GathersEveryStackOfTheItem()
        {
            EventSystem.current.SetSelectedGameObject(SlotObject(LoneMushroom));
            yield return null;

            controller.GatherAllOnFocusedSlot();

            Assert.That(controller.Hand.Item.Id, Is.EqualTo("demo_mushroom"));
            Assert.That(controller.Hand.Quantity, Is.EqualTo(5), "Not a full stack gathered.");
        }

        [UnityTest]
        public IEnumerator Controller_CreatesThreeByFourInventoryAndQuickBar()
        {
            Assert.That(controller.Inventory.SlotCount, Is.EqualTo(InventoryDemoController.TotalSlotCount));
            Assert.That(controller.Inventory.Count("demo_mushroom"), Is.EqualTo(6));
            Assert.That(subject.transform.Find("InventoryCanvas/InventoryPanel/InventoryGrid_3x4"), Is.Not.Null);
            Assert.That(subject.transform.Find("InventoryCanvas/InventoryPanel/Hotbar_3Slots"), Is.Not.Null);
            Assert.That(subject.GetComponentInChildren<UnityEngine.UI.Image>().sprite, Is.Not.Null);

            UnityEngine.UI.Button[] controls = subject.GetComponentsInChildren<UnityEngine.UI.Button>();
            Assert.That(controls, Is.Empty);

            // Without a CanvasRenderer, the panel, the slots and the key pills are not drawn.
            var shapes = subject.GetComponentsInChildren<CutCornerGraphic>();
            Assert.That(shapes, Is.Not.Empty);
            foreach (var shape in shapes)
            {
                Assert.That(shape.GetComponent<CanvasRenderer>(), Is.Not.Null, $"{shape.name} cannot be drawn.");
            }

            Assert.That(controller.Inventory.GetSlot(InventoryDemoController.MainSlotCount).ItemId, Is.EqualTo("demo_mushroom"));
            Assert.That(controller.Inventory.GetSlot(InventoryDemoController.MainSlotCount).Quantity, Is.EqualTo(5));
            Assert.That(controller.Inventory.GetSlot(InventoryDemoController.MainSlotCount + 1).ItemId, Is.EqualTo("demo_herb"));
            Assert.That(controller.Inventory.GetSlot(InventoryDemoController.MainSlotCount + 2).ItemId, Is.EqualTo("demo_spore"));

            Assert.That(controller.Inventory.MoveOrSwap(InventoryDemoController.MainSlotCount, 0), Is.True);
            Assert.That(controller.Inventory.GetSlot(0).ItemId, Is.EqualTo("demo_mushroom"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator InventoryPrototypeScene_BootstrapsInventoryController()
        {
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/Prototype/InventaireScene.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return load;

            InventoryDemoController sceneController = Object.FindFirstObjectByType<InventoryDemoController>();
            Assert.That(sceneController, Is.Not.Null);
            Assert.That(sceneController.Inventory.Count("demo_mushroom"), Is.EqualTo(6));
            Assert.That(sceneController.Inventory.SlotCount, Is.EqualTo(InventoryDemoController.TotalSlotCount));
        }

        [UnityTest]
        public IEnumerator TheItemIsInHand_AsSoonAsTheSlotIsPressed()
        {
            Press(Spores, PointerEventData.InputButton.Left);

            Assert.That(controller.Hand.Quantity, Is.EqualTo(8), "The press waited before taking the stack.");
            Assert.That(subject.transform.Find("InventoryCanvas/HeldItem").gameObject.activeSelf, Is.True, "Nothing follows the mouse.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator ADoublePressWithin250ms_GathersEverythingOntoTheMouse()
        {
            Press(LoneMushroom, PointerEventData.InputButton.Left);
            Press(LoneMushroom, PointerEventData.InputButton.Left);

            Assert.That(controller.Hand.Quantity, Is.EqualTo(5), "A full stack of mushrooms is 5.");
            Assert.That(QuantityIn(FullMushrooms), Is.EqualTo(1));
            yield return null;
        }

        [UnityTest]
        public IEnumerator ASecondPressAfter300ms_PutsTheItemBack()
        {
            Press(LoneMushroom, PointerEventData.InputButton.Left);
            yield return new WaitForSecondsRealtime(0.3f);
            Press(LoneMushroom, PointerEventData.InputButton.Left);

            Assert.That(controller.Hand.IsHolding, Is.False);
            Assert.That(QuantityIn(LoneMushroom), Is.EqualTo(1));
            Assert.That(QuantityIn(FullMushrooms), Is.EqualTo(5));
        }

        [UnityTest]
        public IEnumerator ARightPress_TakesHalf()
        {
            Press(Spores, PointerEventData.InputButton.Right);

            Assert.That(controller.Hand.Quantity, Is.EqualTo(4));
            Assert.That(QuantityIn(Spores), Is.EqualTo(4));
            yield return null;
        }

        [UnityTest]
        public IEnumerator ClosingTheInventory_PutsBackWhatWasHeld()
        {
            Press(Spores, PointerEventData.InputButton.Left);

            subject.SetActive(false);

            Assert.That(controller.Hand.IsHolding, Is.False);
            Assert.That(QuantityIn(Spores), Is.EqualTo(8));
            yield return null;
        }

        [UnityTest]
        public IEnumerator ASlotWithOneItem_ShowsNoNumber()
        {
            Assert.That(controller.QuantityTextOf(LoneMushroom), Is.Empty);
            Assert.That(controller.QuantityTextOf(FullMushrooms), Is.EqualTo("5"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator TheTitles_FollowTheLanguage()
        {
            yield return LocalizationSettings.InitializationOperation;

            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("en");
            yield return WaitForText(controller.Title, "Inventory");

            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("fr");
            yield return WaitForText(controller.Title, "Inventaire");
        }

        private static IEnumerator WaitForText(UnityEngine.UI.Text label, string expected)
        {
            var elapsed = 0f;
            while (elapsed < TextTimeoutSeconds && label.text != expected)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(label.text, Is.EqualTo(expected));
        }
    }
}
