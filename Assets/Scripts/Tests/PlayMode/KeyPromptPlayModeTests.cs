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
            prompt = UiPrefabs.Instantiate<KeyPrompt>("KeyPrompt");
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
        public void WhatAPressAndAHoldDo_AreWrittenRightOfTheKey()
        {
            prompt.Show(new Vector2(300f, 300f), KeyE);
            prompt.SetActions(UiText.HintSqueeze, UiText.HintTakeBack);
            Canvas.ForceUpdateCanvases();

            Assert.That(prompt.PressActionKey, Is.EqualTo(UiText.HintSqueeze));
            Assert.That(prompt.HoldActionKey, Is.EqualTo(UiText.HintTakeBack));
            foreach (var label in prompt.GetComponentsInChildren<LocalizedLabel>())
            {
                if (label.EntryKey == UiText.HintSqueeze || label.EntryKey == UiText.HintTakeBack)
                {
                    var corners = new Vector3[4];
                    ((RectTransform)label.transform).GetWorldCorners(corners);
                    Assert.That(corners[0].x, Is.GreaterThan(prompt.ScreenPoint.x), $"{label.EntryKey} is not right of the key.");
                }
            }

            prompt.SetActions(UiText.HintGather, null);
            Assert.That(prompt.PressActionKey, Is.EqualTo(UiText.HintGather));
            Assert.That(prompt.HoldActionKey, Is.Empty, "A key that cannot be held says what holding does.");

            prompt.Hide();
            prompt.Show(new Vector2(300f, 300f), KeyE);
            Assert.That(prompt.PressActionKey, Is.Empty, "A hidden key kept its line.");
        }

        [UnityTest]
        public IEnumerator APulsingKey_GrowsAndShrinks_ToBeNoticed()
        {
            prompt.Pulses = true;
            prompt.Show(new Vector2(300f, 300f), KeyE);
            var smallest = float.MaxValue;
            var largest = float.MinValue;
            var start = Time.unscaledTime;
            while (Time.unscaledTime - start < 1.2f)
            {
                yield return null;
                smallest = Mathf.Min(smallest, prompt.Scale);
                largest = Mathf.Max(largest, prompt.Scale);
            }

            Assert.That(largest, Is.GreaterThan(1.03f));
            Assert.That(smallest, Is.LessThan(0.97f));
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

        [Test]
        public void AKeyToPress_HasTheTriangle_AndNoBorder()
        {
            prompt.Show(new Vector2(100f, 100f), KeyE);
            prompt.SetMarks(true, false, 0f);

            Assert.That(prompt.IsPressMarkVisible, Is.True, "No triangle over a key to press.");
            Assert.That(prompt.IsHoldBorderVisible, Is.False, "A key that cannot be held has a border.");
        }

        [Test]
        public void AKeyToHold_HasItsBorder_FilledAsFarAsItIsHeld_AndNoTriangle()
        {
            prompt.Show(new Vector2(100f, 100f), KeyE);
            prompt.SetMarks(false, true, 0f);

            Assert.That(prompt.IsHoldBorderVisible, Is.True, "No border before the key is held.");
            Assert.That(prompt.HoldProgress, Is.Zero);
            Assert.That(prompt.IsPressMarkVisible, Is.False);

            prompt.SetMarks(false, true, 0.4f);

            Assert.That(prompt.HoldProgress, Is.EqualTo(0.4f).Within(0.001f));
        }

        [Test]
        public void AKeyToPressAndToHold_HasBoth()
        {
            prompt.Show(new Vector2(100f, 100f), KeyE);
            prompt.SetMarks(true, true, 0f);

            Assert.That(prompt.IsPressMarkVisible, Is.True);
            Assert.That(prompt.IsHoldBorderVisible, Is.True);
        }

        [Test]
        public void AText_TakesTheKeysPlace_OnItsSmallPanel()
        {
            prompt.Show(new Vector2(100f, 100f), KeyE);
            prompt.SetMarks(true, false, 0f);

            prompt.ShowText(new Vector2(100f, 100f), UiText.LabelHoldSeed);

            Assert.That(prompt.IsTextVisible, Is.True);
            Assert.That(prompt.IsKeyVisible, Is.False);
            Assert.That(prompt.IsPressMarkVisible, Is.False, "The triangle of a hidden key is still reported.");
            Assert.That(prompt.TextKey, Is.EqualTo(UiText.LabelHoldSeed));
        }

        [UnityTest]
        public IEnumerator AText_IsReadOnItsPanel_WhichGrowsAroundIt()
        {
            prompt.ShowText(new Vector2(100f, 100f), UiText.LabelNeedsSeed);
            var panel = System.Linq.Enumerable.First(prompt.GetComponentsInChildren<RectTransform>(true), rect => rect.name == "TextPanel");
            var label = panel.GetComponentInChildren<UnityEngine.UI.Text>(true);
            for (var frame = 0; frame < 120 && string.IsNullOrEmpty(label.text); frame++)
            {
                yield return null;
            }

            yield return null;
            Assert.That(label.gameObject.activeInHierarchy, Is.True, "The text of the panel is hidden: only the bottom of the wood shows.");
            Assert.That(label.text, Is.Not.Empty, "The panel shows no text.");
            Assert.That(panel.rect.width, Is.GreaterThan(100f), "The panel did not grow around its text.");
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
