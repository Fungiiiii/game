using System;
using System.Collections.Generic;
using Fungiiiii.Runtime.World;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Fungiiiii.Gathering
{
    /// <summary>
    /// Reads from the forest's Terrain where resources may appear: the decor mushrooms
    /// scattered over it, the foot of its trees and a scatter over the playable area become
    /// the spots of the gatherable mushrooms, and the foot of its trees the spots of the
    /// sticks.
    /// </summary>
    internal static class ForestResourceSpots
    {
        /// <summary>Directions tried around a trunk before the tree is given up.</summary>
        private const int DrawsPerTree = 6;

        /// <summary>The forest's group of hand-placed objects.</summary>
        private const string LandmarksName = "Landmarks";

        /// <summary>A stick falls from this share of its tree's height: from within the leaves.</summary>
        private const float StickDropShareOfCrown = 0.6f;

        // Something small standing on the ground is in the way if this capsule, above the
        // ground, touches a trunk or a rock. Up to a 30° slope it does not touch the ground itself.
        private const float ObstacleCapsuleBottom = 0.9f;
        private const float ObstacleCapsuleTop = 2f;
        private const float ObstacleCapsuleRadius = 0.45f;

        /// <summary>Whether a mushroom or a plant standing here would go through a trunk or a rock. Triggers are ignored.</summary>
        public static bool TouchesTrunkOrRock(Vector3 groundPoint)
        {
            return Physics.CheckCapsule(
                groundPoint + Vector3.up * ObstacleCapsuleBottom,
                groundPoint + Vector3.up * ObstacleCapsuleTop,
                ObstacleCapsuleRadius,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// Hides the forest's hand-placed objects whose name starts with
        /// <paramref name="namePrefix"/>, in its group of landmarks: decor that must not stay,
        /// such as a mushroom that cannot be gathered. Their poses are added to
        /// <paramref name="taken"/> when it is given.
        /// </summary>
        public static void TakeLandmarks(Scene forest, string namePrefix, List<GatherableSpot> taken = null)
        {
            foreach (var root in forest.GetRootGameObjects())
            {
                if (root.name != LandmarksName)
                {
                    continue;
                }

                foreach (Transform child in root.transform)
                {
                    if (child.name.StartsWith(namePrefix, System.StringComparison.Ordinal))
                    {
                        taken?.Add(new GatherableSpot(child.position, child.rotation, child.lossyScale));
                        child.gameObject.SetActive(false);
                    }
                }
            }
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
            FindTreeFootSpots(
                terrain,
                treePrefabs,
                playableArea,
                minTrunkDistance,
                maxTrunkDistance,
                maxSlopeDegrees,
                true,
                random,
                spots,
                (point, normal, yaw, crownHeight) =>
                {
                    // Lying on the slope, turned at random around it; it falls from the leaves.
                    var rotation = Quaternion.FromToRotation(Vector3.up, normal) * Quaternion.Euler(0f, yaw, 0f);
                    return new GatherableSpot(point, rotation, Vector3.one, crownHeight * StickDropShareOfCrown);
                });
        }

        /// <summary>
        /// Spots for spore mushrooms: one at the foot of each tree of the playable area.
        /// Grass is allowed, unlike for sticks: a mushroom stands taller than the grass.
        /// </summary>
        public static void FindTreeFootMushroomSpots(
            Terrain terrain,
            IReadOnlyList<GameObject> treePrefabs,
            Rect playableArea,
            float minTrunkDistance,
            float maxTrunkDistance,
            float maxSlopeDegrees,
            MushroomPose pose,
            System.Random random,
            List<GatherableSpot> spots)
        {
            FindTreeFootSpots(
                terrain,
                treePrefabs,
                playableArea,
                minTrunkDistance,
                maxTrunkDistance,
                maxSlopeDegrees,
                false,
                random,
                spots,
                (point, _, yaw, _) => pose.At(point, yaw, random));
        }

        /// <summary>
        /// Spots for spore mushrooms scattered over the playable area, about
        /// <paramref name="spacing"/> apart, on gentle ground and out of the paths.
        /// </summary>
        public static void ScatterMushroomSpots(
            Terrain terrain,
            Rect playableArea,
            float spacing,
            float maxSlopeDegrees,
            IReadOnlyList<ExclusionCircle> paths,
            MushroomPose pose,
            System.Random random,
            List<GatherableSpot> spots)
        {
            var data = terrain.terrainData;
            var origin = terrain.GetPosition();
            var size = data.size;

            foreach (var sample in PoissonDiskSampler.Sample(playableArea.size, spacing, random.Next()))
            {
                var point = new Vector3(playableArea.xMin + sample.x, 0f, playableArea.yMin + sample.y);
                var normalizedX = (point.x - origin.x) / size.x;
                var normalizedZ = (point.z - origin.z) / size.z;
                if (data.GetSteepness(normalizedX, normalizedZ) > maxSlopeDegrees || IsOnPath(point, paths))
                {
                    continue;
                }

                point.y = terrain.SampleHeight(point) + origin.y;
                spots.Add(pose.At(point, (float)random.NextDouble() * 360f, random));
            }
        }

        /// <summary>Height of the top of a model above its pivot, in metres, from its meshes.</summary>
        private static float TopOf(GameObject prefab)
        {
            var top = 0f;
            foreach (var meshFilter in prefab.GetComponentsInChildren<MeshFilter>(true))
            {
                var mesh = meshFilter.sharedMesh;
                if (mesh == null)
                {
                    continue;
                }

                // A prefab asset sits at the origin: its transforms give heights above the pivot.
                var bounds = mesh.bounds;
                var toPrefab = meshFilter.transform.localToWorldMatrix;
                for (var corner = 0; corner < 8; corner++)
                {
                    var local = new Vector3(
                        (corner & 1) == 0 ? bounds.min.x : bounds.max.x,
                        (corner & 2) == 0 ? bounds.min.y : bounds.max.y,
                        (corner & 4) == 0 ? bounds.min.z : bounds.max.z);
                    top = Mathf.Max(top, toPrefab.MultiplyPoint3x4(local).y);
                }
            }

            return top;
        }

        /// <summary>
        /// Removes the mushroom spots that would overlap something: a stick spot closer than
        /// <paramref name="stickClearance"/>, a mushroom spot kept before it closer than
        /// <paramref name="mushroomClearance"/>, or a solid obstacle. Earlier spots win, so
        /// the forest's own decor mushrooms, listed first, are the ones kept.
        /// </summary>
        public static void RemoveOverlaps(
            List<GatherableSpot> mushrooms,
            IReadOnlyList<GatherableSpot> sticks,
            float stickClearance,
            float mushroomClearance,
            Func<Vector3, bool> isBlocked)
        {
            var stickGrid = new SpotGrid(stickClearance);
            foreach (var stick in sticks)
            {
                stickGrid.Add(stick.Position);
            }

            var keptGrid = new SpotGrid(mushroomClearance);
            var kept = 0;
            for (var index = 0; index < mushrooms.Count; index++)
            {
                var mushroom = mushrooms[index];
                var point = mushroom.Position;
                if (stickGrid.HasWithin(point, stickClearance) || keptGrid.HasWithin(point, mushroomClearance) || isBlocked(point))
                {
                    continue;
                }

                keptGrid.Add(point);
                mushrooms[kept++] = mushroom;
            }

            mushrooms.RemoveRange(kept, mushrooms.Count - kept);
        }

        private static void FindTreeFootSpots(
            Terrain terrain,
            IReadOnlyList<GameObject> treePrefabs,
            Rect playableArea,
            float minTrunkDistance,
            float maxTrunkDistance,
            float maxSlopeDegrees,
            bool bareGroundOnly,
            System.Random random,
            List<GatherableSpot> spots,
            Func<Vector3, Vector3, float, float, GatherableSpot> poseAt)
        {
            var data = terrain.terrainData;
            var origin = terrain.GetPosition();
            var size = data.size;
            var isTree = new bool[data.treePrototypes.Length];
            var crownHeights = new float[data.treePrototypes.Length];
            foreach (var prefab in treePrefabs)
            {
                var prototype = PrototypeIndexOf(data, prefab);
                if (prototype >= 0)
                {
                    isTree[prototype] = true;
                    crownHeights[prototype] = TopOf(prefab);
                }
            }

            var grass = bareGroundOnly ? new GrassMap(data) : null;

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
                    if (data.GetSteepness(normalizedX, normalizedZ) > maxSlopeDegrees ||
                        (grass != null && grass.Grows(normalizedX, normalizedZ)))
                    {
                        continue;
                    }

                    point.y = terrain.SampleHeight(point) + origin.y;
                    var crownHeight = crownHeights[tree.prototypeIndex] * tree.heightScale;
                    spots.Add(poseAt(point, data.GetInterpolatedNormal(normalizedX, normalizedZ), yaw, crownHeight));
                    break;
                }
            }
        }

        private static bool IsOnPath(Vector3 point, IReadOnlyList<ExclusionCircle> paths)
        {
            foreach (var circle in paths)
            {
                if (circle.Contains(point))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Index of <paramref name="prefab"/> among the Terrain's tree prototypes, or -1.</summary>
        internal static int PrototypeIndexOf(TerrainData data, GameObject prefab)
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

        /// <summary>Points sorted into square cells, to find a close one without testing them all.</summary>
        private sealed class SpotGrid
        {
            private readonly Dictionary<Vector2Int, List<Vector3>> cells = new Dictionary<Vector2Int, List<Vector3>>();
            private readonly float cellSize;

            public SpotGrid(float cellSize)
            {
                this.cellSize = Mathf.Max(cellSize, 0.01f);
            }

            public void Add(Vector3 point)
            {
                var cell = CellOf(point);
                if (!cells.TryGetValue(cell, out var points))
                {
                    points = new List<Vector3>();
                    cells.Add(cell, points);
                }

                points.Add(point);
            }

            /// <summary>Whether a point closer than <paramref name="distance"/>, at most the cell size, was added. Measured on the ground.</summary>
            public bool HasWithin(Vector3 point, float distance)
            {
                var centre = CellOf(point);
                var squared = distance * distance;
                for (var x = -1; x <= 1; x++)
                {
                    for (var z = -1; z <= 1; z++)
                    {
                        if (!cells.TryGetValue(new Vector2Int(centre.x + x, centre.y + z), out var points))
                        {
                            continue;
                        }

                        foreach (var other in points)
                        {
                            var flat = other - point;
                            flat.y = 0f;
                            if (flat.sqrMagnitude < squared)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            private Vector2Int CellOf(Vector3 point)
            {
                return new Vector2Int(Mathf.FloorToInt(point.x / cellSize), Mathf.FloorToInt(point.z / cellSize));
            }
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

    /// <summary>How a spore mushroom stands on its spot: upright, at a random size, slightly sunk.</summary>
    internal readonly struct MushroomPose
    {
        public MushroomPose(float minScale, float maxScale, float sink)
        {
            MinScale = minScale;
            MaxScale = maxScale;
            Sink = sink;
        }

        public float MinScale { get; }

        public float MaxScale { get; }

        /// <summary>How deep it goes into the ground, in metres, so it never floats on a slope.</summary>
        public float Sink { get; }

        public GatherableSpot At(Vector3 groundPoint, float yawDegrees, System.Random random)
        {
            var scale = Mathf.Lerp(MinScale, MaxScale, (float)random.NextDouble());
            return new GatherableSpot(
                groundPoint + Vector3.down * Sink,
                Quaternion.Euler(0f, yawDegrees, 0f),
                Vector3.one * scale);
        }
    }
}
