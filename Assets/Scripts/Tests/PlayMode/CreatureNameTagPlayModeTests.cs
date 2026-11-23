using System.Collections;
using Fungiiiii.Runtime.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Fungiiiii.Tests.PlayMode.PlaytestLoopGame;

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>The name of the creature the player looks at, readable from afar.</summary>
    public sealed class CreatureNameTagPlayModeTests
    {
        [UnityTest]
        public IEnumerator LookingAtASpongeus_FromTwelveMetres_ShowsItsName_LookingAway_HidesIt()
        {
            yield return Load();
            var game = Bootstrap;
            var spongeus = game.Mushrooms[0];
            StandBefore(spongeus.transform.position, 12f);
            yield return Wait(0.6f);
            Assert.That(spongeus.gameObject.activeSelf, Is.True);

            // Held still where it stands, so that the view stays on it.
            spongeus.transform.position = Ground(spongeus.transform.position);
            var look = game.ViewCamera.GetComponent<FirstPersonCamera>();
            LookAt(game, look, spongeus.transform.position + Vector3.up * 0.3f);
            yield return null;
            yield return null;

            Assert.That(game.NameTag.IsShown, Is.True, "No name over the Spongeus looked at from 12 m.");
            Assert.That(game.NameTag.ShownKey, Is.EqualTo(game.ItemFor(spongeus).NameKey));
            yield return Wait(0.3f);
            Assert.That(game.NameTag.Opacity, Is.EqualTo(1f), "The name did not fade in.");

            look.SetView(180f, 0f);
            yield return null;
            yield return null;
            Assert.That(game.NameTag.IsShown, Is.False, "The name stays with the Spongeus behind the player.");
            yield return Wait(0.3f);
            Assert.That(game.NameTag.Opacity, Is.Zero, "The name did not fade out.");
        }

        [UnityTest]
        public IEnumerator LookingTwentyDegreesBesideASpongeus_StillShowsItsName_WithoutADropWhileDry()
        {
            // Since the playtest of 23/11: the name had to be aimed at within 6 degrees, and hid.
            yield return Load();
            var game = Bootstrap;
            var spongeus = game.Mushrooms[0];
            StandBefore(spongeus.transform.position, 8f);
            yield return Wait(0.6f);
            spongeus.transform.position = Ground(spongeus.transform.position);
            var look = game.ViewCamera.GetComponent<FirstPersonCamera>();
            var toSpongeus = spongeus.transform.position + Vector3.up * 0.3f - game.ViewCamera.transform.position;
            var yaw = Mathf.Atan2(toSpongeus.x, toSpongeus.z) * Mathf.Rad2Deg;
            var pitch = -Mathf.Atan2(toSpongeus.y, new Vector2(toSpongeus.x, toSpongeus.z).magnitude) * Mathf.Rad2Deg;
            look.SetView(yaw + 20f, pitch);
            yield return null;
            yield return null;

            Assert.That(game.NameTag.IsShown, Is.True, "No name with the Spongeus 20 degrees off the middle of the view.");
            Assert.That(game.NameTag.ShowsWater, Is.False, "A drop shows by a dry Spongeus.");
        }

        [UnityTest]
        public IEnumerator ASpongeusTooFar_ShowsNoName()
        {
            yield return Load();
            var game = Bootstrap;
            var spongeus = game.Mushrooms[0];
            StandBefore(spongeus.transform.position, 25f);
            yield return Wait(0.6f);
            var look = game.ViewCamera.GetComponent<FirstPersonCamera>();
            LookAt(game, look, spongeus.transform.position + Vector3.up * 0.3f);
            yield return null;
            yield return null;

            Assert.That(game.NameTag.IsShown, Is.False, "A name shows from 25 m.");
        }

        private static Vector3 Ground(Vector3 point)
        {
            point.y = Terrain.activeTerrain.SampleHeight(point) + Terrain.activeTerrain.GetPosition().y;
            return point;
        }

        private static void LookAt(Fungiiiii.Game.GameBootstrap game, FirstPersonCamera look, Vector3 point)
        {
            var toPoint = point - game.ViewCamera.transform.position;
            var yaw = Mathf.Atan2(toPoint.x, toPoint.z) * Mathf.Rad2Deg;
            var pitch = -Mathf.Atan2(toPoint.y, new Vector2(toPoint.x, toPoint.z).magnitude) * Mathf.Rad2Deg;
            look.SetView(yaw, pitch);
        }
    }
}
