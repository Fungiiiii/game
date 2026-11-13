using System;
using System.Collections.Generic;
using System.Linq;
using Fungiiiii.Runtime.World;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fungiiiii.Editor.World
{
    /// <summary>
    /// Writes a forest into a Terrain's trees from the ForestScatter layers placed
    /// under it. Each layer becomes one tree prototype. A Terrain holds a single list
    /// of tree instances, so the whole forest is rewritten at once, as one undo step.
    ///
    /// Layers are written in hierarchy order and the first one wins: an instance
    /// closer than <see cref="CrossLayerSpacing"/> to an instance of an earlier
    /// layer is dropped, so two models never grow into each other. Every instance
    /// is sunk into the ground by what its foot needs on a slope, so no side of
    /// it floats.
    /// </summary>
    internal static class TerrainForestGenerator
    {
        /// <summary>
        /// Generates using every ScatterExclusionZone and ScatterExclusionPath in the open scenes.
        /// </summary>
        internal static int Generate(Terrain terrain)
        {
            return Generate(terrain, FindExclusions());
        }

        /// <summary>
        /// Every exclusion circle of the open scenes: the zones, then the paths cut
        /// into circles.
        /// </summary>
        internal static List<ExclusionCircle> FindExclusions()
        {
            List<ExclusionCircle> exclusions = Object.FindObjectsByType<ScatterExclusionZone>(FindObjectsSortMode.InstanceID)
                .Select(zone => zone.ToCircle())
                .ToList();
            foreach (ScatterExclusionPath path in Object.FindObjectsByType<ScatterExclusionPath>(FindObjectsSortMode.InstanceID))
            {
                exclusions.AddRange(path.ToCircles());
            }

            return exclusions;
        }

        internal static int Generate(Terrain terrain, IReadOnlyList<ExclusionCircle> exclusions)
        {
            TerrainData data = terrain.terrainData;
            ForestScatter[] layers = terrain.GetComponentsInChildren<ForestScatter>();
            Vector3 origin = terrain.transform.position;
            Vector3 size = data.size;

            var prototypes = new TreePrototype[layers.Length];
            var instances = new List<TreeInstance>();
            var placed = new SpacingGrid();

            for (int index = 0; index < layers.Length; index++)
            {
                ForestScatter layer = layers[index];
                if (layer.Prefab == null)
                {
                    throw new InvalidOperationException($"Scatter layer '{layer.name}' has no prefab.");
                }

                prototypes[index] = new TreePrototype { prefab = layer.Prefab };
                float footRadius = FootRadius(layer.Prefab);
                var accepted = new List<Vector2>();

                foreach (ForestScatter.Placement placement in layer.ComputePlacements(exclusions))
                {
                    // Tree positions are normalised to the terrain: 0 to 1 on X and Z.
                    var normalised = new Vector3(
                        (placement.Position.x - origin.x) / size.x,
                        0f,
                        (placement.Position.z - origin.z) / size.z);

                    if (normalised.x < 0f || normalised.x > 1f || normalised.z < 0f || normalised.z > 1f)
                    {
                        continue;
                    }

                    // Cliff faces stay bare: a tree there floats or sinks on one side.
                    float steepness = Steepness(data, normalised.x, normalised.z);
                    if (layer.MaxSlope < 90f && steepness > layer.MaxSlope)
                    {
                        continue;
                    }

                    var ground = new Vector2(placement.Position.x, placement.Position.z);
                    if (placed.HasNeighbour(ground, layer.MinDistance))
                    {
                        continue;
                    }

                    accepted.Add(ground);
                    float sink = SinkDepth(footRadius * placement.Scale, steepness);
                    normalised.y = Mathf.Max(0f, (data.GetInterpolatedHeight(normalised.x, normalised.z) - sink) / size.y);

                    instances.Add(new TreeInstance
                    {
                        position = normalised,
                        widthScale = placement.Scale,
                        heightScale = placement.Scale,
                        rotation = placement.Yaw * Mathf.Deg2Rad,
                        color = Color.white,
                        lightmapColor = Color.white,
                        prototypeIndex = index,
                    });
                }

                // Added after the layer, so a layer keeps its own Poisson spacing.
                foreach (Vector2 ground in accepted)
                {
                    placed.Add(ground, layer.MinDistance);
                }
            }

            Undo.RegisterCompleteObjectUndo(data, "Generate forest");
            data.treePrototypes = prototypes;
            data.RefreshPrototypes();
            data.SetTreeInstances(instances.ToArray(), false);

            TerrainGrass grass = terrain.GetComponentInChildren<TerrainGrass>();
            if (grass != null)
            {
                WriteGrass(terrain, grass, exclusions);
            }

            EditorUtility.SetDirty(data);
            return instances.Count;
        }

        /// <summary>
        /// Writes the grass as the Terrain's single detail layer. Call it inside an
        /// undo record of the TerrainData.
        /// </summary>
        private static void WriteGrass(Terrain terrain, TerrainGrass grass, IReadOnlyList<ExclusionCircle> exclusions)
        {
            if (grass.Tuft == null)
            {
                throw new InvalidOperationException($"Grass '{grass.name}' has no tuft model.");
            }

            TerrainData data = terrain.terrainData;
            data.detailPrototypes = new[]
            {
                new DetailPrototype
                {
                    prototype = grass.Tuft,
                    usePrototypeMesh = true,
                    useInstancing = true,
                    renderMode = DetailRenderMode.VertexLit,
                    minWidth = 0.9f,
                    maxWidth = 1.3f,
                    minHeight = 0.7f,
                    maxHeight = 1.3f,
                    noiseSpread = 0.3f,
                    // Tufts vary between fresh and dry, so the patches do not read as one carpet.
                    healthyColor = new Color(1f, 1f, 0.92f),
                    dryColor = new Color(0.82f, 0.78f, 0.55f),
                },
            };

            // The default coverage mode reads each value as a share of 255: 1 to 6
            // would draw almost nothing. Our values are tuft counts per cell.
            data.SetDetailScatterMode(DetailScatterMode.InstanceCountMode);

            int resolution = data.detailResolution;
            Vector3 origin = terrain.transform.position;
            float cellSize = data.size.x / resolution;
            int[,] density = GrassDensity.Compute(resolution, new Vector2(origin.x, origin.z), cellSize, grass.Seed,
                grass.PatchSize, grass.Coverage, grass.MaxDensity, exclusions,
                (x, z) => Steepness(data, (x + 0.5f) / resolution, (z + 0.5f) / resolution) > grass.MaxSlope);
            data.SetDetailLayer(0, 0, 0, density);
        }

        /// <summary>
        /// Slope of the ground in degrees at a normalised position, measured on the
        /// heights themselves. TerrainData.GetSteepness can return a stale value right
        /// after SetHeights in edit mode, which the relief tool does just before.
        /// </summary>
        internal static float Steepness(TerrainData data, float x, float z)
        {
            float step = 1f / (data.heightmapResolution - 1);
            float x0 = Mathf.Max(0f, x - step), x1 = Mathf.Min(1f, x + step);
            float z0 = Mathf.Max(0f, z - step), z1 = Mathf.Min(1f, z + step);
            float slopeX = (data.GetInterpolatedHeight(x1, z) - data.GetInterpolatedHeight(x0, z)) / ((x1 - x0) * data.size.x);
            float slopeZ = (data.GetInterpolatedHeight(x, z1) - data.GetInterpolatedHeight(x, z0)) / ((z1 - z0) * data.size.z);
            return Mathf.Atan(Mathf.Sqrt(slopeX * slopeX + slopeZ * slopeZ)) * Mathf.Rad2Deg;
        }

        /// <summary>Always sunk this much, so the foot never shows a seam with the ground.</summary>
        internal const float BaseSink = 0.05f;

        /// <summary>
        /// Two instances of different layers stay at least this far apart: half the
        /// smaller of the two layers' own minimum distance.
        /// </summary>
        internal static float CrossLayerSpacing(float minDistanceA, float minDistanceB)
        {
            return Mathf.Min(minDistanceA, minDistanceB) * 0.5f;
        }

        /// <summary>
        /// How deep the foot of a model must go so that its downhill edge still
        /// touches the ground on a slope of <paramref name="steepness"/> degrees.
        /// </summary>
        internal static float SinkDepth(float footRadius, float steepness)
        {
            return footRadius * Mathf.Tan(Mathf.Min(steepness, 60f) * Mathf.Deg2Rad) + BaseSink;
        }

        /// <summary>
        /// Radius of the model's foot: its trunk capsule when it has one, otherwise
        /// the half width of its renderers.
        /// </summary>
        internal static float FootRadius(GameObject prefab)
        {
            if (prefab.TryGetComponent(out CapsuleCollider trunk))
            {
                return trunk.radius;
            }

            // Renderer.bounds is empty on a prefab asset: measure the meshes instead.
            Bounds? bounds = null;
            foreach (MeshFilter filter in prefab.GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh == null)
                {
                    continue;
                }

                Bounds local = filter.sharedMesh.bounds;
                Matrix4x4 toRoot = prefab.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                for (int corner = 0; corner < 8; corner++)
                {
                    var offset = new Vector3(
                        (corner & 1) == 0 ? -local.extents.x : local.extents.x,
                        (corner & 2) == 0 ? -local.extents.y : local.extents.y,
                        (corner & 4) == 0 ? -local.extents.z : local.extents.z);
                    Vector3 point = toRoot.MultiplyPoint3x4(local.center + offset);
                    if (bounds.HasValue)
                    {
                        Bounds grown = bounds.Value;
                        grown.Encapsulate(point);
                        bounds = grown;
                    }
                    else
                    {
                        bounds = new Bounds(point, Vector3.zero);
                    }
                }
            }

            return bounds.HasValue ? Mathf.Min(bounds.Value.extents.x, bounds.Value.extents.z) : 0f;
        }

        /// <summary>Accepted instances, bucketed by 16 m cells for a fast neighbour test.</summary>
        private sealed class SpacingGrid
        {
            private const float CellSize = 16f;

            private readonly Dictionary<Vector2Int, List<(Vector2 position, float minDistance)>> cells =
                new Dictionary<Vector2Int, List<(Vector2 position, float minDistance)>>();

            public void Add(Vector2 position, float minDistance)
            {
                Vector2Int cell = CellOf(position);
                if (!cells.TryGetValue(cell, out List<(Vector2 position, float minDistance)> list))
                {
                    list = new List<(Vector2 position, float minDistance)>();
                    cells[cell] = list;
                }

                list.Add((position, minDistance));
            }

            public bool HasNeighbour(Vector2 position, float minDistance)
            {
                Vector2Int centre = CellOf(position);
                for (int dz = -1; dz <= 1; dz++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (!cells.TryGetValue(centre + new Vector2Int(dx, dz), out List<(Vector2 position, float minDistance)> list))
                        {
                            continue;
                        }

                        foreach ((Vector2 other, float otherDistance) in list)
                        {
                            float spacing = CrossLayerSpacing(minDistance, otherDistance);
                            if ((other - position).sqrMagnitude < spacing * spacing)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            private static Vector2Int CellOf(Vector2 position)
            {
                return new Vector2Int(Mathf.FloorToInt(position.x / CellSize), Mathf.FloorToInt(position.y / CellSize));
            }
        }

        internal static void Clear(Terrain terrain)
        {
            Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Clear forest");
            terrain.terrainData.SetTreeInstances(Array.Empty<TreeInstance>(), false);
            if (terrain.terrainData.detailPrototypes.Length > 0)
            {
                int resolution = terrain.terrainData.detailResolution;
                terrain.terrainData.SetDetailLayer(0, 0, 0, new int[resolution, resolution]);
            }
            EditorUtility.SetDirty(terrain.terrainData);
        }
    }
}
