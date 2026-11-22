using System;
using System.Collections.Generic;
using Fungiiiii.Controls;
using Fungiiiii.Crafting;
using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The crafting panel, next to the inventory (mockup B): every recipe at the top, even
    /// those that cannot be made yet; the open recipe's details at the bottom, with what is
    /// missing in red and where it comes from; under them, the keys to craft and to track.
    /// Crafting uses the control that moves items: a second click on the open recipe, A on
    /// a gamepad, or the craft key. It opens and closes with the inventory, nothing open.
    /// </summary>
    internal sealed class CraftingPanel : MonoBehaviour, IHintSource
    {
        private const int None = -1;

        [SerializeField]
        private RecipeBook book;

        [SerializeField]
        private RecipeRow rowPrefab;

        [SerializeField]
        private RectTransform list;

        [Tooltip("The open recipe's details.")]
        [SerializeField]
        private GameObject details;

        [Tooltip("Shown instead of the details while no recipe is open: \"Choose a recipe\".")]
        [SerializeField]
        private GameObject nothingOpen;

        [SerializeField]
        private ItemIcon detailIcon;

        [SerializeField]
        private LocalizedLabel detailName;

        [SerializeField]
        private LocalizedLabel detailDescription;

        [SerializeField]
        private IngredientRow ingredientPrefab;

        [SerializeField]
        private RectTransform ingredients;

        [Tooltip("The keys to craft and to track, at the bottom of the panel.")]
        [SerializeField]
        private ControlHintsPanel actions;

        private readonly List<RecipeRow> rows = new List<RecipeRow>();
        private readonly List<IngredientRow> ingredientRows = new List<IngredientRow>();
        private readonly List<ControlHint> hints = new List<ControlHint>();

        private PlayerInventory inventory;
        private RecipeTracker tracker;
        private InputAction craftAction;
        private InputAction trackAction;
        private int open = None;
        private int focused = None;

        public event Action HintsChanged;

        public IReadOnlyList<ControlHint> Hints => hints;

        public RecipeBook Book => book;

        public IReadOnlyList<RecipeRow> Rows => rows;

        /// <summary>The recipe whose details are shown; null when none is.</summary>
        public CraftingRecipe OpenedRecipe => open == None ? null : rows[open].Recipe;

        public bool AreDetailsShown => details.activeSelf;

        public IReadOnlyList<IngredientRow> IngredientRows => ingredientRows;

        public string FindProblem()
        {
            return book == null ? $"{name} has no recipe book." : book.FindProblem();
        }

        /// <param name="screen">The inventory next to it: the gamepad moves from its slots to the recipes and back.</param>
        public void Bind(PlayerInventory playerInventory, RecipeTracker recipeTracker, PlayerControls controls, InventoryScreen screen)
        {
            inventory = playerInventory;
            tracker = recipeTracker;
            craftAction = controls.UI.Craft;
            trackAction = controls.UI.Track;

            for (var index = 0; index < book.Recipes.Count; index++)
            {
                var row = Instantiate(rowPrefab, list);
                row.name = $"Recipe_{book.Recipes[index].name}";
                row.Initialize(this, index, book.Recipes[index]);
                rows.Add(row);
            }

            LinkNavigation(screen);
            inventory.Changed += Refresh;
            tracker.Changed += Refresh;
            actions.Initialize(controls);
            actions.ShowFrom(this);
            Refresh();
        }

        /// <summary>A click on a recipe: opens it, or crafts it when it is already open.</summary>
        public void Click(int index)
        {
            if (open == index)
            {
                Craft(index);
            }
            else
            {
                OpenRecipe(index);
            }
        }

        /// <summary>A on a gamepad: crafts the recipe in focus.</summary>
        public void Submit(int index)
        {
            OpenRecipe(index);
            Craft(index);
        }

        /// <summary>The mouse or the gamepad moves on or off a recipe; the gamepad opens it.</summary>
        public void Focus(int index, bool isFocused, bool openIt)
        {
            rows[index].SetFocused(isFocused);
            if (isFocused)
            {
                focused = index;
                if (openIt)
                {
                    OpenRecipe(index);
                }
            }
            else if (focused == index)
            {
                focused = None;
            }
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.Changed -= Refresh;
            }

            if (tracker != null)
            {
                tracker.Changed -= Refresh;
            }
        }

        private void OnDisable()
        {
            // Nothing stays open or lit for the next opening.
            var eventSystem = EventSystem.current;
            if (eventSystem != null && focused != None && eventSystem.currentSelectedGameObject == rows[focused].gameObject)
            {
                eventSystem.SetSelectedGameObject(null);
            }

            foreach (var row in rows)
            {
                row.SetFocused(false);
            }

            focused = None;
            open = None;
            if (inventory != null)
            {
                Refresh();
            }
        }

        private void Update()
        {
            if (craftAction != null && craftAction.WasPressedThisFrame() && open != None)
            {
                Craft(open);
            }

            if (trackAction != null && trackAction.WasPressedThisFrame())
            {
                // On a gamepad the track button is also "gather all" on a slot: it tracks only on a recipe.
                var onGamepad = trackAction.activeControl?.device is Gamepad;
                var target = onGamepad ? focused : open;
                if (target != None)
                {
                    tracker.Toggle(rows[target].Recipe);
                }
            }
        }

        private void OpenRecipe(int index)
        {
            if (open == index)
            {
                return;
            }

            open = index;
            Refresh();
        }

        private void Craft(int index)
        {
            var recipe = rows[index].Recipe;
            if (Crafter.TryCraft(inventory, recipe))
            {
                tracker.Crafted(recipe);
            }
        }

        private void Refresh()
        {
            for (var index = 0; index < rows.Count; index++)
            {
                var recipe = rows[index].Recipe;
                rows[index].Show(Crafter.CanCraft(inventory, recipe), tracker.IsTracked(recipe), index == open);
            }

            details.SetActive(open != None);
            nothingOpen.SetActive(open == None);
            if (open != None)
            {
                ShowDetails(rows[open].Recipe);
            }

            RefreshHints();
        }

        private void ShowDetails(CraftingRecipe recipe)
        {
            var result = recipe.Result;
            detailIcon.Show(result);
            detailName.Show(result.NameKey);
            var hasDescription = !string.IsNullOrEmpty(result.DescriptionKey);
            detailDescription.gameObject.SetActive(hasDescription);
            if (hasDescription)
            {
                detailDescription.Show(result.DescriptionKey);
            }

            var needed = recipe.Ingredients;
            while (ingredientRows.Count < needed.Count)
            {
                ingredientRows.Add(Instantiate(ingredientPrefab, ingredients));
            }

            for (var line = 0; line < ingredientRows.Count; line++)
            {
                var used = line < needed.Count;
                ingredientRows[line].gameObject.SetActive(used);
                if (used)
                {
                    var ingredient = needed[line];
                    ingredientRows[line].Show(ingredient.Item, inventory.Count(ingredient.Item.Id), ingredient.Amount);
                }
            }
        }

        /// <summary>Craft while the open recipe can be made, track or untrack while one is open.</summary>
        private void RefreshHints()
        {
            hints.Clear();
            if (open != None)
            {
                var recipe = rows[open].Recipe;
                if (rows[open].CanBeCrafted)
                {
                    hints.Add(new ControlHint(UiText.HintCraft, HintInput.Of(craftAction), HintInput.At("<Gamepad>/buttonSouth")));
                }

                hints.Add(new ControlHint(tracker.IsTracked(recipe) ? UiText.HintUntrack : UiText.HintTrack, HintInput.Of(trackAction)));
            }

            HintsChanged?.Invoke();
        }

        /// <summary>
        /// The recipes are a column the stick moves through; left of it is the inventory's
        /// right column, right of that column are the recipes.
        /// </summary>
        private void LinkNavigation(InventoryScreen screen)
        {
            if (rows.Count == 0)
            {
                return;
            }

            var inventorySide = screen != null ? screen.RightmostSlot : null;
            for (var index = 0; index < rows.Count; index++)
            {
                var navigation = new Navigation { mode = Navigation.Mode.Explicit };
                navigation.selectOnUp = index > 0 ? rows[index - 1].Selectable : null;
                navigation.selectOnDown = index < rows.Count - 1 ? rows[index + 1].Selectable : null;
                navigation.selectOnLeft = inventorySide;
                rows[index].Selectable.navigation = navigation;
            }

            if (screen != null)
            {
                screen.LinkRightTo(rows[0].Selectable);
            }
        }
    }
}
