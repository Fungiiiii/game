using System;

namespace Fungiiiii.Survival
{
    /// <summary>Owns player health, stamina and a single poison state.</summary>
    public sealed class PlayerVitals
    {
        private float health;
        private float stamina;

        public PlayerVitals(float maxHealth, float maxStamina)
        {
            ValidatePositiveFinite(maxHealth, nameof(maxHealth));
            ValidatePositiveFinite(maxStamina, nameof(maxStamina));
            MaxHealth = maxHealth;
            MaxStamina = maxStamina;
            health = maxHealth;
            stamina = maxStamina;
            Poison = new PoisonState();
            Poison.Changed += _ => RaiseChanged();
        }

        public event Action<VitalsSnapshot> Changed;

        public float MaxHealth { get; }
        public float MaxStamina { get; }
        public PoisonState Poison { get; }
        public bool IsDead => health <= 0f;
        public float PoisonDamagePerSecond => MaxHealth * (Poison.DamageRate / 100f);
        public VitalsSnapshot Current => new VitalsSnapshot(
            health, MaxHealth, stamina, MaxStamina, Poison.Intensity);

        public void SetHealth(float value)
        {
            ValidateFinite(value, nameof(value));
            float clamped = Math.Min(Math.Max(value, 0f), MaxHealth);
            if (health == clamped) return;
            health = clamped;
            RaiseChanged();
        }

        public void SetStamina(float value)
        {
            ValidateFinite(value, nameof(value));
            float clamped = Math.Min(Math.Max(value, 0f), MaxStamina);
            if (stamina == clamped) return;
            stamina = clamped;
            RaiseChanged();
        }

        /// <summary>Applies the only poison damage tick to health.</summary>
        public void Tick(float deltaTime)
        {
            ValidateFinite(deltaTime, nameof(deltaTime));
            if (deltaTime <= 0f || IsDead || Poison.DamageRate <= 0f) return;
            double damage = (double)MaxHealth * (Poison.DamageRate / 100f) * deltaTime;
            SetHealth((float)Math.Max(0d, health - damage));
        }

        public void Reset()
        {
            bool changed = health != MaxHealth || stamina != MaxStamina;
            health = MaxHealth;
            stamina = MaxStamina;
            if (!Poison.Clear() && changed) RaiseChanged();
        }

        private void RaiseChanged() => Changed?.Invoke(Current);

        private static void ValidatePositiveFinite(float value, string name)
        {
            ValidateFinite(value, name);
            if (value <= 0f)
                throw new ArgumentOutOfRangeException(name, "Value must be greater than zero.");
        }

        private static void ValidateFinite(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(name, "Value must be finite.");
        }
    }

    public readonly struct VitalsSnapshot
    {
        public VitalsSnapshot(float health, float maxHealth, float stamina,
            float maxStamina, float poisonPercentage)
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
        public float PoisonNormalized => PoisonPercentage / PoisonState.MaxIntensity;
        public float HealthNormalized => Health / MaxHealth;
        public float StaminaNormalized => Stamina / MaxStamina;
    }
}
