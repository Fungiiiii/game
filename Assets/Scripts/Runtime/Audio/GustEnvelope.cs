using UnityEngine;

namespace Fungiiiii.Runtime.Audio
{
    /// <summary>
    /// How loud a sound is over time, from 0 (silence) to 1 (its loudest moment).
    /// Lets the visible wind rise and fall with the gust being heard.
    /// </summary>
    public sealed class GustEnvelope
    {
        private readonly float[] levels;
        private readonly float step;

        private GustEnvelope(float[] levels, float step, float duration)
        {
            this.levels = levels;
            this.step = step;
            Duration = duration;
        }

        /// <summary>Length of the sound, in seconds.</summary>
        public float Duration { get; }

        /// <summary>
        /// Measures a sound as it is stored in an <see cref="AudioClip"/>.
        /// </summary>
        /// <param name="samples">Samples, channels interleaved, as <see cref="AudioClip.GetData"/> returns them.</param>
        /// <param name="channels">Number of channels, at least one.</param>
        /// <param name="frequency">Samples per second and per channel.</param>
        /// <param name="step">Length of each measured slice, in seconds.</param>
        public static GustEnvelope FromSamples(float[] samples, int channels, int frequency, float step)
        {
            int frames = samples.Length / channels;
            int framesPerSlice = Mathf.Max(1, Mathf.RoundToInt(step * frequency));
            int sliceCount = Mathf.Max(1, (frames + framesPerSlice - 1) / framesPerSlice);
            var levels = new float[sliceCount];
            float peak = 0f;

            for (int slice = 0; slice < sliceCount; slice++)
            {
                int first = slice * framesPerSlice * channels;
                int end = Mathf.Min(first + framesPerSlice * channels, frames * channels);
                double sumOfSquares = 0;

                for (int i = first; i < end; i++)
                {
                    sumOfSquares += samples[i] * samples[i];
                }

                float rms = end > first ? Mathf.Sqrt((float)(sumOfSquares / (end - first))) : 0f;
                levels[slice] = rms;
                peak = Mathf.Max(peak, rms);
            }

            if (peak > 0f)
            {
                for (int slice = 0; slice < sliceCount; slice++)
                {
                    levels[slice] /= peak;
                }
            }

            return new GustEnvelope(levels, framesPerSlice / (float)frequency, frames / (float)frequency);
        }

        /// <summary>
        /// Loudness at a moment of the sound, from 0 to 1; 0 before it starts and after it ends.
        /// </summary>
        /// <param name="time">Seconds since the sound started.</param>
        public float Evaluate(float time)
        {
            if (time < 0f || time >= Duration)
            {
                return 0f;
            }

            // Each level describes the middle of its slice: blend the two around the moment.
            float position = Mathf.Clamp(time / step - 0.5f, 0f, levels.Length - 1);
            int below = (int)position;
            int above = Mathf.Min(below + 1, levels.Length - 1);
            return Mathf.Lerp(levels[below], levels[above], position - below);
        }
    }
}
