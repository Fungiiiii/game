using UnityEngine;

namespace Fungiiiii.Runtime.Player
{
    /// <summary>
    /// Drives the player capsule from a move input.
    ///
    /// Movement is kinematic: a <see cref="CharacterController"/> sweeps the capsule
    /// against static colliders. This component owns the player position, and nothing
    /// else should write to this transform.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    public sealed class PlayerMotor : MonoBehaviour
    {
        /// <summary>
        /// Movement is world relative for this prototype, because the demo camera never
        /// rotates. A free camera will pass its own yaw here instead.
        /// </summary>
        private const float MovementYawDegrees = 0f;

        private const float NegligibleSqrMagnitude = 0.000001f;

        [Header("References")]
        [SerializeField]
        private PlayerInputReader inputReader;

        [Tooltip("Mesh squashed when crouching. The controller capsule is resized separately.")]
        [SerializeField]
        private Transform visualRoot;

        [Header("Ground speeds")]
        [SerializeField, Min(0f)]
        private float walkSpeed = 4f;

        [SerializeField, Min(0f)]
        private float sprintSpeed = 7f;

        [SerializeField, Min(0f)]
        private float crouchSpeed = 1.8f;

        [Header("Stance")]
        [SerializeField, Min(0.1f)]
        private float standingHeight = 2f;

        [SerializeField, Min(0.1f)]
        private float crouchHeight = 1.2f;

        [Tooltip("Metres of capsule height gained or lost per second while changing stance.")]
        [SerializeField, Min(0.1f)]
        private float stanceTransitionSpeed = 6f;

        [Header("Dodge")]
        [SerializeField, Min(0f)]
        private float dodgeSpeed = 10f;

        [SerializeField, Min(0.05f)]
        private float dodgeDurationSeconds = 0.25f;

        [SerializeField, Min(0.05f)]
        private float dodgeCooldownSeconds = 0.7f;

        [Header("Gravity and turning")]
        [Tooltip("Twice the real world value: a realistic fall feels floaty in a game.")]
        [SerializeField]
        private float gravity = -19.62f;

        [Tooltip("Downward speed kept while grounded so the controller keeps reporting contact.")]
        [SerializeField, Min(0f)]
        private float groundedStickSpeed = 2f;

        [SerializeField, Min(0f)]
        private float turnSpeedDegreesPerSecond = 720f;

        private CharacterController characterController;
        private PlayerDodge dodge;
        private float verticalVelocity;

        /// <summary>
        /// Input consumed by the next tick. Written by <see cref="Update"/> from the
        /// serialized reader, or directly by a test or another simulation driver.
        /// </summary>
        public Vector2 MoveInput { get; set; }

        /// <summary>True while the sprint control is held.</summary>
        public bool IsSprinting { get; set; }

        /// <summary>True while the crouch control is held.</summary>
        public bool IsCrouching { get; set; }

        /// <summary>Horizontal velocity applied during the last tick, in world space.</summary>
        public Vector3 HorizontalVelocity { get; private set; }

        /// <summary>True while a dodge is overriding normal movement.</summary>
        public bool IsDodging => dodge is { IsDodging: true };

        public bool IsGrounded => characterController != null && characterController.isGrounded;

        public float WalkSpeed
        {
            get => walkSpeed;
            set => walkSpeed = Mathf.Max(0f, value);
        }

        /// <summary>
        /// Requests a dodge along the current move direction, or straight ahead when
        /// there is no input. Refused while a dodge or its cooldown is running.
        /// </summary>
        public bool TryDodge()
        {
            if (dodge == null)
            {
                return false;
            }

            var inputDirection =
                PlayerMovementCalculator.CalculateHorizontalVelocity(MoveInput, MovementYawDegrees, 1f);

            var dodgeDirection =
                inputDirection.sqrMagnitude > NegligibleSqrMagnitude ? inputDirection : transform.forward;

            return dodge.TryStart(dodgeDirection);
        }

        /// <summary>
        /// Advances the player by a deterministic amount of time.
        ///
        /// Public so tests and other simulation drivers can tick movement without
        /// relying on frame timing, mirroring PatrolAgent.Tick.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f || characterController == null)
            {
                return;
            }

            dodge.Tick(deltaTime);

            if (dodge.IsDodging)
            {
                HorizontalVelocity = dodge.Velocity;
            }
            else
            {
                var speed = PlayerMovementCalculator.SelectSpeed(
                    IsSprinting,
                    IsCrouching,
                    walkSpeed,
                    sprintSpeed,
                    crouchSpeed);

                HorizontalVelocity =
                    PlayerMovementCalculator.CalculateHorizontalVelocity(MoveInput, MovementYawDegrees, speed);
            }

            verticalVelocity = PlayerMovementCalculator.ApplyGravity(
                verticalVelocity,
                characterController.isGrounded,
                gravity,
                groundedStickSpeed,
                deltaTime);

            var displacement = HorizontalVelocity + Vector3.up * verticalVelocity;
            characterController.Move(displacement * deltaTime);

            UpdateStance(deltaTime);
            FaceMovementDirection(deltaTime);
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            dodge = new PlayerDodge(new PlayerDodge.Configuration(
                dodgeSpeed,
                dodgeDurationSeconds,
                Mathf.Max(dodgeCooldownSeconds, dodgeDurationSeconds)));

            ApplyHeight(standingHeight);
        }

        private void Update()
        {
            if (inputReader != null)
            {
                MoveInput = inputReader.MoveInput;
                IsSprinting = inputReader.IsSprintHeld;
                IsCrouching = inputReader.IsCrouchHeld;

                if (inputReader.WasDodgePressedThisFrame)
                {
                    TryDodge();
                }
            }

            Tick(Time.deltaTime);
        }

        private void UpdateStance(float deltaTime)
        {
            var targetHeight = IsCrouching ? crouchHeight : standingHeight;
            var newHeight = Mathf.MoveTowards(
                characterController.height,
                targetHeight,
                stanceTransitionSpeed * deltaTime);

            ApplyHeight(newHeight);
        }

        private void ApplyHeight(float height)
        {
            // Keep the feet planted: shrinking the capsule around its own centre would
            // leave the player hovering half the lost height above the ground.
            var centreOffset = (height - standingHeight) * 0.5f;

            characterController.height = height;
            characterController.center = new Vector3(0f, centreOffset, 0f);

            if (visualRoot == null)
            {
                return;
            }

            visualRoot.localScale = new Vector3(1f, height / standingHeight, 1f);
            visualRoot.localPosition = new Vector3(0f, centreOffset, 0f);
        }

        private void FaceMovementDirection(float deltaTime)
        {
            if (HorizontalVelocity.sqrMagnitude <= NegligibleSqrMagnitude)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(HorizontalVelocity, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeedDegreesPerSecond * deltaTime);
        }

        private void OnValidate()
        {
            walkSpeed = Mathf.Max(0f, walkSpeed);
            sprintSpeed = Mathf.Max(0f, sprintSpeed);
            crouchSpeed = Mathf.Max(0f, crouchSpeed);
            standingHeight = Mathf.Max(0.1f, standingHeight);
            crouchHeight = Mathf.Clamp(crouchHeight, 0.1f, standingHeight);
            stanceTransitionSpeed = Mathf.Max(0.1f, stanceTransitionSpeed);
            dodgeDurationSeconds = Mathf.Max(0.05f, dodgeDurationSeconds);
            dodgeCooldownSeconds = Mathf.Max(dodgeCooldownSeconds, dodgeDurationSeconds);
            groundedStickSpeed = Mathf.Max(0f, groundedStickSpeed);
            turnSpeedDegreesPerSecond = Mathf.Max(0f, turnSpeedDegreesPerSecond);
        }
    }
}
