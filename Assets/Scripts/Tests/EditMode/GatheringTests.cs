using System;
using Fungiiiii.Gathering;
using NUnit.Framework;
using UnityEngine;
using Random = System.Random;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class GatheringTests
    {
        private const float MinRespawn = 30f;
        private const float MaxRespawn = 60f;

        [Test]
        public void AResource_StartsAvailable_AndIsGoneOnceGathered()
        {
            var resource = new RespawningResource(MinRespawn, MaxRespawn, new Random(1));

            Assert.That(resource.IsAvailable, Is.True);
            Assert.That(resource.TryGather(), Is.True);
            Assert.That(resource.IsAvailable, Is.False);
        }

        [Test]
        public void AGoneResource_CannotBeGatheredAgain()
        {
            var resource = new RespawningResource(MinRespawn, MaxRespawn, new Random(1));
            resource.TryGather();

            Assert.That(resource.TryGather(), Is.False);
        }

        [Test]
        public void TheRespawnDelay_IsBetweenThirtyAndSixtySeconds()
        {
            var random = new Random(7);
            for (var attempt = 0; attempt < 200; attempt++)
            {
                var resource = new RespawningResource(MinRespawn, MaxRespawn, random);
                resource.TryGather();

                Assert.That(resource.SecondsUntilRespawn, Is.InRange(MinRespawn, MaxRespawn));
            }
        }

        [Test]
        public void AResource_ComesBack_OnceItsDelayHasElapsed()
        {
            var resource = new RespawningResource(MinRespawn, MaxRespawn, new Random(3));
            resource.TryGather();
            var delay = resource.SecondsUntilRespawn;

            Assert.That(resource.Tick(delay - 0.1f), Is.False);
            Assert.That(resource.IsAvailable, Is.False);

            Assert.That(resource.Tick(0.2f), Is.True);
            Assert.That(resource.IsAvailable, Is.True);
            Assert.That(resource.TryGather(), Is.True);
        }

        [Test]
        public void TheSameSeed_GivesTheSameDelays()
        {
            var first = new RespawningResource(MinRespawn, MaxRespawn, new Random(42));
            var second = new RespawningResource(MinRespawn, MaxRespawn, new Random(42));
            first.TryGather();
            second.TryGather();

            Assert.That(first.SecondsUntilRespawn, Is.EqualTo(second.SecondsUntilRespawn));
        }

        [Test]
        public void Reset_BringsAGoneResourceBack()
        {
            var resource = new RespawningResource(MinRespawn, MaxRespawn, new Random(1));
            resource.TryGather();

            resource.Reset();

            Assert.That(resource.IsAvailable, Is.True);
        }

        [Test]
        public void AnInvertedDelayRange_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RespawningResource(60f, 30f, new Random(1)));
        }

        [Test]
        public void Targeting_PicksTheCandidateAimedAt()
        {
            var eye = new Vector3(0f, 1.6f, 0f);
            var candidates = new[]
            {
                new InteractionCandidate(new Vector3(-1f, 0.3f, 1.5f), 2f, true),
                new InteractionCandidate(new Vector3(0f, 0.3f, 1.5f), 2f, true),
            };
            var lookingAtSecond = (candidates[1].Point - eye).normalized;

            Assert.That(InteractionTargeting.Pick(eye, lookingAtSecond, candidates, 25f), Is.EqualTo(1));
        }

        [Test]
        public void Targeting_IgnoresWhatIsOutOfReach()
        {
            var eye = new Vector3(0f, 1.6f, 0f);
            var candidates = new[] { new InteractionCandidate(new Vector3(0f, 0.3f, 2.5f), 2f, true) };
            var forward = (candidates[0].Point - eye).normalized;

            Assert.That(InteractionTargeting.Pick(eye, forward, candidates, 25f), Is.EqualTo(InteractionTargeting.None));
        }

        [Test]
        public void Targeting_MeasuresReachOnTheGround_SoLookingDownAtYourFeetCounts()
        {
            var eye = new Vector3(0f, 1.6f, 0f);
            var candidates = new[] { new InteractionCandidate(new Vector3(0f, 0.3f, 1.9f), 2f, true) };
            var forward = (candidates[0].Point - eye).normalized;

            Assert.That(InteractionTargeting.Pick(eye, forward, candidates, 25f), Is.EqualTo(0));
        }

        [Test]
        public void Targeting_IgnoresWhatIsBehindOrOutsideTheCone()
        {
            var eye = new Vector3(0f, 1.6f, 0f);
            var candidates = new[]
            {
                new InteractionCandidate(new Vector3(0f, 0.3f, -1f), 2f, true),
                new InteractionCandidate(new Vector3(1.5f, 0.3f, 0.5f), 2f, true),
            };

            Assert.That(InteractionTargeting.Pick(eye, Vector3.forward, candidates, 25f), Is.EqualTo(InteractionTargeting.None));
        }

        [Test]
        public void Targeting_IgnoresDisabledCandidates()
        {
            var eye = new Vector3(0f, 1.6f, 0f);
            var candidates = new[] { new InteractionCandidate(new Vector3(0f, 0.3f, 1.5f), 2f, false) };
            var forward = (candidates[0].Point - eye).normalized;

            Assert.That(InteractionTargeting.Pick(eye, forward, candidates, 25f), Is.EqualTo(InteractionTargeting.None));
        }
    }
}
