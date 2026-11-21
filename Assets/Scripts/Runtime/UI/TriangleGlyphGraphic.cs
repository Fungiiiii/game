using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The triangle of a PlayStation pad, drawn as a hollow triangle: the menu font has no
    /// such glyph.
    /// </summary>
    internal sealed class TriangleGlyphGraphic : MaskableGraphic
    {
        [SerializeField, Range(0.02f, 0.5f), Tooltip("Thickness of the stroke, as a share of the height.")]
        private float stroke = 0.16f;

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            var rect = GetPixelAdjustedRect();
            var outerTop = new Vector2(rect.center.x, rect.yMax);
            var outerLeft = new Vector2(rect.xMin, rect.yMin);
            var outerRight = new Vector2(rect.xMax, rect.yMin);

            // The inner triangle has the same centre, shrunk by the stroke.
            var centre = (outerTop + outerLeft + outerRight) / 3f;
            var shrink = 1f - stroke * 3f;
            var innerTop = centre + (outerTop - centre) * shrink;
            var innerLeft = centre + (outerLeft - centre) * shrink;
            var innerRight = centre + (outerRight - centre) * shrink;

            Polygon.Add(vertexHelper, color, outerLeft, outerRight, innerRight, innerLeft);
            Polygon.Add(vertexHelper, color, outerRight, outerTop, innerTop, innerRight);
            Polygon.Add(vertexHelper, color, outerTop, outerLeft, innerLeft, innerTop);
        }
    }
}
