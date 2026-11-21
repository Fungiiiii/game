using Fungiiiii.Survival;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class StaminaRegulatorTests
    {
        private const float Tolerance = 0.001f;

        private static PlayerVitals NewVitals()
        {
            return new PlayerVitals(maxHealth: 100f, maxStamina: 100f);
        }

        [Test]
        public void Sprinting_DrainsFifteenPercentPerSecond()
        {
            var vitals = NewVitals();
            var stamina = new StaminaRegulator(vitals, StaminaRegulator.Settings.Default);

            stamina.Tick(1f, sprinting: true);

            Assert.That(vitals.Current.Stamina, Is.EqualTo(85f).Within(Tolerance));
        }

        [Test]
        public void NoRegeneration_DuringTheFirstSecondAfterSpending()
        {
            var vitals = NewVitals();
            var stamina = new StaminaRegulator(vitals, StaminaRegulator.Settings.Default);
            stamina.Tick(1f, sprinting: true);

            stamina.Tick(0.9f, sprinting: false);

            Assert.That(vitals.Current.Stamina, Is.EqualTo(85f).Within(Tolerance));
        }

        [Test]
        public void Walking_RegeneratesTwentyPercentPerSecond_OnceTheDelayIsOver()
        {
            var vitals = NewVitals();
            var stamina = new StaminaRegulator(vitals, StaminaRegulator.Settings.Default);
            stamina.Tick(2f, sprinting: true);

            // Walking is not spending: 1 s of delay, then 0.5 s of regeneration.
            stamina.Tick(1.5f, sprinting: false);

            Assert.That(vitals.Current.Stamina, Is.EqualTo(80f).Within(Tolerance));
        }

        [Test]
        public void Regeneration_StopsAtTheMaximum()
        {
            var vitals = NewVitals();
            var stamina = new StaminaRegulator(vitals, StaminaRegulator.Settings.Default);
            stamina.Tick(1f, sprinting: true);

            stamina.Tick(10f, sprinting: false);

            Assert.That(vitals.Current.Stamina, Is.EqualTo(100f).Within(Tolerance));
        }

        [Test]
        public void AtZero_NoSprint_UntilTwentyPercent()
        {
            var vitals = NewVitals();
            var stamina = new StaminaRegulator(vitals, StaminaRegulator.Settings.Default);

            stamina.Tick(10f, sprinting: true);

            Assert.That(vitals.Current.Stamina, Is.EqualTo(0f));
            Assert.That(stamina.IsExhausted, Is.True);
            Assert.That(stamina.CanSprint, Is.False);

            // 1 s of delay, then 0.95 s at 20 % per second: 19 %, still below the threshold.
            stamina.Tick(1.95f, sprinting: false);
            Assert.That(stamina.CanSprint, Is.False);

            // Past the threshold by a margin, so float rounding cannot leave it a hair below.
            stamina.Tick(0.1f, sprinting: false);
            Assert.That(vitals.Current.Stamina, Is.EqualTo(21f).Within(Tolerance));
            Assert.That(stamina.IsExhausted, Is.False);
            Assert.That(stamina.CanSprint, Is.True);
        }

        [Test]
        public void HoldingSprintWhileExhausted_DoesNotBlockRegeneration()
        {
            var vitals = NewVitals();
            var stamina = new StaminaRegulator(vitals, StaminaRegulator.Settings.Default);
            stamina.Tick(10f, sprinting: true);

            stamina.Tick(2f, sprinting: true);

            Assert.That(vitals.Current.Stamina, Is.EqualTo(20f).Within(Tolerance));
        }

        [Test]
        public void Reset_ClearsExhaustion()
        {
            var vitals = NewVitals();
            var stamina = new StaminaRegulator(vitals, StaminaRegulator.Settings.Default);
            stamina.Tick(10f, sprinting: true);

            vitals.Reset();
            stamina.Reset();

            Assert.That(stamina.IsExhausted, Is.False);
            Assert.That(stamina.CanSprint, Is.True);
        }
    }
}
