using System.Collections.Generic;
using Fungiiiii.Capture;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class NetImpactTests
    {
        private const float Radius = 1.5f;

        [Test]
        public void SelectTarget_ReturnsMinusOne_WhenNoMushroomIsInTheZone()
        {
            var positions = new List<Vector3> { new Vector3(3f, 0f, 0f), new Vector3(0f, 0f, -2f) };

            Assert.That(NetImpact.SelectTarget(Vector3.zero, Radius, positions), Is.EqualTo(-1));
        }

        [Test]
        public void SelectTarget_ReturnsMinusOne_WithNoMushroomAtAll()
        {
            Assert.That(NetImpact.SelectTarget(Vector3.zero, Radius, new List<Vector3>()), Is.EqualTo(-1));
        }

        [Test]
        public void SelectTarget_ReturnsTheOnlyMushroomInTheZone()
        {
            var positions = new List<Vector3> { new Vector3(5f, 0f, 0f), new Vector3(1f, 0f, 0.5f) };

            Assert.That(NetImpact.SelectTarget(Vector3.zero, Radius, positions), Is.EqualTo(1));
        }

        [Test]
        public void SelectTarget_PicksTheMushroomClosestToTheCentre()
        {
            var positions = new List<Vector3>
            {
                new Vector3(1.2f, 0f, 0f),
                new Vector3(0f, 0f, 0.4f),
                new Vector3(-0.8f, 0f, 0f)
            };

            Assert.That(NetImpact.SelectTarget(Vector3.zero, Radius, positions), Is.EqualTo(1));
        }

        [Test]
        public void SelectTarget_IncludesAMushroomExactlyOnTheEdge()
        {
            var positions = new List<Vector3> { new Vector3(Radius, 0f, 0f) };

            Assert.That(NetImpact.SelectTarget(Vector3.zero, Radius, positions), Is.EqualTo(0));
        }

        [Test]
        public void SelectTarget_MeasuresOnTheGroundPlane_IgnoringHeight()
        {
            // A net resting on a tall cap still targets the mushroom under it.
            var positions = new List<Vector3> { new Vector3(0.5f, -3f, 0f) };

            Assert.That(NetImpact.SelectTarget(Vector3.zero, Radius, positions), Is.EqualTo(0));
        }

        [Test]
        public void FindWithin_ListsEveryMushroomInsideTheRadius()
        {
            var positions = new List<Vector3>
            {
                new Vector3(2f, 0f, 0f),
                new Vector3(3.5f, 0f, 0f),
                new Vector3(0f, 0f, -3f)
            };
            var results = new List<int> { 42 };

            NetImpact.FindWithin(Vector3.zero, 3f, positions, results);

            Assert.That(results, Is.EqualTo(new[] { 0, 2 }));
        }
    }
}
