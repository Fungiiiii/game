using System.Collections;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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

            model.SetPoisonPercentage(25f);
            yield return null;
            Assert.That(hud.PoisonBarVisible, Is.True);
            Assert.That(hud.PoisonPercentage, Is.EqualTo(25f).Within(0.01f));

            model.SetPoisonPercentage(0f);
            yield return null;
            Assert.That(hud.PoisonBarVisible, Is.False);

            Object.Destroy(root);
        }
    }
}
