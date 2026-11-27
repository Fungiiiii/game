using System.Collections;
using System.Linq;
using Fungiiiii.Game;
using Fungiiiii.Runtime.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using static Fungiiiii.Tests.PlayMode.PlaytestLoopGame;

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>The opening shot: the seed falling from the giant tree, then the view back to the player.</summary>
    public sealed class SeedFallIntroPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator TheGame_OpensOnTheSeedFallingFromTheGiantTree_ThenGivesTheViewBack()
        {
            InputSystem.AddDevice<Keyboard>();
            yield return Load(skipIntro: false);
            var game = Bootstrap;
            var view = game.ViewCamera;
            var look = view.GetComponent<FirstPersonCamera>();
            var seed = SeedOnTheGround(game);
            var player = game.Interactor.transform;
            var baseFieldOfView = view.fieldOfView;

            Assert.That(game.Intro.IsPlaying, Is.True, "No opening shot.");
            Assert.That(game.Controls.enabled, Is.False, "The player has the controls during the shot.");
            Assert.That(game.ObjectiveHud.IsVisible, Is.False, "The interface shows during the shot.");
            yield return null;
            Assert.That(Flat(view.transform.position, seed.transform.position), Is.LessThan(10f), "The camera is not before the seed.");
            Assert.That(view.transform.forward.y, Is.GreaterThan(0.5f), "The camera does not look up at the leaves.");

            yield return Wait(1.2f);
            var above = seed.transform.position.y - Ground(seed.transform.position);
            Assert.That(above, Is.GreaterThan(0.5f), "The seed is not falling.");
            var toSeed = seed.transform.position - view.transform.position;
            Assert.That(Vector3.Angle(view.transform.forward, toSeed), Is.LessThan(5f), "The camera does not follow the falling seed.");
            Assert.That(view.fieldOfView, Is.LessThan(baseFieldOfView), "The view does not narrow on the seed.");

            yield return WaitUntil(() => !game.Intro.IsPlaying, 8f);
            Assert.That(game.Intro.IsPlaying, Is.False, "The shot did not end.");
            yield return null;
            Assert.That(seed.transform.position.y - Ground(seed.transform.position), Is.LessThan(0.1f), "The seed did not land.");
            Assert.That(view.fieldOfView, Is.EqualTo(baseFieldOfView).Within(0.01f));
            Assert.That(look.enabled, Is.True);
            Assert.That(game.Controls.enabled, Is.True, "The player did not get the controls back.");
            Assert.That(game.ObjectiveHud.IsVisible, Is.True, "The interface did not come back.");
            Assert.That(Vector3.Distance(view.transform.position, player.position), Is.LessThan(2f), "The view is not back with the player.");
            var toSeedAtEnd = seed.transform.position - view.transform.position;
            Assert.That(Vector3.Angle(view.transform.forward, toSeedAtEnd), Is.LessThan(10f), "The seed is not right before the player when the shot ends.");
        }

        [UnityTest]
        public IEnumerator TheShot_DrawsBackFromTheSeed_ToThePlayer_BeforeGivingTheControls()
        {
            InputSystem.AddDevice<Keyboard>();
            yield return Load(skipIntro: false);
            var game = Bootstrap;
            var view = game.ViewCamera;
            var player = game.Interactor.transform;
            var seed = SeedOnTheGround(game);
            yield return WaitUntil(() => seed.transform.position.y - Ground(seed.transform.position) < 0.05f, 8f);

            var distances = new System.Collections.Generic.List<float>();
            while (game.Intro.IsPlaying)
            {
                distances.Add(Vector3.Distance(view.transform.position, player.position));
                yield return null;
            }

            // Not a cut: the view comes closer to the player over several frames.
            Assert.That(distances.Count, Is.GreaterThan(10));
            var farthest = distances.Max();
            Assert.That(farthest, Is.GreaterThan(1.5f), "The view was never away from the player.");
            Assert.That(distances.Count(distance => distance > 0.3f && distance < farthest - 0.3f), Is.GreaterThan(3), "The view jumps back to the player.");
        }

        [UnityTest]
        public IEnumerator AFirstPress_OnlyAsksForASecond_WhichSkipsTheShot_AndTheSeedLiesOnTheGround()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            yield return Load(skipIntro: false);
            var game = Bootstrap;
            var seed = SeedOnTheGround(game);
            yield return Wait(0.8f);
            Assert.That(game.Intro.IsAskingToSkip, Is.False, "The hint shows before any press.");

            yield return Tap(keyboard.spaceKey);
            Assert.That(game.Intro.IsPlaying, Is.True, "A single press skipped the shot.");
            Assert.That(game.Intro.IsAskingToSkip, Is.True, "A first press does not say a second one skips.");

            yield return Tap(keyboard.spaceKey);
            Assert.That(game.Intro.IsPlaying, Is.False, "A second press did not skip the shot.");
            Assert.That(game.Intro.IsAskingToSkip, Is.False, "The hint stays after the shot.");
            Assert.That(Vector3.Distance(game.ViewCamera.transform.position, game.Interactor.transform.position), Is.LessThan(2f), "The view is not back with the player at once.");
            Assert.That(seed.transform.position.y - Ground(seed.transform.position), Is.LessThan(0.1f), "The seed is left in the air.");
            Assert.That(game.Controls.enabled, Is.True);
            Assert.That(game.ObjectiveHud.IsVisible, Is.True);
        }

        [UnityTest]
        public IEnumerator TheHint_GoesAfterThreeSeconds_AndAPressThenAsksAgain()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            yield return Load(skipIntro: false);
            var game = Bootstrap;

            yield return Tap(keyboard.spaceKey);
            Assert.That(game.Intro.IsAskingToSkip, Is.True);
            yield return Wait(3.2f);
            Assert.That(game.Intro.IsAskingToSkip, Is.False, "The hint did not go.");
            Assert.That(game.Intro.IsPlaying, Is.True);

            yield return Tap(keyboard.spaceKey);
            Assert.That(game.Intro.IsPlaying, Is.True, "A press long after the first skipped the shot.");
            Assert.That(game.Intro.IsAskingToSkip, Is.True);

            // The shot lets go of its button listener before the fixture resets the input system.
            game.Intro.Skip();
        }

        private IEnumerator Tap(UnityEngine.InputSystem.Controls.KeyControl key)
        {
            Press(key);
            yield return null;
            Release(key);
            yield return null;
        }

        private static Fungiiiii.Items.DroppedItem SeedOnTheGround(GameBootstrap game)
        {
            return game.Dropped.Items.Single(item => item.Item == Seed(game));
        }

        private static float Ground(Vector3 point)
        {
            return Terrain.activeTerrain.SampleHeight(point) + Terrain.activeTerrain.GetPosition().y;
        }

        private static float Flat(Vector3 a, Vector3 b)
        {
            var offset = a - b;
            offset.y = 0f;
            return offset.magnitude;
        }
    }
}
