using System.Collections.Generic;
using Fungiiiii.Capture;
using Fungiiiii.Companions;
using Fungiiiii.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class WildSoakTests
    {
        private static readonly ItemDefinition Water = ItemDefinition.Create("water", Color.blue, 16);

        private sealed class Fern : IAbsorbable
        {
            public Fern(Vector3 position, ItemDefinition yield, bool full = true)
            {
                Position = position;
                Yield = yield;
                CanBeAbsorbed = full;
            }

            public Vector3 Position { get; }

            public ItemDefinition Yield { get; }

            public bool CanBeAbsorbed { get; set; }

            public bool Absorb()
            {
                if (!CanBeAbsorbed)
                {
                    return false;
                }

                CanBeAbsorbed = false;
                return true;
            }
        }

        private static WildSoak NewSoak() => new WildSoak(seekRadius: 10f, soakSeconds: 1f, arrivalDistance: 0.5f, yield: Water);

        [Test]
        public void WithNothingInReach_ItWanders()
        {
            var soak = NewSoak();
            var far = new Fern(new Vector3(50f, 0f, 0f), Water);

            Assert.That(soak.Step(Vector3.zero, new List<IAbsorbable> { far }, 1f, 0.1f), Is.Null);
            Assert.That(soak.IsBusy, Is.False);
        }

        [Test]
        public void ItWalksToTheNearestFullSource()
        {
            var soak = NewSoak();
            var near = new Fern(new Vector3(4f, 0f, 0f), Water);
            var further = new Fern(new Vector3(-8f, 0f, 0f), Water);

            var next = soak.Step(Vector3.zero, new List<IAbsorbable> { further, near }, 2f, 0.5f);

            Assert.That(next.HasValue, Is.True);
            Assert.That(next.Value.x, Is.EqualTo(1f).Within(0.001f));
            Assert.That(soak.Target, Is.SameAs(near));
        }

        [Test]
        public void ADrySourceOrAnotherYield_IsIgnored()
        {
            var soak = NewSoak();
            var dry = new Fern(new Vector3(2f, 0f, 0f), Water, full: false);
            var other = new Fern(new Vector3(3f, 0f, 0f), ItemDefinition.Create("spore", Color.magenta, 16));

            Assert.That(soak.Step(Vector3.zero, new List<IAbsorbable> { dry, other }, 1f, 0.1f), Is.Null);
        }

        [Test]
        public void StandingOnTheSource_ItSoaksItUp_AndIsWet()
        {
            var soak = NewSoak();
            var fern = new Fern(new Vector3(0.2f, 0f, 0f), Water);
            var sources = new List<IAbsorbable> { fern };

            soak.Step(Vector3.zero, sources, 1f, 0.6f);
            Assert.That(soak.IsSoaking, Is.True);
            Assert.That(soak.Wetness, Is.EqualTo(0.6f).Within(0.001f));

            soak.Step(Vector3.zero, sources, 1f, 0.6f);

            Assert.That(soak.IsWet, Is.True);
            Assert.That(soak.Wetness, Is.EqualTo(1f));
            Assert.That(fern.CanBeAbsorbed, Is.False, "The fern did not dry.");
            Assert.That(soak.Step(Vector3.zero, sources, 1f, 0.1f), Is.Null, "A wet one goes on soaking.");
        }

        [Test]
        public void ASourceThatDriesOnTheWay_IsLetGo()
        {
            var soak = NewSoak();
            var fern = new Fern(new Vector3(5f, 0f, 0f), Water);
            var sources = new List<IAbsorbable> { fern };
            soak.Step(Vector3.zero, sources, 1f, 0.1f);

            fern.CanBeAbsorbed = false;

            Assert.That(soak.Step(new Vector3(0.1f, 0f, 0f), sources, 1f, 0.1f), Is.Null);
            Assert.That(soak.IsBusy, Is.False);
        }

        [Test]
        public void Interrupt_ForgetsTheSource_AndWhatWasSoaked()
        {
            var soak = NewSoak();
            var fern = new Fern(new Vector3(0.2f, 0f, 0f), Water);
            soak.Step(Vector3.zero, new List<IAbsorbable> { fern }, 1f, 0.6f);

            soak.Interrupt();

            Assert.That(soak.IsBusy, Is.False);
            Assert.That(soak.Wetness, Is.Zero);
            Assert.That(fern.CanBeAbsorbed, Is.True);
        }
    }
}
