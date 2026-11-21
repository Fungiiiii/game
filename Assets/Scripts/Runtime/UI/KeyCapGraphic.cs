using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// A low poly key cap drawn as flat facets, without any texture: a dark wooden square
    /// with cut corners, a darker base and a cyan rim on its top and sides, as on the
    /// Ramassage artboard.
    /// </summary>
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

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            var rect = GetPixelAdjustedRect();
            var unit = Mathf.Min(rect.width, rect.height) / DesignSize;
            var cut = DesignCorner * unit;
            var baseHeight = DesignBase * unit;
            var rim = DesignRim * unit;

            var left = rect.xMin;
            var right = rect.xMax;
            var bottom = rect.yMin;
            var top = rect.yMax;

            // The rim is the outer octagon down to the base; the face is drawn inside it.
            Polygon.Add(
                vertexHelper,
                Tint(Rim),
                new Vector2(left + cut, top),
                new Vector2(right - cut, top),
                new Vector2(right, top - cut),
                new Vector2(right, bottom + baseHeight),
                new Vector2(left, bottom + baseHeight),
                new Vector2(left, top - cut));

            Polygon.Add(
                vertexHelper,
                Tint(Face),
                new Vector2(left + cut, top - rim),
                new Vector2(right - cut, top - rim),
                new Vector2(right - rim, top - cut),
                new Vector2(right - rim, bottom + baseHeight),
                new Vector2(left + rim, bottom + baseHeight),
                new Vector2(left + rim, top - cut));

            Polygon.Add(
                vertexHelper,
                Tint(Base),
                new Vector2(left, bottom + baseHeight),
                new Vector2(right, bottom + baseHeight),
                new Vector2(right, bottom + cut * 0.5f),
                new Vector2(right - cut * 0.5f, bottom),
                new Vector2(left + cut * 0.5f, bottom),
                new Vector2(left, bottom + cut * 0.5f));
        }

        private Color32 Tint(Color32 flat)
        {
            return (Color)flat * color;
        }
    }
}
