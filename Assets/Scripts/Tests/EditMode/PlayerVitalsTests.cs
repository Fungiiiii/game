using System;
using System.Collections.Generic;
using Fungiiiii.Survival;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class PlayerVitalsTests
    {
        [Test]
        public void NewVitals_StartAtMaximumWithAnEmptyPoisonState()
        {
            var vitals = new PlayerVitals(100f, 80f);

            Assert.That(vitals.Poison, Is.Not.Null);
            Assert.That(vitals.Poison.Intensity, Is.Zero);
            Assert.That(vitals.Current.Health, Is.EqualTo(100f));
            Assert.That(vitals.Current.Stamina, Is.EqualTo(80f));
            Assert.That(vitals.Current.IsPoisoned, Is.False);
            Assert.That(vitals.IsDead, Is.False);
        }

        [Test]
        public void HealthAndStamina_ClampAndNotifyOnlyWhenTheirSnapshotChanges()
        {
            var vitals = new PlayerVitals(100f, 80f);
            var snapshots = new List<VitalsSnapshot>();
            vitals.Changed += snapshots.Add;

            vitals.SetHealth(-10f);
            vitals.SetHealth(-10f);
            vitals.SetStamina(0f);
            vitals.SetStamina(100f);

            Assert.That(snapshots, Has.Count.EqualTo(3));
            Assert.That(snapshots[0].Health, Is.Zero);
            Assert.That(snapshots[1].Stamina, Is.Zero);
            Assert.That(snapshots[2].Stamina, Is.EqualTo(80f));
            Assert.That(vitals.Current.HealthNormalized, Is.Zero);
            Assert.That(vitals.Current.StaminaNormalized, Is.EqualTo(1f));
        }

        [Test]
        public void PoisonChanges_PropagateAsVitalsSnapshotsExactlyOnce()
        {
            var vitals = new PlayerVitals(100f, 80f);
            var poison = vitals.Poison;
            var snapshots = new List<VitalsSnapshot>();
            vitals.Changed += snapshots.Add;

            poison.Apply(25f);
            poison.SetIntensity(25f);
            poison.Clear();

            Assert.That(vitals.Poison, Is.SameAs(poison));
            Assert.That(snapshots, Has.Count.EqualTo(2));
            Assert.That(snapshots[0].PoisonPercentage, Is.EqualTo(25f));
            Assert.That(snapshots[0].PoisonNormalized, Is.EqualTo(0.25f));
            Assert.That(snapshots[0].Health, Is.EqualTo(100f));
            Assert.That(snapshots[1].PoisonPercentage, Is.Zero);
            Assert.That(vitals.Current.IsPoisoned, Is.False);
        }

        [TestCase(49f, 200f)]
        [TestCase(50f, 199.6f)]
        [TestCase(75f, 197.17157f)]
        [TestCase(100f, 180f)]
        public void Tick_AppliesPoisonRateToMaximumHealth(float intensity, float expectedHealth)
        {
            var vitals = new PlayerVitals(200f, 80f);
            vitals.Poison.SetIntensity(intensity);

            vitals.Tick(1f);

            Assert.That(vitals.PoisonDamagePerSecond,
                Is.EqualTo(200f - expectedHealth).Within(0.0002f));
            Assert.That(vitals.Current.Health, Is.EqualTo(expectedHealth).Within(0.0002f));
        }

        [Test]
        public void SkippingTick_PreservesHealthUntilSimulationResumes()
        {
            var vitals = new PlayerVitals(100f, 80f);
            vitals.Poison.SetIntensity(100f);

            // A paused owner does not call Tick; changing poison alone never damages health.
            Assert.That(vitals.Current.Health, Is.EqualTo(100f));

            vitals.Tick(1f);

            Assert.That(vitals.Current.Health, Is.EqualTo(90f).Within(0.0001f));
        }

        [Test]
        public void Cure_StopsFutureDamageWithoutRestoringLostHealth()
        {
            var vitals = new PlayerVitals(100f, 80f);
            vitals.Poison.SetIntensity(100f);
            vitals.Tick(1f);

            vitals.Poison.Clear();
            vitals.Tick(2f);

            Assert.That(vitals.Current.Health, Is.EqualTo(90f).Within(0.0001f));
            Assert.That(vitals.Current.IsPoisoned, Is.False);
        }

        [Test]
        public void DeathAndReset_RestoreVitalsAndClearPoison()
        {
            var vitals = new PlayerVitals(100f, 80f);
            vitals.SetStamina(15f);
            vitals.Poison.SetIntensity(100f);

            vitals.Tick(20f);
            Assert.That(vitals.IsDead, Is.True);
            Assert.That(vitals.Current.Health, Is.Zero);

            vitals.Tick(1f);
            Assert.That(vitals.Current.Health, Is.Zero);

            var resetSnapshots = new List<VitalsSnapshot>();
            vitals.Changed += resetSnapshots.Add;
            vitals.Reset();

            Assert.That(resetSnapshots, Has.Count.EqualTo(1));
            Assert.That(resetSnapshots[0].Health, Is.EqualTo(100f));
            Assert.That(resetSnapshots[0].Stamina, Is.EqualTo(80f));
            Assert.That(resetSnapshots[0].PoisonPercentage, Is.Zero);
            Assert.That(vitals.IsDead, Is.False);
            Assert.That(vitals.Current.Health, Is.EqualTo(100f));
            Assert.That(vitals.Current.Stamina, Is.EqualTo(80f));
            Assert.That(vitals.Poison.Intensity, Is.Zero);
        }

        [Test]
        public void Tick_SubdividedTimeMatchesOneLongStep()
        {
            var oneStep = new PlayerVitals(100f, 80f);
            var tenSteps = new PlayerVitals(100f, 80f);
            oneStep.Poison.SetIntensity(75f);
            tenSteps.Poison.SetIntensity(75f);

            oneStep.Tick(1f);
            for (var step = 0; step < 10; step++)
            {
                tenSteps.Tick(0.1f);
            }

            Assert.That(tenSteps.Current.Health, Is.EqualTo(oneStep.Current.Health).Within(0.0002f));
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void NonFiniteVitalsAndTime_AreRejected(float invalid)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerVitals(invalid, 80f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerVitals(100f, invalid));

            var vitals = new PlayerVitals(100f, 80f);
            Assert.Throws<ArgumentOutOfRangeException>(() => vitals.SetHealth(invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => vitals.SetStamina(invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => vitals.Tick(invalid));
        }
    }
}
