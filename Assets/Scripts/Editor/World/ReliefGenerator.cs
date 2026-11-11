using System;
using System.Collections.Generic;
using Fungiiiii.Runtime.World;
using UnityEngine;

namespace Fungiiiii.Editor.World
{
    /// <summary>
    /// Square grid of height samples laid on the ground: sample [0, 0] sits at
    /// <see cref="Origin"/>, samples are <see cref="CellSize"/> metres apart.
    /// </summary>
    internal readonly struct ReliefGrid
    {
        public ReliefGrid(int resolution, Vector2 origin, float cellSize)
        {
            Resolution = resolution;
            Origin = origin;
            CellSize = cellSize;
        }

        public int Resolution { get; }
        public Vector2 Origin { get; }
        public float CellSize { get; }

        public Vector2 PositionOf(int x, int z) => Origin + new Vector2(x, z) * CellSize;
    }

    /// <summary>
    /// Computes a low poly relief, in metres: hills cut into plateaus, cliffs past
    /// the playable edge, level clearings. Plain C#, deterministic for a seed.
    /// </summary>
    internal static class ReliefGenerator
    {
        private const int MaxSlopePasses = 2000;

        /// <summary>
        /// Heights in metres, indexed [z, x] like TerrainData.SetHeights.
        /// </summary>
        internal static float[,] Generate(ReliefSettings settings, ReliefGrid grid, Vector2 playableCentre, IReadOnlyList<ExclusionCircle> flatAreas)
        {
            int n = grid.Resolution;
            var heights = new float[n, n];
            var cliff = new bool[n, n];
            var fixedHeight = new bool[n, n];

            for (int z = 0; z < n; z++)
            {
                for (int x = 0; x < n; x++)
                {
                    Vector2 position = grid.PositionOf(x, z);
                    float border = BorderDistance(position, playableCentre, settings.PlayableSize);
                    heights[z, x] = TerracedHeight(settings, position);
                    if (border > 0f)
                    {
                        int tier = Mathf.Min(settings.CliffTiers, Mathf.FloorToInt(border / settings.CliffSpacing) + 1);
                        heights[z, x] += tier * settings.CliffHeight;
                        cliff[z, x] = true;
                    }
                }
            }

            Flatten(settings, grid, flatAreas, heights, cliff, fixedHeight);
            LimitSlope(settings, grid, heights, cliff, fixedHeight);
            return heights;
        }

        /// <summary>
        /// Largest step allowed between two neighbour samples. Limiting X and Z
        /// separately to tan(slope) / sqrt(2) keeps every triangle of the mesh
        /// under the slope, whatever way its quad is split.
        /// </summary>
        internal static float MaxStep(float cellSize, float maxSlopeDegrees)
        {
            return cellSize * Mathf.Tan(maxSlopeDegrees * Mathf.Deg2Rad) / Mathf.Sqrt(2f);
        }

        /// <summary>
        /// Distance outside the playable rectangle, in metres. Zero or less inside.
        /// </summary>
        internal static float BorderDistance(Vector2 position, Vector2 centre, Vector2 playableSize)
        {
            Vector2 offset = position - centre;
            return Mathf.Max(Mathf.Abs(offset.x) - playableSize.x * 0.5f, Mathf.Abs(offset.y) - playableSize.y * 0.5f);
        }

        private static float TerracedHeight(ReliefSettings settings, Vector2 position)
        {
            float noise = Fbm(position / settings.HillWavelength, settings.Octaves, settings.Seed);
            float height = settings.BaseHeight + settings.HillAmplitude * (noise * 2f - 1f);

            float steps = height / settings.TerraceStep;
            float floor = Mathf.Floor(steps);
            float within = steps - floor;
            float flatShare = 1f - settings.TerraceRamp;
            float rise = within < flatShare ? 0f : Mathf.SmoothStep(0f, 1f, (within - flatShare) / settings.TerraceRamp);
            return Mathf.Max(0f, (floor + rise) * settings.TerraceStep);
        }

        private static void Flatten(ReliefSettings settings, ReliefGrid grid, IReadOnlyList<ExclusionCircle> flatAreas,
            float[,] heights, bool[,] cliff, bool[,] fixedHeight)
        {
            if (flatAreas == null)
            {
                return;
            }

            int n = grid.Resolution;
            foreach (ExclusionCircle area in flatAreas)
            {
                float level = TerracedHeight(settings, area.Center);
                float outer = area.Radius + settings.FlattenBlend;
                for (int z = 0; z < n; z++)
                {
                    for (int x = 0; x < n; x++)
                    {
                        if (cliff[z, x])
                        {
                            continue;
                        }

                        float distance = Vector2.Distance(grid.PositionOf(x, z), area.Center);
                        if (distance <= area.Radius)
                        {
                            heights[z, x] = level;
                            fixedHeight[z, x] = true;
                        }
                        else if (distance < outer && !fixedHeight[z, x])
                        {
                            float t = Mathf.SmoothStep(0f, 1f, (distance - area.Radius) / settings.FlattenBlend);
                            heights[z, x] = Mathf.Lerp(level, heights[z, x], t);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Brings every walkable slope under the limit. The higher sample of a too
        /// steep pair comes down; next to a level clearing, the lower one comes up
        /// instead, so clearings stay level. Cliff samples are left alone.
        /// </summary>
        private static void LimitSlope(ReliefSettings settings, ReliefGrid grid, float[,] heights, bool[,] cliff, bool[,] fixedHeight)
        {
            int n = grid.Resolution;
            float maxStep = MaxStep(grid.CellSize, settings.MaxWalkableSlope);

            for (int pass = 0; pass < MaxSlopePasses; pass++)
            {
                bool changed = false;
                for (int z = 0; z < n; z++)
                {
                    for (int x = 0; x < n; x++)
                    {
                        if (cliff[z, x])
                        {
                            continue;
                        }

                        if (x + 1 < n && !cliff[z, x + 1])
                        {
                            changed |= Relax(heights, fixedHeight, z, x, z, x + 1, maxStep);
                        }

                        if (z + 1 < n && !cliff[z + 1, x])
                        {
                            changed |= Relax(heights, fixedHeight, z, x, z + 1, x, maxStep);
                        }
                    }
                }

                if (!changed)
                {
                    return;
                }
            }

            throw new InvalidOperationException($"Relief slopes did not settle after {MaxSlopePasses} passes.");
        }

        private static bool Relax(float[,] heights, bool[,] fixedHeight, int za, int xa, int zb, int xb, float maxStep)
        {
            float a = heights[za, xa];
            float b = heights[zb, xb];
            if (Mathf.Abs(a - b) <= maxStep * 1.0001f)
            {
                return false;
            }

            bool aIsHigher = a > b;
            int zh = aIsHigher ? za : zb, xh = aIsHigher ? xa : xb;
            int zl = aIsHigher ? zb : za, xl = aIsHigher ? xb : xa;

            if (!fixedHeight[zh, xh])
            {
                heights[zh, xh] = heights[zl, xl] + maxStep;
                return true;
            }

            if (!fixedHeight[zl, xl])
            {
                heights[zl, xl] = heights[zh, xh] - maxStep;
                return true;
            }

            return false;
        }

        private static float Fbm(Vector2 position, int octaves, int seed)
        {
            float sum = 0f;
            float weight = 0f;
            float amplitude = 1f;
            float frequency = 1f;
            for (int octave = 0; octave < octaves; octave++)
            {
                sum += ValueNoise(position * frequency, unchecked(seed + octave * 7919)) * amplitude;
                weight += amplitude;
                amplitude *= 0.5f;
                frequency *= 2f;
            }

            return sum / weight;
        }

        private static float ValueNoise(Vector2 position, int seed)
        {
            int x0 = Mathf.FloorToInt(position.x);
            int z0 = Mathf.FloorToInt(position.y);
            float tx = Mathf.SmoothStep(0f, 1f, position.x - x0);
            float tz = Mathf.SmoothStep(0f, 1f, position.y - z0);

            float bottom = Mathf.Lerp(Lattice(x0, z0, seed), Lattice(x0 + 1, z0, seed), tx);
            float top = Mathf.Lerp(Lattice(x0, z0 + 1, seed), Lattice(x0 + 1, z0 + 1, seed), tx);
            return Mathf.Lerp(bottom, top, tz);
        }

        /// <summary>Integer hash to [0, 1]: the same on every machine.</summary>
        private static float Lattice(int x, int z, int seed)
        {
            unchecked
            {
                uint h = (uint)x * 374761393u + (uint)z * 668265263u + (uint)seed * 2246822519u;
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFFu) / 16777215f;
            }
        }
    }
}
