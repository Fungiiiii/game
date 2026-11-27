using System;

namespace Fungiiiii.Gathering
{
    /// <summary>
    /// One spot that can be gathered: available, then gone for a random delay once
    /// gathered, then back at the same spot.
    /// </summary>
    public sealed class RespawningResource
    {
        private readonly float minRespawnSeconds;
        private readonly float maxRespawnSeconds;
        private readonly Random random;

        public RespawningResource(float minRespawnSeconds, float maxRespawnSeconds, Random random)
        {
            if (float.IsNaN(minRespawnSeconds) || minRespawnSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(minRespawnSeconds));
            }

            if (float.IsNaN(maxRespawnSeconds) || maxRespawnSeconds < minRespawnSeconds)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRespawnSeconds));
            }

            this.minRespawnSeconds = minRespawnSeconds;
            this.maxRespawnSeconds = maxRespawnSeconds;
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            IsAvailable = true;
        }

        public bool IsAvailable { get; private set; }

        /// <summary>Seconds left before the resource is back. Zero while it is available.</summary>
        public float SecondsUntilRespawn { get; private set; }

        /// <summary>Gathers the resource. Refused while it is gone.</summary>
        public bool TryGather()
        {
            if (!IsAvailable)
            {
                return false;
            }

            IsAvailable = false;
            SecondsUntilRespawn = minRespawnSeconds +
                                  (float)random.NextDouble() * (maxRespawnSeconds - minRespawnSeconds);
            return true;
        }

        /// <summary>Counts down the respawn. Returns true on the tick the resource comes back.</summary>
        public bool Tick(float deltaTime)
        {
            if (IsAvailable || deltaTime <= 0f)
            {
                return false;
            }

            SecondsUntilRespawn = Math.Max(0f, SecondsUntilRespawn - deltaTime);
            if (SecondsUntilRespawn > 0f)
            {
                return false;
            }

            IsAvailable = true;
            return true;
        }

        /// <summary>Back at once, for a reset.</summary>
        public void Reset()
        {
            IsAvailable = true;
            SecondsUntilRespawn = 0f;
        }
    }
}
