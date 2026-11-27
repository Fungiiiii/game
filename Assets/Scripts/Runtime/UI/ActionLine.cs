using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// One control at the foot of a wooden panel: the key, then what it does. A key to
    /// press has a cyan triangle over its top edge; a key to hold has a border around it,
    /// filled as far as it is held, and the word "Hold" before what it does.
    /// </summary>
    internal sealed class ActionLine : MonoBehaviour
    {
        [SerializeField]
        private GlyphView glyph;

        [Tooltip("Over the top edge of the key, for a short press.")]
        [SerializeField]
        private TriangleMarkGraphic pressMark;

        [Tooltip("Around the key, for a hold.")]
        [SerializeField]
        private HoldBorderGraphic holdBorder;

        [Tooltip("\"Hold\", before what holding does.")]
        [SerializeField]
        private LocalizedLabel holdWord;

        [Tooltip("What the key does: \"Collect the thread\", \"Drop\".")]
        [SerializeField]
        private LocalizedLabel action;

        private Glyph shown;

        /// <summary>The entry shown for what the key does.</summary>
        public string ActionKey => action.EntryKey;

        /// <summary>Whether the key is to hold rather than to press.</summary>
        public bool IsHold => holdBorder.gameObject.activeSelf;

        public float HoldProgress => holdBorder.Progress;

        /// <summary>Shows <paramref name="control"/> doing <paramref name="actionKey"/>, held to <paramref name="holdProgress"/> for a key to hold.</summary>
        public void Show(Glyph control, bool hold, float holdProgress, string actionKey)
        {
            if (!control.Equals(shown))
            {
                shown = control;
                glyph.Show(control);
            }

            if (pressMark.gameObject.activeSelf == hold)
            {
                pressMark.gameObject.SetActive(!hold);
            }

            if (holdBorder.gameObject.activeSelf != hold)
            {
                holdBorder.gameObject.SetActive(hold);
            }

            if (holdWord.gameObject.activeSelf != hold)
            {
                holdWord.gameObject.SetActive(hold);
            }

            holdBorder.Progress = hold ? holdProgress : 0f;
            if (action.EntryKey != actionKey)
            {
                action.Show(actionKey);
            }
        }
    }
}
