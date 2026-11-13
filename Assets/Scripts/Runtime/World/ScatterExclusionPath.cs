using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Runtime.World
{
    /// <summary>
    /// A path on the ground where <see cref="ForestScatter"/> places nothing: a gap
    /// in the trees, painted as path soil and smoothed by the relief tool. It is a
    /// smooth curve through <see cref="Points"/> (Catmull-Rom), as wide as twice
    /// <see cref="Radius"/>. Only the X/Z positions matter.
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
        /// Circles along the curve, at most half a radius apart, so their union
        /// covers the whole corridor with no gap between two of them.
        /// </summary>
        public List<ExclusionCircle> ToCircles()
        {
            var circles = new List<ExclusionCircle>();
            foreach (Vector3 centre in Sample(radius * 0.5f))
            {
                circles.Add(new ExclusionCircle(centre, radius));
            }

            return circles;
        }

        /// <summary>
        /// Points along the curve, consecutive ones at most <paramref name="spacing"/>
        /// apart. The curve passes through every corner and turns smoothly there.
        /// </summary>
        public List<Vector3> Sample(float spacing)
        {
            var samples = new List<Vector3>();
            if (points.Length == 0)
            {
                return samples;
            }

            samples.Add(points[0]);
            for (int i = 0; i + 1 < points.Length; i++)
            {
                Vector3 before = points[Mathf.Max(0, i - 1)];
                Vector3 after = points[Mathf.Min(points.Length - 1, i + 2)];
                // Curves are a little longer than their chord: oversample, then the
                // spacing between samples stays under the requested one.
                float length = Vector3.Distance(points[i], points[i + 1]) * 1.5f;
                int steps = Mathf.Max(1, Mathf.CeilToInt(length / spacing));
                for (int step = 1; step <= steps; step++)
                {
                    samples.Add(CatmullRom(before, points[i], points[i + 1], after, step / (float)steps));
                }
            }

            return samples;
        }

        private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * (2f * p1 + (p2 - p0) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (3f * p1 - p0 - 3f * p2 + p3) * t3);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.6f, 0.25f, 0.9f);
            List<Vector3> curve = Sample(2f);
            for (int i = 0; i + 1 < curve.Count; i++)
            {
                Gizmos.DrawLine(curve[i], curve[i + 1]);
            }
        }
    }
}
