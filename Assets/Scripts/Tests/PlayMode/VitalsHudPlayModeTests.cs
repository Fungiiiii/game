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
        public IEnumerator PoisonBar_IsHiddenUntilTheModelIsPoisoned()
        {
            var root = new GameObject(nameof(PoisonBar_IsHiddenUntilTheModelIsPoisoned));
            var model = new PlayerVitals(100f, 100f);
            var hud = root.AddComponent<VitalsHud>();
            hud.Initialize(model);

            yield return null;

            Assert.That(hud.HealthBarVisible, Is.True);
            Assert.That(hud.StaminaBarVisible, Is.True);
            Assert.That(hud.PoisonBarVisible, Is.False);

            model.SetPoisoned(true);
            yield return null;
            Assert.That(hud.PoisonBarVisible, Is.True);

            model.SetPoisoned(false);
            yield return null;
            Assert.That(hud.PoisonBarVisible, Is.False);

            Object.Destroy(root);
        }
    }
}
