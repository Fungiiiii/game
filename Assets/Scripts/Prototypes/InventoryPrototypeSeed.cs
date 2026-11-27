using Fungiiiii.Inventory;
using Fungiiiii.UI;
using UnityEngine;

namespace Fungiiiii.Prototypes
{
    /// <summary>
    /// Sample items for the standalone InventaireScene, which has no player: the screen
    /// makes its own inventory there, and this fills it so that every press can be tried.
    /// The game never uses it: a player's inventory starts as the game says.
    /// </summary>
    [RequireComponent(typeof(InventoryScreen))]
    [DisallowMultipleComponent]
    internal sealed class InventoryPrototypeSeed : MonoBehaviour
    {
        [SerializeField, Min(1)]
        private int mushroomStackSize = 5;

        // Start: the screen has built its inventory in its own Awake.
        private void Start()
        {
            var screen = GetComponent<InventoryScreen>();
            var inventory = screen.Inventory;
            var quickBar = screen.Layout.QuickBarFirstSlot;

            inventory.Add(ItemDefinition.Create("demo_mushroom", new Color(0.95f, 0.52f, 0.18f, 1f), mushroomStackSize), 6);
            inventory.Add(ItemDefinition.Create("demo_herb", new Color(0.25f, 0.78f, 0.35f, 1f), 10), 3);
            inventory.Add(ItemDefinition.Create("demo_spore", new Color(0.55f, 0.35f, 0.92f, 1f), 16), 8);
            inventory.Add(ItemDefinition.Create("demo_crystal", new Color(0.25f, 0.75f, 0.95f, 1f), 4), 2);

            // Three of them in the quick bar, so that moving items is visible at once.
            inventory.MoveOrSwap(0, quickBar);
            inventory.MoveOrSwap(2, quickBar + 1);
            inventory.MoveOrSwap(3, quickBar + 2);
        }
    }
}
