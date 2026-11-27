using System;
using Fungiiiii.Gathering;
using NUnit.Framework;
using Random = System.Random;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class ChanceRefillTests
    {
        [Test]
        public void ItStartsEmpty_AndNothingCanBeTaken()
        {
            var refill = new ChanceRefill(20f, 0.25f, new Random(1));

            Assert.That(refill.IsFull, Is.False);
            Assert.That(refill.TryTake(), Is.False);
        }

        [Test]
        public void WithACertainChance_ItFillsAtTheFirstDraw_NotBefore()
        {
            var refill = new ChanceRefill(20f, 1f, new Random(1));

            Assert.That(refill.Tick(19.9f), Is.False, "It filled before the first draw.");
            Assert.That(refill.Tick(0.2f), Is.True);
            Assert.That(refill.IsFull, Is.True);
        }

        [Test]
        public void WithNoChance_ItNeverFills()
        {
            var refill = new ChanceRefill(20f, 0f, new Random(1));

            for (var step = 0; step < 1000; step++)
            {
                Assert.That(refill.Tick(1f), Is.False);
            }
        }

        [Test]
        public void OnAverage_ItStaysEmptyForTheIntervalDividedByTheChance()
        {
            // 25 % every 20 s: about 80 s empty on average, the fern's tuning.
            var random = new Random(7);
            const int Trials = 2000;
            var total = 0f;
            for (var trial = 0; trial < Trials; trial++)
            {
                var refill = new ChanceRefill(20f, 0.25f, random);
                var elapsed = 0f;
                while (!refill.Tick(1f))
                {
                    elapsed += 1f;
                }

                total += elapsed + 1f;
            }

            Assert.That(total / Trials, Is.EqualTo(80f).Within(6f));
        }

        [Test]
        public void ALongTick_DrawsOncePerIntervalItCovers()
        {
            // One frame of 100 s covers five draws: with 25 %, filling is likely but not certain.
            var filled = 0;
            for (var seed = 0; seed < 1000; seed++)
            {
                if (new ChanceRefill(20f, 0.25f, new Random(seed)).Tick(100f))
                {
                    filled++;
                }
            }

            // 1 - 0.75^5 = 76 %.
            Assert.That(filled / 1000f, Is.EqualTo(0.763f).Within(0.05f));
        }

        [Test]
        public void TakingIt_EmptiesIt_AndTheTimerStartsOver()
        {
            var refill = new ChanceRefill(20f, 1f, new Random(1));
            refill.Tick(20f);

            Assert.That(refill.TryTake(), Is.True);
            Assert.That(refill.IsFull, Is.False);
            Assert.That(refill.TryTake(), Is.False, "It was taken twice.");
            Assert.That(refill.Tick(19.9f), Is.False, "The timer did not start over.");
            Assert.That(refill.Tick(0.2f), Is.True);
        }

        [Test]
        public void WhileFull_TimeDoesNothing()
        {
            var refill = new ChanceRefill(20f, 1f, new Random(1));
            refill.Tick(20f);

            Assert.That(refill.Tick(100f), Is.False);
            Assert.That(refill.IsFull, Is.True);
        }

        [Test]
        public void Restart_FillsWithTheGivenChance()
        {
            var refill = new ChanceRefill(20f, 0.25f, new Random(3));

            refill.Restart(1f);
            Assert.That(refill.IsFull, Is.True);
            refill.Restart(0f);
            Assert.That(refill.IsFull, Is.False);

            var full = 0;
            for (var trial = 0; trial < 2000; trial++)
            {
                refill.Restart(0.5f);
                full += refill.IsFull ? 1 : 0;
            }

            Assert.That(full / 2000f, Is.EqualTo(0.5f).Within(0.05f));
        }

        [Test]
        public void TheSameSeed_GivesTheSameDraws()
        {
            var first = new ChanceRefill(20f, 0.25f, new Random(42));
            var second = new ChanceRefill(20f, 0.25f, new Random(42));

            for (var step = 0; step < 500; step++)
            {
                Assert.That(first.Tick(1f), Is.EqualTo(second.Tick(1f)));
                if (first.IsFull)
                {
                    first.TryTake();
                    second.TryTake();
                }
            }
        }

        [Test]
        public void InvalidSettings_AreRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChanceRefill(0f, 0.5f, new Random(1)));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChanceRefill(20f, 1.5f, new Random(1)));
            Assert.Throws<ArgumentNullException>(() => new ChanceRefill(20f, 0.5f, null));
        }
    }
}
