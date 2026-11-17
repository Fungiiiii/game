using System.Collections.Generic;
using System.Linq;
using Fungiiiii.Runtime.World;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fungiiiii.Editor.World
{
    /// <summary>
    /// Writes a <see cref="TerrainRelief"/> into its Terrain: the heightmap, then the
    /// ground layers (forest floor, moss, rock, path soil). One undo step.
    /// </summary>
    internal static class TerrainReliefTool
    {
        /// <summary>Order of the Terrain's layers that the ground painting expects.</summary>
        internal const int FloorLayer = 0;
        internal const int MossLayer = 1;
        internal const int CliffLayer = 2;
        internal const int PathLayer = 3;

        /// <summary>Ground steeper than this, in degrees, is painted as rock.</summary>
        internal const float CliffPaintSlope = 44f;

        /// <summary>
        /// Edge of a painted area, in metres, on each side of its outline. Kept
        /// short: low poly reads with sharp edges, not soft gradients.
        /// </summary>
        private const float PaintEdge = 0.75f;

        /// <summary>Share of a path's width painted as path soil; the rest of the gap stays green.</summary>
        private const float TrailShare = 0.45f;

        /// <summary>
        /// Uses every ScatterExclusionZone and ScatterExclusionPath in the open scenes.
        /// Zones are painted as mossy clearings, and levelled when marked
        /// FlattenGround; paths are smoothed and painted as path soil. Grass patches
        /// get moss underneath.
        /// </summary>
        internal static void Apply(TerrainRelief relief)
        {
            Terrain terrain = relief.GetComponentInParent<Terrain>();
            ScatterExclusionZone[] zones = Object.FindObjectsByType<ScatterExclusionZone>(FindObjectsSortMode.InstanceID);
            List<ExclusionCircle> flats = zones.Where(zone => zone.FlattenGround).Select(zone => zone.ToCircle()).ToList();
            List<ExclusionCircle> clearings = zones.Select(zone => zone.ToCircle()).ToList();
            List<ExclusionCircle> paths = Object.FindObjectsByType<ScatterExclusionPath>(FindObjectsSortMode.InstanceID)
                .SelectMany(path => path.ToCircles())
                .ToList();
            Vector3 centre = relief.transform.position;
            Apply(terrain, relief.Settings, new Vector2(centre.x, centre.z), flats, clearings, paths,
                terrain.GetComponentInChildren<TerrainGrass>());
        }

        internal static void Apply(Terrain terrain, ReliefSettings settings, Vector2 playableCentre,
            IReadOnlyList<ExclusionCircle> flats, IReadOnlyList<ExclusionCircle> clearings,
            IReadOnlyList<ExclusionCircle> paths = null, TerrainGrass grass = null)
        {
            TerrainData data = terrain.terrainData;
            Vector3 origin = terrain.transform.position;
            int resolution = data.heightmapResolution;
            var grid = new ReliefGrid(resolution, new Vector2(origin.x, origin.z), data.size.x / (resolution - 1));

            float[,] metres = ReliefGenerator.Generate(settings, grid, playableCentre, flats, paths);
            var normalised = new float[resolution, resolution];
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    normalised[z, x] = Mathf.Clamp01((metres[z, x] - origin.y) / data.size.y);
                }
            }

            var undoTargets = new List<Object> { data };
            undoTargets.AddRange(data.alphamapTextures);
            Undo.RegisterCompleteObjectUndo(undoTargets.ToArray(), "Generate relief");
            data.SetHeights(0, 0, normalised);
            // The trail is the middle of the gap the path opens in the trees.
            List<ExclusionCircle> trail = paths?.Select(circle => new ExclusionCircle(
                new Vector3(circle.Center.x, 0f, circle.Center.y), circle.Radius * TrailShare)).ToList();
            PaintGround(terrain, clearings, trail, grass);
            EditorUtility.SetDirty(data);
        }

        private static void PaintGround(Terrain terrain, IReadOnlyList<ExclusionCircle> clearings,
            IReadOnlyList<ExclusionCircle> paths, TerrainGrass grass)
        {
            TerrainData data = terrain.terrainData;
            if (data.alphamapLayers <= PathLayer)
            {
                Debug.LogWarning($"[TerrainRelief] {terrain.name} needs 4 terrain layers (floor, moss, rock, path) to paint the ground.", terrain);
                return;
            }

            int resolution = data.alphamapResolution;
            Vector3 origin = terrain.transform.position;
            float pixel = data.size.x / resolution;
            float[,] clearing = Coverage(clearings, resolution, origin, pixel);
            float[,] path = Coverage(paths, resolution, origin, pixel);
            var maps = new float[resolution, resolution, data.alphamapLayers];

            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float nx = (x + 0.5f) / resolution;
                    float nz = (z + 0.5f) / resolution;
                    var position = new Vector2(origin.x + nx * data.size.x, origin.z + nz * data.size.z);

                    float rock = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(CliffPaintSlope - 1.5f, CliffPaintSlope, TerrainForestGenerator.Steepness(data, nx, nz)));
                    float soil = path[z, x] * (1f - rock);
                    float moss = grass != null ? GrassDensity.MossCover(position, grass.Seed, grass.PatchSize, grass.Coverage) : 0f;
                    moss = Mathf.Max(moss, clearing[z, x]) * (1f - rock) * (1f - soil);

                    maps[z, x, CliffLayer] = rock;
                    maps[z, x, PathLayer] = soil;
                    maps[z, x, MossLayer] = moss;
                    maps[z, x, FloorLayer] = Mathf.Max(0f, 1f - rock - soil - moss);
                }
            }

            data.SetAlphamaps(0, 0, maps);
        }

        /// <summary>
        /// How much each splat pixel is inside the circles, 0 to 1, with a short
        /// edge. Circles are rasterized one by one over their own bounds only.
        /// </summary>
        private static float[,] Coverage(IReadOnlyList<ExclusionCircle> circles, int resolution, Vector3 origin, float pixel)
        {
            var cover = new float[resolution, resolution];
            if (circles == null)
            {
                return cover;
            }

            foreach (ExclusionCircle circle in circles)
            {
                float outer = circle.Radius + PaintEdge;
                int x0 = Mathf.Max(0, Mathf.FloorToInt((circle.Center.x - outer - origin.x) / pixel));
                int x1 = Mathf.Min(resolution - 1, Mathf.CeilToInt((circle.Center.x + outer - origin.x) / pixel));
                int z0 = Mathf.Max(0, Mathf.FloorToInt((circle.Center.y - outer - origin.z) / pixel));
                int z1 = Mathf.Min(resolution - 1, Mathf.CeilToInt((circle.Center.y + outer - origin.z) / pixel));
                for (int z = z0; z <= z1; z++)
                {
                    for (int x = x0; x <= x1; x++)
                    {
                        var position = new Vector2(origin.x + (x + 0.5f) * pixel, origin.z + (z + 0.5f) * pixel);
                        float distance = Vector2.Distance(position, circle.Center);
                        float inside = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(circle.Radius - PaintEdge, outer, distance));
                        cover[z, x] = Mathf.Max(cover[z, x], inside);
                    }
                }
            }

            return cover;
        }
    }
}
