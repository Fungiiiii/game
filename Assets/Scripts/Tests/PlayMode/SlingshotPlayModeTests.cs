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
            var game = Bootstrap;

            Assert.That(game.Mushrooms.Count, Is.GreaterThan(0));
            foreach (var mushroom in game.Mushrooms)
            {
                var position = mushroom.transform.position;
                Assert.That(position.z, Is.InRange(-60f, 120f), $"{mushroom.name} is out of the mist.");
            }
        }

        [UnityTest]
        public IEnumerator TheFirstFernsMet_AreFull_EachWithASpongeus_AndNoSpongeusLivesAwayFromTheFerns()
        {
            yield return Load();
            var game = Bootstrap;
            var ferns = game.RefillingPlants.Plants;
            var spongeus = WildSpongeus(game);
            Debug.Log($"{ferns.Count} ferns, {spongeus.Count} wild Spongeus.");

            Assert.That(game.RefillingPlants.FirstMet.Count, Is.EqualTo(3));
            foreach (var fern in game.RefillingPlants.FirstMet)
            {
                Assert.That(fern.IsFull, Is.True, $"The fern met first at {fern.Position} is not full.");
                Assert.That(spongeus.Count(each => Flat(each.transform.position, fern.Position) < 5f), Is.EqualTo(1), $"Not one Spongeus beside the fern met first at {fern.Position}.");
            }

            Assert.That(spongeus.Count, Is.InRange(3, ferns.Count / 2), "Not one Spongeus beside some of the ferns only.");
            foreach (var each in spongeus)
            {
                Assert.That(ferns.Min(fern => Flat(each.transform.position, fern.Position)), Is.LessThan(5f), $"{each.name} lives away from the ferns.");
            }
        }

        [UnityTest]
        public IEnumerator AWildSpongeus_DrinksOnlyNearThePlayer_AndLookedAt()
        {
            yield return Load();
            var game = Bootstrap;
            var fern = game.RefillingPlants.FirstMet[0];
            var spongeus = WildSpongeus(game).OrderBy(each => Flat(each.transform.position, fern.Position)).First();

            yield return Wait(6f);
            Assert.That(spongeus.IsWet, Is.False, "The Spongeus drank with the player far away.");
            Assert.That(fern.IsFull, Is.True);

            StandBefore(fern.Position, 8f);
            var look = game.ViewCamera.GetComponent<Fungiiiii.Runtime.Player.FirstPersonCamera>();
            look.SetView(180f, 0f);
            yield return Wait(5f);
            Assert.That(spongeus.IsWet, Is.False, "The Spongeus drank behind the player's back.");
            Assert.That(fern.IsFull, Is.True);

            LookAt(game, look, spongeus.transform.position);
            yield return WaitUntil(() => spongeus.IsWet, 15f);
            Assert.That(spongeus.IsWet, Is.True, "The Spongeus did not drink, looked at from 8 m.");
            Assert.That(fern.IsFull, Is.False, "The fern did not dry.");
        }

        [UnityTest]
        public IEnumerator AWildSpongeus_DrinksAtAFullFern_AndLooksWet()
        {
            yield return Load();
            var game = Bootstrap;
            var (mushroom, fern) = SpongeusBesideAFullFern(game);

            yield return WaitUntil(() => mushroom.IsWet, 10f);

            Assert.That(mushroom.IsWet, Is.True, "The Spongeus did not drink.");
            Assert.That(fern.IsFull, Is.False, "The fern did not dry.");
        }

        [UnityTest]
        public IEnumerator TheSlingshot_ShootsAPebbleFromTheInventory_OnlyInHand()
        {
            yield return Load();
            var game = Bootstrap;
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
            var game = Bootstrap;
            var (wet, _) = SpongeusBesideAFullFern(game);
            yield return WaitUntil(() => wet.IsWet, 10f);
            Assert.That(wet.IsWet, Is.True);
            // Every Spongeus lives beside a fern: the others are emptied so the dry one stays dry.
            foreach (var fern in game.RefillingPlants.Plants)
            {
                fern.Restart(0f);
            }

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

        /// <summary>A wild Spongeus put next to a fern made full, the player 8 m away: calm, it goes and drinks.</summary>
        private static (WildMushroom, RefillingPlant) SpongeusBesideAFullFern(Fungiiiii.Game.GameBootstrap game)
        {
            var mushroom = game.Mushrooms.First(each => each.Soak != null);
            var fern = game.RefillingPlants.Plants.OrderBy(plant => Vector3.Distance(plant.Position, mushroom.transform.position)).First();
            fern.Restart(1f);
            mushroom.transform.position = fern.Position + Vector3.right * 2f;
            // It drinks only under the player's eyes.
            StandBefore(fern.Position, 8f);
            return (mushroom, fern);
        }

        private static System.Collections.Generic.List<WildMushroom> WildSpongeus(Fungiiiii.Game.GameBootstrap game)
        {
            return game.Mushrooms.Where(mushroom => mushroom.Soak != null).ToList();
        }

        /// <summary>Turns the view to the middle of <paramref name="point"/>'s Spongeus: it wanders around its fern.</summary>
        private static void LookAt(Fungiiiii.Game.GameBootstrap game, Fungiiiii.Runtime.Player.FirstPersonCamera look, Vector3 point)
        {
            var toPoint = point + Vector3.up * 0.3f - game.ViewCamera.transform.position;
            var yaw = Mathf.Atan2(toPoint.x, toPoint.z) * Mathf.Rad2Deg;
            var pitch = -Mathf.Atan2(toPoint.y, new Vector2(toPoint.x, toPoint.z).magnitude) * Mathf.Rad2Deg;
            look.SetView(yaw, pitch);
        }

        private static float Flat(Vector3 a, Vector3 b)
        {
            var offset = a - b;
            offset.y = 0f;
            return offset.magnitude;
        }

        private static Fungiiiii.Inventory.ItemDefinition WaterItem(Fungiiiii.Game.GameBootstrap game)
        {
            return game.RefillingPlants.Plants[0].Yield;
        }
    }
}
