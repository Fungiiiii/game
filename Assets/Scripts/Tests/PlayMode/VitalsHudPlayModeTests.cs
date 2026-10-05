using System.Collections;
using Fungiiiii.Survival;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class VitalsHudPlayModeTests
    {
        [UnityTest]
        public IEnumerator PoisonBar_ReflectsThePoisonPercentage()
        {
            var root = new GameObject(nameof(PoisonBar_ReflectsThePoisonPercentage));
            var model = new PlayerVitals(100f, 100f);
            var hud = root.AddComponent<VitalsHud>();
            hud.Initialize(model);

            yield return null;

            Assert.That(hud.HealthBarVisible, Is.True);
            Assert.That(hud.StaminaBarVisible, Is.True);
            Assert.That(hud.PoisonBarVisible, Is.False);

            model.Poison.SetIntensity(25f);
            yield return null;
            Assert.That(hud.PoisonBarVisible, Is.True);
            Assert.That(hud.PoisonPercentage, Is.EqualTo(25f).Within(0.01f));

            model.Poison.Clear();
            yield return null;
            Assert.That(hud.PoisonBarVisible, Is.False);

            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator PoisonStateChanges_RefreshTheDisplayedHud()
        {
            var root = new GameObject(nameof(PoisonStateChanges_RefreshTheDisplayedHud));
            var model = new PlayerVitals(100f, 100f);
            var hud = root.AddComponent<VitalsHud>();
            hud.Initialize(model);

            model.Poison.Apply(40f);
            yield return null;
            Assert.That(hud.PoisonBarVisible, Is.True);
            Assert.That(hud.PoisonPercentage, Is.EqualTo(40f).Within(0.01f));

            model.Poison.Remove(15f);
            yield return null;
            Assert.That(hud.PoisonBarVisible, Is.True);
            Assert.That(hud.PoisonPercentage, Is.EqualTo(25f).Within(0.01f));

            model.Poison.Clear();
            yield return null;
            Assert.That(hud.PoisonBarVisible, Is.False);
            Assert.That(hud.PoisonPercentage, Is.EqualTo(0f).Within(0.01f));

            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator HUDPrototypeScene_BootstrapsPrototype()
        {
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/Prototype/HUDScene.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return load;
            yield return null;

            VitalsHud hud = Object.FindFirstObjectByType<VitalsHud>();
            Assert.That(hud, Is.Not.Null);
            Assert.That(hud.HealthBarVisible, Is.True);
            Assert.That(hud.StaminaBarVisible, Is.True);
        }
    }
}
