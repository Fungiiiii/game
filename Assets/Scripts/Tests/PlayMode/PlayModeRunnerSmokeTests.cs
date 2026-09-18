using System.Collections;
using Fungiiiii.Champimaison;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>
    /// Proves the PlayMode runner actually executes. An empty test project and a
    /// broken test runner look identical from the outside, so this assembly ships
    /// with a test from the start - see docs/unity-init.md.
    /// </summary>
    public sealed class PlayModeRunnerSmokeTests
    {
        [UnityTest]
        public IEnumerator GameObject_SurvivesAFrame()
        {
            var subject = new GameObject(nameof(GameObject_SurvivesAFrame));

            yield return null;

            Assert.That(subject, Is.Not.Null, "The PlayMode runner did not reach the next frame.");
            Object.Destroy(subject);
        }

        [UnityTest]
        public IEnumerator ChampimaisonPrototype_ExposesThePlantAndUpgradeFlow()
        {
            var subject = new GameObject(nameof(ChampimaisonPrototype_ExposesThePlantAndUpgradeFlow));
            ChampimaisonPrototype prototype = subject.AddComponent<ChampimaisonPrototype>();

            yield return null;

            Assert.That(prototype.State.Tier, Is.EqualTo(ChampimaisonTier.Empty));
            Assert.That(prototype.TryPlantSeed(), Is.True);
            Assert.That(prototype.State.Tier, Is.EqualTo(ChampimaisonTier.Tier0));
            Assert.That(prototype.TryUpgradeToTier1(), Is.True);
            Assert.That(prototype.State.Tier, Is.EqualTo(ChampimaisonTier.Tier1));

            Object.Destroy(subject);
        }
    }
}
