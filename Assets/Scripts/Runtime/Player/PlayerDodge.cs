using System;
using UnityEngine;

namespace Fungiiiii.Runtime.Player
{
    /// <summary>
    /// Deterministic dodge timing with no Unity lifecycle dependency.
    ///
    /// A dodge locks a direction on start, overrides normal movement for its whole
    /// duration, and then blocks further dodges until its cooldown elapses.
    /// </summary>
    public sealed class PlayerDodge
    {
        public readonly struct Configuration
        {
            public Configuration(float speed, float durationSeconds, float cooldownSeconds)
            {
                if (speed <= 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(speed));
                }

                if (durationSeconds <= 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(durationSeconds));
                }

                if (cooldownSeconds < durationSeconds)
                {
                    throw new ArgumentOutOfRangeException(nameof(cooldownSeconds));
                }

                Speed = speed;
                DurationSeconds = durationSeconds;
                CooldownSeconds = cooldownSeconds;
            }

            public float Speed { get; }
            public float DurationSeconds { get; }
            public float CooldownSeconds { get; }

            public static Configuration Default => new Configuration(
                speed: 10f,
                durationSeconds: 0.25f,
                cooldownSeconds: 0.7f);
        }

        private readonly Configuration configuration;
        private Vector3 direction;
        private float remainingDodgeSeconds;
        private float remainingCooldownSeconds;

        public PlayerDodge(Configuration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>True while the dodge is overriding normal movement.</summary>
        public bool IsDodging => remainingDodgeSeconds > 0f;

        /// <summary>True when a new dodge may be started.</summary>
        public bool IsReady => remainingDodgeSeconds <= 0f && remainingCooldownSeconds <= 0f;

        /// <summary>
        /// Velocity the dodge imposes this step, or zero when no dodge is running.
        /// </summary>
        public Vector3 Velocity => IsDodging ? direction * configuration.Speed : Vector3.zero;

        /// <summary>
        /// Starts a dodge along <paramref name="dodgeDirection"/> and reports whether it
        /// began. A zero direction, an active dodge or a running cooldown all refuse.
        /// </summary>
        public bool TryStart(Vector3 dodgeDirection)
        {
            if (!IsReady)
            {
                return false;
            }

            var planarDirection = new Vector3(dodgeDirection.x, 0f, dodgeDirection.z);
            if (planarDirection.sqrMagnitude <= 0.000001f)
            {
                return false;
            }

            direction = planarDirection.normalized;
            remainingDodgeSeconds = configuration.DurationSeconds;
            remainingCooldownSeconds = configuration.CooldownSeconds;
            return true;
        }

        /// <summary>
        /// Advances dodge and cooldown timers by a deterministic amount of time.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            remainingDodgeSeconds = Mathf.Max(0f, remainingDodgeSeconds - deltaTime);
            remainingCooldownSeconds = Mathf.Max(0f, remainingCooldownSeconds - deltaTime);
        }
    }
}
