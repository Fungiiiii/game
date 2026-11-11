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

        [Test]
        public void InnerHoleStaysEmptyAndLeavesTheRestUnchanged()
        {
            var gameObject = new GameObject("ForestScatterHoleTest");
            try
            {
                var scatter = gameObject.AddComponent<ForestScatter>();
                scatter.Configure(Area, MinDistance, 1f, 1f, rotate: true, newSeed: 5);
                var hole = new Vector2(30f, 20f);
                var full = scatter.ComputePlacements(null);

                scatter.ConfigureLimits(hole, slope: 90f);
                var ring = scatter.ComputePlacements(null);

                Assert.That(ring, Is.Not.Empty);
                Assert.That(ring.Count, Is.LessThan(full.Count));
                foreach (var placement in ring)
                {
                    bool inHole = Mathf.Abs(placement.Position.x) < hole.x / 2f && Mathf.Abs(placement.Position.z) < hole.y / 2f;
                    Assert.That(inHole, Is.False);
                }

                // The hole removes placements without reshuffling the others.
                foreach (var placement in ring)
                {
                    Assert.That(full.Exists(p => p.Position == placement.Position && p.Yaw == placement.Yaw), Is.True);
                }
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void PathCirclesCoverTheWholeCorridor()
        {
            var gameObject = new GameObject("ScatterExclusionPathTest");
            try
            {
                var path = gameObject.AddComponent<ScatterExclusionPath>();
                var corners = new[] { new Vector3(0f, 0f, 0f), new Vector3(30f, 0f, 0f), new Vector3(30f, 0f, 25f) };
                path.Configure(corners, newRadius: 4f);

                List<ExclusionCircle> circles = path.ToCircles();

                // Any point within 3.5 m of the polyline is inside a circle.
                for (int i = 0; i + 1 < corners.Length; i++)
                {
                    Vector3 along = (corners[i + 1] - corners[i]).normalized;
                    Vector3 side = Vector3.Cross(Vector3.up, along);
                    for (float t = 0f; t <= 1f; t += 0.01f)
                    {
                        Vector3 point = Vector3.Lerp(corners[i], corners[i + 1], t) + side * 3.5f;
                        Assert.That(circles.Exists(circle => circle.Contains(point)), Is.True, $"gap at {point}");
                    }
                }

                Assert.That(circles.Exists(circle => circle.Contains(new Vector3(15f, 0f, 10f))), Is.False, "the path is wider than its radius");
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
