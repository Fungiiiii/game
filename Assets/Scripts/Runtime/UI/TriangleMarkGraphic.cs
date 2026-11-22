using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// A flat triangle pointing down, in the graphic's colour: in cyan, laid over the top
    /// edge of a key, it says a short press does something; in the shadow brown, under a
    /// wooden panel, it points at what the panel is about.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    internal sealed class TriangleMarkGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            var rect = GetPixelAdjustedRect();
            Polygon.Add(
                vertexHelper,
                color,
                new Vector2(rect.xMin, rect.yMax),
                new Vector2(rect.xMax, rect.yMax),
                new Vector2(rect.center.x, rect.yMin));
        }
    }
}
