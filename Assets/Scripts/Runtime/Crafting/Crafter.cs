using System;
using Fungiiiii.Inventory;

namespace Fungiiiii.Crafting
{
    /// <summary>
    /// Crafting, all or nothing: every ingredient is taken at once and the result is
    /// collected like an item picked up, the quick bar first. When the result cannot fit,
    /// nothing is taken, nothing moves, and nothing is crafted.
    /// </summary>
    public static class Crafter
    {
        public static bool CanCraft(PlayerInventory inventory, CraftingRecipe recipe)
        {
            Validate(inventory, recipe);
            foreach (var ingredient in recipe.Ingredients)
            {
                if (inventory.Count(ingredient.Item.Id) < ingredient.Amount)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool TryCraft(PlayerInventory inventory, CraftingRecipe recipe)
        {
            if (!CanCraft(inventory, recipe) || !Craft(inventory.Copy(), recipe, collect: false))
            {
                return false;
            }

            return Craft(inventory, recipe, collect: true);
        }

        /// <summary>Takes the ingredients, then adds the result; tried first on a copy, so that a result that cannot fit moves nothing.</summary>
        private static bool Craft(PlayerInventory inventory, CraftingRecipe recipe, bool collect)
        {
            foreach (var ingredient in recipe.Ingredients)
            {
                inventory.Remove(ingredient.Item.Id, ingredient.Amount);
            }

            var added = collect ? inventory.Collect(recipe.Result, 1) : inventory.Add(recipe.Result, 1);
            return added == 1;
        }

        private static void Validate(PlayerInventory inventory, CraftingRecipe recipe)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }
        }
    }
}
