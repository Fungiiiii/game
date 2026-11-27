using Fungiiiii.Controls;
using Fungiiiii.Runtime.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fungiiiii.Runtime.Player
{
    /// <summary>
    /// The player's first-person camera. It sits at the player's eyes, turns with the mouse
    /// or the right stick, and hands its yaw to the <see cref="PlayerMotor"/> so that
    /// forward follows the view. Its actions come from the player's controls.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    internal sealed class FirstPersonCamera : MonoBehaviour
    {
        /// <summary>Degrees of rotation per pixel of mouse movement.</summary>
        private const float MouseDegreesPerPixel = 0.1f;

        private const float StickDegreesPerSecond = 150f;
        private const float MaxPitchDegrees = 85f;

        /// <summary>Eyes sit this far below the top of the controller capsule.</summary>
        private const float EyeDepthBelowTop = 0.25f;

        private InputAction lookAction;
        private InputAction cursorCaptureAction;
        private IMenuState menu;
        private PlayerMotor motor;
        private CharacterController characterController;
        private float yaw;
        private float pitch;

        public Camera Camera { get; private set; }

        /// <summary>
        /// Raised once the view has followed the player this frame. What is drawn over the
        /// world at a point of the screen is placed then, not on its own LateUpdate, whose
        /// order with this one Unity does not guarantee.
        /// </summary>
        public event System.Action Moved;

        /// <summary>Multiplies how fast the view turns: the net's aim slows it as it narrows the view.</summary>
        public float LookScale { get; set; } = 1f;

        /// <param name="look">Turns the view.</param>
        /// <param name="cursorCapture">Takes the cursor back after Escape released it in the Editor.</param>
        /// <param name="menuState">Under a menu the view stops turning and the pointer is freed.</param>
        public void Initialize(PlayerMotor playerMotor, float initialYawDegrees, InputAction look, InputAction cursorCapture, IMenuState menuState)
        {
            menu = menuState;
            menu.MenuOpenChanged += OnMenuOpenChanged;
            lookAction = look;
            cursorCaptureAction = cursorCapture;
            motor = playerMotor;
            characterController = playerMotor.GetComponent<CharacterController>();
            yaw = initialYawDegrees;
            pitch = 0f;
            motor.MovementYawDegrees = yaw;
            Follow();
        }

        /// <summary>Where the view stands and looks from, with the player: where it is put every frame.</summary>
        public Pose ViewPose
        {
            get
            {
                var eyeHeight = 0f;
                if (characterController != null)
                {
                    // The capsule shrinks with its feet planted while crouching: the eyes follow its top.
                    eyeHeight = characterController.center.y + characterController.height * 0.5f - EyeDepthBelowTop;
                }

                return new Pose(motor.transform.position + Vector3.up * eyeHeight, Quaternion.Euler(pitch, yaw, 0f));
            }
        }

        /// <summary>Turns the view to a point, from the player's eyes.</summary>
        public void LookAt(Vector3 point)
        {
            var toPoint = point - ViewPose.position;
            var flat = new Vector2(toPoint.x, toPoint.z).magnitude;
            SetView(Mathf.Atan2(toPoint.x, toPoint.z) * Mathf.Rad2Deg, -Mathf.Atan2(toPoint.y, flat) * Mathf.Rad2Deg);
        }

        /// <summary>Points the view, for a reset.</summary>
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

            // Turned back on after something else moved the camera (the opening shot): it is
            // with the player at once, not one frame later.
            Follow();
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDestroy()
        {
            if (menu != null)
            {
                menu.MenuOpenChanged -= OnMenuOpenChanged;
            }
        }

        private void OnMenuOpenChanged(bool open)
        {
            enabled = !open;
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

            var pose = ViewPose;
            transform.SetPositionAndRotation(pose.position, pose.rotation);
            Moved?.Invoke();
        }
    }
}
