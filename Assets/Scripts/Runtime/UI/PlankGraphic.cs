using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// A low poly wooden plank drawn as flat facets, without any texture: a rectangle
    /// with cut corners, a lit top edge, a dark underside and one darker facet at its
    /// end. Colours are those of the tree trunks in the project palette.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    internal sealed class PlankGraphic : MaskableGraphic
    {
        /// <summary>Palette "Wood", "Bark pale" and "Wood dark".</summary>
        private static readonly Color32 Face = new Color32(0x8A, 0x5F, 0x42, 0xFF);
        private static readonly Color32 LitEdge = new Color32(0xA8, 0x90, 0x78, 0xFF);
        private static readonly Color32 Underside = new Color32(0x4F, 0x34, 0x24, 0xFF);

        /// <summary>Share of the underside colour mixed into the end facet.</summary>
        private const float EndFacetShade = 0.4f;

        /// <summary>How much lighter the plank gets when fully highlighted.</summary>
        private const float HighlightGain = 0.15f;

        [SerializeField, Min(0f), Tooltip("Size of the cut corners, in canvas units.")]
        private float corner = 21f;

        [SerializeField, Min(0f), Tooltip("Height of the lit top edge, in canvas units.")]
        private float litEdge = 10f;

        [SerializeField, Min(0f), Tooltip("Height of the dark underside, in canvas units.")]
        private float underside = 15f;

        [SerializeField, Range(0f, 1f), Tooltip("Where the end facet starts along the plank: 0 is its left end, 1 its right end.")]
        private float endFacetStart = 0.74f;

        private float highlight;

        /// <summary>From 0 (at rest) to 1 (hovered or selected): the plank gets lighter.</summary>
        public float Highlight
        {
            get => highlight;
            set
            {
                var clamped = Mathf.Clamp01(value);
                if (!Mathf.Approximately(clamped, highlight))
                {
                    highlight = clamped;
                    SetVerticesDirty();
                }
            }
        }

        /// <summary>Where the end facet starts, so that two planks side by side do not look stamped.</summary>
        public float EndFacetStart
        {
            get => endFacetStart;
            set
            {
                endFacetStart = Mathf.Clamp01(value);
                SetVerticesDirty();
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

            // Corners are cut at 45 degrees, never deeper than half the plank.
            var cut = Mathf.Min(corner, rect.width * 0.5f, rect.height * 0.5f);
            var lit = Mathf.Min(litEdge, cut);
            var under = Mathf.Min(underside, cut);

            var face = Shade(Face);
            var endFacet = Shade(Color32.Lerp(Face, Underside, EndFacetShade));

            // The face, between the lit edge and the underside.
            var faceTop = top - lit;
            var faceBottom = bottom + under;
            Polygon.Add(
                vertexHelper,
                face,
                new Vector2(left + cut - lit, faceTop),
                new Vector2(right - cut + lit, faceTop),
                new Vector2(right, top - cut),
                new Vector2(right, bottom + cut),
                new Vector2(right - cut + under, faceBottom),
                new Vector2(left + cut - under, faceBottom),
                new Vector2(left, bottom + cut),
                new Vector2(left, top - cut));

            // The darker facet at the right end, leaning like a saw cut.
            var facetTop = Mathf.Lerp(left, right, endFacetStart);
            var facetBottom = facetTop - (faceTop - faceBottom) * 0.27f;
            if (facetTop < right - cut + lit)
            {
                Polygon.Add(
                    vertexHelper,
                    endFacet,
                    new Vector2(facetTop, faceTop),
                    new Vector2(right - cut + lit, faceTop),
                    new Vector2(right, top - cut),
                    new Vector2(right, bottom + cut),
                    new Vector2(right - cut + under, faceBottom),
                    new Vector2(facetBottom, faceBottom));
            }

            Polygon.Add(
                vertexHelper,
                Shade(LitEdge),
                new Vector2(left + cut, top),
                new Vector2(right - cut, top),
                new Vector2(right - cut + lit, faceTop),
                new Vector2(left + cut - lit, faceTop));

            Polygon.Add(
                vertexHelper,
                Shade(Underside),
                new Vector2(left + cut - under, faceBottom),
                new Vector2(right - cut + under, faceBottom),
                new Vector2(right - cut, bottom),
                new Vector2(left + cut, bottom));
        }

        private Color32 Shade(Color32 baseColor)
        {
            Color lit = Color.Lerp(baseColor, Color.white, HighlightGain * highlight);
            return lit * color;
        }
    }
}
