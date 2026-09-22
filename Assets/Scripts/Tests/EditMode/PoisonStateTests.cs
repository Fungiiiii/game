using Fungiiiii.Survival;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class PoisonStateTests
    {
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void NonFiniteInputs_AreRejectedWithoutChangingState(float value)
        {
            PoisonState poison = new PoisonState(100f);
            poison.SetIntensity(75f);
            int changes = 0;
            poison.Changed += _ => changes++;

            Assert.That(() => new PoisonState(value), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => new PoisonSimulation(value), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => poison.Apply(value), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => poison.Remove(value), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => poison.SetIntensity(value), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => poison.TickDamage(value), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => PoisonState.CalculateDamageRate(value), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => PoisonState.CalculateDamagePerSecond(value, 100f), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => PoisonState.CalculateDamagePerSecond(100f, value), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(poison.Intensity, Is.EqualTo(75f));
            Assert.That(changes, Is.Zero);
            poison.Clear();
            Assert.That(() => poison.TickDamage(value), Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void NonPositiveHealth_IsRejectedByAllHealthEntryPoints(float value)
        {
            Assert.That(() => new PoisonState(value), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => new PoisonSimulation(value), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => PoisonState.CalculateDamagePerSecond(50f, value), Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void ExtremeFiniteInputs_ClampDosesAndSaturateDamage()
        {
            PoisonState poison = new PoisonState(float.MaxValue);
            poison.Apply(75f);
            poison.Apply(float.MaxValue);
            Assert.That(poison.Intensity, Is.EqualTo(100f));
            Assert.That(poison.DamagePerSecond, Is.EqualTo(float.MaxValue * 0.1f));
            Assert.That(poison.TickDamage(float.MaxValue), Is.EqualTo(float.MaxValue));
            Assert.That(PoisonState.CalculateDamageRate(float.MinValue), Is.Zero);
            poison.Remove(float.MaxValue);
            Assert.That(poison.Intensity, Is.Zero);
        }

        [TestCase(0.00001f)]
        [TestCase(0.00005f)]
        public void TinyPositiveIntensity_CanBeAppliedAndClearedExactly(float intensity)
        {
            PoisonState poison = new PoisonState(100f);
            int changes = 0;
            poison.Changed += snapshot =>
            {
                changes++;
                Assert.That(snapshot.Intensity, Is.EqualTo(poison.Intensity));
            };
            Assert.That(poison.SetIntensity(intensity), Is.True);
            Assert.That(poison.IsActive, Is.True);
            Assert.That(poison.Clear(), Is.True);
            Assert.That(poison.IsActive, Is.False);
            Assert.That(poison.Clear(), Is.False);
            Assert.That(changes, Is.EqualTo(2));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void NonPositiveDosesAndTime_AreNoOps(float value)
        {
            PoisonSimulation simulation = new PoisonSimulation(100f);
            simulation.Poison.SetIntensity(100f);
            Assert.That(simulation.Poison.Apply(value), Is.False);
            Assert.That(simulation.Poison.Remove(value), Is.False);
            Assert.That(simulation.Poison.TickDamage(value), Is.Zero);
            simulation.Simulate(value);
            Assert.That(simulation.CurrentHealth, Is.EqualTo(100f));
        }

        [Test]
        public void Simulation_PauseFreezesHealthAndResumeContinuesDamage()
        {
            PoisonSimulation simulation = new PoisonSimulation(200f);
            simulation.Poison.SetIntensity(100f);
            simulation.Simulate(1f);
            Assert.That(simulation.CurrentHealth, Is.EqualTo(180f));
            simulation.TogglePause();
            Assert.That(simulation.IsPaused, Is.True);
            simulation.Simulate(10f);
            Assert.That(simulation.CurrentHealth, Is.EqualTo(180f));
            simulation.TogglePause();
            simulation.Simulate(1f);
            Assert.That(simulation.CurrentHealth, Is.EqualTo(160f));
        }

        [Test]
        public void Simulation_DeathStopsAtZeroAndResetRestoresTheSameModel()
        {
            PoisonSimulation simulation = new PoisonSimulation(float.MaxValue);
            PoisonState poison = simulation.Poison;
            poison.SetIntensity(100f);
            simulation.Simulate(float.MaxValue);
            Assert.That(simulation.CurrentHealth, Is.Zero);
            Assert.That(simulation.IsDead, Is.True);
            poison.Clear();
            simulation.Simulate(1f);
            Assert.That(simulation.IsDead, Is.True);
            poison.SetIntensity(100f);
            simulation.Simulate(float.MaxValue);
            Assert.That(simulation.CurrentHealth, Is.Zero);
            simulation.TogglePause();
            simulation.Reset();
            Assert.That(simulation.Poison, Is.SameAs(poison));
            Assert.That(simulation.CurrentHealth, Is.EqualTo(simulation.MaxHealth));
            Assert.That(simulation.IsDead, Is.False);
            Assert.That(simulation.IsPaused, Is.False);
            Assert.That(poison.IsActive, Is.False);
            poison.SetIntensity(100f);
            simulation.Simulate(1f);
            Assert.That(simulation.CurrentHealth, Is.LessThan(simulation.MaxHealth));
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void Simulation_InvalidTimeIsRejectedWhenClearPausedOrDead(float value)
        {
            PoisonSimulation simulation = new PoisonSimulation(100f);
            Assert.That(() => simulation.Simulate(value), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            simulation.TogglePause();
            Assert.That(() => simulation.Simulate(value), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(simulation.CurrentHealth, Is.EqualTo(100f));
            simulation.TogglePause();
            simulation.Poison.SetIntensity(100f);
            simulation.Simulate(10f);
            Assert.That(() => simulation.Simulate(value), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(simulation.CurrentHealth, Is.Zero);
        }

        [Test]
        public void Simulation_SubdividedTimeMatchesSingleStepAndClearStopsDamage()
        {
            PoisonSimulation single = new PoisonSimulation(100f);
            PoisonSimulation subdivided = new PoisonSimulation(100f);
            single.Poison.SetIntensity(75f);
            subdivided.Poison.SetIntensity(75f);
            single.Simulate(2f);
            for (int i = 0; i < 20; i++)
            {
                subdivided.Simulate(0.1f);
            }
            Assert.That(subdivided.CurrentHealth, Is.EqualTo(single.CurrentHealth).Within(0.0001f));
            subdivided.Poison.Clear();
            float health = subdivided.CurrentHealth;
            subdivided.Simulate(10f);
            Assert.That(subdivided.CurrentHealth, Is.EqualTo(health));
        }

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
