using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    internal static class Polygon
    {
        /// <summary>A convex polygon in one flat colour, as a fan around its first corner.</summary>
        public static void Add(VertexHelper vertexHelper, Color32 flatColor, params Vector2[] corners)
        {
            var first = vertexHelper.currentVertCount;
            foreach (var point in corners)
            {
                vertexHelper.AddVert(point, flatColor, Vector4.zero);
            }

            for (var index = 1; index < corners.Length - 1; index++)
            {
                vertexHelper.AddTriangle(first, first + index, first + index + 1);
            }
        }
    }
}
