using Fungiiiii.Game;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class ObjectiveRulesTests
    {
        private static Objective For(bool hasSeed = false, bool planted = false, int stage = 0, bool canEnter = false)
        {
            return ObjectiveRules.For(new ObjectiveState(hasSeed, planted, stage, canEnter));
        }

        [Test]
        public void AtTheStart_TheSeedIsToBeFound()
        {
            Assert.That(For(), Is.EqualTo(Objective.FindSeed));
        }

        [Test]
        public void WithTheSeed_ItIsToBePlanted()
        {
            Assert.That(For(hasSeed: true), Is.EqualTo(Objective.PlantSeed));
        }

        [Test]
        public void OncePlanted_TheSeedIsToBeWokenUp()
        {
            Assert.That(For(planted: true), Is.EqualTo(Objective.AwakenSeed));
        }

        [Test]
        public void OnceAwake_TheChampimaisonIsToBeGrown_WithoutAnObjectiveForTheNet()
        {
            // Since the playtest of 23/11: the net is a way to get water, not an objective.
            Assert.That(For(planted: true, stage: 1), Is.EqualTo(Objective.GrowChampimaison));
        }

        [Test]
        public void OnceOpen_TheChampimaisonIsToBeEntered()
        {
            Assert.That(For(planted: true, stage: 2, canEnter: true), Is.EqualTo(Objective.EnterChampimaison));
        }

        [Test]
        public void EveryObjective_HasItsOwnText()
        {
            var keys = new System.Collections.Generic.HashSet<string>();
            foreach (Objective objective in System.Enum.GetValues(typeof(Objective)))
            {
                Assert.That(keys.Add(ObjectiveRules.KeyFor(objective)), Is.True, $"{objective} shares its text.");
            }
        }
    }
}
