using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// A small wooden plate with an item's name, just below and right of the inventory
    /// slot in focus: under the mouse, or selected with the gamepad. Under the name, what the
    /// item is for and where to find it, when the item says so. Its owner tells it what to
    /// show and where.
    /// </summary>
    internal sealed class ItemNamePlate : MonoBehaviour
    {
        [Tooltip("The plate; it sizes itself to the name.")]
        [SerializeField]
        private RectTransform plate;

        [SerializeField]
        private LocalizedLabel label;

        [Tooltip("Under the name: what the item is for. Hidden for an item without description.")]
        [SerializeField]
        private LocalizedLabel description;

        [Tooltip("Under the description: where to find the item. Hidden for an item without source.")]
        [SerializeField]
        private LocalizedLabel source;

        [Tooltip("Where the plate's top left corner goes from the slot's centre, in canvas units.")]
        [SerializeField]
        private Vector2 offset = new Vector2(28f, -28f);

        private Canvas canvas;

        public bool IsVisible => plate.gameObject.activeSelf;

        /// <summary>The key of the name shown, in the UI string table; empty while hidden.</summary>
        public string NameKey => IsVisible ? label.EntryKey : string.Empty;

        /// <summary>The key of the description shown; empty when none is.</summary>
        public string DescriptionKey => IsVisible && description.gameObject.activeSelf ? description.EntryKey : string.Empty;

        /// <summary>The key of where the item is found, shown; empty when none is.</summary>
        public string SourceKey => IsVisible && source.gameObject.activeSelf ? source.EntryKey : string.Empty;

        private void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            Hide();
        }

        /// <summary>Shows the name of <paramref name="item"/> by the slot whose centre is at <paramref name="slotCentre"/>, in pixels.</summary>
        public void Show(ItemDefinition item, Vector2 slotCentre)
        {
            if (item == null || string.IsNullOrEmpty(item.NameKey))
            {
                Hide();
                return;
            }

            plate.gameObject.SetActive(true);
            label.Show(item.NameKey);
            ShowLine(description, item.DescriptionKey);
            ShowLine(source, item.SourceKey);
            var scale = canvas != null ? canvas.scaleFactor : 1f;
            plate.position = slotCentre + offset * scale;
            transform.SetAsLastSibling();
        }

        public void Hide()
        {
            plate.gameObject.SetActive(false);
        }

        private static void ShowLine(LocalizedLabel line, string entryKey)
        {
            var shown = !string.IsNullOrEmpty(entryKey);
            line.gameObject.SetActive(shown);
            if (shown)
            {
                line.Show(entryKey);
            }
        }
    }
}
