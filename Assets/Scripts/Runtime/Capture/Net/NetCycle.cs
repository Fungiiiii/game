using System;

namespace Fungiiiii.Capture
{
    /// <summary>Where the capture net is in its throw cycle.</summary>
    public enum NetPhase
    {
        /// <summary>In the player's hand: the only phase a throw is accepted in.</summary>
        Ready,

        /// <summary>In the air.</summary>
        Flying,

        /// <summary>On a mushroom: net and mushroom are frozen until the outcome is shown.</summary>
        Holding,

        /// <summary>Coming back to the player. Its duration is the recharge.</summary>
        Returning
    }

    /// <summary>What a landed net does next.</summary>
    public enum NetLandingOutcome
    {
        /// <summary>No mushroom in the impact zone.</summary>
        Miss,

        /// <summary>A mushroom was in the zone and the capture roll succeeded.</summary>
        CaptureSucceeded,

        /// <summary>A mushroom was in the zone and the capture roll failed.</summary>
        CaptureFailed
    }

    /// <summary>
    /// Timing of one permanent capture net: thrown, landed, held on a mushroom, then
    /// returned to the player. The return is the recharge: a new throw is only accepted
    /// once the net is back in hand.
    ///
    /// The capture roll is decided when the net lands, and revealed when the hold ends.
    /// </summary>
    public sealed class NetCycle
    {
        private readonly float holdSeconds;
        private readonly float returnSeconds;
        private NetLandingOutcome pendingOutcome;
        private float phaseElapsedSeconds;

        public NetCycle(float holdSeconds, float returnSeconds)
        {
            if (holdSeconds < 0f || float.IsNaN(holdSeconds) || float.IsInfinity(holdSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(holdSeconds));
            }

            if (returnSeconds <= 0f || float.IsNaN(returnSeconds) || float.IsInfinity(returnSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(returnSeconds));
            }

            this.holdSeconds = holdSeconds;
            this.returnSeconds = returnSeconds;
            Phase = NetPhase.Ready;
        }

        /// <summary>Raised when a throw is accepted.</summary>
        public event Action Thrown;

        /// <summary>Raised when the net touches something. The argument is true if it landed on a mushroom.</summary>
        public event Action<bool> Landed;

        /// <summary>Raised when the hold ends on a successful capture.</summary>
        public event Action CaptureSucceeded;

        /// <summary>Raised when the hold ends on a failed capture.</summary>
        public event Action CaptureFailed;

        /// <summary>Raised when the net is back in hand.</summary>
        public event Action Returned;

        public NetPhase Phase { get; private set; }

        /// <summary>True while returning with a captured mushroom inside.</summary>
        public bool IsCarryingCatch { get; private set; }

        /// <summary>0 when the hold starts, 1 when it ends. 0 outside the Holding phase.</summary>
        public float HoldProgress =>
            Phase == NetPhase.Holding && holdSeconds > 0f ? Math.Min(1f, phaseElapsedSeconds / holdSeconds) : 0f;

        /// <summary>0 when the return starts, 1 when the net is back. 0 outside the Returning phase.</summary>
        public float ReturnProgress =>
            Phase == NetPhase.Returning ? Math.Min(1f, phaseElapsedSeconds / returnSeconds) : 0f;

        /// <summary>Starts a throw. Refused unless the net is in hand.</summary>
        public bool TryThrow()
        {
            if (Phase != NetPhase.Ready)
            {
                return false;
            }

            EnterPhase(NetPhase.Flying);
            IsCarryingCatch = false;
            Thrown?.Invoke();
            return true;
        }

        /// <summary>
        /// Ends the flight. A miss starts the return at once; a capture attempt holds the
        /// net first, and the outcome is revealed when the hold ends.
        /// </summary>
        /// <returns>False if the net was not flying.</returns>
        public bool Land(NetLandingOutcome outcome)
        {
            if (Phase != NetPhase.Flying)
            {
                return false;
            }

            pendingOutcome = outcome;
            var landedOnMushroom = outcome != NetLandingOutcome.Miss;

            EnterPhase(landedOnMushroom ? NetPhase.Holding : NetPhase.Returning);
            Landed?.Invoke(landedOnMushroom);

            if (landedOnMushroom && holdSeconds <= 0f)
            {
                EndHold();
            }

            return true;
        }

        /// <summary>
        /// Advances the hold and the return. Time left over when a phase ends carries
        /// into the next one, so a long frame does not stretch the cycle.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            phaseElapsedSeconds += deltaTime;

            if (Phase == NetPhase.Holding && phaseElapsedSeconds >= holdSeconds)
            {
                var leftover = phaseElapsedSeconds - holdSeconds;
                EndHold();
                phaseElapsedSeconds = leftover;
            }

            if (Phase == NetPhase.Returning && phaseElapsedSeconds >= returnSeconds)
            {
                EnterPhase(NetPhase.Ready);
                IsCarryingCatch = false;
                Returned?.Invoke();
            }
        }

        /// <summary>Puts the net back in hand at once, for a reset.</summary>
        public void Reset()
        {
            EnterPhase(NetPhase.Ready);
            IsCarryingCatch = false;
        }

        private void EndHold()
        {
            var succeeded = pendingOutcome == NetLandingOutcome.CaptureSucceeded;

            EnterPhase(NetPhase.Returning);
            IsCarryingCatch = succeeded;

            if (succeeded)
            {
                CaptureSucceeded?.Invoke();
            }
            else
            {
                CaptureFailed?.Invoke();
            }
        }

        private void EnterPhase(NetPhase phase)
        {
            Phase = phase;
            phaseElapsedSeconds = 0f;
        }
    }
}
