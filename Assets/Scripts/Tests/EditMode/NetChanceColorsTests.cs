using Fungiiiii.Capture;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class NetChanceColorsTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void ZeroChance_IsPureRed_Impossible()
        {
            AssertColor(NetChanceColors.Evaluate(0f), NetChanceColors.Impossible);
        }

        [Test]
        public void NegativeOrUnknownChance_IsShownAsImpossible()
        {
            AssertColor(NetChanceColors.Evaluate(-0.2f), NetChanceColors.Impossible);
            AssertColor(NetChanceColors.Evaluate(float.NaN), NetChanceColors.Impossible);
        }

        [Test]
        public void ATinyChance_IsOrange_LowButPossible()
        {
            var color = NetChanceColors.Evaluate(0.01f);

            Assert.That(color.r, Is.EqualTo(NetChanceColors.Low.r).Within(0.02f));
            Assert.That(color.g, Is.EqualTo(NetChanceColors.Low.g).Within(0.02f));
            Assert.That(color.g, Is.GreaterThan(NetChanceColors.Impossible.g + 0.3f), "Must not read as red.");
        }

        [Test]
        public void HalfChance_IsYellow()
        {
            AssertColor(NetChanceColors.Evaluate(0.5f), NetChanceColors.Even);
        }

        [Test]
        public void FullChance_IsTheGreenestGreen()
        {
            AssertColor(NetChanceColors.Evaluate(1f), NetChanceColors.Certain);
            AssertColor(NetChanceColors.Evaluate(1.5f), NetChanceColors.Certain);
        }

        [Test]
        public void TheGradient_LosesRedAsTheChanceRises()
        {
            var previousRed = float.PositiveInfinity;

            for (var step = 1; step <= 20; step++)
            {
                var red = NetChanceColors.Evaluate(step / 20f).r;
                Assert.That(red, Is.LessThanOrEqualTo(previousRed + Tolerance));
                previousRed = red;
            }
        }

        private static void AssertColor(Color actual, Color expected)
        {
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(Tolerance));
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(Tolerance));
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(Tolerance));
        }
    }
}
