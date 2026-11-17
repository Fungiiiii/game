using UnityEngine;

namespace Fungiiiii.Runtime.Audio
{
    /// <summary>
    /// Cross-fades the three looping beds of a biome as the listener walks from
    /// south to north. The zones of the Mystic Forest are bands along Z, so the
    /// listener's Z alone decides the mix: no trigger volumes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZoneAmbience : MonoBehaviour
    {
        [SerializeField, Tooltip("Whose position decides the mix. Usually the camera.")]
        private Transform listener;

        [SerializeField] private AudioSource south;
        [SerializeField] private AudioSource middle;
        [SerializeField] private AudioSource north;

        [SerializeField, Tooltip("World Z where the south bed starts to fade into the middle one.")]
        private float southFadeStart = -120f;

        [SerializeField, Tooltip("World Z where only the middle bed is left.")]
        private float southFadeEnd = -70f;

        [SerializeField, Tooltip("World Z where the middle bed starts to fade into the north one.")]
        private float northFadeStart = 130f;

        [SerializeField, Tooltip("World Z where only the north bed is left.")]
        private float northFadeEnd = 170f;

        [SerializeField, Range(0f, 1f), Tooltip("Volume of the south bed when it plays alone.")]
        private float southVolume = 0.5f;

        [SerializeField, Range(0f, 1f), Tooltip("Volume of the middle bed when it plays alone.")]
        private float middleVolume = 0.5f;

        [SerializeField, Range(0f, 1f), Tooltip("Volume of the north bed when it plays alone.")]
        private float northVolume = 0.5f;

        // The mix follows a position that changes every frame, and setting three
        // volumes costs nothing: an Update is the simplest honest way to do it.
        private void Update()
        {
            if (listener == null)
            {
                return;
            }

            ZoneMix mix = ZoneMix.At(listener.position.z, southFadeStart, southFadeEnd, northFadeStart, northFadeEnd);
            south.volume = mix.South * southVolume;
            middle.volume = mix.Middle * middleVolume;
            north.volume = mix.North * northVolume;
        }
    }
}
