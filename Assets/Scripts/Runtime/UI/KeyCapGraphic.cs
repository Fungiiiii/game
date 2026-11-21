using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// A low poly key cap drawn as flat facets, without any texture: a dark wooden square
    /// with its four corners cut, a cyan rim all around it, corners included, and a darker
    /// base inside the rim, as on the Ramassage artboard.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    internal sealed class KeyCapGraphic : MaskableGraphic
    {
        /// <summary>Palette "Wood dark", the shadow brown and the cyan of what is aimed at.</summary>
        private static readonly Color32 Face = new Color32(0x4F, 0x34, 0x24, 0xFF);
        private static readonly Color32 Base = new Color32(0x2B, 0x27, 0x23, 0xFF);
        private static readonly Color32 Rim = new Color32(0x6F, 0xD6, 0xEA, 0xFF);

        /// <summary>Sizes on a 44 unit key, scaled with it.</summary>
        private const float DesignSize = 44f;
        private const float DesignCorner = 6f;
        private const float DesignBase = 4f;
        private const float DesignRim = 2f;

        /// <summary>
        /// Moving a 45 degree cut inwards by the rim's width shortens it by this share of
        /// the width, so that the rim keeps the same thickness along the cut.
        /// </summary>
        private const float CutInset = 0.41421356f;

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            var outer = GetPixelAdjustedRect();
            var unit = Mathf.Min(outer.width, outer.height) / DesignSize;
            var rim = DesignRim * unit;
            var cut = Mathf.Min(DesignCorner * unit, outer.width * 0.5f, outer.height * 0.5f);

            var inner = new Rect(outer.xMin + rim, outer.yMin + rim, outer.width - 2f * rim, outer.height - 2f * rim);
            var innerCut = Mathf.Max(0f, cut - rim * CutInset);

            Polygon.Add(vertexHelper, Tint(Rim), Octagon(outer, cut));
            Polygon.Add(vertexHelper, Tint(Face), Octagon(inner, innerCut));
            Polygon.Add(vertexHelper, Tint(Base), LowerBand(inner, innerCut, DesignBase * unit));
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
