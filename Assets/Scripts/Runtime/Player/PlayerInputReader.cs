using UnityEngine;
using UnityEngine.InputSystem;

namespace Fungiiiii.Runtime.Player
{
    /// <summary>
    /// Exposes the player move input to the rest of the player components.
    ///
    /// The shared InputSystem_Actions asset does not generate a C# wrapper class, and
    /// the prototype scene deliberately holds no serialized references, so the action
    /// is built in code with the same bindings as the asset's Player/Move action.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        private InputAction moveAction;

        /// <summary>
        /// Current move input on the horizontal plane, in the range expected by
        /// <see cref="PlayerMovementCalculator.CalculateHorizontalVelocity"/>.
        /// Returns zero while the component is disabled.
        /// </summary>
        public Vector2 MoveInput =>
            moveAction is { enabled: true } ? moveAction.ReadValue<Vector2>() : Vector2.zero;

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
        }

        private void OnEnable()
        {
            moveAction?.Enable();
        }

        private void OnDisable()
        {
            moveAction?.Disable();
        }

        private void OnDestroy()
        {
            moveAction?.Dispose();
            moveAction = null;
        }
    }
}
