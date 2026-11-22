using System.Collections;
using System.Linq;
using Fungiiiii.Champimaison;
using Fungiiiii.Game;
using Fungiiiii.Inventory;
using Fungiiiii.Runtime.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>
    /// What the tests of the playtest loop share: loading the game, finding its items, and
    /// moving the player. Items are found through the game's own data, never by asset path.
    /// </summary>
    internal static class PlaytestLoopGame
    {
        public const string ScenePath = "Assets/Scenes/Game/Game.unity";
        public const string InteriorScenePath = "Assets/Scenes/Game/ChampimaisonInterior.unity";
        private const float LoadTimeoutSeconds = 60f;

        public static GameBootstrap Bootstrap => Object.FindFirstObjectByType<GameBootstrap>();

        public static IEnumerator Load()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            var elapsed = 0f;
            GameBootstrap game = null;
            while (elapsed < LoadTimeoutSeconds && (game == null || !game.IsReady))
            {
                game = Object.FindFirstObjectByType<GameBootstrap>();
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(game != null && game.IsReady, Is.True, "The game was not ready in time.");
        }

        public static IEnumerator Wait(float seconds)
        {
            var elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        public static IEnumerator WaitUntil(System.Func<bool> condition, float timeoutSeconds)
        {
            var elapsed = 0f;
            while (!condition() && elapsed < timeoutSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        public static ItemDefinition Seed(GameBootstrap game) => game.MoleHoles.Seed;

        public static ItemDefinition Wood(GameBootstrap game) => Ingredient(game, "slingshot", 0);

        public static ItemDefinition Pebble(GameBootstrap game) => Ingredient(game, "slingshot", 1);

        public static ItemDefinition Thread(GameBootstrap game) => Ingredient(game, "net", 0);

        public static ItemDefinition Slingshot(GameBootstrap game) => game.Crafting.Book.Recipes.First(recipe => recipe.Result.Id == "slingshot").Result;

        private static ItemDefinition Ingredient(GameBootstrap game, string resultId, int index)
        {
            return game.Crafting.Book.Recipes.First(recipe => recipe.Result.Id == resultId).Ingredients[index].Item;
        }

        /// <summary>Puts <paramref name="item"/> in the first slot of the quick bar and selects it: in hand.</summary>
        public static void PutInHand(GameBootstrap game, ItemDefinition item, int quantity = 1)
        {
            game.Inventory.AddToSlot(InventoryLayout.Standard.QuickBarFirstSlot, item, quantity);
            game.Hotbar.Select(0);
        }

        /// <summary>Takes the seed in hand and plants it in the first mole hole.</summary>
        public static FungalHole Plant(GameBootstrap game)
        {
            PutInHand(game, Seed(game));
            Assert.That(game.MoleHoles.Holes[0].Interact(game.Inventory), Is.True, "The seed was not planted.");
            return game.Hole;
        }

        /// <summary>Puts the player <paramref name="distance"/> metres south of <paramref name="point"/>, looking at it.</summary>
        public static void StandBefore(Vector3 point, float distance)
        {
            var player = Object.FindFirstObjectByType<PlayerMotor>();
            var controller = player.GetComponent<CharacterController>();
            var position = point + Vector3.back * distance;
            position.y = Terrain.activeTerrain.SampleHeight(position) + Terrain.activeTerrain.GetPosition().y + 1.1f;

            // A CharacterController would snap back to its cached position: it is off while moved.
            controller.enabled = false;
            player.transform.SetPositionAndRotation(position, Quaternion.identity);
            controller.enabled = true;
            Object.FindFirstObjectByType<FirstPersonCamera>().SetView(0f, 20f);
        }
    }
}
