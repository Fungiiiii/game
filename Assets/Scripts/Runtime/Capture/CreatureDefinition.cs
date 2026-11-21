using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// One species of mushroom to catch with the net, as data: how it looks, how hard it is
    /// to catch, how it wanders and flees, and the item it becomes once caught. Where it
    /// lives comes from the level (<see cref="CreaturePopulation"/>).
    /// </summary>
    [CreateAssetMenu(fileName = "Creature", menuName = "Fungiiiii/Creature Definition")]
    public sealed class CreatureDefinition : ScriptableObject
    {
        [SerializeField]
        private GameObject model;

        [Tooltip("How hard the net catches it.")]
        [SerializeField]
        private NetTargetProfile profile = NetTargetProfile.Common;

        [Tooltip("What it becomes in the inventory once caught (RG_CA_03).")]
        [SerializeField]
        private ItemDefinition caughtItem;

        [Header("Wandering around its home")]
        [SerializeField, Min(0f)]
        private float wanderRadius = 4f;

        [SerializeField, Min(0f)]
        private float wanderSpeed = 0.8f;

        [SerializeField, Min(0f)]
        private float wanderPauseSeconds = 1.5f;

        [Tooltip("Speed at which it runs from the net, in metres per second.")]
        [SerializeField, Min(0f)]
        private float fleeSpeed = 4f;

        public GameObject Model => model;

        public NetTargetProfile Profile => profile;

        public ItemDefinition CaughtItem => caughtItem;

        public float WanderRadius => wanderRadius;

        public float WanderSpeed => wanderSpeed;

        public float WanderPauseSeconds => wanderPauseSeconds;

        public float FleeSpeed => fleeSpeed;

        /// <summary>What is missing for it to be used, or null when it is sound.</summary>
        public string FindProblem()
        {
            if (model == null)
            {
                return $"'{name}' has no model.";
            }

            return caughtItem == null ? $"'{name}' becomes no item once caught." : null;
        }
    }
}
