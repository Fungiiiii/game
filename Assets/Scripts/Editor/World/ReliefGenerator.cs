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
        private const int MaxSlopePasses = 500;

        /// <summary>
        /// Heights in metres, indexed [z, x] like TerrainData.SetHeights.
        /// </summary>
        internal static float[,] Generate(ReliefSettings settings, ReliefGrid grid, Vector2 playableCentre,
            IReadOnlyList<ExclusionCircle> flatAreas, IReadOnlyList<ExclusionCircle> paths = null)
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
                    float border = CliffDistance(settings, position, playableCentre);
                    heights[z, x] = TerracedHeight(settings, position);
                    if (border > 0f)
                    {
                        int tier = Mathf.Min(settings.CliffTiers, Mathf.FloorToInt(border / settings.CliffSpacing) + 1);
                        for (int t = 0; t < tier; t++)
                        {
                            // Each tier a little higher or lower, so the ledges never line up.
                            float variation = ValueNoise.Fbm(position / 60f, 2, unchecked(settings.Seed + 211 + t * 13));
                            heights[z, x] += settings.CliffHeight * Mathf.Lerp(0.75f, 1.25f, variation);
                        }

                        heights[z, x] += (ValueNoise.Lattice(x, z, unchecked(settings.Seed + 307)) - 0.5f) * 2f * settings.CliffRoughness;
                        cliff[z, x] = true;
                    }
                    else
                    {
                        // A gentle noise, not a random value per sample: small facets, no rubble.
                        heights[z, x] += (ValueNoise.Fbm(position / 9f, 2, unchecked(settings.Seed + 401)) - 0.5f) * 2f * settings.GroundRoughness;
                    }
                }
            }

            SmoothPaths(grid, paths, heights, cliff);
            Flatten(settings, grid, flatAreas, heights, cliff, fixedHeight);
            LimitSlope(settings, grid, heights, cliff, fixedHeight);
            return heights;
        }

        /// <summary>Half width of the averaging window along a path, in samples.</summary>
        private const int PathSmoothing = 3;

        /// <summary>
        /// A path is walked ground: its bumps are averaged away, a little wider than
        /// the path itself so its edges blend into the ground around.
        /// </summary>
        private static void SmoothPaths(ReliefGrid grid, IReadOnlyList<ExclusionCircle> paths, float[,] heights, bool[,] cliff)
        {
            if (paths == null || paths.Count == 0)
            {
                return;
            }

            int n = grid.Resolution;
            var onPath = new bool[n, n];
            foreach (ExclusionCircle circle in paths)
            {
                float reach = circle.Radius + grid.CellSize * 2f;
                Vector2 local = (circle.Center - grid.Origin) / grid.CellSize;
                int span = Mathf.CeilToInt(reach / grid.CellSize);
                for (int z = Mathf.Max(0, (int)local.y - span); z <= Mathf.Min(n - 1, (int)local.y + span); z++)
                {
                    for (int x = Mathf.Max(0, (int)local.x - span); x <= Mathf.Min(n - 1, (int)local.x + span); x++)
                    {
                        if (!cliff[z, x] && Vector2.Distance(grid.PositionOf(x, z), circle.Center) <= reach)
                        {
                            onPath[z, x] = true;
                        }
                    }
                }
            }

            for (int pass = 0; pass < 2; pass++)
            {
                var source = (float[,])heights.Clone();
                for (int z = 0; z < n; z++)
                {
                    for (int x = 0; x < n; x++)
                    {
                        if (!onPath[z, x])
                        {
                            continue;
                        }

                        float sum = 0f;
                        int count = 0;
                        for (int dz = -PathSmoothing; dz <= PathSmoothing; dz++)
                        {
                            for (int dx = -PathSmoothing; dx <= PathSmoothing; dx++)
                            {
                                int zz = z + dz, xx = x + dx;
                                if (zz >= 0 && xx >= 0 && zz < n && xx < n && !cliff[zz, xx])
                                {
                                    sum += source[zz, xx];
                                    count++;
                                }
                            }
                        }

                        heights[z, x] = sum / count;
                    }
                }
            }
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
        /// Distance past the cliff line, in metres; zero or less on walkable ground.
        /// The cliff line is the playable edge, wandering in and out by up to
        /// CliffWobble so the border never reads as a straight wall.
        /// </summary>
        internal static float CliffDistance(ReliefSettings settings, Vector2 position, Vector2 playableCentre)
        {
            float wobble = (ValueNoise.Fbm(position / 45f, 2, unchecked(settings.Seed + 101)) - 0.5f) * 2f * settings.CliffWobble;
            return BorderDistance(position, playableCentre, settings.PlayableSize) + wobble;
        }

        /// <summary>
        /// Distance outside the playable rectangle, in metres. Zero or less inside.
        /// </summary>
        internal static float BorderDistance(Vector2 position, Vector2 centre, Vector2 playableSize)
        {
            Vector2 offset = position - centre;
            return Mathf.Max(Mathf.Abs(offset.x) - playableSize.x * 0.5f, Mathf.Abs(offset.y) - playableSize.y * 0.5f);
        }

        internal static float TerracedHeight(ReliefSettings settings, Vector2 position)
        {
            float noise = ValueNoise.Fbm(position / settings.HillWavelength, settings.Octaves, settings.Seed);
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
        /// Brings every walkable slope under the limit; cliff samples are left alone.
        ///
        /// Two steps, both monotone, so they always settle. First a floor is spread
        /// from the level clearings: no sample may sit more than one step per cell
        /// below a clearing, or the clearing's edge would be too steep. Then the
        /// ground only comes down, never under that floor, until no neighbour is
        /// more than one step lower. Clearings never move.
        /// </summary>
        private static void LimitSlope(ReliefSettings settings, ReliefGrid grid, float[,] heights, bool[,] cliff, bool[,] fixedHeight)
        {
            int n = grid.Resolution;
            float maxStep = MaxStep(grid.CellSize, settings.MaxWalkableSlope);

            var floor = new float[n, n];
            for (int z = 0; z < n; z++)
            {
                for (int x = 0; x < n; x++)
                {
                    floor[z, x] = fixedHeight[z, x] ? heights[z, x] : float.NegativeInfinity;
                }
            }

            Sweep(n, cliff, (z, x, zn, xn) =>
            {
                float candidate = floor[zn, xn] - maxStep;
                if (candidate <= floor[z, x] + Tolerance)
                {
                    return false;
                }

                floor[z, x] = candidate;
                return true;
            });

            for (int z = 0; z < n; z++)
            {
                for (int x = 0; x < n; x++)
                {
                    if (!cliff[z, x] && !fixedHeight[z, x])
                    {
                        heights[z, x] = Mathf.Max(heights[z, x], floor[z, x]);
                    }
                }
            }

            Sweep(n, cliff, (z, x, zn, xn) =>
            {
                if (fixedHeight[z, x])
                {
                    return false;
                }

                float candidate = Mathf.Max(floor[z, x], heights[zn, xn] + maxStep);
                if (candidate >= heights[z, x] - Tolerance)
                {
                    return false;
                }

                heights[z, x] = candidate;
                return true;
            });
        }

        private const float Tolerance = 0.0001f;

        /// <summary>
        /// Applies <paramref name="relax"/> to every walkable sample against each of
        /// its four neighbours, sweeping forward then backward, until nothing changes.
        /// <paramref name="relax"/> gets (z, x) and the neighbour (zn, xn) and says
        /// whether it changed the sample.
        /// </summary>
        private static void Sweep(int n, bool[,] cliff, Func<int, int, int, int, bool> relax)
        {
            for (int round = 0; round < MaxSlopePasses; round++)
            {
                bool changed = false;
                for (int z = 0; z < n; z++)
                {
                    for (int x = 0; x < n; x++)
                    {
                        changed |= RelaxWith(n, cliff, relax, z, x, z, x - 1) | RelaxWith(n, cliff, relax, z, x, z - 1, x);
                    }
                }

                for (int z = n - 1; z >= 0; z--)
                {
                    for (int x = n - 1; x >= 0; x--)
                    {
                        changed |= RelaxWith(n, cliff, relax, z, x, z, x + 1) | RelaxWith(n, cliff, relax, z, x, z + 1, x);
                    }
                }

                if (!changed)
                {
                    return;
                }
            }

            throw new InvalidOperationException($"Relief slopes did not settle after {MaxSlopePasses} rounds.");
        }

        private static bool RelaxWith(int n, bool[,] cliff, Func<int, int, int, int, bool> relax, int z, int x, int zn, int xn)
        {
            if (zn < 0 || xn < 0 || zn >= n || xn >= n || cliff[z, x] || cliff[zn, xn])
            {
                return false;
            }

            return relax(z, x, zn, xn);
        }
    }
}
