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
    public sealed class MysticForestAudioPlayModeTests
    {
        private const string ScenePath = "Assets/Scenes/Biomes/MysticForest.unity";

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator OnlyTheSouthBedIsHeardAtTheSpawn()
        {
            ZoneAmbience ambience = Object.FindFirstObjectByType<ZoneAmbience>();
            Assert.That(ambience, Is.Not.Null, "the scene has no ZoneAmbience");

            AudioSource south = Bed(ambience, "Bed_South");
            AudioSource middle = Bed(ambience, "Bed_Middle");
            AudioSource north = Bed(ambience, "Bed_North");

            Assert.That(south.volume, Is.GreaterThan(0f));
            Assert.That(middle.volume, Is.Zero);
            Assert.That(north.volume, Is.Zero);
            yield break;
        }

        [UnityTest]
        public IEnumerator EverySoundHasItsClipAndLoopsWhereItShould()
        {
            foreach (AudioSource source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
            {
                bool isEmitter = source.TryGetComponent(out RandomOneShotEmitter _);

                Assert.That(source.clip != null || isEmitter, $"{source.name} has no clip");
                Assert.That(source.loop, Is.EqualTo(!isEmitter), $"{source.name} loop");
            }

            Assert.That(Object.FindFirstObjectByType<RandomOneShotEmitter>(), Is.Not.Null, "the scene has no gust emitter");
            yield break;
        }

        private static AudioSource Bed(ZoneAmbience ambience, string name)
        {
            Transform child = ambience.transform.Find(name);
            Assert.That(child, Is.Not.Null, $"{name} is missing");
            return child.GetComponent<AudioSource>();
        }
    }
}
#endif
