using System.Collections;
using System.Collections.Generic;
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
        private readonly List<GameObject> roots = new List<GameObject>();
        private Scene loadedScene;

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach (GameObject root in roots)
                if (root != null) Object.Destroy(root);
            roots.Clear();
            if (loadedScene.IsValid() && loadedScene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(loadedScene);
            loadedScene = default;
            yield return null;
        }

        [UnityTest]
        public IEnumerator PoisonHud_ReflectsApplicationAndHidesAtZero()
        {
            GameObject root = new GameObject(nameof(PoisonHud_ReflectsApplicationAndHidesAtZero));
            roots.Add(root);
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
            roots.Add(root);
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
                new LoadSceneParameters(LoadSceneMode.Additive));
            yield return load;
            loadedScene = SceneManager.GetSceneByPath("Assets/Scenes/Prototype/PoisonScene.unity");
            yield return null;

            PoisonPrototypeController controller = null;
            Camera camera = null;
            foreach (GameObject sceneRoot in loadedScene.GetRootGameObjects())
            {
                if (controller == null) controller = sceneRoot.GetComponentInChildren<PoisonPrototypeController>();
                if (camera == null) camera = sceneRoot.GetComponentInChildren<Camera>();
            }
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.IsPoisonVisible, Is.False);
            Assert.That(camera, Is.Not.Null);
            Assert.That(camera.isActiveAndEnabled, Is.True);
            Assert.That(camera.targetTexture, Is.Null);
            Assert.That(camera.targetDisplay, Is.Zero);
        }
#endif
    }
}
