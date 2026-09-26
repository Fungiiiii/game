using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Runtime.World
{
    /// <summary>
    /// Settings for one layer of a forest (trees, rocks...) scattered over an area.
    ///
    /// A layer sits under a Terrain. Generation runs in the editor and writes the
    /// placements into that Terrain's trees (see TerrainForestGenerator in the editor
    /// assembly): nothing runs at play time, and the scene stores no instance.
    /// Keep this object's rotation at 0 and scale at 1: only its position is used.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ForestScatter : MonoBehaviour
    {
        [SerializeField, Tooltip("The prefab to scatter, with an LODGroup on its root (PF_Tree, PF_Rock_Large...).")]
        private GameObject prefab;

        [SerializeField, Tooltip("Area size in metres (X, Z), centred on this object.")]
        private Vector2 areaSize = new Vector2(100f, 100f);

        [SerializeField, Min(0.1f), Tooltip("Minimum distance between two instances, in metres.")]
        private float minDistance = 5f;

        [SerializeField, Min(0.01f)]
        private float minScale = 0.85f;

        [SerializeField, Min(0.01f)]
        private float maxScale = 1.25f;

        [SerializeField, Tooltip("Random rotation around the vertical axis.")]
        private bool randomYRotation;

        [SerializeField, Tooltip("Same seed, same layout. Change it for a different scatter.")]
        private int seed = 1;

        public GameObject Prefab => prefab;

        /// <summary>
        /// Where each instance goes, in world space. Y is left to the Terrain,
        /// which snaps every tree to its heightmap.
        /// </summary>
        public List<Placement> ComputePlacements(IReadOnlyList<ExclusionCircle> exclusions)
        {
            var result = new List<Placement>();
            var samples = PoissonDiskSampler.Sample(areaSize, minDistance, seed);

            // Separate stream so scale/rotation don't change the positions.
            var random = new System.Random(unchecked(seed * 486187739 + 1));
            Vector3 origin = transform.position - new Vector3(areaSize.x * 0.5f, 0f, areaSize.y * 0.5f);

            foreach (Vector2 sample in samples)
            {
                // Always draw the random values, even for excluded points, so adding
                // a zone doesn't reshuffle every other tree.
                float scale = Mathf.Lerp(minScale, maxScale, (float)random.NextDouble());
                float yaw = (float)random.NextDouble() * 360f;

                var position = origin + new Vector3(sample.x, 0f, sample.y);
                if (IsExcluded(position, exclusions))
                {
                    continue;
                }

                result.Add(new Placement(position, scale, randomYRotation ? yaw : 0f));
            }

            return result;
        }

        internal void Configure(Vector2 area, float distance, float scaleMin, float scaleMax, bool rotate, int newSeed)
        {
            areaSize = area;
            minDistance = distance;
            minScale = scaleMin;
            maxScale = scaleMax;
            randomYRotation = rotate;
            seed = newSeed;
        }

        private static bool IsExcluded(Vector3 position, IReadOnlyList<ExclusionCircle> exclusions)
        {
            if (exclusions == null)
            {
                return false;
            }

            for (int i = 0; i < exclusions.Count; i++)
            {
                if (exclusions[i].Contains(position))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnValidate()
        {
            if (minScale > maxScale)
            {
                (minScale, maxScale) = (maxScale, minScale);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.4f, 0.9f, 0.4f, 0.9f);
            Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, 0.01f, areaSize.y));
        }

        public readonly struct Placement
        {
            public Placement(Vector3 position, float scale, float yaw)
            {
                Position = position;
                Scale = scale;
                Yaw = yaw;
            }

            public Vector3 Position { get; }
            public float Scale { get; }
            public float Yaw { get; }
        }
    }
}
