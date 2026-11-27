using System.Collections.Generic;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Crafting
{
    /// <summary>One item made from others, as data: a new recipe is an asset, not code.</summary>
    [CreateAssetMenu(fileName = "Recipe", menuName = "Fungiiiii/Crafting Recipe")]
    public sealed class CraftingRecipe : ScriptableObject
    {
        [Tooltip("What crafting gives: one of it.")]
        [SerializeField]
        private ItemDefinition result;

        [Tooltip("Taken from the inventory, all at once.")]
        [SerializeField]
        private ItemAmount[] ingredients = new ItemAmount[0];

        public ItemDefinition Result => result;

        public IReadOnlyList<ItemAmount> Ingredients => ingredients;

        /// <summary>A recipe that exists only at runtime, for tests.</summary>
        public static CraftingRecipe Create(ItemDefinition result, params ItemAmount[] ingredients)
        {
            var recipe = CreateInstance<CraftingRecipe>();
            recipe.name = result != null ? result.Id : "Recipe";
            recipe.result = result;
            recipe.ingredients = ingredients;
            return recipe;
        }

        public string FindProblem()
        {
            if (result == null)
            {
                return $"The recipe '{name}' makes nothing.";
            }

            if (ingredients.Length == 0)
            {
                return $"The recipe '{name}' takes nothing.";
            }

            foreach (var ingredient in ingredients)
            {
                if (ingredient.Item == null || ingredient.Amount <= 0)
                {
                    return $"The recipe '{name}' has an empty ingredient.";
                }
            }

            return null;
        }
    }
}
