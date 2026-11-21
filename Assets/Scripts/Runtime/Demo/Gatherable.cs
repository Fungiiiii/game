using System;
using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Demo
{
    /// <summary>
    /// Something the player gathers with the interact control: a decor mushroom for spores,
    /// a stick for wood. It appears on a spot of its <see cref="GatherableField"/>, shows an
    /// outline so the player sees it can be taken, and disappears once gathered.
    /// </summary>
    internal sealed class Gatherable : MonoBehaviour, IInteractable
    {
        private const float AimHeight = 0.3f;

        /// <summary>The key to press floats this far above the top of the model.</summary>
        private const float PromptAboveTop = 0.25f;

        private ItemDefinition item;
        private ItemOutline outline;
        private float reach;
        private Renderer[] renderers;

        /// <summary>Raised when the player has taken it, after it disappeared.</summary>
        public event Action<Gatherable> Gathered;

        public ItemDefinition Item => item;

        public bool IsAvailable { get; private set; }

        /// <summary>The spot of its field it stands on. Meaningless while it is not available.</summary>
        public int Spot { get; private set; }

        public InteractionCandidate Candidate =>
            new InteractionCandidate(transform.position + Vector3.up * AimHeight, reach, IsAvailable, ModelBounds());

        public Vector3 PromptPoint
        {
            get
            {
                var point = transform.position;
                point.y = Mathf.Max(point.y, ModelBounds().max.y) + PromptAboveTop;
                return point;
            }
        }

        /// <summary>The box around every renderer of the model, outline included, in world space.</summary>
        private Bounds ModelBounds()
        {
            var bounds = new Bounds(transform.position, Vector3.zero);
            foreach (var modelRenderer in renderers)
            {
                bounds.Encapsulate(modelRenderer.bounds);
            }

            return bounds;
        }

        public void Initialize(ItemDefinition gatheredItem, ItemOutline itemOutline, float interactionReach)
        {
            item = gatheredItem;
            outline = itemOutline;
            reach = interactionReach;
            renderers = GetComponentsInChildren<Renderer>(true);
            Disappear();
        }

        /// <summary>Shows it on a spot.</summary>
        public void Appear(int spot, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Spot = spot;
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = scale;
            outline.SetTargeted(false);
            IsAvailable = true;
            gameObject.SetActive(true);
        }

        /// <summary>Hides it without giving anything, for a reset.</summary>
        public void Disappear()
        {
            IsAvailable = false;
            gameObject.SetActive(false);
        }

        public void SetTargeted(bool targeted)
        {
            outline.SetTargeted(targeted && IsAvailable);
        }

        /// <summary>Puts one item in the inventory. Refused while gone, or if the inventory is full.</summary>
        public bool Interact(PlayerInventory inventory)
        {
            if (!IsAvailable || inventory.Add(item, 1) == 0)
            {
                return false;
            }

            Disappear();
            Gathered?.Invoke(this);
            return true;
        }
    }
}
