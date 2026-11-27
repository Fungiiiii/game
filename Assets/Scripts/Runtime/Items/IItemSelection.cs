using System;
using Fungiiiii.Inventory;

namespace Fungiiiii.Items
{
    /// <summary>The slot whose item the player holds: the selected slot of the quick bar.</summary>
    public interface IItemSelection
    {
        /// <summary>Raised when another slot is selected, or when what the selected slot holds changes.</summary>
        event Action SelectedSlotChanged;

        InventorySlot SelectedSlot { get; }
    }
}
