using Fungiiiii.Capture;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class NetTrajectoryTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void PositionAt_FollowsTheParabolaForTheGivenSpeedAndGravity()
        {
            var trajectory = new NetTrajectory(new Vector3(1f, 2f, 3f), new Vector3(0f, 4f, 10f), 9.81f);

            var position = trajectory.PositionAt(0.5f);

            Assert.That(position.x, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(position.y, Is.EqualTo(2f + 4f * 0.5f - 0.5f * 9.81f * 0.25f).Within(Tolerance));
            Assert.That(position.z, Is.EqualTo(3f + 10f * 0.5f).Within(Tolerance));
        }

        [Test]
        public void VelocityAt_LosesGravityTimesTimeVertically()
        {
            var trajectory = new NetTrajectory(Vector3.zero, new Vector3(2f, 5f, 0f), 10f);

            var velocity = trajectory.VelocityAt(0.3f);

            Assert.That(velocity.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(velocity.y, Is.EqualTo(2f).Within(Tolerance));
        }

        [Test]
        public void Launch_ScalesTheDirectionToTheLaunchSpeed()
        {
            var trajectory = NetTrajectory.Launch(Vector3.zero, new Vector3(0f, 3f, 4f), 10f, 9.81f);

            Assert.That(trajectory.InitialVelocity.y, Is.EqualTo(6f).Within(Tolerance));
            Assert.That(trajectory.InitialVelocity.z, Is.EqualTo(8f).Within(Tolerance));
        }

        [Test]
        public void TryFindLanding_OnFlatGround_LandsWhereTheParabolaMeetsTheGround()
        {
            // Thrown at 45° from the ground: it lands at speed² / g.
            var speed = 11f;
            var gravity = 9.81f;
            var trajectory = NetTrajectory.Launch(Vector3.zero, new Vector3(0f, 1f, 1f), speed, gravity);

            var landed = trajectory.TryFindLanding(0.02f, 5f, CastOnGroundPlane, out var landing);

            Assert.That(landed, Is.True);
            Assert.That(landing.Point.z, Is.EqualTo(speed * speed / gravity).Within(0.05f));
            Assert.That(landing.Point.y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(landing.FlightSeconds, Is.EqualTo(2f * speed * Mathf.Sqrt(0.5f) / gravity).Within(0.01f));
        }

        [Test]
        public void TryFindLanding_ReturnsFalse_WhenNothingIsHitBeforeTheMaximumFlightTime()
        {
            var trajectory = NetTrajectory.Launch(Vector3.zero, Vector3.up, 11f, 9.81f);

            var landed = trajectory.TryFindLanding(0.05f, 0.5f, NeverHit, out _);

            Assert.That(landed, Is.False);
        }

        private static bool CastOnGroundPlane(Vector3 from, Vector3 to, out Vector3 hitPoint)
        {
            hitPoint = to;
            if (from.y < 0f || to.y > 0f || Mathf.Approximately(from.y, to.y))
            {
                return false;
            }

            var fraction = from.y / (from.y - to.y);
            hitPoint = Vector3.Lerp(from, to, fraction);
            return true;
        }

        private static bool NeverHit(Vector3 from, Vector3 to, out Vector3 hitPoint)
        {
            hitPoint = to;
            return false;
        }
    }
}
