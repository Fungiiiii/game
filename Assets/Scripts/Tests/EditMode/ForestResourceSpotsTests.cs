using System.Collections.Generic;
using System.Linq;
using Fungiiiii.Demo;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class ForestResourceSpotsTests
    {
        private const float StickClearance = 1.5f;
        private const float MushroomClearance = 1.2f;

        private static GatherableSpot At(float x, float z)
        {
            return new GatherableSpot(new Vector3(x, 0f, z), Quaternion.identity, Vector3.one);
        }

        private static List<GatherableSpot> Remove(List<GatherableSpot> mushrooms, IReadOnlyList<GatherableSpot> sticks, System.Func<Vector3, bool> blocked = null)
        {
            ForestResourceSpots.RemoveOverlaps(mushrooms, sticks, StickClearance, MushroomClearance, blocked ?? (_ => false));
            return mushrooms;
        }

        [Test]
        public void AMushroomCloseToAStick_IsRemoved()
        {
            var mushrooms = new List<GatherableSpot> { At(1.4f, 0f), At(0f, 1.6f) };

            Remove(mushrooms, new[] { At(0f, 0f) });

            Assert.That(mushrooms.Select(spot => spot.Position), Is.EqualTo(new[] { new Vector3(0f, 0f, 1.6f) }));
        }

        [Test]
        public void OfTwoMushroomsTooClose_TheFirstStays()
        {
            var mushrooms = new List<GatherableSpot> { At(10f, 10f), At(10.5f, 10.5f), At(20f, 20f), At(21.3f, 20f) };

            Remove(mushrooms, new GatherableSpot[0]);

            Assert.That(
                mushrooms.Select(spot => spot.Position),
                Is.EqualTo(new[] { new Vector3(10f, 0f, 10f), new Vector3(20f, 0f, 20f), new Vector3(21.3f, 0f, 20f) }));
        }

        [Test]
        public void ARemovedMushroom_DoesNotPushAwayTheNextOnes()
        {
            // The middle one is removed by the stick: the last is measured against what is kept.
            var mushrooms = new List<GatherableSpot> { At(0f, 0f), At(1f, 5f), At(2f, 5f) };

            Remove(mushrooms, new[] { At(0.4f, 5f) });

            Assert.That(
                mushrooms.Select(spot => spot.Position),
                Is.EqualTo(new[] { Vector3.zero, new Vector3(2f, 0f, 5f) }));
        }

        [Test]
        public void ABlockedMushroom_IsRemoved()
        {
            var mushrooms = new List<GatherableSpot> { At(0f, 0f), At(5f, 0f) };

            Remove(mushrooms, new GatherableSpot[0], point => point.x > 1f);

            Assert.That(mushrooms.Select(spot => spot.Position), Is.EqualTo(new[] { Vector3.zero }));
        }

        [Test]
        public void TheDistances_AreMeasuredOnTheGround()
        {
            var mushrooms = new List<GatherableSpot> { new GatherableSpot(new Vector3(0f, 5f, 0f), Quaternion.identity, Vector3.one) };

            Remove(mushrooms, new[] { At(0.5f, 0f) });

            Assert.That(mushrooms, Is.Empty);
        }
    }
}
