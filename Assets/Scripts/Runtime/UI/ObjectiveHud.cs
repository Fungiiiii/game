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
    /// while some are missing, where to find them; then, under a dashed line and their own
    /// heading, the recipes the player tracks with their ingredients: what the quest asks
    /// and what the player chose to craft read as two blocks. All kept up to date as the inventory changes. Once the
    /// Champimaison has grown a first time, and until the player catches a companion, an
    /// optional line comes first: catch a Champonge with the net, for water. It says what
    /// the net is for, which players did not guess.
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

        [Tooltip("The dashed line and the heading over the tracked recipes: shown while one is tracked.")]
        [SerializeField]
        private GameObject[] trackedHeader;

        private readonly List<TrackedRecipeView> views = new List<TrackedRecipeView>();
        private readonly List<IngredientRow> needRows = new List<IngredientRow>();
        private readonly List<ItemDefinition> companionItems = new List<ItemDefinition>();

        private Func<string> objectiveKey;
        private RecipeTracker tracker;
        private PlayerInventory inventory;
        private FungalNetwork network;
        private Canvas canvas;
        private ItemDefinition captureTool;
        private IngredientRow captureRow;
        private bool hasCaught;

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

        /// <summary>Whether the line and the heading over the tracked recipes show.</summary>
        public bool ShowsTrackedHeader => trackedHeader.Length > 0 && trackedHeader[0].activeSelf;

        /// <summary>The optional line to catch a companion with the net, when it has been made.</summary>
        public IngredientRow CaptureRow => captureRow;

        /// <summary>Whether the optional line to catch a companion with the net shows.</summary>
        public bool SuggestsCapture => captureRow != null && captureRow.gameObject.activeSelf;

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

        /// <summary>
        /// Lets the optional line to catch a companion show, from the Champimaison's second
        /// stage on, until one of <paramref name="companions"/> has been in the inventory.
        /// </summary>
        /// <param name="net">What catches them; its icon starts the line.</param>
        /// <param name="companions">The items the companions become once caught.</param>
        public void SuggestCapture(ItemDefinition net, IEnumerable<ItemDefinition> companions)
        {
            captureTool = net;
            companionItems.Clear();
            companionItems.AddRange(companions);
            ShowNeeds();
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
            ShowCaptureSuggestion();
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

        private void ShowCaptureSuggestion()
        {
            foreach (var companion in companionItems)
            {
                hasCaught |= inventory != null && inventory.Count(companion.Id) > 0;
            }

            var suggest = captureTool != null && !hasCaught && network != null && network.Stage >= 1 && !network.IsComplete;
            if (suggest && captureRow == null)
            {
                captureRow = Instantiate(needPrefab, needs);
                captureRow.name = "Optional Capture";
                captureRow.transform.SetAsFirstSibling();
                captureRow.ShowGoal(captureTool, UiText.ObjectiveOptionalCapture, UiText.ObjectiveOptionalCaptureHint);
            }

            if (captureRow != null && captureRow.gameObject.activeSelf != suggest)
            {
                captureRow.gameObject.SetActive(suggest);
            }
        }

        private void ShowTracked()
        {
            var recipes = tracker.Tracked;
            foreach (var part in trackedHeader)
            {
                if (part.activeSelf != recipes.Count > 0)
                {
                    part.SetActive(recipes.Count > 0);
                }
            }

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
