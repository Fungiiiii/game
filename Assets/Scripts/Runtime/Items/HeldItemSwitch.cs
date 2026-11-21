using System;
using System.Collections.Generic;
using Fungiiiii.Inventory;

namespace Fungiiiii.Items
{
    /// <summary>
    /// Which item is in hand, and what it does there. An item does something in hand when a
    /// behaviour is registered for it, for a tool built by its scene like the net, or when
    /// its definition has a held prefab, made once the first time it is held. Any other
    /// item, wood for instance, is held without doing anything.
    /// </summary>
    public sealed class HeldItemSwitch
    {
        private readonly Dictionary<ItemDefinition, IHeldItem> behaviours = new Dictionary<ItemDefinition, IHeldItem>();
        private readonly Func<ItemDefinition, IHeldItem> makeFromPrefab;

        /// <param name="fromPrefab">Makes the behaviour of an item from its held prefab; may return null.</param>
        public HeldItemSwitch(Func<ItemDefinition, IHeldItem> fromPrefab)
        {
            makeFromPrefab = fromPrefab ?? throw new ArgumentNullException(nameof(fromPrefab));
        }

        /// <summary>The item in hand, null with empty hands.</summary>
        public ItemDefinition CurrentItem { get; private set; }

        /// <summary>What the item in hand does, null when it does nothing.</summary>
        public IHeldItem Current { get; private set; }

        public void Register(ItemDefinition item, IHeldItem behaviour)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            behaviours[item] = behaviour ?? throw new ArgumentNullException(nameof(behaviour));
        }

        /// <summary>Takes <paramref name="item"/> in hand, putting away what was there; null empties the hands.</summary>
        public void Hold(ItemDefinition item)
        {
            if (item == CurrentItem)
            {
                return;
            }

            if (Current != null)
            {
                Current.Unequip();
            }

            CurrentItem = item;
            Current = BehaviourOf(item);

            if (Current != null)
            {
                Current.Equip();
            }
        }

        public void UsePrimary()
        {
            if (Current != null)
            {
                Current.UsePrimary();
            }
        }

        public void HoldSecondary(bool held)
        {
            if (Current != null)
            {
                Current.HoldSecondary(held);
            }
        }

        public void Cancel()
        {
            if (Current != null)
            {
                Current.Cancel();
            }
        }

        private IHeldItem BehaviourOf(ItemDefinition item)
        {
            if (item == null)
            {
                return null;
            }

            if (behaviours.TryGetValue(item, out var behaviour))
            {
                return behaviour;
            }

            if (item.HeldPrefab == null)
            {
                return null;
            }

            behaviour = makeFromPrefab(item);
            if (behaviour != null)
            {
                behaviours[item] = behaviour;
            }

            return behaviour;
        }
    }
}
