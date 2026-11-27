using Fungiiiii.Crafting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// One recipe in the crafting panel's list: what it makes, whether it can be made now,
    /// whether it is tracked, and a mark while its details are open. The mouse over it
    /// lights it; a click opens it, a second click crafts. The gamepad opens the recipe it
    /// moves to, and A crafts it.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    internal sealed class RecipeRow : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        [SerializeField]
        private CutCornerGraphic background;

        [SerializeField]
        private ItemIcon icon;

        [SerializeField]
        private LocalizedLabel itemName;

        [Tooltip("\"Possible\" or \"Missing\".")]
        [SerializeField]
        private LocalizedLabel state;

        [Tooltip("Shown while the recipe is tracked.")]
        [SerializeField]
        private GameObject trackedTag;

        [Tooltip("Shown while the recipe's details are open.")]
        [SerializeField]
        private GameObject openMark;

        [Tooltip("How much lighter the row is while the mouse or the gamepad is on it.")]
        [SerializeField, Min(1f)]
        private float focusBrightness = 1.35f;

        private CraftingPanel owner;
        private int index;
        private bool selectedByPointer;

        public CraftingRecipe Recipe { get; private set; }

        public bool CanBeCrafted { get; private set; }

        public bool IsTracked => trackedTag.activeSelf;

        public bool IsOpen => openMark.activeSelf;

        public Selectable Selectable => GetComponent<Selectable>();

        public void Initialize(CraftingPanel panel, int rowIndex, CraftingRecipe recipe)
        {
            owner = panel;
            index = rowIndex;
            Recipe = recipe;
            icon.Show(recipe.Result);
            itemName.Show(recipe.Result.NameKey);
        }

        public void Show(bool canBeCrafted, bool tracked, bool open)
        {
            CanBeCrafted = canBeCrafted;
            state.Show(canBeCrafted ? UiText.CraftPossible : UiText.CraftMissing);
            trackedTag.SetActive(tracked);
            openMark.SetActive(open);
        }

        public void SetFocused(bool focused)
        {
            background.Brightness = focused ? focusBrightness : 1f;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                owner.Click(index);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            selectedByPointer = true;
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            // The mouse only lights the row: it opens on a click. The gamepad opens what it moves to.
            owner.Focus(index, true, openIt: !selectedByPointer);
            selectedByPointer = false;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            owner.Focus(index, false, openIt: false);
        }

        /// <summary>A on a gamepad: crafts, as the second click does.</summary>
        public void OnSubmit(BaseEventData eventData)
        {
            owner.Submit(index);
        }
    }
}
