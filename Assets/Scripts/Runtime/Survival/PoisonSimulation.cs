using System;

namespace Fungiiiii.Survival
{
    /// <summary>Owns prototype health and time progression independently of Unity.</summary>
    public sealed class PoisonSimulation
    {
        public PoisonSimulation(float maxHealth)
        {
            Poison = new PoisonState(maxHealth);
            CurrentHealth = maxHealth;
        }

        public PoisonState Poison { get; }
        public float MaxHealth => Poison.MaxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;
        public bool IsPaused { get; private set; }

        public void TogglePause() => IsPaused = !IsPaused;

        public void Simulate(float deltaTime)
        {
            // Validate even when paused or dead.
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), "Elapsed time must be finite.");
            }

            if (IsPaused || IsDead || deltaTime <= 0f)
            {
                return;
            }

            CurrentHealth = Math.Max(0f, CurrentHealth - Poison.TickDamage(deltaTime));
        }

        public void Reset()
        {
            CurrentHealth = MaxHealth;
            IsPaused = false;
            Poison.Clear();
        }
    }
}
