using System;
using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// Looks straight down from <paramref name="from"/> for at most
    /// <paramref name="maxDistance"/> metres and reports the first surface below.
    /// A physics raycast in the game, a fake terrain in tests.
    /// </summary>
    public delegate bool NetGroundProbe(Vector3 from, float maxDistance, out Vector3 groundPoint);

    /// <summary>
    /// Where the points of a net go: a centre and a ring of rim points, either flat
    /// (in the air) or draped point by point on whatever lies below (ground, slope,
    /// rock, mushroom cap). A net model can later be bound to these points.
    /// </summary>
    public static class NetRimLayout
    {
        /// <summary>Lays the rim flat and level around <paramref name="centre"/>.</summary>
        public static void Flat(Vector3 centre, float radius, Vector3[] rim)
        {
            ValidateArguments(radius, rim);

            for (var index = 0; index < rim.Length; index++)
            {
                rim[index] = centre + Offset(index, rim.Length, radius);
            }
        }

        /// <summary>
        /// Drapes the net: each rim point, and the centre, is moved onto the first surface
        /// found below it, starting <paramref name="probeHeight"/> above the net and
        /// looking down twice that far, then lifted by <paramref name="lift"/> so it lies
        /// on the surface instead of in it. A point with nothing below stays flat.
        /// </summary>
        /// <returns>The draped centre.</returns>
        public static Vector3 Drape(
            Vector3 centre,
            float radius,
            Vector3[] rim,
            NetGroundProbe probe,
            float probeHeight,
            float lift)
        {
            ValidateArguments(radius, rim);

            if (probe == null)
            {
                throw new ArgumentNullException(nameof(probe));
            }

            if (probeHeight <= 0f || float.IsNaN(probeHeight) || float.IsInfinity(probeHeight))
            {
                throw new ArgumentOutOfRangeException(nameof(probeHeight));
            }

            for (var index = 0; index < rim.Length; index++)
            {
                rim[index] = DrapePoint(centre + Offset(index, rim.Length, radius), probe, probeHeight, lift);
            }

            return DrapePoint(centre, probe, probeHeight, lift);
        }

        private static Vector3 DrapePoint(Vector3 point, NetGroundProbe probe, float probeHeight, float lift)
        {
            return probe(point + Vector3.up * probeHeight, probeHeight * 2f, out var ground)
                ? ground + Vector3.up * lift
                : point;
        }

        private static Vector3 Offset(int index, int count, float radius)
        {
            var angle = 2f * Mathf.PI * index / count;
            return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        private static void ValidateArguments(float radius, Vector3[] rim)
        {
            if (radius < 0f || float.IsNaN(radius) || float.IsInfinity(radius))
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }

            if (rim == null)
            {
                throw new ArgumentNullException(nameof(rim));
            }

            if (rim.Length < 3)
            {
                throw new ArgumentException("A rim needs at least three points.", nameof(rim));
            }
        }
    }
}
