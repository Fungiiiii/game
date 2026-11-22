using System.Collections;
using System.Linq;
using Fungiiiii.Capture;
using Fungiiiii.Gathering;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Fungiiiii.Tests.PlayMode.PlaytestLoopGame;

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>The wild Spongeus of the mist, and the slingshot that fells them.</summary>
    public sealed class SlingshotPlayModeTests
    {
        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator TheMushrooms_LiveInTheMist_NotInTheSpawnClearing()
        {
            yield return Load();
            var game = Game;

            Assert.That(game.Mushrooms.Count, Is.GreaterThan(0));
            foreach (var mushroom in game.Mushrooms)
            {
                var position = mushroom.transform.position;
                Assert.That(position.z, Is.InRange(-60f, 120f), $"{mushroom.name} is out of the mist.");
            }
        }

        [UnityTest]
        public IEnumerator AWildSpongeus_DrinksAtAFullFern_AndLooksWet()
        {
            yield return Load();
            var game = Game;
            var (mushroom, fern) = SpongeusBesideAFullFern(game);

            yield return WaitUntil(() => mushroom.IsWet, 10f);

            Assert.That(mushroom.IsWet, Is.True, "The Spongeus did not drink.");
            Assert.That(fern.IsFull, Is.False, "The fern did not dry.");
        }

        [UnityTest]
        public IEnumerator TheSlingshot_ShootsAPebbleFromTheInventory_OnlyInHand()
        {
            yield return Load();
            var game = Game;
            game.Inventory.AddToSlot(0, Pebble(game), 2);
            PutInHand(game, Slingshot(game));
            yield return null;
            var origin = game.ViewCamera.transform.position;

            Assert.That(game.Slingshot.IsEquipped, Is.True);
            Assert.That(game.Slingshot.TryShoot(origin, Vector3.forward), Is.True);
            Assert.That(game.Inventory.Count(Pebble(game).Id), Is.EqualTo(1), "The shot took no pebble.");

            game.Hotbar.Select(1);
            yield return Wait(1f);
            Assert.That(game.Slingshot.TryShoot(origin, Vector3.forward), Is.False, "A slingshot put away shot.");

            game.Hotbar.Select(0);
            game.Inventory.Remove(Pebble(game).Id, 1);
            yield return null;
            Assert.That(game.Slingshot.TryShoot(origin, Vector3.forward), Is.False, "It shot without a pebble.");
        }

        [UnityTest]
        public IEnumerator AFelledWetSpongeus_LetsGoOfWater_ADryOneOfNothing()
        {
            yield return Load();
            var game = Game;
            var (wet, _) = SpongeusBesideAFullFern(game);
            yield return WaitUntil(() => wet.IsWet, 10f);
            Assert.That(wet.IsWet, Is.True);
            var dry = game.Mushrooms.First(mushroom => mushroom != wet && !mushroom.IsWet);
            var water = WaterItem(game);

            yield return Fell(game, dry);
            Assert.That(dry.IsWet, Is.False, "The dry Spongeus drank before it fell: run the test again.");
            Assert.That(dry.IsFelled, Is.True);
            Assert.That(game.Dropped.Items.Any(item => item.Item == water), Is.False, "A dry Spongeus let go of water.");

            yield return Fell(game, wet);
            Assert.That(wet.IsFelled, Is.True);
            var drop = game.Dropped.Items.SingleOrDefault(item => item.Item == water);
            Assert.That(drop, Is.Not.Null, "The wet Spongeus let go of no water.");
            var flat = drop.transform.position - wet.transform.position;
            flat.y = 0f;
            Assert.That(flat.magnitude, Is.LessThan(1f), "The water is not where it fell.");
        }

        /// <summary>Shoots straight down onto it until it falls.</summary>
        private static IEnumerator Fell(Fungiiiii.Game.GameBootstrap game, WildMushroom target)
        {
            game.Inventory.AddToSlot(1, Pebble(game), 10);
            PutInHand(game, Slingshot(game));
            for (var shot = 0; shot < 10 && !target.IsFelled; shot++)
            {
                var origin = target.transform.position + Vector3.up * 2f;
                Assert.That(game.Slingshot.TryShoot(origin, Vector3.down), Is.True);
                yield return Wait(0.8f);
            }

            game.Inventory.TakeFromSlot(Fungiiiii.Inventory.InventoryLayout.Standard.QuickBarFirstSlot, 1, out _);
        }

        /// <summary>A wild Spongeus put next to a fern made full: calm, it goes and drinks.</summary>
        private static (WildMushroom, RefillingPlant) SpongeusBesideAFullFern(Fungiiiii.Game.GameBootstrap game)
        {
            var mushroom = game.Mushrooms.First(each => each.Soak != null);
            var fern = game.RefillingPlants.Plants.OrderBy(plant => Vector3.Distance(plant.Position, mushroom.transform.position)).First();
            fern.Restart(1f);
            mushroom.transform.position = fern.Position + Vector3.right * 2f;
            return (mushroom, fern);
        }

        private static Fungiiiii.Inventory.ItemDefinition WaterItem(Fungiiiii.Game.GameBootstrap game)
        {
            return game.RefillingPlants.Plants[0].Yield;
        }
    }
}
