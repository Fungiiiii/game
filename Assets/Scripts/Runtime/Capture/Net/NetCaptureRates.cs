using System;
using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// The fixed mushroom profiles of the net prototype. PROVISIONAL: the CaptureSystem
    /// will replace them with per-species data (MushroomData) and a real weakened state.
    /// </summary>
    public enum NetTargetProfile
    {
        Common,
        CommonWeakened,
        Rare
    }

    /// <summary>
    /// PROVISIONAL capture odds for the net, from the validated design note: one rate per
    /// prototype profile, plus a bonus when the mushroom has not noticed the player.
    /// The CaptureSystem's TryCapture will replace this table and its roll.
    /// </summary>
    [Serializable]
    public struct NetCaptureRates
    {
        [Range(0f, 1f)]
        [SerializeField]
        private float common;

        [Range(0f, 1f)]
        [SerializeField]
        private float commonWeakened;

        [Range(0f, 1f)]
        [SerializeField]
        private float rare;

        [Tooltip("Added to the rate when the mushroom has not noticed the player (0.2 = +20 points).")]
        [Range(0f, 1f)]
        [SerializeField]
        private float surpriseBonus;

        public NetCaptureRates(float common, float commonWeakened, float rare, float surpriseBonus)
        {
            this.common = ValidateRate(common, nameof(common));
            this.commonWeakened = ValidateRate(commonWeakened, nameof(commonWeakened));
            this.rare = ValidateRate(rare, nameof(rare));
            this.surpriseBonus = ValidateRate(surpriseBonus, nameof(surpriseBonus));
        }

        /// <summary>Starting values of the design note: 40 %, 80 %, 10 %, +20 points.</summary>
        public static NetCaptureRates Default => new NetCaptureRates(0.4f, 0.8f, 0.1f, 0.2f);

        public float Common => common;

        public float CommonWeakened => commonWeakened;

        public float Rare => rare;

        public float SurpriseBonus => surpriseBonus;

        public float RateFor(NetTargetProfile profile)
        {
            switch (profile)
            {
                case NetTargetProfile.Common:
                    return common;
                case NetTargetProfile.CommonWeakened:
                    return commonWeakened;
                case NetTargetProfile.Rare:
                    return rare;
                default:
                    throw new ArgumentOutOfRangeException(nameof(profile), profile, null);
            }
        }

        /// <summary>Chance of success, between 0 and 1, for one capture attempt.</summary>
        /// <param name="isUnaware">True if the mushroom has not noticed the player.</param>
        public float SuccessChance(NetTargetProfile profile, bool isUnaware)
        {
            var chance = RateFor(profile) + (isUnaware ? surpriseBonus : 0f);
            return Mathf.Clamp01(chance);
        }

        /// <summary>
        /// Draws one attempt. Inject a seeded generator for a deterministic draw.
        /// </summary>
        public static bool Roll(float chance, System.Random random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            return random.NextDouble() < chance;
        }

        private static float ValidateRate(float rate, string parameterName)
        {
            if (rate < 0f || rate > 1f || float.IsNaN(rate))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }

            return rate;
        }
    }
}
