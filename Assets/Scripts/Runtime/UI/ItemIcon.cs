using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// How an item looks anywhere in the interface: the quick bar, the slots of the
    /// inventory, the item held by the mouse. Its icon when its definition has one, else
    /// the square of its colour, until the icons are drawn. Every screen that shows an item
    /// puts one on an Image: a new icon then shows everywhere at once.
    /// </summary>
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public sealed class ItemIcon : MonoBehaviour
    {
        private Image image;

        /// <summary>The item shown, null when none is.</summary>
        public ItemDefinition Item { get; private set; }

        // Taken on first use: the screens build their icons before Awake can run.
        private Image Image
        {
            get
            {
                if (image == null)
                {
                    image = GetComponent<Image>();
                }

                return image;
            }
        }

        /// <summary>Shows <paramref name="item"/>; null shows nothing.</summary>
        public void Show(ItemDefinition item)
        {
            Item = item;
            var target = Image;
            target.enabled = item != null;
            if (item == null)
            {
                return;
            }

            var hasIcon = item.Icon != null;
            target.sprite = hasIcon ? item.Icon : null;
            target.color = hasIcon ? Color.white : item.Color;
            target.preserveAspect = hasIcon;
        }
    }
}
