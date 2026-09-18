using System;
using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Minimal, event-driven vitals model used by the HUD prototype.
    /// </summary>
    public sealed class PlayerVitals
    {
        private float _health;
        private float _stamina;
        private bool _isPoisoned;

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
            _isPoisoned);

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

        public void SetPoisoned(bool value)
        {
            if (_isPoisoned == value)
            {
                return;
            }

            _isPoisoned = value;
            RaiseChanged();
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
            bool isPoisoned)
        {
            Health = health;
            MaxHealth = maxHealth;
            Stamina = stamina;
            MaxStamina = maxStamina;
            IsPoisoned = isPoisoned;
        }

        public float Health { get; }

        public float MaxHealth { get; }

        public float Stamina { get; }

        public float MaxStamina { get; }

        public bool IsPoisoned { get; }

        public float HealthNormalized => Health / MaxHealth;

        public float StaminaNormalized => Stamina / MaxStamina;
    }
}
