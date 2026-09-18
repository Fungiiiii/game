using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Inventory
{
    /// <summary>
    /// Runtime data needed to add one item type to an inventory.
    /// The identifier is a stable gameplay key, not a player-facing label.
    /// </summary>
    public readonly struct InventoryItemDefinition : IEquatable<InventoryItemDefinition>
    {
        public InventoryItemDefinition(string id, Color color, int maxStackSize)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("An inventory item id is required.", nameof(id));
            }

            if (maxStackSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxStackSize), "The maximum stack size must be positive.");
            }

            Id = id;
            Color = color;
            MaxStackSize = maxStackSize;
        }

        public string Id { get; }

        public Color Color { get; }

        public int MaxStackSize { get; }

        public bool Equals(InventoryItemDefinition other)
        {
            return Id == other.Id && MaxStackSize == other.MaxStackSize && Color == other.Color;
        }

        public override bool Equals(object obj)
        {
            return obj is InventoryItemDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Color, MaxStackSize);
        }

        public static bool operator ==(InventoryItemDefinition left, InventoryItemDefinition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InventoryItemDefinition left, InventoryItemDefinition right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// Immutable view of one inventory slot.
    /// </summary>
    public readonly struct InventorySlot
    {
        internal InventorySlot(int index, string itemId, Color color, int quantity, int maxStackSize)
        {
            Index = index;
            ItemId = itemId;
            Color = color;
            Quantity = quantity;
            MaxStackSize = maxStackSize;
        }

        public int Index { get; }

        public string ItemId { get; }

        public Color Color { get; }

        public int Quantity { get; }

        public int MaxStackSize { get; }

        public bool IsEmpty => Quantity <= 0 || string.IsNullOrEmpty(ItemId);
    }

    /// <summary>
    /// Small, scene-independent inventory implementation for the player POC.
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

        public int SlotCount => slots.Length;

        public InventorySlot GetSlot(int index)
        {
            if (index < 0 || index >= slots.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            SlotData slot = slots[index];
            return new InventorySlot(index, slot.ItemId, slot.Color, slot.Quantity, slot.MaxStackSize);
        }

        public int Count(string itemId)
        {
            ValidateItemId(itemId);

            int total = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].ItemId == itemId)
                {
                    total += slots[i].Quantity;
                }
            }

            return total;
        }

        /// <summary>
        /// Adds as many items as fit and returns the number that was added.
        /// Existing compatible stacks are filled before empty slots are used.
        /// </summary>
        public int Add(InventoryItemDefinition item, int quantity)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "The quantity must be positive.");
            }

            int remaining = quantity;
            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (slots[i].ItemId != item.Id)
                {
                    continue;
                }

                int added = AddToSlot(i, item, remaining, slots[i].MaxStackSize);
                remaining -= added;
            }

            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (!slots[i].IsEmpty)
                {
                    continue;
                }

                int added = AddToSlot(i, item, remaining, item.MaxStackSize);
                remaining -= added;
            }

            int addedTotal = quantity - remaining;
            if (addedTotal > 0)
            {
                Changed?.Invoke();
            }

            return addedTotal;
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
                if (slots[i].ItemId != itemId)
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
        /// Takes up to the requested amount from one slot and returns the item definition.
        /// This is used by cursor-style inventory interactions such as split stacks.
        /// </summary>
        public int TakeFromSlot(int index, int quantity, out InventoryItemDefinition item)
        {
            ValidateIndex(index);
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "The quantity must be positive.");
            }

            SlotData slot = slots[index];
            if (slot.IsEmpty)
            {
                item = default;
                return 0;
            }

            item = new InventoryItemDefinition(slot.ItemId, slot.Color, slot.MaxStackSize);
            int taken = Math.Min(quantity, slot.Quantity);
            slot.Quantity -= taken;
            slots[index] = slot.Quantity > 0 ? slot : default;
            Changed?.Invoke();
            return taken;
        }

        /// <summary>
        /// Places as many items as fit in one slot and returns the amount placed.
        /// </summary>
        public int AddToSlot(int index, InventoryItemDefinition item, int quantity)
        {
            ValidateIndex(index);
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "The quantity must be positive.");
            }

            int added = AddToSlot(index, item, quantity, item.MaxStackSize);
            if (added > 0)
            {
                Changed?.Invoke();
            }

            return added;
        }

        /// <summary>
        /// Consolidates compatible stacks of one item into the fewest possible slots,
        /// preferring the slot that was double-clicked.
        /// </summary>
        public bool StackAll(string itemId, int preferredSlot)
        {
            ValidateItemId(itemId);
            ValidateIndex(preferredSlot);

            var matchingIndexes = new List<int>();
            var hasDefinition = false;
            InventoryItemDefinition definition = default;
            int totalQuantity = 0;

            for (int i = 0; i < slots.Length; i++)
            {
                SlotData slot = slots[i];
                if (slot.IsEmpty || slot.ItemId != itemId)
                {
                    continue;
                }

                if (!hasDefinition)
                {
                    definition = new InventoryItemDefinition(slot.ItemId, slot.Color, slot.MaxStackSize);
                    hasDefinition = true;
                }

                if (!AreCompatible(slot, definition))
                {
                    continue;
                }

                matchingIndexes.Add(i);
                totalQuantity += slot.Quantity;
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
                int quantity = Math.Min(remaining, definition.MaxStackSize);
                slots[index].ItemId = definition.Id;
                slots[index].Color = definition.Color;
                slots[index].MaxStackSize = definition.MaxStackSize;
                slots[index].Quantity = quantity;
                remaining -= quantity;
            }

            Changed?.Invoke();
            return true;
        }

        /// <summary>
        /// Moves an item to an empty slot, merges compatible stacks, or swaps two slots.
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

            if (AreCompatible(source, destination))
            {
                int availableCapacity = destination.MaxStackSize - destination.Quantity;
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

        private int AddToSlot(int index, InventoryItemDefinition item, int quantity, int maxStackSize)
        {
            if (!slots[index].IsEmpty && !AreCompatible(slots[index], item))
            {
                return 0;
            }

            int capacity = maxStackSize - slots[index].Quantity;
            int added = Math.Min(quantity, capacity);
            if (added <= 0)
            {
                return 0;
            }

            if (slots[index].IsEmpty)
            {
                slots[index].ItemId = item.Id;
                slots[index].Color = item.Color;
                slots[index].MaxStackSize = item.MaxStackSize;
            }

            slots[index].Quantity += added;
            return added;
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

        private static bool AreCompatible(SlotData left, SlotData right)
        {
            return left.ItemId == right.ItemId &&
                   left.MaxStackSize == right.MaxStackSize &&
                   left.Color == right.Color;
        }

        private static bool AreCompatible(SlotData slot, InventoryItemDefinition item)
        {
            return slot.ItemId == item.Id &&
                   slot.MaxStackSize == item.MaxStackSize &&
                   slot.Color == item.Color;
        }

        private struct SlotData
        {
            public string ItemId;
            public Color Color;
            public int Quantity;
            public int MaxStackSize;

            public bool IsEmpty => Quantity <= 0 || string.IsNullOrEmpty(ItemId);
        }
    }
}
