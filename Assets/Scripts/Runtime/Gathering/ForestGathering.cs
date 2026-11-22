using System;
using System.Collections.Generic;
using Fungiiiii.Gathering;
using Fungiiiii.Outlines;
using Fungiiiii.Runtime.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fungiiiii.Gathering
{
    /// <summary>
    /// The sticks and spore mushrooms of the forest: where each kind can stand, and a field
    /// of each that keeps a few around the players. A component of the level, set in the
    /// Inspector; the kinds themselves are data (<see cref="GatherableDefinition"/>).
    ///
    /// Every decor mushroom of the forest becomes a spore spot, so none stays that cannot be
    /// gathered; the foot of the trees and a scatter over the forest add more, so that a few
    /// are always within reach. Sticks lie at the foot of the trees and fall from their leaves.
    /// </summary>
    internal sealed class ForestGathering : MonoBehaviour
    {
        /// <summary>Spore mushrooms go 2 cm into the ground, so none floats on a slope.</summary>
        private const float MushroomSink = 0.02f;

        /// <summary>The prefix of the forest's hand-placed decor mushrooms.</summary>
        private const string DecorMushroomNamePrefix = "PF_MushroomCluster";

        [SerializeField]
        private GatheringSettings settings;

        [SerializeField]
        private GatherableDefinition sticks;

        [SerializeField]
        private GatherableDefinition sporeMushrooms;

        [Header("Where they stand")]
        [Tooltip("Decor mushroom of the forest: the Terrain's instances of it become spore spots.")]
        [SerializeField]
        private GameObject decorMushroomPrefab;

        [Tooltip("Trees of the forest's Terrain under which sticks and mushrooms appear.")]
        [SerializeField]
        private GameObject[] treePrefabs = new GameObject[0];

        [Tooltip("Ground rectangle (x, z, width, depth) where they appear: the walkable part of the forest.")]
        [SerializeField]
        private Rect playableArea = new Rect(-290f, -290f, 580f, 580f);

        [Tooltip("Sticks lie between these two distances from a trunk, in metres.")]
        [SerializeField]
        private Vector2 stickTrunkDistance = new Vector2(1f, 2.5f);

        [Tooltip("No stick on ground steeper than this, in degrees.")]
        [SerializeField, Range(0f, 90f)]
        private float stickMaxSlope = 30f;

        [Tooltip("Spore mushrooms at the foot of a tree stand between these two distances from its trunk, in metres.")]
        [SerializeField]
        private Vector2 mushroomTrunkDistance = new Vector2(1f, 2.2f);

        [Tooltip("Spore mushrooms are also scattered over the playable area, this far apart, in metres.")]
        [SerializeField, Min(1f)]
        private float mushroomScatterSpacing = 15f;

        [Tooltip("No spore mushroom on ground steeper than this, in degrees.")]
        [SerializeField, Range(0f, 90f)]
        private float mushroomMaxSlope = 30f;

        [Tooltip("Size of a spore mushroom, drawn between these two scales.")]
        [SerializeField]
        private Vector2 mushroomScale = new Vector2(0.8f, 1.7f);

        [Tooltip("No spore mushroom closer than this to a stick spot, in metres.")]
        [SerializeField, Min(0f)]
        private float mushroomStickClearance = 1.5f;

        [Tooltip("No spore mushroom closer than this to another one, in metres.")]
        [SerializeField, Min(0f)]
        private float mushroomClearance = 1.2f;

        private readonly List<Gatherable> gatherables = new List<Gatherable>();
        private readonly List<GatherableField> fields = new List<GatherableField>();
        private readonly List<GatherableSpot> mushroomSpots = new List<GatherableSpot>();
        private readonly List<GatherableSpot> stickSpots = new List<GatherableSpot>();

        /// <summary>Raised for every stick or mushroom built, so that the interact control can use it.</summary>
        public event Action<Gatherable> GatherableCreated;

        /// <summary>Every stick and mushroom built so far, on the map or waiting for a spot.</summary>
        public IReadOnlyList<Gatherable> Gatherables => gatherables;

        /// <summary>Every place a spore mushroom may appear, overlaps removed.</summary>
        public IReadOnlyList<GatherableSpot> MushroomSpots => mushroomSpots;

        /// <summary>Every place a stick may appear.</summary>
        public IReadOnlyList<GatherableSpot> StickSpots => stickSpots;

        public GatherableDefinition Sticks => sticks;

        public GatherableDefinition SporeMushrooms => sporeMushrooms;

        /// <summary>What is missing for the component to work, or null when it is sound.</summary>
        public string FindProblem()
        {
            if (settings == null || sticks == null || sporeMushrooms == null || decorMushroomPrefab == null)
            {
                return $"{name} lacks its settings, a kind or the decor mushroom.";
            }

            if (treePrefabs.Length == 0 || Array.IndexOf(treePrefabs, null) >= 0)
            {
                return $"{name} has no tree, or an empty one.";
            }

            return sticks.FindProblem() ?? sporeMushrooms.FindProblem();
        }

        /// <summary>Finds every spot in the forest, then fills the bubble of each player.</summary>
        /// <param name="session">The forest's Terrain for this session: its decor mushrooms are taken out of it.</param>
        public void Populate(Scene forest, SessionTerrain session, IReadOnlyList<FieldPlayer> players, ItemOutlineFactory outlines, System.Random random)
        {
            // Hugo's hand-placed decor mushrooms are hidden, and their places become spots
            // like those of the Terrain, whose own are taken out for this session.
            ForestResourceSpots.TakeLandmarks(forest, DecorMushroomNamePrefix, mushroomSpots);
            session.TakeTrees(decorMushroomPrefab, mushroomSpots);
            var terrain = session.Terrain;

            ForestResourceSpots.FindStickSpots(
                terrain,
                treePrefabs,
                playableArea,
                Mathf.Min(stickTrunkDistance.x, stickTrunkDistance.y),
                Mathf.Max(stickTrunkDistance.x, stickTrunkDistance.y),
                stickMaxSlope,
                new System.Random(random.Next()),
                stickSpots);

            AddMoreMushroomSpots(terrain, random);
            ForestResourceSpots.RemoveOverlaps(mushroomSpots, stickSpots, mushroomStickClearance, mushroomClearance, ForestResourceSpots.TouchesTrunkOrRock);

            AddField(sporeMushrooms, mushroomSpots, players, outlines, random);
            AddField(sticks, stickSpots, players, outlines, random);
        }


        /// <summary>
        /// Spore spots at the foot of the trees and scattered over the forest, out of its
        /// paths: with the decor mushrooms alone, some places had too few within reach.
        /// </summary>
        private void AddMoreMushroomSpots(Terrain terrain, System.Random random)
        {
            var pose = new MushroomPose(
                Mathf.Min(mushroomScale.x, mushroomScale.y),
                Mathf.Max(mushroomScale.x, mushroomScale.y),
                MushroomSink);

            ForestResourceSpots.FindTreeFootMushroomSpots(
                terrain,
                treePrefabs,
                playableArea,
                Mathf.Min(mushroomTrunkDistance.x, mushroomTrunkDistance.y),
                Mathf.Max(mushroomTrunkDistance.x, mushroomTrunkDistance.y),
                mushroomMaxSlope,
                pose,
                new System.Random(random.Next()),
                mushroomSpots);

            var paths = new List<ExclusionCircle>();
            foreach (var path in FindObjectsByType<ScatterExclusionPath>(FindObjectsSortMode.InstanceID))
            {
                paths.AddRange(path.ToCircles());
            }

            ForestResourceSpots.ScatterMushroomSpots(
                terrain,
                playableArea,
                mushroomScatterSpacing,
                mushroomMaxSlope,
                paths,
                pose,
                new System.Random(random.Next()),
                mushroomSpots);
        }



        private void AddField(GatherableDefinition kind, List<GatherableSpot> spots, IReadOnlyList<FieldPlayer> players, ItemOutlineFactory outlines, System.Random random)
        {
            if (spots.Count == 0)
            {
                Debug.LogWarning($"{name} found no spot for {kind.name}: none will appear.", this);
            }

            var field = new GameObject(kind.name).AddComponent<GatherableField>();
            field.transform.SetParent(transform, false);
            field.Initialize(
                kind.Item,
                kind.Variants,
                spots,
                settings.For(kind),
                new System.Random(random.Next()),
                players,
                settings.Reach,
                outlines,
                Register);
            fields.Add(field);
        }

        private void Register(Gatherable gatherable)
        {
            gatherables.Add(gatherable);
            GatherableCreated?.Invoke(gatherable);
        }
    }
}
