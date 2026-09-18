using System.Collections.Generic;
using Fungiiiii.Capture;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class MushroomCaptureStateMachineTests
    {
        private static readonly MushroomCaptureStateMachine.Configuration TestConfiguration =
            new MushroomCaptureStateMachine.Configuration(
                alertDistance: 3f,
                captureDistance: 1f,
                fleeDelaySeconds: 1f,
                calmDistance: 5f);

        [Test]
        public void Idle_EntersAlert_WhenObserverEntersAlertDistance()
        {
            var stateMachine = new MushroomCaptureStateMachine(TestConfiguration);

            Assert.That(stateMachine.Tick(3f, 0f), Is.True);
            Assert.That(stateMachine.CurrentState, Is.EqualTo(MushroomCaptureState.Alert));
        }

        [Test]
        public void Alert_ReturnsToIdle_WhenObserverLeavesBeforeFleeDelay()
        {
            var stateMachine = new MushroomCaptureStateMachine(TestConfiguration);
            stateMachine.Tick(3f, 0f);

            Assert.That(stateMachine.Tick(3.1f, 0.25f), Is.True);
            Assert.That(stateMachine.CurrentState, Is.EqualTo(MushroomCaptureState.Idle));
        }

        [Test]
        public void Alert_TransitionsToFlee_AfterFleeDelay()
        {
            var stateMachine = new MushroomCaptureStateMachine(TestConfiguration);
            stateMachine.Tick(3f, 0f);

            Assert.That(stateMachine.Tick(2f, 1f), Is.True);
            Assert.That(stateMachine.CurrentState, Is.EqualTo(MushroomCaptureState.Flee));
        }

        [Test]
        public void Flee_ReturnsToIdle_WhenObserverIsCalm()
        {
            var stateMachine = new MushroomCaptureStateMachine(TestConfiguration);
            stateMachine.Tick(3f, 0f);
            stateMachine.Tick(2f, 1f);

            Assert.That(stateMachine.Tick(5f, 0f), Is.True);
            Assert.That(stateMachine.CurrentState, Is.EqualTo(MushroomCaptureState.Idle));
        }

        [Test]
        public void Alert_CapturesOnlyWithinCaptureDistance_AndRaisesEvent()
        {
            var stateMachine = new MushroomCaptureStateMachine(TestConfiguration);
            var transitions = new List<MushroomCaptureState>();
            var capturedEventCount = 0;
            stateMachine.StateChanged += (_, nextState) => transitions.Add(nextState);
            stateMachine.Captured += () => capturedEventCount++;

            stateMachine.Tick(3f, 0f);
            Assert.That(stateMachine.TryCapture(1.01f), Is.False);
            Assert.That(stateMachine.TryCapture(1f), Is.True);
            Assert.That(stateMachine.CurrentState, Is.EqualTo(MushroomCaptureState.Captured));
            Assert.That(capturedEventCount, Is.EqualTo(1));
            Assert.That(transitions, Is.EqualTo(new[]
            {
                MushroomCaptureState.Alert,
                MushroomCaptureState.Captured
            }));
        }

        [Test]
        public void Captured_IsTerminalUntilReset()
        {
            var stateMachine = new MushroomCaptureStateMachine(TestConfiguration);
            stateMachine.Tick(3f, 0f);
            stateMachine.TryCapture(1f);

            Assert.That(stateMachine.Tick(0f, 10f), Is.False);
            Assert.That(stateMachine.Reset(), Is.True);
            Assert.That(stateMachine.CurrentState, Is.EqualTo(MushroomCaptureState.Idle));
        }
    }
}
