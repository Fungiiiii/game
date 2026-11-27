using System;
using UnityEngine;

namespace Fungiiiii.Inventory
{
    /// <summary>
    /// A number of one item, as data: a line of a recipe or of a Champimaison stage's cost.
    /// </summary>
    [Serializable]
    public struct ItemAmount
    {
        [SerializeField]
        private ItemDefinition item;

        [SerializeField, Min(1)]
        private int amount;

        public ItemAmount(ItemDefinition item, int amount)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "An amount must be positive.");
            }

            this.item = item;
            this.amount = amount;
        }

        public ItemDefinition Item => item;

        public int Amount => amount;
    }
}
