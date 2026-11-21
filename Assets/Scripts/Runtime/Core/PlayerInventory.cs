using System;
using System.Collections.Generic;

namespace Fungiiiii.Inventory
{
    /// <summary>
    /// Immutable view of one inventory slot.
    /// </summary>
    public readonly struct InventorySlot
    {
        internal InventorySlot(int index, ItemDefinition item, int quantity)
        {
            Index = index;
            Item = item;
            Quantity = quantity;
        }

        public int Index { get; }

        /// <summary>The item in the slot, null when it is empty.</summary>
        public ItemDefinition Item { get; }

        public int Quantity { get; }

        /// <summary>Id of the item in the slot, null when it is empty.</summary>
        public string ItemId => Item != null ? Item.Id : null;

        public bool IsEmpty => Item == null || Quantity <= 0;
    }

    /// <summary>
    /// Small, scene-independent inventory implementation for the player POC.
    /// A slot holds a definition and a quantity: two stacks are the same item when they
    /// point to the same <see cref="ItemDefinition"/>.
    /// </summary>
    public sealed class PlayerInventory
    {
        private readonly SlotData[] slots;

        public PlayerInventory(int slotCount)
        {
            if (slotCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotCount), "An inventory must have at least one slot.");
            }

            slots = new SlotData[slotCount];
        }

        public event Action Changed;

        /// <summary>Raised when items taken from the world are added, with how many were.</summary>
        public event Action<ItemDefinition, int> Collected;

        public int SlotCount => slots.Length;

        public InventorySlot GetSlot(int index)
        {
            ValidateIndex(index);
            var slot = slots[index];
            return slot.IsEmpty ? new InventorySlot(index, null, 0) : new InventorySlot(index, slot.Item, slot.Quantity);
        }

        public int Count(string itemId)
        {
            ValidateItemId(itemId);

            int total = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty && slots[i].Item.Id == itemId)
                {
                    total += slots[i].Quantity;
                }
            }

            return total;
        }

        /// <summary>
        /// Adds as many items as fit and returns the number that was added.
        /// Existing stacks of the item are filled before empty slots are used.
        /// </summary>
        public int Add(ItemDefinition item, int quantity)
        {
            ValidateItem(item);
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "The quantity must be positive.");
            }

            int remaining = quantity;
            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (slots[i].Item == item)
                {
                    remaining -= Fill(i, item, remaining);
                }
            }

            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (slots[i].IsEmpty)
                {
                    remaining -= Fill(i, item, remaining);
                }
            }

            int addedTotal = quantity - remaining;
            if (addedTotal > 0)
            {
                Changed?.Invoke();
            }

            return addedTotal;
        }

        /// <summary>
        /// Adds items taken from the world (gathered, caught, picked up) as <see cref="Add"/>
        /// does, and tells whoever shows the player what they collect. Moving items inside
        /// the inventory goes through <see cref="Add"/>, which tells nothing.
        /// </summary>
        public int Collect(ItemDefinition item, int quantity)
        {
            var added = Add(item, quantity);
            if (added > 0)
            {
                Collected?.Invoke(item, added);
            }

            return added;
        }

        /// <summary>
        /// Removes up to the requested quantity, starting with the last stack.
        /// Empty overflow slots are therefore released first.
        /// </summary>
        public int Remove(string itemId, int quantity)
        {
            ValidateItemId(itemId);
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "The quantity must be positive.");
            }

            int remaining = quantity;
            for (int i = slots.Length - 1; i >= 0 && remaining > 0; i--)
            {
                if (slots[i].IsEmpty || slots[i].Item.Id != itemId)
                {
                    continue;
                }

                int removed = Math.Min(remaining, slots[i].Quantity);
                slots[i].Quantity -= removed;
                remaining -= removed;
                if (slots[i].Quantity == 0)
                {
                    slots[i] = default;
                }
            }

            int removedTotal = quantity - remaining;
            if (removedTotal > 0)
            {
                Changed?.Invoke();
            }

            return removedTotal;
        }

        /// <summary>
        /// Takes up to the requested amount from one slot and returns the item it held.
        /// This is used by cursor-style inventory interactions such as split stacks.
        /// </summary>
        public int TakeFromSlot(int index, int quantity, out ItemDefinition item)
        {
            ValidateIndex(index);
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "The quantity must be positive.");
            }

            SlotData slot = slots[index];
            if (slot.IsEmpty)
            {
                item = null;
                return 0;
            }

            item = slot.Item;
            int taken = Math.Min(quantity, slot.Quantity);
            slot.Quantity -= taken;
            slots[index] = slot.Quantity > 0 ? slot : default;
            Changed?.Invoke();
            return taken;
        }

        /// <summary>
        /// Places as many items as fit in one slot and returns the amount placed.
        /// </summary>
        public int AddToSlot(int index, ItemDefinition item, int quantity)
        {
            ValidateIndex(index);
            ValidateItem(item);
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "The quantity must be positive.");
            }

            int added = Fill(index, item, quantity);
            if (added > 0)
            {
                Changed?.Invoke();
            }

            return added;
        }

        /// <summary>
        /// Consolidates the stacks of one item into the fewest possible slots,
        /// preferring the slot that was double-clicked.
        /// </summary>
        public bool StackAll(string itemId, int preferredSlot)
        {
            ValidateItemId(itemId);
            ValidateIndex(preferredSlot);

            var matchingIndexes = new List<int>();
            ItemDefinition item = null;
            int totalQuantity = 0;

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty || slots[i].Item.Id != itemId)
                {
                    continue;
                }

                // Not ??=: it would bypass Unity's null check on the asset (UNT0023).
                if (item == null)
                {
                    item = slots[i].Item;
                }

                matchingIndexes.Add(i);
                totalQuantity += slots[i].Quantity;
            }

            if (matchingIndexes.Count <= 1)
            {
                return false;
            }

            if (matchingIndexes.Remove(preferredSlot))
            {
                matchingIndexes.Insert(0, preferredSlot);
            }

            foreach (int index in matchingIndexes)
            {
                slots[index] = default;
            }

            int remaining = totalQuantity;
            foreach (int index in matchingIndexes)
            {
                int quantity = Math.Min(remaining, item.MaxStackSize);
                slots[index] = quantity > 0 ? new SlotData { Item = item, Quantity = quantity } : default;
                remaining -= quantity;
            }

            Changed?.Invoke();
            return true;
        }

        /// <summary>
        /// Moves an item to an empty slot, merges stacks of the same item, or swaps two slots.
        /// Returns false when the source is empty or both indexes are identical.
        /// </summary>
        public bool MoveOrSwap(int fromIndex, int toIndex)
        {
            ValidateIndex(fromIndex);
            ValidateIndex(toIndex);

            if (fromIndex == toIndex || slots[fromIndex].IsEmpty)
            {
                return false;
            }

            SlotData source = slots[fromIndex];
            SlotData destination = slots[toIndex];

            if (destination.IsEmpty)
            {
                slots[toIndex] = source;
                slots[fromIndex] = default;
                Changed?.Invoke();
                return true;
            }

            if (source.Item == destination.Item)
            {
                int availableCapacity = destination.Item.MaxStackSize - destination.Quantity;
                if (availableCapacity > 0)
                {
                    int transferred = Math.Min(source.Quantity, availableCapacity);
                    destination.Quantity += transferred;
                    source.Quantity -= transferred;
                    slots[toIndex] = destination;
                    slots[fromIndex] = source.Quantity > 0 ? source : default;
                    Changed?.Invoke();
                    return true;
                }
            }

            slots[fromIndex] = destination;
            slots[toIndex] = source;
            Changed?.Invoke();
            return true;
        }

        private int Fill(int index, ItemDefinition item, int quantity)
        {
            if (!slots[index].IsEmpty && slots[index].Item != item)
            {
                return 0;
            }

            int added = Math.Min(quantity, item.MaxStackSize - slots[index].Quantity);
            if (added <= 0)
            {
                return 0;
            }

            slots[index].Item = item;
            slots[index].Quantity += added;
            return added;
        }

        private static void ValidateItem(ItemDefinition item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
        }

        private static void ValidateItemId(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                throw new ArgumentException("An inventory item id is required.", nameof(itemId));
            }
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= slots.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        private struct SlotData
        {
            public ItemDefinition Item;
            public int Quantity;

            public bool IsEmpty => Item == null || Quantity <= 0;
        }
    }
}
