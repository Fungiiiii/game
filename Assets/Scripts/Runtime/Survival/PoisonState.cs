using System;

namespace Fungiiiii.Survival
{
    /// <summary>
    /// Deterministic poison model shared by gameplay and presentation.
    /// </summary>
    public sealed class PoisonState
    {
        public const float MaxIntensity = 100f;
        public const float DamageThreshold = 50f;
        public const float DamageRateAtThreshold = 0.2f;
        public const float DamageRateAtMaximum = 10f;

        private readonly float maxHealth;
        private float intensity;

        public PoisonState(float maxHealth)
        {
            if (maxHealth <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHealth), "Max health must be greater than zero.");
            }

            this.maxHealth = maxHealth;
        }

        public event Action<PoisonSnapshot> Changed;

        public float MaxHealth => maxHealth;

        public float Intensity => intensity;

        public bool IsActive => intensity > 0f;

        public PoisonSnapshot Current => new PoisonSnapshot(
            intensity,
            MaxIntensity,
            CalculateDamagePerSecond(intensity, maxHealth));

        public float DamagePerSecond => CalculateDamagePerSecond(intensity, maxHealth);

        /// <summary>
        /// Adds a dose of poison and clamps the result to the model range.
        /// </summary>
        public bool Apply(float amount)
        {
            if (amount <= 0f)
            {
                return false;
            }

            return SetIntensity(intensity + amount);
        }

        /// <summary>
        /// Removes a dose of poison and clamps the result to the model range.
        /// </summary>
        public bool Remove(float amount)
        {
            if (amount <= 0f)
            {
                return false;
            }

            return SetIntensity(intensity - amount);
        }

        public bool SetIntensity(float value)
        {
            float clampedValue = Clamp(value, 0f, MaxIntensity);
            if (Math.Abs(intensity - clampedValue) < 0.0001f)
            {
                return false;
            }

            intensity = clampedValue;
            Changed?.Invoke(Current);
            return true;
        }

        public bool Clear()
        {
            return SetIntensity(0f);
        }

        /// <summary>
        /// Returns health damage for a simulation step without mutating health.
        /// </summary>
        public float TickDamage(float deltaTime)
        {
            if (deltaTime <= 0f || !IsActive)
            {
                return 0f;
            }

            return DamagePerSecond * deltaTime;
        }

        /// <summary>
        /// Returns damage as a percentage of max health per second.
        /// Poison is harmless below 50 %, then follows an exponential curve.
        /// </summary>
        public static float CalculateDamageRate(float poisonIntensity)
        {
            float clampedValue = Clamp(poisonIntensity, 0f, MaxIntensity);
            if (clampedValue < DamageThreshold)
            {
                return 0f;
            }

            float curvePosition = (clampedValue - DamageThreshold) / (MaxIntensity - DamageThreshold);
            float damageRangeMultiplier = DamageRateAtMaximum / DamageRateAtThreshold;
            return DamageRateAtThreshold * Pow(damageRangeMultiplier, curvePosition);
        }

        public static float CalculateDamagePerSecond(float poisonIntensity, float maxHealth)
        {
            if (maxHealth <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHealth), "Max health must be greater than zero.");
            }

            return maxHealth * CalculateDamageRate(poisonIntensity) / 100f;
        }

        private static float Clamp(float value, float minimum, float maximum)
        {
            return Math.Min(Math.Max(value, minimum), maximum);
        }

        private static float Pow(float value, float exponent)
        {
            return (float)Math.Pow(value, exponent);
        }
    }

    public readonly struct PoisonSnapshot
    {
        public PoisonSnapshot(float intensity, float maxIntensity, float damagePerSecond)
        {
            Intensity = intensity;
            MaxIntensity = maxIntensity;
            DamagePerSecond = damagePerSecond;
        }

        public float Intensity { get; }

        public float MaxIntensity { get; }

        public float DamagePerSecond { get; }

        public bool IsActive => Intensity > 0f;

        public float Normalized => MaxIntensity <= 0f ? 0f : Intensity / MaxIntensity;
    }
}
