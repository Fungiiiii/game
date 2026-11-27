using System.Collections.Generic;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// What the player just collected, a short line each above the quick bar: the amount, such as +1, then the item's name.
    /// Collecting the same item again adds to its line; a line fades out after a moment,
    /// and only the last few stay, so that the feed never weighs on the screen.
    /// </summary>
    internal sealed class PickupFeed : MonoBehaviour
    {
        [Tooltip("The lines' parent; it lays them out, the newest at the bottom.")]
        [SerializeField]
        private RectTransform lines;

        [SerializeField]
        private PickupLine linePrefab;

        [SerializeField, Min(1)]
        private int maxLines = 3;

        [Tooltip("Seconds a line stays fully shown after its last item.")]
        [SerializeField, Min(0f)]
        private float shownSeconds = 2f;

        [SerializeField, Min(0.01f)]
        private float fadeSeconds = 0.4f;

        private readonly List<PickupLine> shown = new List<PickupLine>();
        private PlayerInventory inventory;

        /// <summary>The lines on screen, the oldest first.</summary>
        public IReadOnlyList<PickupLine> Lines => shown;

        /// <summary>Shows what is collected into this inventory from now on.</summary>
        public void Bind(PlayerInventory playerInventory)
        {
            inventory = playerInventory;
            inventory.Collected += Show;
        }

        private void Show(ItemDefinition item, int quantity)
        {
            var line = shown.Find(candidate => candidate.Item == item);
            if (line == null)
            {
                if (shown.Count >= maxLines)
                {
                    Destroy(shown[0].gameObject);
                    shown.RemoveAt(0);
                }

                line = Instantiate(linePrefab, lines);
                shown.Add(line);
            }

            line.Add(item, quantity);
        }

        private void Update()
        {
            for (var index = shown.Count - 1; index >= 0; index--)
            {
                if (!shown[index].Age(Time.deltaTime, shownSeconds, fadeSeconds))
                {
                    Destroy(shown[index].gameObject);
                    shown.RemoveAt(index);
                }
            }
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.Collected -= Show;
            }
        }
    }
}
