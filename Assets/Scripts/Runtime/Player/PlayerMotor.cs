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

        [SerializeField]
        private PlayerInputReader inputReader;

        [SerializeField, Min(0f)]
        private float walkSpeed = 4f;

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
        /// Horizontal velocity applied during the last tick, in world space.
        /// </summary>
        public Vector3 HorizontalVelocity { get; private set; }

        public bool IsGrounded => characterController != null && characterController.isGrounded;

        public float WalkSpeed
        {
            get => walkSpeed;
            set => walkSpeed = Mathf.Max(0f, value);
        }

        /// <summary>
        /// Assigns the reader polled every frame. Used by the prototype bootstrap,
        /// which builds its objects at runtime and has no Inspector to wire.
        /// </summary>
        public void SetInputReader(PlayerInputReader reader)
        {
            inputReader = reader;
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

            HorizontalVelocity =
                PlayerMovementCalculator.CalculateHorizontalVelocity(MoveInput, MovementYawDegrees, walkSpeed);

            verticalVelocity = PlayerMovementCalculator.ApplyGravity(
                verticalVelocity,
                characterController.isGrounded,
                gravity,
                groundedStickSpeed,
                deltaTime);

            var displacement = HorizontalVelocity + Vector3.up * verticalVelocity;
            characterController.Move(displacement * deltaTime);

            FaceMovementDirection(deltaTime);
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (inputReader != null)
            {
                MoveInput = inputReader.MoveInput;
            }

            Tick(Time.deltaTime);
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
            groundedStickSpeed = Mathf.Max(0f, groundedStickSpeed);
            turnSpeedDegreesPerSecond = Mathf.Max(0f, turnSpeedDegreesPerSecond);
        }
    }
}
