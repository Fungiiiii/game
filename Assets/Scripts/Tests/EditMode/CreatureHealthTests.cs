using Fungiiiii.Combat;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class CreatureHealthTests
    {
        [Test]
        public void ItFalls_OnceItsHealthRunsOut()
        {
            var health = new CreatureHealth(2f);
            var died = 0;
            health.Died += () => died++;

            Assert.That(health.Damage(1f), Is.False);
            Assert.That(health.IsDead, Is.False);

            Assert.That(health.Damage(1f), Is.True);
            Assert.That(health.IsDead, Is.True);
            Assert.That(died, Is.EqualTo(1));
        }

        [Test]
        public void AFelledCreature_IsNotFelledAgain()
        {
            var health = new CreatureHealth(1f);
            var died = 0;
            health.Died += () => died++;
            health.Damage(5f);

            Assert.That(health.Damage(1f), Is.False);
            Assert.That(health.Current, Is.Zero);
            Assert.That(died, Is.EqualTo(1));
        }
    }
}
