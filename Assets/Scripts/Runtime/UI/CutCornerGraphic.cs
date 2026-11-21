using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// A low poly panel drawn as flat facets, without any texture: a rectangle with cut
    /// corners, an optional band along its top and its bottom, and an optional see-through
    /// facet over its right part, leaning like a saw cut. The inventory's panel, slots and
    /// key pills are all drawn with it.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    internal sealed class CutCornerGraphic : MaskableGraphic
    {
        [SerializeField, Min(0f), Tooltip("Size of the cut corners, in canvas units.")]
        private float corner = 10f;

        [SerializeField]
        private Color face = Color.white;

        [SerializeField]
        private Color topBand = Color.clear;

        [SerializeField, Min(0f), Tooltip("No deeper than the corners.")]
        private float topBandHeight;

        [SerializeField]
        private Color bottomBand = Color.clear;

        [SerializeField, Min(0f), Tooltip("No deeper than the corners.")]
        private float bottomBandHeight;

        [SerializeField, Tooltip("A see-through facet over the right part; clear for none.")]
        private Color facet = Color.clear;

        [SerializeField, Range(0f, 1f), Tooltip("Where the facet starts along the top edge, as a share of the width.")]
        private float facetTopShare = 1f;

        [SerializeField, Range(0f, 1f), Tooltip("Where the facet starts along the bottom edge, as a share of the width.")]
        private float facetBottomShare = 1f;

        private float brightness = 1f;

        /// <summary>Multiplies every colour, for a slot under the pointer.</summary>
        public float Brightness
        {
            get => brightness;
            set
            {
                if (!Mathf.Approximately(value, brightness))
                {
                    brightness = value;
                    SetVerticesDirty();
                }
            }
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            var rect = GetPixelAdjustedRect();
            var left = rect.xMin;
            var right = rect.xMax;
            var bottom = rect.yMin;
            var top = rect.yMax;
            var cut = Mathf.Min(corner, rect.width * 0.5f, rect.height * 0.5f);
            var upper = Mathf.Min(topBandHeight, cut);
            var lower = Mathf.Min(bottomBandHeight, cut);

            Polygon.Add(
                vertexHelper,
                Tint(face),
                new Vector2(left + cut - upper, top - upper),
                new Vector2(right - cut + upper, top - upper),
                new Vector2(right, top - cut),
                new Vector2(right, bottom + cut),
                new Vector2(right - cut + lower, bottom + lower),
                new Vector2(left + cut - lower, bottom + lower),
                new Vector2(left, bottom + cut),
                new Vector2(left, top - cut));

            if (upper > 0f)
            {
                Polygon.Add(
                    vertexHelper,
                    Tint(topBand),
                    new Vector2(left + cut, top),
                    new Vector2(right - cut, top),
                    new Vector2(right - cut + upper, top - upper),
                    new Vector2(left + cut - upper, top - upper));
            }

            if (lower > 0f)
            {
                Polygon.Add(
                    vertexHelper,
                    Tint(bottomBand),
                    new Vector2(left + cut - lower, bottom + lower),
                    new Vector2(right - cut + lower, bottom + lower),
                    new Vector2(right - cut, bottom),
                    new Vector2(left + cut, bottom));
            }

            var facetTop = Mathf.Lerp(left, right, facetTopShare);
            var facetBottom = Mathf.Lerp(left, right, facetBottomShare);
            if (facet.a > 0f && facetTop < right - cut && facetBottom < right - cut)
            {
                Polygon.Add(
                    vertexHelper,
                    Tint(facet),
                    new Vector2(facetTop, top),
                    new Vector2(right - cut, top),
                    new Vector2(right, top - cut),
                    new Vector2(right, bottom + cut),
                    new Vector2(right - cut, bottom),
                    new Vector2(facetBottom, bottom));
            }
        }

        private Color32 Tint(Color flat)
        {
            var lit = new Color(
                Mathf.Clamp01(flat.r * brightness),
                Mathf.Clamp01(flat.g * brightness),
                Mathf.Clamp01(flat.b * brightness),
                flat.a);
            return lit * color;
        }
    }
}
