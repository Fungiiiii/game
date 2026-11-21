using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Interaction
{
    /// <summary>Something the player can use with the interact control, seen from the targeting.</summary>
    public readonly struct InteractionCandidate
    {
        public InteractionCandidate(Vector3 point, float reach, bool isEnabled)
            : this(point, reach, isEnabled, default, hasBounds: false)
        {
        }

        /// <param name="bounds">The box around its model, in world space: the crosshair on it targets it first.</param>
        public InteractionCandidate(Vector3 point, float reach, bool isEnabled, Bounds bounds)
            : this(point, reach, isEnabled, bounds, hasBounds: true)
        {
        }

        private InteractionCandidate(Vector3 point, float reach, bool isEnabled, Bounds bounds, bool hasBounds)
        {
            Point = point;
            Reach = reach;
            IsEnabled = isEnabled;
            Bounds = bounds;
            HasBounds = hasBounds;
        }

        /// <summary>The point the player has to look at.</summary>
        public Vector3 Point { get; }

        /// <summary>Furthest ground distance, in metres, from which it can be used.</summary>
        public float Reach { get; }

        public bool IsEnabled { get; }

        /// <summary>The box around its model, when it has one.</summary>
        public Bounds Bounds { get; }

        public bool HasBounds { get; }
    }

    /// <summary>
    /// Picks what the player is aiming at, close by, among the enabled candidates within
    /// reach. What is right under the crosshair wins, the nearest first: the stick looked
    /// at beats the mushroom at the player's feet. Otherwise it is the candidate whose
    /// point is nearest the centre of the view, inside a cone around it.
    /// </summary>
    public static class InteractionTargeting
    {
        public const int None = -1;

        /// <param name="eye">Position of the view.</param>
        /// <param name="forward">Direction of the view.</param>
        /// <param name="maxAngleDegrees">Half angle of the cone around the view.</param>
        /// <returns>Index of the target in <paramref name="candidates"/>, or <see cref="None"/>.</returns>
        public static int Pick(
            Vector3 eye,
            Vector3 forward,
            IReadOnlyList<InteractionCandidate> candidates,
            float maxAngleDegrees)
        {
            var crosshair = new Ray(eye, forward);
            var underCrosshair = None;
            var nearestHit = float.PositiveInfinity;
            var best = None;
            var bestAngle = maxAngleDegrees;

            for (var index = 0; index < candidates.Count; index++)
            {
                var candidate = candidates[index];
                if (!candidate.IsEnabled || !IsWithinReach(eye, candidate))
                {
                    continue;
                }

                if (candidate.HasBounds && candidate.Bounds.IntersectRay(crosshair, out var hit) && hit < nearestHit)
                {
                    underCrosshair = index;
                    nearestHit = hit;
                }

                var toPoint = candidate.Point - eye;
                if (toPoint.sqrMagnitude <= 0f)
                {
                    best = index;
                    bestAngle = 0f;
                    continue;
                }

                var angle = Vector3.Angle(forward, toPoint);
                if (angle <= bestAngle)
                {
                    best = index;
                    bestAngle = angle;
                }
            }

            return underCrosshair != None ? underCrosshair : best;
        }

        // Reach is measured on the ground: looking down at your feet is not further away.
        private static bool IsWithinReach(Vector3 eye, InteractionCandidate candidate)
        {
            var flat = candidate.Point - eye;
            flat.y = 0f;
            return flat.magnitude <= candidate.Reach;
        }
    }
}
