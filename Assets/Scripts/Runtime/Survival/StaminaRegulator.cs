using System;

namespace Fungiiiii.Survival
{
    /// <summary>
    /// Stamina rules of RG_SU_03 and RG_SU_04, applied to the stamina of a
    /// <see cref="PlayerVitals"/>: sprinting drains it over time, and it regenerates once
    /// nothing has been spent for a short delay, walking included. At zero the player is
    /// exhausted: no sprint until stamina is back to a recovery threshold. The dodge, and
    /// its cost, were taken out of the game (playtest of 21/11).
    ///
    /// All amounts are percentages of the maximum stamina.
    /// </summary>
    public sealed class StaminaRegulator
    {
        public readonly struct Settings
        {
            public Settings(
                float sprintCostPerSecond,
                float regenerationPerSecond,
                float regenerationDelaySeconds,
                float recoveryThreshold)
            {
                ValidateNonNegative(sprintCostPerSecond, nameof(sprintCostPerSecond));
                ValidateNonNegative(regenerationPerSecond, nameof(regenerationPerSecond));
                ValidateNonNegative(regenerationDelaySeconds, nameof(regenerationDelaySeconds));
                ValidateNonNegative(recoveryThreshold, nameof(recoveryThreshold));

                SprintCostPerSecond = sprintCostPerSecond;
                RegenerationPerSecond = regenerationPerSecond;
                RegenerationDelaySeconds = regenerationDelaySeconds;
                RecoveryThreshold = recoveryThreshold;
            }

            public float SprintCostPerSecond { get; }
            public float RegenerationPerSecond { get; }
            public float RegenerationDelaySeconds { get; }
            public float RecoveryThreshold { get; }

            /// <summary>Starting values of RG_SU_03 and RG_SU_04, to balance in playtest.</summary>
            public static Settings Default => new Settings(
                sprintCostPerSecond: 15f,
                regenerationPerSecond: 20f,
                regenerationDelaySeconds: 1f,
                recoveryThreshold: 20f);

            private static void ValidateNonNegative(float value, string name)
            {
                if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                {
                    throw new ArgumentOutOfRangeException(name, "Value must be finite and not negative.");
                }
            }
        }

        private readonly PlayerVitals vitals;
        private readonly Settings settings;
        private float secondsSinceSpending;

        public StaminaRegulator(PlayerVitals vitals, Settings settings)
        {
            this.vitals = vitals ?? throw new ArgumentNullException(nameof(vitals));
            this.settings = settings;
            secondsSinceSpending = settings.RegenerationDelaySeconds;
        }

        /// <summary>True from the moment stamina hits zero until it is back to the recovery threshold.</summary>
        public bool IsExhausted { get; private set; }

        public bool CanSprint => !IsExhausted && vitals.Current.Stamina > 0f;

        /// <summary>
        /// Advances stamina by <paramref name="deltaTime"/> seconds.
        /// </summary>
        /// <param name="sprinting">True if the player sprinted during that time.</param>
        public void Tick(float deltaTime, bool sprinting)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            if (sprinting && CanSprint)
            {
                Spend(Percent(settings.SprintCostPerSecond) * deltaTime);
                return;
            }

            var previousIdleSeconds = secondsSinceSpending;
            secondsSinceSpending += deltaTime;

            // Only the part of this step past the delay regenerates.
            var regeneratingSeconds = Math.Min(
                deltaTime,
                secondsSinceSpending - Math.Max(previousIdleSeconds, settings.RegenerationDelaySeconds));

            if (regeneratingSeconds <= 0f)
            {
                return;
            }

            vitals.SetStamina(vitals.Current.Stamina + Percent(settings.RegenerationPerSecond) * regeneratingSeconds);

            if (IsExhausted && vitals.Current.Stamina >= Percent(settings.RecoveryThreshold))
            {
                IsExhausted = false;
            }
        }

        /// <summary>Full stamina, not exhausted. Does not touch the vitals: reset them separately.</summary>
        public void Reset()
        {
            IsExhausted = false;
            secondsSinceSpending = settings.RegenerationDelaySeconds;
        }

        private void Spend(float amount)
        {
            secondsSinceSpending = 0f;
            vitals.SetStamina(vitals.Current.Stamina - amount);

            if (vitals.Current.Stamina <= 0f)
            {
                IsExhausted = true;
            }
        }

        private float Percent(float percentage)
        {
            return vitals.MaxStamina * percentage / 100f;
        }
    }
}
