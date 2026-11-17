using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fungiiiii.Runtime.Audio
{
    /// <summary>
    /// Plays one of its clips now and then, after a random wait, and never the
    /// same clip twice in a row. Used for the gusts of the Mystic Forest.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class RandomOneShotEmitter : MonoBehaviour
    {
        [SerializeField] private AudioClip[] clips;

        [SerializeField, Min(0f), Tooltip("Shortest wait between two sounds, in seconds.")]
        private float minInterval = 25f;

        [SerializeField, Min(0f), Tooltip("Longest wait between two sounds, in seconds.")]
        private float maxInterval = 70f;

        [SerializeField, Range(0f, 1f)] private float minVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] private float maxVolume = 0.8f;

        private AudioSource source;
        private int previous = -1;

        /// <summary>
        /// Raised each time a sound starts, with the clip and the volume it plays at.
        /// </summary>
        public event Action<AudioClip, float> Played;

        /// <summary>The sounds this emitter picks from.</summary>
        public IReadOnlyList<AudioClip> Clips => clips ?? Array.Empty<AudioClip>();

        private void Awake()
        {
            source = GetComponent<AudioSource>();
        }

        // The coroutine belongs to this component: Unity stops it when the
        // component is disabled or destroyed, and OnEnable starts a fresh one.
        private void OnEnable()
        {
            StartCoroutine(PlayNowAndThen());
        }

        private IEnumerator PlayNowAndThen()
        {
            if (clips == null || clips.Length == 0)
            {
                yield break;
            }

            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
                previous = PickDifferent(clips.Length, previous, Random.value);
                float volume = Random.Range(minVolume, maxVolume);
                source.PlayOneShot(clips[previous], volume);
                Played?.Invoke(clips[previous], volume);
            }
        }

        /// <summary>
        /// Index of the next clip: any of them but the previous one.
        /// </summary>
        /// <param name="count">Number of clips, at least one.</param>
        /// <param name="previous">Index played last, or -1 if none was.</param>
        /// <param name="roll">A random number from 0 to 1.</param>
        internal static int PickDifferent(int count, int previous, float roll)
        {
            if (count <= 1)
            {
                return 0;
            }

            bool hasPrevious = previous >= 0 && previous < count;
            int choices = hasPrevious ? count - 1 : count;
            int pick = Mathf.Min((int)(roll * choices), choices - 1);
            return hasPrevious && pick >= previous ? pick + 1 : pick;
        }
    }
}
