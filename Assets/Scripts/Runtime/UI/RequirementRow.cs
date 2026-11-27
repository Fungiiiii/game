using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// One line of a cost: the item's icon and name, how many the player carries while the
    /// line is not complete, and how many were dropped out of how many are required, in
    /// the colour of the line's state: coral when nothing can be dropped, gold when some
    /// can, moss green with a tick once complete.
    /// </summary>
    internal sealed class RequirementRow : MonoBehaviour
    {
        [SerializeField]
        private ItemIcon icon;

        [SerializeField]
        private LocalizedLabel itemName;

        [Tooltip("\"dropped/required\"")]
        [SerializeField]
        private Text count;

        [Tooltip("How many the player carries: \"{0} in the backpack\".")]
        [SerializeField]
        private LocalizedLabel carried;

        [Tooltip("Shown once the line is complete.")]
        [SerializeField]
        private GameObject tick;

        [SerializeField]
        private Color nothingToDropColor = new Color32(0xF0, 0x8A, 0x74, 0xFF);

        [SerializeField]
        private Color someToDropColor = new Color32(0xE8, 0xCF, 0x7E, 0xFF);

        [SerializeField]
        private Color completeColor = new Color32(0x9C, 0xB9, 0x55, 0xFF);

        /// <summary>What the count reads, for the tests.</summary>
        public string CountText => count.text;

        public bool IsComplete => tick.activeSelf;

        public int ShownCarried { get; private set; }

        public void Show(ItemDefinition item, int done, int required, int inBackpack)
        {
            if (icon.Item != item)
            {
                icon.Show(item);
                itemName.Show(item.NameKey);
            }

            count.text = $"{done}/{required}"; // i18n-exempt: two numbers, no words
            var complete = done >= required;
            tick.SetActive(complete);

            count.color = complete ? completeColor : inBackpack > 0 ? someToDropColor : nothingToDropColor;

            ShownCarried = complete ? 0 : inBackpack;
            carried.gameObject.SetActive(!complete);
            if (!complete)
            {
                carried.Show(UiText.PanelInBackpack, ShownCarried);
            }
        }
    }
}
