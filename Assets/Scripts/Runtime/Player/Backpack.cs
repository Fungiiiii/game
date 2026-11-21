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
        [Tooltip("Slots of the inventory: the bag and the quick bar together.")]
        [SerializeField, Min(1)]
        private int slotCount = 15;

        private PlayerInventory inventory;

        // Built on first use: the screens may ask for it before this component's Awake.
        public PlayerInventory Inventory => inventory ??= new PlayerInventory(slotCount);
    }
}
