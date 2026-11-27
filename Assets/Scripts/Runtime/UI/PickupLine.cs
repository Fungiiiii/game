using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// One line of the <see cref="PickupFeed"/>: the item, then the amount and its name in the
    /// current language, worded by the <c>pickup.line</c> entry so that each language orders them.
    /// </summary>
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
        private LocalizedString line;
        private bool isLineShown;
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
                    line = new LocalizedString(UiText.Table, UiText.PickupLine);
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

        private void ShowLine(string localized)
        {
            text.text = localized;
        }

        private void Refresh()
        {
            if (line == null || string.IsNullOrEmpty(shownName))
            {
                text.text = $"+{Count}"; // i18n-exempt: a number, no words
                return;
            }

            line.Arguments = new object[] { Count, shownName };
            if (isLineShown)
            {
                line.RefreshString();
            }
            else
            {
                isLineShown = true;
                line.StringChanged += ShowLine;
            }
        }

        private void OnDestroy()
        {
            if (itemName != null)
            {
                itemName.StringChanged -= ShowName;
            }

            if (isLineShown)
            {
                line.StringChanged -= ShowLine;
            }
        }
    }
}
