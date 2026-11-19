using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Demo
{
    /// <summary>
    /// A spot the player gathers with the interact control: a decor mushroom for spores,
    /// a stick for wood. Its visual disappears once gathered and comes back at the same
    /// spot after a random delay. A cyan ring shows it can be gathered.
    /// </summary>
    internal sealed class Gatherable : MonoBehaviour, IDemoInteractable
    {
        private const float AimHeight = 0.3f;

        private InventoryItemDefinition item;
        private RespawningResource resource;
        private GameObject visual;
        private DemoGroundRing ring;
        private float reach;

        public InventoryItemDefinition Item => item;

        public bool IsAvailable => resource != null && resource.IsAvailable;

        public InteractionCandidate Candidate =>
            new InteractionCandidate(transform.position + Vector3.up * AimHeight, reach, IsAvailable);

        /// <param name="gatheredVisual">Hidden while the resource is gone. Not owned: it may be a scene object.</param>
        public void Initialize(
            InventoryItemDefinition gatheredItem,
            GameObject gatheredVisual,
            RespawningResource respawningResource,
            float interactionReach,
            float ringRadius,
            Material ringMaterial,
            System.Func<Vector3, float> groundHeight)
        {
            item = gatheredItem;
            visual = gatheredVisual;
            resource = respawningResource;
            reach = interactionReach;
            ring = new DemoGroundRing(transform, transform.position, ringRadius, ringMaterial, groundHeight);
            Show(true);
        }

        public void SetTargeted(bool targeted)
        {
            ring?.SetTargeted(targeted);
        }

        /// <summary>Puts one item in the inventory. Refused while gone, or if the inventory is full.</summary>
        public bool Interact(PlayerInventory inventory)
        {
            if (!IsAvailable || inventory.Add(item, 1) == 0)
            {
                return false;
            }

            resource.TryGather();
            Show(false);
            return true;
        }

        /// <summary>Back at once, for a demo reset.</summary>
        public void ResetResource()
        {
            resource.Reset();
            Show(true);
        }

        private void Update()
        {
            if (resource != null && resource.Tick(Time.deltaTime))
            {
                Show(true);
            }
        }

        private void Show(bool available)
        {
            if (visual != null)
            {
                visual.SetActive(available);
            }

            ring?.SetVisible(available);
            ring?.SetTargeted(false);
        }
    }
}
