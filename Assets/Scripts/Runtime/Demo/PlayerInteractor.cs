using System;
using System.Collections.Generic;
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
    /// key to press above it, and uses it on a press. Interact is built in code, like Dodge
    /// in PlayerInputReader.
    /// </summary>
    internal sealed class PlayerInteractor : MonoBehaviour
    {
        private const int KeyboardBinding = 0;
        private const int GamepadBinding = 1;

        private const InputControlPath.HumanReadableStringOptions KeyNameOptions =
            InputControlPath.HumanReadableStringOptions.OmitDevice | InputControlPath.HumanReadableStringOptions.UseShortNames;

        private readonly List<InteractionCandidate> candidates = new List<InteractionCandidate>();

        private Camera view;
        private PlayerInventory inventory;
        private IReadOnlyList<IDemoInteractable> interactables;
        private float maxAimAngleDegrees;
        private InputAction interactAction;
        private IDemoInteractable targeted;
        private KeyPrompt prompt;
        private IDisposable buttonWatch;
        private InputDevice lastDevice;
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
            Font promptFont)
        {
            view = viewCamera;
            inventory = playerInventory;
            interactables = usable;
            maxAimAngleDegrees = aimConeDegrees;

            prompt = new GameObject("Interact Prompt").AddComponent<KeyPrompt>();
            prompt.Initialize(promptFont);
        }

        private void Awake()
        {
            interactAction = new InputAction("Interact", InputActionType.Button);
            interactAction.AddBinding("<Keyboard>/e");
            interactAction.AddBinding("<Gamepad>/buttonNorth");

            // The key shown is that of the device used last: keyboard and mouse, or a pad.
            buttonWatch = InputSystem.onAnyButtonPress.Call(control => lastDevice = control.device);
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
            buttonWatch?.Dispose();

            if (prompt != null)
            {
                Destroy(prompt.gameObject);
            }
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
            if (keyLabel != null && labelDevice == lastDevice)
            {
                return keyLabel;
            }

            labelDevice = lastDevice;
            var onGamepad = lastDevice is Gamepad;
            var path = interactAction.bindings[onGamepad ? GamepadBinding : KeyboardBinding].effectivePath;
            keyLabel = ShortKeyName(InputControlPath.ToHumanReadableString(path, KeyNameOptions, onGamepad ? lastDevice : null));
            return keyLabel;
        }

        /// <summary>A key name short enough for the key cap: PlayStation's Triangle becomes its symbol.</summary>
        internal static string ShortKeyName(string humanReadable)
        {
            return humanReadable == "Triangle" ? KeyPrompt.TriangleLabel : humanReadable;
        }
    }
}
