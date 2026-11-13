using System.Collections.Generic;
using Fungiiiii.Editor.World;
using Fungiiiii.Runtime.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class ReliefGeneratorTests
    {
        // 129 samples over 512 m: 4 m cells, the size the real Terrain uses.
        private const int Resolution = 129;
        private const float CellSize = 4f;
        private const float MaxSlope = 40f;

        private static readonly ReliefGrid Grid = new ReliefGrid(Resolution, new Vector2(-256f, -256f), CellSize);
        private static readonly Vector2 Playable = new Vector2(360f, 360f);
        private static readonly List<ExclusionCircle> NoFlats = new List<ExclusionCircle>();

        [Test]
        public void SameSeedGivesSameRelief()
        {
            float[,] first = ReliefGenerator.Generate(Settings(seed: 11), Grid, Vector2.zero, NoFlats);
            float[,] second = ReliefGenerator.Generate(Settings(seed: 11), Grid, Vector2.zero, NoFlats);
            float[,] other = ReliefGenerator.Generate(Settings(seed: 12), Grid, Vector2.zero, NoFlats);

            Assert.That(second, Is.EqualTo(first));
            Assert.That(other, Is.Not.EqualTo(first));
        }

        [Test]
        public void ReliefIsNotFlat()
        {
            float[,] heights = ReliefGenerator.Generate(Settings(seed: 3), Grid, Vector2.zero, NoFlats);

            float min = float.MaxValue, max = float.MinValue;
            ForEachPlayableSample(3, (x, z) =>
            {
                min = Mathf.Min(min, heights[z, x]);
                max = Mathf.Max(max, heights[z, x]);
            });

            Assert.That(max - min, Is.GreaterThan(6f), "the playable area should have hills");
        }

        [Test]
        public void NoWalkableTriangleIsSteeperThanTheLimit()
        {
            foreach (int seed in new[] { 1, 2, 3, 20716 })
            {
                float[,] heights = ReliefGenerator.Generate(Settings(seed), Grid, Vector2.zero, NoFlats);

                float steepest = 0f;
                for (int z = 0; z + 1 < Resolution; z++)
                {
                    for (int x = 0; x + 1 < Resolution; x++)
                    {
                        if (!IsPlayable(x, z, seed) || !IsPlayable(x + 1, z, seed) || !IsPlayable(x, z + 1, seed) || !IsPlayable(x + 1, z + 1, seed))
                        {
                            continue;
                        }

                        // Both ways a quad can be split into two triangles.
                        steepest = Mathf.Max(steepest, TriangleSlope(heights, x, z, x + 1, z, x, z + 1));
                        steepest = Mathf.Max(steepest, TriangleSlope(heights, x + 1, z + 1, x, z + 1, x + 1, z));
                        steepest = Mathf.Max(steepest, TriangleSlope(heights, x, z, x + 1, z, x + 1, z + 1));
                        steepest = Mathf.Max(steepest, TriangleSlope(heights, x, z, x + 1, z + 1, x, z + 1));
                    }
                }

                Assert.That(steepest, Is.LessThanOrEqualTo(MaxSlope + 0.01f), $"seed {seed}");
            }
        }

        [Test]
        public void TheBorderRisesInCliffsTooSteepToWalk()
        {
            float[,] heights = ReliefGenerator.Generate(Settings(seed: 5), Grid, Vector2.zero, NoFlats);

            // Walk east along the middle row and find the step at the playable edge.
            int row = Resolution / 2;
            float steepestStep = 0f;
            for (int x = 0; x + 1 < Resolution; x++)
            {
                if (IsPlayable(x, row, 5) && !IsPlayable(x + 1, row, 5))
                {
                    steepestStep = Mathf.Max(steepestStep, heights[row, x + 1] - heights[row, x]);
                }
            }

            float slope = Mathf.Atan2(steepestStep, CellSize) * Mathf.Rad2Deg;
            Assert.That(slope, Is.GreaterThan(FirstPersonSlopeLimit + 10f), "the playable edge must be a cliff the player cannot climb");
        }

        [Test]
        public void FlatAreasAreLevel()
        {
            var clearing = new ExclusionCircle(new Vector3(40f, 0f, -30f), 14f);

            float[,] heights = ReliefGenerator.Generate(Settings(seed: 9), Grid, Vector2.zero, new List<ExclusionCircle> { clearing });

            float? level = null;
            ForEachPlayableSample(9, (x, z) =>
            {
                Vector2 position = Grid.PositionOf(x, z);
                if (Vector2.Distance(position, clearing.Center) > clearing.Radius)
                {
                    return;
                }

                level ??= heights[z, x];
                Assert.That(heights[z, x], Is.EqualTo(level.Value).Within(0.0001f));
            });
            Assert.That(level.HasValue, Is.True);
        }

        [Test]
        public void ApplyWritesTheHeightmapAndIsUndoneInOneStep()
        {
            var data = new TerrainData { heightmapResolution = 65 };
            data.size = new Vector3(256f, 60f, 256f);
            GameObject terrainObject = Terrain.CreateTerrainGameObject(data);
            try
            {
                var terrain = terrainObject.GetComponent<Terrain>();
                terrainObject.transform.position = new Vector3(-128f, 0f, -128f);
                float[,] before = data.GetHeights(0, 0, 65, 65);
                Undo.IncrementCurrentGroup();

                var settings = new ReliefSettings();
                settings.Configure(newSeed: 4, playable: new Vector2(180f, 180f), slope: MaxSlope);
                TerrainReliefTool.Apply(terrain, settings, Vector2.zero, NoFlats, NoFlats);
                Assume.That(data.GetHeights(0, 0, 65, 65), Is.Not.EqualTo(before));

                Undo.PerformUndo();

                Assert.That(data.GetHeights(0, 0, 65, 65), Is.EqualTo(before));
            }
            finally
            {
                Object.DestroyImmediate(terrainObject);
                Object.DestroyImmediate(data);
            }
        }

        // The first-person player's CharacterController slope limit.
        private const float FirstPersonSlopeLimit = 45f;

        private static ReliefSettings Settings(int seed)
        {
            var settings = new ReliefSettings();
            settings.Configure(seed, Playable, MaxSlope);
            return settings;
        }

        // The cliff line wanders with the seed: a sample is walkable for the seed of the relief it belongs to.
        private static bool IsPlayable(int x, int z, int seed)
        {
            return ReliefGenerator.CliffDistance(Settings(seed), Grid.PositionOf(x, z), Vector2.zero) <= 0f;
        }

        private static void ForEachPlayableSample(int seed, System.Action<int, int> action)
        {
            for (int z = 0; z < Resolution; z++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    if (IsPlayable(x, z, seed))
                    {
                        action(x, z);
                    }
                }
            }
        }

        private static float TriangleSlope(float[,] h, int xa, int za, int xb, int zb, int xc, int zc)
        {
            var a = new Vector3(xa * CellSize, h[za, xa], za * CellSize);
            var b = new Vector3(xb * CellSize, h[zb, xb], zb * CellSize);
            var c = new Vector3(xc * CellSize, h[zc, xc], zc * CellSize);
            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
            return Vector3.Angle(normal, Vector3.up) is var angle && angle > 90f ? 180f - angle : angle;
        }
    }
}
