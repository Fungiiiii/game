using System;
using System.Collections.Generic;
using Fungiiiii.Inventory;

namespace Fungiiiii.Champimaison
{
    /// <summary>
    /// What has been dropped towards a list of required items, kept between visits: each
    /// deposit takes from the inventory what is still missing, never more, and the list is
    /// complete once every line is.
    /// </summary>
    public sealed class RequirementProgress
    {
        private readonly ItemAmount[] lines;
        private readonly int[] deposited;

        public RequirementProgress(IReadOnlyList<ItemAmount> requirements)
        {
            if (requirements == null || requirements.Count == 0)
            {
                throw new ArgumentException("A requirement needs at least one line.", nameof(requirements));
            }

            lines = new ItemAmount[requirements.Count];
            var ids = new HashSet<string>();
            for (var line = 0; line < requirements.Count; line++)
            {
                var requirement = requirements[line];
                if (requirement.Item == null || requirement.Amount <= 0)
                {
                    throw new ArgumentException($"Line {line} has no item or no amount.", nameof(requirements));
                }

                if (!ids.Add(requirement.Item.Id))
                {
                    throw new ArgumentException($"The item '{requirement.Item.Id}' is required twice.", nameof(requirements));
                }

                lines[line] = requirement;
            }

            deposited = new int[lines.Length];
        }

        /// <summary>Raised after a deposit took something.</summary>
        public event Action Changed;

        public int LineCount => lines.Length;

        public bool IsComplete
        {
            get
            {
                for (var line = 0; line < lines.Length; line++)
                {
                    if (Missing(line) > 0)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public ItemAmount Required(int line) => lines[line];

        public int Deposited(int line) => deposited[line];

        public int Missing(int line) => lines[line].Amount - deposited[line];

        /// <summary>Whether a deposit from this inventory would take something.</summary>
        public bool CanTakeFrom(PlayerInventory inventory)
        {
            ValidateInventory(inventory);
            for (var line = 0; line < lines.Length; line++)
            {
                if (Missing(line) > 0 && inventory.Count(lines[line].Item.Id) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Whether a deposit from this inventory would complete the list.</summary>
        public bool WouldComplete(PlayerInventory inventory)
        {
            ValidateInventory(inventory);
            for (var line = 0; line < lines.Length; line++)
            {
                if (inventory.Count(lines[line].Item.Id) < Missing(line))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Takes from <paramref name="inventory"/>, for each line, what is still missing or
        /// what it holds, whichever is less.
        /// </summary>
        /// <returns>How many items were taken in all.</returns>
        public int Deposit(PlayerInventory inventory)
        {
            ValidateInventory(inventory);
            var total = 0;
            for (var line = 0; line < lines.Length; line++)
            {
                var id = lines[line].Item.Id;
                var take = Math.Min(Missing(line), inventory.Count(id));
                if (take <= 0)
                {
                    continue;
                }

                var taken = inventory.Remove(id, take);
                deposited[line] += taken;
                total += taken;
            }

            if (total > 0)
            {
                Changed?.Invoke();
            }

            return total;
        }

        private static void ValidateInventory(PlayerInventory inventory)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }
        }
    }
}
