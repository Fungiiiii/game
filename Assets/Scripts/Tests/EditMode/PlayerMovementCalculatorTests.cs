using Fungiiiii.Runtime.Player;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class PlayerMovementCalculatorTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void ForwardInputMovesAlongPositiveZ()
        {
            var velocity = PlayerMovementCalculator.CalculateHorizontalVelocity(Vector2.up, 0f, 4f);

            Assert.That(velocity.x, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(velocity.y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(velocity.z, Is.EqualTo(4f).Within(Tolerance));
        }

        [Test]
        public void DiagonalInputIsNotFasterThanStraightInput()
        {
            var straight = PlayerMovementCalculator.CalculateHorizontalVelocity(Vector2.up, 0f, 4f);
            var diagonal = PlayerMovementCalculator.CalculateHorizontalVelocity(Vector2.one, 0f, 4f);

            Assert.That(diagonal.magnitude, Is.EqualTo(straight.magnitude).Within(Tolerance));
        }

        [Test]
        public void PartialInputKeepsItsMagnitude()
        {
            var velocity = PlayerMovementCalculator.CalculateHorizontalVelocity(new Vector2(0f, 0.5f), 0f, 4f);

            Assert.That(velocity.magnitude, Is.EqualTo(2f).Within(Tolerance));
        }

        [Test]
        public void YawRotatesInputIntoReferenceSpace()
        {
            var velocity = PlayerMovementCalculator.CalculateHorizontalVelocity(Vector2.up, 90f, 4f);

            Assert.That(velocity.x, Is.EqualTo(4f).Within(Tolerance));
            Assert.That(velocity.z, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ZeroInputProducesNoVelocity()
        {
            var velocity = PlayerMovementCalculator.CalculateHorizontalVelocity(Vector2.zero, 0f, 4f);

            Assert.That(velocity, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void ZeroSpeedProducesNoVelocity()
        {
            var velocity = PlayerMovementCalculator.CalculateHorizontalVelocity(Vector2.up, 0f, 0f);

            Assert.That(velocity, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void WalkSpeedIsUsedWhenNeitherSprintingNorCrouching()
        {
            var speed = PlayerMovementCalculator.SelectSpeed(false, false, 4f, 7f, 1.8f);

            Assert.That(speed, Is.EqualTo(4f).Within(Tolerance));
        }

        [Test]
        public void SprintSpeedIsUsedWhenSprinting()
        {
            var speed = PlayerMovementCalculator.SelectSpeed(true, false, 4f, 7f, 1.8f);

            Assert.That(speed, Is.EqualTo(7f).Within(Tolerance));
        }

        [Test]
        public void CrouchSpeedIsUsedWhenCrouching()
        {
            var speed = PlayerMovementCalculator.SelectSpeed(false, true, 4f, 7f, 1.8f);

            Assert.That(speed, Is.EqualTo(1.8f).Within(Tolerance));
        }

        [Test]
        public void CrouchingWinsOverAHeldSprint()
        {
            var speed = PlayerMovementCalculator.SelectSpeed(true, true, 4f, 7f, 1.8f);

            Assert.That(speed, Is.EqualTo(1.8f).Within(Tolerance));
        }

        [Test]
        public void GravityAccumulatesWhileAirborne()
        {
            var velocity = PlayerMovementCalculator.ApplyGravity(0f, false, -10f, 2f, 0.5f);

            Assert.That(velocity, Is.EqualTo(-5f).Within(Tolerance));
        }

        [Test]
        public void GroundedVelocityIsClampedToStickSpeed()
        {
            var velocity = PlayerMovementCalculator.ApplyGravity(-50f, true, -10f, 2f, 0.5f);

            Assert.That(velocity, Is.EqualTo(-2f).Within(Tolerance));
        }

        [Test]
        public void GroundedUpwardVelocityStillIntegratesGravity()
        {
            var velocity = PlayerMovementCalculator.ApplyGravity(5f, true, -10f, 2f, 0.5f);

            Assert.That(velocity, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void NonPositiveDeltaTimeLeavesVerticalVelocityUnchanged()
        {
            var velocity = PlayerMovementCalculator.ApplyGravity(-3f, false, -10f, 2f, 0f);

            Assert.That(velocity, Is.EqualTo(-3f).Within(Tolerance));
        }
    }
}
