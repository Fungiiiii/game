#if UNITY_EDITOR
using System.Collections;
using Fungiiiii.Runtime.Audio;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class MysticForestWindPlayModeTests
    {
        private const string ScenePath = "Assets/Scenes/Biomes/MysticForest.unity";
        private static readonly int WindGustId = Shader.PropertyToID("_WindGust");

        private float timeScale;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            timeScale = Time.timeScale;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = timeScale;
        }

        [UnityTest]
        public IEnumerator TheVisibleWindRisesAndFallsWithAHeardGust()
        {
            WindGustDriver driver = Object.FindFirstObjectByType<WindGustDriver>();
            Assert.That(driver, Is.Not.Null, "the scene has no WindGustDriver");

            RandomOneShotEmitter emitter = driver.GetComponent<RandomOneShotEmitter>();
            Assert.That(emitter.Clips.Count, Is.GreaterThan(0), "the gust emitter has no clip");

            AudioClip clip = emitter.Clips[0];
            Assert.That(Shader.GetGlobalFloat(WindGustId), Is.Zero, "wind before any gust");

            // Ten times faster: the 7 to 8 s gust lasts under a second of test.
            Time.timeScale = 10f;
            driver.Follow(clip, 1f);
            float strongest = 0f;
            float end = Time.time + clip.length + 0.5f;

            while (Time.time < end)
            {
                yield return null;
                strongest = Mathf.Max(strongest, Shader.GetGlobalFloat(WindGustId));
            }

            Assert.That(strongest, Is.GreaterThan(0.9f), "the gust never blew at its loudest");
            Assert.That(Shader.GetGlobalFloat(WindGustId), Is.Zero, "wind left after the gust");
        }
    }
}
#endif
