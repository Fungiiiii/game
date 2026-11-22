using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// One ingredient in a recipe's details: its icon and name, how many the player has out
    /// of how many are needed, and, while some are missing, where the item comes from.
    /// </summary>
    internal sealed class IngredientRow : MonoBehaviour
    {
        [SerializeField]
        private ItemIcon icon;

        [SerializeField]
        private LocalizedLabel itemName;

        [Tooltip("Optional: where the item comes from, shown while some are missing.")]
        [SerializeField]
        private LocalizedLabel source;

        [Tooltip("Off for the recipes tracked during play: the line stays short.")]
        [SerializeField]
        private bool showSource = true;

        [Tooltip("\"owned/needed\"")]
        [SerializeField]
        private Text count;

        [SerializeField]
        private Color enoughColor = new Color32(0xF2, 0xE6, 0xCC, 0xFF);

        [SerializeField]
        private Color missingColor = new Color32(0xF0, 0x84, 0x6B, 0xFF);

        public string CountText => count.text;

        public bool IsMissing { get; private set; }

        public bool ShowsSource => source != null && source.gameObject.activeSelf;

        public void Show(ItemDefinition item, int owned, int needed)
        {
            if (icon.Item != item)
            {
                icon.Show(item);
                itemName.Show(item.NameKey);
            }

            IsMissing = owned < needed;
            count.text = $"{owned}/{needed}";
            count.color = IsMissing ? missingColor : enoughColor;

            if (source == null)
            {
                return;
            }

            var withSource = showSource && IsMissing && !string.IsNullOrEmpty(item.SourceKey);
            source.gameObject.SetActive(withSource);
            if (withSource)
            {
                source.Show(item.SourceKey);
            }
        }
    }
}
