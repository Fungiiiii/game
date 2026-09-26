using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Runtime.World
{
    /// <summary>
    /// Random 2D points that are never closer than a minimum distance (Bridson's algorithm).
    ///
    /// Gives a natural scatter: no visible grid, no clumps, no holes. Deterministic for a given seed,
    /// so the same forest can be regenerated identically by anyone on the team.
    /// </summary>
    internal static class PoissonDiskSampler
    {
        private const int DefaultAttemptsPerPoint = 30;

        /// <summary>
        /// Returns points in [0, areaSize.x) x [0, areaSize.y).
        /// </summary>
        public static List<Vector2> Sample(Vector2 areaSize, float minDistance, int seed, int attemptsPerPoint = DefaultAttemptsPerPoint)
        {
            if (minDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(minDistance), "Minimum distance must be positive.");
            }

            var points = new List<Vector2>();
            if (areaSize.x <= 0f || areaSize.y <= 0f)
            {
                return points;
            }

            var random = new System.Random(seed);
            float cellSize = minDistance / Mathf.Sqrt(2f);
            int gridWidth = Mathf.CeilToInt(areaSize.x / cellSize);
            int gridHeight = Mathf.CeilToInt(areaSize.y / cellSize);
            var grid = new int[gridWidth * gridHeight];
            for (int i = 0; i < grid.Length; i++)
            {
                grid[i] = -1;
            }

            var active = new List<int>();
            float minDistanceSquared = minDistance * minDistance;

            AddPoint(new Vector2((float)random.NextDouble() * areaSize.x, (float)random.NextDouble() * areaSize.y));

            while (active.Count > 0)
            {
                int activeSlot = random.Next(active.Count);
                Vector2 origin = points[active[activeSlot]];
                bool found = false;

                for (int attempt = 0; attempt < attemptsPerPoint; attempt++)
                {
                    float angle = (float)(random.NextDouble() * Math.PI * 2.0);
                    float distance = minDistance * (1f + (float)random.NextDouble());
                    Vector2 candidate = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;

                    if (IsValid(candidate))
                    {
                        AddPoint(candidate);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    active[activeSlot] = active[active.Count - 1];
                    active.RemoveAt(active.Count - 1);
                }
            }

            return points;

            void AddPoint(Vector2 point)
            {
                points.Add(point);
                int index = points.Count - 1;
                active.Add(index);
                grid[CellX(point) + CellY(point) * gridWidth] = index;
            }

            bool IsValid(Vector2 candidate)
            {
                if (candidate.x < 0f || candidate.y < 0f || candidate.x >= areaSize.x || candidate.y >= areaSize.y)
                {
                    return false;
                }

                int cx = CellX(candidate);
                int cy = CellY(candidate);
                for (int y = Mathf.Max(0, cy - 2); y <= Mathf.Min(gridHeight - 1, cy + 2); y++)
                {
                    for (int x = Mathf.Max(0, cx - 2); x <= Mathf.Min(gridWidth - 1, cx + 2); x++)
                    {
                        int neighbour = grid[x + y * gridWidth];
                        if (neighbour >= 0 && (points[neighbour] - candidate).sqrMagnitude < minDistanceSquared)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            int CellX(Vector2 p) => Mathf.Clamp((int)(p.x / cellSize), 0, gridWidth - 1);
            int CellY(Vector2 p) => Mathf.Clamp((int)(p.y / cellSize), 0, gridHeight - 1);
        }
    }
}
