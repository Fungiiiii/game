using System.Collections.Generic;
using Fungiiiii.Crafting;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>A tracked recipe, top right during play: what it makes, then one line per ingredient.</summary>
    internal sealed class TrackedRecipeView : MonoBehaviour
    {
        [SerializeField]
        private ItemIcon icon;

        [SerializeField]
        private LocalizedLabel itemName;

        [SerializeField]
        private IngredientRow linePrefab;

        [SerializeField]
        private RectTransform lines;

        private readonly List<IngredientRow> shownLines = new List<IngredientRow>();

        public CraftingRecipe Recipe { get; private set; }

        public IReadOnlyList<IngredientRow> Lines => shownLines;

        public void Show(CraftingRecipe recipe, PlayerInventory inventory)
        {
            if (Recipe != recipe)
            {
                Recipe = recipe;
                icon.Show(recipe.Result);
                itemName.Show(recipe.Result.NameKey);
            }

            var ingredients = recipe.Ingredients;
            while (shownLines.Count < ingredients.Count)
            {
                shownLines.Add(Instantiate(linePrefab, lines));
            }

            for (var line = 0; line < shownLines.Count; line++)
            {
                var used = line < ingredients.Count;
                shownLines[line].gameObject.SetActive(used);
                if (used)
                {
                    var ingredient = ingredients[line];
                    shownLines[line].Show(ingredient.Item, inventory.Count(ingredient.Item.Id), ingredient.Amount);
                }
            }
        }
    }
}
