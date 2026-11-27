using Fungiiiii.Capture;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Companions
{
    /// <summary>
    /// A caught creature that can be taken out of the inventory to follow the player, as
    /// data: which creature, how it follows, what it soaks up and gives when squeezed, and
    /// which part of its model darkens once full. The Champonge carries water. A new
    /// companion is an asset, not code.
    /// </summary>
    [CreateAssetMenu(fileName = "Companion", menuName = "Fungiiiii/Companion Definition")]
    public sealed class CompanionDefinition : ScriptableObject
    {
        [Tooltip("The creature: its model, and the item it becomes once caught, which takes it out.")]
        [SerializeField]
        private CreatureDefinition creature;

        [Header("Following")]
        [Tooltip("Its place is this far behind the player, in metres.")]
        [SerializeField, Min(0f)]
        private float followDistance = 2.5f;

        [Tooltip("It rests as soon as it is this close to the player, in metres, wherever it is around them.")]
        [SerializeField, Min(0.1f)]
        private float stopDistance = 3f;

        [Tooltip("Its place is this far to the player's right (negative: left), in metres: it walks beside them.")]
        [SerializeField]
        private float sideOffset = 1f;

        [Tooltip("Resting, it sets off only once the player is further than this, in metres: a step or a turn does not move it.")]
        [SerializeField, Min(0f)]
        private float startDistance = 5f;

        [Tooltip("Its speed close to the player, in metres per second: the player walks at 4.")]
        [SerializeField, Min(0.1f)]
        private float walkSpeed = 3.5f;

        [Tooltip("Its top speed to catch up, in metres per second: the player sprints at 7.")]
        [SerializeField, Min(0.1f)]
        private float runSpeed = 8f;

        [Tooltip("How much faster it goes per metre it lags beyond the start distance.")]
        [SerializeField, Min(0f)]
        private float catchUpGain = 1f;

        [Tooltip("How quickly it speeds up and brakes, in metres per second squared.")]
        [SerializeField, Min(0.1f)]
        private float acceleration = 12f;

        [Tooltip("Further than this from the player, it comes back behind them, out of sight.")]
        [SerializeField, Min(1f)]
        private float catchUpDistance = 25f;

        [Tooltip("How close the player must be to take it back or squeeze it, in metres.")]
        [SerializeField, Min(0f)]
        private float reach = 2.5f;

        [Header("Soaking up")]
        [Tooltip("What it carries once full and gives when squeezed: it only goes for what yields this.")]
        [SerializeField]
        private ItemDefinition carriedItem;

        [Tooltip("How many of the carried item one squeeze gives.")]
        [SerializeField, Min(1)]
        private int carriedQuantity = 1;

        [Tooltip("It goes by itself for what it can soak up this close to the player, in metres.")]
        [SerializeField, Min(0f)]
        private float seekRadius = 10f;

        [Tooltip("How long soaking up takes.")]
        [SerializeField, Min(0f)]
        private float soakSeconds = 1f;

        [Tooltip("It starts soaking this close to what it soaks up, in metres.")]
        [SerializeField, Min(0.05f)]
        private float arrivalDistance = 0.6f;

        [Header("Looking alive")]
        [Tooltip("How fast it turns towards where it goes, in degrees per second.")]
        [SerializeField, Min(1f)]
        private float turnSpeed = 540f;

        [Tooltip("Height of its little hops while it walks, in metres.")]
        [SerializeField, Min(0f)]
        private float hopHeight = 0.07f;

        [Tooltip("How many hops it makes per metre walked.")]
        [SerializeField, Min(0f)]
        private float hopsPerMetre = 1.5f;

        [Header("Looking full")]
        [Tooltip("The material of the model's part that darkens once full: the Champonge's cap.")]
        [SerializeField]
        private Material fullPartMaterial;

        [Tooltip("Colour the full part is multiplied by once full.")]
        [SerializeField]
        private Color fullTint = new Color(0.65f, 0.65f, 0.7f, 1f);

        [Tooltip("Sheen of the full part once full, from 0 (matt) to 1: light, as the water fern's.")]
        [SerializeField, Range(0f, 1f)]
        private float fullSmoothness = 0.4f;

        [Tooltip("How long the part takes to go back to its colour once squeezed.")]
        [SerializeField, Min(0f)]
        private float drySeconds = 1f;

        public CreatureDefinition Creature => creature;

        /// <summary>The inventory item that takes it out, and that it becomes again when taken back.</summary>
        public ItemDefinition Item => creature != null ? creature.CaughtItem : null;

        public float Reach => reach;

        public float TurnSpeed => turnSpeed;

        public float HopHeight => hopHeight;

        public float HopsPerMetre => hopsPerMetre;

        public ItemDefinition CarriedItem => carriedItem;

        public int CarriedQuantity => carriedQuantity;

        public Material FullPartMaterial => fullPartMaterial;

        public Color FullTint => fullTint;

        public float FullSmoothness => fullSmoothness;

        public float DrySeconds => drySeconds;

        public CompanionTuning ToTuning()
        {
            return new CompanionTuning(
                followDistance,
                stopDistance,
                startDistance,
                sideOffset,
                walkSpeed,
                runSpeed,
                catchUpGain,
                acceleration,
                catchUpDistance,
                seekRadius,
                soakSeconds,
                arrivalDistance);
        }

        /// <summary>What is missing for it to be used, or null when it is sound.</summary>
        public string FindProblem()
        {
            if (creature == null)
            {
                return $"'{name}' has no creature.";
            }

            if (creature.FindProblem() is { } problem)
            {
                return problem;
            }

            if (carriedItem == null)
            {
                return $"'{name}' carries no item.";
            }

            if (fullPartMaterial == null)
            {
                return $"'{name}' has no material for the part that darkens once full.";
            }

            if (startDistance <= stopDistance || catchUpDistance <= startDistance)
            {
                return $"'{name}' must stop closer than it sets off, and set off closer than it catches up.";
            }

            return runSpeed < walkSpeed ? $"'{name}' runs slower than it walks." : null;
        }
    }
}
