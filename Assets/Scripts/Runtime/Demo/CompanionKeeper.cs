using System;
using System.Collections.Generic;
using Fungiiiii.Companions;
using Fungiiiii.Inventory;
using Fungiiiii.Items;
using UnityEngine;

namespace Fungiiiii.Demo
{
    /// <summary>
    /// The companions of the level: which caught creatures can be taken out of the quick bar
    /// to follow the player, set in the Inspector, and the one out at the moment. One
    /// companion at most is out at a time. Taken out, it leaves the inventory and joins
    /// what the interact control can act on; taken back, it goes back.
    /// </summary>
    internal sealed class CompanionKeeper : MonoBehaviour
    {
        [Tooltip("The creatures that can be taken out, each a companion definition.")]
        [SerializeField]
        private CompanionDefinition[] companions = new CompanionDefinition[0];

        [Tooltip("How far in front of the player a companion is put down, in metres.")]
        [SerializeField, Min(0f)]
        private float releaseDistance = 1.5f;

        private readonly List<(ItemDefinition Item, IHeldItem Behaviour)> heldBehaviours = new List<(ItemDefinition, IHeldItem)>();
        private Transform player;
        private Transform view;
        private Func<Vector3, float> groundHeight;
        private PlayerInventory inventory;
        private IItemSelection selection;
        private ItemOutlineFactory outlines;
        private ICollection<IInteractable> interactables;
        private IReadOnlyList<IAbsorbable> sources;

        /// <summary>Raised when a companion is taken out or comes back.</summary>
        public event Action ActiveChanged;

        /// <summary>The companion out, or null.</summary>
        public Companion Active { get; private set; }

        /// <summary>What each companion's item does in hand: it takes the companion out. Register them on the player's hands.</summary>
        public IReadOnlyList<(ItemDefinition Item, IHeldItem Behaviour)> HeldBehaviours => heldBehaviours;

        /// <summary>What is missing for the component to work, or null when it is sound.</summary>
        public string FindProblem()
        {
            foreach (var companion in companions)
            {
                if (companion == null)
                {
                    return $"{name} has an empty companion.";
                }

                if (companion.FindProblem() is { } problem)
                {
                    return problem;
                }
            }

            return null;
        }

        /// <param name="followed">The player companions follow.</param>
        /// <param name="viewPoint">The player's view: a companion is put down in front of it.</param>
        /// <param name="ground">Height of the ground under a point.</param>
        /// <param name="playerInventory">Where companions come from and go back to.</param>
        /// <param name="itemSelection">The quick bar slot a companion is taken out of.</param>
        /// <param name="usable">What the interact control can act on: the companion out joins it.</param>
        /// <param name="absorbable">What companions could soak up.</param>
        public void Bind(
            Transform followed,
            Transform viewPoint,
            Func<Vector3, float> ground,
            PlayerInventory playerInventory,
            IItemSelection itemSelection,
            ItemOutlineFactory outlineFactory,
            ICollection<IInteractable> usable,
            IReadOnlyList<IAbsorbable> absorbable)
        {
            player = followed;
            view = viewPoint;
            groundHeight = ground;
            inventory = playerInventory;
            selection = itemSelection;
            outlines = outlineFactory;
            interactables = usable;
            sources = absorbable;

            heldBehaviours.Clear();
            foreach (var companion in companions)
            {
                heldBehaviours.Add((companion.Item, new CompanionInHand(this, companion)));
            }
        }

        /// <summary>
        /// Takes <paramref name="companion"/> out of the selected quick bar slot and puts it
        /// down in front of the player. Refused while a companion is already out, or when the
        /// slot does not hold it.
        /// </summary>
        public bool Release(CompanionDefinition companion)
        {
            var slot = selection.SelectedSlot;
            if (Active != null || slot.IsEmpty || slot.Item != companion.Item)
            {
                return false;
            }

            inventory.TakeFromSlot(slot.Index, 1, out _);
            Active = Spawn(companion);
            ActiveChanged?.Invoke();
            return true;
        }

        /// <summary>Removes the companion out without giving it back, for a reset that empties the inventory.</summary>
        public void Clear()
        {
            if (Active != null)
            {
                Remove(Active);
            }
        }

        private Companion Spawn(CompanionDefinition companion)
        {
            var forward = Vector3.ProjectOnPlane(view.forward, Vector3.up);
            forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
            var position = player.position + forward * releaseDistance;
            position.y = groundHeight(position);

            var root = new GameObject($"Companion ({companion.name})");
            root.transform.SetParent(transform, false);
            root.transform.SetPositionAndRotation(position, Quaternion.LookRotation(-forward));
            var model = Instantiate(companion.Creature.Model, root.transform, false);
            model.name = companion.Creature.Model.name;

            var spawned = root.AddComponent<Companion>();
            spawned.Initialize(companion, player, view, groundHeight, sources, outlines.Create(model));
            spawned.TakenBack += Remove;
            interactables.Add(spawned);
            return spawned;
        }

        private void Remove(Companion companion)
        {
            companion.TakenBack -= Remove;
            interactables.Remove(companion);
            Destroy(companion.gameObject);
            Active = null;
            ActiveChanged?.Invoke();
        }
    }
}
