using System.Collections.Generic;
using Fungiiiii.Capture;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class NetPreviewDotsTests
    {
        private const float Speed = 11f;
        private const float Gravity = 9.81f;
        private const float HandHeight = 1.45f;

        /// <summary>Flight from the hand, at <paramref name="pitchDegrees"/> above the horizon, until it reaches the flat ground.</summary>
        private static List<Vector3> DotsFor(float pitchDegrees)
        {
            var direction = Quaternion.Euler(-pitchDegrees, 0f, 0f) * Vector3.forward;
            var trajectory = NetTrajectory.Launch(new Vector3(0f, HandHeight, 0f), direction, Speed, Gravity);
            trajectory.TryFindLanding(NetPreviewDots.TimeStep, 3f, Ground, out var landing);

            var dots = new List<Vector3>();
            NetPreviewDots.Place(trajectory, landing.FlightSeconds, dots);
            return dots;
        }

        private static bool Ground(Vector3 from, Vector3 to, out Vector3 hitPoint)
        {
            hitPoint = to;
            if (to.y > 0f)
            {
                return false;
            }

            var along = from.y / (from.y - to.y);
            hitPoint = Vector3.Lerp(from, to, along);
            return true;
        }

        // A millimetre: the dots must not jump by a sample when the aim moves slightly.
        private const float Tolerance = 0.001f;

        [TestCase(-20f)]
        [TestCase(0f)]
        [TestCase(15f)]
        [TestCase(30f)]
        public void TwoDotsInARow_AreExactlyTheSpacingApart(float pitchDegrees)
        {
            var dots = DotsFor(pitchDegrees);

            for (var index = 1; index < dots.Count; index++)
            {
                Assert.That(Vector3.Distance(dots[index - 1], dots[index]), Is.EqualTo(NetPreviewDots.Spacing).Within(Tolerance));
            }
        }

        [TestCase(-20f)]
        [TestCase(15f)]
        public void TheFirstDot_IsExactlyTheClearanceFromTheHand(float pitchDegrees)
        {
            var hand = new Vector3(0f, HandHeight, 0f);

            var dots = DotsFor(pitchDegrees);

            Assert.That(Vector3.Distance(dots[0], hand), Is.EqualTo(NetPreviewDots.Clearance).Within(Tolerance));
        }

        [Test]
        public void ALongFlight_HasNoMoreThanTheMaximum()
        {
            var trajectory = NetTrajectory.Launch(Vector3.zero, Vector3.forward, 40f, Gravity);
            var dots = new List<Vector3>();

            NetPreviewDots.Place(trajectory, 3f, dots);

            Assert.That(dots, Has.Count.EqualTo(NetPreviewDots.MaxDots));
        }

        [Test]
        public void TheNumberOfDots_GrowsWithTheRange()
        {
            var down = DotsFor(-20f).Count;
            var level = DotsFor(0f).Count;
            var up = DotsFor(15f).Count;
            var higher = DotsFor(30f).Count;

            Assert.That(down, Is.GreaterThan(0));
            Assert.That(level, Is.GreaterThan(down));
            Assert.That(up, Is.GreaterThan(level));
            Assert.That(higher, Is.GreaterThan(up));
        }

        [Test]
        public void ThePreviousDotsAreCleared()
        {
            var trajectory = NetTrajectory.Launch(Vector3.zero, Vector3.forward, Speed, Gravity);
            var dots = new List<Vector3> { Vector3.one * 99f };

            NetPreviewDots.Place(trajectory, 0.5f, dots);

            Assert.That(dots, Has.No.Member(Vector3.one * 99f));
        }
    }
}
