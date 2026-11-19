using Fungiiiii.Capture;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    /// <summary>
    /// ResolveCaptureAttempt, added for the capture net. The existing state machine tests
    /// are left as they were.
    /// </summary>
    public sealed class MushroomCaptureResolveAttemptTests
    {
        private static readonly MushroomCaptureStateMachine.Configuration TestConfiguration =
            new MushroomCaptureStateMachine.Configuration(
                alertDistance: 3f,
                captureDistance: 1f,
                fleeDelaySeconds: 1f,
                calmDistance: 5f);

        [Test]
        public void Success_CapturesTheMushroom_AndRaisesCapturedOnce()
        {
            var stateMachine = new MushroomCaptureStateMachine(TestConfiguration);
            stateMachine.Tick(3f, 0f);
            var captured = 0;
            stateMachine.Captured += () => captured++;

            Assert.That(stateMachine.ResolveCaptureAttempt(true), Is.True);
            Assert.That(stateMachine.ResolveCaptureAttempt(true), Is.False);

            Assert.That(stateMachine.CurrentState, Is.EqualTo(MushroomCaptureState.Captured));
            Assert.That(captured, Is.EqualTo(1));
        }

        [Test]
        public void Failure_MakesTheMushroomFlee()
        {
            var stateMachine = new MushroomCaptureStateMachine(TestConfiguration);
            stateMachine.Tick(3f, 0f);
            var captured = 0;
            stateMachine.Captured += () => captured++;

            Assert.That(stateMachine.ResolveCaptureAttempt(false), Is.True);

            Assert.That(stateMachine.CurrentState, Is.EqualTo(MushroomCaptureState.Flee));
            Assert.That(captured, Is.EqualTo(0));
        }

        [Test]
        public void IsAccepted_OnAnIdleMushroom()
        {
            var stateMachine = new MushroomCaptureStateMachine(TestConfiguration);

            Assert.That(stateMachine.ResolveCaptureAttempt(true), Is.True);
            Assert.That(stateMachine.CurrentState, Is.EqualTo(MushroomCaptureState.Captured));
        }

        [Test]
        public void IsAccepted_OnAFleeingMushroom()
        {
            var stateMachine = new MushroomCaptureStateMachine(TestConfiguration);
            stateMachine.ResolveCaptureAttempt(false);

            Assert.That(stateMachine.ResolveCaptureAttempt(true), Is.True);
            Assert.That(stateMachine.CurrentState, Is.EqualTo(MushroomCaptureState.Captured));
        }

        [Test]
        public void ACapturedMushroom_NoLongerChanges()
        {
            var stateMachine = new MushroomCaptureStateMachine(TestConfiguration);
            stateMachine.ResolveCaptureAttempt(true);
            var changes = 0;
            stateMachine.StateChanged += (_, _) => changes++;

            Assert.That(stateMachine.ResolveCaptureAttempt(false), Is.False);
            Assert.That(stateMachine.ResolveCaptureAttempt(true), Is.False);

            Assert.That(stateMachine.CurrentState, Is.EqualTo(MushroomCaptureState.Captured));
            Assert.That(changes, Is.EqualTo(0));
        }
    }
}
