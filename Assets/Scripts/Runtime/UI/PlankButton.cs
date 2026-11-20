using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// A menu button drawn as a wooden plank. The plank under the pointer becomes the
    /// selected one, and the selected plank gets lighter and slides forward: mouse,
    /// keyboard and gamepad show the same thing, and only one plank stands out at a time.
    /// </summary>
    [RequireComponent(typeof(Button))]
    internal sealed class PlankButton : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler
    {
        /// <summary>Seconds to go from rest to fully highlighted. Runs on real time: menus work while the game is paused.</summary>
        private const float TransitionSeconds = 0.14f;

        private RectTransform face;
        private PlankGraphic plank;
        private Vector2 slide;
        private float target;
        private float current;

        public Button Button { get; private set; }

        public LocalizedLabel Label { get; private set; }

        /// <param name="movingFace">Holds the plank and its label: it slides, while the button's own rectangle stays put under the pointer.</param>
        public void Initialize(RectTransform movingFace, PlankGraphic plankGraphic, LocalizedLabel label, Vector2 slideWhenHighlighted)
        {
            Button = GetComponent<Button>();
            face = movingFace;
            plank = plankGraphic;
            Label = label;
            slide = slideWhenHighlighted;
            Apply();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (EventSystem.current != null && Button.IsInteractable())
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
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
            if (face == null)
            {
                return;
            }

            // Ease out: quick start, soft landing.
            var eased = 1f - (1f - current) * (1f - current);
            face.anchoredPosition = slide * eased;
            plank.Highlight = eased;
        }
    }
}
