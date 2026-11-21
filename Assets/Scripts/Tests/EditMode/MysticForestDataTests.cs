using System.Linq;
using Fungiiiii.Editor.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Fungiiiii.Tests.EditMode
{
    /// <summary>
    /// Checks the forest as it is saved in its Terrain data. It holds once the forest has
    /// been regenerated with the current generator (Fungiiiii > Playtest > 6. Regenerate
    /// Mystic Forest).
    /// </summary>
    public sealed class MysticForestDataTests
    {
        private const string TerrainDataPath = "Assets/Scenes/Biomes/MysticForestTerrainData.asset";

        // Regression: 65 border trees stood inside a boulder at the foot of the cliffs.
        [Test]
        public void NoTreeTrunkStandsInsideABoulder()
        {
            var data = AssetDatabase.LoadAssetAtPath<TerrainData>(TerrainDataPath);
            Assume.That(data, Is.Not.Null);

            TreePrototype[] prototypes = data.treePrototypes;
            float[] footprints = prototypes.Select(p => TerrainForestGenerator.FootprintRadius(p.prefab)).ToArray();
            bool[] isTree = prototypes.Select(p => p.prefab.TryGetComponent<CapsuleCollider>(out _)).ToArray();
            bool[] isBoulder = prototypes.Select(p => p.prefab.name == "PF_Rock_Boulder").ToArray();
            Assume.That(isBoulder.Any(b => b), Is.True);

            Vector3 size = data.size;
            TreeInstance[] instances = data.treeInstances;
            var boulders = instances.Where(i => isBoulder[i.prototypeIndex]).ToArray();
            var trees = instances.Where(i => isTree[i.prototypeIndex]).ToArray();

            int inside = 0;
            foreach (TreeInstance boulder in boulders)
            {
                var rock = new Vector2(boulder.position.x * size.x, boulder.position.z * size.z);
                float rockReach = footprints[boulder.prototypeIndex] * boulder.widthScale;
                foreach (TreeInstance tree in trees)
                {
                    var trunk = new Vector2(tree.position.x * size.x, tree.position.z * size.z);
                    float spacing = rockReach + footprints[tree.prototypeIndex] * tree.widthScale;
                    if ((trunk - rock).sqrMagnitude < (spacing - 0.01f) * (spacing - 0.01f))
                    {
                        inside++;
                    }
                }
            }

            Assert.That(inside, Is.Zero, "trees touch or stand inside a boulder");
        }
    }
}
