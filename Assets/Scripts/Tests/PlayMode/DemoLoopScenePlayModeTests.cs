using System.Collections;
using System.Linq;
using Fungiiiii.Capture;
using Fungiiiii.Demo;
using Fungiiiii.Runtime.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class DemoLoopScenePlayModeTests
    {
        private const string ScenePath = "Assets/Scenes/Prototype/DemoLoopScene.unity";
        private const string ForestScenePath = "Assets/Scenes/Biomes/MysticForest.unity";

        /// <summary>The forest is large: loading it additively takes a few seconds in batch mode.</summary>
        private const float LoadTimeoutSeconds = 60f;

        /// <summary>Flight, freeze and return of a net dropped from just above, with margin.</summary>
        private const float ThrowTimeoutSeconds = 5f;

        /// <summary>A Spongeus caught by surprise has 60 %: 12 failures in a row are below 1 in 50 000.</summary>
        private const int MaxCaptureAttempts = 12;

        private static IEnumerator LoadDemo()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
#endif
            var elapsed = 0f;
            DemoLoopBootstrap demo = null;
            while (elapsed < LoadTimeoutSeconds && (demo == null || !demo.IsReady))
            {
                demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(demo, Is.Not.Null, "The scene has no DemoLoopBootstrap.");
            Assert.That(demo.IsReady, Is.True, "The demo was not ready in time.");
        }

        [UnityTest]
        public IEnumerator SceneLoad_LoadsTheForest_WithOnePlayerAndOneCamera()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();

            Assert.That(SceneManager.GetSceneByPath(ForestScenePath).isLoaded, Is.True, "The forest is not loaded.");
            Assert.That(
                Object.FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None),
                Has.Length.EqualTo(1),
                "There must be exactly one player.");
            Assert.That(
                Object.FindObjectsByType<FirstPersonTestPlayer>(FindObjectsSortMode.None),
                Is.Empty,
                "The forest's test player is still active.");

            var renderingCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)
                .Where(camera => camera.isActiveAndEnabled)
                .ToArray();
            Assert.That(renderingCameras, Has.Length.EqualTo(1), "There must be exactly one camera.");
            Assert.That(
                renderingCameras[0].GetComponent<PrototypeFirstPersonCamera>(),
                Is.Not.Null,
                "The forest's camera is not the player's view.");

            Assert.That(demo.Mushrooms.Count, Is.GreaterThan(0));
            Assert.That(demo.Gatherables.Count, Is.GreaterThan(0));
            Assert.That(demo.Hole, Is.Not.Null);
            Assert.That(demo.IsInventoryOpen, Is.False);

            for (var index = 0; index < demo.Inventory.SlotCount; index++)
            {
                Assert.That(demo.Inventory.GetSlot(index).IsEmpty, Is.True, "The inventory must start empty.");
            }
        }

        [UnityTest]
        public IEnumerator GatheringASpot_PutsOneItemInTheInventory_AndHidesIt()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var spot = demo.Gatherables[0];

            Assert.That(spot.Interact(demo.Inventory), Is.True);
            Assert.That(demo.Inventory.Count(spot.Item.Id), Is.EqualTo(1));
            Assert.That(spot.IsAvailable, Is.False);
            Assert.That(spot.Interact(demo.Inventory), Is.False, "A gathered spot was gathered twice.");
        }

        [UnityTest]
        public IEnumerator ACapture_PutsTheMushroomInTheInventory()
        {
            yield return LoadDemo();
            var demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            var target = demo.Mushrooms.First(mushroom => mushroom.Profile == NetTargetProfile.Common);
            var item = demo.ItemFor(target);

            for (var attempt = 0; attempt < MaxCaptureAttempts && demo.Inventory.Count(item.Id) == 0; attempt++)
            {
                demo.ResetDemo();
                yield return null;

                // Dropped from above, so the landing does not depend on the aim of a long throw.
                var origin = target.transform.position + Vector3.up * 3f;
                Assert.That(demo.Thrower.TryThrow(origin, Vector3.down), Is.True);

                var elapsed = 0f;
                var returned = false;
                demo.Thrower.Cycle.Returned += OnReturned;
                while (elapsed < ThrowTimeoutSeconds && !returned)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                demo.Thrower.Cycle.Returned -= OnReturned;
                Assert.That(returned, Is.True, "The net never came back.");

                void OnReturned() => returned = true;
            }

            Assert.That(demo.Inventory.Count(item.Id), Is.EqualTo(1), "No capture put the mushroom in the inventory.");
        }
    }
}
