using Fungiiiii.Runtime.Audio;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class AmbienceAudioTests
    {
        private const float SouthFadeStart = -120f;
        private const float SouthFadeEnd = -70f;
        private const float NorthFadeStart = 130f;
        private const float NorthFadeEnd = 170f;
        private const float Tolerance = 1e-4f;

        private static ZoneMix MixAt(float z)
        {
            return ZoneMix.At(z, SouthFadeStart, SouthFadeEnd, NorthFadeStart, NorthFadeEnd);
        }

        [TestCase(-244f, 1f, 0f, 0f, TestName = "OnlyTheSouthBedAtTheSpawn")]
        [TestCase(30f, 0f, 1f, 0f, TestName = "OnlyTheMiddleBedInTheMiddle")]
        [TestCase(215f, 0f, 0f, 1f, TestName = "OnlyTheNorthBedAtTheStandingStones")]
        public void OneBedAloneAwayFromTheFades(float z, float south, float middle, float north)
        {
            ZoneMix mix = MixAt(z);

            Assert.That(mix.South, Is.EqualTo(south).Within(Tolerance));
            Assert.That(mix.Middle, Is.EqualTo(middle).Within(Tolerance));
            Assert.That(mix.North, Is.EqualTo(north).Within(Tolerance));
        }

        [Test]
        public void TwoBedsShareAFadeEqually()
        {
            ZoneMix mix = MixAt((SouthFadeStart + SouthFadeEnd) * 0.5f);

            Assert.That(mix.South, Is.EqualTo(mix.Middle).Within(Tolerance));
            Assert.That(mix.South, Is.GreaterThan(0.5f));
            Assert.That(mix.North, Is.Zero);
        }

        [Test]
        public void LoudnessStaysEvenFromSouthToNorth()
        {
            for (float z = -300f; z <= 300f; z += 2.5f)
            {
                ZoneMix mix = MixAt(z);
                float power = mix.South * mix.South + mix.Middle * mix.Middle + mix.North * mix.North;

                Assert.That(power, Is.EqualTo(1f).Within(Tolerance), $"at z = {z}");
            }
        }

        [Test]
        public void NeverTheSameClipTwiceInARow()
        {
            const int Count = 4;

            for (int previous = -1; previous < Count; previous++)
            {
                for (float roll = 0f; roll <= 1f; roll += 0.05f)
                {
                    int pick = RandomOneShotEmitter.PickDifferent(Count, previous, roll);

                    Assert.That(pick, Is.InRange(0, Count - 1));
                    Assert.That(pick, Is.Not.EqualTo(previous));
                }
            }
        }

        [Test]
        public void EveryOtherClipCanBePicked()
        {
            var seen = new bool[3];

            for (float roll = 0f; roll < 1f; roll += 0.01f)
            {
                seen[RandomOneShotEmitter.PickDifferent(3, 1, roll)] = true;
            }

            Assert.That(seen, Is.EqualTo(new[] { true, false, true }));
        }

        [Test]
        public void ASingleClipIsAlwaysPicked()
        {
            Assert.That(RandomOneShotEmitter.PickDifferent(1, 0, 0.7f), Is.Zero);
        }
    }
}
