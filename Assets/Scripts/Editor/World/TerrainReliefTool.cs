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
    /// ground layers (forest floor, clearings, cliffs). One undo step.
    /// </summary>
    internal static class TerrainReliefTool
    {
        /// <summary>Order of the Terrain's layers that the ground painting expects.</summary>
        internal const int FloorLayer = 0;
        internal const int ClearingLayer = 1;
        internal const int CliffLayer = 2;

        /// <summary>Ground steeper than this, in degrees, is painted as rock.</summary>
        internal const float CliffPaintSlope = 44f;

        /// <summary>Soft edge of a painted clearing, in metres, on each side of its circle.</summary>
        private const float ClearingEdge = 2.5f;

        /// <summary>
        /// Uses every ScatterExclusionZone and ScatterExclusionPath in the open scenes:
        /// all of them are painted as clearings, the zones marked FlattenGround are
        /// also levelled.
        /// </summary>
        internal static void Apply(TerrainRelief relief)
        {
            Terrain terrain = relief.GetComponentInParent<Terrain>();
            ScatterExclusionZone[] zones = Object.FindObjectsByType<ScatterExclusionZone>(FindObjectsSortMode.InstanceID);
            List<ExclusionCircle> flats = zones.Where(zone => zone.FlattenGround).Select(zone => zone.ToCircle()).ToList();
            List<ExclusionCircle> clearings = TerrainForestGenerator.FindExclusions();
            Vector3 centre = relief.transform.position;
            Apply(terrain, relief.Settings, new Vector2(centre.x, centre.z), flats, clearings);
        }

        internal static void Apply(Terrain terrain, ReliefSettings settings, Vector2 playableCentre,
            IReadOnlyList<ExclusionCircle> flats, IReadOnlyList<ExclusionCircle> clearings)
        {
            TerrainData data = terrain.terrainData;
            Vector3 origin = terrain.transform.position;
            int resolution = data.heightmapResolution;
            var grid = new ReliefGrid(resolution, new Vector2(origin.x, origin.z), data.size.x / (resolution - 1));

            float[,] metres = ReliefGenerator.Generate(settings, grid, playableCentre, flats);
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
            PaintGround(terrain, clearings);
            EditorUtility.SetDirty(data);
        }

        private static void PaintGround(Terrain terrain, IReadOnlyList<ExclusionCircle> clearings)
        {
            TerrainData data = terrain.terrainData;
            if (data.alphamapLayers <= CliffLayer)
            {
                Debug.LogWarning($"[TerrainRelief] {terrain.name} needs 3 terrain layers (floor, clearing, cliff) to paint the ground.", terrain);
                return;
            }

            int resolution = data.alphamapResolution;
            var maps = new float[resolution, resolution, data.alphamapLayers];
            Vector3 origin = terrain.transform.position;

            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float nx = (x + 0.5f) / resolution;
                    float nz = (z + 0.5f) / resolution;
                    var position = new Vector2(origin.x + nx * data.size.x, origin.z + nz * data.size.z);

                    float cliff = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(CliffPaintSlope - 6f, CliffPaintSlope, TerrainForestGenerator.Steepness(data, nx, nz)));
                    float clearing = 0f;
                    foreach (ExclusionCircle circle in clearings)
                    {
                        float distance = Vector2.Distance(position, circle.Center);
                        float inside = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(circle.Radius - ClearingEdge, circle.Radius + ClearingEdge, distance));
                        clearing = Mathf.Max(clearing, inside);
                    }

                    clearing *= 1f - cliff;
                    maps[z, x, CliffLayer] = cliff;
                    maps[z, x, ClearingLayer] = clearing;
                    maps[z, x, FloorLayer] = 1f - cliff - clearing;
                }
            }

            data.SetAlphamaps(0, 0, maps);
        }
    }
}
