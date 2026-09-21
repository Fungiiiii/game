using UnityEngine;
using UnityEngine.InputSystem;

namespace Fungiiiii.Runtime.Player
{
    /// <summary>
    /// Exposes the player movement input to the rest of the player components.
    ///
    /// The shared InputSystem_Actions asset does not generate a C# wrapper class, and
    /// the prototype scene deliberately holds no serialized references, so the actions
    /// are built in code with the same bindings as the asset's Player map.
    ///
    /// Dodge is the exception: the shared asset has no such action yet, so its bindings
    /// are chosen here. Space currently belongs to Jump, which this prototype does not
    /// implement; a real Dodge action has to be added to the asset before production.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        private InputAction moveAction;
        private InputAction sprintAction;
        private InputAction crouchAction;
        private InputAction dodgeAction;

        /// <summary>
        /// Current move input on the horizontal plane. Zero while the component is disabled.
        /// </summary>
        public Vector2 MoveInput =>
            moveAction is { enabled: true } ? moveAction.ReadValue<Vector2>() : Vector2.zero;

        /// <summary>True while the sprint control is held down.</summary>
        public bool IsSprintHeld => sprintAction is { enabled: true } && sprintAction.IsPressed();

        /// <summary>True while the crouch control is held down.</summary>
        public bool IsCrouchHeld => crouchAction is { enabled: true } && crouchAction.IsPressed();

        /// <summary>True only on the frame the dodge control was pressed.</summary>
        public bool WasDodgePressedThisFrame =>
            dodgeAction is { enabled: true } && dodgeAction.WasPressedThisFrame();

        private void Awake()
        {
            moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");

            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            moveAction.AddBinding("<Gamepad>/leftStick");

            sprintAction = new InputAction("Sprint", InputActionType.Button);
            sprintAction.AddBinding("<Keyboard>/leftShift");
            sprintAction.AddBinding("<Gamepad>/leftStickPress");

            crouchAction = new InputAction("Crouch", InputActionType.Button);
            crouchAction.AddBinding("<Keyboard>/c");
            crouchAction.AddBinding("<Gamepad>/buttonEast");

            dodgeAction = new InputAction("Dodge", InputActionType.Button);
            dodgeAction.AddBinding("<Keyboard>/space");
            dodgeAction.AddBinding("<Gamepad>/buttonWest");
        }

        private void OnEnable()
        {
            moveAction?.Enable();
            sprintAction?.Enable();
            crouchAction?.Enable();
            dodgeAction?.Enable();
        }

        private void OnDisable()
        {
            moveAction?.Disable();
            sprintAction?.Disable();
            crouchAction?.Disable();
            dodgeAction?.Disable();
        }

        private void OnDestroy()
        {
            moveAction?.Dispose();
            sprintAction?.Dispose();
            crouchAction?.Dispose();
            dodgeAction?.Dispose();

            moveAction = null;
            sprintAction = null;
            crouchAction = null;
            dodgeAction = null;
        }
    }
}
