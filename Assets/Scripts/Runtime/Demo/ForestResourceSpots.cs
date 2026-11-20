using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Demo
{
    /// <summary>
    /// Reads from the forest's Terrain where resources may appear: the decor mushrooms
    /// scattered over it become the spots of the gatherable mushrooms, and the foot of
    /// its trees the spots of the sticks.
    /// </summary>
    internal static class ForestResourceSpots
    {
        /// <summary>Directions tried around a trunk before the tree is given up.</summary>
        private const int DrawsPerTree = 6;

        /// <summary>
        /// The pose of every decor mushroom the Terrain draws, and a copy of the Terrain's
        /// data without them. The copy is for this play session only: the asset of the
        /// forest is never changed.
        /// </summary>
        /// <returns>The copy to give to the Terrain, or null when the Terrain draws no such mushroom.</returns>
        public static TerrainData TakeDecorMushrooms(Terrain terrain, GameObject mushroomPrefab, List<GatherableSpot> spots)
        {
            var data = terrain.terrainData;
            var prototype = PrototypeIndexOf(data, mushroomPrefab);
            if (prototype < 0)
            {
                return null;
            }

            var origin = terrain.GetPosition();
            var size = data.size;
            var kept = new List<TreeInstance>(data.treeInstanceCount);

            foreach (var tree in data.treeInstances)
            {
                if (tree.prototypeIndex != prototype)
                {
                    kept.Add(tree);
                    continue;
                }

                spots.Add(new GatherableSpot(
                    origin + Vector3.Scale(tree.position, size),
                    Quaternion.Euler(0f, tree.rotation * Mathf.Rad2Deg, 0f),
                    new Vector3(tree.widthScale, tree.heightScale, tree.widthScale)));
            }

            var copy = Object.Instantiate(data);
            copy.name = data.name + " (play session)";
            copy.SetTreeInstances(kept.ToArray(), false);
            return copy;
        }

        /// <summary>
        /// Spots for sticks: one at the foot of each tree of the playable area, lying on
        /// bare ground between <paramref name="minTrunkDistance"/> and
        /// <paramref name="maxTrunkDistance"/> from the trunk. A tree with only grass or
        /// steep ground around it gets none.
        /// </summary>
        public static void FindStickSpots(
            Terrain terrain,
            IReadOnlyList<GameObject> treePrefabs,
            Rect playableArea,
            float minTrunkDistance,
            float maxTrunkDistance,
            float maxSlopeDegrees,
            System.Random random,
            List<GatherableSpot> spots)
        {
            var data = terrain.terrainData;
            var origin = terrain.GetPosition();
            var size = data.size;
            var isTree = new bool[data.treePrototypes.Length];
            foreach (var prefab in treePrefabs)
            {
                var prototype = PrototypeIndexOf(data, prefab);
                if (prototype >= 0)
                {
                    isTree[prototype] = true;
                }
            }

            var grass = new GrassMap(data);

            foreach (var tree in data.treeInstances)
            {
                if (!isTree[tree.prototypeIndex])
                {
                    continue;
                }

                var trunk = origin + Vector3.Scale(tree.position, size);

                for (var draw = 0; draw < DrawsPerTree; draw++)
                {
                    var angle = (float)random.NextDouble() * Mathf.PI * 2f;
                    var distance = Mathf.Lerp(minTrunkDistance, maxTrunkDistance, (float)random.NextDouble());
                    var yaw = (float)random.NextDouble() * 360f;

                    var point = trunk + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
                    if (!playableArea.Contains(new Vector2(point.x, point.z)))
                    {
                        continue;
                    }

                    var normalizedX = (point.x - origin.x) / size.x;
                    var normalizedZ = (point.z - origin.z) / size.z;
                    if (data.GetSteepness(normalizedX, normalizedZ) > maxSlopeDegrees || grass.Grows(normalizedX, normalizedZ))
                    {
                        continue;
                    }

                    point.y = terrain.SampleHeight(point) + origin.y;

                    // Lying on the slope, turned at random around it.
                    var normal = data.GetInterpolatedNormal(normalizedX, normalizedZ);
                    var rotation = Quaternion.FromToRotation(Vector3.up, normal) * Quaternion.Euler(0f, yaw, 0f);
                    spots.Add(new GatherableSpot(point, rotation, Vector3.one));
                    break;
                }
            }
        }

        private static int PrototypeIndexOf(TerrainData data, GameObject prefab)
        {
            var prototypes = data.treePrototypes;
            for (var index = 0; index < prototypes.Length; index++)
            {
                if (prototypes[index].prefab == prefab)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>Where the Terrain draws grass, read once from its detail layers.</summary>
        private sealed class GrassMap
        {
            private readonly bool[,] grows;
            private readonly int width;
            private readonly int height;

            public GrassMap(TerrainData data)
            {
                width = data.detailWidth;
                height = data.detailHeight;
                grows = new bool[height, width];

                for (var layer = 0; layer < data.detailPrototypes.Length; layer++)
                {
                    var density = data.GetDetailLayer(0, 0, width, height, layer);
                    for (var z = 0; z < height; z++)
                    {
                        for (var x = 0; x < width; x++)
                        {
                            grows[z, x] |= density[z, x] > 0;
                        }
                    }
                }
            }

            public bool Grows(float normalizedX, float normalizedZ)
            {
                var x = Mathf.Clamp(Mathf.FloorToInt(normalizedX * width), 0, width - 1);
                var z = Mathf.Clamp(Mathf.FloorToInt(normalizedZ * height), 0, height - 1);
                return grows[z, x];
            }
        }
    }
}
