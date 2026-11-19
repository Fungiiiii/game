using System.Collections.Generic;
using Fungiiiii.Capture;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class NetLandingResolverTests
    {
        private const float ImpactRadius = 1.5f;
        private const float ScareRadius = 3f;

        [Test]
        public void Resolve_OnAMiss_ScaresTheMushroomsCloseToTheImpact()
        {
            var resolver = CreateResolver(NetCaptureRates.Default, seed: 1);
            var candidates = new List<NetCandidate>
            {
                Candidate(new Vector3(2f, 0f, 0f)),
                Candidate(new Vector3(0f, 0f, 2.9f)),
                Candidate(new Vector3(4f, 0f, 0f))
            };
            var scared = new List<int>();

            var result = resolver.Resolve(Vector3.zero, candidates, scared);

            Assert.That(result.Outcome, Is.EqualTo(NetLandingOutcome.Miss));
            Assert.That(result.TargetIndex, Is.EqualTo(-1));
            Assert.That(scared, Is.EqualTo(new[] { 0, 1 }));
        }

        [Test]
        public void Resolve_OnAHit_TargetsOnlyTheClosestMushroom_AndScaresNone()
        {
            var resolver = CreateResolver(new NetCaptureRates(1f, 1f, 1f, 0f), seed: 1);
            var candidates = new List<NetCandidate>
            {
                Candidate(new Vector3(1f, 0f, 0f)),
                Candidate(new Vector3(0.2f, 0f, 0f)),
                Candidate(new Vector3(2.5f, 0f, 0f))
            };
            var scared = new List<int>();

            var result = resolver.Resolve(Vector3.zero, candidates, scared);

            Assert.That(result.TargetIndex, Is.EqualTo(1));
            Assert.That(result.Outcome, Is.EqualTo(NetLandingOutcome.CaptureSucceeded));
            Assert.That(scared, Is.Empty);
        }

        [Test]
        public void Resolve_FailsTheAttempt_WhenTheChanceIsZero()
        {
            var resolver = CreateResolver(new NetCaptureRates(0f, 0f, 0f, 0f), seed: 1);
            var candidates = new List<NetCandidate> { Candidate(Vector3.zero) };

            var result = resolver.Resolve(Vector3.zero, candidates, new List<int>());

            Assert.That(result.Outcome, Is.EqualTo(NetLandingOutcome.CaptureFailed));
        }

        [Test]
        public void Resolve_UsesTheProfileAndTheSurpriseBonusOfTheTarget()
        {
            var resolver = CreateResolver(NetCaptureRates.Default, seed: 1);
            var candidates = new List<NetCandidate>
            {
                new NetCandidate(Vector3.zero, NetTargetProfile.Rare, isUnaware: true)
            };

            var result = resolver.Resolve(Vector3.zero, candidates, new List<int>());

            Assert.That(result.SuccessChance, Is.EqualTo(0.3f).Within(0.0001f));
        }

        [Test]
        public void Resolve_GivesTheSameOutcomes_WithTheSameSeed()
        {
            var candidates = new List<NetCandidate> { Candidate(Vector3.zero) };
            var first = CreateResolver(NetCaptureRates.Default, seed: 2024);
            var second = CreateResolver(NetCaptureRates.Default, seed: 2024);

            for (var attempt = 0; attempt < 30; attempt++)
            {
                Assert.That(
                    first.Resolve(Vector3.zero, candidates, new List<int>()).Outcome,
                    Is.EqualTo(second.Resolve(Vector3.zero, candidates, new List<int>()).Outcome));
            }
        }

        [Test]
        public void TryPreview_ReportsTheTargetAndItsChance_WithoutDrawing()
        {
            var resolver = CreateResolver(NetCaptureRates.Default, seed: 1);
            var candidates = new List<NetCandidate>
            {
                new NetCandidate(new Vector3(0.5f, 0f, 0f), NetTargetProfile.CommonWeakened, isUnaware: false)
            };

            Assert.That(resolver.TryPreview(Vector3.zero, candidates, out var target, out var chance), Is.True);
            Assert.That(target, Is.EqualTo(0));
            Assert.That(chance, Is.EqualTo(0.8f).Within(0.0001f));
            Assert.That(resolver.TryPreview(new Vector3(10f, 0f, 0f), candidates, out _, out _), Is.False);
        }

        private static NetLandingResolver CreateResolver(NetCaptureRates rates, int seed)
        {
            return new NetLandingResolver(ImpactRadius, ScareRadius, rates, new System.Random(seed));
        }

        private static NetCandidate Candidate(Vector3 position)
        {
            return new NetCandidate(position, NetTargetProfile.Common, isUnaware: false);
        }
    }
}
