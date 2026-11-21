using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// One slot of the inventory screen: its item and quantity, lit while it has the focus.
    /// It hands its presses and its focus to the screen. The mouse over it selects it, so
    /// the mouse and the gamepad share one focus.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    internal sealed class InventorySlotView : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        [SerializeField]
        private CutCornerGraphic background;

        [SerializeField]
        private ItemIcon item;

        [SerializeField]
        private Text quantity;

        [Tooltip("How much lighter the slot is while it has the focus.")]
        [SerializeField, Min(1f)]
        private float focusBrightness = 1.45f;

        private InventoryScreen owner;
        private int index;

        public string QuantityText => quantity.text;

        public void Initialize(InventoryScreen screen, int slotIndex)
        {
            owner = screen;
            index = slotIndex;
        }

        public void Show(InventorySlot slot)
        {
            item.Show(slot.Item);
            quantity.text = StackLabel.For(slot.Quantity);
        }

        public void SetHovered(bool hovered)
        {
            background.Brightness = hovered ? focusBrightness : 1f;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            owner.PressSlot(index, eventData.button, eventData.position);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            owner.Focus(index, true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            owner.Focus(index, false);
        }

        /// <summary>A on a gamepad: the left click.</summary>
        public void OnSubmit(BaseEventData eventData)
        {
            owner.PressSlot(index, PointerEventData.InputButton.Left, owner.ScreenPointOf(index));
        }
    }
}
