using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// What marks a complete line of a cost or a recipe: a moss green disc with a dark tick,
    /// drawn as flat low poly facets without any texture. The green is also told apart by
    /// its lightness and by the tick, not by its hue alone.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    internal sealed class TickGraphic : MaskableGraphic
    {
        /// <summary>Palette moss green and the shadow brown.</summary>
        private static readonly Color32 Disc = new Color32(0x9C, 0xB9, 0x55, 0xFF);
        private static readonly Color32 Mark = new Color32(0x2B, 0x27, 0x23, 0xFF);

        /// <summary>Few sides, so that it reads as low poly like the rest of the interface.</summary>
        private const int Sides = 16;

        /// <summary>The tick, on a 24 unit square with its origin at the bottom left.</summary>
        private static readonly Vector2 TickStart = new Vector2(7f, 11.5f);
        private static readonly Vector2 TickBend = new Vector2(10.2f, 8.3f);
        private static readonly Vector2 TickEnd = new Vector2(17f, 15f);
        private const float DesignSize = 24f;
        private const float DesignRadius = 10f;
        private const float DesignStroke = 2.6f;

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            var rect = GetPixelAdjustedRect();
            var unit = Mathf.Min(rect.width, rect.height) / DesignSize;
            var centre = rect.center;

            var disc = new Vector2[Sides];
            for (var side = 0; side < Sides; side++)
            {
                var angle = side * Mathf.PI * 2f / Sides;
                disc[side] = centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * DesignRadius * unit;
            }

            Polygon.Add(vertexHelper, Tint(Disc), disc);

            var origin = centre - Vector2.one * (DesignSize * 0.5f * unit);
            var start = origin + TickStart * unit;
            var bend = origin + TickBend * unit;
            var end = origin + TickEnd * unit;
            var stroke = DesignStroke * unit;
            AddStroke(vertexHelper, start, bend, stroke, Tint(Mark));
            AddStroke(vertexHelper, bend, end, stroke, Tint(Mark));
        }

        /// <summary>A straight stroke, lengthened by half its width at each end so that two strokes meet without a notch.</summary>
        private static void AddStroke(VertexHelper vertexHelper, Vector2 from, Vector2 to, float width, Color32 colour)
        {
            var along = (to - from).normalized * (width * 0.5f);
            var across = new Vector2(-along.y, along.x);
            from -= along;
            to += along;
            Polygon.Add(vertexHelper, colour, from + across, to + across, to - across, from - across);
        }

        private Color32 Tint(Color32 flat)
        {
            return (Color)flat * color;
        }
    }
}
