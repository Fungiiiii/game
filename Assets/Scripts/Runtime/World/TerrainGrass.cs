using UnityEngine;

namespace Fungiiiii.Runtime.World
{
    /// <summary>
    /// Settings of the grass of the Terrain this object sits under: tufts drawn as
    /// Terrain details, in patches, never in a clearing, a path or on steep ground.
    /// Written in the editor with the forest (see TerrainForestGenerator in the
    /// editor assembly); nothing runs at play time.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TerrainGrass : MonoBehaviour
    {
        [SerializeField, Tooltip("The tuft model. Its import must not rotate it: a Terrain detail only keeps position, yaw and scale.")]
        private GameObject tuft;

        [SerializeField, Tooltip("Same seed, same patches.")]
        private int seed = 1;

        [SerializeField, Min(1f), Tooltip("Typical size of a grass patch, in metres.")]
        private float patchSize = 18f;

        [SerializeField, Range(0f, 1f), Tooltip("Roughly the share of the ground covered by patches.")]
        private float coverage = 0.55f;

        [SerializeField, Range(1, 16), Tooltip("Tufts per detail cell at the heart of a patch.")]
        private int maxDensity = 6;

        [SerializeField, Range(0f, 90f), Tooltip("No grass where the ground is steeper than this, in degrees.")]
        private float maxSlope = 30f;

        public GameObject Tuft => tuft;
        public int Seed => seed;
        public float PatchSize => patchSize;
        public float Coverage => coverage;
        public int MaxDensity => maxDensity;
        public float MaxSlope => maxSlope;

        internal void Configure(GameObject newTuft, int newSeed, float size, float share, int density, float slope)
        {
            tuft = newTuft;
            seed = newSeed;
            patchSize = size;
            coverage = share;
            maxDensity = density;
            maxSlope = slope;
        }
    }
}
