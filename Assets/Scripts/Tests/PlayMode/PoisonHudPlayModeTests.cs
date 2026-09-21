using System.Collections;
using Fungiiiii.Survival;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class PoisonHudPlayModeTests
    {
        [UnityTest]
        public IEnumerator PoisonHud_ReflectsApplicationAndHidesAtZero()
        {
            GameObject root = new GameObject(nameof(PoisonHud_ReflectsApplicationAndHidesAtZero));
            PoisonPrototypeController controller = root.AddComponent<PoisonPrototypeController>();

            yield return null;

            Assert.That(controller.IsPoisonVisible, Is.False);

            controller.ApplyPoison(25f);
            yield return null;

            Assert.That(controller.IsPoisonVisible, Is.True);
            Assert.That(controller.PoisonPercentage, Is.EqualTo(25f).Within(0.01f));

            controller.RemovePoison(25f);
            yield return null;

            Assert.That(controller.IsPoisonVisible, Is.False);
            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator PoisonPrototype_DealsDamageOnlyAfterThreshold()
        {
            GameObject root = new GameObject(nameof(PoisonPrototype_DealsDamageOnlyAfterThreshold));
            PoisonPrototypeController controller = root.AddComponent<PoisonPrototypeController>();

            yield return null;

            controller.ApplyPoison(50f);
            controller.Simulate(1f);

            Assert.That(controller.CurrentHealth, Is.EqualTo(99.8f).Within(0.0001f));
            Object.Destroy(root);
        }

#if UNITY_EDITOR
        [UnityTest]
        public IEnumerator PoisonPrototypeScene_BootstrapsThePrototypeController()
        {
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/Prototype/PoisonScene.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return load;
            yield return null;

            PoisonPrototypeController controller = Object.FindFirstObjectByType<PoisonPrototypeController>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.IsPoisonVisible, Is.False);
            Assert.That(Camera.main, Is.Not.Null);
        }
#endif
    }
}
