using System;
using System.Collections.Generic;
using Fungiiiii.Survival;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class PoisonStateTests
    {
        [Test]
        public void NewState_HasNoIntensityOrDamage()
        {
            var poison = new PoisonState();

            Assert.That(poison.Intensity, Is.Zero);
            Assert.That(poison.IsActive, Is.False);
            Assert.That(poison.DamageRate, Is.Zero);
        }

        [TestCase(49f, 0f)]
        [TestCase(50f, 0.2f)]
        [TestCase(75f, 1.4142135f)]
        [TestCase(100f, 10f)]
        public void DamageRate_FollowsTheThresholdAndExponentialCurve(float intensity, float expectedRate)
        {
            var poison = new PoisonState();
            poison.SetIntensity(intensity);

            Assert.That(PoisonState.CalculateDamageRate(intensity), Is.EqualTo(expectedRate).Within(0.0001f));
            Assert.That(poison.DamageRate, Is.EqualTo(expectedRate).Within(0.0001f));
        }

        [Test]
        public void IntensityChanges_ClampAndPublishOnlyChangedSnapshots()
        {
            var poison = new PoisonState();
            var snapshots = new List<PoisonSnapshot>();
            poison.Changed += snapshots.Add;

            poison.Apply(30f);
            poison.Apply(float.MaxValue);
            poison.SetIntensity(100f);
            poison.Remove(20f);
            poison.Remove(float.MaxValue);
            poison.Clear();

            Assert.That(snapshots, Has.Count.EqualTo(4));
            Assert.That(snapshots.ConvertAll(snapshot => snapshot.Intensity),
                Is.EqualTo(new[] { 30f, 100f, 80f, 0f }));
            Assert.That(snapshots[1].IsActive, Is.True);
            Assert.That(snapshots[1].Normalized, Is.EqualTo(1f));
            Assert.That(snapshots[1].DamageRate, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(snapshots[3].IsActive, Is.False);
            Assert.That(poison.Intensity, Is.Zero);
        }

        [Test]
        public void TinyExposure_CanBeObservedAndCompletelyCleared()
        {
            var poison = new PoisonState();
            var intensities = new List<float>();
            poison.Changed += snapshot => intensities.Add(snapshot.Intensity);

            poison.Apply(0.0000001f);
            Assert.That(poison.Intensity, Is.GreaterThan(0f));

            poison.Clear();

            Assert.That(poison.Intensity, Is.Zero);
            Assert.That(intensities, Is.EqualTo(new[] { 0.0000001f, 0f }));
        }

        [Test]
        public void OutOfRangeIntensity_ClampsForBothStateAndStaticCalculation()
        {
            var poison = new PoisonState();

            poison.SetIntensity(150f);
            Assert.That(poison.Intensity, Is.EqualTo(100f));
            Assert.That(PoisonState.CalculateDamageRate(150f), Is.EqualTo(10f).Within(0.0001f));

            poison.SetIntensity(-25f);
            Assert.That(poison.Intensity, Is.Zero);
            Assert.That(PoisonState.CalculateDamageRate(-25f), Is.Zero);
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void NonFiniteIntensityAndDose_AreRejectedWithoutChangingState(float invalid)
        {
            var poison = new PoisonState();
            poison.SetIntensity(25f);
            var changeCount = 0;
            poison.Changed += _ => changeCount++;

            Assert.Throws<ArgumentOutOfRangeException>(() => poison.Apply(invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => poison.Remove(invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => poison.SetIntensity(invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => PoisonState.CalculateDamageRate(invalid));

            Assert.That(poison.Intensity, Is.EqualTo(25f));
            Assert.That(changeCount, Is.Zero);
        }
    }
}
