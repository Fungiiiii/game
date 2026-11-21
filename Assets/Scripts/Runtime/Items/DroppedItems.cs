using System;
using System.Collections.Generic;
using Fungiiiii.Capture;
using Fungiiiii.Interaction;
using Fungiiiii.Inventory;
using Fungiiiii.Outlines;
using Fungiiiii.Runtime.World;
using Fungiiiii.UI;
using UnityEngine;

namespace Fungiiiii.Items
{
    /// <summary>
    /// What the player drops from the inventory: as in Minecraft or Terraria, the item's
    /// picture from the inventory falls from their hand and floats over the ground in front
    /// of them, outlined like what can be gathered, until they pick it up again.
    /// </summary>
    internal sealed class DroppedItems : MonoBehaviour
    {
        [Tooltip("How far in front of the player a dropped item lands, in metres.")]
        [SerializeField, Min(0f)]
        private float distance = 1.5f;

        [Tooltip("How close the player must be to pick it up, in metres.")]
        [SerializeField, Min(0f)]
        private float reach = 2f;

        [Tooltip("Side of the picture, in metres.")]
        [SerializeField, Min(0.05f)]
        private float pictureSize = 0.4f;

        [Tooltip("Height of the picture's centre above the ground, in metres.")]
        [SerializeField, Min(0f)]
        private float floatHeight = 0.35f;

        private readonly List<DroppedItem> items = new List<DroppedItem>();
        private readonly List<Material> materials = new List<Material>();
        private InventoryScreen screen;
        private Transform view;
        private Func<Vector3, float> groundHeight;
        private ItemOutlineFactory outlines;
        private ICollection<IInteractable> interactables;
        private System.Random random;

        /// <summary>The stacks lying on the ground.</summary>
        public IReadOnlyList<DroppedItem> Items => items;

        /// <summary>Drops what <paramref name="inventoryScreen"/> lets go of, in front of <paramref name="viewPoint"/>.</summary>
        /// <param name="usable">What the interact control can act on: each dropped stack joins it, and leaves it once picked up.</param>
        public void Bind(
            InventoryScreen inventoryScreen,
            Transform viewPoint,
            Func<Vector3, float> ground,
            ItemOutlineFactory outlineFactory,
            ICollection<IInteractable> usable,
            System.Random tumbles)
        {
            screen = inventoryScreen;
            view = viewPoint;
            groundHeight = ground;
            outlines = outlineFactory;
            interactables = usable;
            random = tumbles;
            screen.Dropped += OnDropped;
        }

        /// <summary>Puts a stack on the ground in front of the player, falling from their hand.</summary>
        public DroppedItem Drop(ItemDefinition item, int quantity)
        {
            var forward = Vector3.ProjectOnPlane(view.forward, Vector3.up);
            forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
            var landing = view.position + forward * distance;
            landing.y = groundHeight(landing);

            var dropped = new GameObject($"Dropped {item.name}").AddComponent<DroppedItem>();
            dropped.transform.SetParent(transform, false);
            dropped.transform.SetPositionAndRotation(landing, Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f));
            var model = CreatePicture(item, dropped.transform);
            dropped.Initialize(item, quantity, outlines.Create(model), reach);
            dropped.PickedUp += Remove;
            items.Add(dropped);
            interactables.Add(dropped);

            dropped.gameObject.AddComponent<FallIn>().Play(view.position.y - landing.y - 0.5f, (float)random.NextDouble());
            return dropped;
        }

        private void OnDropped(ItemDefinition item, int quantity)
        {
            Drop(item, quantity);
        }

        /// <summary>Removes every dropped stack, for a reset.</summary>
        public void Clear()
        {
            for (var index = items.Count - 1; index >= 0; index--)
            {
                Remove(items[index]);
            }
        }

        /// <summary>As in the inventory: the item's icon, or a square of its colour until it has one.</summary>
        private GameObject CreatePicture(ItemDefinition item, Transform parent)
        {
            var picture = GameObject.CreatePrimitive(PrimitiveType.Quad);
            picture.name = "Picture";
            Destroy(picture.GetComponent<Collider>());
            picture.transform.SetParent(parent, false);
            picture.transform.localScale = Vector3.one * pictureSize;
            picture.transform.localPosition = Vector3.up * floatHeight;

            var material = Primitives.CreateUnlitMaterial($"Dropped {item.name}", item.Icon != null ? Color.white : item.Color);
            if (item.Icon != null)
            {
                material.mainTexture = item.Icon.texture;
            }

            materials.Add(material);
            picture.GetComponent<Renderer>().sharedMaterial = material;
            picture.AddComponent<FacesCamera>().Initialize(view, (float)random.NextDouble());
            return picture;
        }

        private void Remove(DroppedItem dropped)
        {
            dropped.PickedUp -= Remove;
            items.Remove(dropped);
            interactables.Remove(dropped);
            Destroy(dropped.gameObject);
        }

        private void OnDestroy()
        {
            if (screen != null)
            {
                screen.Dropped -= OnDropped;
            }

            foreach (var material in materials)
            {
                Destroy(material);
            }
        }
    }
}
