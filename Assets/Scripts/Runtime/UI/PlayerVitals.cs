using System;
using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Minimal, event-driven vitals model used by the HUD prototype.
    /// </summary>
    public sealed class PlayerVitals
    {
        public const float MaxPoisonPercentage = 100f;
        public const float PoisonDamageThresholdPercentage = 50f;
        public const float PoisonDamageAtThresholdPercentageOfMaxHealthPerSecond = 0.2f;
        public const float PoisonDamageAtMaximumPercentageOfMaxHealthPerSecond = 10f;

        private float _health;
        private float _stamina;
        private float _poisonPercentage;

        public PlayerVitals(float maxHealth, float maxStamina)
        {
            if (maxHealth <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHealth), "Max health must be greater than zero.");
            }

            if (maxStamina <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(maxStamina), "Max stamina must be greater than zero.");
            }

            MaxHealth = maxHealth;
            MaxStamina = maxStamina;
            _health = maxHealth;
            _stamina = maxStamina;
        }

        public event Action<VitalsSnapshot> Changed;

        public float MaxHealth { get; }

        public float MaxStamina { get; }

        public VitalsSnapshot Current => new VitalsSnapshot(
            _health,
            MaxHealth,
            _stamina,
            MaxStamina,
            _poisonPercentage);

        public float PoisonPercentage => _poisonPercentage;

        public bool IsPoisoned => _poisonPercentage > 0f;

        /// <summary>
        /// Current poison damage in health points per second.
        /// </summary>
        public float PoisonDamagePerSecond => MaxHealth * CalculatePoisonDamageRate(_poisonPercentage) / 100f;

        public void SetHealth(float value)
        {
            var clampedValue = Mathf.Clamp(value, 0f, MaxHealth);
            if (Mathf.Approximately(_health, clampedValue))
            {
                return;
            }

            _health = clampedValue;
            RaiseChanged();
        }

        public void SetStamina(float value)
        {
            var clampedValue = Mathf.Clamp(value, 0f, MaxStamina);
            if (Mathf.Approximately(_stamina, clampedValue))
            {
                return;
            }

            _stamina = clampedValue;
            RaiseChanged();
        }

        public void SetPoisonPercentage(float value)
        {
            var clampedValue = Mathf.Clamp(value, 0f, MaxPoisonPercentage);
            if (Mathf.Approximately(_poisonPercentage, clampedValue))
            {
                return;
            }

            _poisonPercentage = clampedValue;
            RaiseChanged();
        }

        /// <summary>
        /// Compatibility helper for callers that still need an on/off poison state.
        /// New gameplay code should use SetPoisonPercentage instead.
        /// </summary>
        public void SetPoisoned(bool value)
        {
            SetPoisonPercentage(value ? MaxPoisonPercentage : 0f);
        }

        /// <summary>
        /// Applies the current poison damage for a simulation step.
        /// Poison starts damaging health at 50% and follows an exponential curve up to 100%.
        /// </summary>
        public void TickPoisonDamage(float deltaTime)
        {
            if (deltaTime <= 0f || PoisonDamagePerSecond <= 0f || _health <= 0f)
            {
                return;
            }

            SetHealth(_health - PoisonDamagePerSecond * deltaTime);
        }

        /// <summary>
        /// Returns poison damage as a percentage of max health per second.
        /// The curve is 0 below 50%, 0.2% at 50%, and 10% at 100%.
        /// </summary>
        public static float CalculatePoisonDamageRate(float poisonPercentage)
        {
            var clampedValue = Mathf.Clamp(poisonPercentage, 0f, MaxPoisonPercentage);
            if (clampedValue < PoisonDamageThresholdPercentage)
            {
                return 0f;
            }

            var curvePosition = Mathf.InverseLerp(
                PoisonDamageThresholdPercentage,
                MaxPoisonPercentage,
                clampedValue);
            var damageRangeMultiplier = PoisonDamageAtMaximumPercentageOfMaxHealthPerSecond /
                                        PoisonDamageAtThresholdPercentageOfMaxHealthPerSecond;
            return PoisonDamageAtThresholdPercentageOfMaxHealthPerSecond * Mathf.Pow(damageRangeMultiplier, curvePosition);
        }

        private void RaiseChanged()
        {
            Changed?.Invoke(Current);
        }
    }

    public readonly struct VitalsSnapshot
    {
        public VitalsSnapshot(
            float health,
            float maxHealth,
            float stamina,
            float maxStamina,
            float poisonPercentage)
        {
            Health = health;
            MaxHealth = maxHealth;
            Stamina = stamina;
            MaxStamina = maxStamina;
            PoisonPercentage = poisonPercentage;
        }

        public float Health { get; }

        public float MaxHealth { get; }

        public float Stamina { get; }

        public float MaxStamina { get; }

        public float PoisonPercentage { get; }

        public bool IsPoisoned => PoisonPercentage > 0f;

        public float PoisonNormalized => PoisonPercentage / PlayerVitals.MaxPoisonPercentage;

        public float HealthNormalized => Health / MaxHealth;

        public float StaminaNormalized => Stamina / MaxStamina;
    }
}
