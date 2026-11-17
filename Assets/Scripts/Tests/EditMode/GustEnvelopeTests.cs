using Fungiiiii.Runtime.Audio;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class GustEnvelopeTests
    {
        private const int Frequency = 1000;
        private const float Step = 0.1f;

        // A 440 Hz-like tone whose loudness rises to its peak in the middle and fades out.
        private static float[] SwellingTone(float seconds, int channels)
        {
            int frames = Mathf.RoundToInt(seconds * Frequency);
            var samples = new float[frames * channels];

            for (int frame = 0; frame < frames; frame++)
            {
                float loudness = Mathf.Sin(Mathf.PI * frame / frames);
                float value = loudness * Mathf.Sin(frame * 0.7f);

                for (int channel = 0; channel < channels; channel++)
                {
                    samples[frame * channels + channel] = value;
                }
            }

            return samples;
        }

        [Test]
        public void SilenceMakesNoGust()
        {
            GustEnvelope envelope = GustEnvelope.FromSamples(new float[2000], 1, Frequency, Step);

            Assert.That(envelope.Evaluate(1f), Is.Zero);
        }

        [Test]
        public void TheLoudestMomentReachesOne()
        {
            GustEnvelope envelope = GustEnvelope.FromSamples(SwellingTone(4f, 2), 2, Frequency, Step);

            Assert.That(envelope.Duration, Is.EqualTo(4f).Within(1e-4f));
            Assert.That(envelope.Evaluate(2f), Is.EqualTo(1f).Within(0.05f));
            Assert.That(envelope.Evaluate(0.05f), Is.LessThan(0.2f));
            Assert.That(envelope.Evaluate(1f), Is.GreaterThan(envelope.Evaluate(0.3f)));
        }

        [Test]
        public void NoGustBeforeOrAfterTheSound()
        {
            GustEnvelope envelope = GustEnvelope.FromSamples(SwellingTone(4f, 1), 1, Frequency, Step);

            Assert.That(envelope.Evaluate(-0.5f), Is.Zero);
            Assert.That(envelope.Evaluate(4f), Is.Zero);
            Assert.That(envelope.Evaluate(10f), Is.Zero);
        }

        [Test]
        public void ASoundOnOneChannelOnlyStillCounts()
        {
            float[] mono = SwellingTone(2f, 1);
            var stereo = new float[mono.Length * 2];

            for (int frame = 0; frame < mono.Length; frame++)
            {
                stereo[frame * 2 + 1] = mono[frame];
            }

            GustEnvelope envelope = GustEnvelope.FromSamples(stereo, 2, Frequency, Step);

            Assert.That(envelope.Evaluate(1f), Is.EqualTo(1f).Within(0.05f));
        }
    }
}
