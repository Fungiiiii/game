using System.Collections.Generic;
using Fungiiiii.Editor.World;
using Fungiiiii.Runtime.World;
using NUnit.Framework;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class GrassAndGroveTests
    {
        private const int Resolution = 128;
        private const float CellSize = 2f;
        private static readonly Vector2 Origin = new Vector2(-128f, -128f);
        private static readonly List<ExclusionCircle> NoExclusions = new List<ExclusionCircle>();

        [Test]
        public void SameSeedGivesSameGrass()
        {
            int[,] first = Grass(seed: 4, NoExclusions, steep: (x, z) => false);
            int[,] second = Grass(seed: 4, NoExclusions, steep: (x, z) => false);
            int[,] other = Grass(seed: 5, NoExclusions, steep: (x, z) => false);

            Assert.That(second, Is.EqualTo(first));
            Assert.That(other, Is.Not.EqualTo(first));
        }

        [Test]
        public void GrassGrowsInPatchesWithBareGroundBetween()
        {
            int[,] density = Grass(seed: 4, NoExclusions, steep: (x, z) => false);

            int covered = 0;
            foreach (int cell in density)
            {
                Assert.That(cell, Is.InRange(0, 6));
                covered += cell > 0 ? 1 : 0;
            }

            float share = covered / (float)density.Length;
            Assert.That(share, Is.InRange(0.2f, 0.85f), "grass should cover part of the ground, not all or nothing");
        }

        [Test]
        public void NoGrassInClearingsPathsOrOnSteepGround()
        {
            var clearing = new ExclusionCircle(new Vector3(10f, 0f, 10f), 20f);

            int[,] density = Grass(seed: 4, new List<ExclusionCircle> { clearing }, steep: (x, z) => x < 20);

            for (int z = 0; z < Resolution; z++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    Vector2 centre = Origin + new Vector2(x + 0.5f, z + 0.5f) * CellSize;
                    if (x < 20 || clearing.Contains(new Vector3(centre.x, 0f, centre.y)))
                    {
                        Assert.That(density[z, x], Is.Zero, $"grass at cell ({x}, {z})");
                    }
                }
            }
        }

        [Test]
        public void GrovesLeaveOpenGroundWithoutReshufflingTheTrees()
        {
            var gameObject = new GameObject("GroveTest");
            try
            {
                var scatter = gameObject.AddComponent<ForestScatter>();
                scatter.Configure(new Vector2(200f, 200f), 6f, 1f, 1f, rotate: true, newSeed: 8);
                List<ForestScatter.Placement> even = scatter.ComputePlacements(null);

                scatter.ConfigureGroves(size: 40f, coverage: 0.5f);
                List<ForestScatter.Placement> groves = scatter.ComputePlacements(null);

                Assert.That(groves.Count, Is.InRange(even.Count / 5, even.Count * 4 / 5), "groves should keep part of the trees");
                foreach (ForestScatter.Placement placement in groves)
                {
                    Assert.That(even.Exists(p => p.Position == placement.Position && p.Yaw == placement.Yaw), Is.True);
                }
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        private static int[,] Grass(int seed, List<ExclusionCircle> exclusions, System.Func<int, int, bool> steep)
        {
            return GrassDensity.Compute(Resolution, Origin, CellSize, seed, patchSize: 18f, coverage: 0.55f, maxDensity: 6, exclusions, steep);
        }
    }
}
