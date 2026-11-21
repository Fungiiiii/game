using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Runtime.Player
{
    /// <summary>
    /// What the player carries: their inventory belongs to them, not to the screen that
    /// shows it, so that it can later be saved or sent over the network with the player.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Backpack : MonoBehaviour
    {
        [Tooltip("Slots of the bag, before the quick bar.")]
        [SerializeField, Min(0)]
        private int bagSlotCount = 12;

        [Tooltip("Slots of the quick bar: its selected slot is the item in hand.")]
        [SerializeField, Min(1)]
        private int quickBarSlotCount = 3;

        private PlayerInventory inventory;

        /// <summary>Which slots are the bag and which the quick bar.</summary>
        public InventoryLayout Layout => new InventoryLayout(bagSlotCount, quickBarSlotCount);

        // Built on first use: the screens may ask for it before this component's Awake.
        public PlayerInventory Inventory => inventory ??= new PlayerInventory(Layout.SlotCount);
    }
}
