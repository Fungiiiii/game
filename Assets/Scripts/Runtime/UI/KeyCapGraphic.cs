using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// A low poly key cap drawn as flat facets, without any texture: a square of light wood
    /// with its four corners cut and a dark base, like the planks of the menus. No rim: a
    /// short press is shown by a triangle over the key (<see cref="TriangleMarkGraphic"/>),
    /// a hold by a border around it (<see cref="HoldBorderGraphic"/>).
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    internal sealed class KeyCapGraphic : MaskableGraphic
    {
        /// <summary>Palette "Wood" and the shadow brown.</summary>
        private static readonly Color32 Face = new Color32(0x8A, 0x5F, 0x42, 0xFF);
        private static readonly Color32 Base = new Color32(0x2B, 0x27, 0x23, 0xFF);

        /// <summary>Sizes on a 44 unit key, scaled with it.</summary>
        private const float DesignSize = 44f;
        private const float DesignCorner = 7f;
        private const float DesignBase = 5f;

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            var rect = GetPixelAdjustedRect();
            var unit = Mathf.Min(rect.width, rect.height) / DesignSize;
            var cut = Mathf.Min(DesignCorner * unit, rect.width * 0.5f, rect.height * 0.5f);

            Polygon.Add(vertexHelper, Tint(Face), Octagon(rect, cut));
            Polygon.Add(vertexHelper, Tint(Base), LowerBand(rect, cut, DesignBase * unit));
        }

        /// <summary>A rectangle with its four corners cut at 45 degrees, clockwise from the top left.</summary>
        private static Vector2[] Octagon(Rect rect, float cut)
        {
            return new[]
            {
                new Vector2(rect.xMin + cut, rect.yMax),
                new Vector2(rect.xMax - cut, rect.yMax),
                new Vector2(rect.xMax, rect.yMax - cut),
                new Vector2(rect.xMax, rect.yMin + cut),
                new Vector2(rect.xMax - cut, rect.yMin),
                new Vector2(rect.xMin + cut, rect.yMin),
                new Vector2(rect.xMin, rect.yMin + cut),
                new Vector2(rect.xMin, rect.yMax - cut),
            };
        }

        /// <summary>The bottom <paramref name="height"/> of an octagon, following its lower cut corners.</summary>
        private static Vector2[] LowerBand(Rect rect, float cut, float height)
        {
            var top = rect.yMin + height;
            if (height < cut)
            {
                // The band ends within the cut corners: its top edge is narrower than the octagon.
                var narrowing = cut - height;
                return new[]
                {
                    new Vector2(rect.xMin + narrowing, top),
                    new Vector2(rect.xMax - narrowing, top),
                    new Vector2(rect.xMax - cut, rect.yMin),
                    new Vector2(rect.xMin + cut, rect.yMin),
                };
            }

            return new[]
            {
                new Vector2(rect.xMin, top),
                new Vector2(rect.xMax, top),
                new Vector2(rect.xMax, rect.yMin + cut),
                new Vector2(rect.xMax - cut, rect.yMin),
                new Vector2(rect.xMin + cut, rect.yMin),
                new Vector2(rect.xMin, rect.yMin + cut),
            };
        }

        private Color32 Tint(Color32 flat)
        {
            return (Color)flat * color;
        }
    }
}
