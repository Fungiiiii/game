using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The drawn parts of the key hints, in flat low-poly shapes of the palette: a mouse
    /// with one part lit, the PlayStation cross, circle and square, the Xbox View icon.
    /// No texture: the shapes are built here, as for the planks and the key cap.
    /// </summary>
    internal sealed class GlyphShapeGraphic : MaskableGraphic
    {
        public enum Shape
        {
            MouseLeft,
            MouseRight,
            MouseWheel,
            Cross,
            Circle,
            Square,
            View,
        }

        private const int CircleSegments = 24;

        // Palette: "Body light", "Wood dark", the shadow brown, a dimmed wood and the cyan of the targets.
        private static readonly Color32 Cream = new Color32(0xF2, 0xE6, 0xCC, 0xFF);
        private static readonly Color32 WoodDark = new Color32(0x4F, 0x34, 0x24, 0xFF);
        private static readonly Color32 Shadow = new Color32(0x2B, 0x27, 0x23, 0xFF);
        private static readonly Color32 DimWood = new Color32(0x6B, 0x4A, 0x36, 0xFF);
        private static readonly Color32 Cyan = new Color32(0x6F, 0xD6, 0xEA, 0xFF);

        private Shape shape;

        public Shape Drawn
        {
            get => shape;
            set
            {
                shape = value;
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            var rect = GetPixelAdjustedRect();

            switch (shape)
            {
                case Shape.MouseLeft:
                case Shape.MouseRight:
                case Shape.MouseWheel:
                    AddMouse(vertexHelper, rect);
                    break;
                case Shape.Cross:
                    AddCross(vertexHelper, rect);
                    break;
                case Shape.Circle:
                    AddRing(vertexHelper, rect);
                    break;
                case Shape.Square:
                    AddFrame(vertexHelper, rect.center, rect.size * 0.6f, rect.height * 0.12f, Cream);
                    break;
                case Shape.View:
                    AddView(vertexHelper, rect);
                    break;
            }
        }

        /// <summary>A mouse drawn on a 24 x 30 grid, the button or the wheel of the hint lit.</summary>
        private void AddMouse(VertexHelper vertexHelper, Rect rect)
        {
            Vector2 At(float x, float y) => new Vector2(rect.xMin + x / 24f * rect.width, rect.yMax - y / 30f * rect.height);

            Polygon.Add(vertexHelper, WoodDark, At(4, 0), At(20, 0), At(24, 6), At(24, 24), At(18, 30), At(6, 30), At(0, 24), At(0, 6));
            Polygon.Add(vertexHelper, Shadow, At(0, 25), At(24, 25), At(19, 30), At(5, 30));
            Polygon.Add(vertexHelper, shape == Shape.MouseLeft ? Cream : DimWood, At(4, 0), At(11.4f, 0), At(11.4f, 12), At(0, 12), At(0, 6));
            Polygon.Add(vertexHelper, shape == Shape.MouseRight ? Cream : DimWood, At(12.6f, 0), At(20, 0), At(24, 6), At(24, 12), At(12.6f, 12));
            Polygon.Add(vertexHelper, shape == Shape.MouseWheel ? Cyan : Shadow, At(10.4f, 3), At(13.6f, 3), At(13.6f, 10), At(10.4f, 10));
        }

        private static void AddCross(VertexHelper vertexHelper, Rect rect)
        {
            var half = rect.size * 0.28f;
            var thickness = rect.height * 0.13f;
            AddBar(vertexHelper, rect.center - half, rect.center + half, thickness, Cream);
            AddBar(vertexHelper, rect.center + new Vector2(-half.x, half.y), rect.center + new Vector2(half.x, -half.y), thickness, Cream);
        }

        private static void AddRing(VertexHelper vertexHelper, Rect rect)
        {
            var outer = Mathf.Min(rect.width, rect.height) * 0.34f;
            var inner = outer - rect.height * 0.12f;
            for (var index = 0; index < CircleSegments; index++)
            {
                var from = index * Mathf.PI * 2f / CircleSegments;
                var to = (index + 1) * Mathf.PI * 2f / CircleSegments;
                var a = new Vector2(Mathf.Cos(from), Mathf.Sin(from));
                var b = new Vector2(Mathf.Cos(to), Mathf.Sin(to));
                Polygon.Add(vertexHelper, Cream, rect.center + a * outer, rect.center + b * outer, rect.center + b * inner, rect.center + a * inner);
            }
        }

        /// <summary>Two overlapping windows, as on the View button.</summary>
        private static void AddView(VertexHelper vertexHelper, Rect rect)
        {
            var size = new Vector2(rect.width * 0.45f, rect.height * 0.45f);
            var stroke = rect.height * 0.1f;
            var back = rect.center + new Vector2(-rect.width * 0.1f, rect.height * 0.1f);
            var front = rect.center + new Vector2(rect.width * 0.1f, -rect.height * 0.1f);
            AddFrame(vertexHelper, back, size, stroke, Cream);
            Polygon.Add(vertexHelper, WoodDark, front - size * 0.5f, front + new Vector2(size.x, -size.y) * 0.5f, front + size * 0.5f, front + new Vector2(-size.x, size.y) * 0.5f);
            AddFrame(vertexHelper, front, size, stroke, Cream);
        }

        private static void AddFrame(VertexHelper vertexHelper, Vector2 centre, Vector2 size, float stroke, Color32 frameColor)
        {
            var min = centre - size * 0.5f;
            var max = centre + size * 0.5f;
            Polygon.Add(vertexHelper, frameColor, new Vector2(min.x, max.y - stroke), new Vector2(max.x, max.y - stroke), max, new Vector2(min.x, max.y));
            Polygon.Add(vertexHelper, frameColor, min, new Vector2(max.x, min.y), new Vector2(max.x, min.y + stroke), new Vector2(min.x, min.y + stroke));
            Polygon.Add(vertexHelper, frameColor, min, new Vector2(min.x + stroke, min.y), new Vector2(min.x + stroke, max.y), new Vector2(min.x, max.y));
            Polygon.Add(vertexHelper, frameColor, new Vector2(max.x - stroke, min.y), new Vector2(max.x, min.y), max, new Vector2(max.x - stroke, max.y));
        }

        private static void AddBar(VertexHelper vertexHelper, Vector2 from, Vector2 to, float thickness, Color32 barColor)
        {
            var across = Vector2.Perpendicular((to - from).normalized) * (thickness * 0.5f);
            Polygon.Add(vertexHelper, barColor, from - across, to - across, to + across, from + across);
        }
    }
}
