using UnityEngine;

namespace Fungiiiii.Survival
{
    /// <summary>The stamina rules (RG_SU_03, RG_SU_04) as data, in % of the maximum.</summary>
    [CreateAssetMenu(fileName = "StaminaSettings", menuName = "Fungiiiii/Stamina Settings")]
    public sealed class StaminaSettings : ScriptableObject
    {
        [SerializeField, Min(0f)]
        private float sprintCostPerSecond = 15f;

        [SerializeField, Min(0f)]
        private float regenerationPerSecond = 20f;

        [SerializeField, Min(0f)]
        private float regenerationDelaySeconds = 1f;

        [SerializeField, Min(0f)]
        private float recoveryThreshold = 20f;

        public StaminaRegulator.Settings ToRegulatorSettings()
        {
            return new StaminaRegulator.Settings(sprintCostPerSecond, regenerationPerSecond, regenerationDelaySeconds, recoveryThreshold);
        }
    }
}
