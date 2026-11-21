using System;
using Fungiiiii.Inventory;

namespace Fungiiiii.UI
{
    /// <summary>
    /// What the mouse holds while the inventory is open, and what each press does with it
    /// (playtest of 20/11, feedback #2). Everything happens on the press, without waiting
    /// to see whether a double click follows.
    ///
    /// Left press: take the whole stack; put it all down on an empty slot; top up a stack
    /// of the same item, keeping the rest in hand; swap with another item. A second left
    /// press on the same slot within <see cref="DoubleClickSeconds"/>, with something in
    /// hand, gathers every stack of that item onto the mouse, up to a full stack.
    /// Right press: take half, rounded up; put a single one down; nothing on another item.
    /// </summary>
    internal sealed class InventoryHand
    {
        public const float DoubleClickSeconds = 0.25f;

        private const int NoSlot = -1;

        private readonly PlayerInventory inventory;
        private int lastLeftSlot = NoSlot;
        private float lastLeftTime;

        public InventoryHand(PlayerInventory inventory)
        {
            this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        }

        /// <summary>Raised whenever what is held changes.</summary>
        public event Action Changed;

        public bool IsHolding => Quantity > 0;

        /// <summary>The item held, null when nothing is.</summary>
        public ItemDefinition Item { get; private set; }

        public int Quantity { get; private set; }

        /// <summary>The slot the held item was taken from: it goes back there when the inventory closes.</summary>
        public int OriginSlot { get; private set; } = NoSlot;

        /// <param name="time">Unscaled time of the press, in seconds.</param>
        public void PressLeft(int slot, float time)
        {
            var isDoubleClick = IsHolding && slot == lastLeftSlot && time - lastLeftTime < DoubleClickSeconds;
            if (isDoubleClick)
            {
                // Reset, so that a third press is a press of its own and not a second double click.
                lastLeftSlot = NoSlot;
                GatherAll();
                return;
            }

            lastLeftSlot = slot;
            lastLeftTime = time;

            if (!IsHolding)
            {
                Take(slot, inventory.GetSlot(slot).Quantity);
                return;
            }

            var target = inventory.GetSlot(slot);
            if (target.IsEmpty || target.Item == Item)
            {
                PutDown(slot, Quantity);
                return;
            }

            Swap(slot, target.Quantity);
        }

        public void PressRight(int slot)
        {
            lastLeftSlot = NoSlot;
            var target = inventory.GetSlot(slot);

            if (!IsHolding)
            {
                Take(slot, (target.Quantity + 1) / 2);
                return;
            }

            if (target.IsEmpty || target.Item == Item)
            {
                PutDown(slot, 1);
            }
        }

        /// <summary>
        /// Puts what is held back where it came from, or anywhere it fits. What fits nowhere
        /// stays in hand.
        /// </summary>
        public void PutBack()
        {
            lastLeftSlot = NoSlot;
            if (!IsHolding)
            {
                return;
            }

            if (OriginSlot != NoSlot)
            {
                var origin = inventory.GetSlot(OriginSlot);
                if (origin.IsEmpty || origin.Item == Item)
                {
                    Quantity -= inventory.AddToSlot(OriginSlot, Item, Quantity);
                }
            }

            if (IsHolding)
            {
                Quantity -= inventory.Add(Item, Quantity);
            }

            AfterChange();
        }

        /// <summary>Drops what is held without putting it anywhere, for a reset.</summary>
        public void Clear()
        {
            lastLeftSlot = NoSlot;
            Quantity = 0;
            AfterChange();
        }

        private void Take(int slot, int quantity)
        {
            if (quantity <= 0)
            {
                return;
            }

            Quantity = inventory.TakeFromSlot(slot, quantity, out var taken);
            Item = taken;
            OriginSlot = slot;
            AfterChange();
        }

        private void PutDown(int slot, int quantity)
        {
            Quantity -= inventory.AddToSlot(slot, Item, quantity);
            AfterChange();
        }

        private void Swap(int slot, int targetQuantity)
        {
            var targetQuantityTaken = inventory.TakeFromSlot(slot, targetQuantity, out var targetItem);
            inventory.AddToSlot(slot, Item, Quantity);
            Item = targetItem;
            Quantity = targetQuantityTaken;
            OriginSlot = slot;
            AfterChange();
        }

        /// <summary>Takes from the other stacks of the held item, incomplete ones first, until the hand holds a full stack.</summary>
        private void GatherAll()
        {
            GatherFrom(fullStacks: false);
            GatherFrom(fullStacks: true);
            AfterChange();
        }

        private void GatherFrom(bool fullStacks)
        {
            for (var index = 0; index < inventory.SlotCount && Quantity < Item.MaxStackSize; index++)
            {
                var slot = inventory.GetSlot(index);
                if (slot.IsEmpty || slot.Item != Item || (slot.Quantity >= Item.MaxStackSize) != fullStacks)
                {
                    continue;
                }

                Quantity += inventory.TakeFromSlot(index, Item.MaxStackSize - Quantity, out _);
            }
        }

        private void AfterChange()
        {
            if (!IsHolding)
            {
                Item = null;
                OriginSlot = NoSlot;
            }

            Changed?.Invoke();
        }
    }
}
