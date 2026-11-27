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
        public IEnumerator LookingAtAChamponge_FromTwelveMetres_ShowsItsName_LookingAway_HidesIt()
        {
            yield return Load();
            var game = Bootstrap;
            var champonge = game.Mushrooms[0];
            StandBefore(champonge.transform.position, 12f);
            yield return Wait(0.6f);
            Assert.That(champonge.gameObject.activeSelf, Is.True);

            // Held still where it stands, so that the view stays on it.
            champonge.transform.position = Ground(champonge.transform.position);
            var look = game.ViewCamera.GetComponent<FirstPersonCamera>();
            LookAt(game, look, champonge.transform.position + Vector3.up * 0.3f);
            yield return null;
            yield return null;

            Assert.That(game.NameTag.IsShown, Is.True, "No name over the Champonge looked at from 12 m.");
            Assert.That(game.NameTag.ShownKey, Is.EqualTo(game.ItemFor(champonge).NameKey));
            yield return Wait(0.3f);
            Assert.That(game.NameTag.Opacity, Is.EqualTo(1f), "The name did not fade in.");

            look.SetView(180f, 0f);
            yield return null;
            yield return null;
            Assert.That(game.NameTag.IsShown, Is.False, "The name stays with the Champonge behind the player.");
            yield return Wait(0.3f);
            Assert.That(game.NameTag.Opacity, Is.Zero, "The name did not fade out.");
        }

        [UnityTest]
        public IEnumerator LookingTwentyDegreesBesideAChamponge_StillShowsItsName_WithoutADropWhileDry()
        {
            // Since the playtest of 23/11: the name had to be aimed at within 6 degrees, and hid.
            yield return Load();
            var game = Bootstrap;
            var champonge = game.Mushrooms[0];
            StandBefore(champonge.transform.position, 8f);
            yield return Wait(0.6f);
            champonge.transform.position = Ground(champonge.transform.position);
            var look = game.ViewCamera.GetComponent<FirstPersonCamera>();
            var toChamponge = champonge.transform.position + Vector3.up * 0.3f - game.ViewCamera.transform.position;
            var yaw = Mathf.Atan2(toChamponge.x, toChamponge.z) * Mathf.Rad2Deg;
            var pitch = -Mathf.Atan2(toChamponge.y, new Vector2(toChamponge.x, toChamponge.z).magnitude) * Mathf.Rad2Deg;
            look.SetView(yaw + 20f, pitch);
            yield return null;
            yield return null;

            Assert.That(game.NameTag.IsShown, Is.True, "No name with the Champonge 20 degrees off the middle of the view.");
            Assert.That(game.NameTag.ShowsWater, Is.False, "A drop shows by a dry Champonge.");
        }

        [UnityTest]
        public IEnumerator AChampongeTooFar_ShowsNoName()
        {
            yield return Load();
            var game = Bootstrap;
            var champonge = game.Mushrooms[0];
            StandBefore(champonge.transform.position, 25f);
            yield return Wait(0.6f);
            var look = game.ViewCamera.GetComponent<FirstPersonCamera>();
            LookAt(game, look, champonge.transform.position + Vector3.up * 0.3f);
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
