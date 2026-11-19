using Fungiiiii.Capture;
using NUnit.Framework;
using UnityEngine;
using Random = System.Random;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class MushroomWanderTests
    {
        private const float Radius = 5f;
        private const float Speed = 1f;
        private const float DeltaTime = 0.1f;

        [Test]
        public void AWanderingMushroom_StaysWithinItsRadiusOfHome()
        {
            var home = new Vector3(10f, 2f, -4f);
            var wander = new MushroomWander(home, Radius, Speed, pauseSeconds: 0.5f, new Random(11));
            var position = home;

            for (var step = 0; step < 5000; step++)
            {
                position = wander.Step(position, DeltaTime);
                var flat = position - home;
                flat.y = 0f;

                Assert.That(flat.magnitude, Is.LessThanOrEqualTo(Radius + 0.001f));
            }
        }

        [Test]
        public void EachStep_MovesAtMostItsSpeed_OnTheGroundPlane()
        {
            var home = Vector3.zero;
            var wander = new MushroomWander(home, Radius, Speed, pauseSeconds: 0f, new Random(5));
            var position = home;

            for (var step = 0; step < 500; step++)
            {
                var next = wander.Step(position, DeltaTime);

                Assert.That((next - position).magnitude, Is.LessThanOrEqualTo(Speed * DeltaTime + 0.0001f));
                Assert.That(next.y, Is.EqualTo(position.y));
                position = next;
            }
        }

        [Test]
        public void AWanderingMushroom_ActuallyMoves()
        {
            var home = Vector3.zero;
            var wander = new MushroomWander(home, Radius, Speed, pauseSeconds: 0f, new Random(5));
            var position = home;

            for (var step = 0; step < 50; step++)
            {
                position = wander.Step(position, DeltaTime);
            }

            Assert.That(Vector3.Distance(position, home), Is.GreaterThan(0.1f));
        }

        [Test]
        public void TheSameSeed_GivesTheSameStroll()
        {
            var first = new MushroomWander(Vector3.zero, Radius, Speed, 0.5f, new Random(9));
            var second = new MushroomWander(Vector3.zero, Radius, Speed, 0.5f, new Random(9));
            var a = Vector3.zero;
            var b = Vector3.zero;

            for (var step = 0; step < 300; step++)
            {
                a = first.Step(a, DeltaTime);
                b = second.Step(b, DeltaTime);
            }

            Assert.That(a, Is.EqualTo(b));
        }

        [Test]
        public void AMushroomFarFromHome_WalksBackTowardsIt()
        {
            var home = Vector3.zero;
            var wander = new MushroomWander(home, Radius, Speed, pauseSeconds: 0f, new Random(2));
            var position = new Vector3(20f, 0f, 0f);

            var next = wander.Step(position, 1f);

            Assert.That(next.x, Is.LessThan(position.x));
        }
    }
}
