using System.Collections;
using Fungiiiii.Runtime.Player;
using NUnit.Framework;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class PlayerMovementDemoPlayModeTests
    {
        private const string ScenePath = "Assets/Scenes/Prototype/MouvementJoueurScene.unity";
        private const int SpawnFrameBudget = 60;

        private GameObject localPlayer;

        private static IEnumerator LoadPrototypeScene()
        {
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return load;
            yield return null;
        }

        private IEnumerator StartHostAndWaitForLocalPlayer()
        {
            var manager = NetworkManager.Singleton;
            Assert.That(manager, Is.Not.Null, "The bootstrap did not create a NetworkManager.");
            Assert.That(manager.StartHost(), Is.True, "The host did not start.");

            for (var frame = 0; frame < SpawnFrameBudget; frame++)
            {
                var client = manager.LocalClient;
                if (client != null && client.PlayerObject != null)
                {
                    localPlayer = client.PlayerObject.gameObject;
                    yield break;
                }

                yield return null;
            }

            Assert.Fail("The host's local player was never spawned.");
        }

        [UnityTearDown]
        public IEnumerator ShutDownNetworking()
        {
            // NetworkManager lives in DontDestroyOnLoad, so it would otherwise survive into
            // the next test and block that test's bootstrap from creating its own.
            var manager = NetworkManager.Singleton;
            if (manager != null)
            {
                manager.Shutdown();
                Object.Destroy(manager.gameObject);
            }

            localPlayer = null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator SceneLoadBuildsTheLowPassageAndANetworkManagerButNoPlayer()
        {
            yield return LoadPrototypeScene();

            Assert.That(
                GameObject.Find("Low Passage Lintel (Crouch To Pass)"),
                Is.Not.Null,
                "The low passage was not created, so crouching cannot be exercised against collision.");

            var manager = NetworkManager.Singleton;
            Assert.That(manager, Is.Not.Null, "The bootstrap did not create a NetworkManager.");
            Assert.That(manager.NetworkConfig.PlayerPrefab, Is.Not.Null, "No player prefab is registered.");

            Assert.That(
                Object.FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None),
                Is.Empty,
                "A player exists before any session started; players must only come from Netcode.");
        }

        [UnityTest]
        public IEnumerator SceneLoadBuildsTheDemoOnce()
        {
            yield return LoadPrototypeScene();

            // Regression: a RuntimeInitializeOnLoadMethod hook used to add a second
            // bootstrap next to the one placed in the scene.
            Assert.That(
                Object.FindObjectsByType<PlayerMovementDemoBootstrap>(FindObjectsSortMode.None),
                Has.Length.EqualTo(1),
                "The demo has more than one entry point, so it is built more than once.");
        }

        [UnityTest]
        public IEnumerator HostSpawnsExactlyOneOwnerAuthoritativePlayer()
        {
            yield return LoadPrototypeScene();
            yield return StartHostAndWaitForLocalPlayer();

            Assert.That(Object.FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None), Has.Length.EqualTo(1));

            var networkObject = localPlayer.GetComponent<NetworkObject>();
            Assert.That(networkObject.IsOwner, Is.True, "The host does not own its own player.");

            Assert.That(localPlayer.GetComponent<PlayerMotor>().enabled, Is.True, "The owner's motor is disabled.");
            Assert.That(localPlayer.GetComponent<PlayerInputReader>().enabled, Is.True, "The owner's input is disabled.");

            foreach (var networkTransform in localPlayer.GetComponentsInChildren<NetworkTransform>())
            {
                Assert.That(
                    networkTransform.AuthorityMode,
                    Is.EqualTo(NetworkTransform.AuthorityModes.Owner),
                    networkTransform.name + " is not owner-authoritative.");
            }
        }

        [UnityTest]
        public IEnumerator PlayerMovesWhenGivenForwardInput()
        {
            yield return LoadPrototypeScene();
            yield return StartHostAndWaitForLocalPlayer();

            var motor = localPlayer.GetComponent<PlayerMotor>();
            var startPosition = localPlayer.transform.position;

            // Ticked directly rather than through Update, so no frame elapses and the
            // input reader cannot overwrite the input this test just set.
            motor.MoveInput = Vector2.up;
            for (var step = 0; step < 10; step++)
            {
                motor.Tick(0.05f);
            }

            Assert.That(
                localPlayer.transform.position.z - startPosition.z,
                Is.GreaterThan(0.5f),
                "The player did not move forward while the CharacterController was driven.");
        }

        [UnityTest]
        public IEnumerator CrouchingShrinksTheControllerCapsule()
        {
            yield return LoadPrototypeScene();
            yield return StartHostAndWaitForLocalPlayer();

            var motor = localPlayer.GetComponent<PlayerMotor>();
            var controller = localPlayer.GetComponent<CharacterController>();
            var standingHeight = controller.height;

            motor.IsCrouching = true;
            for (var step = 0; step < 10; step++)
            {
                motor.Tick(0.05f);
            }

            Assert.That(
                controller.height,
                Is.LessThan(standingHeight),
                "Crouching did not shrink the controller, so the low passage stays impassable.");

            motor.IsCrouching = false;
            for (var step = 0; step < 10; step++)
            {
                motor.Tick(0.05f);
            }

            Assert.That(
                controller.height,
                Is.EqualTo(standingHeight).Within(0.001f),
                "The player did not stand back up after releasing crouch.");
        }
    }
}
