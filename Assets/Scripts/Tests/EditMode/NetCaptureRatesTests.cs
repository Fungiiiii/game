using System;
using Fungiiiii.Capture;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class NetCaptureRatesTests
    {
        private const float Tolerance = 0.0001f;

        [TestCase(NetTargetProfile.Common, 0.4f)]
        [TestCase(NetTargetProfile.CommonWeakened, 0.8f)]
        [TestCase(NetTargetProfile.Rare, 0.1f)]
        public void SuccessChance_UsesTheRateOfEachProfile_OnAnAwareMushroom(NetTargetProfile profile, float expected)
        {
            var rates = NetCaptureRates.Default;

            Assert.That(rates.SuccessChance(profile, isUnaware: false), Is.EqualTo(expected).Within(Tolerance));
        }

        [TestCase(NetTargetProfile.Common, 0.6f)]
        [TestCase(NetTargetProfile.CommonWeakened, 1f)]
        [TestCase(NetTargetProfile.Rare, 0.3f)]
        public void SuccessChance_AddsTheSurpriseBonus_OnAnUnawareMushroom(NetTargetProfile profile, float expected)
        {
            var rates = NetCaptureRates.Default;

            Assert.That(rates.SuccessChance(profile, isUnaware: true), Is.EqualTo(expected).Within(Tolerance));
        }

        [Test]
        public void SuccessChance_NeverExceedsCertainty()
        {
            var rates = new NetCaptureRates(0.9f, 0.95f, 0.5f, 0.3f);

            Assert.That(rates.SuccessChance(NetTargetProfile.CommonWeakened, isUnaware: true), Is.EqualTo(1f));
        }

        [Test]
        public void Constructor_RefusesARateOutsideZeroToOne()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new NetCaptureRates(1.2f, 0.8f, 0.1f, 0.2f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new NetCaptureRates(0.4f, 0.8f, -0.1f, 0.2f));
        }

        [TestCase(0.1f, NetChanceBand.Low)]
        [TestCase(0.3f, NetChanceBand.Low)]
        [TestCase(0.35f, NetChanceBand.Medium)]
        [TestCase(0.4f, NetChanceBand.Medium)]
        [TestCase(0.6f, NetChanceBand.High)]
        [TestCase(1f, NetChanceBand.High)]
        public void Classify_SortsChancesIntoTheCircleColours(float chance, NetChanceBand expected)
        {
            Assert.That(NetCaptureRates.Classify(chance), Is.EqualTo(expected));
        }

        [Test]
        public void Roll_IsDeterministic_WithASeededGenerator()
        {
            var first = new Random(1234);
            var second = new Random(1234);

            for (var draw = 0; draw < 50; draw++)
            {
                Assert.That(NetCaptureRates.Roll(0.4f, first), Is.EqualTo(NetCaptureRates.Roll(0.4f, second)));
            }
        }

        [Test]
        public void Roll_AlwaysSucceedsAtCertainty_AndNeverAtZero()
        {
            var random = new Random(7);

            for (var draw = 0; draw < 50; draw++)
            {
                Assert.That(NetCaptureRates.Roll(1f, random), Is.True);
                Assert.That(NetCaptureRates.Roll(0f, random), Is.False);
            }
        }

        [Test]
        public void Roll_SucceedsAtRoughlyTheGivenRate()
        {
            var random = new Random(99);
            var successes = 0;
            const int draws = 10000;

            for (var draw = 0; draw < draws; draw++)
            {
                if (NetCaptureRates.Roll(0.4f, random))
                {
                    successes++;
                }
            }

            Assert.That(successes / (float)draws, Is.EqualTo(0.4f).Within(0.02f));
        }
    }
}
