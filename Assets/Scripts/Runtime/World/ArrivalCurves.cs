using UnityEngine;

namespace Fungiiiii.Runtime.World
{
    /// <summary>
    /// How things that appear in the world arrive, so that none pops in under the player's
    /// eyes: a mushroom grows out of the ground, a stick falls from the leaves.
    /// </summary>
    public static class ArrivalCurves
    {
        private const float Gravity = 9.81f;

        /// <summary>Share of its own size after <paramref name="elapsed"/> seconds of a growth lasting <paramref name="duration"/>: fast first, then settling.</summary>
        public static float Grow(float elapsed, float duration)
        {
            var progress = Mathf.Clamp01(elapsed / duration);
            var left = 1f - progress;
            return 1f - left * left * left;
        }

        /// <summary>How long a free fall from <paramref name="height"/> metres lasts, in seconds.</summary>
        public static float FallSeconds(float height)
        {
            return Mathf.Sqrt(2f * Mathf.Max(0f, height) / Gravity);
        }

        /// <summary>Height above the ground after <paramref name="elapsed"/> seconds of a free fall from <paramref name="height"/>.</summary>
        public static float HeightAbove(float elapsed, float height)
        {
            return Mathf.Max(0f, height - 0.5f * Gravity * elapsed * elapsed);
        }
    }
}
