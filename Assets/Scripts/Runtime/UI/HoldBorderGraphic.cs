using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The border of a key that can be held, close around it: an octagon with its corners
    /// cut like the key's, drawn as flat facets without any texture. A see-through cream
    /// track shows the key can be held; the cyan of what is aimed at fills it while it is
    /// held, from the top left corner, clockwise.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    internal sealed class HoldBorderGraphic : MaskableGraphic
    {
        /// <summary>Palette cream, see through, and the cyan of what is aimed at.</summary>
        private static readonly Color32 Track = new Color32(0xF2, 0xE6, 0xCC, 0x38);
        private static readonly Color32 Fill = new Color32(0x6F, 0xD6, 0xEA, 0xFF);

        /// <summary>
        /// Moving a 45 degree cut inwards by the border's width shortens it by this share of
        /// the width, so that the border keeps the same thickness along the cut.
        /// </summary>
        private const float CutInset = 0.41421356f;

        [Tooltip("Width of the border, in canvas units.")]
        [SerializeField, Min(0.5f)]
        private float thickness = 4f;

        [Tooltip("Size of the outer cut corners, in canvas units.")]
        [SerializeField, Min(0f)]
        private float corner = 10f;

        [SerializeField, Range(0f, 1f)]
        private float progress;

        /// <summary>How much of the border is filled, from 0 to 1.</summary>
        public float Progress
        {
            get => progress;
            set
            {
                var clamped = Mathf.Clamp01(value);
                if (Mathf.Approximately(clamped, progress))
                {
                    return;
                }

                progress = clamped;
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            var outerRect = GetPixelAdjustedRect();
            var width = Mathf.Min(thickness, outerRect.width * 0.5f, outerRect.height * 0.5f);
            var outerCut = Mathf.Min(corner, outerRect.width * 0.5f, outerRect.height * 0.5f);
            var innerRect = new Rect(
                outerRect.xMin + width,
                outerRect.yMin + width,
                outerRect.width - 2f * width,
                outerRect.height - 2f * width);
            var innerCut = Mathf.Max(0f, outerCut - width * CutInset);

            var outer = Octagon(outerRect, outerCut);
            var inner = Octagon(innerRect, innerCut);
            AddBorder(vertexHelper, outer, inner, 1f, Tint(Track));
            if (progress > 0f)
            {
                AddBorder(vertexHelper, outer, inner, progress, Tint(Fill));
            }
        }

        /// <summary>
        /// The share <paramref name="share"/> of the border, along its middle line, from the
        /// first corner of the octagons, one quad per side.
        /// </summary>
        private static void AddBorder(VertexHelper vertexHelper, Vector2[] outer, Vector2[] inner, float share, Color32 colour)
        {
            var count = outer.Length;
            var lengths = new float[count];
            var total = 0f;
            for (var side = 0; side < count; side++)
            {
                var next = (side + 1) % count;
                lengths[side] = (Vector2.Distance(outer[side], outer[next]) + Vector2.Distance(inner[side], inner[next])) * 0.5f;
                total += lengths[side];
            }

            var left = total * share;
            for (var side = 0; side < count && left > 0f; side++)
            {
                var next = (side + 1) % count;
                var part = lengths[side] <= 0f ? 1f : Mathf.Min(1f, left / lengths[side]);
                Polygon.Add(
                    vertexHelper,
                    colour,
                    outer[side],
                    Vector2.Lerp(outer[side], outer[next], part),
                    Vector2.Lerp(inner[side], inner[next], part),
                    inner[side]);
                left -= lengths[side];
            }
        }

        /// <summary>A rectangle with its four corners cut at 45 degrees, clockwise from the top left corner.</summary>
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

        private Color32 Tint(Color32 flat)
        {
            return (Color)flat * color;
        }
    }
}
