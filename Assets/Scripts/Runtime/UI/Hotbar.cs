using System;
using System.Collections.Generic;
using Fungiiiii.Inventory;
using Fungiiiii.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The quick bar of the game, bottom centre: three wooden planks showing three slots
    /// of the inventory. One is selected: the mouse wheel, the keys 1 to 3 and the
    /// gamepad shoulders change it, and the selected plank gets lighter and rises.
    /// What the selected slot holds is what the player has in hand; its name shows above
    /// the planks for a moment when it changes (<see cref="HeldItemName"/>).
    ///
    /// The controls are given by the owner, from the player's controls.
    /// </summary>
    internal sealed class Hotbar : MonoBehaviour, IItemSelection
    {
        private const float TransitionSeconds = 0.12f;

        [Tooltip("The planks' parent, bottom centre; it lays them out.")]
        [SerializeField]
        private RectTransform slots;

        [SerializeField]
        private HotbarSlot slotPrefab;

        [Tooltip("The name of the item in hand, above the planks.")]
        [SerializeField]
        private HeldItemName heldItemName;

        [Tooltip("Where each plank's end facet starts, in turn, so that the planks do not look stamped.")]
        [SerializeField]
        private float[] endFacetStarts = { 0.7f, 0.6f, 0.76f };

        private PlayerInventory inventory;
        private int firstSlot;
        private HotbarSlot[] views;
        private InputAction nextAction;
        private InputAction previousAction;
        private IReadOnlyList<InputAction> slotActions;

        /// <summary>Raised when another slot is selected, or when what the selected slot holds changes.</summary>
        public event Action SelectedSlotChanged;

        /// <summary>Position of the selected slot in the bar, from 0.</summary>
        public int Selected { get; private set; }

        public int SlotCount => views.Length;

        /// <summary>The inventory slot that is selected.</summary>
        public InventorySlot SelectedSlot => inventory.GetSlot(firstSlot + Selected);

        public bool IsVisible => slots.gameObject.activeSelf;

        /// <summary>The row of slots, laid out on the screen.</summary>
        public RectTransform Slots => slots;

        /// <summary>The name of the item in hand, above the planks.</summary>
        public HeldItemName HeldItemName => heldItemName;

        /// <param name="playerInventory">The inventory whose slots the bar shows.</param>
        /// <param name="first">Index of the bar's first slot in the inventory.</param>
        /// <param name="count">Number of slots in the bar.</param>
        /// <param name="next">Selects the next slot, the last going round to the first.</param>
        /// <param name="previous">Selects the previous slot.</param>
        /// <param name="slotKeys">Select a slot directly: the first key the first slot, and so on.</param>
        public void Initialize(
            PlayerInventory playerInventory,
            int first,
            int count,
            InputAction next,
            InputAction previous,
            IReadOnlyList<InputAction> slotKeys)
        {
            nextAction = next;
            previousAction = previous;
            slotActions = slotKeys;
            inventory = playerInventory;
            firstSlot = first;

            views = new HotbarSlot[count];
            for (var index = 0; index < count; index++)
            {
                views[index] = Instantiate(slotPrefab, slots);
                views[index].EndFacetStart = endFacetStarts[index % endFacetStarts.Length];
            }

            inventory.Changed += HandleInventoryChanged;
            Refresh();
            views[Selected].Target = 1f;
            if (heldItemName != null)
            {
                heldItemName.Bind(this);
            }
        }

        /// <summary>Selects the slot at <paramref name="position"/> in the bar, from 0.</summary>
        public void Select(int position)
        {
            if (position < 0 || position >= SlotCount)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            if (position == Selected)
            {
                return;
            }

            views[Selected].Target = 0f;
            Selected = position;
            views[Selected].Target = 1f;
            SelectedSlotChanged?.Invoke();
        }

        public void SetVisible(bool visible)
        {
            slots.gameObject.SetActive(visible);
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.Changed -= HandleInventoryChanged;
            }
        }

        private void Update()
        {
            if (views == null)
            {
                return;
            }

            // Its actions are in the Player map, off while a menu is open.
            ReadInput();

            foreach (var view in views)
            {
                view.Animate(Time.unscaledDeltaTime / TransitionSeconds);
            }
        }

        private void ReadInput()
        {
            if (nextAction.WasPressedThisFrame())
            {
                Select((Selected + 1) % SlotCount);
            }
            else if (previousAction.WasPressedThisFrame())
            {
                Select((Selected + SlotCount - 1) % SlotCount);
            }

            for (var index = 0; index < slotActions.Count && index < SlotCount; index++)
            {
                if (slotActions[index].WasPressedThisFrame())
                {
                    Select(index);
                }
            }
        }

        private void HandleInventoryChanged()
        {
            Refresh();
            SelectedSlotChanged?.Invoke();
        }

        private void Refresh()
        {
            for (var index = 0; index < views.Length; index++)
            {
                views[index].Show(inventory.GetSlot(firstSlot + index));
            }
        }
    }
}
