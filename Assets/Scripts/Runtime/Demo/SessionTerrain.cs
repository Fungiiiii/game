using System;
using System.Collections.Generic;
using Fungiiiii.Gathering;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fungiiiii.Demo
{
    /// <summary>
    /// The forest's Terrain for one play session. What the game takes out of it, the decor
    /// mushrooms that become spore spots and the decor ferns that would be mistaken for
    /// water ferns, is taken out of a copy of its data, made the first time something is
    /// taken: the forest's own asset is never changed. Every taker shares the one copy.
    /// </summary>
    internal sealed class SessionTerrain : IDisposable
    {
        private TerrainData copy;

        public SessionTerrain(Terrain terrain)
        {
            Terrain = terrain != null ? terrain : throw new ArgumentNullException(nameof(terrain));
        }

        public Terrain Terrain { get; }

        /// <summary>
        /// Takes every instance of <paramref name="prefab"/> the Terrain draws out of it.
        /// Their poses are added to <paramref name="taken"/> when it is given.
        /// </summary>
        /// <returns>How many were taken.</returns>
        public int TakeTrees(GameObject prefab, List<GatherableSpot> taken = null)
        {
            var data = Terrain.terrainData;
            var prototype = ForestResourceSpots.PrototypeIndexOf(data, prefab);
            if (prototype < 0)
            {
                return 0;
            }

            var origin = Terrain.GetPosition();
            var size = data.size;
            var kept = new List<TreeInstance>(data.treeInstanceCount);
            var count = 0;

            foreach (var tree in data.treeInstances)
            {
                if (tree.prototypeIndex != prototype)
                {
                    kept.Add(tree);
                    continue;
                }

                count++;
                taken?.Add(new GatherableSpot(
                    origin + Vector3.Scale(tree.position, size),
                    Quaternion.Euler(0f, tree.rotation * Mathf.Rad2Deg, 0f),
                    new Vector3(tree.widthScale, tree.heightScale, tree.widthScale)));
            }

            if (count > 0)
            {
                Writable().SetTreeInstances(kept.ToArray(), false);
            }

            return count;
        }

        /// <summary>The copy, made and given to the Terrain and its collider the first time.</summary>
        private TerrainData Writable()
        {
            if (copy != null)
            {
                return copy;
            }

            var data = Terrain.terrainData;
            copy = Object.Instantiate(data);
            copy.name = data.name + " (play session)";
            Terrain.terrainData = copy;
            if (Terrain.TryGetComponent<TerrainCollider>(out var terrainCollider))
            {
                terrainCollider.terrainData = copy;
            }

            return copy;
        }

        public void Dispose()
        {
            if (copy != null)
            {
                Object.Destroy(copy);
                copy = null;
            }
        }
    }
}
