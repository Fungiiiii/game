using System;
using System.Collections.Generic;
using Fungiiiii.Gathering;
using NUnit.Framework;
using UnityEngine;
using Random = System.Random;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class ResourceSpawnerTests
    {
        private const float TickSeconds = 10f;

        private static readonly Vector3 FarAway = new Vector3(10000f, 0f, 10000f);

        /// <summary>A square grid of spots, 10 m apart, starting at the origin.</summary>
        private static Vector3[] Grid(int side)
        {
            var spots = new Vector3[side * side];
            for (var index = 0; index < spots.Length; index++)
            {
                spots[index] = new Vector3(index % side * 10f, 0f, index / side * 10f);
            }

            return spots;
        }

        private static ResourceSpawner Spawner(Vector3[] spots, int perTick, int maxAlive, float minDistance, int seed = 1)
        {
            return new ResourceSpawner(
                spots,
                new ResourceSpawner.Settings(TickSeconds, perTick, maxAlive, minDistance),
                new Random(seed));
        }

        [Test]
        public void Fill_PutsTheMaximumOnTheMap_OnDistinctSpots()
        {
            var spawner = Spawner(Grid(10), 1, 30, 20f);
            var spawned = new List<int>();

            spawner.Fill(spawned);

            Assert.That(spawned, Has.Count.EqualTo(30));
            Assert.That(spawned, Is.Unique);
            Assert.That(spawner.AliveCount, Is.EqualTo(30));
        }

        [Test]
        public void Fill_StopsWhenEverySpotIsTaken()
        {
            var spawner = Spawner(Grid(3), 1, 50, 0f);
            var spawned = new List<int>();

            spawner.Fill(spawned);

            Assert.That(spawned, Has.Count.EqualTo(9));
        }

        [Test]
        public void NothingAppears_BeforeATickHasElapsed()
        {
            var spawner = Spawner(Grid(10), 2, 30, 0f);
            var spawned = new List<int>();

            spawner.Tick(TickSeconds - 0.1f, FarAway, spawned);

            Assert.That(spawned, Is.Empty);
        }

        [Test]
        public void EachTick_AddsThePerTickCount()
        {
            var spawner = Spawner(Grid(10), 2, 30, 0f);
            var spawned = new List<int>();

            spawner.Tick(TickSeconds, FarAway, spawned);
            Assert.That(spawned, Has.Count.EqualTo(2));

            // Ticks add up over small frames, and a long frame holds several ticks.
            spawner.Tick(TickSeconds * 0.5f, FarAway, spawned);
            spawner.Tick(TickSeconds * 0.5f, FarAway, spawned);
            Assert.That(spawned, Has.Count.EqualTo(4));

            spawner.Tick(TickSeconds * 3f, FarAway, spawned);
            Assert.That(spawned, Has.Count.EqualTo(10));
            Assert.That(spawned, Is.Unique);
        }

        [Test]
        public void Ticks_NeverGoPastTheMaximum()
        {
            var spawner = Spawner(Grid(10), 3, 7, 0f);
            var spawned = new List<int>();

            spawner.Tick(TickSeconds * 20f, FarAway, spawned);

            Assert.That(spawned, Has.Count.EqualTo(7));
            Assert.That(spawner.AliveCount, Is.EqualTo(7));
        }

        [Test]
        public void NothingAppears_CloseToThePlayer()
        {
            var spots = Grid(10);
            var player = new Vector3(45f, 0f, 45f);
            const float minDistance = 20f;
            var spawner = Spawner(spots, 5, 60, minDistance);
            var spawned = new List<int>();

            spawner.Tick(TickSeconds * 12f, player, spawned);

            Assert.That(spawned, Is.Not.Empty);
            foreach (var spot in spawned)
            {
                Assert.That(Vector3.Distance(spots[spot], player), Is.GreaterThanOrEqualTo(minDistance));
            }
        }

        [Test]
        public void TheDistanceToThePlayer_IsMeasuredOnTheGround()
        {
            // Straight above the only spot: far in a straight line, but not on the ground.
            var spawner = Spawner(new[] { Vector3.zero }, 1, 1, 20f);
            var spawned = new List<int>();

            spawner.Tick(TickSeconds, new Vector3(0f, 100f, 0f), spawned);

            Assert.That(spawned, Is.Empty);
        }

        [Test]
        public void AGatheredResource_FreesItsSpot_AndAnotherAppearsOnALaterTick()
        {
            var spawner = Spawner(Grid(4), 1, 16, 0f);
            var spawned = new List<int>();
            spawner.Fill(spawned);
            Assert.That(spawner.AliveCount, Is.EqualTo(16));

            spawner.Release(spawned[0]);
            Assert.That(spawner.AliveCount, Is.EqualTo(15));
            Assert.That(spawner.IsOccupied(spawned[0]), Is.False);

            var later = new List<int>();
            spawner.Tick(TickSeconds, FarAway, later);

            Assert.That(later, Is.EqualTo(new[] { spawned[0] }));
            Assert.That(spawner.AliveCount, Is.EqualTo(16));
        }

        [Test]
        public void ReleasingAFreeSpot_ChangesNothing()
        {
            var spawner = Spawner(Grid(4), 1, 4, 0f);

            spawner.Release(3);
            spawner.Release(3);

            Assert.That(spawner.AliveCount, Is.Zero);

            var spawned = new List<int>();
            spawner.Fill(spawned);
            Assert.That(spawned, Is.Unique);
        }

        [Test]
        public void TheSameSeed_GivesTheSameSpots()
        {
            var first = new List<int>();
            var second = new List<int>();

            Spawner(Grid(10), 2, 30, 0f, seed: 42).Tick(TickSeconds * 8f, FarAway, first);
            Spawner(Grid(10), 2, 30, 0f, seed: 42).Tick(TickSeconds * 8f, FarAway, second);

            Assert.That(first, Is.EqualTo(second));
        }

        [Test]
        public void Clear_EmptiesTheMap_AndRestartsTheClock()
        {
            var spawner = Spawner(Grid(10), 1, 30, 0f);
            var spawned = new List<int>();
            spawner.Fill(spawned);
            spawner.Tick(TickSeconds - 1f, FarAway, spawned);

            spawner.Clear();

            Assert.That(spawner.AliveCount, Is.Zero);
            var afterClear = new List<int>();
            spawner.Tick(2f, FarAway, afterClear);
            Assert.That(afterClear, Is.Empty, "The clock kept the time elapsed before the clear.");
        }

        [Test]
        public void ATickThatIsNotPositive_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ResourceSpawner.Settings(0f, 1, 1, 0f));
        }
    }
}
