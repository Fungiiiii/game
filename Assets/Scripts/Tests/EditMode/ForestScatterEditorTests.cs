using System.Collections.Generic;
using System.Linq;
using Fungiiiii.Editor.World;
using Fungiiiii.Runtime.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class ForestScatterEditorTests
    {
        private const string PrefabPath = "Assets/ForestScatterEditorTests_Prefab.prefab";
        private const int OriginalSeed = 7;
        private const float TerrainSize = 60f;

        private static readonly List<ExclusionCircle> NoExclusions = new List<ExclusionCircle>();

        private GameObject prefab;
        private GameObject terrainObject;
        private Terrain terrain;
        private ForestScatter trees;

        [SetUp]
        public void SetUp()
        {
            var source = new GameObject("ForestScatterEditorTests_Source");
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(source.transform, false);
            source.AddComponent<LODGroup>().SetLODs(new[] { new LOD(0.01f, visual.GetComponentsInChildren<Renderer>()) });
            prefab = PrefabUtility.SaveAsPrefabAsset(source, PrefabPath);
            Object.DestroyImmediate(source);

            var data = new TerrainData { heightmapResolution = 33 };
            data.size = new Vector3(TerrainSize, 10f, TerrainSize);
            terrainObject = Terrain.CreateTerrainGameObject(data);
            terrain = terrainObject.GetComponent<Terrain>();

            trees = CreateLayer("Trees", OriginalSeed);
            Undo.IncrementCurrentGroup();
        }

        [TearDown]
        public void TearDown()
        {
            TerrainData data = terrain.terrainData;
            Object.DestroyImmediate(terrainObject);
            Object.DestroyImmediate(data);
            AssetDatabase.DeleteAsset(PrefabPath);
        }

        [Test]
        public void GenerateWritesEveryPlacementIntoTheTerrain()
        {
            int expected = trees.ComputePlacements(NoExclusions).Count;

            int written = TerrainForestGenerator.Generate(terrain, NoExclusions);

            Assert.That(written, Is.EqualTo(expected));
            Assert.That(terrain.terrainData.treeInstanceCount, Is.EqualTo(expected));
            Assert.That(terrain.terrainData.treePrototypes.Single().prefab, Is.EqualTo(prefab));
        }

        [Test]
        public void InstancesLandOnTheTerrainAndKeepTheirScale()
        {
            TerrainForestGenerator.Generate(terrain, NoExclusions);

            Assert.That(terrain.terrainData.treeInstances, Is.Not.Empty);
            foreach (TreeInstance instance in terrain.terrainData.treeInstances)
            {
                Assert.That(instance.position.x, Is.InRange(0f, 1f));
                Assert.That(instance.position.z, Is.InRange(0f, 1f));
                Assert.That(instance.widthScale, Is.InRange(0.85f, 1.25f));
                Assert.That(instance.heightScale, Is.EqualTo(instance.widthScale));
            }
        }

        // Requirement changed with the low poly forest: a later layer now loses the
        // instances that would grow into an earlier layer's, so only the first layer
        // keeps every placement.
        [Test]
        public void EachLayerBecomesItsOwnPrototype()
        {
            ForestScatter rocks = CreateLayer("Rocks", seed: 99);

            TerrainForestGenerator.Generate(terrain, NoExclusions);

            TreeInstance[] instances = terrain.terrainData.treeInstances;
            Assert.That(terrain.terrainData.treePrototypes.Length, Is.EqualTo(2));
            Assert.That(instances.Count(i => i.prototypeIndex == 0), Is.EqualTo(trees.ComputePlacements(NoExclusions).Count));
            Assert.That(instances.Count(i => i.prototypeIndex == 1), Is.InRange(1, rocks.ComputePlacements(NoExclusions).Count));
        }

        [Test]
        public void InstancesOfDifferentLayersNeverGrowIntoEachOther()
        {
            CreateLayer("Rocks", seed: 99);
            CreateLayer("Bushes", seed: 1234);

            TerrainForestGenerator.Generate(terrain, NoExclusions);

            TreeInstance[] instances = terrain.terrainData.treeInstances;
            float spacing = TerrainForestGenerator.CrossLayerSpacing(5f, 5f);
            for (int i = 0; i < instances.Length; i++)
            {
                for (int j = i + 1; j < instances.Length; j++)
                {
                    if (instances[i].prototypeIndex == instances[j].prototypeIndex)
                    {
                        continue;
                    }

                    var a = new Vector2(instances[i].position.x, instances[i].position.z) * TerrainSize;
                    var b = new Vector2(instances[j].position.x, instances[j].position.z) * TerrainSize;
                    Assert.That(Vector2.Distance(a, b), Is.GreaterThanOrEqualTo(spacing - 0.001f));
                }
            }
        }

        [Test]
        public void InstancesOnASlopeSinkSoNoSideFloats()
        {
            // A constant 20 degree slope rising along X, starting 5 m above the
            // terrain's bottom so every foot has room to sink.
            TerrainData data = terrain.terrainData;
            int n = data.heightmapResolution;
            const float floor = 5f;
            float rise = TerrainSize * Mathf.Tan(20f * Mathf.Deg2Rad);
            data.size = new Vector3(TerrainSize, floor + rise, TerrainSize);
            var heights = new float[n, n];
            for (int z = 0; z < n; z++)
            {
                for (int x = 0; x < n; x++)
                {
                    heights[z, x] = (floor + rise * x / (n - 1f)) / (floor + rise);
                }
            }

            data.SetHeights(0, 0, heights);

            TerrainForestGenerator.Generate(terrain, NoExclusions);

            float footRadius = TerrainForestGenerator.FootRadius(prefab);
            Assume.That(footRadius, Is.GreaterThan(0.1f));
            foreach (TreeInstance instance in data.treeInstances)
            {
                float ground = data.GetInterpolatedHeight(instance.position.x, instance.position.z);
                float foot = instance.position.y * data.size.y;
                float expected = TerrainForestGenerator.SinkDepth(footRadius * instance.widthScale, 20f);
                Assert.That(ground - foot, Is.EqualTo(expected).Within(0.05f));
                Assert.That(ground - foot, Is.GreaterThan(footRadius * instance.widthScale * Mathf.Tan(20f * Mathf.Deg2Rad) - 0.01f),
                    "the downhill edge of the foot floats above the ground");
            }
        }

        [Test]
        public void ExclusionZonesStayEmpty()
        {
            var clearing = new ExclusionCircle(new Vector3(TerrainSize / 2f, 0f, TerrainSize / 2f), 10f);

            TerrainForestGenerator.Generate(terrain, new List<ExclusionCircle> { clearing });

            foreach (TreeInstance instance in terrain.terrainData.treeInstances)
            {
                var world = new Vector3(instance.position.x * TerrainSize, 0f, instance.position.z * TerrainSize);
                Assert.That(clearing.Contains(world), Is.False);
            }
        }

        // Regression: the new seed used to be recorded in its own undo step,
        // separate from the generation. One Ctrl+Z restored the forest but left
        // the new seed, so the scene no longer matched its own seed.
        [Test]
        public void ReseedAndGenerateIsUndoneInOneStep()
        {
            TerrainForestGenerator.Generate(terrain, NoExclusions);
            Vector3[] before = Positions();
            Undo.IncrementCurrentGroup();

            ForestScatterEditor.ReseedAndGenerate(trees, newSeed: 99);
            Assume.That(Seed(), Is.EqualTo(99));
            Assume.That(Positions(), Is.Not.EqualTo(before));

            Undo.PerformUndo();

            Assert.That(Seed(), Is.EqualTo(OriginalSeed));
            Assert.That(Positions(), Is.EqualTo(before));
        }

        [Test]
        public void NoInstanceOnGroundSteeperThanTheLayerAllows()
        {
            // A ramp along X: 0 m high on the left half, rising 30 m over the right half.
            TerrainData data = terrain.terrainData;
            int n = data.heightmapResolution;
            var heights = new float[n, n];
            for (int z = 0; z < n; z++)
            {
                for (int x = n / 2; x < n; x++)
                {
                    heights[z, x] = (x - n / 2f) / (n / 2f);
                }
            }

            data.size = new Vector3(TerrainSize, 30f, TerrainSize);
            data.SetHeights(0, 0, heights);
            trees.ConfigureLimits(Vector2.zero, slope: 20f);

            TerrainForestGenerator.Generate(terrain, NoExclusions);

            Assert.That(data.treeInstances, Is.Not.Empty);
            foreach (TreeInstance instance in data.treeInstances)
            {
                Assert.That(TerrainForestGenerator.Steepness(data, instance.position.x, instance.position.z), Is.LessThanOrEqualTo(20f));
            }
        }

        private ForestScatter CreateLayer(string name, int seed)
        {
            var layerObject = new GameObject(name);
            layerObject.transform.SetParent(terrainObject.transform, false);
            layerObject.transform.position = new Vector3(TerrainSize / 2f, 0f, TerrainSize / 2f);

            var layer = layerObject.AddComponent<ForestScatter>();
            layer.Configure(new Vector2(TerrainSize, TerrainSize), 5f, 0.85f, 1.25f, rotate: true, newSeed: seed);
            var serialized = new SerializedObject(layer);
            serialized.FindProperty("prefab").objectReferenceValue = prefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return layer;
        }

        private int Seed() => new SerializedObject(trees).FindProperty("seed").intValue;

        private Vector3[] Positions() => terrain.terrainData.treeInstances.Select(i => i.position).ToArray();
    }
}
