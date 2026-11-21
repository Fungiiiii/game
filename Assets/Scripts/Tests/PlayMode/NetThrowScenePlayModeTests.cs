using System.Collections;
using Fungiiiii.Capture;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class NetThrowScenePlayModeTests
    {
        private const string ScenePath = "Assets/Scenes/Prototype/NetThrowScene.unity";

        /// <summary>Flight of a net dropped from just above a mushroom, plus the freeze, with margin.</summary>
        private const float OutcomeTimeoutSeconds = 3f;

        private static IEnumerator LoadNetScene()
        {
#if UNITY_EDITOR
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return load;
#endif
            // Start runs on the first frame after the load.
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator SceneLoad_BuildsThePlayerTheNetAndThreeMushrooms()
        {
            yield return LoadNetScene();

            var demo = Object.FindFirstObjectByType<NetThrowDemo>();
            Assert.That(demo, Is.Not.Null, "The scene has no NetThrowDemo bootstrap.");
            Assert.That(demo.Thrower, Is.Not.Null, "The net thrower was not set up.");
            Assert.That(demo.Thrower.Cycle.Phase, Is.EqualTo(NetPhase.Ready));
            Assert.That(demo.Mushrooms.Count, Is.EqualTo(3));
            Assert.That(Object.FindFirstObjectByType<PrototypeFirstPersonCamera>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator TheArc_ShowsOnlyWhileAiming()
        {
            yield return LoadNetScene();
            var thrower = Object.FindFirstObjectByType<NetThrowDemo>().Thrower;

            yield return null;
            Assert.That(thrower.VisibleDotCount, Is.Zero, "The arc shows without aiming.");

            thrower.HoldAim = true;
            yield return null;
            yield return null;
            Assert.That(thrower.VisibleDotCount, Is.InRange(1, 24));
        }

        [UnityTest]
        public IEnumerator ANetDroppedOnAMushroom_LandsOnIt_AndChangesItsState()
        {
            yield return LoadNetScene();

            var demo = Object.FindFirstObjectByType<NetThrowDemo>();
            var mushroom = demo.Mushrooms[0];
            Assert.That(mushroom.State, Is.EqualTo(MushroomCaptureState.Idle));

            var landedOnMushroom = false;
            demo.Thrower.Cycle.Landed += onMushroom => landedOnMushroom = onMushroom;

            // Dropped from above, so the landing does not depend on the aim of a long throw.
            var origin = mushroom.transform.position + Vector3.up * 3f;
            Assert.That(demo.Thrower.TryThrow(origin, Vector3.down), Is.True);

            var elapsed = 0f;
            while (elapsed < OutcomeTimeoutSeconds && demo.Thrower.Cycle.Phase != NetPhase.Returning)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.That(landedOnMushroom, Is.True, "The net did not land on the mushroom.");
            Assert.That(demo.Thrower.Cycle.Phase, Is.EqualTo(NetPhase.Returning), "The freeze never ended.");
            Assert.That(
                mushroom.State,
                Is.EqualTo(MushroomCaptureState.Captured).Or.EqualTo(MushroomCaptureState.Flee),
                "The capture outcome was not applied to the mushroom.");
        }
    }
}
