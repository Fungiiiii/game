using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// One ingredient in a recipe's details: its icon and name, how many the player has out
    /// of how many are needed, and, while some are missing, where the item comes from. As
    /// in the lines of a wooden panel, the count is moss green with a tick when there are
    /// enough, coral when some are missing.
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

        [Tooltip("Optional: shown when there are enough.")]
        [SerializeField]
        private GameObject tick;

        [SerializeField]
        private Color enoughColor = new Color32(0x9C, 0xB9, 0x55, 0xFF);

        [SerializeField]
        private Color missingColor = new Color32(0xF0, 0x8A, 0x74, 0xFF);

        public string CountText => count.text;

        /// <summary>The name shown, an entry of the UI string table.</summary>
        public string TextKey => itemName.EntryKey;

        /// <summary>The line under the name, an entry of the UI string table; empty when hidden.</summary>
        public string SourceKey => ShowsSource ? source.EntryKey : string.Empty;

        public bool IsMissing { get; private set; }

        public bool ShowsSource => source != null && source.gameObject.activeSelf;

        /// <summary>
        /// A line that is no ingredient, something to do: the icon of what it is done with,
        /// what to do and a hint under it, without a count.
        /// </summary>
        public void ShowGoal(ItemDefinition doneWith, string textKey, string hintKey)
        {
            icon.Show(doneWith);
            itemName.Show(textKey);
            IsMissing = true;
            count.text = string.Empty;
            if (tick != null)
            {
                tick.SetActive(false);
            }

            if (source != null)
            {
                source.gameObject.SetActive(true);
                source.Show(hintKey);
            }
        }

        public void Show(ItemDefinition item, int owned, int needed)
        {
            if (icon.Item != item)
            {
                icon.Show(item);
                itemName.Show(item.NameKey);
            }

            IsMissing = owned < needed;
            count.text = $"{owned}/{needed}"; // i18n-exempt: two numbers, no words
            count.color = IsMissing ? missingColor : enoughColor;
            if (tick != null && tick.activeSelf == IsMissing)
            {
                tick.SetActive(!IsMissing);
            }

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
