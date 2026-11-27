using System.Collections.Generic;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Gathering
{
    /// <summary>
    /// A plant that fills up by chance, like a bush with berries, and that only a companion
    /// can empty, as data: its prefab, what it yields, how often it fills, how densely it
    /// grows, and the short text shown over it. The water fern is one. Where it grows comes
    /// from the level.
    /// </summary>
    [CreateAssetMenu(fileName = "RefillingPlant", menuName = "Fungiiiii/Refilling Plant Definition")]
    public sealed class RefillingPlantDefinition : ScriptableObject
    {
        [Tooltip("The plant: a prefab whose root carries a RefillingPlant, with its model and what shows it full.")]
        [SerializeField]
        private GameObject prefab;

        [Tooltip("What it yields to the companion that empties it.")]
        [SerializeField]
        private ItemDefinition yield;

        [Tooltip("Decor it could be mistaken for, taken out of the level so the player is not misled: the decor fern, for the water fern.")]
        [SerializeField]
        private GameObject[] lookAlikes = new GameObject[0];

        [Header("Filling up")]
        [Tooltip("While empty, it draws its chance to fill this often, in seconds.")]
        [SerializeField, Min(0.1f)]
        private float refillIntervalSeconds = 20f;

        [Tooltip("Chance to fill at each draw.")]
        [SerializeField, Range(0f, 1f)]
        private float refillChance = 0.25f;

        [Tooltip("Share of the plants full at the start of a game.")]
        [SerializeField, Range(0f, 1f)]
        private float fullAtStart = 0.5f;

        [Header("Looking full")]
        [Tooltip("Colour the plant is multiplied by while full: darker, as a wet surface.")]
        [SerializeField]
        private Color fullTint = new Color(0.7f, 0.8f, 0.85f, 1f);

        [Tooltip("Sheen of the plant while full, from 0 (matt) to 1: light, or its flat faces flash like plastic.")]
        [SerializeField, Range(0f, 1f)]
        private float fullSmoothness = 0.4f;

        [Header("Where it grows")]
        [Tooltip("Plants stand about this far apart, in metres.")]
        [SerializeField, Min(1f)]
        private float spacing = 30f;

        [Tooltip("No plant on ground steeper than this, in degrees.")]
        [SerializeField, Range(0f, 90f)]
        private float maxSlope = 30f;

        [Tooltip("Size of a plant, drawn between these two scales.")]
        [SerializeField]
        private Vector2 scale = new Vector2(0.9f, 1.2f);

        [Header("Shown to the player")]
        [Tooltip("How close the player must be to aim at it, in metres.")]
        [SerializeField, Min(0f)]
        private float reach = 4f;

        [Tooltip("Key of the UI string table shown over it, full and aimed at, instead of a key to press.")]
        [SerializeField]
        private string labelKey;

        public GameObject Prefab => prefab;

        public ItemDefinition Yield => yield;

        /// <summary>Decor prefabs it could be mistaken for, taken out of the level.</summary>
        public IReadOnlyList<GameObject> LookAlikes => lookAlikes;

        public Color FullTint => fullTint;

        public float FullSmoothness => fullSmoothness;

        public float RefillIntervalSeconds => refillIntervalSeconds;

        public float RefillChance => refillChance;

        public float FullAtStart => fullAtStart;

        public float Spacing => spacing;

        public float MaxSlope => maxSlope;

        public float MinScale => Mathf.Min(scale.x, scale.y);

        public float MaxScale => Mathf.Max(scale.x, scale.y);

        public float Reach => reach;

        public string LabelKey => labelKey;

        /// <summary>What is missing for it to be used, or null when it is sound.</summary>
        public string FindProblem()
        {
            if (prefab == null || yield == null)
            {
                return $"'{name}' has no prefab or yields nothing.";
            }

            if (System.Array.IndexOf(lookAlikes, null) >= 0)
            {
                return $"'{name}' has an empty look-alike.";
            }

            return string.IsNullOrEmpty(labelKey) ? $"'{name}' has no text to show over it." : null;
        }
    }
}
