using UnityEngine;
using UnityEngine.Serialization;

namespace Fungiiiii.UI
{
    /// <summary>
    /// What is shown over what the player aims at, as in the wooden panels design system:
    /// the key, a square wooden key cap with, in cream, the control as the key hints draw it
    /// (<see cref="Glyph"/>): a name, a PlayStation shape, a mouse. A cyan triangle laid over
    /// its top edge says a short press does something; a border around it says the key can
    /// be held, and fills while it is held. Where nothing can be done, a short text on a
    /// small wooden panel takes the key's place instead (<c>label.needs_spongeus</c>). It pops
    /// in when it appears. Where it goes and what it shows is told by its owner.
    /// </summary>
    internal sealed class KeyPrompt : MonoBehaviour
    {
        private const float PopStartScale = 0.85f;
        private const float PopSeconds = 0.1f;

        [SerializeField]
        private RectTransform key;

        [SerializeField]
        private GlyphView glyph;

        [Tooltip("Over the top edge of the key: a short press does something.")]
        [SerializeField]
        private TriangleMarkGraphic pressMark;

        [Tooltip("Around the key: it can be held, and fills while it is held.")]
        [FormerlySerializedAs("holdRing")]
        [SerializeField]
        private HoldBorderGraphic holdBorder;

        [Tooltip("The small wooden panel of the short text, shown instead of the key.")]
        [SerializeField]
        private RectTransform textPanel;

        [Tooltip("The short text shown instead of the key.")]
        [SerializeField]
        private LocalizedLabel text;

        private Glyph shown;
        private float popElapsed = PopSeconds;

        public bool IsVisible => IsKeyVisible || IsTextVisible;

        /// <summary>Whether the key to press is shown.</summary>
        public bool IsKeyVisible => key.gameObject.activeSelf;

        /// <summary>Whether the short text is shown instead of the key.</summary>
        public bool IsTextVisible => textPanel.gameObject.activeSelf;

        /// <summary>What the key reads, empty while hidden or when the control is drawn.</summary>
        public string Label => IsKeyVisible ? glyph.Label : string.Empty;

        /// <summary>The entry of the UI string table shown instead of the key; empty when the key is shown.</summary>
        public string TextKey => IsTextVisible ? text.EntryKey : string.Empty;

        /// <summary>The control shown, while the key is visible.</summary>
        public Glyph Shown => shown;

        /// <summary>Whether the triangle over the key is shown: a short press does something.</summary>
        public bool IsPressMarkVisible => IsKeyVisible && pressMark.gameObject.activeSelf;

        /// <summary>Whether the border around the key is shown: the key can be held.</summary>
        public bool IsHoldBorderVisible => IsKeyVisible && holdBorder.gameObject.activeSelf;

        /// <summary>How much of the border is filled, from 0 to 1.</summary>
        public float HoldProgress => holdBorder.Progress;

        /// <summary>Where it stands on the screen, in pixels.</summary>
        public Vector2 ScreenPoint => Root.position;

        /// <summary>Scale of what is shown: below 1 while it pops in.</summary>
        public float Scale => Root.localScale.x;

        private RectTransform Root => IsTextVisible ? textPanel : key;

        private void Awake()
        {
            Hide();
        }

        /// <summary>Shows the key at a point of the screen, in pixels. It pops in if nothing was shown.</summary>
        public void Show(Vector2 screenPoint, Glyph control)
        {
            var wasVisible = IsVisible;
            textPanel.gameObject.SetActive(false);
            key.gameObject.SetActive(true);
            if (!wasVisible)
            {
                popElapsed = 0f;
            }

            key.position = screenPoint;
            if (!control.Equals(shown))
            {
                shown = control;
                glyph.Show(control);
            }

            ApplyPop();
        }

        /// <summary>Shows a short text instead of the key, an entry of the UI string table. It pops in if nothing was shown.</summary>
        public void ShowText(Vector2 screenPoint, string entryKey)
        {
            var wasVisible = IsVisible;
            key.gameObject.SetActive(false);
            shown = default;
            textPanel.gameObject.SetActive(true);
            if (!wasVisible)
            {
                popElapsed = 0f;
            }

            textPanel.position = screenPoint;
            if (text.EntryKey != entryKey)
            {
                text.Show(entryKey);
            }

            ApplyPop();
        }

        /// <summary>
        /// Shows the triangle when a short press does something, and the border when the key
        /// can be held, filled to <paramref name="holdProgress"/>. Both can show at once.
        /// </summary>
        public void SetMarks(bool canPress, bool canHold, float holdProgress)
        {
            if (pressMark.gameObject.activeSelf != canPress)
            {
                pressMark.gameObject.SetActive(canPress);
            }

            if (holdBorder.gameObject.activeSelf != canHold)
            {
                holdBorder.gameObject.SetActive(canHold);
            }

            holdBorder.Progress = canHold ? holdProgress : 0f;
        }

        /// <summary>Plays the pop again, for a key that moves to another target.</summary>
        public void PopIn()
        {
            popElapsed = 0f;
            if (IsVisible)
            {
                ApplyPop();
            }
        }

        public void Hide()
        {
            key.gameObject.SetActive(false);
            textPanel.gameObject.SetActive(false);
            SetMarks(false, false, 0f);
            shown = default;
        }

        private void Update()
        {
            if (!IsVisible || popElapsed >= PopSeconds)
            {
                return;
            }

            // Unscaled: the pop plays the same at any game speed.
            popElapsed += Time.unscaledDeltaTime;
            ApplyPop();
        }

        private void ApplyPop()
        {
            var progress = Mathf.Clamp01(popElapsed / PopSeconds);
            Root.localScale = Vector3.one * Mathf.Lerp(PopStartScale, 1f, progress);
        }
    }
}
