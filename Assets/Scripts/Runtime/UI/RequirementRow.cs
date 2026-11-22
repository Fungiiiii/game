using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// One line of a cost: the item's icon and name, how many were dropped out of how many
    /// are required, how many the player carries, and a tick once the line is complete.
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

            ShownCarried = complete ? 0 : inBackpack;
            carried.gameObject.SetActive(ShownCarried > 0);
            if (ShownCarried > 0)
            {
                carried.Show(UiText.PanelInBackpack, ShownCarried);
            }
        }
    }
}
