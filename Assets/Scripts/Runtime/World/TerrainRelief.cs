using System;
using UnityEngine;

namespace Fungiiiii.Runtime.World
{
    /// <summary>
    /// Settings of the low poly relief of the Terrain this object sits under.
    ///
    /// Generation runs in the editor (see TerrainReliefTool in the editor assembly)
    /// and writes the Terrain's heightmap: nothing runs at play time. The playable
    /// area is centred on this object; past its edge the ground rises in cliffs.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TerrainRelief : MonoBehaviour
    {
        [SerializeField]
        private ReliefSettings settings = new ReliefSettings();

        public ReliefSettings Settings => settings;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.95f, 0.75f, 0.3f, 0.9f);
            Vector2 size = settings.PlayableSize;
            Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0.01f, size.y));
        }
    }

    /// <summary>
    /// Plain-data relief settings. Same values and seed, same heightmap.
    /// </summary>
    [Serializable]
    public sealed class ReliefSettings
    {
        [SerializeField, Tooltip("Same seed, same relief. Change it for different hills.")]
        private int seed = 1;

        [SerializeField, Tooltip("Walkable area (X, Z) in metres, centred on the relief object. Cliffs rise past its edge.")]
        private Vector2 playableSize = new Vector2(600f, 600f);

        [SerializeField, Min(0f), Tooltip("Mean ground height, in metres. Keep it above the hill amplitude: the ground must stay above the terrain's bottom, where trees can no longer be sunk.")]
        private float baseHeight = 20f;

        [SerializeField, Min(0f), Tooltip("How far the hills go above and below the mean height, in metres.")]
        private float hillAmplitude = 16f;

        [SerializeField, Min(1f), Tooltip("Typical distance between two hills, in metres.")]
        private float hillWavelength = 170f;

        [SerializeField, Range(1, 5)]
        private int octaves = 3;

        [SerializeField, Min(0.1f), Tooltip("Height of one plateau step, in metres.")]
        private float terraceStep = 4f;

        [SerializeField, Range(0.05f, 1f), Tooltip("Share of each step spent sloping. Low values give flat plateaus and short, steep slopes.")]
        private float terraceRamp = 0.3f;

        [SerializeField, Range(1f, 89f), Tooltip("Steepest ground allowed in the playable area, in degrees. Keep it under the player's slope limit (45).")]
        private float maxWalkableSlope = 40f;

        [SerializeField, Min(0f), Tooltip("Height of one cliff tier past the playable edge, in metres.")]
        private float cliffHeight = 14f;

        [SerializeField, Range(1, 5)]
        private int cliffTiers = 2;

        [SerializeField, Min(1f), Tooltip("Distance between two cliff tiers, in metres.")]
        private float cliffSpacing = 30f;

        [SerializeField, Min(0f), Tooltip("Width of the slope around a flattened clearing, in metres.")]
        private float flattenBlend = 6f;

        [SerializeField, Min(0f), Tooltip("How far the cliff line wanders in and out of the playable edge, in metres.")]
        private float cliffWobble = 12f;

        [SerializeField, Min(0f), Tooltip("Random height of each cliff sample, in metres: rough rock faces instead of one flat wall.")]
        private float cliffRoughness = 3f;

        [SerializeField, Min(0f), Tooltip("Random height of each walkable sample, in metres: small facets that catch the light. Level clearings stay level.")]
        private float groundRoughness = 0.5f;

        public int Seed => seed;
        public Vector2 PlayableSize => playableSize;
        public float BaseHeight => baseHeight;
        public float HillAmplitude => hillAmplitude;
        public float HillWavelength => hillWavelength;
        public int Octaves => octaves;
        public float TerraceStep => terraceStep;
        public float TerraceRamp => terraceRamp;
        public float MaxWalkableSlope => maxWalkableSlope;
        public float CliffHeight => cliffHeight;
        public int CliffTiers => cliffTiers;
        public float CliffSpacing => cliffSpacing;
        public float FlattenBlend => flattenBlend;
        public float CliffWobble => cliffWobble;
        public float CliffRoughness => cliffRoughness;
        public float GroundRoughness => groundRoughness;

        internal void Configure(int newSeed, Vector2 playable, float slope)
        {
            seed = newSeed;
            playableSize = playable;
            maxWalkableSlope = slope;
        }
    }
}
