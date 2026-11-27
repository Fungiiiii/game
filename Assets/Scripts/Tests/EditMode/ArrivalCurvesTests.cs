using Fungiiiii.Runtime.World;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class ArrivalCurvesTests
    {
        [Test]
        public void AGrowingThing_StartsWithNoSize_AndEndsAtItsOwn()
        {
            Assert.That(ArrivalCurves.Grow(0f, 1f), Is.Zero);
            Assert.That(ArrivalCurves.Grow(1f, 1f), Is.EqualTo(1f));
            Assert.That(ArrivalCurves.Grow(5f, 1f), Is.EqualTo(1f), "It grows past its size.");
        }

        [Test]
        public void AGrowingThing_GrowsFastFirst_ThenSettles()
        {
            Assert.That(ArrivalCurves.Grow(0.5f, 1f), Is.GreaterThan(0.5f));
            Assert.That(ArrivalCurves.Grow(0.25f, 1f), Is.LessThan(ArrivalCurves.Grow(0.5f, 1f)));
        }

        [Test]
        public void AFall_LastsAsLongAsAFreeFallFromItsHeight()
        {
            // From 4.9 m, a free fall lasts one second.
            Assert.That(ArrivalCurves.FallSeconds(4.905f), Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void AFallingThing_StartsAtItsHeight_AndLandsOnTheGround()
        {
            Assert.That(ArrivalCurves.HeightAbove(0f, 4f), Is.EqualTo(4f));
            Assert.That(ArrivalCurves.HeightAbove(ArrivalCurves.FallSeconds(4f), 4f), Is.Zero);
            Assert.That(ArrivalCurves.HeightAbove(10f, 4f), Is.Zero, "It falls through the ground.");
        }

        [Test]
        public void AFallingThing_SpeedsUp()
        {
            var duration = ArrivalCurves.FallSeconds(4f);
            var firstHalf = 4f - ArrivalCurves.HeightAbove(duration * 0.5f, 4f);
            var secondHalf = ArrivalCurves.HeightAbove(duration * 0.5f, 4f);

            Assert.That(secondHalf, Is.GreaterThan(firstHalf));
        }
    }
}
