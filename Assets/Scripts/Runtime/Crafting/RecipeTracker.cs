using System;
using System.Collections.Generic;

namespace Fungiiiii.Crafting
{
    /// <summary>
    /// The recipes the player follows, shown top right during play with their ingredients.
    /// A recipe is followed until it is crafted or the player lets it go.
    /// </summary>
    public sealed class RecipeTracker
    {
        private readonly List<CraftingRecipe> tracked = new List<CraftingRecipe>();

        public event Action Changed;

        /// <summary>In the order they were tracked.</summary>
        public IReadOnlyList<CraftingRecipe> Tracked => tracked;

        public bool IsTracked(CraftingRecipe recipe) => tracked.Contains(recipe);

        /// <summary>Tracks <paramref name="recipe"/>, or lets it go if it was tracked.</summary>
        public void Toggle(CraftingRecipe recipe)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            if (!tracked.Remove(recipe))
            {
                tracked.Add(recipe);
            }

            Changed?.Invoke();
        }

        /// <summary>A crafted recipe is no longer tracked.</summary>
        public void Crafted(CraftingRecipe recipe)
        {
            if (tracked.Remove(recipe))
            {
                Changed?.Invoke();
            }
        }
    }
}
