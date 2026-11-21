using Fungiiiii.Runtime.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// First-person camera of the net prototype only. It sits at the player's eyes, turns
    /// with the mouse or the right stick, and hands its yaw to the <see cref="PlayerMotor"/>
    /// so that forward follows the view.
    ///
    /// Not the game's camera: the definitive first-person camera belongs to the player
    /// movement ticket. The Look action is built in code, like Dodge in PlayerInputReader.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    internal sealed class PrototypeFirstPersonCamera : MonoBehaviour
    {
        /// <summary>Degrees of rotation per pixel of mouse movement.</summary>
        private const float MouseDegreesPerPixel = 0.1f;

        private const float StickDegreesPerSecond = 150f;
        private const float MaxPitchDegrees = 85f;

        /// <summary>Eyes sit this far below the top of the controller capsule.</summary>
        private const float EyeDepthBelowTop = 0.25f;

        private InputAction lookAction;
        private PlayerMotor motor;
        private CharacterController characterController;
        private float yaw;
        private float pitch;

        public Camera Camera { get; private set; }

        /// <summary>Multiplies how fast the view turns: the net's aim slows it as it narrows the view.</summary>
        public float LookScale { get; set; } = 1f;

        public void Initialize(PlayerMotor playerMotor, float initialYawDegrees)
        {
            motor = playerMotor;
            characterController = playerMotor.GetComponent<CharacterController>();
            yaw = initialYawDegrees;
            pitch = 0f;
            motor.MovementYawDegrees = yaw;
            Follow();
        }

        /// <summary>Points the view, for a demo reset.</summary>
        public void SetView(float yawDegrees, float pitchDegrees)
        {
            yaw = yawDegrees;
            pitch = Mathf.Clamp(pitchDegrees, -MaxPitchDegrees, MaxPitchDegrees);
        }

        private void Awake()
        {
            Camera = GetComponent<Camera>();
            Camera.nearClipPlane = 0.05f;

            lookAction = new InputAction("Look", InputActionType.Value, expectedControlType: "Vector2");
            lookAction.AddBinding("<Mouse>/delta");
            lookAction.AddBinding("<Gamepad>/rightStick");
        }

        private void OnEnable()
        {
            lookAction?.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            lookAction?.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDestroy()
        {
            lookAction?.Dispose();
            lookAction = null;
        }

        private void Update()
        {
            // Escape releases the cursor in the Editor: a click takes it back.
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            var look = lookAction.ReadValue<Vector2>();
            var fromMouse = lookAction.activeControl != null && lookAction.activeControl.device is Mouse;
            var scale = (fromMouse ? MouseDegreesPerPixel : StickDegreesPerSecond * Time.deltaTime) * LookScale;

            yaw += look.x * scale;
            pitch = Mathf.Clamp(pitch - look.y * scale, -MaxPitchDegrees, MaxPitchDegrees);

            if (motor != null)
            {
                motor.MovementYawDegrees = yaw;
            }
        }

        private void LateUpdate()
        {
            Follow();
        }

        private void Follow()
        {
            if (motor == null)
            {
                return;
            }

            var playerTransform = motor.transform;
            var eyeHeight = 0f;

            if (characterController != null)
            {
                // The capsule shrinks with its feet planted while crouching: the eyes follow its top.
                eyeHeight = characterController.center.y + characterController.height * 0.5f - EyeDepthBelowTop;
            }

            transform.SetPositionAndRotation(
                playerTransform.position + Vector3.up * eyeHeight,
                Quaternion.Euler(pitch, yaw, 0f));
        }
    }
}
