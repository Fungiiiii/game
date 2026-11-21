using System.Collections;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class KeyPromptPlayModeTests
    {
        private static readonly Glyph KeyE = new Glyph(GlyphKind.Key, "E");

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
            prompt.Show(new Vector2(100f, 100f), KeyE);

            Assert.That(prompt.Scale, Is.LessThan(1f));

            yield return new WaitForSecondsRealtime(0.2f);
            prompt.Show(new Vector2(120f, 100f), KeyE);

            Assert.That(prompt.Scale, Is.EqualTo(1f).Within(0.001f), "Moving with its target made it pop again.");
        }

        [Test]
        public void APlayStationButton_IsDrawn_NotWritten()
        {
            prompt.Show(new Vector2(100f, 100f), new Glyph(GlyphKind.Face, null, FaceShape.Triangle));

            Assert.That(prompt.Label, Is.Empty);
            Assert.That(prompt.GetComponentInChildren<TriangleGlyphGraphic>().enabled, Is.True);

            prompt.Show(new Vector2(100f, 100f), KeyE);

            Assert.That(prompt.Label, Is.EqualTo("E"));
            Assert.That(prompt.GetComponentInChildren<TriangleGlyphGraphic>().enabled, Is.False);
        }

        [UnityTest]
        public IEnumerator PopIn_PlaysThePopAgain_OnAKeyAlreadyShown()
        {
            prompt.Show(new Vector2(100f, 100f), KeyE);
            yield return new WaitForSecondsRealtime(0.2f);

            prompt.PopIn();

            Assert.That(prompt.Scale, Is.LessThan(1f));

            yield return new WaitForSecondsRealtime(0.2f);

            Assert.That(prompt.Scale, Is.EqualTo(1f).Within(0.001f));
        }
    }
}
