using System.Collections.Generic;
using Fungiiiii.Capture;
using Fungiiiii.Controls;
using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using Fungiiiii.Runtime.Player;
using Fungiiiii.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

namespace Fungiiiii.Interaction
{
    /// <summary>
    /// The player's interact control: shows what the player aims at close by, with the
    /// key to press above it, and uses it on a press. The key is that of the Interact
    /// action on the device the player used last.
    ///
    /// A target that can also be held (<see cref="IHoldInteractable"/>) gets a ring around
    /// the key that fills while it is held; on it, a short press is decided on the release
    /// (<see cref="TapOrHold"/>). A target that shows a text (<see cref="ILabelledInteractable"/>)
    /// gets the text instead of the key.
    /// </summary>
    internal sealed class PlayerInteractor : MonoBehaviour, IHintSource
    {
        private readonly List<InteractionCandidate> candidates = new List<InteractionCandidate>();
        private readonly List<ControlHint> hints = new List<ControlHint>();

        [Tooltip("How far from the centre of the view a target can be aimed at, in degrees.")]
        [SerializeField, Range(1f, 90f)]
        private float maxAimAngleDegrees = 25f;

        [Tooltip("On a target that can be held, a press released within this time is a short press, in seconds.")]
        [SerializeField, Min(0.05f)]
        private float tapMaxSeconds = 0.3f;

        [Tooltip("How long the control is held to do what holding does, in seconds.")]
        [SerializeField, Min(0.1f)]
        private float holdSeconds = 0.8f;

        private FirstPersonCamera viewCamera;
        private Camera view;
        private PlayerInventory inventory;
        private IReadOnlyList<IInteractable> interactables;
        private PlayerControls controls;
        private IInteractable targeted;
        private KeyPrompt prompt;
        private TapOrHold press;
        private string shownHintKey;
        private string shownHoldHintKey;
        private readonly List<string> keyPaths = new List<string>();
        private InputDevice glyphDevice;
        private Glyph? keyGlyph;

        /// <summary>The key shown above the target.</summary>
        public KeyPrompt Prompt => prompt;

        /// <summary>What the player aims at, or null.</summary>
        public IInteractable Targeted => targeted;

        /// <summary>Raised when the target changes, and with it the hint.</summary>
        public event System.Action HintsChanged;

        /// <summary>What the interact control does with the target; nothing without one.</summary>
        public IReadOnlyList<ControlHint> Hints => hints;

        public void Initialize(
            FirstPersonCamera firstPersonCamera,
            PlayerInventory playerInventory,
            IReadOnlyList<IInteractable> usable,
            KeyPrompt keyPrompt,
            PlayerControls playerControls)
        {
            controls = playerControls;
            controls.MenuOpenChanged += OnMenuOpenChanged;
            viewCamera = firstPersonCamera;
            view = firstPersonCamera.Camera;
            // The key goes over the target once the view has moved for the frame.
            viewCamera.Moved += ShowPrompt;
            inventory = playerInventory;
            interactables = usable;
            prompt = keyPrompt;
            press = new TapOrHold(tapMaxSeconds, holdSeconds);
        }

        private void OnDisable()
        {
            press?.Cancel();
            SetTarget(null);
        }

        private void OnMenuOpenChanged(bool open)
        {
            enabled = !open;
        }

        private void OnDestroy()
        {
            if (controls != null)
            {
                controls.MenuOpenChanged -= OnMenuOpenChanged;
            }

            if (viewCamera != null)
            {
                viewCamera.Moved -= ShowPrompt;
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
            if (targeted == null)
            {
                return;
            }

            // What the target does may change while it is aimed at: a Spongeus soaks up water.
            RefreshHints();

            var interact = controls.Player.Interact;
            if (targeted is IHoldInteractable holdable)
            {
                UseHoldable(holdable, interact);
            }
            else if (interact.WasPressedThisFrame() && targeted.HintKey != null)
            {
                targeted.Interact(inventory);
            }
        }

        /// <summary>A short press does what a press does, on the release; holding does what holding does.</summary>
        private void UseHoldable(IHoldInteractable holdable, InputAction interact)
        {
            if (interact.WasPressedThisFrame())
            {
                press.Press();
            }

            var outcome = press.Tick(Time.deltaTime);
            if (interact.WasReleasedThisFrame())
            {
                var release = press.Release();
                if (outcome == PressOutcome.None)
                {
                    outcome = release;
                }
            }

            if (outcome == PressOutcome.Tap && targeted.HintKey != null)
            {
                targeted.Interact(inventory);
            }
            else if (outcome == PressOutcome.Hold)
            {
                holdable.InteractHold(inventory);
            }
        }

        private void SetTarget(IInteractable next)
        {
            if (next == targeted)
            {
                return;
            }

            var previous = targeted;
            targeted?.SetTargeted(false);
            targeted = next;
            targeted?.SetTargeted(true);

            // A press started on another target does nothing on this one.
            press?.Cancel();
            RefreshHints(force: true);

            // From one target straight to another, the key pops in again over the new one.
            if (previous != null && next != null && prompt != null)
            {
                prompt.PopIn();
            }

            ShowPrompt();
        }

        private void RefreshHints(bool force = false)
        {
            var hintKey = targeted?.HintKey;
            var holdHintKey = (targeted as IHoldInteractable)?.HoldHintKey;
            if (!force && hintKey == shownHintKey && holdHintKey == shownHoldHintKey)
            {
                return;
            }

            shownHintKey = hintKey;
            shownHoldHintKey = holdHintKey;
            hints.Clear();
            if (controls != null)
            {
                AddHint(hintKey);
                AddHint(holdHintKey);
            }

            HintsChanged?.Invoke();
        }

        private void AddHint(string labelKey)
        {
            if (labelKey != null)
            {
                hints.Add(new ControlHint(labelKey, HintInput.Of(controls.Player.Interact)));
            }
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

            var labelKey = (targeted as ILabelledInteractable)?.LabelKey;
            if (labelKey != null)
            {
                prompt.ShowText(screenPoint, labelKey);
                return;
            }

            prompt.Show(screenPoint, KeyGlyph());
            prompt.SetHold(targeted is IHoldInteractable, press.HoldProgress);
        }

        /// <summary>The interact control as the key hints draw it, on the device used last; worked out again only when that device changes.</summary>
        private Glyph KeyGlyph()
        {
            var device = controls.CurrentDevice;
            if (keyGlyph.HasValue && glyphDevice == device)
            {
                return keyGlyph.Value;
            }

            glyphDevice = device;
            keyPaths.Clear();
            HintInput.Of(controls.Player.Interact).CollectPaths(controls.CurrentScheme.bindingGroup, keyPaths);
            keyGlyph = InputGlyphs.Describe(keyPaths, device is DualShockGamepad);
            return keyGlyph.Value;
        }
    }
}
