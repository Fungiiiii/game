#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using Fungiiiii.Survival;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class PoisonTestBenchTests
    {
        private GameObject root;
        private Scene loadedScene;

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (root != null) Object.Destroy(root);
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(loadedScene);
                while (!unload.isDone) yield return null;
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator ShortcutDispatcherAppliesRemovesPausesAndResets()
        {
            PoisonTestBenchController controller = CreateController();
            controller.HandleShortcut(Key.P);
            Assert.That(controller.Poison.Intensity, Is.EqualTo(10f));
            controller.HandleShortcut(Key.O);
            Assert.That(controller.Poison.Intensity, Is.Zero);
            controller.HandleShortcut(Key.Space);
            Assert.That(controller.IsPaused, Is.True);
            controller.HandleShortcut(Key.R);
            Assert.That(controller.IsPaused, Is.False);
            Assert.That(controller.CurrentHealth, Is.EqualTo(100f));

            Click("HelpButton");
            controller.HandleShortcut(Key.P);
            controller.HandleShortcut(Key.R);
            controller.HandleShortcut(Key.Space);
            Assert.That(controller.Poison.Intensity, Is.Zero);
            Assert.That(controller.IsPaused, Is.True);
            Click("CloseHelpButton");

            controller.SetPoison(100f);
            controller.Simulate(20f);
            controller.HandleShortcut(Key.O);
            Assert.That(controller.Poison.Intensity, Is.EqualTo(100f));
            controller.HandleShortcut(Key.R);
            Assert.That(controller.IsDead, Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ButtonsDriveAuthoritativeVitalsAndDiagnostics()
        {
            PoisonTestBenchController controller = CreateController();
            Click("ApplyButton");
            yield return null;
            Assert.That(controller.Poison.Intensity, Is.EqualTo(10f));
            Assert.That(controller.IsPoisonVisible, Is.True);
            Click("RemoveButton");
            Assert.That(controller.Poison.Intensity, Is.Zero);
            Assert.That(controller.IsPoisonVisible, Is.False);
            Click("BelowThresholdButton");
            Assert.That(controller.Poison.Intensity, Is.EqualTo(49f));
            controller.Simulate(1f);
            Assert.That(controller.CurrentHealth, Is.EqualTo(100f));
            Click("ThresholdButton");
            controller.Simulate(1f);
            Assert.That(controller.CurrentHealth, Is.EqualTo(99.8f).Within(0.001f));
            Click("MaximumPoisonButton");
            controller.Simulate(2f);
            yield return null;
            Assert.That(Label("HealthValue"), Does.Not.Contain(100f.ToString("0.0")));
            Assert.That(Label("PoisonValue"), Does.Contain("100"));
            Assert.That(Label("DamageValue"), Does.Contain(10f.ToString("0.00")));
            Click("PauseButton");
            controller.Simulate(2f);
            yield return null;
            Assert.That(controller.CurrentHealth, Is.EqualTo(79.8f).Within(0.001f));
            Assert.That(Label("StateLabel"), Is.EqualTo("PAUSED"));
            Click("CureButton");
            Assert.That(controller.Poison.Intensity, Is.Zero);
            Assert.That(controller.CurrentHealth, Is.EqualTo(79.8f).Within(0.001f));
            Click("ResetButton");
            yield return null;
            Assert.That(controller.CurrentHealth, Is.EqualTo(100f));
            Assert.That(controller.IsPaused, Is.False);
            Assert.That(controller.IsPoisonVisible, Is.False);
            Assert.That(Label("StateLabel"), Is.EqualTo("HEALTHY"));
        }

        [UnityTest]
        public IEnumerator DeathOffersRestartAndGuidePreservesPauseState()
        {
            PoisonTestBenchController controller = CreateController();
            PoisonPrototypeView view = root.GetComponent<PoisonPrototypeView>();
            Click("HelpButton");
            Assert.That(view.IsHelpVisible, Is.True);
            Assert.That(controller.IsPaused, Is.True);
            Click("CloseHelpButton");
            Assert.That(controller.IsPaused, Is.False);
            controller.TogglePause();
            Click("HelpButton");
            Click("CloseHelpButton");
            Assert.That(controller.IsPaused, Is.True);
            controller.TogglePause();
            controller.SetPoison(100f);
            controller.Simulate(20f);
            yield return null;
            Assert.That(view.IsDeathVisible, Is.True);
            Assert.That(controller.CurrentHealth, Is.Zero);
            Assert.That(Label("StateLabel"), Is.EqualTo("DEAD"));
            Click("RestartButton");
            yield return null;
            Assert.That(view.IsDeathVisible, Is.False);
            Assert.That(controller.CurrentHealth, Is.EqualTo(100f));
            Assert.That(controller.Poison.Intensity, Is.Zero);
        }

#if UNITY_EDITOR
        [UnityTest]
        public IEnumerator PoisonSceneBootstrapsTestBenchAndPlayerHud()
        {
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/Prototype/PoisonScene.unity",
                new LoadSceneParameters(LoadSceneMode.Additive));
            while (!load.isDone) yield return null;
            loadedScene = SceneManager.GetSceneByPath("Assets/Scenes/Prototype/PoisonScene.unity");
            Assert.That(loadedScene.IsValid() && loadedScene.isLoaded, Is.True);
            PoisonTestBenchController controller = null;
            foreach (GameObject candidate in loadedScene.GetRootGameObjects())
            {
                controller = candidate.GetComponent<PoisonTestBenchController>();
                if (controller != null) break;
            }
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.GetComponent<PoisonPrototypeView>(), Is.Not.Null);
            Assert.That(controller.GetComponent<VitalsHud>(), Is.Not.Null);
            Assert.That(controller.IsPoisonVisible, Is.False);
        }
#endif

        private PoisonTestBenchController CreateController()
        {
            root = new GameObject("PoisonTest");
            PoisonTestBenchController controller = root.AddComponent<PoisonTestBenchController>();
            controller.enabled = false;
            return controller;
        }

        private void Click(string name)
        {
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
            {
                if (button.name != name) continue;
                Assert.That(button.gameObject.activeInHierarchy && button.interactable, Is.True, name);
                button.onClick.Invoke();
                return;
            }
            Assert.Fail($"Missing test bench button: {name}");
        }

        private string Label(string name)
        {
            foreach (Text label in root.GetComponentsInChildren<Text>(true))
                if (label.name == name) return label.text;
            Assert.Fail($"Missing diagnostic label: {name}");
            return string.Empty;
        }
    }
}
#endif
