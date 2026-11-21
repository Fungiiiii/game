using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>One plank of the quick bar, with its item and the quantity; the selected one gets lighter and rises.</summary>
    internal sealed class HotbarSlot : MonoBehaviour
    {
        [Tooltip("Rises when selected; the slot's own rectangle stays put.")]
        [SerializeField]
        private RectTransform face;

        [SerializeField]
        private CanvasGroup faceGroup;

        [SerializeField]
        private PlankGraphic plank;

        [SerializeField]
        private ItemIcon item;

        [SerializeField]
        private Text quantity;

        [Tooltip("How far the selected plank rises, in canvas units.")]
        [SerializeField, Min(0f)]
        private float selectedRise = 12f;

        [Tooltip("Planks that are not selected are slightly see-through.")]
        [SerializeField, Range(0f, 1f)]
        private float restAlpha = 0.75f;

        private float current;

        /// <summary>1 when selected, 0 otherwise.</summary>
        public float Target { get; set; }

        /// <summary>Where the plank's end facet starts, so that the planks do not look stamped.</summary>
        public float EndFacetStart
        {
            set => plank.EndFacetStart = value;
        }

        private void Awake()
        {
            Apply();
        }

        public void Show(InventorySlot slot)
        {
            item.Show(slot.Item);
            quantity.text = StackLabel.For(slot.Quantity);
        }

        public void Animate(float step)
        {
            if (Mathf.Approximately(current, Target))
            {
                return;
            }

            current = Mathf.MoveTowards(current, Target, step);
            Apply();
        }

        private void Apply()
        {
            // Ease out, as on the menu planks.
            var eased = 1f - (1f - current) * (1f - current);
            face.anchoredPosition = Vector2.up * (selectedRise * eased);
            faceGroup.alpha = Mathf.Lerp(restAlpha, 1f, eased);
            plank.Highlight = eased;
        }
    }
}
