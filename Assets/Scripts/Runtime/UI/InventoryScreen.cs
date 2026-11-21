using System;
using System.Collections.Generic;
using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The inventory screen: a 3-column by 4-row item panel and a three-slot quick bar on a
    /// wooden panel, as on the Inventaire artboard (playtest of 20/11, feedback #2). Presses
    /// act at once through an <see cref="InventoryHand"/>; what is held follows the mouse,
    /// and goes back to its slot when the screen closes. As in Minecraft, a click out of
    /// the panel drops what is held, the whole stack or one item, and the drop control
    /// drops one item of the slot under the pointer or the gamepad's focus. Items show as squares of their
    /// colour until their icons are drawn.
    /// </summary>
    public sealed class InventoryScreen : MonoBehaviour, IGameScreen, IHintSource
    {
        private const int NoSlot = -1;

        [Tooltip("The bag's slots; its column count is the number of columns the gamepad moves across.")]
        [SerializeField]
        private GridLayoutGroup bag;

        [SerializeField]
        private RectTransform quickBar;

        [SerializeField]
        private InventorySlotView bagSlotPrefab;

        [SerializeField]
        private InventorySlotView quickBarSlotPrefab;

        [Tooltip("What the mouse holds, on the canvas; it follows the pointer.")]
        [SerializeField]
        private RectTransform held;

        [SerializeField]
        private ItemIcon heldItem;

        [SerializeField]
        private Text heldCount;

        [SerializeField]
        private Text title;

        private PlayerInventory inventory;
        private InventoryLayout layout = InventoryLayout.Standard;
        private InventoryHand hand;
        private InventorySlotView[] slotViews;
        private int focusedSlot = NoSlot;
        private InputAction navigateAction;
        private InputAction takeHalfAction;
        private InputAction gatherAllAction;
        private InputAction dropAction;
        private ControlHint[] hints = new ControlHint[0];

        public PlayerInventory Inventory => inventory;

        /// <summary>Which slots the screen draws as the bag and which as the quick bar.</summary>
        public InventoryLayout Layout => layout;

        private int BagColumnCount => bag.constraintCount;

        private int BagRowCount => (layout.BagSlotCount + BagColumnCount - 1) / BagColumnCount;

        /// <summary>
        /// Shows the inventory a player carries, once. Without it, the screen makes its own
        /// inventory when it first shows, for the standalone prototype scene.
        /// </summary>
        public void Bind(PlayerInventory playerInventory, InventoryLayout inventoryLayout)
        {
            if (playerInventory == null)
            {
                throw new ArgumentNullException(nameof(playerInventory));
            }

            if (playerInventory.SlotCount < inventoryLayout.SlotCount)
            {
                throw new ArgumentException(
                    $"The layout has {inventoryLayout.SlotCount} slots; this inventory has {playerInventory.SlotCount}.",
                    nameof(playerInventory));
            }

            if (inventory != null)
            {
                throw new InvalidOperationException("The inventory screen already shows an inventory.");
            }

            inventory = playerInventory;
            layout = inventoryLayout;
            hand = new InventoryHand(inventory);
            CreateSlots();
            inventory.Changed += RefreshSlots;
            hand.Changed += RefreshHeld;
            RefreshSlots();
            RefreshHeld();
        }

        /// <summary>Raised when items leave the inventory for the world, with how many.</summary>
        public event Action<ItemDefinition, int> Dropped;

        // It has no button of its own that asks to close: Tab and Escape close it.
        event Action IGameScreen.CloseRequested
        {
            add { }
            remove { }
        }

        /// <summary>Shows the screen; the screen stack calls it.</summary>
        public void Open()
        {
            gameObject.SetActive(true);
        }

        /// <summary>Hides the screen, putting back what the mouse held.</summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// The controls of the screen. With a gamepad, the first move selects a slot, X takes
        /// half or puts down one, Y gathers every stack of the item; A and the stick are the
        /// Submit and Navigate of the event system. Close and Cancel only show in the key hints:
        /// the screen stack closes the screen.
        /// </summary>
        public void SetControls(InventoryScreenControls controls)
        {
            navigateAction = controls.Navigate;
            takeHalfAction = controls.TakeHalf;
            gatherAllAction = controls.GatherAll;
            dropAction = controls.Drop;

            // The mouse buttons and A are read by the event system, not by an action of the game.
            hints = new[]
            {
                new ControlHint(UiText.HintTakeOrPlace, HintInput.At("<Mouse>/leftButton"), HintInput.At("<Gamepad>/buttonSouth")),
                new ControlHint(UiText.HintHalfOrOne, HintInput.At("<Mouse>/rightButton"), HintInput.Of(controls.TakeHalf)),
                new ControlHint(UiText.HintGatherAll, HintInput.At("<Mouse>/leftButton"), HintInput.Of(controls.GatherAll), twiceWithMouse: true, optional: true),
                new ControlHint(UiText.HintDrop, HintInput.Of(controls.Drop)),
                new ControlHint(UiText.HintClose, HintInput.Of(controls.Close), HintInput.Of(controls.Cancel)),
            };
        }

        // The screen's hints never change while it is open.
        event Action IHintSource.HintsChanged
        {
            add { }
            remove { }
        }

        /// <summary>What the mouse or the gamepad does on the screen, for the key hints.</summary>
        public IReadOnlyList<ControlHint> Hints => hints;

        /// <summary>The slot the mouse is over or the gamepad has selected, -1 for none.</summary>
        internal int FocusedSlot => focusedSlot;

        /// <summary>What the mouse holds.</summary>
        internal InventoryHand Hand => hand;

        /// <summary>The screen's title, for tests.</summary>
        internal Text Title => title;

        private void Awake()
        {
            if (inventory == null)
            {
                Bind(new PlayerInventory(layout.SlotCount), layout);
            }
        }

        private void OnDisable()
        {
            // Closing the screen puts back what the mouse held.
            hand?.PutBack();
            Unfocus();
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (hand.IsHolding && mouse != null && mouse.delta.ReadValue() != Vector2.zero)
            {
                FollowPointer(mouse.position.ReadValue());
            }

            ReadGamepad();
        }

        private void ReadGamepad()
        {
            if (focusedSlot == NoSlot)
            {
                // Nothing to move from yet: the first push selects the first slot.
                if (navigateAction != null && navigateAction.WasPerformedThisFrame() && navigateAction.ReadValue<Vector2>() != Vector2.zero)
                {
                    EventSystem.current.SetSelectedGameObject(slotViews[0].gameObject);
                }

                return;
            }

            if (takeHalfAction != null && takeHalfAction.WasPressedThisFrame())
            {
                TakeHalfOnFocusedSlot();
            }

            if (gatherAllAction != null && gatherAllAction.WasPressedThisFrame())
            {
                GatherAllOnFocusedSlot();
            }

            if (dropAction != null && dropAction.WasPressedThisFrame())
            {
                DropFromFocusedSlot();
            }
        }

        /// <summary>Q or RB: one item of the slot under the pointer, or of the gamepad's focus, goes out.</summary>
        internal void DropFromFocusedSlot()
        {
            if (focusedSlot != NoSlot && inventory.TakeFromSlot(focusedSlot, 1, out var item) > 0)
            {
                Dropped?.Invoke(item, 1);
            }
        }

        /// <summary>A press out of the panel: the left button drops what is held, the right button one item of it.</summary>
        internal void PressOutside(PointerEventData.InputButton button)
        {
            if (button != PointerEventData.InputButton.Left && button != PointerEventData.InputButton.Right)
            {
                return;
            }

            var dropped = hand.Drop(button == PointerEventData.InputButton.Left, out var item);
            if (dropped > 0)
            {
                Dropped?.Invoke(item, dropped);
            }
        }

        /// <summary>X on a gamepad, the right click of the mouse.</summary>
        internal void TakeHalfOnFocusedSlot()
        {
            PressSlot(focusedSlot, PointerEventData.InputButton.Right, ScreenPointOf(focusedSlot));
        }

        /// <summary>Y on a gamepad, the double click of the mouse.</summary>
        internal void GatherAllOnFocusedSlot()
        {
            hand.GatherAll(focusedSlot);
            FollowPointer(ScreenPointOf(focusedSlot));
        }

        /// <summary>A slot gets the focus, from the mouse over it or the gamepad selecting it; the held item follows a gamepad.</summary>
        internal void Focus(int index, bool focused)
        {
            slotViews[index].SetHovered(focused);
            if (focused)
            {
                focusedSlot = index;
                var mouse = Mouse.current;
                if (hand.IsHolding && (mouse == null || mouse.delta.ReadValue() == Vector2.zero))
                {
                    FollowPointer(ScreenPointOf(index));
                }
            }
            else if (focusedSlot == index)
            {
                focusedSlot = NoSlot;
            }
        }

        /// <summary>Where a slot stands on the screen, in pixels.</summary>
        internal Vector2 ScreenPointOf(int index)
        {
            return slotViews[index].transform.position;
        }

        /// <summary>
        /// No slot keeps the focus once the screen is closed: the event system cannot tell a
        /// hidden slot it lost it, so the highlight would still show on the next opening.
        /// </summary>
        private void Unfocus()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem != null && focusedSlot != NoSlot && eventSystem.currentSelectedGameObject == slotViews[focusedSlot].gameObject)
            {
                eventSystem.SetSelectedGameObject(null);
            }

            if (slotViews != null)
            {
                foreach (var view in slotViews)
                {
                    view.SetHovered(false);
                }
            }

            focusedSlot = NoSlot;
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.Changed -= RefreshSlots;
            }

            if (hand != null)
            {
                hand.Changed -= RefreshHeld;
            }
        }

        /// <summary>A press on a slot, from its pointer handler or from a test.</summary>
        internal void PressSlot(int index, PointerEventData.InputButton button, Vector2 screenPosition)
        {
            if (button == PointerEventData.InputButton.Left)
            {
                hand.PressLeft(index, Time.unscaledTime);
            }
            else if (button == PointerEventData.InputButton.Right)
            {
                hand.PressRight(index);
            }

            FollowPointer(screenPosition);
        }

        /// <summary>The number shown on a slot, empty when there is none.</summary>
        internal string QuantityTextOf(int index) => slotViews[index].QuantityText;

        /// <summary>The slot's object, which takes the pointer and the gamepad's focus.</summary>
        internal GameObject SlotObject(int index) => slotViews[index].gameObject;

        private void CreateSlots()
        {
            slotViews = new InventorySlotView[layout.SlotCount];
            for (var index = 0; index < layout.BagSlotCount; index++)
            {
                slotViews[index] = CreateSlot(bagSlotPrefab, bag.transform, index);
            }

            for (var position = 0; position < layout.QuickBarSlotCount; position++)
            {
                var index = layout.QuickBarSlot(position);
                slotViews[index] = CreateSlot(quickBarSlotPrefab, quickBar, index);
            }

            LinkSlotNavigation();
        }

        private InventorySlotView CreateSlot(InventorySlotView prefab, Transform parent, int index)
        {
            var slot = Instantiate(prefab, parent);
            slot.name = $"InventorySlot_{index}";
            slot.Initialize(this, index);
            return slot;
        }

        /// <summary>
        /// The stick and the D-pad move from slot to slot as they are laid out: the 3 x 4
        /// grid, then the quick bar under it.
        /// </summary>
        private void LinkSlotNavigation()
        {
            var rows = BagRowCount;
            for (var index = 0; index < layout.SlotCount; index++)
            {
                var isBag = !layout.IsInQuickBar(index);
                var column = isBag ? index % BagColumnCount : index - layout.QuickBarFirstSlot;
                var row = isBag ? index / BagColumnCount : rows;
                var lastColumn = isBag ? Mathf.Min(BagColumnCount, layout.BagSlotCount - row * BagColumnCount) - 1 : layout.QuickBarSlotCount - 1;

                var navigation = new Navigation { mode = Navigation.Mode.Explicit };
                navigation.selectOnLeft = column > 0 ? SelectableOf(index - 1) : null;
                navigation.selectOnRight = column < lastColumn ? SelectableOf(index + 1) : null;
                navigation.selectOnUp = row > 0 ? SelectableOf(Mathf.Min((row - 1) * BagColumnCount + column, layout.BagSlotCount - 1)) : null;
                if (isBag && index + BagColumnCount < layout.BagSlotCount)
                {
                    navigation.selectOnDown = SelectableOf(index + BagColumnCount);
                }
                else if (isBag)
                {
                    navigation.selectOnDown = SelectableOf(layout.QuickBarSlot(Mathf.Min(column, layout.QuickBarSlotCount - 1)));
                }

                SelectableOf(index).navigation = navigation;
            }
        }

        private Selectable SelectableOf(int index) => slotViews[index].GetComponent<Selectable>();

        private void FollowPointer(Vector2 screenPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)held.parent, screenPosition, null, out var local);
            held.anchoredPosition = local;
        }

        private void RefreshSlots()
        {
            if (slotViews == null)
            {
                return;
            }

            for (var index = 0; index < slotViews.Length; index++)
            {
                slotViews[index].Show(inventory.GetSlot(index));
            }
        }

        private void RefreshHeld()
        {
            held.gameObject.SetActive(hand.IsHolding);
            if (hand.IsHolding)
            {
                heldItem.Show(hand.Item);
                heldCount.text = StackLabel.For(hand.Quantity);
                held.SetAsLastSibling();
            }
        }

    }

    /// <summary>The actions the inventory screen uses or shows.</summary>
    public readonly struct InventoryScreenControls
    {
        public InventoryScreenControls(InputAction navigate, InputAction takeHalf, InputAction gatherAll, InputAction drop, InputAction close, InputAction cancel)
        {
            Navigate = navigate;
            TakeHalf = takeHalf;
            GatherAll = gatherAll;
            Drop = drop;
            Close = close;
            Cancel = cancel;
        }

        public InputAction Navigate { get; }

        public InputAction TakeHalf { get; }

        public InputAction GatherAll { get; }

        /// <summary>Drops one item of the slot in focus: Q, RB.</summary>
        public InputAction Drop { get; }

        /// <summary>Closes the screen with the keyboard: Tab.</summary>
        public InputAction Close { get; }

        /// <summary>Closes the screen with a gamepad: B.</summary>
        public InputAction Cancel { get; }
    }
}
