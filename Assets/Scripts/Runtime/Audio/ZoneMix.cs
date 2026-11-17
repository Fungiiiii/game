using UnityEngine;

namespace Fungiiiii.Runtime.Audio
{
    /// <summary>
    /// Volumes of the three zone beds for a position along the south-north axis.
    ///
    /// The beds cross-fade at equal power: the squares of the three volumes always
    /// add up to one, so the loudness stays even while walking through a fade.
    /// </summary>
    internal readonly struct ZoneMix
    {
        private ZoneMix(float south, float middle, float north)
        {
            South = south;
            Middle = middle;
            North = north;
        }

        public float South { get; }

        public float Middle { get; }

        public float North { get; }

        /// <param name="z">World Z of the listener; south is negative.</param>
        /// <param name="southFadeStart">Z where the south bed starts to give way to the middle one.</param>
        /// <param name="southFadeEnd">Z where only the middle bed is left.</param>
        /// <param name="northFadeStart">Z where the middle bed starts to give way to the north one.</param>
        /// <param name="northFadeEnd">Z where only the north bed is left.</param>
        public static ZoneMix At(float z, float southFadeStart, float southFadeEnd, float northFadeStart, float northFadeEnd)
        {
            float intoMiddle = Mathf.InverseLerp(southFadeStart, southFadeEnd, z);
            float intoNorth = Mathf.InverseLerp(northFadeStart, northFadeEnd, z);

            float south = Mathf.Sqrt(1f - intoMiddle);
            float north = Mathf.Sqrt(intoNorth);
            float middle = Mathf.Sqrt(intoMiddle * (1f - intoNorth));
            return new ZoneMix(south, middle, north);
        }
    }
}
