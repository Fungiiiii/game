using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>One line of the key hints: the control, ×2 for a double click, then what it does.</summary>
    internal sealed class HintRow : MonoBehaviour
    {
        [Tooltip("Where the control is drawn, before the ×2.")]
        [SerializeField]
        private RectTransform control;

        [Tooltip("The ×2 of a double click.")]
        [SerializeField]
        private GameObject twice;

        [SerializeField]
        private LocalizedLabel label;

        public void Show(GlyphView glyphPrefab, Glyph glyph, bool doubleClick, string labelKey)
        {
            var view = Instantiate(glyphPrefab, control);
            view.transform.SetAsFirstSibling();
            view.Show(glyph);
            twice.SetActive(doubleClick);
            label.Show(labelKey);
        }
    }
}
