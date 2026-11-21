using System;
using System.Collections.Generic;
using Fungiiiii.Gathering;
using NUnit.Framework;
using UnityEngine;
using Random = System.Random;

namespace Fungiiiii.Tests.EditMode
{
    /// <summary>
    /// The spawner keeps a few resources around each player (playtest of 20/11, feedback
    /// #5): it replaced the spawner that spread them over the whole map, so these tests
    /// were rewritten for the new requirement.
    /// </summary>
    public sealed class ResourceSpawnerTests
    {
        private const float TickSeconds = 5f;
        private const int Target = 5;
        private const float Radius = 50f;
        private const float MinDistance = 20f;
        private const float StartMinDistance = 8f;
        private const float DespawnDistance = 75f;
        private const float SightDistance = 185f;
        private const float HalfView = 45f;

        /// <summary>A square of spots 2 m apart, centred on the origin, 200 m wide.</summary>
        private static readonly Vector3[] Field = Square(new Vector3(0f, 0f, 0f), 100f, 2f);

        private static Vector3[] Square(Vector3 centre, float halfSide, float step)
        {
            var side = Mathf.RoundToInt(halfSide * 2f / step) + 1;
            var spots = new Vector3[side * side];
            for (var index = 0; index < spots.Length; index++)
            {
                spots[index] = centre + new Vector3(index % side * step - halfSide, 0f, index / side * step - halfSide);
            }

            return spots;
        }

        private static ResourceSpawner Spawner(IReadOnlyList<Vector3> spots, int maxAlive = 40, int perTick = 1, int seed = 1)
        {
            return new ResourceSpawner(
                spots,
                new ResourceSpawner.Settings(
                    TickSeconds,
                    perTick,
                    Target,
                    Radius,
                    MinDistance,
                    StartMinDistance,
                    DespawnDistance,
                    SightDistance,
                    maxAlive),
                new Random(seed));
        }

        private static ResourceViewer Viewer(Vector3 position, Vector3 forward)
        {
            return new ResourceViewer(position, forward, HalfView);
        }

        private static ResourceViewer[] One(Vector3 position) => new[] { Viewer(position, Vector3.forward) };

        private static float GroundDistance(Vector3 a, Vector3 b)
        {
            var flat = a - b;
            flat.y = 0f;
            return flat.magnitude;
        }

        private static int CountWithin(ResourceSpawner spawner, IReadOnlyList<Vector3> spots, Vector3 position, float radius)
        {
            var count = 0;
            for (var spot = 0; spot < spots.Count; spot++)
            {
                if (spawner.IsOccupied(spot) && GroundDistance(spots[spot], position) <= radius)
                {
                    count++;
                }
            }

            return count;
        }

        private static void RunTicks(ResourceSpawner spawner, IReadOnlyList<ResourceViewer> viewers, int ticks, List<int> spawned, List<int> despawned = null)
        {
            despawned ??= new List<int>();
            for (var tick = 0; tick < ticks; tick++)
            {
                spawner.Tick(TickSeconds, viewers, spawned, despawned);
            }
        }

        [Test]
        public void Ticks_KeepTheTargetCountWithinTheBubble_AndNoMore()
        {
            var spawner = Spawner(Field);
            var viewers = One(Vector3.zero);
            var spawned = new List<int>();

            RunTicks(spawner, viewers, Target - 1, spawned);
            Assert.That(CountWithin(spawner, Field, Vector3.zero, Radius), Is.EqualTo(Target - 1), "One per tick.");

            RunTicks(spawner, viewers, 20, spawned);
            Assert.That(CountWithin(spawner, Field, Vector3.zero, Radius), Is.EqualTo(Target));
            Assert.That(spawner.AliveCount, Is.EqualTo(Target));
        }

        [Test]
        public void NothingAppears_BeforeATickHasElapsed()
        {
            var spawner = Spawner(Field);
            var spawned = new List<int>();

            spawner.Tick(TickSeconds - 0.1f, One(Vector3.zero), spawned, new List<int>());

            Assert.That(spawned, Is.Empty);
        }

        [Test]
        public void OnATick_NothingAppearsCloserThanTheMinimum_NorOutsideTheBubble()
        {
            var spawner = Spawner(Field);
            var spawned = new List<int>();

            RunTicks(spawner, One(Vector3.zero), 10, spawned);

            Assert.That(spawned, Has.Count.EqualTo(Target));
            foreach (var spot in spawned)
            {
                Assert.That(GroundDistance(Field[spot], Vector3.zero), Is.InRange(MinDistance, Radius));
            }
        }

        [Test]
        public void OnATick_NothingAppearsInTheFieldOfView_NorInItsMargin()
        {
            var spawner = Spawner(Field);
            var spawned = new List<int>();

            RunTicks(spawner, One(Vector3.zero), 10, spawned);

            foreach (var spot in spawned)
            {
                var angle = Vector3.Angle(Vector3.forward, Field[spot]);
                Assert.That(angle, Is.GreaterThan(HalfView + ResourceSpawner.SightMarginDegrees));
            }
        }

        [Test]
        public void TheFieldOfView_HidesSpotsEvenFarAway()
        {
            // Every spot is straight ahead of the player, near the edge of the bubble:
            // the fog of the forest does not hide that far, so none may pop in.
            var ahead = new[] { new Vector3(0f, 0f, 45f), new Vector3(3f, 0f, 48f) };
            var spawner = Spawner(ahead);
            var spawned = new List<int>();

            RunTicks(spawner, One(Vector3.zero), 5, spawned);

            Assert.That(spawned, Is.Empty);
        }

        [Test]
        public void BeyondTheSightDistance_ASpotInViewMayBeUsed()
        {
            // The second player looks straight at the first one's bubble, but from past the fog.
            var spawner = Spawner(Field);
            var viewers = new[]
            {
                Viewer(Vector3.zero, Vector3.forward),
                Viewer(new Vector3(0f, 0f, -300f), Vector3.forward),
            };

            RunTicks(spawner, viewers, 10, new List<int>());

            Assert.That(CountWithin(spawner, Field, Vector3.zero, Radius), Is.EqualTo(Target));
        }

        [Test]
        public void TheDistanceToThePlayer_IsMeasuredOnTheGround()
        {
            // Straight above the only spot: far in a straight line, but not on the ground.
            var spawner = Spawner(new[] { Vector3.zero });
            var spawned = new List<int>();

            RunTicks(spawner, new[] { Viewer(new Vector3(0f, 100f, 0f), Vector3.back) }, 3, spawned);

            Assert.That(spawned, Is.Empty);
        }

        [Test]
        public void Resources_GoAwayBeyondTheDespawnDistance_AndFreeTheirSpot()
        {
            var spawner = Spawner(Field);
            var spawned = new List<int>();
            RunTicks(spawner, One(Vector3.zero), 10, spawned);
            Assert.That(spawner.AliveCount, Is.EqualTo(Target));

            // A few metres away: everything is still within the despawn distance.
            var despawned = new List<int>();
            RunTicks(spawner, One(new Vector3(0f, 0f, 20f)), 1, new List<int>(), despawned);
            Assert.That(despawned, Is.Empty);

            var farAway = new[] { Viewer(new Vector3(500f, 0f, 500f), Vector3.forward) };
            RunTicks(spawner, farAway, 1, new List<int>(), despawned);

            Assert.That(spawner.AliveCount, Is.Zero);
            Assert.That(despawned, Is.SupersetOf(spawned));
            foreach (var spot in spawned)
            {
                Assert.That(spawner.IsOccupied(spot), Is.False);
            }
        }

        [Test]
        public void TheMaximumOverTheMap_HoldsForAllPlayersTogether()
        {
            var spots = new List<Vector3>(Field);
            spots.AddRange(Square(new Vector3(400f, 0f, 0f), 60f, 2f));
            spots.AddRange(Square(new Vector3(0f, 0f, 400f), 60f, 2f));
            var spawner = Spawner(spots, maxAlive: 12);
            var viewers = new[]
            {
                Viewer(Vector3.zero, Vector3.forward),
                Viewer(new Vector3(400f, 0f, 0f), Vector3.forward),
                Viewer(new Vector3(0f, 0f, 400f), Vector3.forward),
            };

            RunTicks(spawner, viewers, 20, new List<int>());

            Assert.That(spawner.AliveCount, Is.EqualTo(12));
        }

        [Test]
        public void TwoPlayersFarApart_EachGetTheirOwnBubble()
        {
            var spots = new List<Vector3>(Field);
            spots.AddRange(Square(new Vector3(300f, 0f, 0f), 60f, 2f));
            var spawner = Spawner(spots);
            var viewers = new[]
            {
                Viewer(Vector3.zero, Vector3.forward),
                Viewer(new Vector3(300f, 0f, 0f), Vector3.back),
            };

            RunTicks(spawner, viewers, 10, new List<int>());

            Assert.That(CountWithin(spawner, spots, Vector3.zero, Radius), Is.EqualTo(Target));
            Assert.That(CountWithin(spawner, spots, new Vector3(300f, 0f, 0f), Radius), Is.EqualTo(Target));
        }

        [Test]
        public void ASpotSeenByAnotherPlayer_IsNotUsed()
        {
            // The only spots behind the first player are right in front of the second.
            var spots = new[] { new Vector3(0f, 0f, -30f), new Vector3(2f, 0f, -32f) };
            var spawner = Spawner(spots);
            var viewers = new[]
            {
                Viewer(Vector3.zero, Vector3.forward),
                Viewer(new Vector3(0f, 0f, -70f), Vector3.forward),
            };

            RunTicks(spawner, viewers, 5, new List<int>());

            Assert.That(spawner.AliveCount, Is.Zero);
        }

        [Test]
        public void Fill_FillsTheBubbleAtOnce_FromTheStartDistance_EvenInView()
        {
            var spawner = Spawner(Field);
            var spawned = new List<int>();

            spawner.Fill(One(Vector3.zero), spawned);

            Assert.That(spawned, Has.Count.EqualTo(Target));
            Assert.That(spawned, Is.Unique);
            foreach (var spot in spawned)
            {
                Assert.That(GroundDistance(Field[spot], Vector3.zero), Is.InRange(StartMinDistance, Radius));
            }
        }

        [Test]
        public void Fill_CanUseSpotsCloserThanATick()
        {
            // Only spots between the start distance and the tick distance.
            var close = new[] { new Vector3(0f, 0f, -10f), new Vector3(10f, 0f, 0f) };
            var spawner = Spawner(close);
            var spawned = new List<int>();

            spawner.Fill(One(Vector3.zero), spawned);

            Assert.That(spawned, Has.Count.EqualTo(2));
        }

        [Test]
        public void AGatheredResource_FreesItsSpot_AndAnotherAppearsOnALaterTick()
        {
            var spawner = Spawner(Field);
            var spawned = new List<int>();
            spawner.Fill(One(Vector3.zero), spawned);

            spawner.Release(spawned[0]);
            Assert.That(spawner.AliveCount, Is.EqualTo(Target - 1));
            Assert.That(spawner.IsOccupied(spawned[0]), Is.False);

            var later = new List<int>();
            RunTicks(spawner, One(Vector3.zero), 1, later);

            Assert.That(later, Has.Count.EqualTo(1));
            Assert.That(spawner.AliveCount, Is.EqualTo(Target));
        }

        [Test]
        public void ReleasingAFreeSpot_ChangesNothing()
        {
            var spawner = Spawner(Field);

            spawner.Release(3);
            spawner.Release(3);

            Assert.That(spawner.AliveCount, Is.Zero);
        }

        [Test]
        public void TheSameSeed_GivesTheSameSpots()
        {
            var first = new List<int>();
            var second = new List<int>();
            var viewers = One(Vector3.zero);

            var a = Spawner(Field, seed: 42);
            a.Fill(viewers, first);
            RunTicks(a, viewers, 4, first);

            var b = Spawner(Field, seed: 42);
            b.Fill(viewers, second);
            RunTicks(b, viewers, 4, second);

            Assert.That(first, Is.EqualTo(second));
        }

        [Test]
        public void Clear_EmptiesTheMap_AndRestartsTheClock()
        {
            var spawner = Spawner(Field);
            var viewers = One(Vector3.zero);
            spawner.Fill(viewers, new List<int>());
            spawner.Tick(TickSeconds - 1f, viewers, new List<int>(), new List<int>());

            spawner.Clear();

            Assert.That(spawner.AliveCount, Is.Zero);
            var afterClear = new List<int>();
            spawner.Tick(2f, viewers, afterClear, new List<int>());
            Assert.That(afterClear, Is.Empty, "The clock kept the time elapsed before the clear.");
        }

        [Test]
        public void SettingsThatContradictEachOther_AreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new ResourceSpawner.Settings(0f, 1, Target, Radius, MinDistance, StartMinDistance, DespawnDistance, SightDistance, 40),
                "A tick that is not positive.");
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new ResourceSpawner.Settings(TickSeconds, 1, Target, Radius, 60f, StartMinDistance, DespawnDistance, SightDistance, 40),
                "A minimum distance past the bubble.");
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new ResourceSpawner.Settings(TickSeconds, 1, Target, Radius, MinDistance, StartMinDistance, 40f, SightDistance, 40),
                "Despawning inside the bubble.");
        }
    }
}
