using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// What is shown over what the player aims at, as on the Ramassage artboard: the key to
    /// press, a square wooden key cap with a cyan rim and, in cream, the control as the key
    /// hints draw it (<see cref="Glyph"/>): a name, a PlayStation shape, a mouse. When the
    /// key can also be held, a ring around it fills while it is held. Where nothing can be
    /// done, a short text takes the key's place instead (« Il faut un Spongeus »). It pops
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

        [Tooltip("Around the key: fills while a key that can be held is held.")]
        [SerializeField]
        private HoldRingGraphic holdRing;

        [Tooltip("The short text shown instead of the key.")]
        [SerializeField]
        private LocalizedLabel text;

        private Glyph shown;
        private float popElapsed = PopSeconds;

        public bool IsVisible => IsKeyVisible || IsTextVisible;

        /// <summary>Whether the key to press is shown.</summary>
        public bool IsKeyVisible => key.gameObject.activeSelf;

        /// <summary>Whether the short text is shown instead of the key.</summary>
        public bool IsTextVisible => text.gameObject.activeSelf;

        /// <summary>What the key reads, empty while hidden or when the control is drawn.</summary>
        public string Label => IsKeyVisible ? glyph.Label : string.Empty;

        /// <summary>The entry of the UI string table shown instead of the key; empty when the key is shown.</summary>
        public string TextKey => IsTextVisible ? text.EntryKey : string.Empty;

        /// <summary>The control shown, while the key is visible.</summary>
        public Glyph Shown => shown;

        /// <summary>Whether the ring around the key is shown: the key can be held.</summary>
        public bool IsHoldRingVisible => IsKeyVisible && holdRing.gameObject.activeSelf;

        /// <summary>How much of the ring is filled, from 0 to 1.</summary>
        public float HoldProgress => holdRing.Progress;

        /// <summary>Where it stands on the screen, in pixels.</summary>
        public Vector2 ScreenPoint => Root.position;

        /// <summary>Scale of what is shown: below 1 while it pops in.</summary>
        public float Scale => Root.localScale.x;

        private RectTransform Root => IsTextVisible ? (RectTransform)text.transform : key;

        private void Awake()
        {
            Hide();
        }

        /// <summary>Shows the key at a point of the screen, in pixels. It pops in if nothing was shown.</summary>
        public void Show(Vector2 screenPoint, Glyph control)
        {
            var wasVisible = IsVisible;
            text.gameObject.SetActive(false);
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
            text.gameObject.SetActive(true);
            if (!wasVisible)
            {
                popElapsed = 0f;
            }

            text.transform.position = screenPoint;
            if (text.EntryKey != entryKey)
            {
                text.Show(entryKey);
            }

            ApplyPop();
        }

        /// <summary>Shows the ring around the key when it can be held, filled to <paramref name="progress"/>.</summary>
        public void SetHold(bool canHold, float progress)
        {
            if (holdRing.gameObject.activeSelf != canHold)
            {
                holdRing.gameObject.SetActive(canHold);
            }

            holdRing.Progress = canHold ? progress : 0f;
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
            text.gameObject.SetActive(false);
            SetHold(false, 0f);
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
