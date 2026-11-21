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
    /// movement ticket. Its actions come from the player's controls.
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
        private InputAction cursorCaptureAction;
        private PlayerMotor motor;
        private CharacterController characterController;
        private float yaw;
        private float pitch;

        public Camera Camera { get; private set; }

        /// <summary>Multiplies how fast the view turns: the net's aim slows it as it narrows the view.</summary>
        public float LookScale { get; set; } = 1f;

        /// <param name="look">Turns the view.</param>
        /// <param name="cursorCapture">Takes the cursor back after Escape released it in the Editor.</param>
        public void Initialize(PlayerMotor playerMotor, float initialYawDegrees, InputAction look, InputAction cursorCapture)
        {
            lookAction = look;
            cursorCaptureAction = cursorCapture;
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
        }

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (lookAction == null)
            {
                return;
            }

            // Escape releases the cursor in the Editor: a click takes it back.
            if (cursorCaptureAction.WasPressedThisFrame() && Cursor.lockState != CursorLockMode.Locked)
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
