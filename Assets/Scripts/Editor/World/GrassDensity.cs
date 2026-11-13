using System;
using System.Collections.Generic;
using Fungiiiii.Runtime.World;
using UnityEngine;

namespace Fungiiiii.Editor.World
{
    /// <summary>
    /// How many grass tufts each Terrain detail cell holds. Plain C#, deterministic
    /// for a seed: patches come from a value noise, thicker at their heart, and
    /// clearings, paths and steep ground stay bare.
    /// </summary>
    internal static class GrassDensity
    {
        /// <summary>
        /// Densities indexed [z, x] like TerrainData.SetDetailLayer. Cell [0, 0]
        /// covers <paramref name="origin"/> to origin + cellSize.
        /// </summary>
        internal static int[,] Compute(int resolution, Vector2 origin, float cellSize, int seed, float patchSize,
            float coverage, int maxDensity, IReadOnlyList<ExclusionCircle> exclusions, Func<int, int, bool> tooSteep)
        {
            var density = new int[resolution, resolution];
            float threshold = Mathf.Lerp(0.75f, 0.25f, coverage);

            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    var centre = origin + new Vector2(x + 0.5f, z + 0.5f) * cellSize;
                    float noise = ValueNoise.Fbm(centre / patchSize, 2, seed);
                    if (noise < threshold || IsExcluded(centre, exclusions) || tooSteep(x, z))
                    {
                        continue;
                    }

                    // From the edge of a patch (threshold) to its heart (+0.08): a short
                    // sparse fringe, then thick grass.
                    float heart = Mathf.InverseLerp(threshold, threshold + 0.08f, noise);
                    density[z, x] = Mathf.Max(1, Mathf.RoundToInt(heart * maxDensity));
                }
            }

            return density;
        }

        /// <summary>
        /// Moss under a grass patch, 0 to 1: the same noise as the grass, a little
        /// wider, so every patch stands on green ground with a crisp edge.
        /// </summary>
        internal static float MossCover(Vector2 position, int seed, float patchSize, float coverage)
        {
            float threshold = Mathf.Lerp(0.75f, 0.25f, coverage);
            float noise = ValueNoise.Fbm(position / patchSize, 2, seed);
            return Mathf.InverseLerp(threshold - 0.05f, threshold - 0.03f, noise);
        }

        private static bool IsExcluded(Vector2 position, IReadOnlyList<ExclusionCircle> exclusions)
        {
            if (exclusions == null)
            {
                return false;
            }

            var world = new Vector3(position.x, 0f, position.y);
            for (int i = 0; i < exclusions.Count; i++)
            {
                if (exclusions[i].Contains(world))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
