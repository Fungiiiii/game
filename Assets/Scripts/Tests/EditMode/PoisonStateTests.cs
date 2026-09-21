using Fungiiiii.Survival;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class PoisonStateTests
    {
        [Test]
        public void NewState_StartsClearAndPublishesNormalizedSnapshot()
        {
            PoisonState poison = new PoisonState(100f);

            Assert.That(poison.Intensity, Is.EqualTo(0f));
            Assert.That(poison.IsActive, Is.False);
            Assert.That(poison.Current.Normalized, Is.EqualTo(0f));
            Assert.That(poison.DamagePerSecond, Is.EqualTo(0f));
        }

        [Test]
        public void Apply_AccumulatesAndClampsAtMaximum()
        {
            PoisonState poison = new PoisonState(100f);
            int changeCount = 0;
            poison.Changed += _ => changeCount++;

            Assert.That(poison.Apply(25f), Is.True);
            Assert.That(poison.Apply(100f), Is.True);
            Assert.That(poison.Intensity, Is.EqualTo(PoisonState.MaxIntensity));
            Assert.That(changeCount, Is.EqualTo(2));
        }

        [Test]
        public void RemoveAndClear_ExposeInactiveStateAtZero()
        {
            PoisonState poison = new PoisonState(100f);
            poison.Apply(75f);

            Assert.That(poison.Remove(25f), Is.True);
            Assert.That(poison.Intensity, Is.EqualTo(50f));
            Assert.That(poison.Clear(), Is.True);
            Assert.That(poison.IsActive, Is.False);
            Assert.That(poison.Remove(1f), Is.False);
        }

        [Test]
        public void DamageRate_UsesTheExponentialCurveFromTheThreshold()
        {
            Assert.That(PoisonState.CalculateDamageRate(49.9f), Is.EqualTo(0f));
            Assert.That(PoisonState.CalculateDamageRate(50f), Is.EqualTo(0.2f).Within(0.0001f));
            Assert.That(PoisonState.CalculateDamageRate(75f), Is.EqualTo(1.4142135f).Within(0.0001f));
            Assert.That(PoisonState.CalculateDamageRate(100f), Is.EqualTo(10f).Within(0.0001f));
        }

        [Test]
        public void TickDamage_UsesMaxHealthAndElapsedTimeWithoutMutatingIntensity()
        {
            PoisonState poison = new PoisonState(200f);
            poison.Apply(50f);

            float damage = poison.TickDamage(1f);

            Assert.That(damage, Is.EqualTo(0.4f).Within(0.0001f));
            Assert.That(poison.Intensity, Is.EqualTo(50f));
        }

        [Test]
        public void InvalidMaxHealth_IsRejected()
        {
            Assert.That(() => new PoisonState(0f), Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void NonFiniteIntensity_IsRejected()
        {
            PoisonState poison = new PoisonState(100f);

            Assert.That(() => poison.SetIntensity(float.NaN), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => poison.Apply(float.PositiveInfinity), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(poison.Intensity, Is.EqualTo(0f));
        }
    }
}
