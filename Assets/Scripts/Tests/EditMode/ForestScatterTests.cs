using System.Collections.Generic;
using Fungiiiii.Runtime.World;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class ForestScatterTests
    {
        private const float MinDistance = 5f;
        private static readonly Vector2 Area = new Vector2(60f, 60f);

        [Test]
        public void SamplerRespectsMinimumDistance()
        {
            var points = PoissonDiskSampler.Sample(Area, MinDistance, seed: 42);

            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    Assert.That(Vector2.Distance(points[i], points[j]), Is.GreaterThanOrEqualTo(MinDistance - 0.0001f));
                }
            }
        }

        [Test]
        public void SamplerStaysInsideAreaAndFillsIt()
        {
            var points = PoissonDiskSampler.Sample(Area, MinDistance, seed: 7);

            // A 60 x 60 area holds at most ~166 points at 5 m; Bridson reliably fills well over a third.
            Assert.That(points.Count, Is.GreaterThan(60));
            foreach (var p in points)
            {
                Assert.That(p.x, Is.InRange(0f, Area.x));
                Assert.That(p.y, Is.InRange(0f, Area.y));
            }
        }

        [Test]
        public void SameSeedGivesSameResult()
        {
            var first = PoissonDiskSampler.Sample(Area, MinDistance, seed: 3);
            var second = PoissonDiskSampler.Sample(Area, MinDistance, seed: 3);
            var other = PoissonDiskSampler.Sample(Area, MinDistance, seed: 4);

            Assert.That(second, Is.EqualTo(first));
            Assert.That(other, Is.Not.EqualTo(first));
        }

        [Test]
        public void PlacementsRespectScaleRangeRotationAndExclusions()
        {
            var gameObject = new GameObject("ForestScatterTest");
            try
            {
                var scatter = gameObject.AddComponent<ForestScatter>();
                scatter.Configure(Area, MinDistance, 0.85f, 1.25f, rotate: false, newSeed: 1);
                var zone = new ExclusionCircle(Vector3.zero, 10f);

                var placements = scatter.ComputePlacements(new List<ExclusionCircle> { zone });

                Assert.That(placements, Is.Not.Empty);
                foreach (var placement in placements)
                {
                    Assert.That(placement.Scale, Is.InRange(0.85f, 1.25f));
                    Assert.That(placement.Yaw, Is.EqualTo(0f));
                    Assert.That(zone.Contains(placement.Position), Is.False);
                }
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
