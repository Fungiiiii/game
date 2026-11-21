using UnityEngine;
using UnityEngine.EventSystems;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Everything out of the inventory's panel: a press there drops what the mouse holds.
    /// It sits under the panel, which catches the presses on itself.
    /// </summary>
    internal sealed class InventoryDropZone : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private InventoryScreen screen;

        public void OnPointerDown(PointerEventData eventData)
        {
            screen.PressOutside(eventData.button);
        }
    }
}
