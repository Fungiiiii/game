using System.Collections;
using System.Linq;
using Fungiiiii.Capture;
using Fungiiiii.Runtime.Player;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using static Fungiiiii.Tests.PlayMode.PlaytestLoopGame;

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>A wild Spongeus that drank, squeezed by hand by a player crouched behind it.</summary>
    public sealed class SpongeusSqueezePlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator Crouched_BehindAWetSpongeus_EGivesItsWater_AndItRunsOffDry()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            yield return Load();
            var game = Bootstrap;
            var wet = WetSpongeus(game);
            yield return WaitUntil(() => wet.IsWet, 15f);
            Assert.That(wet.IsWet, Is.True, "The Spongeus did not drink.");

            Press(keyboard.leftCtrlKey);
            yield return null;
            var squeeze = wet.GetComponent<WildSqueeze>();
            StandBehind(game, wet, 1.2f);
            yield return null;
            yield return null;

            Assert.That(wet.IsSneakedUpOn, Is.True, "Crouched behind it, the player is noticed.");
            Assert.That(squeeze.CanBeSqueezed, Is.True);
            Assert.That(game.Interactor.Targeted, Is.SameAs(squeeze), "The Spongeus is not aimed at from behind.");
            Assert.That(game.Interactor.Prompt.PressActionKey, Is.EqualTo(UiText.HintSqueeze), "Nothing says E squeezes it.");

            var water = WaterId(game);
            Assert.That(squeeze.Interact(game.Inventory), Is.True);
            Assert.That(game.Inventory.Count(water), Is.EqualTo(1), "The squeeze gave no water.");
            Assert.That(wet.IsWet, Is.False, "Squeezed, it is still wet.");
            Assert.That(wet.IsUnaware, Is.False, "Squeezed, it does not run off.");
            Assert.That(squeeze.Interact(game.Inventory), Is.False, "A dry Spongeus was squeezed.");
            Release(keyboard.leftCtrlKey);
        }

        [UnityTest]
        public IEnumerator Standing_OrInFront_TheSpongeusNoticesThePlayer_AndCannotBeSqueezed()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            yield return Load();
            var game = Bootstrap;
            var wet = WetSpongeus(game);
            yield return WaitUntil(() => wet.IsWet, 15f);
            var squeeze = wet.GetComponent<WildSqueeze>();

            // Standing behind it.
            StandBehind(game, wet, 1.2f);
            yield return null;
            Assert.That(wet.IsSneakedUpOn, Is.False, "A standing player goes unnoticed.");
            Assert.That(squeeze.CanBeSqueezed, Is.False, "It is squeezed by a standing player.");

            // Crouched, in front of it.
            Press(keyboard.leftCtrlKey);
            yield return null;
            StandBehind(game, wet, -1.2f);
            yield return null;
            Assert.That(wet.IsSneakedUpOn, Is.False, "A player in front of it goes unnoticed.");
            Assert.That(squeeze.CanBeSqueezed, Is.False, "It is squeezed from the front.");
            Release(keyboard.leftCtrlKey);
        }

        [UnityTest]
        public IEnumerator LookingAtAFullSpongeus_FromSevenMetres_ShowsTheCrouchKey_UntilTheFirstSqueeze()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            yield return Load();
            var game = Bootstrap;
            var wet = WetSpongeus(game);
            yield return WaitUntil(() => wet.IsDripping, 15f);
            Assert.That(wet.IsDripping, Is.True, "The Spongeus did not fill up.");
            var hint = game.SneakHint;

            StandBehind(game, wet, 7f);
            yield return null;
            yield return null;
            Assert.That(hint.IsShown, Is.True, "Nothing says to sneak up on the full Spongeus.");
            Assert.That(hint.ActionKey, Is.EqualTo(UiText.HintSneak));
            Assert.That(hint.KeyLabel, Is.Not.Empty, "The crouch key has no name.");

            game.ViewCamera.GetComponent<FirstPersonCamera>().SetView(wet.transform.eulerAngles.y + 180f, 0f);
            yield return null;
            yield return null;
            Assert.That(hint.IsShown, Is.False, "The hint shows without looking at the Spongeus.");

            Press(keyboard.leftCtrlKey);
            StandBehind(game, wet, 7f);
            yield return null;
            yield return null;
            Assert.That(hint.IsShown, Is.False, "The hint stays once the player crouches.");

            StandBehind(game, wet, 1.2f);
            yield return null;
            yield return null;
            Assert.That(wet.GetComponent<WildSqueeze>().Interact(game.Inventory), Is.True);
            Assert.That(hint.IsLearned, Is.True, "The first squeeze did not end the hint.");
            Release(keyboard.leftCtrlKey);
        }

        /// <summary>A wild Spongeus next to a full fern, the player looking at it from 8 m: it goes and drinks.</summary>
        private static WildMushroom WetSpongeus(Fungiiiii.Game.GameBootstrap game)
        {
            var spongeus = game.Mushrooms.First(each => each.Soak != null);
            var fern = game.RefillingPlants.Plants.OrderBy(plant => Vector3.Distance(plant.Position, spongeus.transform.position)).First();
            fern.Restart(1f);
            spongeus.transform.position = fern.Position + Vector3.right * 2f;
            StandBefore(fern.Position, 8f);
            game.ViewCamera.GetComponent<FirstPersonCamera>().LookAt(spongeus.transform.position);
            return spongeus;
        }

        /// <summary>Puts the player <paramref name="distance"/> metres behind it (in front when negative), looking at it.</summary>
        private static void StandBehind(Fungiiiii.Game.GameBootstrap game, WildMushroom spongeus, float distance)
        {
            var facing = spongeus.transform.forward;
            facing.y = 0f;
            var position = spongeus.transform.position - facing.normalized * distance;
            position.y = Terrain.activeTerrain.SampleHeight(position) + Terrain.activeTerrain.GetPosition().y + 1.1f;
            var player = Object.FindFirstObjectByType<PlayerMotor>();
            var controller = player.GetComponent<CharacterController>();
            controller.enabled = false;
            player.transform.position = position;
            controller.enabled = true;
            game.ViewCamera.GetComponent<FirstPersonCamera>().LookAt(spongeus.transform.position + Vector3.up * 0.2f);
        }

        private static string WaterId(Fungiiiii.Game.GameBootstrap game)
        {
            return game.RefillingPlants.Plants[0].Yield.Id;
        }
    }
}
