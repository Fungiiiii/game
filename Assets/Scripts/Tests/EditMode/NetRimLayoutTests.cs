using System;
using Fungiiiii.Capture;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class NetRimLayoutTests
    {
        private const float Tolerance = 0.0001f;
        private const float ProbeHeight = 1.5f;
        private const float Lift = 0.03f;

        [Test]
        public void Flat_PutsEveryRimPointAtTheRadius_AtTheCentreHeight()
        {
            var centre = new Vector3(2f, 1f, -3f);
            var rim = new Vector3[16];

            NetRimLayout.Flat(centre, 0.6f, rim);

            foreach (var point in rim)
            {
                Assert.That(point.y, Is.EqualTo(1f).Within(Tolerance));
                Assert.That(NetImpact.HorizontalDistance(point, centre), Is.EqualTo(0.6f).Within(Tolerance));
            }
        }

        [Test]
        public void Drape_OnFlatGround_LaysEveryPointJustAboveTheGround()
        {
            var rim = new Vector3[12];

            var centre = NetRimLayout.Drape(new Vector3(0f, 0.4f, 0f), 1.5f, rim, FlatGroundAtZero, ProbeHeight, Lift);

            Assert.That(centre.y, Is.EqualTo(Lift).Within(Tolerance));
            foreach (var point in rim)
            {
                Assert.That(point.y, Is.EqualTo(Lift).Within(Tolerance));
                Assert.That(NetImpact.HorizontalDistance(point, Vector3.zero), Is.EqualTo(1.5f).Within(Tolerance));
            }
        }

        [Test]
        public void Drape_FollowsTheTerrain_PointByPoint()
        {
            var rim = new Vector3[8];

            NetRimLayout.Drape(Vector3.zero, 1f, rim, StepUpOnPositiveX, ProbeHeight, Lift);

            foreach (var point in rim)
            {
                var expected = point.x > 0.01f ? 0.5f + Lift : Lift;
                Assert.That(point.y, Is.EqualTo(expected).Within(Tolerance), $"Point at x = {point.x}");
            }
        }

        [Test]
        public void Drape_LeavesAPointWithNothingBelowItFlat()
        {
            var rim = new Vector3[8];

            var centre = NetRimLayout.Drape(new Vector3(0f, 2f, 0f), 1f, rim, NothingBelow, ProbeHeight, Lift);

            Assert.That(centre.y, Is.EqualTo(2f).Within(Tolerance));
            foreach (var point in rim)
            {
                Assert.That(point.y, Is.EqualTo(2f).Within(Tolerance));
            }
        }

        [Test]
        public void Drape_LooksDownFromAboveTheNet_SoItCanClimbOntoACap()
        {
            var rim = new Vector3[8];

            // A cap 0.8 m high under the centre only: the net lands beside it, at ground level.
            var centre = NetRimLayout.Drape(Vector3.zero, 1f, rim, CapUnderTheCentre, ProbeHeight, Lift);

            Assert.That(centre.y, Is.EqualTo(0.8f + Lift).Within(Tolerance));
            Assert.That(rim[0].y, Is.EqualTo(Lift).Within(Tolerance));
        }

        [Test]
        public void ARimNeedsAtLeastThreePoints()
        {
            Assert.Throws<ArgumentException>(() => NetRimLayout.Flat(Vector3.zero, 1f, new Vector3[2]));
        }

        private static bool FlatGroundAtZero(Vector3 from, float maxDistance, out Vector3 groundPoint)
        {
            groundPoint = new Vector3(from.x, 0f, from.z);
            return from.y >= 0f && from.y <= maxDistance;
        }

        private static bool StepUpOnPositiveX(Vector3 from, float maxDistance, out Vector3 groundPoint)
        {
            groundPoint = new Vector3(from.x, from.x > 0.01f ? 0.5f : 0f, from.z);
            return true;
        }

        private static bool NothingBelow(Vector3 from, float maxDistance, out Vector3 groundPoint)
        {
            groundPoint = from;
            return false;
        }

        private static bool CapUnderTheCentre(Vector3 from, float maxDistance, out Vector3 groundPoint)
        {
            var onCap = NetImpact.HorizontalDistance(from, Vector3.zero) < 0.3f;
            groundPoint = new Vector3(from.x, onCap ? 0.8f : 0f, from.z);
            return true;
        }
    }
}
