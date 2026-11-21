using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// A menu opens with no button selected, so that the mouse finds none lit. The first
    /// move of the gamepad or of the arrow keys then selects <see cref="first"/>; from
    /// there they move from button to button.
    /// </summary>
    internal sealed class SelectFirstOnMove : MonoBehaviour
    {
        [Tooltip("The button the first move selects.")]
        [SerializeField]
        private Selectable first;

        private void Update()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null || eventSystem.currentSelectedGameObject != null)
            {
                return;
            }

            if (eventSystem.currentInputModule is InputSystemUIInputModule module
                && module.move != null
                && module.move.action != null
                && module.move.action.WasPerformedThisFrame()
                && module.move.action.ReadValue<Vector2>() != Vector2.zero)
            {
                eventSystem.SetSelectedGameObject(first.gameObject);
            }
        }
    }
}
