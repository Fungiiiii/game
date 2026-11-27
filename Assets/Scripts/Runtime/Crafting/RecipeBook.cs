using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Crafting
{
    /// <summary>The recipes the crafting panel lists, in the order it shows them.</summary>
    [CreateAssetMenu(fileName = "RecipeBook", menuName = "Fungiiiii/Recipe Book")]
    public sealed class RecipeBook : ScriptableObject
    {
        [SerializeField]
        private CraftingRecipe[] recipes = new CraftingRecipe[0];

        public IReadOnlyList<CraftingRecipe> Recipes => recipes;

        /// <summary>A book that exists only at runtime, for tests.</summary>
        public static RecipeBook Create(params CraftingRecipe[] recipes)
        {
            var book = CreateInstance<RecipeBook>();
            book.recipes = recipes;
            return book;
        }

        public string FindProblem()
        {
            if (recipes.Length == 0)
            {
                return $"The recipe book '{name}' is empty.";
            }

            foreach (var recipe in recipes)
            {
                if (recipe == null)
                {
                    return $"The recipe book '{name}' has an empty entry.";
                }

                var problem = recipe.FindProblem();
                if (problem != null)
                {
                    return problem;
                }
            }

            return null;
        }
    }
}
