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

        [Header("Jump")]
        [Tooltip("How high the feet rise, in metres.")]
        [SerializeField, Min(0f)]
        private float jumpHeight = 1.2f;

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
        private float verticalVelocity;

        /// <summary>
        /// Input consumed by the next tick. Written by <see cref="Update"/> from the
        /// serialized reader, or directly by a test or another simulation driver.
        /// </summary>
        public Vector2 MoveInput { get; set; }

        /// <summary>
        /// Yaw, in degrees, the move input is expressed relative to. Zero keeps movement
        /// world relative, as the overhead demo camera never rotates; a first-person
        /// camera writes its own yaw here so that forward follows the view.
        /// </summary>
        public float MovementYawDegrees { get; set; }

        /// <summary>True while the sprint control is held.</summary>
        public bool IsSprinting { get; set; }

        /// <summary>True while the crouch control is held.</summary>
        public bool IsCrouching { get; set; }

        /// <summary>
        /// False keeps the player at walking speed even with sprint held, for instance
        /// while stamina is exhausted. True by default: without a stamina owner, nothing changes.
        /// </summary>
        public bool SprintAllowed { get; set; } = true;

        /// <summary>True if the last tick moved the player at sprint speed.</summary>
        public bool IsSprintMoving { get; private set; }

        /// <summary>Horizontal velocity applied during the last tick, in world space.</summary>
        public Vector3 HorizontalVelocity { get; private set; }

        public bool IsGrounded => characterController != null && characterController.isGrounded;

        public float WalkSpeed
        {
            get => walkSpeed;
            set => walkSpeed = Mathf.Max(0f, value);
        }

        /// <summary>
        /// Leaves the ground with the speed that reaches <see cref="jumpHeight"/>. Refused in
        /// the air and while crouching.
        /// </summary>
        public bool TryJump()
        {
            if (characterController == null || !characterController.isGrounded || IsCrouching)
            {
                return false;
            }

            verticalVelocity = PlayerMovementCalculator.JumpSpeed(jumpHeight, gravity);
            return true;
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

            var sprinting = IsSprinting && SprintAllowed;
            IsSprintMoving = sprinting && !IsCrouching && MoveInput.sqrMagnitude > NegligibleSqrMagnitude;

            var speed = PlayerMovementCalculator.SelectSpeed(
                sprinting,
                IsCrouching,
                walkSpeed,
                sprintSpeed,
                crouchSpeed);

            HorizontalVelocity =
                PlayerMovementCalculator.CalculateHorizontalVelocity(MoveInput, MovementYawDegrees, speed);

            var previousVerticalVelocity = verticalVelocity;
            verticalVelocity = PlayerMovementCalculator.ApplyGravity(
                verticalVelocity,
                characterController.isGrounded,
                gravity,
                groundedStickSpeed,
                deltaTime);

            // The mean of the speeds before and after the step is exact under constant
            // gravity: a jump rises as high at any frame rate.
            var meanVerticalVelocity = (previousVerticalVelocity + verticalVelocity) * 0.5f;
            var displacement = HorizontalVelocity + Vector3.up * meanVerticalVelocity;
            characterController.Move(displacement * deltaTime);

            UpdateStance(deltaTime);
            FaceMovementDirection(deltaTime);
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            ApplyHeight(standingHeight);
        }

        private void Update()
        {
            if (inputReader != null)
            {
                MoveInput = inputReader.MoveInput;
                IsSprinting = inputReader.IsSprintHeld;
                IsCrouching = inputReader.IsCrouchHeld;

                if (inputReader.WasJumpPressedThisFrame)
                {
                    TryJump();
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
            jumpHeight = Mathf.Max(0f, jumpHeight);
            groundedStickSpeed = Mathf.Max(0f, groundedStickSpeed);
            turnSpeedDegreesPerSecond = Mathf.Max(0f, turnSpeedDegreesPerSecond);
        }
    }
}
