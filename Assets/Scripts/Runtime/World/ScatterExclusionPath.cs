using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Runtime.World
{
    /// <summary>
    /// A path on the ground where <see cref="ForestScatter"/> places nothing: a gap
    /// in the trees, painted as a clearing by the relief tool. It is a polyline
    /// through <see cref="Points"/>, as wide as twice <see cref="Radius"/>. Only the
    /// X/Z positions matter.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScatterExclusionPath : MonoBehaviour
    {
        [SerializeField, Tooltip("Corners of the path, in world space.")]
        private Vector3[] points = new Vector3[0];

        [SerializeField, Min(0.5f), Tooltip("Half the width of the path, in metres.")]
        private float radius = 4f;

        public float Radius => radius;

        public IReadOnlyList<Vector3> Points => points;

        internal void Configure(Vector3[] newPoints, float newRadius)
        {
            points = newPoints;
            radius = newRadius;
        }

        /// <summary>
        /// Circles along the path, half a radius apart, so their union covers the
        /// whole corridor with no gap between two of them.
        /// </summary>
        public List<ExclusionCircle> ToCircles()
        {
            var circles = new List<ExclusionCircle>();
            if (points.Length == 0)
            {
                return circles;
            }

            float spacing = radius * 0.5f;
            circles.Add(new ExclusionCircle(points[0], radius));
            for (int i = 0; i + 1 < points.Length; i++)
            {
                float length = Vector3.Distance(points[i], points[i + 1]);
                int steps = Mathf.Max(1, Mathf.CeilToInt(length / spacing));
                for (int step = 1; step <= steps; step++)
                {
                    circles.Add(new ExclusionCircle(Vector3.Lerp(points[i], points[i + 1], step / (float)steps), radius));
                }
            }

            return circles;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.6f, 0.25f, 0.9f);
            for (int i = 0; i + 1 < points.Length; i++)
            {
                Gizmos.DrawLine(points[i], points[i + 1]);
            }
        }
    }
}
