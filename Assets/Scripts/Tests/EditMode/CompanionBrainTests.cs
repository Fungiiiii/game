using System.Collections.Generic;
using Fungiiiii.Companions;
using Fungiiiii.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class CompanionBrainTests
    {
        private const float Frame = 1f / 60f;
        private const float FollowDistance = 2.5f;
        private const float StartDistance = 5f;
        private const float SideOffset = 1f;
        private const float WalkSpeed = 3.5f;
        private const float RunSpeed = 8f;
        private const float CatchUpGain = 1f;
        private const float Acceleration = 12f;
        private const float CatchUp = 25f;
        private const float SeekRadius = 10f;
        private const float SoakSeconds = 1f;
        private const float Arrival = 0.6f;

        /// <summary>Its place with the player at the origin looking along +Z: behind, on their right.</summary>
        private static readonly Vector3 PlaceAtOrigin = new Vector3(SideOffset, 0f, -FollowDistance);

        private ItemDefinition water;
        private ItemDefinition wood;

        private sealed class Source : IAbsorbable
        {
            public Source(Vector3 position, ItemDefinition yield, bool full = true)
            {
                Position = position;
                Yield = yield;
                CanBeAbsorbed = full;
            }

            public Vector3 Position { get; set; }

            public ItemDefinition Yield { get; }

            public bool CanBeAbsorbed { get; set; }

            public int Absorbed { get; private set; }

            public bool Absorb()
            {
                if (!CanBeAbsorbed)
                {
                    return false;
                }

                CanBeAbsorbed = false;
                Absorbed++;
                return true;
            }
        }

        [SetUp]
        public void CreateItems()
        {
            water = ItemDefinition.Create("water", Color.blue, 16);
            wood = ItemDefinition.Create("wood", Color.red, 16);
        }

        [TearDown]
        public void DestroyItems()
        {
            Object.DestroyImmediate(water);
            Object.DestroyImmediate(wood);
        }

        private CompanionBrain NewBrain()
        {
            return new CompanionBrain(
                new CompanionTuning(FollowDistance, StartDistance, SideOffset, WalkSpeed, RunSpeed, CatchUpGain, Acceleration, CatchUp, SeekRadius, SoakSeconds, Arrival),
                water);
        }

        /// <summary>Runs the brain for a while and returns where the companion ends up.</summary>
        private static Vector3 Run(CompanionBrain brain, Vector3 position, Vector3 player, IReadOnlyList<IAbsorbable> sources, float seconds)
        {
            for (var elapsed = 0f; elapsed < seconds; elapsed += Frame)
            {
                position = brain.Step(position, player, Vector3.forward, sources, Frame).Position;
            }

            return position;
        }

        private static float Flat(Vector3 a, Vector3 b)
        {
            return new Vector2(a.x - b.x, a.z - b.z).magnitude;
        }

        private static readonly IAbsorbable[] None = new IAbsorbable[0];

        [Test]
        public void Far_ItGoesToItsPlaceBehindAndBesideThePlayer_AndRestsThere()
        {
            var brain = NewBrain();
            var end = Run(brain, new Vector3(-12f, 0f, 6f), Vector3.zero, None, 5f);

            Assert.That(Flat(end, PlaceAtOrigin), Is.LessThan(0.35f), "It did not reach its place behind and beside the player.");
            Assert.That(brain.IsResting, Is.True);
            Assert.That(brain.Velocity.magnitude, Is.LessThan(0.01f), "It did not stop.");
        }

        [Test]
        public void Resting_ASmallStepOrATurnOfThePlayer_DoesNotMoveIt()
        {
            var brain = NewBrain();
            var start = new Vector3(3f, 0f, 1f);
            var end = Run(brain, start, Vector3.zero, None, 1f);
            Assert.That(end, Is.EqualTo(start), "Resting close to the player, it moved.");

            // The player steps 1.5 m away: still within its start distance.
            end = Run(brain, end, new Vector3(-1.5f, 0f, 0f), None, 1f);
            Assert.That(end, Is.EqualTo(start), "It set off for a small step.");
        }

        [Test]
        public void OnceThePlayerHasGone_ItSetsOff_SpeedingUpWithoutJumping()
        {
            var brain = NewBrain();
            var position = new Vector3(0f, 0f, -StartDistance - 0.5f);
            var step = brain.Step(position, Vector3.zero, Vector3.forward, None, Frame);

            Assert.That(step.Moved, Is.True, "It did not set off with the player beyond its start distance.");
            Assert.That(step.Velocity.magnitude, Is.LessThanOrEqualTo(Acceleration * Frame + 0.0001f), "It reached its speed at once.");
        }

        [Test]
        public void ItNeverGoesFasterThanItsRun_AndKeepsItsHeight()
        {
            var brain = NewBrain();
            var position = new Vector3(0f, 2f, -24f);
            for (var frame = 0; frame < 120; frame++)
            {
                var step = brain.Step(position, Vector3.zero, Vector3.forward, None, Frame);
                Assert.That(step.Velocity.magnitude, Is.LessThanOrEqualTo(RunSpeed + 0.0001f));
                Assert.That(step.Position.y, Is.EqualTo(2f));
                position = step.Position;
            }
        }

        [Test]
        public void BehindAWalkingPlayer_ItWalks_BehindASprintingPlayer_ItRuns_LaggingButKeepingUp()
        {
            foreach (var (playerSpeed, maxLag) in new[] { (4f, 7f), (7f, 10f) })
            {
                var brain = NewBrain();
                var position = PlaceAtOrigin;
                var player = Vector3.zero;
                var lags = new List<float>();
                for (var elapsed = 0f; elapsed < 10f; elapsed += Frame)
                {
                    player += Vector3.forward * playerSpeed * Frame;
                    position = brain.Step(position, player, Vector3.forward, None, Frame).Position;
                    lags.Add(Flat(position, player));
                }

                var lag = lags[lags.Count - 1];
                Assert.That(lag, Is.GreaterThan(StartDistance), $"At {playerSpeed} m/s it sticks to the player.");
                Assert.That(lag, Is.LessThan(maxLag), $"At {playerSpeed} m/s it falls behind for good.");
                Assert.That(brain.Velocity.magnitude, Is.EqualTo(playerSpeed).Within(0.2f), $"At {playerSpeed} m/s it does not match the player's pace.");
            }
        }

        [Test]
        public void TooFarFromThePlayer_ItComesBackToItsPlace_AtOnce()
        {
            var brain = NewBrain();
            var step = brain.Step(new Vector3(40f, 1f, 0f), Vector3.zero, Vector3.forward, None, Frame);

            Assert.That(step.CaughtUp, Is.True);
            Assert.That(step.Position.x, Is.EqualTo(PlaceAtOrigin.x).Within(0.001f));
            Assert.That(step.Position.z, Is.EqualTo(PlaceAtOrigin.z).Within(0.001f), "It does not come back behind the player.");
            Assert.That(step.Velocity, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Empty_ItGoesToTheNearestFullSourceCloseToThePlayer_SoaksIt_AndComesBackFull()
        {
            var brain = NewBrain();
            var near = new Source(new Vector3(5f, 0f, 0f), water);
            var further = new Source(new Vector3(-8f, 0f, 0f), water);
            var sources = new IAbsorbable[] { further, near };

            var end = Run(brain, new Vector3(2f, 0f, 0f), Vector3.zero, sources, 5f);

            Assert.That(near.Absorbed, Is.EqualTo(1), "It did not soak up the nearest source.");
            Assert.That(further.Absorbed, Is.Zero, "Full, it went for another source.");
            Assert.That(brain.IsFull, Is.True);
            Assert.That(brain.Fullness, Is.EqualTo(1f));
            Assert.That(brain.Activity, Is.EqualTo(CompanionActivity.Following));
            Assert.That(Flat(end, PlaceAtOrigin), Is.LessThan(0.35f), "Full, it did not come back to its place.");
        }

        [Test]
        public void Soaking_TakesItsTime_AndTheFullnessGrows()
        {
            var brain = NewBrain();
            var source = new Source(new Vector3(0.3f, 0f, 0f), water);
            var sources = new IAbsorbable[] { source };

            brain.Step(Vector3.zero, Vector3.zero, Vector3.forward, sources, Frame);
            Assert.That(brain.Activity, Is.EqualTo(CompanionActivity.Soaking), "Standing on the source, it did not start soaking.");

            Run(brain, Vector3.zero, Vector3.zero, sources, 0.5f);
            Assert.That(brain.Fullness, Is.EqualTo(0.5f).Within(0.05f));
            Assert.That(source.Absorbed, Is.Zero, "It soaked up before the time.");

            Run(brain, Vector3.zero, Vector3.zero, sources, 0.6f);
            Assert.That(source.Absorbed, Is.EqualTo(1));
            Assert.That(brain.IsFull, Is.True);
        }

        [Test]
        public void ItIgnores_EmptySources_FarSources_AndWhatYieldsAnotherItem()
        {
            var brain = NewBrain();
            var empty = new Source(new Vector3(3f, 0f, 0f), water, full: false);
            var far = new Source(new Vector3(SeekRadius + 2f, 0f, 0f), water);
            var other = new Source(new Vector3(-3f, 0f, 0f), wood);

            Run(brain, PlaceAtOrigin, Vector3.zero, new IAbsorbable[] { empty, far, other }, 3f);

            Assert.That(brain.Activity, Is.EqualTo(CompanionActivity.Following));
            Assert.That(empty.Absorbed + far.Absorbed + other.Absorbed, Is.Zero);
            Assert.That(brain.IsFull, Is.False);
        }

        [Test]
        public void ASourceEmptiedOnTheWay_IsGivenUp()
        {
            var brain = NewBrain();
            var source = new Source(new Vector3(8f, 0f, 0f), water);
            var sources = new IAbsorbable[] { source };

            Run(brain, Vector3.zero, Vector3.zero, sources, 0.2f);
            Assert.That(brain.Activity, Is.EqualTo(CompanionActivity.Seeking));

            source.CanBeAbsorbed = false;
            Run(brain, Vector3.zero, Vector3.zero, sources, Frame * 2f);
            Assert.That(brain.Activity, Is.EqualTo(CompanionActivity.Following));
            Assert.That(brain.Target, Is.Null);
        }

        [Test]
        public void WhenThePlayerWalksAwayFromTheSource_ItFollowsThemInstead()
        {
            var brain = NewBrain();
            var source = new Source(new Vector3(8f, 0f, 0f), water);
            var sources = new IAbsorbable[] { source };
            var position = Run(brain, Vector3.zero, Vector3.zero, sources, 0.2f);

            Run(brain, position, new Vector3(-15f, 0f, 0f), sources, Frame * 2f);

            Assert.That(brain.Activity, Is.EqualTo(CompanionActivity.Following));
            Assert.That(source.Absorbed, Is.Zero);
        }

        [Test]
        public void Squeezing_EmptiesIt_AndItGoesForSourcesAgain()
        {
            var brain = NewBrain();
            Assert.That(brain.TryEmpty(), Is.False, "An empty companion was squeezed.");

            var first = new Source(new Vector3(0.3f, 0f, 0f), water);
            Run(brain, Vector3.zero, Vector3.zero, new IAbsorbable[] { first }, 1.5f);
            Assert.That(brain.IsFull, Is.True);

            Assert.That(brain.TryEmpty(), Is.True);
            Assert.That(brain.IsFull, Is.False);
            Assert.That(brain.Fullness, Is.Zero);

            var second = new Source(new Vector3(-0.3f, 0f, 0f), water);
            Run(brain, Vector3.zero, Vector3.zero, new IAbsorbable[] { first, second }, 1.5f);
            Assert.That(second.Absorbed, Is.EqualTo(1));
        }
    }
}
