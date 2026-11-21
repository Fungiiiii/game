using System;

namespace Fungiiiii.Inventory
{
    /// <summary>
    /// How a player's inventory is laid out: the bag first, then the quick bar, whose
    /// selected slot is the item in hand. The inventory itself is a plain row of slots;
    /// this says which of them belong to which.
    /// </summary>
    public readonly struct InventoryLayout : IEquatable<InventoryLayout>
    {
        /// <summary>The layout of the Inventaire artboard: a bag of 3 x 4 slots and a quick bar of 3.</summary>
        public static readonly InventoryLayout Standard = new InventoryLayout(12, 3);

        public InventoryLayout(int bagSlotCount, int quickBarSlotCount)
        {
            if (bagSlotCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bagSlotCount));
            }

            if (quickBarSlotCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quickBarSlotCount), "The quick bar holds the item in hand: it needs a slot.");
            }

            BagSlotCount = bagSlotCount;
            QuickBarSlotCount = quickBarSlotCount;
        }

        public int BagSlotCount { get; }

        public int QuickBarSlotCount { get; }

        public int SlotCount => BagSlotCount + QuickBarSlotCount;

        /// <summary>Index, in the inventory, of the quick bar's first slot.</summary>
        public int QuickBarFirstSlot => BagSlotCount;

        /// <summary>Index, in the inventory, of the quick bar's slot at <paramref name="position"/>, from 0.</summary>
        public int QuickBarSlot(int position)
        {
            if (position < 0 || position >= QuickBarSlotCount)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            return BagSlotCount + position;
        }

        public bool IsInQuickBar(int slot) => slot >= BagSlotCount && slot < SlotCount;

        public bool Equals(InventoryLayout other) => BagSlotCount == other.BagSlotCount && QuickBarSlotCount == other.QuickBarSlotCount;

        public override bool Equals(object obj) => obj is InventoryLayout other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(BagSlotCount, QuickBarSlotCount);
    }
}
