using System;
using System.Collections.Generic;
using Fungiiiii.Companions;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// A wild creature that soaks up what it finds, as the Spongeus drinks at the water
    /// ferns: calm and dry, it walks to the nearest full source within reach, soaks it up for
    /// a moment, and is wet from then on. Fleeing interrupts it. Without Unity: positions in,
    /// where to go out.
    /// </summary>
    public sealed class WildSoak
    {
        private readonly float seekRadius;
        private readonly float soakSeconds;
        private readonly float arrivalDistance;
        private readonly ItemDefinition yield;
        private IAbsorbable target;
        private float soakElapsed;

        public WildSoak(float seekRadius, float soakSeconds, float arrivalDistance, ItemDefinition yield)
        {
            if (seekRadius <= 0f || soakSeconds <= 0f || arrivalDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(seekRadius), "The reach, the time to soak up and the arrival distance must be positive.");
            }

            this.seekRadius = seekRadius;
            this.soakSeconds = soakSeconds;
            this.arrivalDistance = arrivalDistance;
            this.yield = yield != null ? yield : throw new ArgumentNullException(nameof(yield));
        }

        public bool IsWet { get; private set; }

        /// <summary>Walking to a source or soaking it up.</summary>
        public bool IsBusy => target != null;

        public bool IsSoaking { get; private set; }

        /// <summary>How wet it looks, from 0 (dry) to 1 (wet): it fills while it soaks up.</summary>
        public float Wetness => IsWet ? 1f : IsSoaking ? Mathf.Clamp01(soakElapsed / soakSeconds) : 0f;

        public IAbsorbable Target => target;

        /// <summary>
        /// One step while calm.
        /// </summary>
        /// <returns>Where to stand after this step, or null when it has nothing to soak up and may wander.</returns>
        public Vector3? Step(Vector3 position, IReadOnlyList<IAbsorbable> sources, float speed, float deltaTime)
        {
            if (IsWet)
            {
                return null;
            }

            if (target != null && !target.CanBeAbsorbed)
            {
                Interrupt();
            }

            if (target == null)
            {
                target = Nearest(position, sources);
                if (target == null)
                {
                    return null;
                }
            }

            var offset = target.Position - position;
            offset.y = 0f;
            var distance = offset.magnitude;
            if (distance > arrivalDistance)
            {
                IsSoaking = false;
                soakElapsed = 0f;
                return position + offset / distance * Mathf.Min(distance - arrivalDistance * 0.5f, speed * deltaTime);
            }

            IsSoaking = true;
            soakElapsed += deltaTime;
            if (soakElapsed >= soakSeconds)
            {
                IsWet = target.Absorb();
                target = null;
                IsSoaking = false;
                soakElapsed = 0f;
            }

            return position;
        }

        /// <summary>Squeezed by hand: what it soaked up comes out, and it is dry again. False when it was dry.</summary>
        public bool Squeeze()
        {
            if (!IsWet)
            {
                return false;
            }

            IsWet = false;
            Interrupt();
            return true;
        }

        /// <summary>It flees: what it was heading for is forgotten.</summary>
        public void Interrupt()
        {
            target = null;
            IsSoaking = false;
            soakElapsed = 0f;
        }

        private IAbsorbable Nearest(Vector3 position, IReadOnlyList<IAbsorbable> sources)
        {
            IAbsorbable nearest = null;
            var nearestSquared = seekRadius * seekRadius;
            foreach (var source in sources)
            {
                if (source.Yield != yield || !source.CanBeAbsorbed)
                {
                    continue;
                }

                var offset = source.Position - position;
                offset.y = 0f;
                if (offset.sqrMagnitude <= nearestSquared)
                {
                    nearest = source;
                    nearestSquared = offset.sqrMagnitude;
                }
            }

            return nearest;
        }
    }
}
