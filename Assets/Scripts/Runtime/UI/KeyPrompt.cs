using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The key to press, shown over what the player aims at, as on the Ramassage artboard:
    /// a square wooden key cap with a cyan rim and the key's name in cream. It pops in when
    /// it appears. Where it goes and what it reads is told by its owner.
    /// </summary>
    internal sealed class KeyPrompt : MonoBehaviour
    {
        /// <summary>Under the quick bar (50), the inventory (100) and the pause menu (200).</summary>
        private const int CanvasSortingOrder = 40;

        /// <summary>44 px on a 1080p screen, the canvas' reference height.</summary>
        private const float KeySize = 44f;
        private const float LetterShare = 0.62f;
        private const float TriangleShare = 0.42f;

        private const float PopStartScale = 0.85f;
        private const float PopSeconds = 0.1f;

        /// <summary>The label that is drawn as a triangle instead of a letter.</summary>
        public const string TriangleLabel = "△";

        private static readonly Color32 Cream = new Color32(0xF2, 0xE6, 0xCC, 0xFF);
        private static readonly Color32 Shadow = new Color32(0x2B, 0x27, 0x23, 0xFF);

        private GameObject canvasObject;
        private RectTransform key;
        private Text letter;
        private TriangleGlyphGraphic triangle;
        private float popElapsed = PopSeconds;

        public bool IsVisible => canvasObject != null && canvasObject.activeSelf;

        /// <summary>What the key reads, empty while hidden.</summary>
        public string Label { get; private set; } = string.Empty;

        /// <summary>Where it stands on the screen, in pixels.</summary>
        public Vector2 ScreenPoint => key != null ? (Vector2)key.position : Vector2.zero;

        public void Initialize(Font font)
        {
            var canvas = WoodMenu.CreateCanvas("Key Prompt Canvas", CanvasSortingOrder);
            canvasObject = canvas.gameObject;
            canvasObject.transform.SetParent(transform, false);
            // Nothing on it is clicked.
            Destroy(canvasObject.GetComponent<GraphicRaycaster>());

            var keyObject = new GameObject("Key", typeof(RectTransform), typeof(CanvasRenderer));
            key = (RectTransform)keyObject.transform;
            key.SetParent(canvas.transform, false);
            key.anchorMin = key.anchorMax = Vector2.zero;
            key.sizeDelta = new Vector2(KeySize, KeySize);
            keyObject.AddComponent<KeyCapGraphic>().raycastTarget = false;

            var letterObject = new GameObject("Letter", typeof(RectTransform), typeof(Text));
            var letterRect = (RectTransform)letterObject.transform;
            letterRect.SetParent(key, false);
            letterRect.anchorMin = Vector2.zero;
            letterRect.anchorMax = Vector2.one;
            letterRect.offsetMin = letterRect.offsetMax = Vector2.zero;
            letter = letterObject.GetComponent<Text>();
            letter.font = font;
            letter.fontSize = Mathf.RoundToInt(KeySize * LetterShare);
            letter.color = Cream;
            letter.alignment = TextAnchor.MiddleCenter;
            letter.horizontalOverflow = HorizontalWrapMode.Overflow;
            letter.raycastTarget = false;
            var shadow = letterObject.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = Shadow;
            shadow.effectDistance = new Vector2(0f, -1f);

            var triangleObject = new GameObject("Triangle", typeof(RectTransform), typeof(CanvasRenderer));
            var triangleRect = (RectTransform)triangleObject.transform;
            triangleRect.SetParent(key, false);
            triangleRect.sizeDelta = Vector2.one * (KeySize * TriangleShare);
            triangle = triangleObject.AddComponent<TriangleGlyphGraphic>();
            triangle.color = Cream;
            triangle.raycastTarget = false;

            Hide();
        }

        /// <summary>Shows the key at a point of the screen, in pixels. It pops in if it was hidden.</summary>
        public void Show(Vector2 screenPoint, string label)
        {
            if (!IsVisible)
            {
                canvasObject.SetActive(true);
                popElapsed = 0f;
            }

            key.position = screenPoint;
            if (label != Label)
            {
                Label = label;
                var isTriangle = label == TriangleLabel;
                letter.text = isTriangle ? string.Empty : label;
                triangle.enabled = isTriangle;
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

        /// <summary>Scale of the key: below 1 while it pops in.</summary>
        public float Scale => key != null ? key.localScale.x : 0f;

        public void Hide()
        {
            canvasObject.SetActive(false);
            Label = string.Empty;
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
