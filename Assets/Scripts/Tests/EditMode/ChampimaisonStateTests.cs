using Fungiiiii.Champimaison;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class ChampimaisonStateTests
    {
        [Test]
        public void NewState_StartsWithEmptyPlotAndFictionalResources()
        {
            var state = new ChampimaisonState();

            Assert.That(state.Tier, Is.EqualTo(ChampimaisonTier.Empty));
            Assert.That(state.Spores, Is.EqualTo(3));
            Assert.That(state.Dew, Is.EqualTo(2));
            Assert.That(state.CanPlantSeed, Is.True);
            Assert.That(state.CanUpgradeToTier1, Is.False);
        }

        [Test]
        public void PlantSeed_ConsumesOneSporeAndCreatesTier0()
        {
            var state = new ChampimaisonState();

            bool planted = state.TryPlantSeed();

            Assert.That(planted, Is.True);
            Assert.That(state.Tier, Is.EqualTo(ChampimaisonTier.Tier0));
            Assert.That(state.Spores, Is.EqualTo(2));
            Assert.That(state.Dew, Is.EqualTo(2));
            Assert.That(state.TryPlantSeed(), Is.False);
        }

        [Test]
        public void UpgradeFromTier0_ConsumesTwoSporesAndOneDew()
        {
            var state = new ChampimaisonState();
            state.TryPlantSeed();

            bool upgraded = state.TryUpgradeToTier1();

            Assert.That(upgraded, Is.True);
            Assert.That(state.Tier, Is.EqualTo(ChampimaisonTier.Tier1));
            Assert.That(state.Spores, Is.EqualTo(0));
            Assert.That(state.Dew, Is.EqualTo(1));
            Assert.That(state.TryUpgradeToTier1(), Is.False);
        }

        [Test]
        public void UpgradeBeforePlanting_IsRejectedWithoutChangingResources()
        {
            var state = new ChampimaisonState();

            bool upgraded = state.TryUpgradeToTier1();

            Assert.That(upgraded, Is.False);
            Assert.That(state.Tier, Is.EqualTo(ChampimaisonTier.Empty));
            Assert.That(state.Spores, Is.EqualTo(3));
            Assert.That(state.Dew, Is.EqualTo(2));
        }

        [Test]
        public void Reset_RestoresTheInitialFlow()
        {
            var state = new ChampimaisonState();
            state.TryPlantSeed();
            state.TryUpgradeToTier1();

            state.Reset();

            Assert.That(state.Tier, Is.EqualTo(ChampimaisonTier.Empty));
            Assert.That(state.Spores, Is.EqualTo(3));
            Assert.That(state.Dew, Is.EqualTo(2));
            Assert.That(state.CanPlantSeed, Is.True);
        }
    }
}
