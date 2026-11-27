using Fungiiiii.Inventory;
using Fungiiiii.Items;
using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The name of the item in hand, just above the quick bar, as in Minecraft: it shows
    /// for a moment when another item comes into hand, then fades out. Empty hands show
    /// nothing. It sits in the quick bar, so it hides with it.
    /// </summary>
    internal sealed class HeldItemName : MonoBehaviour
    {
        [SerializeField]
        private LocalizedLabel label;

        [Tooltip("Fades the name out.")]
        [SerializeField]
        private CanvasGroup group;

        [Tooltip("Seconds the name stays fully shown once an item comes into hand.")]
        [SerializeField, Min(0f)]
        private float shownSeconds = 2f;

        [SerializeField, Min(0.01f)]
        private float fadeSeconds = 0.4f;

        private IItemSelection selection;
        private ItemDefinition inHand;
        private float age;

        /// <summary>Whether the name can be seen, even fading.</summary>
        public bool IsVisible => group.alpha > 0f;

        /// <summary>The key of the name shown, in the UI string table.</summary>
        public string NameKey => label.EntryKey;

        private void Awake()
        {
            group.alpha = 0f;
        }

        /// <summary>Shows the name of what <paramref name="itemSelection"/> puts in hand from now on.</summary>
        public void Bind(IItemSelection itemSelection)
        {
            selection = itemSelection;
            inHand = ItemOf(selection.SelectedSlot);
            selection.SelectedSlotChanged += Refresh;
        }

        private void Refresh()
        {
            var item = ItemOf(selection.SelectedSlot);
            if (item == inHand)
            {
                // The same item, more or fewer of it: nothing new in hand.
                return;
            }

            inHand = item;
            if (item == null || string.IsNullOrEmpty(item.NameKey))
            {
                group.alpha = 0f;
                return;
            }

            label.Show(item.NameKey);
            age = 0f;
            group.alpha = 1f;
        }

        private void Update()
        {
            if (group.alpha <= 0f)
            {
                return;
            }

            // Unscaled: the name fades the same under the pause.
            age += Time.unscaledDeltaTime;
            group.alpha = 1f - Mathf.Clamp01((age - shownSeconds) / fadeSeconds);
        }

        private void OnDestroy()
        {
            if (selection != null)
            {
                selection.SelectedSlotChanged -= Refresh;
            }
        }

        private static ItemDefinition ItemOf(InventorySlot slot)
        {
            return slot.IsEmpty ? null : slot.Item;
        }
    }
}
