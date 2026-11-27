using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>One line of the key hints: the control, x2 for a double click, then what it does; greyed when it cannot be done now.</summary>
    internal sealed class HintRow : MonoBehaviour
    {
        /// <summary>How visible a hint that cannot be done now stays: dimmed, still readable.</summary>
        private const float UnavailableAlpha = 0.4f;

        [Tooltip("Where the control is drawn, before the x2.")]
        [SerializeField]
        private RectTransform control;

        [Tooltip("The x2 of a double click.")]
        [SerializeField]
        private GameObject twice;

        [SerializeField]
        private LocalizedLabel label;

        public bool IsAvailable { get; private set; } = true;

        public void Show(GlyphView glyphPrefab, Glyph glyph, bool doubleClick, string labelKey, bool available)
        {
            var view = Instantiate(glyphPrefab, control);
            view.transform.SetAsFirstSibling();
            view.Show(glyph);
            twice.SetActive(doubleClick);
            label.Show(labelKey);
            IsAvailable = available;
            if (!available)
            {
                // Rows are made anew for every change of hints: the group is added only to the greyed ones.
                gameObject.AddComponent<CanvasGroup>().alpha = UnavailableAlpha;
            }
        }
    }
}
