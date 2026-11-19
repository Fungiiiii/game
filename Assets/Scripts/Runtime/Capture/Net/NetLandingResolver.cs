using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>A mushroom the net could land on, as seen at landing time.</summary>
    public readonly struct NetCandidate
    {
        public NetCandidate(Vector3 position, NetTargetProfile profile, bool isUnaware)
        {
            Position = position;
            Profile = profile;
            IsUnaware = isUnaware;
        }

        public Vector3 Position { get; }

        public NetTargetProfile Profile { get; }

        /// <summary>True if the mushroom has not noticed the player.</summary>
        public bool IsUnaware { get; }
    }

    /// <summary>What a landed net does.</summary>
    public readonly struct NetLandingResult
    {
        public NetLandingResult(int targetIndex, float successChance, NetLandingOutcome outcome)
        {
            TargetIndex = targetIndex;
            SuccessChance = successChance;
            Outcome = outcome;
        }

        /// <summary>Index of the targeted candidate, or -1 on a miss.</summary>
        public int TargetIndex { get; }

        /// <summary>Chance the attempt had, or 0 on a miss.</summary>
        public float SuccessChance { get; }

        public NetLandingOutcome Outcome { get; }
    }

    /// <summary>
    /// Decides what a landed net does: one capture attempt on the mushroom closest to
    /// the impact, or, on a miss, which nearby mushrooms are scared away.
    ///
    /// Plain C#, with an injected generator, so the host can run it as it is and tests
    /// can fix the draw.
    /// </summary>
    public sealed class NetLandingResolver
    {
        private readonly float impactRadius;
        private readonly float scareRadius;
        private readonly NetCaptureRates rates;
        private readonly System.Random random;
        private readonly List<Vector3> positionBuffer = new List<Vector3>();

        public NetLandingResolver(float impactRadius, float scareRadius, NetCaptureRates rates, System.Random random)
        {
            if (impactRadius < 0f || float.IsNaN(impactRadius) || float.IsInfinity(impactRadius))
            {
                throw new ArgumentOutOfRangeException(nameof(impactRadius));
            }

            if (scareRadius < 0f || float.IsNaN(scareRadius) || float.IsInfinity(scareRadius))
            {
                throw new ArgumentOutOfRangeException(nameof(scareRadius));
            }

            this.impactRadius = impactRadius;
            this.scareRadius = scareRadius;
            this.rates = rates;
            this.random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public float ImpactRadius => impactRadius;

        /// <summary>
        /// Resolves a landing. On a miss, <paramref name="scaredIndices"/> receives every
        /// candidate within the scare radius; otherwise it is left empty.
        /// </summary>
        public NetLandingResult Resolve(
            Vector3 impactPoint,
            IReadOnlyList<NetCandidate> candidates,
            List<int> scaredIndices)
        {
            if (scaredIndices == null)
            {
                throw new ArgumentNullException(nameof(scaredIndices));
            }

            scaredIndices.Clear();
            FillPositions(candidates);

            var target = NetImpact.SelectTarget(impactPoint, impactRadius, positionBuffer);
            if (target < 0)
            {
                NetImpact.FindWithin(impactPoint, scareRadius, positionBuffer, scaredIndices);
                return new NetLandingResult(-1, 0f, NetLandingOutcome.Miss);
            }

            var candidate = candidates[target];
            var chance = rates.SuccessChance(candidate.Profile, candidate.IsUnaware);
            var outcome = NetCaptureRates.Roll(chance, random)
                ? NetLandingOutcome.CaptureSucceeded
                : NetLandingOutcome.CaptureFailed;

            return new NetLandingResult(target, chance, outcome);
        }

        /// <summary>
        /// Reports, without drawing, which candidate a net landing at
        /// <paramref name="impactPoint"/> would target and the chance it would have.
        /// Used by the aiming preview.
        /// </summary>
        /// <returns>False if no candidate is within the impact radius.</returns>
        public bool TryPreview(
            Vector3 impactPoint,
            IReadOnlyList<NetCandidate> candidates,
            out int targetIndex,
            out float successChance)
        {
            FillPositions(candidates);

            targetIndex = NetImpact.SelectTarget(impactPoint, impactRadius, positionBuffer);
            if (targetIndex < 0)
            {
                successChance = 0f;
                return false;
            }

            var candidate = candidates[targetIndex];
            successChance = rates.SuccessChance(candidate.Profile, candidate.IsUnaware);
            return true;
        }

        private void FillPositions(IReadOnlyList<NetCandidate> candidates)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            positionBuffer.Clear();
            for (var index = 0; index < candidates.Count; index++)
            {
                positionBuffer.Add(candidates[index].Position);
            }
        }
    }
}
