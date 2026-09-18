using NUnit.Framework;
using Fungiiiii.UI;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class PlayerVitalsTests
    {
        [Test]
        public void NewVitals_StartAtMaximumAndWithoutPoison()
        {
            var vitals = new PlayerVitals(100f, 80f);

            Assert.That(vitals.Current.Health, Is.EqualTo(100f));
            Assert.That(vitals.Current.Stamina, Is.EqualTo(80f));
            Assert.That(vitals.Current.IsPoisoned, Is.False);
        }

        [Test]
        public void Changes_AreClampedAndRaiseOneSnapshotEvent()
        {
            var vitals = new PlayerVitals(100f, 80f);
            var changeCount = 0;
            VitalsSnapshot lastSnapshot = default;
            vitals.Changed += snapshot =>
            {
                changeCount++;
                lastSnapshot = snapshot;
            };

            vitals.SetHealth(-10f);
            vitals.SetStamina(0f);
            vitals.SetStamina(100f);

            Assert.That(changeCount, Is.EqualTo(3));
            Assert.That(lastSnapshot.Stamina, Is.EqualTo(80f));
            Assert.That(lastSnapshot.StaminaNormalized, Is.EqualTo(1f));
        }

        [Test]
        public void PoisonPercentage_IsClampedAndExposesPoisonState()
        {
            var vitals = new PlayerVitals(100f, 100f);
            var changeCount = 0;
            vitals.Changed += _ => changeCount++;

            vitals.SetPoisonPercentage(125f);
            vitals.SetPoisonPercentage(125f);

            Assert.That(changeCount, Is.EqualTo(1));
            Assert.That(vitals.Current.PoisonPercentage, Is.EqualTo(100f));
            Assert.That(vitals.Current.IsPoisoned, Is.True);

            vitals.SetPoisonPercentage(-10f);

            Assert.That(changeCount, Is.EqualTo(2));
            Assert.That(vitals.Current.PoisonPercentage, Is.EqualTo(0f));
            Assert.That(vitals.Current.IsPoisoned, Is.False);
        }

        [Test]
        public void PoisonDamageRate_FollowsTheRequestedExponentialCurve()
        {
            Assert.That(PlayerVitals.CalculatePoisonDamageRate(49.9f), Is.EqualTo(0f));
            Assert.That(PlayerVitals.CalculatePoisonDamageRate(50f), Is.EqualTo(0.2f).Within(0.0001f));
            Assert.That(PlayerVitals.CalculatePoisonDamageRate(75f), Is.EqualTo(Mathf.Sqrt(2f)).Within(0.0001f));
            Assert.That(PlayerVitals.CalculatePoisonDamageRate(100f), Is.EqualTo(10f).Within(0.0001f));
        }

        [Test]
        public void PoisonDamage_UsesMaxHealthAndElapsedTime()
        {
            var vitals = new PlayerVitals(200f, 100f);
            vitals.SetPoisonPercentage(50f);

            vitals.TickPoisonDamage(1f);

            Assert.That(vitals.Current.Health, Is.EqualTo(199.6f).Within(0.0001f));
        }

        [Test]
        public void LegacyPoisonToggle_MapsToPercentageEndpoints()
        {
            var vitals = new PlayerVitals(100f, 100f);

            vitals.SetPoisoned(true);
            Assert.That(vitals.Current.PoisonPercentage, Is.EqualTo(100f));

            vitals.SetPoisoned(false);
            Assert.That(vitals.Current.PoisonPercentage, Is.EqualTo(0f));
        }
    }
}
