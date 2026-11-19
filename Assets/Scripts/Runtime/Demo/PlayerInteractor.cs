using System.Collections.Generic;
using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fungiiiii.Demo
{
    /// <summary>
    /// The interact control of the demo: shows what the player aims at close by, and uses
    /// it on a press. Interact is built in code, like Dodge in PlayerInputReader.
    /// </summary>
    internal sealed class PlayerInteractor : MonoBehaviour
    {
        private readonly List<InteractionCandidate> candidates = new List<InteractionCandidate>();

        private Camera view;
        private PlayerInventory inventory;
        private IReadOnlyList<IDemoInteractable> interactables;
        private float maxAimAngleDegrees;
        private InputAction interactAction;
        private IDemoInteractable targeted;

        public void Initialize(
            Camera viewCamera,
            PlayerInventory playerInventory,
            IReadOnlyList<IDemoInteractable> usable,
            float aimConeDegrees)
        {
            view = viewCamera;
            inventory = playerInventory;
            interactables = usable;
            maxAimAngleDegrees = aimConeDegrees;
        }

        private void Awake()
        {
            interactAction = new InputAction("Interact", InputActionType.Button);
            interactAction.AddBinding("<Keyboard>/e");
            interactAction.AddBinding("<Gamepad>/buttonNorth");
        }

        private void OnEnable()
        {
            interactAction?.Enable();
        }

        private void OnDisable()
        {
            interactAction?.Disable();
            SetTarget(null);
        }

        private void OnDestroy()
        {
            interactAction?.Dispose();
            interactAction = null;
        }

        private void Update()
        {
            if (view == null || interactables == null)
            {
                return;
            }

            candidates.Clear();
            foreach (var interactable in interactables)
            {
                candidates.Add(interactable.Candidate);
            }

            var viewTransform = view.transform;
            var index = InteractionTargeting.Pick(
                viewTransform.position,
                viewTransform.forward,
                candidates,
                maxAimAngleDegrees);

            SetTarget(index == InteractionTargeting.None ? null : interactables[index]);

            if (targeted != null && interactAction.WasPressedThisFrame())
            {
                targeted.Interact(inventory);
            }
        }

        private void SetTarget(IDemoInteractable next)
        {
            if (next == targeted)
            {
                return;
            }

            targeted?.SetTargeted(false);
            targeted = next;
            targeted?.SetTargeted(true);
        }
    }
}
