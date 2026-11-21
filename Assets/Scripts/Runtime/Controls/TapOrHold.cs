using System;

namespace Fungiiiii.Controls
{
    /// <summary>What a press of a tap-or-hold control came to.</summary>
    public enum PressOutcome
    {
        None,

        /// <summary>Released quickly enough: decided on the release.</summary>
        Tap,

        /// <summary>Held long enough: decided the moment the hold time is reached, without waiting for the release.</summary>
        Hold,
    }

    /// <summary>
    /// Tells a short press from a long one on the same control: E pressed briefly squeezes
    /// the Spongeus, E held takes it back. A tap is decided on the release, under
    /// <c>tapMaxSeconds</c>; a hold the moment it lasts <c>holdSeconds</c>. A press released
    /// between the two does nothing.
    /// </summary>
    public sealed class TapOrHold
    {
        private readonly float tapMaxSeconds;
        private readonly float holdSeconds;
        private float held;
        private bool decided;

        public TapOrHold(float tapMaxSeconds, float holdSeconds)
        {
            if (float.IsNaN(tapMaxSeconds) || tapMaxSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(tapMaxSeconds));
            }

            if (float.IsNaN(holdSeconds) || holdSeconds <= tapMaxSeconds)
            {
                throw new ArgumentOutOfRangeException(nameof(holdSeconds), "A hold must last longer than a tap.");
            }

            this.tapMaxSeconds = tapMaxSeconds;
            this.holdSeconds = holdSeconds;
        }

        public bool IsPressed { get; private set; }

        /// <summary>
        /// How far the press is towards a hold, from 0 to 1, counted only once it can no
        /// longer be a short press, as in other games: a quick press never shows a ring.
        /// 0 when not pressed, within the short press, or once decided.
        /// </summary>
        public float HoldProgress => IsPressed && !decided && held > tapMaxSeconds
            ? Math.Min(1f, (held - tapMaxSeconds) / (holdSeconds - tapMaxSeconds))
            : 0f;

        public void Press()
        {
            IsPressed = true;
            decided = false;
            held = 0f;
        }

        /// <summary>Counts the press. Returns <see cref="PressOutcome.Hold"/> once, on the tick the hold time is reached.</summary>
        public PressOutcome Tick(float deltaTime)
        {
            if (!IsPressed || decided || deltaTime <= 0f)
            {
                return PressOutcome.None;
            }

            held += deltaTime;
            if (held < holdSeconds)
            {
                return PressOutcome.None;
            }

            decided = true;
            return PressOutcome.Hold;
        }

        /// <summary>Ends the press. Returns <see cref="PressOutcome.Tap"/> when it was short enough and not already a hold.</summary>
        public PressOutcome Release()
        {
            if (!IsPressed)
            {
                return PressOutcome.None;
            }

            var outcome = !decided && held <= tapMaxSeconds ? PressOutcome.Tap : PressOutcome.None;
            Cancel();
            return outcome;
        }

        /// <summary>Forgets the press without any outcome: the target changed, a menu opened.</summary>
        public void Cancel()
        {
            IsPressed = false;
            decided = false;
            held = 0f;
        }
    }
}
