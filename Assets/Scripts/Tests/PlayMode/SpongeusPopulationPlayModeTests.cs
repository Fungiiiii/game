using System.Collections;
using System.Linq;
using Fungiiiii.Capture;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Fungiiiii.Tests.PlayMode.PlaytestLoopGame;

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>Only the few wild Spongeus nearest the player are out; the far ones go away and come back.</summary>
    public sealed class SpongeusPopulationPlayModeTests
    {
        private const int MaxShown = 6;

        [UnityTest]
        public IEnumerator AtMostSix_AreOut_AllNearThePlayer()
        {
            yield return Load();
            var game = Bootstrap;
            // They live in the mist, far from where the player arrives: the player goes there.
            StandBefore(game.Mushrooms[0].transform.position, 20f);
            yield return Wait(1f);

            var player = game.Interactor.transform.position;
            var shown = game.Mushrooms.Where(mushroom => mushroom.gameObject.activeSelf).ToList();
            Assert.That(game.Mushrooms.Count, Is.GreaterThan(MaxShown), "Too few Spongeus to check the limit.");
            Assert.That(shown.Count, Is.InRange(1, MaxShown), "Not between one and six Spongeus out.");
            foreach (var mushroom in shown)
            {
                Assert.That(Flat(mushroom.transform.position, player), Is.LessThanOrEqualTo(60f), $"{mushroom.name} is out far from the player.");
            }
        }

        [UnityTest]
        public IEnumerator ASpongeus_GoesAway_WhenThePlayerIsFar_AndComesBack()
        {
            yield return Load();
            var game = Bootstrap;
            var spongeus = game.Mushrooms[0];
            StandBefore(spongeus.transform.position, 20f);
            yield return Wait(1f);
            Assert.That(spongeus.gameObject.activeSelf, Is.True, "A Spongeus 20 m away is not out.");
            var home = spongeus.transform.position;

            StandBefore(home, 90f);
            yield return Wait(1.2f);
            Assert.That(spongeus.gameObject.activeSelf, Is.False, "A Spongeus 90 m away is still out.");
            Assert.That(spongeus.IsAvailable, Is.False, "The net could still aim at a Spongeus gone away.");
            var hiddenAt = spongeus.transform.position;
            yield return Wait(1f);
            Assert.That(Flat(spongeus.transform.position, hiddenAt), Is.LessThan(0.01f), "Gone away, it still wanders.");

            StandBefore(home, 20f);
            yield return Wait(1.2f);
            Assert.That(spongeus.gameObject.activeSelf, Is.True, "The Spongeus did not come back with the player.");
        }

        private static float Flat(Vector3 a, Vector3 b)
        {
            var offset = a - b;
            offset.y = 0f;
            return offset.magnitude;
        }
    }
}
