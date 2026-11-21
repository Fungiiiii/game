using System;
using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using Fungiiiii.Runtime.World;
using Fungiiiii.UI;
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

        /// <summary>How long a mushroom takes to grow out of the ground.</summary>
        private const float GrowSeconds = 1f;

        /// <summary>The key to press floats this far above the top of the model.</summary>
        private const float PromptAboveTop = 0.25f;

        private ItemDefinition item;
        private ItemOutline outline;
        private float reach;
        private Renderer[] renderers;
        private GrowIn growIn;
        private FallIn fallIn;

        /// <summary>Raised when the player has taken it, after it disappeared.</summary>
        public event Action<Gatherable> Gathered;

        public ItemDefinition Item => item;

        public bool IsAvailable { get; private set; }

        /// <summary>The spot of its field it stands on. Meaningless while it is not available.</summary>
        public int Spot { get; private set; }

        public InteractionCandidate Candidate =>
            new InteractionCandidate(transform.position + Vector3.up * AimHeight, reach, IsAvailable, ModelBounds());

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
            growIn = gameObject.AddComponent<GrowIn>();
            fallIn = gameObject.AddComponent<FallIn>();
            Disappear();
        }

        /// <summary>Shows it on a spot.</summary>
        /// <summary>Puts it on a spot, posed as the spot says, at once.</summary>
        public void Appear(int spot, GatherableSpot pose)
        {
            // Taken back from a reset mid-arrival, it must not carry on from its last spot.
            growIn.enabled = false;
            fallIn.enabled = false;
            Spot = spot;
            transform.SetPositionAndRotation(pose.Position, pose.Rotation);
            transform.localScale = pose.Scale;
            outline.SetTargeted(false);
            IsAvailable = true;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Once it has appeared, makes it fall from <paramref name="dropHeight"/> metres, or
        /// grow out of the ground when there is no height: it may appear in the player's view.
        /// </summary>
        public void Arrive(float dropHeight, float tumbleSeed)
        {
            if (dropHeight > 0f)
            {
                fallIn.Play(dropHeight, tumbleSeed);
            }
            else
            {
                growIn.Play(GrowSeconds);
            }
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
