using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// Where the dots of the aiming arc go: evenly spaced along the flight, none right at
    /// the hand, so that their number tells how far the net will go (playtest of 20/11,
    /// feedback #4: a fixed count crowded short throws and thinned long ones).
    ///
    /// Each dot sits exactly where the flight crosses the spacing, found between two
    /// samples: placed on the samples themselves, the dots jumped by a whole sample when
    /// the aim moved slightly.
    /// </summary>
    public static class NetPreviewDots
    {
        /// <summary>The flight is sampled this often, in seconds.</summary>
        public const float TimeStep = 0.04f;

        /// <summary>Each dot is this far, in a straight line, from the previous one.</summary>
        public const float Spacing = 0.75f;

        /// <summary>The first dot is this far from the hand, in metres: closer, it would sit in the view.</summary>
        public const float Clearance = 1f;

        public const int MaxDots = 24;

        private const int CrossingIterations = 16;

        /// <summary>Fills <paramref name="dots"/> with the dots of a flight lasting <paramref name="flightSeconds"/>.</summary>
        public static void Place(NetTrajectory trajectory, float flightSeconds, List<Vector3> dots)
        {
            dots.Clear();
            var anchor = trajectory.Origin;
            var reach = Clearance;
            var from = 0f;

            while (from < flightSeconds && dots.Count < MaxDots)
            {
                var to = Mathf.Min(from + TimeStep, flightSeconds);
                while (dots.Count < MaxDots && IsBeyond(trajectory.PositionAt(to), anchor, reach))
                {
                    from = FindCrossing(trajectory, from, to, anchor, reach);
                    anchor = trajectory.PositionAt(from);
                    dots.Add(anchor);
                    reach = Spacing;
                }

                from = to;
            }
        }

        private static bool IsBeyond(Vector3 point, Vector3 anchor, float reach)
        {
            return (point - anchor).sqrMagnitude >= reach * reach;
        }

        /// <summary>
        /// The time, between <paramref name="inside"/> and <paramref name="outside"/>, when the
        /// flight is <paramref name="reach"/> from <paramref name="anchor"/>. A sample lasts
        /// too short for the flight to turn back towards the anchor, so halving converges.
        /// </summary>
        private static float FindCrossing(NetTrajectory trajectory, float inside, float outside, Vector3 anchor, float reach)
        {
            for (var iteration = 0; iteration < CrossingIterations; iteration++)
            {
                var middle = (inside + outside) * 0.5f;
                if (IsBeyond(trajectory.PositionAt(middle), anchor, reach))
                {
                    outside = middle;
                }
                else
                {
                    inside = middle;
                }
            }

            return outside;
        }
    }
}
