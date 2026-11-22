using System.Collections.Generic;
using Fungiiiii.Champimaison;
using Fungiiiii.Controls;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// What the Champimaison's next stage costs, over it, as in many games: one line per
    /// item (<see cref="RequirementRow"/>), then the place left for the key with its hold
    /// ring, drawn by the key prompt, and what holding it does. Shown while the player is
    /// near and no menu is open, until the last stage is reached.
    /// </summary>
    internal sealed class RequirementPanel : MonoBehaviour
    {
        [SerializeField]
        private RectTransform panel;

        [Tooltip("\"Wake the seed up\", then \"Grow the Champimaison\".")]
        [SerializeField]
        private LocalizedLabel title;

        [SerializeField]
        private RequirementRow rowPrefab;

        [SerializeField]
        private RectTransform rows;

        [Tooltip("Under the lines, right of the key: what holding it does (\"Drop\", \"Wake up\", \"Grow\"). Hidden when there is nothing to drop.")]
        [SerializeField]
        private LocalizedLabel action;

        [Tooltip("Shown only within this distance of the Champimaison, in metres.")]
        [SerializeField, Min(1f)]
        private float showDistance = 8f;

        private readonly List<RequirementRow> shownRows = new List<RequirementRow>();

        private FungalHole hole;
        private PlayerInventory inventory;
        private Camera view;
        private IMenuState menus;
        private bool dirty;

        public bool IsVisible => panel.gameObject.activeSelf;

        public IReadOnlyList<RequirementRow> Rows => shownRows;

        public string TitleKey => title.EntryKey;

        /// <summary>The entry shown for what holding the key does; empty when hidden.</summary>
        public string ActionKey => action.gameObject.activeSelf ? action.EntryKey : string.Empty;

        private void Awake()
        {
            panel.gameObject.SetActive(false);
        }

        public void Bind(FungalHole champimaison, PlayerInventory playerInventory, Camera viewCamera, IMenuState menuState)
        {
            Unbind();
            hole = champimaison;
            inventory = playerInventory;
            view = viewCamera;
            menus = menuState;
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
                && !hole.Network.IsComplete
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
                Refresh();
            }
        }

        private void Refresh()
        {
            dirty = false;
            var network = hole.Network;
            title.Show(network.Stage == 0 ? UiText.PanelAwakenTitle : UiText.PanelGrowTitle);
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

            var holdKey = hole.HoldHintKey;
            action.gameObject.SetActive(holdKey != null);
            if (holdKey != null)
            {
                action.Show(holdKey);
            }
        }
    }
}
