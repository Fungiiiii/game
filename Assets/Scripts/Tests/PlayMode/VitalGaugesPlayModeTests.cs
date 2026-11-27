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
    public sealed class VitalGaugesPlayModeTests
    {
        [UnityTest]
        public IEnumerator PoisonGauge_ReflectsThePoisonPercentage()
        {
            var hud = UiPrefabs.Instantiate<VitalGauges>("VitalGauges");
            var root = hud.gameObject;
            var model = new PlayerVitals(100f, 100f);
            hud.Initialize(model);

            yield return null;

            Assert.That(hud.HealthGaugeVisible, Is.True);
            Assert.That(hud.StaminaGaugeVisible, Is.True);
            Assert.That(hud.PoisonGaugeVisible, Is.False);

            model.Poison.SetIntensity(25f);
            yield return null;
            Assert.That(hud.PoisonGaugeVisible, Is.True);
            Assert.That(hud.PoisonPercentage, Is.EqualTo(25f).Within(0.01f));

            model.Poison.Clear();
            yield return null;
            Assert.That(hud.PoisonGaugeVisible, Is.False);

            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator PoisonStateChanges_RefreshTheGauges()
        {
            var hud = UiPrefabs.Instantiate<VitalGauges>("VitalGauges");
            var root = hud.gameObject;
            var model = new PlayerVitals(100f, 100f);
            hud.Initialize(model);

            model.Poison.Apply(40f);
            yield return null;
            Assert.That(hud.PoisonGaugeVisible, Is.True);
            Assert.That(hud.PoisonPercentage, Is.EqualTo(40f).Within(0.01f));

            model.Poison.Remove(15f);
            yield return null;
            Assert.That(hud.PoisonGaugeVisible, Is.True);
            Assert.That(hud.PoisonPercentage, Is.EqualTo(25f).Within(0.01f));

            model.Poison.Clear();
            yield return null;
            Assert.That(hud.PoisonGaugeVisible, Is.False);
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

            VitalGauges hud = Object.FindFirstObjectByType<VitalGauges>();
            Assert.That(hud, Is.Not.Null);
            Assert.That(hud.HealthGaugeVisible, Is.True);
            Assert.That(hud.StaminaGaugeVisible, Is.True);
        }
    }
}
