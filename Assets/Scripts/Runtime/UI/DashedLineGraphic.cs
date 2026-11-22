using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// A dashed line across the graphic, as tall as it, in the graphic's colour: what parts
    /// the heading, the body and the controls of a wooden panel.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    internal sealed class DashedLineGraphic : MaskableGraphic
    {
        [SerializeField, Min(1f), Tooltip("Length of a dash, in canvas units.")]
        private float dash = 6f;

        [SerializeField, Min(0f), Tooltip("Space between two dashes, in canvas units.")]
        private float gap = 4f;

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            var rect = GetPixelAdjustedRect();
            for (var start = rect.xMin; start < rect.xMax; start += dash + gap)
            {
                var end = Mathf.Min(start + dash, rect.xMax);
                Polygon.Add(
                    vertexHelper,
                    color,
                    new Vector2(start, rect.yMax),
                    new Vector2(end, rect.yMax),
                    new Vector2(end, rect.yMin),
                    new Vector2(start, rect.yMin));
            }
        }
    }
}
