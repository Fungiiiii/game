using NUnit.Framework;
using Fungiiiii.UI;

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
        public void PoisonState_ChangesOnlyWhenTheStateChanges()
        {
            var vitals = new PlayerVitals(100f, 100f);
            var changeCount = 0;
            vitals.Changed += _ => changeCount++;

            vitals.SetPoisoned(true);
            vitals.SetPoisoned(true);
            vitals.SetPoisoned(false);

            Assert.That(changeCount, Is.EqualTo(2));
            Assert.That(vitals.Current.IsPoisoned, Is.False);
        }
    }
}
