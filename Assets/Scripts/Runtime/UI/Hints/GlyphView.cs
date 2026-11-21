using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Draws a control (<see cref="Glyph"/>) with the parts its prefab has: the control's
    /// name in the text, its drawn shape (a mouse, a PlayStation shape, the View icon) or
    /// the PlayStation triangle. A part the prefab leaves out is not drawn.
    /// </summary>
    internal sealed class GlyphView : MonoBehaviour
    {
        [SerializeField]
        private Text text;

        [SerializeField]
        private GlyphShapeGraphic shape;

        [SerializeField]
        private TriangleGlyphGraphic triangle;

        /// <summary>What the text reads, empty when the control is drawn.</summary>
        public string Label => text != null ? text.text : string.Empty;

        public void Show(Glyph glyph)
        {
            if (text != null)
            {
                text.text = glyph.Text ?? string.Empty;
            }

            if (triangle != null)
            {
                triangle.enabled = glyph.Kind == GlyphKind.Face && glyph.Face == FaceShape.Triangle;
            }

            if (shape != null)
            {
                var drawn = GlyphShapeGraphic.Of(glyph);
                shape.enabled = drawn.HasValue;
                if (drawn.HasValue)
                {
                    shape.Drawn = drawn.Value;
                }
            }
        }
    }
}
