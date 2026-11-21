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
        /// <summary>Row of the hash lattice the fade draws from, apart from any other use of the seed.</summary>
        private const int FadeHashRow = 7;

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

        [SerializeField, Tooltip("Empty rectangle (X, Z) in the middle of the area, in metres. Leave at 0 for none. Used for a ring of forest around the playable area.")]
        private Vector2 innerHoleSize;

        [SerializeField, Range(0f, 90f), Tooltip("No instance where the ground is steeper than this, in degrees. 90 places everywhere.")]
        private float maxSlope = 90f;

        [SerializeField, Min(0f), Tooltip("Typical size of a grove, in metres. 0 spreads the layer evenly.")]
        private float groveSize;

        [SerializeField, Range(0f, 1f), Tooltip("Roughly the share of the area covered by groves; the rest stays open. Only used when Grove Size is above 0.")]
        private float groveCoverage = 0.6f;

        [SerializeField, Range(0f, 0.5f), Tooltip("Height drawn apart from the width, as a share: 0.15 makes some trees up to 15 % taller or shorter than their width suggests. Two trees of the same model never share a silhouette.")]
        private float heightVariation;

        [SerializeField, Min(0f), Tooltip("Depth of the band along the area's south edge (lowest Z) over which the layer thins out to nothing, in metres. 0 keeps a hard edge. Two zones whose areas overlap fade into one another instead of stacking.")]
        private float fadeSouth;

        [SerializeField, Min(0f), Tooltip("Depth of the band along the area's north edge (highest Z) over which the layer thins out to nothing, in metres. 0 keeps a hard edge.")]
        private float fadeNorth;

        public GameObject Prefab => prefab;

        public float MaxSlope => maxSlope;

        public float MinDistance => minDistance;

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

            for (int index = 0; index < samples.Count; index++)
            {
                Vector2 sample = samples[index];

                // Always draw the random values, even for excluded points, so adding
                // a zone doesn't reshuffle every other tree.
                float scale = Mathf.Lerp(minScale, maxScale, (float)random.NextDouble());
                float yaw = (float)random.NextDouble() * 360f;
                float stretch = Mathf.Lerp(1f - heightVariation, 1f + heightVariation, (float)random.NextDouble());

                var position = origin + new Vector3(sample.x, 0f, sample.y);
                if (IsInHole(position) || !IsInGrove(position) || IsExcluded(position, exclusions) || IsFadedOut(position, index))
                {
                    continue;
                }

                result.Add(new Placement(position, scale, randomYRotation ? yaw : 0f, scale * stretch));
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

        internal void ConfigureLimits(Vector2 holeSize, float slope)
        {
            innerHoleSize = holeSize;
            maxSlope = slope;
        }

        internal void ConfigureHeightVariation(float variation)
        {
            heightVariation = variation;
        }

        internal void ConfigureGroves(float size, float coverage)
        {
            groveSize = size;
            groveCoverage = coverage;
        }

        internal void ConfigureFade(float south, float north)
        {
            fadeSouth = south;
            fadeNorth = north;
        }

        /// <summary>
        /// Near a fading edge, an instance is kept with a chance that falls from 1 at
        /// the inner side of the band to 0 at the edge. The draw is a hash of the
        /// sample, not a value of the random stream: fading a layer removes instances
        /// near its edge and moves none of the others.
        /// </summary>
        private bool IsFadedOut(Vector3 position, int sampleIndex)
        {
            if (fadeSouth <= 0f && fadeNorth <= 0f)
            {
                return false;
            }

            float south = transform.position.z - areaSize.y * 0.5f;
            float north = transform.position.z + areaSize.y * 0.5f;
            float keep = EdgeShare(position.z - south, fadeSouth) * EdgeShare(north - position.z, fadeNorth);
            if (keep >= 1f)
            {
                return false;
            }

            return ValueNoise.Lattice(sampleIndex, FadeHashRow, unchecked(seed * 92821 + 3)) >= keep;
        }

        /// <summary>1 beyond the band, falling linearly to 0 at the edge.</summary>
        private static float EdgeShare(float distanceToEdge, float band)
        {
            return band > 0f ? Mathf.Clamp01(distanceToEdge / band) : 1f;
        }

        /// <summary>
        /// Groves are where a seeded noise rises above a threshold set by the
        /// coverage: trees gather in clumps with open ground between them, instead
        /// of an even plantation.
        /// </summary>
        private bool IsInGrove(Vector3 position)
        {
            if (groveSize <= 0f)
            {
                return true;
            }

            float noise = ValueNoise.Fbm(new Vector2(position.x, position.z) / groveSize, 2, unchecked(seed * 31 + 17));
            // The noise sits mostly between 0.25 and 0.75: map the coverage onto that range.
            float threshold = Mathf.Lerp(0.75f, 0.25f, groveCoverage);
            return noise >= threshold;
        }

        private bool IsInHole(Vector3 position)
        {
            Vector3 offset = position - transform.position;
            return Mathf.Abs(offset.x) < innerHoleSize.x * 0.5f && Mathf.Abs(offset.z) < innerHoleSize.y * 0.5f;
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
            if (innerHoleSize != Vector2.zero)
            {
                Gizmos.DrawWireCube(transform.position, new Vector3(innerHoleSize.x, 0.01f, innerHoleSize.y));
            }
        }

        public readonly struct Placement
        {
            public Placement(Vector3 position, float scale, float yaw)
                : this(position, scale, yaw, scale)
            {
            }

            public Placement(Vector3 position, float scale, float yaw, float heightScale)
            {
                HeightScale = heightScale;
                Position = position;
                Scale = scale;
                Yaw = yaw;
            }

            public Vector3 Position { get; }
            /// <summary>Width scale.</summary>
            public float Scale { get; }

            public float HeightScale { get; }
            public float Yaw { get; }
        }
    }
}
