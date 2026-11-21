using System;
using UnityEngine;

namespace Fungiiiii.Inventory
{
    /// <summary>
    /// One kind of item, as a data asset: every stack of it in any inventory points to this
    /// same definition. The id is the stable gameplay key, saved and sent over the network;
    /// the name shown to the player is a key of the UI string table.
    /// </summary>
    [CreateAssetMenu(fileName = "Item", menuName = "Fungiiiii/Item Definition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [Tooltip("Stable gameplay key, saved and sent over the network. Never shown to the player.")]
        [SerializeField]
        private string id;

        [Tooltip("Key of the item's name in the UI string table.")]
        [SerializeField]
        private string nameKey;

        [Tooltip("What the interface shows for the item. Empty, it shows a square of the colour below.")]
        [SerializeField]
        private Sprite icon;

        [Tooltip("Colour of the placeholder square drawn until the item has an icon.")]
        [SerializeField]
        private Color color = Color.white;

        [SerializeField, Min(1)]
        private int maxStackSize = 1;

        [Tooltip("Optional: what the player holds while the item is selected in the quick bar.")]
        [SerializeField]
        private GameObject heldPrefab;

        public string Id => id;

        public string NameKey => nameKey;

        /// <summary>Null until the item's icon is drawn: the interface shows a square of <see cref="Color"/> instead.</summary>
        public Sprite Icon => icon;

        public Color Color => color;

        public int MaxStackSize => maxStackSize;

        /// <summary>Null when the item does nothing in hand.</summary>
        public GameObject HeldPrefab => heldPrefab;

        /// <summary>An item that exists only at runtime, for prototypes and tests.</summary>
        public static ItemDefinition Create(string id, Color color, int maxStackSize, string nameKey = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("An item id is required.", nameof(id));
            }

            if (maxStackSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxStackSize), "The maximum stack size must be positive.");
            }

            var item = CreateInstance<ItemDefinition>();
            item.name = id;
            item.id = id;
            item.nameKey = nameKey;
            item.color = color;
            item.maxStackSize = maxStackSize;
            return item;
        }
    }
}
