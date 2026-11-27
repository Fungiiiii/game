using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// A menu button drawn as a wooden plank. The plank under the pointer becomes the
    /// selected one, and the selected plank gets lighter and slides forward: mouse,
    /// keyboard and gamepad show the same thing, and only one plank stands out at a time.
    /// The pointer leaving it leaves no plank selected.
    /// </summary>
    [RequireComponent(typeof(Button))]
    internal sealed class PlankButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        /// <summary>Seconds to go from rest to fully highlighted. Runs on real time: menus work while the game is paused.</summary>
        private const float TransitionSeconds = 0.14f;

        [Tooltip("Holds the plank and its label: it slides, while the button's own rectangle stays put under the pointer.")]
        [SerializeField]
        private RectTransform face;

        [SerializeField]
        private PlankGraphic plank;

        [SerializeField]
        private LocalizedLabel label;

        [Tooltip("How far the face slides when the plank is selected, in canvas units.")]
        [SerializeField]
        private Vector2 slide = new Vector2(0f, 6f);

        private float target;
        private float current;

        public Button Button => GetComponent<Button>();

        public LocalizedLabel Label => label;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (EventSystem.current != null && Button.IsInteractable())
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            target = 1f;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            target = 0f;
        }

        private void OnDisable()
        {
            // A hidden menu comes back at rest.
            target = 0f;
            current = 0f;
            Apply();
        }

        // The component stays enabled while it has nothing to animate: the event system
        // does not send select and deselect events to a disabled component.
        private void Update()
        {
            if (Mathf.Approximately(current, target))
            {
                return;
            }

            current = Mathf.MoveTowards(current, target, Time.unscaledDeltaTime / TransitionSeconds);
            Apply();
        }

        private void Apply()
        {
            // Ease out: quick start, soft landing.
            var eased = 1f - (1f - current) * (1f - current);
            face.anchoredPosition = slide * eased;
            plank.Highlight = eased;
        }
    }
}
