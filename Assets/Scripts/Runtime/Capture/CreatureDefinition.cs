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

        [Header("Hit by the slingshot")]
        [SerializeField, Min(0.1f)]
        private float maxHealth = 2f;

        [Tooltip("What it lets go of when felled wet. Empty: nothing.")]
        [SerializeField]
        private ItemDefinition wetDrop;

        [Header("Soaking up, calm: what it soaks up. Empty: it does not.")]
        [SerializeField]
        private ItemDefinition soaksUp;

        [Tooltip("How far it goes to soak something up, in metres.")]
        [SerializeField, Min(0.1f)]
        private float soakReach = 12f;

        [SerializeField, Min(0.1f)]
        private float soakSeconds = 1.5f;

        [Tooltip("It soaks up standing this close, in metres.")]
        [SerializeField, Min(0.1f)]
        private float soakArrivalDistance = 0.6f;

        [Tooltip("It soaks up only while the player is this close, so that the player sees it, in metres.")]
        [SerializeField, Min(0.1f)]
        private float soaksWhenPlayerWithin = 15f;

        [Tooltip("It starts to soak up only within this angle of the middle of the player's view, in degrees: the player looks at it.")]
        [SerializeField, Range(1f, 90f)]
        private float soaksWhenSeenWithinDegrees = 40f;

        [Tooltip("It notices the player within this angle of where it faces, in degrees; a crouched player outside it, behind it, goes unnoticed.")]
        [SerializeField, Range(10f, 180f)]
        private float seesWithinDegrees = 100f;

        [Tooltip("Furthest ground distance a crouched player squeezes it from, by hand, once it soaked something up, in metres.")]
        [SerializeField, Min(0.1f)]
        private float squeezeReach = 1.8f;

        [Tooltip("The material of the model's part that looks wet: the Spongeus's cap.")]
        [SerializeField]
        private Material wetPartMaterial;

        [Tooltip("Colour the wet part is multiplied by.")]
        [SerializeField]
        private Color wetTint = new Color(0.65f, 0.65f, 0.7f, 1f);

        [Tooltip("Sheen of the wet part, from 0 (matt) to 1.")]
        [SerializeField, Range(0f, 1f)]
        private float wetSmoothness = 0.4f;

        [Tooltip("What drips from it while it is soaked through, as from a full fern. Empty: nothing drips.")]
        [SerializeField]
        private ParticleSystem wetDrip;

        public GameObject Model => model;

        public NetTargetProfile Profile => profile;

        public ItemDefinition CaughtItem => caughtItem;

        public float WanderRadius => wanderRadius;

        public float WanderSpeed => wanderSpeed;

        public float WanderPauseSeconds => wanderPauseSeconds;

        public float FleeSpeed => fleeSpeed;

        public float MaxHealth => maxHealth;

        /// <summary>Null when a felled creature lets go of nothing.</summary>
        public ItemDefinition WetDrop => wetDrop;

        /// <summary>Null when it does not soak anything up.</summary>
        public ItemDefinition SoaksUp => soaksUp;

        public float SoakReach => soakReach;

        public float SoakSeconds => soakSeconds;

        public float SoakArrivalDistance => soakArrivalDistance;

        public float SoaksWhenPlayerWithin => soaksWhenPlayerWithin;

        public float SoaksWhenSeenWithinDegrees => soaksWhenSeenWithinDegrees;

        public float SeesWithinDegrees => seesWithinDegrees;

        public float SqueezeReach => squeezeReach;

        public Material WetPartMaterial => wetPartMaterial;

        public Color WetTint => wetTint;

        public float WetSmoothness => wetSmoothness;

        /// <summary>Null when nothing drips from it.</summary>
        public ParticleSystem WetDrip => wetDrip;

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
