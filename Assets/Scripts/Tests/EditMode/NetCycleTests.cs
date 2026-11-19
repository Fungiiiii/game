using Fungiiiii.Capture;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class NetCycleTests
    {
        private const float HoldSeconds = 0.5f;
        private const float ReturnSeconds = 1f;

        [Test]
        public void TryThrow_IsAccepted_WhenTheNetIsInHand()
        {
            var cycle = new NetCycle(HoldSeconds, ReturnSeconds);
            var thrown = 0;
            cycle.Thrown += () => thrown++;

            Assert.That(cycle.TryThrow(), Is.True);
            Assert.That(cycle.Phase, Is.EqualTo(NetPhase.Flying));
            Assert.That(thrown, Is.EqualTo(1));
        }

        [Test]
        public void TryThrow_IsRefusedDuringTheRecharge_ThenAcceptedOnceTheNetIsBack()
        {
            var cycle = new NetCycle(HoldSeconds, ReturnSeconds);
            cycle.TryThrow();

            Assert.That(cycle.TryThrow(), Is.False, "Refused in flight.");

            cycle.Land(NetLandingOutcome.Miss);
            cycle.Tick(ReturnSeconds * 0.9f);

            Assert.That(cycle.Phase, Is.EqualTo(NetPhase.Returning));
            Assert.That(cycle.TryThrow(), Is.False, "Refused while the net is coming back.");

            cycle.Tick(ReturnSeconds * 0.2f);

            Assert.That(cycle.Phase, Is.EqualTo(NetPhase.Ready));
            Assert.That(cycle.TryThrow(), Is.True);
        }

        [Test]
        public void TryThrow_IsRefusedWhileTheNetHoldsAMushroom()
        {
            var cycle = new NetCycle(HoldSeconds, ReturnSeconds);
            cycle.TryThrow();
            cycle.Land(NetLandingOutcome.CaptureSucceeded);

            Assert.That(cycle.Phase, Is.EqualTo(NetPhase.Holding));
            Assert.That(cycle.TryThrow(), Is.False);
        }

        [Test]
        public void Land_OnAMiss_StartsTheReturnAtOnce()
        {
            var cycle = new NetCycle(HoldSeconds, ReturnSeconds);
            bool? landedOnMushroom = null;
            cycle.Landed += onMushroom => landedOnMushroom = onMushroom;
            cycle.TryThrow();

            cycle.Land(NetLandingOutcome.Miss);

            Assert.That(cycle.Phase, Is.EqualTo(NetPhase.Returning));
            Assert.That(landedOnMushroom, Is.False);
            Assert.That(cycle.IsCarryingCatch, Is.False);
        }

        [Test]
        public void Land_IsRefused_WhenTheNetIsNotFlying()
        {
            var cycle = new NetCycle(HoldSeconds, ReturnSeconds);

            Assert.That(cycle.Land(NetLandingOutcome.Miss), Is.False);
            Assert.That(cycle.Phase, Is.EqualTo(NetPhase.Ready));
        }

        [Test]
        public void Hold_RevealsASuccessOnlyWhenItEnds_ThenReturnsWithTheCatch()
        {
            var cycle = new NetCycle(HoldSeconds, ReturnSeconds);
            var succeeded = 0;
            var failed = 0;
            cycle.CaptureSucceeded += () => succeeded++;
            cycle.CaptureFailed += () => failed++;
            cycle.TryThrow();
            cycle.Land(NetLandingOutcome.CaptureSucceeded);

            cycle.Tick(HoldSeconds * 0.5f);
            Assert.That(succeeded, Is.EqualTo(0), "Revealed before the freeze ended.");

            cycle.Tick(HoldSeconds * 0.6f);
            cycle.Tick(0.01f);

            Assert.That(succeeded, Is.EqualTo(1));
            Assert.That(failed, Is.EqualTo(0));
            Assert.That(cycle.Phase, Is.EqualTo(NetPhase.Returning));
            Assert.That(cycle.IsCarryingCatch, Is.True);
        }

        [Test]
        public void Hold_RevealsAFailure_ThenReturnsEmpty()
        {
            var cycle = new NetCycle(HoldSeconds, ReturnSeconds);
            var failed = 0;
            cycle.CaptureFailed += () => failed++;
            cycle.TryThrow();
            cycle.Land(NetLandingOutcome.CaptureFailed);

            cycle.Tick(HoldSeconds);

            Assert.That(failed, Is.EqualTo(1));
            Assert.That(cycle.Phase, Is.EqualTo(NetPhase.Returning));
            Assert.That(cycle.IsCarryingCatch, Is.False);
        }

        [Test]
        public void Return_RaisesReturnedOnce_AndEmptiesTheNet()
        {
            var cycle = new NetCycle(HoldSeconds, ReturnSeconds);
            var returned = 0;
            cycle.Returned += () => returned++;
            cycle.TryThrow();
            cycle.Land(NetLandingOutcome.CaptureSucceeded);

            cycle.Tick(HoldSeconds + ReturnSeconds + 0.1f);
            cycle.Tick(1f);

            Assert.That(returned, Is.EqualTo(1));
            Assert.That(cycle.Phase, Is.EqualTo(NetPhase.Ready));
            Assert.That(cycle.IsCarryingCatch, Is.False);
        }

        [Test]
        public void Reset_PutsTheNetBackInHand()
        {
            var cycle = new NetCycle(HoldSeconds, ReturnSeconds);
            cycle.TryThrow();

            cycle.Reset();

            Assert.That(cycle.Phase, Is.EqualTo(NetPhase.Ready));
            Assert.That(cycle.TryThrow(), Is.True);
        }
    }
}
