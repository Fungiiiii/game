using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Fungiiiii.Capture;
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
        public IEnumerator CapturePrototypeScene_BootstrapsCapturePrototypeGeometry()
        {
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/Prototype/CaptureChampignonScene.unity",
                new LoadSceneParameters(LoadSceneMode.Single));

            yield return load;

            Assert.That(Object.FindFirstObjectByType<MushroomCaptureDemo>(), Is.Not.Null);
            Assert.That(GameObject.Find("Mushroom Placeholder"), Is.Not.Null);
            Assert.That(GameObject.Find("Mushroom Cap"), Is.Not.Null);
            Assert.That(GameObject.Find("State Color Marker"), Is.Not.Null);
        }
    }
}
