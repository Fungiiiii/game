using System.Collections;
using Fungiiiii.Capture;
using Fungiiiii.Champimaison;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

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

        [UnityTest]
        public IEnumerator ChampimaisonPrototypeScene_BootstrapsPrototype()
        {
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/Prototype/ChampimaisonScene.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return load;
            yield return null;

            Assert.That(Object.FindFirstObjectByType<ChampimaisonPrototype>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator CapturePrototypeScene_BootstrapsCapturePrototypeGeometry()
        {
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/Prototype/CaptureChampignonScene.unity",
                new LoadSceneParameters(LoadSceneMode.Single));

            yield return load;
            yield return null;

            Assert.That(Object.FindFirstObjectByType<MushroomCaptureDemo>(), Is.Not.Null);
            Assert.That(GameObject.Find("Mushroom Placeholder"), Is.Not.Null);
            Assert.That(GameObject.Find("Mushroom Cap"), Is.Not.Null);
            Assert.That(GameObject.Find("State Color Marker"), Is.Not.Null);
        }
    }
}
