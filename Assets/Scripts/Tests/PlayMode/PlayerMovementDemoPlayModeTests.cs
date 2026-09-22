using System.Collections;
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
    public sealed class PlayerMovementDemoPlayModeTests
    {
        private const string ScenePath = "Assets/Scenes/Prototype/MouvementJoueurScene.unity";
        // The scene-placed bootstrap is the demo's only entry point, and builds the demo
        // under itself.
        private const string DemoRootName = "Player Movement Prototype Bootstrap";
        private const string PlayerName = "Player";

        private static IEnumerator LoadPrototypeScene()
        {
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return load;
            yield return null;
        }

        [UnityTest]
        public IEnumerator RuntimeBootstrapCreatesThePlayerAndTheLowPassage()
        {
            yield return LoadPrototypeScene();

            var demoRoot = GameObject.Find(DemoRootName);
            Assert.That(demoRoot, Is.Not.Null, "The runtime movement demo was not created after scene load.");

            try
            {
                var playerObject = GameObject.Find(PlayerName);
                Assert.That(playerObject, Is.Not.Null, "The runtime player was not created.");
                Assert.That(playerObject.GetComponent<PlayerMotor>(), Is.Not.Null);
                Assert.That(playerObject.GetComponent<CharacterController>(), Is.Not.Null);
                Assert.That(playerObject.GetComponent<PlayerInputReader>(), Is.Not.Null);

                Assert.That(
                    GameObject.Find("Low Passage Lintel (Crouch To Pass)"),
                    Is.Not.Null,
                    "The low passage was not created, so crouching cannot be exercised against collision.");
            }
            finally
            {
                if (demoRoot != null)
                {
                    Object.Destroy(demoRoot);
                }
            }
        }

        [UnityTest]
        public IEnumerator SceneLoadCreatesExactlyOnePlayer()
        {
            yield return LoadPrototypeScene();

            var motors = Object.FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None);
            try
            {
                Assert.That(
                    motors.Length,
                    Is.EqualTo(1),
                    "The demo was built more than once: every copy spawns a player reading the same keyboard.");
            }
            finally
            {
                foreach (var motor in motors)
                {
                    if (motor != null)
                    {
                        Object.Destroy(motor.transform.root.gameObject);
                    }
                }
            }
        }

        [UnityTest]
        public IEnumerator PlayerMovesWhenGivenForwardInput()
        {
            yield return LoadPrototypeScene();

            var demoRoot = GameObject.Find(DemoRootName);
            Assert.That(demoRoot, Is.Not.Null);

            try
            {
                var playerObject = GameObject.Find(PlayerName);
                var motor = playerObject.GetComponent<PlayerMotor>();
                var startPosition = playerObject.transform.position;

                // Ticked directly rather than through Update, so no frame elapses and the
                // input reader cannot overwrite the input this test just set.
                motor.MoveInput = Vector2.up;
                for (var step = 0; step < 10; step++)
                {
                    motor.Tick(0.05f);
                }

                Assert.That(
                    playerObject.transform.position.z - startPosition.z,
                    Is.GreaterThan(0.5f),
                    "The player did not move forward while the CharacterController was driven.");
            }
            finally
            {
                if (demoRoot != null)
                {
                    Object.Destroy(demoRoot);
                }
            }
        }

        [UnityTest]
        public IEnumerator CrouchingShrinksTheControllerCapsule()
        {
            yield return LoadPrototypeScene();

            var demoRoot = GameObject.Find(DemoRootName);
            Assert.That(demoRoot, Is.Not.Null);

            try
            {
                var playerObject = GameObject.Find(PlayerName);
                var motor = playerObject.GetComponent<PlayerMotor>();
                var controller = playerObject.GetComponent<CharacterController>();
                var standingHeight = controller.height;

                motor.IsCrouching = true;
                for (var step = 0; step < 10; step++)
                {
                    motor.Tick(0.05f);
                }

                var crouchedHeight = controller.height;
                Assert.That(
                    crouchedHeight,
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
            finally
            {
                if (demoRoot != null)
                {
                    Object.Destroy(demoRoot);
                }
            }
        }
    }
}
