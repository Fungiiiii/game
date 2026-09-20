using UnityEngine;

namespace Fungiiiii.Runtime.Player
{
    /// <summary>
    /// Movement maths for the player character.
    ///
    /// The type holds no state and touches no component, so the movement rules can
    /// be unit tested without loading a scene or entering Play Mode.
    /// </summary>
    public static class PlayerMovementCalculator
    {
        private const float NegligibleSqrMagnitude = 0.000001f;

        /// <summary>
        /// Converts a raw move input into a world space horizontal velocity.
        ///
        /// Input longer than one unit is normalised, so holding two keys diagonally
        /// is not faster than holding one. <paramref name="yawDegrees"/> is the yaw
        /// the input is expressed relative to: pass a camera yaw for camera relative
        /// movement, or zero for world relative movement.
        /// </summary>
        public static Vector3 CalculateHorizontalVelocity(Vector2 moveInput, float yawDegrees, float speed)
        {
            if (speed <= 0f)
            {
                return Vector3.zero;
            }

            var sqrMagnitude = moveInput.sqrMagnitude;
            if (sqrMagnitude <= NegligibleSqrMagnitude)
            {
                return Vector3.zero;
            }

            if (sqrMagnitude > 1f)
            {
                moveInput /= Mathf.Sqrt(sqrMagnitude);
            }

            var localDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            return Quaternion.Euler(0f, yawDegrees, 0f) * localDirection * speed;
        }

        /// <summary>
        /// Picks the ground speed for the current stance.
        ///
        /// Crouching wins over sprinting: sneaking up on a shy mushroom must not be
        /// cancelled by a sprint key that is still held down.
        /// </summary>
        public static float SelectSpeed(
            bool isSprinting,
            bool isCrouching,
            float walkSpeed,
            float sprintSpeed,
            float crouchSpeed)
        {
            if (isCrouching)
            {
                return crouchSpeed;
            }

            return isSprinting ? sprintSpeed : walkSpeed;
        }

        /// <summary>
        /// Integrates gravity for a single step and returns the new vertical velocity.
        ///
        /// While grounded the velocity is clamped to a small downward value instead of
        /// zero: a <see cref="CharacterController"/> only reports itself grounded as
        /// long as it keeps being pushed into the floor.
        /// </summary>
        public static float ApplyGravity(
            float verticalVelocity,
            bool isGrounded,
            float gravity,
            float groundedStickSpeed,
            float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return verticalVelocity;
            }

            if (isGrounded && verticalVelocity <= 0f)
            {
                return -Mathf.Abs(groundedStickSpeed);
            }

            return verticalVelocity + gravity * deltaTime;
        }
    }
}
