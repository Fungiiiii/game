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
    /// A triangle over the key says a short press does something. A target that can be
    /// held (<see cref="IHoldInteractable"/>) gets a border around the key, which fills
    /// while it is held, once the press is past a short one; on it, a short press is
    /// decided on the release (<see cref="TapOrHold"/>). A target that shows a text
    /// (<see cref="ILabelledInteractable"/>) gets the text instead of the key, and one that
    /// shows its keys in its own panel (<see cref="IPanelledInteractable"/>) no key at all.
    /// What a press and a hold do are written right of the key, not in the corner's hints.
    /// </summary>
    internal sealed class PlayerInteractor : MonoBehaviour
    {
        private readonly List<InteractionCandidate> candidates = new List<InteractionCandidate>();

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
        private readonly List<string> keyPaths = new List<string>();
        private InputDevice glyphDevice;
        private Glyph? keyGlyph;

        /// <summary>The key shown above the target.</summary>
        public KeyPrompt Prompt => prompt;

        /// <summary>What the player aims at, or null.</summary>
        public IInteractable Targeted => targeted;

        /// <summary>How much of a hold is done, from 0 to 1: 0 during what may still be a short press.</summary>
        public float HoldProgress => press?.HoldProgress ?? 0f;

        /// <summary>The interact control as the key hints draw it, on the device used last.</summary>
        public Glyph InteractGlyph => KeyGlyph();

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

            // From one target straight to another, the key pops in again over the new one.
            if (previous != null && next != null && prompt != null)
            {
                prompt.PopIn();
            }

            ShowPrompt();
        }

        private void ShowPrompt()
        {
            if (prompt == null)
            {
                return;
            }

            if (targeted == null || !isActiveAndEnabled || view == null || targeted is IPanelledInteractable)
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
            var holdKey = (targeted as IHoldInteractable)?.HoldHintKey;
            var canHold = holdKey != null;
            // A key that does nothing when held is always a key to press.
            var canPress = targeted.HintKey != null || !canHold;
            prompt.SetMarks(canPress, canHold, canHold ? press.HoldProgress : 0f);
            prompt.SetActions(targeted.HintKey, holdKey);
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
