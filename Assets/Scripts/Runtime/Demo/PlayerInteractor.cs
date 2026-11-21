using System.Collections.Generic;
using Fungiiiii.Controls;
using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using Fungiiiii.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Fungiiiii.Demo
{
    /// <summary>
    /// The interact control of the demo: shows what the player aims at close by, with the
    /// key to press above it, and uses it on a press. The key is that of the Interact
    /// action on the device the player used last.
    /// </summary>
    internal sealed class PlayerInteractor : MonoBehaviour
    {
        private const InputControlPath.HumanReadableStringOptions KeyNameOptions =
            InputControlPath.HumanReadableStringOptions.OmitDevice | InputControlPath.HumanReadableStringOptions.UseShortNames;

        private readonly List<InteractionCandidate> candidates = new List<InteractionCandidate>();

        private Camera view;
        private PlayerInventory inventory;
        private IReadOnlyList<IDemoInteractable> interactables;
        private float maxAimAngleDegrees;
        private PlayerControls controls;
        private IDemoInteractable targeted;
        private KeyPrompt prompt;
        private InputDevice labelDevice;
        private string keyLabel;

        /// <summary>The key shown above the target.</summary>
        public KeyPrompt Prompt => prompt;

        /// <summary>What the player aims at, or null.</summary>
        public IDemoInteractable Targeted => targeted;

        public void Initialize(
            Camera viewCamera,
            PlayerInventory playerInventory,
            IReadOnlyList<IDemoInteractable> usable,
            float aimConeDegrees,
            Font promptFont,
            PlayerControls playerControls)
        {
            controls = playerControls;
            view = viewCamera;
            inventory = playerInventory;
            interactables = usable;
            maxAimAngleDegrees = aimConeDegrees;

            prompt = new GameObject("Interact Prompt").AddComponent<KeyPrompt>();
            prompt.Initialize(promptFont);
        }

        private void OnDisable()
        {
            SetTarget(null);
        }

        private void OnDestroy()
        {
            if (prompt != null)
            {
                Destroy(prompt.gameObject);
            }
        }

        private void Update()
        {
            if (view == null || interactables == null || controls == null)
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

            if (targeted != null && controls.Player.Interact.WasPressedThisFrame())
            {
                targeted.Interact(inventory);
            }
        }

        private void LateUpdate()
        {
            ShowPrompt();
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
            ShowPrompt();
        }

        private void ShowPrompt()
        {
            if (prompt == null)
            {
                return;
            }

            if (targeted == null || !isActiveAndEnabled || view == null)
            {
                prompt.Hide();
                return;
            }

            var screenPoint = view.WorldToScreenPoint(targeted.PromptPoint);
            if (screenPoint.z <= 0f)
            {
                // Behind the camera, it would show mirrored on the screen.
                prompt.Hide();
                return;
            }

            prompt.Show(screenPoint, KeyLabel());
        }

        /// <summary>The name of the interact key on the device used last, worked out again only when that device changes.</summary>
        private string KeyLabel()
        {
            var device = controls.CurrentDevice;
            if (keyLabel != null && labelDevice == device)
            {
                return keyLabel;
            }

            labelDevice = device;
            var interact = controls.Player.Interact;
            var scheme = controls.CurrentScheme;
            var index = interact.GetBindingIndex(InputBinding.MaskByGroup(scheme.bindingGroup));
            var onGamepad = device is Gamepad;
            keyLabel = index < 0
                ? string.Empty
                : ShortKeyName(InputControlPath.ToHumanReadableString(interact.bindings[index].effectivePath, KeyNameOptions, onGamepad ? device : null));
            return keyLabel;
        }

        /// <summary>A key name short enough for the key cap: PlayStation's Triangle becomes its symbol.</summary>
        internal static string ShortKeyName(string humanReadable)
        {
            return humanReadable == "Triangle" ? KeyPrompt.TriangleLabel : humanReadable;
        }
    }
}
