using UnityEngine;

namespace Fungiiiii.Demo
{
    /// <summary>
    /// Placeholder ring of light on the ground around something the player can use:
    /// a closed line that follows the relief. Wider while aimed at.
    /// </summary>
    internal sealed class DemoGroundRing
    {
        private const int PointCount = 24;
        private const float Lift = 0.05f;
        private const float Width = 0.03f;
        private const float TargetedWidth = 0.08f;

        private readonly LineRenderer line;

        public DemoGroundRing(
            Transform parent,
            Vector3 centre,
            float radius,
            Material material,
            System.Func<Vector3, float> groundHeight)
        {
            var ringObject = new GameObject("Ground Ring");
            ringObject.transform.SetParent(parent, false);

            line = ringObject.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = true;
            line.loop = true;
            line.positionCount = PointCount;
            line.widthMultiplier = Width;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;

            for (var index = 0; index < PointCount; index++)
            {
                var angle = index * Mathf.PI * 2f / PointCount;
                var point = centre + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                point.y = groundHeight(point) + Lift;
                line.SetPosition(index, point);
            }
        }

        public void SetVisible(bool visible)
        {
            // Destroyed with its parent when the scene unloads, possibly before its owner lets go.
            if (line == null)
            {
                return;
            }

            line.enabled = visible;
        }

        public void SetTargeted(bool targeted)
        {
            if (line == null)
            {
                return;
            }

            line.widthMultiplier = targeted ? TargetedWidth : Width;
        }

        public void SetMaterial(Material material)
        {
            if (line == null)
            {
                return;
            }

            line.sharedMaterial = material;
        }
    }
}
