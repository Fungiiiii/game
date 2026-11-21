using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The ring around the key to press that fills while the key is held, clockwise from
    /// the top, drawn as flat low poly segments without any texture: a dark track, and the
    /// cyan of what is aimed at over the part already held.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    internal sealed class HoldRingGraphic : MaskableGraphic
    {
        /// <summary>The shadow brown of the key cap, see through, and the cyan of what is aimed at.</summary>
        private static readonly Color32 Track = new Color32(0x2B, 0x27, 0x23, 0xA0);
        private static readonly Color32 Fill = new Color32(0x6F, 0xD6, 0xEA, 0xFF);

        /// <summary>Few segments, so that it reads as low poly like the rest of the interface.</summary>
        private const int Segments = 32;

        [Tooltip("Width of the ring, in units of the graphic.")]
        [SerializeField, Min(0.5f)]
        private float thickness = 5f;

        [SerializeField, Range(0f, 1f)]
        private float progress;

        /// <summary>How much of the ring is filled, from 0 to 1.</summary>
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

            var rect = GetPixelAdjustedRect();
            var outer = Mathf.Min(rect.width, rect.height) * 0.5f;
            var inner = Mathf.Max(0f, outer - thickness);
            var centre = rect.center;

            AddArc(vertexHelper, centre, inner, outer, 0f, 1f, Tint(Track));
            if (progress > 0f)
            {
                AddArc(vertexHelper, centre, inner, outer, 0f, progress, Tint(Fill));
            }
        }

        /// <summary>The part of the ring between two shares of a turn, clockwise from the top.</summary>
        private static void AddArc(VertexHelper vertexHelper, Vector2 centre, float inner, float outer, float from, float to, Color32 colour)
        {
            var count = Mathf.Max(1, Mathf.CeilToInt(Segments * (to - from)));
            for (var index = 0; index < count; index++)
            {
                var start = Mathf.Lerp(from, to, index / (float)count);
                var end = Mathf.Lerp(from, to, (index + 1) / (float)count);
                var first = vertexHelper.currentVertCount;
                vertexHelper.AddVert(Point(centre, inner, start), colour, Vector4.zero);
                vertexHelper.AddVert(Point(centre, outer, start), colour, Vector4.zero);
                vertexHelper.AddVert(Point(centre, outer, end), colour, Vector4.zero);
                vertexHelper.AddVert(Point(centre, inner, end), colour, Vector4.zero);
                vertexHelper.AddTriangle(first, first + 1, first + 2);
                vertexHelper.AddTriangle(first, first + 2, first + 3);
            }
        }

        private static Vector2 Point(Vector2 centre, float radius, float share)
        {
            // Clockwise from the top: the angle goes down from 90 degrees.
            var angle = Mathf.PI * 0.5f - share * Mathf.PI * 2f;
            return centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        private Color32 Tint(Color32 flat)
        {
            return (Color)flat * color;
        }
    }
}
