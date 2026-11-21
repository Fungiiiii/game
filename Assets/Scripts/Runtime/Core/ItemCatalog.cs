using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Inventory
{
    /// <summary>
    /// Every item of the game, found by its id: a saved inventory or a network message
    /// carries ids, and this turns them back into definitions.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemCatalog", menuName = "Fungiiiii/Item Catalog")]
    public sealed class ItemCatalog : ScriptableObject
    {
        [SerializeField]
        private ItemDefinition[] items = new ItemDefinition[0];

        public IReadOnlyList<ItemDefinition> Items => items;

        /// <summary>A catalog that exists only at runtime, for tests.</summary>
        public static ItemCatalog Create(params ItemDefinition[] definitions)
        {
            var catalog = CreateInstance<ItemCatalog>();
            catalog.items = definitions;
            return catalog;
        }

        public bool TryGet(string itemId, out ItemDefinition item)
        {
            foreach (var candidate in items)
            {
                if (candidate != null && candidate.Id == itemId)
                {
                    item = candidate;
                    return true;
                }
            }

            item = null;
            return false;
        }

        /// <exception cref="KeyNotFoundException">No item has this id.</exception>
        public ItemDefinition Get(string itemId)
        {
            if (TryGet(itemId, out var item))
            {
                return item;
            }

            throw new KeyNotFoundException($"The item catalog '{name}' has no item '{itemId}'.");
        }

        /// <summary>What is wrong with the catalog: empty entries, missing or repeated ids. Empty when it is sound.</summary>
        public IReadOnlyList<string> FindProblems()
        {
            var problems = new List<string>();
            var seen = new HashSet<string>();

            for (var index = 0; index < items.Length; index++)
            {
                var item = items[index];
                if (item == null)
                {
                    problems.Add($"Entry {index} is empty.");
                }
                else if (string.IsNullOrWhiteSpace(item.Id))
                {
                    problems.Add($"'{item.name}' has no id.");
                }
                else if (!seen.Add(item.Id))
                {
                    problems.Add($"The id '{item.Id}' is used twice.");
                }
            }

            return problems;
        }
    }
}
