using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Gathering
{
    /// <summary>Something the player can use with the interact control, seen from the targeting.</summary>
    public readonly struct InteractionCandidate
    {
        public InteractionCandidate(Vector3 point, float reach, bool isEnabled)
        {
            Point = point;
            Reach = reach;
            IsEnabled = isEnabled;
        }

        /// <summary>The point the player has to look at.</summary>
        public Vector3 Point { get; }

        /// <summary>Furthest ground distance, in metres, from which it can be used.</summary>
        public float Reach { get; }

        public bool IsEnabled { get; }
    }

    /// <summary>
    /// Picks what the player is aiming at, close by: the enabled candidate within reach
    /// whose point is nearest the centre of the view, inside a cone around it.
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
            var best = None;
            var bestAngle = maxAngleDegrees;

            for (var index = 0; index < candidates.Count; index++)
            {
                var candidate = candidates[index];
                if (!candidate.IsEnabled)
                {
                    continue;
                }

                // Reach is measured on the ground: looking down at your feet is not further away.
                var flat = candidate.Point - eye;
                flat.y = 0f;
                if (flat.magnitude > candidate.Reach)
                {
                    continue;
                }

                var toPoint = candidate.Point - eye;
                if (toPoint.sqrMagnitude <= 0f)
                {
                    return index;
                }

                var angle = Vector3.Angle(forward, toPoint);
                if (angle <= bestAngle)
                {
                    best = index;
                    bestAngle = angle;
                }
            }

            return best;
        }
    }
}
