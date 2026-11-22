using System;

namespace Fungiiiii.Combat
{
    /// <summary>The health of a creature that can be hit: it falls once its health runs out.</summary>
    public sealed class CreatureHealth
    {
        public CreatureHealth(float maxHealth)
        {
            if (maxHealth <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHealth), "A creature needs some health.");
            }

            MaxHealth = maxHealth;
            Current = maxHealth;
        }

        /// <summary>Raised once, when the health runs out.</summary>
        public event Action Died;

        public float MaxHealth { get; }

        public float Current { get; private set; }

        public bool IsDead => Current <= 0f;

        /// <returns>True when this hit made it fall.</returns>
        public bool Damage(float amount)
        {
            if (amount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Damage cannot heal.");
            }

            if (IsDead || amount == 0f)
            {
                return false;
            }

            Current = Math.Max(0f, Current - amount);
            if (!IsDead)
            {
                return false;
            }

            Died?.Invoke();
            return true;
        }
    }
}
