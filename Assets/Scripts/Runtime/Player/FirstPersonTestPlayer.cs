using UnityEngine;
using UnityEngine.InputSystem;

namespace Fungiiiii.Runtime.Player
{
    /// <summary>
    /// Test-only first-person player, to walk through the Mystic Forest. Taken from
    /// SylvaTestPlayerBootstrap (branch test/12487v2byt9-sylvatest-player), but placed
    /// in the scene instead of spawning itself: its position and facing in the scene
    /// are the spawn.
    ///
    /// Deliberately standalone and minimal: it reads devices directly instead of going
    /// through the shared input actions, and it is not the game's player controller.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    internal sealed class FirstPersonTestPlayer : MonoBehaviour
    {
        internal const float ControllerHeight = 1.8f;
        internal const float SlopeLimit = 45f;
        private const float ControllerRadius = 0.4f;
        private const float StepOffset = 0.4f;

        private const float WalkSpeed = 5f;
        private const float SprintSpeed = 9f;
        private const float JumpHeight = 1.2f;

        /// <summary>Twice the real world value: a realistic fall feels floaty in a game.</summary>
        private const float Gravity = -19.62f;

        /// <summary>Downward speed kept while grounded so the controller keeps reporting contact.</summary>
        private const float GroundedStickSpeed = 2f;

        /// <summary>Degrees of rotation per pixel of mouse movement.</summary>
        private const float MouseSensitivity = 0.1f;

        private const float MaxPitch = 85f;

        [SerializeField, Tooltip("The camera, child of this object at eye height.")]
        private Transform eye;

        private CharacterController controller;
        private float pitch;
        private float verticalVelocity;

        internal bool IsGrounded => controller.isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            controller.height = ControllerHeight;
            controller.radius = ControllerRadius;
            controller.center = Vector3.zero;
            controller.slopeLimit = SlopeLimit;
            controller.stepOffset = StepOffset;

            // The default (1 mm) drops the small per-frame push down that keeps the
            // player grounded once the frame rate is high enough: at 1000 FPS the
            // push is 2 mm, Move ignores it and isGrounded turns false.
            controller.minMoveDistance = 0f;
        }

        private void OnEnable()
        {
            SetCursorLocked(true);
        }

        private void OnDisable()
        {
            SetCursorLocked(false);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            var input = Vector2.zero;
            bool sprint = false;
            bool jump = false;

            if (keyboard != null)
            {
                // Keys are physical positions: W/A/S/D here are Z/Q/S/D on an AZERTY keyboard.
                if (keyboard.wKey.isPressed) input.y += 1f;
                if (keyboard.sKey.isPressed) input.y -= 1f;
                if (keyboard.dKey.isPressed) input.x += 1f;
                if (keyboard.aKey.isPressed) input.x -= 1f;
                sprint = keyboard.leftShiftKey.isPressed;
                jump = keyboard.spaceKey.wasPressedThisFrame;

                // Escape frees the cursor to reach the Editor; a click takes it back.
                if (keyboard.escapeKey.wasPressedThisFrame)
                {
                    SetCursorLocked(false);
                }
            }

            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame)
                {
                    SetCursorLocked(true);
                }

                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Look(mouse.delta.ReadValue());
                }
            }

            // Runs even without devices, so gravity keeps the player on the ground.
            Step(input, sprint, jump, Time.deltaTime);
        }

        /// <summary>
        /// Moves for one frame. <paramref name="input"/> is X right, Y forward, in -1..1.
        /// </summary>
        internal void Step(Vector2 input, bool sprint, bool jump, float deltaTime)
        {
            input = Vector2.ClampMagnitude(input, 1f);
            float speed = sprint ? SprintSpeed : WalkSpeed;
            Vector3 horizontal = (transform.forward * input.y + transform.right * input.x) * speed;

            if (controller.isGrounded)
            {
                verticalVelocity = jump ? Mathf.Sqrt(2f * JumpHeight * -Gravity) : -GroundedStickSpeed;
            }
            else
            {
                verticalVelocity += Gravity * deltaTime;
            }

            controller.Move((horizontal + Vector3.up * verticalVelocity) * deltaTime);
        }

        private void Look(Vector2 mouseDelta)
        {
            // Mouse delta is already per frame: no Time.deltaTime here.
            transform.Rotate(0f, mouseDelta.x * MouseSensitivity, 0f);
            pitch = Mathf.Clamp(pitch - mouseDelta.y * MouseSensitivity, -MaxPitch, MaxPitch);
            eye.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
