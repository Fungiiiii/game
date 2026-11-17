#if UNITY_EDITOR
using System.Collections;
using Fungiiiii.Runtime.Player;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class MysticForestSpawnPlayModeTests
    {
        private const string ScenePath = "Assets/Scenes/Biomes/MysticForest.unity";

        /// <summary>Frames left to the CharacterController to settle after the load.</summary>
        private const int SettleFrames = 90;

        private FirstPersonTestPlayer player;
        private Terrain terrain;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            player = Object.FindFirstObjectByType<FirstPersonTestPlayer>();
            terrain = Object.FindFirstObjectByType<Terrain>();
            Assert.That(player, Is.Not.Null, "the scene has no first-person player");
        }

        [UnityTest]
        public IEnumerator PlayerSpawnsOnTheGroundWithoutFalling()
        {
            Vector3 start = player.transform.position;

            for (int frame = 0; frame < SettleFrames; frame++)
            {
                yield return null;
            }

            Vector3 end = player.transform.position;
            float feet = end.y - FirstPersonTestPlayer.ControllerHeight * 0.5f;
            float ground = terrain.SampleHeight(end) + terrain.transform.position.y;

            Assert.That(player.IsGrounded, Is.True, "the player is not standing on anything");
            Assert.That(feet, Is.EqualTo(ground).Within(0.15f), "the player's feet are not on the terrain");
            Assert.That(start.y - end.y, Is.LessThan(0.2f), "the player fell after spawning");
            Assert.That(Vector2.Distance(new Vector2(start.x, start.z), new Vector2(end.x, end.z)), Is.LessThan(0.05f),
                "the player slid after spawning");
        }

        [UnityTest]
        public IEnumerator PlayerCanWalkTowardsTheGiantTree()
        {
            // Driven by the test only: Update stops, so it cannot add its own frames.
            player.enabled = false;
            Vector3 start = player.transform.position;

            const float seconds = 2f;
            const float step = 1f / 60f;
            for (float time = 0f; time < seconds; time += step)
            {
                player.Step(Vector2.up, sprint: false, jump: false, step);
                yield return null;
            }

            float walked = Vector2.Distance(new Vector2(start.x, start.z),
                new Vector2(player.transform.position.x, player.transform.position.z));

            // 5 m/s for 2 s: anything well under 10 m means something blocks the way.
            Assert.That(walked, Is.GreaterThan(8f), "the player is blocked at the spawn");
            Assert.That(player.IsGrounded, Is.True);
        }

        [UnityTest]
        public IEnumerator SporesFloatAroundTheCameraInWorldSpace()
        {
            Camera eye = player.GetComponentInChildren<Camera>();
            ParticleSystem spores = eye.GetComponentInChildren<ParticleSystem>();
            Assert.That(spores, Is.Not.Null, "the camera carries no spores");
            Assert.That(spores.main.simulationSpace, Is.EqualTo(ParticleSystemSimulationSpace.World),
                "spores must stay in the air when the player turns, not follow the view");

            yield return null;
            yield return null;

            Assert.That(spores.isPlaying, Is.True);
            Assert.That(spores.particleCount, Is.GreaterThan(0), "no spore in the air");
        }

        [UnityTest]
        public IEnumerator SpawnFacesTheGiantTree()
        {
            GameObject giant = GameObject.Find("PF_TreeGiant");
            Assert.That(giant, Is.Not.Null, "the scene has no giant tree");

            Vector3 toGiant = giant.transform.position - player.transform.position;
            toGiant.y = 0f;
            Vector3 facing = player.transform.forward;
            facing.y = 0f;

            Assert.That(Vector3.Angle(facing, toGiant), Is.LessThan(5f), "the spawn does not face the giant tree");

            // Nothing solid stands between the eye and the giant's trunk.
            Camera eye = player.GetComponentInChildren<Camera>();
            Vector3 trunk = giant.transform.position + Vector3.up * 8f;
            Vector3 toTrunk = trunk - eye.transform.position;
            bool blocked = Physics.Raycast(eye.transform.position, toTrunk.normalized, out RaycastHit hit, toTrunk.magnitude - 4f);
            Assert.That(blocked, Is.False, blocked ? $"the view is blocked by {hit.collider.name}" : string.Empty);
            yield return null;
        }
    }
}
#endif
