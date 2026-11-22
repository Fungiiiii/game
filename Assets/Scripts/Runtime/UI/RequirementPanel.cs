using System.Collections.Generic;
using Fungiiiii.Champimaison;
using Fungiiiii.Controls;
using Fungiiiii.Interaction;
using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The wooden panel over the Champimaison, as in the wooden panels design system: a
    /// heading with the stage, one line per item the next stage costs
    /// (<see cref="RequirementRow"/>), then, while the Champimaison is aimed at, its keys
    /// with what each does (<see cref="ActionLine"/>): a short press, and a hold. Once the
    /// last stage is reached, only the key to enter it is left. Shown while the player is
    /// near and no menu is open.
    /// </summary>
    internal sealed class RequirementPanel : MonoBehaviour
    {
        [SerializeField]
        private RectTransform panel;

        [Tooltip("The heading, the line under it and the lines of the cost: hidden once the last stage is reached.")]
        [SerializeField]
        private GameObject[] costParts;

        [Tooltip("\"Wake the seed up\", then \"Grow the Champimaison\".")]
        [SerializeField]
        private LocalizedLabel title;

        [Tooltip("The stage the cost leads to, out of all of them: \"1/2\".")]
        [SerializeField]
        private Text stage;

        [SerializeField]
        private RequirementRow rowPrefab;

        [SerializeField]
        private RectTransform rows;

        [Tooltip("The line over the keys and the keys: hidden while the Champimaison is not aimed at, or nothing can be done.")]
        [SerializeField]
        private GameObject[] footerParts;

        [Tooltip("What a short press does: collect the thread, enter.")]
        [SerializeField]
        private ActionLine pressLine;

        [Tooltip("What holding does: drop, wake up, grow.")]
        [SerializeField]
        private ActionLine holdLine;

        [Tooltip("Shown only within this distance of the Champimaison, in metres.")]
        [SerializeField, Min(1f)]
        private float showDistance = 8f;

        private readonly List<RequirementRow> shownRows = new List<RequirementRow>();

        private FungalHole hole;
        private PlayerInventory inventory;
        private Camera view;
        private IMenuState menus;
        private PlayerInteractor interactor;
        private bool dirty;

        public bool IsVisible => panel.gameObject.activeSelf;

        public IReadOnlyList<RequirementRow> Rows => shownRows;

        public string TitleKey => title.EntryKey;

        /// <summary>Whether the heading and the cost are shown: false once the last stage is reached.</summary>
        public bool ShowsCost => costParts.Length > 0 && costParts[0].activeSelf;

        /// <summary>The entry shown for what a short press does; empty when it is hidden.</summary>
        public string PressKey => IsFooterLineShown(pressLine) ? pressLine.ActionKey : string.Empty;

        /// <summary>The entry shown for what holding does; empty when it is hidden.</summary>
        public string HoldKey => IsFooterLineShown(holdLine) ? holdLine.ActionKey : string.Empty;

        private void Awake()
        {
            panel.gameObject.SetActive(false);
        }

        public void Bind(
            FungalHole champimaison,
            PlayerInventory playerInventory,
            Camera viewCamera,
            IMenuState menuState,
            PlayerInteractor playerInteractor)
        {
            Unbind();
            hole = champimaison;
            inventory = playerInventory;
            view = viewCamera;
            menus = menuState;
            interactor = playerInteractor;
            hole.Network.Changed += MarkDirty;
            inventory.Changed += MarkDirty;
            dirty = true;
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void Unbind()
        {
            if (hole != null)
            {
                hole.Network.Changed -= MarkDirty;
            }

            if (inventory != null)
            {
                inventory.Changed -= MarkDirty;
            }

            hole = null;
            inventory = null;
        }

        private void MarkDirty()
        {
            dirty = true;
        }

        private void LateUpdate()
        {
            var screenPoint = Vector3.zero;
            var show = hole != null
                && !menus.IsMenuOpen
                && Vector3.Distance(view.transform.position, hole.transform.position) <= showDistance
                && (screenPoint = view.WorldToScreenPoint(hole.PanelPoint)).z > 0f;

            if (panel.gameObject.activeSelf != show)
            {
                panel.gameObject.SetActive(show);
            }

            if (!show)
            {
                return;
            }

            panel.position = screenPoint;
            if (dirty)
            {
                RefreshCost();
            }

            // Every frame: the border of the key to hold fills while it is held.
            RefreshFooter();
        }

        private void RefreshCost()
        {
            dirty = false;
            var network = hole.Network;
            var withCost = !network.IsComplete;
            SetActive(costParts, withCost);
            if (!withCost)
            {
                return;
            }

            title.Show(network.Stage == 0 ? UiText.PanelAwakenTitle : UiText.PanelGrowTitle);
            stage.text = $"{network.Stage + 1}/{network.StageCount}"; // i18n-exempt: two numbers, no words
            var progress = network.Progress;
            while (shownRows.Count < progress.LineCount)
            {
                shownRows.Add(Instantiate(rowPrefab, rows));
            }

            for (var line = 0; line < shownRows.Count; line++)
            {
                var row = shownRows[line];
                var used = line < progress.LineCount;
                row.gameObject.SetActive(used);
                if (!used)
                {
                    continue;
                }

                var required = progress.Required(line);
                row.Show(required.Item, progress.Deposited(line), required.Amount, inventory.Count(required.Item.Id));
            }
        }

        private void RefreshFooter()
        {
            var aimed = ReferenceEquals(interactor.Targeted, hole);
            // Collecting the thread has a line of its own, under the panel (ThreadTag).
            var pressKey = aimed && hole.CanBeEntered ? hole.HintKey : null;
            var holdKey = aimed ? hole.HoldHintKey : null;
            SetActive(footerParts, pressKey != null || holdKey != null);
            ShowLine(pressLine, pressKey, false);
            ShowLine(holdLine, holdKey, true);
        }

        private void ShowLine(ActionLine line, string actionKey, bool hold)
        {
            var shown = actionKey != null;
            if (line.gameObject.activeSelf != shown)
            {
                line.gameObject.SetActive(shown);
            }

            if (shown)
            {
                line.Show(interactor.InteractGlyph, hold, hold ? interactor.HoldProgress : 0f, actionKey);
            }
        }

        private bool IsFooterLineShown(ActionLine line)
        {
            return footerParts.Length > 0 && footerParts[0].activeSelf && line.gameObject.activeSelf;
        }

        private static void SetActive(GameObject[] parts, bool active)
        {
            foreach (var part in parts)
            {
                if (part.activeSelf != active)
                {
                    part.SetActive(active);
                }
            }
        }
    }
}
