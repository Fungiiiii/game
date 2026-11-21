using System;
using Fungiiiii.Gathering;
using Fungiiiii.Interaction;
using Fungiiiii.Inventory;
using Fungiiiii.Outlines;
using Fungiiiii.UI;
using UnityEngine;

namespace Fungiiiii.Items
{
    /// <summary>
    /// A stack dropped from the inventory, lying on the ground. The interact control picks
    /// up as much of it as fits; it goes once nothing is left.
    /// </summary>
    internal sealed class DroppedItem : MonoBehaviour, IInteractable
    {
        private const float AimHeight = 0.2f;

        /// <summary>The key to press floats this far above the top of the model.</summary>
        private const float PromptAboveTop = 0.25f;

        private ItemOutline outline;
        private float reach;
        private Renderer[] renderers;

        /// <summary>Raised once the whole stack is picked up.</summary>
        public event Action<DroppedItem> PickedUp;

        public ItemDefinition Item { get; private set; }

        public int Quantity { get; private set; }

        public InteractionCandidate Candidate =>
            new InteractionCandidate(transform.position + Vector3.up * AimHeight, reach, Quantity > 0, ModelBounds());

        public string HintKey => UiText.HintGather;

        public Vector3 PromptPoint
        {
            get
            {
                var point = transform.position;
                point.y = Mathf.Max(point.y, ModelBounds().max.y) + PromptAboveTop;
                return point;
            }
        }

        public void Initialize(ItemDefinition item, int quantity, ItemOutline itemOutline, float interactionReach)
        {
            Item = item;
            Quantity = quantity;
            outline = itemOutline;
            reach = interactionReach;
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        public void SetTargeted(bool targeted)
        {
            outline.SetTargeted(targeted && Quantity > 0);
        }

        /// <summary>Collects as much of the stack as fits. Refused when nothing fits.</summary>
        public bool Interact(PlayerInventory inventory)
        {
            var added = Quantity > 0 ? inventory.Collect(Item, Quantity) : 0;
            if (added == 0)
            {
                return false;
            }

            Quantity -= added;
            if (Quantity == 0)
            {
                PickedUp?.Invoke(this);
            }

            return true;
        }

        private Bounds ModelBounds()
        {
            var bounds = new Bounds(transform.position, Vector3.zero);
            foreach (var modelRenderer in renderers)
            {
                bounds.Encapsulate(modelRenderer.bounds);
            }

            return bounds;
        }
    }
}
