using System;
using UnityEngine;
using Random = System.Random;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// Slow stroll of an unaware mushroom around its home: walk to a random point of a
    /// disc around home, pause, pick another one. Moves on the ground plane only; the
    /// caller keeps the mushroom on the ground.
    /// </summary>
    public sealed class MushroomWander
    {
        private const float ArrivalDistance = 0.05f;

        private readonly Vector3 home;
        private readonly float radius;
        private readonly float speed;
        private readonly float pauseSeconds;
        private readonly Random random;

        private Vector3? target;
        private float pauseLeft;

        public MushroomWander(Vector3 home, float radius, float speed, float pauseSeconds, Random random)
        {
            if (radius < 0f || float.IsNaN(radius))
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }

            if (speed < 0f || float.IsNaN(speed))
            {
                throw new ArgumentOutOfRangeException(nameof(speed));
            }

            if (pauseSeconds < 0f || float.IsNaN(pauseSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(pauseSeconds));
            }

            this.home = home;
            this.radius = radius;
            this.speed = speed;
            this.pauseSeconds = pauseSeconds;
            this.random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public Vector3 Home => home;

        /// <summary>
        /// Moves <paramref name="current"/> by one step and returns the new position, with
        /// the same height.
        /// </summary>
        public Vector3 Step(Vector3 current, float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return current;
            }

            if (pauseLeft > 0f)
            {
                pauseLeft = Math.Max(0f, pauseLeft - deltaTime);
                return current;
            }

            if (!target.HasValue)
            {
                target = PickTarget();
            }

            var flatTarget = new Vector3(target.Value.x, current.y, target.Value.z);
            var next = Vector3.MoveTowards(current, flatTarget, speed * deltaTime);

            if ((flatTarget - next).sqrMagnitude <= ArrivalDistance * ArrivalDistance)
            {
                target = null;
                pauseLeft = pauseSeconds;
            }

            return next;
        }

        /// <summary>Forgets the current target, for instance after a flight elsewhere.</summary>
        public void Restart()
        {
            target = null;
            pauseLeft = 0f;
        }

        private Vector3 PickTarget()
        {
            // Square root keeps the points evenly spread over the disc, not bunched at its centre.
            var distance = radius * (float)Math.Sqrt(random.NextDouble());
            var angle = random.NextDouble() * 2.0 * Math.PI;
            return home + new Vector3((float)Math.Cos(angle) * distance, 0f, (float)Math.Sin(angle) * distance);
        }
    }
}
