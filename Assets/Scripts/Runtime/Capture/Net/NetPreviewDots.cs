using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// Where the dots of the aiming arc go: evenly spaced along the flight, none right at
    /// the hand, so that their number tells how far the net will go (playtest of 20/11,
    /// feedback #4: a fixed count crowded short throws and thinned long ones).
    /// </summary>
    public static class NetPreviewDots
    {
        /// <summary>The flight is sampled this often, in seconds.</summary>
        public const float TimeStep = 0.04f;

        /// <summary>A dot is placed once the flight is this far, in a straight line, from the previous one.</summary>
        public const float Spacing = 0.75f;

        /// <summary>No dot this close to the hand, in metres: it would sit in the view.</summary>
        public const float Clearance = 1f;

        public const int MaxDots = 24;

        /// <summary>Fills <paramref name="dots"/> with the dots of a flight lasting <paramref name="flightSeconds"/>.</summary>
        public static void Place(NetTrajectory trajectory, float flightSeconds, List<Vector3> dots)
        {
            dots.Clear();
            var origin = trajectory.Origin;
            var previous = origin;

            for (var seconds = TimeStep; seconds <= flightSeconds && dots.Count < MaxDots; seconds += TimeStep)
            {
                var point = trajectory.PositionAt(seconds);
                if ((point - origin).sqrMagnitude < Clearance * Clearance ||
                    (point - previous).sqrMagnitude < Spacing * Spacing)
                {
                    continue;
                }

                dots.Add(point);
                previous = point;
            }
        }
    }
}
