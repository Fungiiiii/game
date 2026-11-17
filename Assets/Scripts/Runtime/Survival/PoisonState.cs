using System;

namespace Fungiiiii.Survival
{
    /// <summary>Bounded poison intensity and health-independent damage rate.</summary>
    public sealed class PoisonState
    {
        public const float MaxIntensity = 100f;
        public const float DamageThreshold = 50f;
        public const float DamageRateAtThreshold = 0.2f;
        public const float DamageRateAtMaximum = 10f;

        private float intensity;

        public event Action<PoisonSnapshot> Changed;

        public float Intensity => intensity;
        public bool IsActive => intensity > 0f;
        public float DamageRate => CalculateDamageRate(intensity);
        public PoisonSnapshot Current => new PoisonSnapshot(intensity, DamageRate);

        public bool Apply(float amount)
        {
            ValidateFinite(amount, nameof(amount));
            return amount > 0f && SetIntensity(intensity + Math.Min(amount, MaxIntensity - intensity));
        }

        public bool Remove(float amount)
        {
            ValidateFinite(amount, nameof(amount));
            return amount > 0f && SetIntensity(intensity - amount);
        }

        public bool SetIntensity(float value)
        {
            ValidateFinite(value, nameof(value));
            float clamped = Math.Min(Math.Max(value, 0f), MaxIntensity);
            if (intensity == clamped) return false;
            intensity = clamped;
            Changed?.Invoke(Current);
            return true;
        }

        public bool Clear() => SetIntensity(0f);

        /// <summary>Percentage of maximum health lost per second.</summary>
        public static float CalculateDamageRate(float value)
        {
            ValidateFinite(value, nameof(value));
            float intensity = Math.Min(Math.Max(value, 0f), MaxIntensity);
            if (intensity < DamageThreshold) return 0f;
            float position = (intensity - DamageThreshold) / (MaxIntensity - DamageThreshold);
            return DamageRateAtThreshold *
                (float)Math.Pow(DamageRateAtMaximum / DamageRateAtThreshold, position);
        }

        private static void ValidateFinite(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(name, "Value must be finite.");
        }
    }

    public readonly struct PoisonSnapshot
    {
        public PoisonSnapshot(float intensity, float damageRate)
        {
            Intensity = intensity;
            DamageRate = damageRate;
        }

        public float Intensity { get; }
        public float DamageRate { get; }
        public bool IsActive => Intensity > 0f;
        public float Normalized => Intensity / PoisonState.MaxIntensity;
    }
}
