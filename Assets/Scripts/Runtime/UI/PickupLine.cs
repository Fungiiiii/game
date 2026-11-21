using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>One line of the <see cref="PickupFeed"/>: the item, then the amount and its name in the current language.</summary>
    internal sealed class PickupLine : MonoBehaviour
    {
        [SerializeField]
        private ItemIcon icon;

        [SerializeField]
        private Text text;

        [Tooltip("Fades the whole line out.")]
        [SerializeField]
        private CanvasGroup group;

        private LocalizedString itemName;
        private string shownName = string.Empty;
        private float age;

        public ItemDefinition Item { get; private set; }

        /// <summary>How many of the item were collected since the line appeared.</summary>
        public int Count { get; private set; }

        public string Text => text.text;

        /// <summary>Counts <paramref name="quantity"/> more of the item, and shows the line fully again.</summary>
        public void Add(ItemDefinition item, int quantity)
        {
            if (Item == null)
            {
                Item = item;
                icon.Show(item);
                if (!string.IsNullOrEmpty(item.NameKey))
                {
                    itemName = new LocalizedString(UiText.Table, item.NameKey);
                    itemName.StringChanged += ShowName;
                }
            }

            Count += quantity;
            age = 0f;
            group.alpha = 1f;
            Refresh();
        }

        /// <summary>Ages the line; false once it has faded out.</summary>
        public bool Age(float deltaTime, float shownSeconds, float fadeSeconds)
        {
            age += deltaTime;
            group.alpha = 1f - Mathf.Clamp01((age - shownSeconds) / fadeSeconds);
            return age < shownSeconds + fadeSeconds;
        }

        private void ShowName(string localized)
        {
            shownName = localized;
            Refresh();
        }

        private void Refresh()
        {
            text.text = string.IsNullOrEmpty(shownName) ? $"+{Count}" : $"+{Count} {shownName}";
        }

        private void OnDestroy()
        {
            if (itemName != null)
            {
                itemName.StringChanged -= ShowName;
            }
        }
    }
}
