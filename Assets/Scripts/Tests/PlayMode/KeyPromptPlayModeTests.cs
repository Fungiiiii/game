using System.Collections;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class KeyPromptPlayModeTests
    {
        private KeyPrompt prompt;

        [SetUp]
        public void SetUp()
        {
            prompt = new GameObject("Key Prompt").AddComponent<KeyPrompt>();
            prompt.Initialize(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(prompt.gameObject);
        }

        [UnityTest]
        public IEnumerator TheKey_PopsInWhenItAppears_ThenStaysAtFullSize()
        {
            prompt.Show(new Vector2(100f, 100f), "E");

            Assert.That(prompt.Scale, Is.LessThan(1f));

            yield return new WaitForSecondsRealtime(0.2f);
            prompt.Show(new Vector2(120f, 100f), "E");

            Assert.That(prompt.Scale, Is.EqualTo(1f).Within(0.001f), "Moving with its target made it pop again.");
        }

        [UnityTest]
        public IEnumerator PopIn_PlaysThePopAgain_OnAKeyAlreadyShown()
        {
            prompt.Show(new Vector2(100f, 100f), "E");
            yield return new WaitForSecondsRealtime(0.2f);

            prompt.PopIn();

            Assert.That(prompt.Scale, Is.LessThan(1f));

            yield return new WaitForSecondsRealtime(0.2f);

            Assert.That(prompt.Scale, Is.EqualTo(1f).Within(0.001f));
        }
    }
}
