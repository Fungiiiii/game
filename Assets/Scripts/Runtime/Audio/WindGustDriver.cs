using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Runtime.Audio
{
    /// <summary>
    /// Makes the visible wind follow the gusts that are heard: while a gust sound
    /// plays, the global shader value <c>_WindGust</c> follows its loudness, and the
    /// grass and the trees lean with it. 0 the rest of the time.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RandomOneShotEmitter))]
    public sealed class WindGustDriver : MonoBehaviour
    {
        private static readonly int WindGustId = Shader.PropertyToID("_WindGust");

        [SerializeField, Min(0.02f), Tooltip("How finely the loudness of each gust is measured, in seconds.")]
        private float measureStep = 0.1f;

        private readonly Dictionary<AudioClip, GustEnvelope> envelopes = new Dictionary<AudioClip, GustEnvelope>();
        private RandomOneShotEmitter emitter;
        private GustEnvelope playing;
        private float playingSince;
        private float playingVolume;

        private void Awake()
        {
            emitter = GetComponent<RandomOneShotEmitter>();

            // Measured once, here, rather than on the frame a gust starts.
            foreach (AudioClip clip in emitter.Clips)
            {
                EnvelopeOf(clip);
            }
        }

        private void OnEnable()
        {
            emitter.Played += Follow;
        }

        private void OnDisable()
        {
            emitter.Played -= Follow;
            playing = null;

            // The value is global: leave none behind, in the Editor or the next scene.
            Shader.SetGlobalFloat(WindGustId, 0f);
        }

        // Runs every frame but only sets a value while a gust is heard.
        private void Update()
        {
            if (playing == null)
            {
                return;
            }

            float elapsed = Time.time - playingSince;

            if (elapsed >= playing.Duration)
            {
                playing = null;
                Shader.SetGlobalFloat(WindGustId, 0f);
                return;
            }

            Shader.SetGlobalFloat(WindGustId, playing.Evaluate(elapsed) * playingVolume);
        }

        /// <summary>
        /// Starts following a gust that has just started to play.
        /// </summary>
        internal void Follow(AudioClip clip, float volume)
        {
            playing = EnvelopeOf(clip);
            playingSince = Time.time;
            playingVolume = volume;
        }

        private GustEnvelope EnvelopeOf(AudioClip clip)
        {
            if (clip == null)
            {
                return null;
            }

            if (envelopes.TryGetValue(clip, out GustEnvelope envelope))
            {
                return envelope;
            }

            if (clip.loadState != AudioDataLoadState.Loaded)
            {
                clip.LoadAudioData();
            }

            var samples = new float[clip.samples * clip.channels];

            if (!clip.GetData(samples, 0))
            {
                // GetData needs the clip decompressed on load; a streamed clip cannot be read.
                Debug.LogWarning($"WindGustDriver cannot read '{clip.name}': set its Load Type to Decompress On Load. The wind will not follow it.", this);
                envelopes[clip] = null;
                return null;
            }

            envelope = GustEnvelope.FromSamples(samples, clip.channels, clip.frequency, measureStep);
            envelopes[clip] = envelope;
            return envelope;
        }
    }
}
