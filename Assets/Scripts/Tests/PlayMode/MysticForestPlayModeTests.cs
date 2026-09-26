#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class MysticForestPlayModeTests
    {
        private const string ScenePath = "Assets/Scenes/Biomes/MysticForest.unity";

        // Terrain tree colliders only exist once the game runs, never in edit mode:
        // this can only be checked in Play Mode, on the real scene.
        [UnityTest]
        public IEnumerator TreeTrunksBlockInPlayMode()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return new WaitForFixedUpdate();

            var terrain = Object.FindFirstObjectByType<Terrain>();
            TerrainData data = terrain.terrainData;
            int treePrototype = System.Array.FindIndex(data.treePrototypes, p => p.prefab.name == "PF_Tree");
            Assume.That(treePrototype, Is.GreaterThanOrEqualTo(0), "PF_Tree is not a tree prototype of the terrain");

            TreeInstance tree = data.treeInstances.First(instance => instance.prototypeIndex == treePrototype);
            Vector3 trunk = Vector3.Scale(tree.position, data.size) + terrain.transform.position;

            bool hit = Physics.Raycast(trunk + new Vector3(-3f, 1.5f, 0f), Vector3.right, out RaycastHit info, 6f);

            Assert.That(hit, Is.True, "a ray aimed at a tree trunk went through it");
            Assert.That(info.collider, Is.InstanceOf<TerrainCollider>());

            // The ray must stop on the trunk surface, not on the ground or anything else.
            float trunkRadius = data.treePrototypes[treePrototype].prefab.GetComponent<CapsuleCollider>().radius * tree.widthScale;
            float distanceToAxis = Vector2.Distance(new Vector2(info.point.x, info.point.z), new Vector2(trunk.x, trunk.z));
            Assert.That(distanceToAxis, Is.EqualTo(trunkRadius).Within(0.05f), "the ray did not stop on the trunk surface");
        }
    }
}
#endif
