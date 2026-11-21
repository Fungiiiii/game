using System.Collections;
using Fungiiiii.Runtime.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>The jump of the Player prefab, on bare ground, away from any scenery.</summary>
    public sealed class PlayerJumpPlayModeTests
    {
        private const float JumpHeight = 1.2f;

        private GameObject ground;
        private GameObject player;
        private PlayerMotor motor;

        [UnitySetUp]
        public IEnumerator StandOnBareGround()
        {
#if UNITY_EDITOR
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.transform.localScale = Vector3.one * 10f;
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Player.prefab");
            player = Object.Instantiate(prefab, Vector3.up * 1.5f, Quaternion.identity);
            motor = player.GetComponent<PlayerMotor>();

            var elapsed = 0f;
            while (!motor.IsGrounded && elapsed < 3f)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(motor.IsGrounded, Is.True, "The player never landed.");
#else
            Assert.Ignore("The Player prefab is read from the editor's assets.");
            yield break;
#endif
        }

        [UnityTearDown]
        public IEnumerator Clear()
        {
            Object.Destroy(player);
            Object.Destroy(ground);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AJump_RisesToItsHeight_AtAnyFrameRate([Values(60, 10)] int framesPerSecond)
        {
            // Ticked by the test, step by step: the batch run's own frame rate swings.
            motor.enabled = false;
            var start = motor.transform.position.y;
            Assert.That(motor.TryJump(), Is.True);

            var step = 1f / framesPerSecond;
            var peak = start;
            for (var elapsed = 0f; elapsed < 1f; elapsed += step)
            {
                motor.Tick(step);
                peak = Mathf.Max(peak, motor.transform.position.y);
            }

            // At 10 frames a second, the highest step can fall short of the top by g·dt²/8: 2.5 cm.
            Assert.That(peak - start, Is.EqualTo(JumpHeight).Within(0.05f));
            Assert.That(motor.IsGrounded, Is.True, "The player did not land back.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator NoJump_InTheAir_NorWhileCrouching()
        {
            Assert.That(motor.TryJump(), Is.True);
            yield return null;
            yield return null;
            Assert.That(motor.TryJump(), Is.False, "The player jumped again in the air.");

            var elapsed = 0f;
            while (!motor.IsGrounded && elapsed < 2f)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            motor.IsCrouching = true;
            Assert.That(motor.TryJump(), Is.False, "The player jumped while crouching.");
        }
    }
}
