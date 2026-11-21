using System;
using System.Collections.Generic;
using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The quick bar of the game, bottom centre: three wooden planks showing three slots
    /// of the inventory. One is selected: the mouse wheel, the keys 1 to 3 and the
    /// gamepad shoulders change it, and the selected plank gets lighter and rises.
    /// What the selected slot holds is what the player has in hand.
    ///
    /// The controls are given by the owner, from the player's controls.
    /// </summary>
    internal sealed class Hotbar : MonoBehaviour
    {
        /// <summary>Above the vitals, under the inventory (100) and the pause menu (200).</summary>
        private const int CanvasSortingOrder = 50;

        private static readonly Vector2 SlotSize = new Vector2(112f, 112f);
        private const float SlotSpacing = 18f;
        private const float BottomMargin = 36f;
        private static readonly Vector2 ItemSize = new Vector2(50f, 50f);
        private static readonly Vector2 HollowSize = new Vector2(64f, 64f);
        private const int QuantityFontSize = 34;

        /// <summary>How far the selected plank rises, in canvas units.</summary>
        private const float SelectedRise = 12f;

        /// <summary>Planks that are not selected are slightly see-through.</summary>
        private const float RestAlpha = 0.75f;

        private const float TransitionSeconds = 0.12f;

        /// <summary>So that the three planks do not look stamped.</summary>
        private static readonly float[] EndFacetStarts = { 0.7f, 0.6f, 0.76f };

        /// <summary>Palette "Body light" and "Wood dark", as on the menu planks.</summary>
        private static readonly Color32 Cream = new Color32(0xF2, 0xE6, 0xCC, 0xFF);
        private static readonly Color32 WoodDark = new Color32(0x4F, 0x34, 0x24, 0xFF);

        private PlayerInventory inventory;
        private int firstSlot;
        private SlotView[] views;
        private GameObject canvasObject;
        private InputAction nextAction;
        private InputAction previousAction;
        private IReadOnlyList<InputAction> slotActions;

        /// <summary>Raised when another slot is selected.</summary>
        public event Action SelectionChanged;

        /// <summary>Position of the selected slot in the bar, from 0.</summary>
        public int Selected { get; private set; }

        public int SlotCount => views.Length;

        /// <summary>The inventory slot that is selected.</summary>
        public InventorySlot SelectedSlot => inventory.GetSlot(firstSlot + Selected);

        /// <summary>False ignores the controls, for instance while a menu uses the mouse.</summary>
        public bool InputEnabled { get; set; } = true;

        public bool IsVisible => canvasObject != null && canvasObject.activeSelf;

        /// <param name="slots">The inventory whose slots the bar shows.</param>
        /// <param name="first">Index of the bar's first slot in the inventory.</param>
        /// <param name="count">Number of slots in the bar.</param>
        /// <param name="next">Selects the next slot, the last going round to the first.</param>
        /// <param name="previous">Selects the previous slot.</param>
        /// <param name="slotKeys">Select a slot directly: the first key the first slot, and so on.</param>
        public void Initialize(
            PlayerInventory slots,
            int first,
            int count,
            Font font,
            InputAction next,
            InputAction previous,
            IReadOnlyList<InputAction> slotKeys)
        {
            nextAction = next;
            previousAction = previous;
            slotActions = slotKeys;
            inventory = slots;
            firstSlot = first;

            var canvas = WoodMenu.CreateCanvas("Hotbar Canvas", CanvasSortingOrder);
            canvasObject = canvas.gameObject;
            canvasObject.transform.SetParent(transform, false);
            var root = (RectTransform)canvas.transform;

            views = new SlotView[count];
            var width = count * SlotSize.x + (count - 1) * SlotSpacing;
            for (var index = 0; index < count; index++)
            {
                var x = -width * 0.5f + SlotSize.x * 0.5f + index * (SlotSize.x + SlotSpacing);
                views[index] = new SlotView(root, font, new Vector2(x, BottomMargin), EndFacetStarts[index % EndFacetStarts.Length]);
            }

            inventory.Changed += Refresh;
            Refresh();
            views[Selected].Target = 1f;
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
            SelectionChanged?.Invoke();
        }

        public void SetVisible(bool visible)
        {
            canvasObject.SetActive(visible);
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.Changed -= Refresh;
            }
        }

        private void Update()
        {
            if (views == null)
            {
                return;
            }

            if (InputEnabled)
            {
                ReadInput();
            }

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

        private void Refresh()
        {
            for (var index = 0; index < views.Length; index++)
            {
                views[index].Show(inventory.GetSlot(firstSlot + index));
            }
        }

        /// <summary>One plank of the bar, with the colour of its item and the quantity.</summary>
        private sealed class SlotView
        {
            private readonly RectTransform face;
            private readonly CanvasGroup faceGroup;
            private readonly PlankGraphic plank;
            private readonly Image item;
            private readonly Text quantity;
            private float current;

            public SlotView(RectTransform parent, Font font, Vector2 bottomCentre, float endFacetStart)
            {
                var slotObject = new GameObject("Hotbar Slot", typeof(RectTransform));
                var rect = (RectTransform)slotObject.transform;
                rect.SetParent(parent, false);
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = bottomCentre;
                rect.sizeDelta = SlotSize;

                // The face rises when selected; the slot's own rectangle stays put.
                var faceObject = new GameObject("Face", typeof(RectTransform), typeof(CanvasGroup));
                face = (RectTransform)faceObject.transform;
                face.SetParent(rect, false);
                Stretch(face);
                faceGroup = faceObject.GetComponent<CanvasGroup>();
                faceGroup.blocksRaycasts = false;

                var plankObject = new GameObject("Plank", typeof(RectTransform), typeof(CanvasRenderer));
                var plankRect = (RectTransform)plankObject.transform;
                plankRect.SetParent(face, false);
                Stretch(plankRect);
                plank = plankObject.AddComponent<PlankGraphic>();
                plank.raycastTarget = false;
                plank.EndFacetStart = endFacetStart;

                // A dark hollow under the item, so that wood shows on a wooden plank.
                CreateSquare("Hollow", face, HollowSize).color = WoodDark;
                item = CreateSquare("Item", face, ItemSize);

                var quantityObject = new GameObject("Quantity", typeof(RectTransform), typeof(Text));
                var quantityRect = (RectTransform)quantityObject.transform;
                quantityRect.SetParent(face, false);
                Stretch(quantityRect);
                quantityRect.offsetMin = new Vector2(0f, 14f);
                quantityRect.offsetMax = new Vector2(-14f, 0f);
                quantity = quantityObject.GetComponent<Text>();
                quantity.font = font;
                quantity.fontSize = QuantityFontSize;
                quantity.color = Cream;
                quantity.alignment = TextAnchor.LowerRight;
                quantity.raycastTarget = false;
                var shadow = quantityObject.AddComponent<Shadow>();
                shadow.effectColor = WoodDark;
                shadow.effectDistance = new Vector2(0f, -3f);

                Apply();
            }

            /// <summary>1 when selected, 0 otherwise.</summary>
            public float Target { get; set; }

            public void Show(InventorySlot slot)
            {
                item.enabled = !slot.IsEmpty;
                item.color = slot.Color;
                // A single item shows no number, as in most quick bars.
                quantity.text = slot.Quantity > 1 ? slot.Quantity.ToString() : string.Empty;
            }

            public void Animate(float step)
            {
                if (Mathf.Approximately(current, Target))
                {
                    return;
                }

                current = Mathf.MoveTowards(current, Target, step);
                Apply();
            }

            private void Apply()
            {
                // Ease out, as on the menu planks.
                var eased = 1f - (1f - current) * (1f - current);
                face.anchoredPosition = Vector2.up * (SelectedRise * eased);
                faceGroup.alpha = Mathf.Lerp(RestAlpha, 1f, eased);
                plank.Highlight = eased;
            }

            private static Image CreateSquare(string squareName, RectTransform parent, Vector2 size)
            {
                var squareObject = new GameObject(squareName, typeof(RectTransform), typeof(Image));
                var rect = (RectTransform)squareObject.transform;
                rect.SetParent(parent, false);
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.55f);
                rect.sizeDelta = size;
                var square = squareObject.GetComponent<Image>();
                square.raycastTarget = false;
                return square;
            }

            private static void Stretch(RectTransform rect)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }
    }
}
