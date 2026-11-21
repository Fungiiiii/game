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
        private const float Speed = 7f;
        private const float CatchUp = 25f;
        private const float SeekRadius = 10f;
        private const float SoakSeconds = 1f;
        private const float Arrival = 0.6f;

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
            return new CompanionBrain(new CompanionTuning(FollowDistance, Speed, CatchUp, SeekRadius, SoakSeconds, Arrival), water);
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

        [Test]
        public void ItWalksToThePlayer_AndStopsAtItsFollowDistance()
        {
            var brain = NewBrain();
            var end = Run(brain, new Vector3(12f, 0f, 0f), Vector3.zero, new IAbsorbable[0], 3f);

            Assert.That(Flat(end, Vector3.zero), Is.EqualTo(FollowDistance).Within(0.01f));
            Assert.That(brain.Activity, Is.EqualTo(CompanionActivity.Following));
        }

        [Test]
        public void CloseToThePlayer_ItStaysPut()
        {
            var brain = NewBrain();
            var start = new Vector3(1f, 0f, 1f);
            var step = brain.Step(start, Vector3.zero, Vector3.forward, new IAbsorbable[0], Frame);

            Assert.That(step.Position, Is.EqualTo(start));
            Assert.That(step.Moved, Is.False);
        }

        [Test]
        public void EachStep_MovesAtMostItsSpeed_AndKeepsItsHeight()
        {
            var brain = NewBrain();
            var position = new Vector3(20f, 3f, 0f);
            var step = brain.Step(position, Vector3.zero, Vector3.forward, new IAbsorbable[0], Frame);

            Assert.That(Flat(step.Position, position), Is.LessThanOrEqualTo(Speed * Frame + 0.0001f));
            Assert.That(step.Position.y, Is.EqualTo(3f));
        }

        [Test]
        public void TooFarFromThePlayer_ItComesBackBehindThem_AtOnce()
        {
            var brain = NewBrain();
            var step = brain.Step(new Vector3(40f, 1f, 0f), Vector3.zero, Vector3.forward, new IAbsorbable[0], Frame);

            Assert.That(step.CaughtUp, Is.True);
            Assert.That(step.Position.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(step.Position.z, Is.EqualTo(-FollowDistance).Within(0.001f), "It does not come back behind the player.");
        }

        [Test]
        public void Empty_ItGoesToTheNearestFullSourceCloseToThePlayer_SoaksIt_AndFollowsAgainFull()
        {
            var brain = NewBrain();
            var near = new Source(new Vector3(5f, 0f, 0f), water);
            var further = new Source(new Vector3(-8f, 0f, 0f), water);
            var sources = new IAbsorbable[] { further, near };

            var end = Run(brain, new Vector3(2f, 0f, 0f), Vector3.zero, sources, 3f);

            Assert.That(near.Absorbed, Is.EqualTo(1), "It did not soak up the nearest source.");
            Assert.That(further.Absorbed, Is.Zero, "Full, it went for another source.");
            Assert.That(brain.IsFull, Is.True);
            Assert.That(brain.Fullness, Is.EqualTo(1f));
            Assert.That(brain.Activity, Is.EqualTo(CompanionActivity.Following));
            Assert.That(Flat(end, Vector3.zero), Is.LessThanOrEqualTo(FollowDistance + 0.01f), "Full, it did not come back to the player.");
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

            Run(brain, Vector3.zero, Vector3.zero, new IAbsorbable[] { empty, far, other }, 3f);

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
