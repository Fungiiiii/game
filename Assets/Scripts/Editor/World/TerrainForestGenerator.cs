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
    /// </summary>
    internal static class TerrainForestGenerator
    {
        /// <summary>
        /// Generates using every ScatterExclusionZone in the open scenes.
        /// </summary>
        internal static int Generate(Terrain terrain)
        {
            List<ExclusionCircle> exclusions = Object.FindObjectsByType<ScatterExclusionZone>(FindObjectsSortMode.None)
                .Select(zone => zone.ToCircle())
                .ToList();
            return Generate(terrain, exclusions);
        }

        internal static int Generate(Terrain terrain, IReadOnlyList<ExclusionCircle> exclusions)
        {
            TerrainData data = terrain.terrainData;
            ForestScatter[] layers = terrain.GetComponentsInChildren<ForestScatter>();
            Vector3 origin = terrain.transform.position;
            Vector3 size = data.size;

            var prototypes = new TreePrototype[layers.Length];
            var instances = new List<TreeInstance>();

            for (int index = 0; index < layers.Length; index++)
            {
                ForestScatter layer = layers[index];
                if (layer.Prefab == null)
                {
                    throw new InvalidOperationException($"Scatter layer '{layer.name}' has no prefab.");
                }

                prototypes[index] = new TreePrototype { prefab = layer.Prefab };

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
            }

            Undo.RegisterCompleteObjectUndo(data, "Generate forest");
            data.treePrototypes = prototypes;
            data.RefreshPrototypes();
            data.SetTreeInstances(instances.ToArray(), true);
            EditorUtility.SetDirty(data);
            return instances.Count;
        }

        internal static void Clear(Terrain terrain)
        {
            Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Clear forest");
            terrain.terrainData.SetTreeInstances(Array.Empty<TreeInstance>(), false);
            EditorUtility.SetDirty(terrain.terrainData);
        }
    }
}
