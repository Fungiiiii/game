using System;
using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// Casts the straight segment between two points of a flight and reports the first hit.
    /// The implementation decides what can stop the net (a physics raycast in the game,
    /// a flat plane in tests).
    /// </summary>
    public delegate bool NetSegmentCast(Vector3 from, Vector3 to, out Vector3 hitPoint);

    /// <summary>Where and when a net flight first touches something.</summary>
    public readonly struct NetLanding
    {
        public NetLanding(Vector3 point, float flightSeconds)
        {
            Point = point;
            FlightSeconds = flightSeconds;
        }

        public Vector3 Point { get; }

        public float FlightSeconds { get; }
    }

    /// <summary>
    /// Ballistic flight of a thrown net: a launch point, a launch velocity and a constant
    /// downward gravity. No drag, no dependency on the Unity lifecycle.
    /// </summary>
    public readonly struct NetTrajectory
    {
        private readonly Vector3 origin;
        private readonly Vector3 initialVelocity;
        private readonly float gravity;

        /// <param name="gravity">Downward acceleration in m/s², as a positive magnitude.</param>
        public NetTrajectory(Vector3 origin, Vector3 initialVelocity, float gravity)
        {
            if (gravity < 0f || float.IsNaN(gravity) || float.IsInfinity(gravity))
            {
                throw new ArgumentOutOfRangeException(nameof(gravity));
            }

            this.origin = origin;
            this.initialVelocity = initialVelocity;
            this.gravity = gravity;
        }

        public Vector3 Origin => origin;

        public Vector3 InitialVelocity => initialVelocity;

        /// <summary>
        /// Builds the flight of a net thrown along <paramref name="direction"/> at
        /// <paramref name="speed"/>. A zero direction throws straight ahead along +Z.
        /// </summary>
        public static NetTrajectory Launch(Vector3 origin, Vector3 direction, float speed, float gravity)
        {
            if (speed < 0f || float.IsNaN(speed) || float.IsInfinity(speed))
            {
                throw new ArgumentOutOfRangeException(nameof(speed));
            }

            var launchDirection = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
            return new NetTrajectory(origin, launchDirection * speed, gravity);
        }

        public Vector3 PositionAt(float seconds)
        {
            return origin + initialVelocity * seconds + Vector3.down * (0.5f * gravity * seconds * seconds);
        }

        public Vector3 VelocityAt(float seconds)
        {
            return initialVelocity + Vector3.down * (gravity * seconds);
        }

        /// <summary>
        /// Walks the flight in steps of <paramref name="timeStep"/> and casts each step
        /// until something is hit or <paramref name="maxFlightSeconds"/> is reached.
        /// The flight time of the hit is interpolated inside the step that hit.
        /// </summary>
        /// <returns>False if nothing was hit before the maximum flight time.</returns>
        public bool TryFindLanding(
            float timeStep,
            float maxFlightSeconds,
            NetSegmentCast cast,
            out NetLanding landing)
        {
            if (timeStep <= 0f || float.IsNaN(timeStep) || float.IsInfinity(timeStep))
            {
                throw new ArgumentOutOfRangeException(nameof(timeStep));
            }

            if (maxFlightSeconds <= 0f || float.IsNaN(maxFlightSeconds) || float.IsInfinity(maxFlightSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(maxFlightSeconds));
            }

            if (cast == null)
            {
                throw new ArgumentNullException(nameof(cast));
            }

            var previousTime = 0f;
            var previousPoint = origin;

            while (previousTime < maxFlightSeconds)
            {
                var nextTime = Mathf.Min(previousTime + timeStep, maxFlightSeconds);
                var nextPoint = PositionAt(nextTime);

                if (cast(previousPoint, nextPoint, out var hitPoint))
                {
                    var segmentLength = Vector3.Distance(previousPoint, nextPoint);
                    var fraction = segmentLength > 0f
                        ? Mathf.Clamp01(Vector3.Distance(previousPoint, hitPoint) / segmentLength)
                        : 0f;

                    landing = new NetLanding(hitPoint, Mathf.Lerp(previousTime, nextTime, fraction));
                    return true;
                }

                previousTime = nextTime;
                previousPoint = nextPoint;
            }

            landing = default;
            return false;
        }
    }
}
