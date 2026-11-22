using Fungiiiii.Champimaison;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class PlantingRulesTests
    {
        [Test]
        public void WithoutTheSeed_AHoleSaysItNeedsOne()
        {
            Assert.That(PlantingRules.PromptFor(seedPlanted: false, seedInHand: false, seedInInventory: false), Is.EqualTo(PlantingPrompt.NeedsSeed));
        }

        [Test]
        public void WithTheSeedInTheBackpack_AHoleAsksForItInHand()
        {
            Assert.That(PlantingRules.PromptFor(seedPlanted: false, seedInHand: false, seedInInventory: true), Is.EqualTo(PlantingPrompt.HoldSeed));
        }

        [Test]
        public void WithTheSeedInHand_AHoleShowsTheKeyToPlant()
        {
            Assert.That(PlantingRules.PromptFor(seedPlanted: false, seedInHand: true, seedInInventory: true), Is.EqualTo(PlantingPrompt.Plant));
        }

        [Test]
        public void OnceTheSeedIsPlanted_TheHolesShowNothing()
        {
            Assert.That(PlantingRules.PromptFor(seedPlanted: true, seedInHand: true, seedInInventory: true), Is.EqualTo(PlantingPrompt.None));
            Assert.That(PlantingRules.PromptFor(seedPlanted: true, seedInHand: false, seedInInventory: false), Is.EqualTo(PlantingPrompt.None));
        }
    }
}
