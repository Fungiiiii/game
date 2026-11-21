using UnityEngine;
using UnityEngine.UI;

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
        /// <summary>Under the quick bar (50), the inventory (100) and the pause menu (200).</summary>
        private const int CanvasSortingOrder = 40;

        /// <summary>44 px on a 1080p screen, the canvas' reference height.</summary>
        private const float KeySize = 44f;
        private const float LetterShare = 0.62f;
        private const float TriangleShare = 0.42f;
        private const float ShapeShare = 0.7f;

        private const float PopStartScale = 0.85f;
        private const float PopSeconds = 0.1f;

        private static readonly Color32 Cream = new Color32(0xF2, 0xE6, 0xCC, 0xFF);
        private static readonly Color32 Shadow = new Color32(0x2B, 0x27, 0x23, 0xFF);

        private GameObject canvasObject;
        private RectTransform key;
        private Text letter;
        private TriangleGlyphGraphic triangle;
        private GlyphShapeGraphic shape;
        private Glyph shown;
        private float popElapsed = PopSeconds;

        public bool IsVisible => canvasObject != null && canvasObject.activeSelf;

        /// <summary>What the key reads, empty while hidden or when the control is drawn.</summary>
        public string Label { get; private set; } = string.Empty;

        /// <summary>The control shown, while the key is visible.</summary>
        public Glyph Shown => shown;

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

            var shapeObject = new GameObject("Shape", typeof(RectTransform), typeof(CanvasRenderer));
            var shapeRect = (RectTransform)shapeObject.transform;
            shapeRect.SetParent(key, false);
            shapeRect.sizeDelta = Vector2.one * (KeySize * ShapeShare);
            shape = shapeObject.AddComponent<GlyphShapeGraphic>();
            shape.raycastTarget = false;

            Hide();
        }

        /// <summary>Shows the key at a point of the screen, in pixels. It pops in if it was hidden.</summary>
        public void Show(Vector2 screenPoint, Glyph glyph)
        {
            if (!IsVisible)
            {
                canvasObject.SetActive(true);
                popElapsed = 0f;
            }

            key.position = screenPoint;
            if (!glyph.Equals(shown))
            {
                Draw(glyph);
            }

            ApplyPop();
        }

        private void Draw(Glyph glyph)
        {
            shown = glyph;
            Label = glyph.Text ?? string.Empty;
            letter.text = Label;
            triangle.enabled = glyph.Kind == GlyphKind.Face && glyph.Face == FaceShape.Triangle;

            var drawn = ShapeOf(glyph);
            shape.enabled = drawn.HasValue;
            if (drawn.HasValue)
            {
                shape.Drawn = drawn.Value;
            }
        }

        /// <summary>The drawn shape of a control without a name; null when it has one, or is the triangle.</summary>
        private static GlyphShapeGraphic.Shape? ShapeOf(Glyph glyph)
        {
            switch (glyph.Kind)
            {
                case GlyphKind.MouseLeft:
                    return GlyphShapeGraphic.Shape.MouseLeft;
                case GlyphKind.MouseRight:
                    return GlyphShapeGraphic.Shape.MouseRight;
                case GlyphKind.MouseWheel:
                    return GlyphShapeGraphic.Shape.MouseWheel;
                case GlyphKind.View:
                    return GlyphShapeGraphic.Shape.View;
                case GlyphKind.Face when glyph.Face == FaceShape.Cross:
                    return GlyphShapeGraphic.Shape.Cross;
                case GlyphKind.Face when glyph.Face == FaceShape.Circle:
                    return GlyphShapeGraphic.Shape.Circle;
                case GlyphKind.Face when glyph.Face == FaceShape.Square:
                    return GlyphShapeGraphic.Shape.Square;
                default:
                    return null;
            }
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
