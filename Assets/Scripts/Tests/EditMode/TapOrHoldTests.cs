using System;
using Fungiiiii.Controls;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class TapOrHoldTests
    {
        private const float TapMax = 0.3f;
        private const float Hold = 0.8f;
        private const float Frame = 1f / 60f;

        private static PressOutcome HoldFor(TapOrHold press, float seconds)
        {
            var outcome = PressOutcome.None;
            for (var elapsed = 0f; elapsed < seconds - 0.0001f; elapsed += Frame)
            {
                var tick = press.Tick(Frame);
                if (tick != PressOutcome.None)
                {
                    outcome = tick;
                }
            }

            return outcome;
        }

        [Test]
        public void AShortPress_IsATap_OnTheRelease()
        {
            var press = new TapOrHold(TapMax, Hold);
            press.Press();

            Assert.That(HoldFor(press, 0.2f), Is.EqualTo(PressOutcome.None), "A tap is decided on the release, not before.");
            Assert.That(press.Release(), Is.EqualTo(PressOutcome.Tap));
        }

        [Test]
        public void APressAndReleaseInTheSameFrame_IsATap()
        {
            var press = new TapOrHold(TapMax, Hold);
            press.Press();

            Assert.That(press.Release(), Is.EqualTo(PressOutcome.Tap));
        }

        [Test]
        public void AHeldPress_IsAHold_TheMomentTheTimeIsReached_Once()
        {
            var press = new TapOrHold(TapMax, Hold);
            press.Press();

            Assert.That(HoldFor(press, 0.75f), Is.EqualTo(PressOutcome.None));
            Assert.That(HoldFor(press, 0.1f), Is.EqualTo(PressOutcome.Hold));
            Assert.That(HoldFor(press, 1f), Is.EqualTo(PressOutcome.None), "A hold was given twice.");
            Assert.That(press.Release(), Is.EqualTo(PressOutcome.None), "The release after a hold is also a tap.");
        }

        [Test]
        public void APressReleasedBetweenTapAndHold_DoesNothing()
        {
            var press = new TapOrHold(TapMax, Hold);
            press.Press();
            HoldFor(press, 0.5f);

            Assert.That(press.Release(), Is.EqualTo(PressOutcome.None));
        }

        [Test]
        public void TheProgress_StartsOnlyPastAShortPress_FillsTowardsTheHold_AndEmptiesOnceDecidedOrReleased()
        {
            var press = new TapOrHold(TapMax, Hold);
            Assert.That(press.HoldProgress, Is.Zero);

            press.Press();
            press.Tick(0.2f);
            Assert.That(press.HoldProgress, Is.Zero, "The ring fills during what may still be a short press.");

            // 0.55 s held: 0.25 s past the short press, half of the 0.5 s left to the hold.
            press.Tick(0.35f);
            Assert.That(press.HoldProgress, Is.EqualTo(0.5f).Within(0.001f));

            press.Tick(0.3f);
            Assert.That(press.HoldProgress, Is.Zero, "The ring stays full once the hold is done.");

            press.Press();
            press.Tick(0.2f);
            press.Release();
            Assert.That(press.HoldProgress, Is.Zero);
        }

        [Test]
        public void Cancel_ForgetsThePress()
        {
            var press = new TapOrHold(TapMax, Hold);
            press.Press();
            press.Cancel();

            Assert.That(press.IsPressed, Is.False);
            Assert.That(press.Release(), Is.EqualTo(PressOutcome.None));
            Assert.That(press.Tick(1f), Is.EqualTo(PressOutcome.None));
        }

        [Test]
        public void AHoldShorterThanATap_IsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TapOrHold(0.5f, 0.4f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TapOrHold(0f, 0.8f));
        }
    }
}
