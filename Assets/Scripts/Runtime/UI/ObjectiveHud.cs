using System;
using System.Collections.Generic;
using Fungiiiii.Crafting;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Top right during play, as in Palworld: the objective, then the recipes the player
    /// tracks with their ingredients, kept up to date as the inventory changes.
    /// </summary>
    internal sealed class ObjectiveHud : MonoBehaviour
    {
        [SerializeField]
        private LocalizedLabel objective;

        [SerializeField]
        private TrackedRecipeView trackedPrefab;

        [SerializeField]
        private RectTransform tracked;

        private readonly List<TrackedRecipeView> views = new List<TrackedRecipeView>();

        private Func<string> objectiveKey;
        private RecipeTracker tracker;
        private PlayerInventory inventory;

        public string ObjectiveKey => objective.EntryKey;

        public IReadOnlyList<TrackedRecipeView> TrackedViews => views;

        /// <param name="currentObjectiveKey">The objective's text now, a key of the UI string table.</param>
        public void Bind(Func<string> currentObjectiveKey, RecipeTracker recipeTracker, PlayerInventory playerInventory)
        {
            objectiveKey = currentObjectiveKey;
            tracker = recipeTracker;
            inventory = playerInventory;
            tracker.Changed += ShowTracked;
            inventory.Changed += ShowTracked;
            ShowObjective();
            ShowTracked();
        }

        /// <summary>Shows the objective again: the owner calls it when the objective changes.</summary>
        public void ShowObjective()
        {
            objective.Show(objectiveKey());
        }

        private void OnDestroy()
        {
            if (tracker != null)
            {
                tracker.Changed -= ShowTracked;
            }

            if (inventory != null)
            {
                inventory.Changed -= ShowTracked;
            }
        }

        private void ShowTracked()
        {
            var recipes = tracker.Tracked;
            while (views.Count < recipes.Count)
            {
                views.Add(Instantiate(trackedPrefab, tracked));
            }

            for (var index = 0; index < views.Count; index++)
            {
                var used = index < recipes.Count;
                views[index].gameObject.SetActive(used);
                if (used)
                {
                    views[index].Show(recipes[index], inventory);
                }
            }
        }
    }
}
