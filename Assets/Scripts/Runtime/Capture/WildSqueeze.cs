using System;
using Fungiiiii.Interaction;
using Fungiiiii.Inventory;
using Fungiiiii.UI;
using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// A wild creature that soaked something up can be squeezed by hand, without the net:
    /// the player, crouched, comes up behind it while it has not noticed them, and a press
    /// of E gives what it carries. It then runs off, dry. Seen from the front, or with the
    /// player standing, nothing can be done: it notices them first.
    /// </summary>
    internal sealed class WildSqueeze : MonoBehaviour, IInteractable
    {
        private WildMushroom mushroom;
        private ItemDefinition gives;
        private Transform player;
        private float reach;
        private float height = 0.5f;
        private Action squeezed;

        /// <summary>Whether a press would squeeze it now.</summary>
        public bool CanBeSqueezed =>
            mushroom != null
            && mushroom.isActiveAndEnabled
            && mushroom.IsFree
            && mushroom.IsUnaware
            && mushroom.IsWet
            && mushroom.IsSneakedUpOn;

        public InteractionCandidate Candidate => new InteractionCandidate(transform.position + Vector3.up * (height * 0.5f), reach, CanBeSqueezed);

        /// <summary>On its body: its name shows above it.</summary>
        public Vector3 PromptPoint => transform.position + Vector3.up * (height * 0.5f);

        public string HintKey => CanBeSqueezed ? UiText.HintSqueeze : null;

        /// <param name="creature">The creature squeezed, on the same object.</param>
        /// <param name="item">What a squeeze gives: what it soaked up.</param>
        /// <param name="squeezer">The player, whom it runs from once squeezed.</param>
        /// <param name="squeezeReach">Furthest ground distance a squeeze is done from, in metres.</param>
        /// <param name="onSqueezed">Told after each squeeze.</param>
        public void Initialize(WildMushroom creature, ItemDefinition item, Transform squeezer, float squeezeReach, Action onSqueezed)
        {
            squeezed = onSqueezed;
            mushroom = creature != null ? creature : throw new ArgumentNullException(nameof(creature));
            gives = item != null ? item : throw new ArgumentNullException(nameof(item));
            player = squeezer != null ? squeezer : throw new ArgumentNullException(nameof(squeezer));
            reach = squeezeReach;
            height = ModelHeight.Of(creature.transform);
        }

        public void SetTargeted(bool targeted)
        {
        }

        public bool Interact(PlayerInventory inventory)
        {
            if (!CanBeSqueezed || inventory.Collect(gives, 1) == 0)
            {
                return false;
            }

            mushroom.Squeeze(player.position);
            squeezed?.Invoke();
            return true;
        }
    }
}
