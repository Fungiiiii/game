using System;
using Fungiiiii.Inventory;

namespace Fungiiiii.Crafting
{
    /// <summary>
    /// Crafting, all or nothing: every ingredient is taken at once and the result is
    /// collected like an item picked up, the quick bar first. When the result cannot fit,
    /// the ingredients go back and nothing is crafted.
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
            if (!CanCraft(inventory, recipe))
            {
                return false;
            }

            foreach (var ingredient in recipe.Ingredients)
            {
                inventory.Remove(ingredient.Item.Id, ingredient.Amount);
            }

            if (inventory.Collect(recipe.Result, 1) == 1)
            {
                return true;
            }

            foreach (var ingredient in recipe.Ingredients)
            {
                inventory.Add(ingredient.Item, ingredient.Amount);
            }

            return false;
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
