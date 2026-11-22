using System;
using System.Collections.Generic;
using Fungiiiii.Champimaison;
using Fungiiiii.Crafting;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Top right during play, as in Palworld: the objective; while it is to feed the
    /// Champimaison, what its stage still needs, each with how many the player has and,
    /// while some are missing, where to find them; then the recipes the player tracks with
    /// their ingredients. All kept up to date as the inventory changes.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    internal sealed class ObjectiveHud : MonoBehaviour
    {
        [SerializeField]
        private LocalizedLabel objective;

        [Tooltip("One line of what the Champimaison's stage needs: the item, how many, where to find it.")]
        [SerializeField]
        private IngredientRow needPrefab;

        [Tooltip("Under the objective: the lines of what the Champimaison's stage needs.")]
        [SerializeField]
        private RectTransform needs;

        [SerializeField]
        private TrackedRecipeView trackedPrefab;

        [SerializeField]
        private RectTransform tracked;

        private readonly List<TrackedRecipeView> views = new List<TrackedRecipeView>();
        private readonly List<IngredientRow> needRows = new List<IngredientRow>();

        private Func<string> objectiveKey;
        private RecipeTracker tracker;
        private PlayerInventory inventory;
        private FungalNetwork network;
        private Canvas canvas;

        public string ObjectiveKey => objective.EntryKey;

        public IReadOnlyList<TrackedRecipeView> TrackedViews => views;

        /// <summary>The lines of what the Champimaison's stage needs, shown or not.</summary>
        public IReadOnlyList<IngredientRow> NeedRows => needRows;

        /// <summary>How many lines of what the Champimaison needs are shown.</summary>
        public int ShownNeeds
        {
            get
            {
                var shown = 0;
                foreach (var row in needRows)
                {
                    shown += row.gameObject.activeSelf ? 1 : 0;
                }

                return shown;
            }
        }

        /// <summary>Whether it is drawn. Hidden while the inventory is open, it keeps following the game.</summary>
        public bool IsVisible => Canvas.enabled;

        private Canvas Canvas
        {
            get
            {
                if (canvas == null)
                {
                    canvas = GetComponent<Canvas>();
                }

                return canvas;
            }
        }

        public void SetVisible(bool visible)
        {
            Canvas.enabled = visible;
        }

        /// <param name="currentObjectiveKey">The objective's text now, a key of the UI string table.</param>
        public void Bind(Func<string> currentObjectiveKey, RecipeTracker recipeTracker, PlayerInventory playerInventory)
        {
            objectiveKey = currentObjectiveKey;
            tracker = recipeTracker;
            inventory = playerInventory;
            tracker.Changed += ShowTracked;
            inventory.Changed += ShowTracked;
            inventory.Changed += ShowNeeds;
            ShowObjective();
            ShowTracked();
        }

        /// <summary>From the planting on: what the Champimaison's stage needs shows under the objective.</summary>
        public void ShowNeedsOf(FungalNetwork champimaison)
        {
            if (network != null)
            {
                network.Changed -= ShowNeeds;
            }

            network = champimaison;
            network.Changed += ShowNeeds;
            ShowNeeds();
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
                inventory.Changed -= ShowNeeds;
            }

            if (network != null)
            {
                network.Changed -= ShowNeeds;
            }
        }

        /// <summary>What is still needed: deposited and carried both count, as the player sees it coming together.</summary>
        private void ShowNeeds()
        {
            var lineCount = network == null || network.IsComplete ? 0 : network.Progress.LineCount;
            while (needRows.Count < lineCount)
            {
                needRows.Add(Instantiate(needPrefab, needs));
            }

            for (var line = 0; line < needRows.Count; line++)
            {
                var used = line < lineCount;
                needRows[line].gameObject.SetActive(used);
                if (used)
                {
                    var required = network.Progress.Required(line);
                    var have = network.Progress.Deposited(line) + inventory.Count(required.Item.Id);
                    needRows[line].Show(required.Item, Mathf.Min(have, required.Amount), required.Amount);
                }
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
