using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The key to press, shown over what the player aims at, as on the Ramassage artboard:
    /// a square wooden key cap with a cyan rim and, in cream, the control as the key hints
    /// draw it (<see cref="Glyph"/>): a name, a PlayStation shape, a mouse. It pops in when
    /// it appears. Where it goes and what it shows is told by its owner.
    /// </summary>
    internal sealed class KeyPrompt : MonoBehaviour
    {
        private const float PopStartScale = 0.85f;
        private const float PopSeconds = 0.1f;

        [SerializeField]
        private RectTransform key;

        [SerializeField]
        private GlyphView glyph;

        private Glyph shown;
        private float popElapsed = PopSeconds;

        public bool IsVisible => key.gameObject.activeSelf;

        /// <summary>What the key reads, empty while hidden or when the control is drawn.</summary>
        public string Label => IsVisible ? glyph.Label : string.Empty;

        /// <summary>The control shown, while the key is visible.</summary>
        public Glyph Shown => shown;

        /// <summary>Where it stands on the screen, in pixels.</summary>
        public Vector2 ScreenPoint => key.position;

        /// <summary>Scale of the key: below 1 while it pops in.</summary>
        public float Scale => key.localScale.x;

        private void Awake()
        {
            Hide();
        }

        /// <summary>Shows the key at a point of the screen, in pixels. It pops in if it was hidden.</summary>
        public void Show(Vector2 screenPoint, Glyph control)
        {
            if (!IsVisible)
            {
                key.gameObject.SetActive(true);
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
            key.localScale = Vector3.one * Mathf.Lerp(PopStartScale, 1f, progress);
        }
    }
}
