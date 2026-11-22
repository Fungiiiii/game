using System.Collections;
using System.Linq;
using Fungiiiii.Runtime.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Fungiiiii.Tests.PlayMode.PlaytestLoopGame;

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>The compass top centre, and its yellow mark on the objective.</summary>
    public sealed class CompassPlayModeTests
    {
        [UnityTest]
        public IEnumerator TheMark_IsInTheMiddle_FacingTheSeed_AndAtTheEdge_WithTheSeedBehind()
        {
            yield return Load();
            var game = Bootstrap;
            var seed = game.Dropped.Items.Single(item => item.Item == Seed(game));
            var look = game.ViewCamera.GetComponent<FirstPersonCamera>();

            Assert.That(game.ObjectiveTargetPoint, Is.Not.Null, "Nothing to point at to find the seed.");
            Assert.That(Flat(game.ObjectiveTargetPoint.Value, seed.transform.position), Is.LessThan(0.5f), "The mark does not point at the seed.");

            look.LookAt(seed.transform.position);
            yield return null;
            yield return null;
            Assert.That(game.Compass.IsShown, Is.True, "No compass during play.");
            Assert.That(game.Compass.IsObjectiveShown, Is.True);
            Assert.That(Mathf.Abs(game.Compass.ObjectivePosition), Is.LessThan(0.1f), "Facing the seed, the mark is not in the middle.");

            var toSeed = seed.transform.position - game.ViewCamera.transform.position;
            look.SetView(Mathf.Atan2(toSeed.x, toSeed.z) * Mathf.Rad2Deg + 180f, 0f);
            yield return null;
            yield return null;
            Assert.That(Mathf.Abs(game.Compass.ObjectivePosition), Is.EqualTo(1f).Within(0.01f), "With the seed behind, the mark does not stick to an edge.");

            game.SetInventoryOpen(true);
            yield return null;
            yield return null;
            Assert.That(game.Compass.IsShown, Is.False, "The compass shows over the inventory.");
        }

        [UnityTest]
        public IEnumerator Planted_TheMark_GoesToAFullFern_WhileWaterIsMissing_ThenToTheChampimaison()
        {
            yield return Load();
            var game = Bootstrap;
            var champimaison = Plant(game);
            yield return null;

            var target = game.ObjectiveTargetPoint;
            Assert.That(target, Is.Not.Null);
            var fern = game.RefillingPlants.Plants.FirstOrDefault(plant => Flat(plant.Position, target.Value) < 0.1f);
            Assert.That(fern, Is.Not.Null, "Without water, the mark does not point at a fern.");
            Assert.That(fern.IsFull, Is.True, "The mark points at an empty fern.");

            var progress = champimaison.Network.Progress;
            for (var line = 0; line < progress.LineCount; line++)
            {
                game.Inventory.Add(progress.Required(line).Item, progress.Required(line).Amount);
            }

            yield return null;
            Assert.That(Flat(game.ObjectiveTargetPoint.Value, champimaison.transform.position), Is.LessThan(0.1f), "With the water, the mark does not go back to the Champimaison.");
        }

        private static float Flat(Vector3 a, Vector3 b)
        {
            var offset = a - b;
            offset.y = 0f;
            return offset.magnitude;
        }
    }
}
