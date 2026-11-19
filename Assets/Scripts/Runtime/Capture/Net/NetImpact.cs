using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// Picks what a landed net affects. Distances are measured on the ground plane, so a
    /// net resting on a mushroom cap and one on the ground beside it compare alike.
    /// </summary>
    public static class NetImpact
    {
        /// <summary>
        /// Returns the index of the position closest to the impact point within
        /// <paramref name="radius"/>, edge included, or -1 if none is. On a tie the
        /// lowest index wins.
        /// </summary>
        public static int SelectTarget(Vector3 impactPoint, float radius, IReadOnlyList<Vector3> positions)
        {
            ValidateRadius(radius);

            if (positions == null)
            {
                throw new ArgumentNullException(nameof(positions));
            }

            var selected = -1;
            var selectedDistance = float.PositiveInfinity;

            for (var index = 0; index < positions.Count; index++)
            {
                var distance = HorizontalDistance(impactPoint, positions[index]);
                if (distance <= radius && distance < selectedDistance)
                {
                    selected = index;
                    selectedDistance = distance;
                }
            }

            return selected;
        }

        /// <summary>
        /// Fills <paramref name="results"/> with the index of every position within
        /// <paramref name="radius"/> of the impact point, edge included. The list is cleared first.
        /// </summary>
        public static void FindWithin(
            Vector3 impactPoint,
            float radius,
            IReadOnlyList<Vector3> positions,
            List<int> results)
        {
            ValidateRadius(radius);

            if (positions == null)
            {
                throw new ArgumentNullException(nameof(positions));
            }

            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            results.Clear();

            for (var index = 0; index < positions.Count; index++)
            {
                if (HorizontalDistance(impactPoint, positions[index]) <= radius)
                {
                    results.Add(index);
                }
            }
        }

        public static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            var dx = a.x - b.x;
            var dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        private static void ValidateRadius(float radius)
        {
            if (radius < 0f || float.IsNaN(radius) || float.IsInfinity(radius))
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }
        }
    }
}
