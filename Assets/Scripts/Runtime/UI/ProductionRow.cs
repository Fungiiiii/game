using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// What an object produces by itself, in its wooden panel, as in the wooden panels design
    /// system: on the left the key that collects it, greyed while nothing waits and lit with
    /// its triangle once something does; the item, and how many wait out of the stock; under
    /// them a bar filling up to the next one, with the seconds left. With a full stock,
    /// production waits: no bar, but a line saying to collect it.
    /// </summary>
    internal sealed class ProductionRow : MonoBehaviour
    {
        [Tooltip("The key that collects what waits.")]
        [SerializeField]
        private GlyphView key;

        [Tooltip("Greys the key while nothing waits.")]
        [SerializeField]
        private CanvasGroup keyGroup;

        [Tooltip("Over the top edge of the key, for a short press: shown once something waits.")]
        [SerializeField]
        private TriangleMarkGraphic pressMark;

        [SerializeField]
        private ItemIcon icon;

        [SerializeField]
        private LocalizedLabel itemName;

        [Tooltip("\"waiting/stock\"")]
        [SerializeField]
        private Text count;

        [Tooltip("The bar to the next one: hidden with a full stock.")]
        [SerializeField]
        private GameObject bar;

        [Tooltip("The filled part of the bar; its right anchor follows the progress.")]
        [SerializeField]
        private RectTransform fill;

        [Tooltip("\"Next in 12 s\", or that the stock is full.")]
        [SerializeField]
        private LocalizedLabel next;

        [SerializeField]
        private Text nextText;

        [Tooltip("How opaque the key is while nothing waits.")]
        [SerializeField, Range(0f, 1f)]
        private float greyedAlpha = 0.35f;

        [SerializeField]
        private Color emptyColor = new Color32(0xCD, 0xB8, 0x96, 0xFF);

        [SerializeField]
        private Color waitingColor = new Color32(0xE8, 0xCF, 0x7E, 0xFF);

        [SerializeField]
        private Color fullColor = new Color32(0x9C, 0xB9, 0x55, 0xFF);

        private Glyph shownKey;
        private int shownSeconds = -1;

        /// <summary>Whether the key is lit: something waits to be collected.</summary>
        public bool CanCollect { get; private set; }

        public string CountText => count.text;

        /// <summary>Whether the bar to the next one shows.</summary>
        public bool ShowsBar => bar.activeSelf;

        /// <summary>How far the bar is filled, from 0 to 1.</summary>
        public float Progress => fill.anchorMax.x;

        /// <summary>The line under the item, an entry of the UI string table.</summary>
        public string NextKey => next.EntryKey;

        /// <param name="control">The key that collects, as on the device used last.</param>
        /// <param name="item">What is produced.</param>
        /// <param name="stored">How many wait.</param>
        /// <param name="stock">How many can wait at most.</param>
        /// <param name="progress">How far the next one is, from 0 to 1.</param>
        /// <param name="secondsLeft">Whole seconds until the next one.</param>
        public void Show(Glyph control, ItemDefinition item, int stored, int stock, float progress, int secondsLeft)
        {
            if (!control.Equals(shownKey))
            {
                shownKey = control;
                key.Show(control);
            }

            if (icon.Item != item)
            {
                icon.Show(item);
                itemName.Show(item.NameKey);
            }

            CanCollect = stored > 0;
            keyGroup.alpha = CanCollect ? 1f : greyedAlpha;
            if (pressMark.gameObject.activeSelf != CanCollect)
            {
                pressMark.gameObject.SetActive(CanCollect);
            }

            var full = stored >= stock;
            count.text = $"{stored}/{stock}"; // i18n-exempt: two numbers, no words
            count.color = full ? fullColor : CanCollect ? waitingColor : emptyColor;
            if (bar.activeSelf == full)
            {
                bar.SetActive(!full);
            }

            var anchor = fill.anchorMax;
            anchor.x = full ? 1f : Mathf.Clamp01(progress);
            fill.anchorMax = anchor;

            nextText.color = full ? fullColor : emptyColor;
            if (full)
            {
                shownSeconds = -1;
                if (next.EntryKey != UiText.PanelThreadFull)
                {
                    next.Show(UiText.PanelThreadFull);
                }
            }
            else if (secondsLeft != shownSeconds || next.EntryKey != UiText.PanelThreadNext)
            {
                // Only when the second changes: the text is built again each time.
                shownSeconds = secondsLeft;
                next.Show(UiText.PanelThreadNext, secondsLeft);
            }
        }
    }
}
