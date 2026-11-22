using System;
using System.Collections.Generic;
using System.Linq;
using Fungiiiii.Combat;
using Fungiiiii.Companions;
using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// The mushrooms to catch in a level: which species live where, set in the Inspector.
    /// Each one wanders around its home and runs from the net; a new species is a
    /// <see cref="CreatureDefinition"/> and a line here, no code. A species that soaks
    /// something up can live beside the plants it soaks up instead of at set homes: one
    /// beside a share of them, and always beside the first ones the player meets, so the
    /// player sees it by the fern it drinks at. Only the few nearest the player are shown:
    /// the others wait, hidden, until the player comes close.
    /// </summary>
    internal sealed class CreaturePopulation : MonoBehaviour
    {
        [Serializable]
        private struct Spawn
        {
            public CreatureDefinition creature;

            [Tooltip("Homes, in world coordinates on the ground (x, z): one mushroom wanders around each.")]
            public Vector2[] homes;

            [Tooltip("Share of the plants it soaks up that have one living beside them, 0 to 1; 0 for none. The first plants the player meets always have one.")]
            [Range(0f, 1f)]
            public float shareOfSources;

            [Tooltip("How far from that plant its home is, in metres.")]
            [Min(0f)]
            public float distanceFromSource;
        }

        /// <summary>Tries at finding a home beside a plant that is not in a trunk or a rock.</summary>
        private const int HomeTries = 8;

        /// <summary>Seconds between two looks at which mushrooms to show.</summary>
        private const float SecondsBetweenChecks = 0.5f;

        [SerializeField]
        private Spawn[] spawns = new Spawn[0];

        [Tooltip("The mushrooms cannot flee out of this ground rectangle (x, z, width, depth).")]
        [SerializeField]
        private Rect area = new Rect(-30f, -260f, 80f, 65f);

        [Tooltip("Most mushrooms shown at once, the nearest the player first.")]
        [SerializeField, Min(1)]
        private int maxShown = 6;

        [Tooltip("A hidden mushroom shows again once the player is this close, in metres.")]
        [SerializeField, Min(1f)]
        private float showWithin = 50f;

        [Tooltip("A mushroom further than this from the player goes away, in metres: more than the distance it shows again from, so that one at the limit does not blink.")]
        [SerializeField, Min(1f)]
        private float hideBeyond = 60f;

        private readonly List<WildMushroom> mushrooms = new List<WildMushroom>();
        private readonly Dictionary<WildMushroom, CreatureDefinition> species = new Dictionary<WildMushroom, CreatureDefinition>();
        private readonly List<WildMushroom> nearest = new List<WildMushroom>();
        private Transform player;
        private float checkLeft;
        private IReadOnlyList<IAbsorbable> sources;
        private Transform playerView;
        private Action<ItemDefinition, Vector3> loot;

        /// <summary>All of them, the hidden ones too.</summary>
        public IReadOnlyList<WildMushroom> Mushrooms => mushrooms;

        /// <summary>Most mushrooms shown at once.</summary>
        public int MaxShown => maxShown;

        /// <summary>Beyond this distance from the player, in metres, a mushroom goes away.</summary>
        public float HideBeyond => hideBeyond;

        /// <summary>The inventory item a mushroom becomes once caught.</summary>
        public ItemDefinition ItemFor(WildMushroom mushroom) => species[mushroom].CaughtItem;

        /// <summary>What is missing for the component to work, or null when it is sound.</summary>
        public string FindProblem()
        {
            foreach (var spawn in spawns)
            {
                if (spawn.creature == null)
                {
                    return $"{name} has a line without creature.";
                }

                var problem = spawn.creature.FindProblem();
                if (problem != null)
                {
                    return problem;
                }

                if (spawn.shareOfSources > 0f && spawn.creature.SoaksUp == null)
                {
                    return $"{name}: '{spawn.creature.name}' lives beside the plants it soaks up, but soaks up nothing.";
                }
            }

            if (showWithin > hideBeyond)
            {
                return $"{name} shows its mushrooms again further than it hides them.";
            }

            return null;
        }

        /// <param name="threat">What the mushrooms watch and run from: the player.</param>
        /// <param name="view">The player's view: a creature soaks something up only when seen.</param>
        /// <param name="groundHeight">Height of the ground under a point.</param>
        /// <param name="absorbable">What a creature that soaks something up may soak up: the water ferns.</param>
        /// <param name="firstMet">The plants the player meets first: one lives beside each of them, whatever the share.</param>
        /// <param name="dropLoot">Puts on the ground, at a point, what a felled creature lets go of.</param>
        public void Populate(
            Transform threat,
            Transform view,
            Func<Vector3, float> groundHeight,
            System.Random random,
            IReadOnlyList<IAbsorbable> absorbable,
            IReadOnlyCollection<IAbsorbable> firstMet,
            Action<ItemDefinition, Vector3> dropLoot)
        {
            player = threat != null ? threat : throw new ArgumentNullException(nameof(threat));
            sources = absorbable ?? throw new ArgumentNullException(nameof(absorbable));
            playerView = view != null ? view : throw new ArgumentNullException(nameof(view));
            loot = dropLoot ?? throw new ArgumentNullException(nameof(dropLoot));
            foreach (var spawn in spawns)
            {
                foreach (var home in spawn.homes)
                {
                    SpawnOne(spawn.creature, home, threat, groundHeight, random);
                }

                if (spawn.shareOfSources <= 0f)
                {
                    continue;
                }

                foreach (var source in sources)
                {
                    // Always drawn, so the first plants met do not change which others have one.
                    var drawn = random.NextDouble() < spawn.shareOfSources;
                    if (source.Yield == spawn.creature.SoaksUp && (drawn || firstMet.Contains(source)))
                    {
                        SpawnOne(spawn.creature, HomeBeside(source.Position, spawn.distanceFromSource, random), threat, groundHeight, random);
                    }
                }
            }

            ShowNearest();
        }

        private void Update()
        {
            if (player == null)
            {
                return;
            }

            checkLeft -= Time.deltaTime;
            if (checkLeft > 0f)
            {
                return;
            }

            checkLeft = SecondsBetweenChecks;
            ShowNearest();
        }

        /// <summary>
        /// Shows the free mushrooms nearest the player, at most <see cref="maxShown"/> and
        /// none beyond the distances; hides the others. Felled and caught ones are left alone.
        /// </summary>
        private void ShowNearest()
        {
            var from = player.position;
            nearest.Clear();
            foreach (var mushroom in mushrooms)
            {
                if (mushroom != null && mushroom.IsFree)
                {
                    nearest.Add(mushroom);
                }
            }

            nearest.Sort((a, b) => FlatSqrDistance(a.transform.position, from).CompareTo(FlatSqrDistance(b.transform.position, from)));
            for (var rank = 0; rank < nearest.Count; rank++)
            {
                var mushroom = nearest[rank];
                var shown = mushroom.gameObject.activeSelf;
                var limit = shown ? hideBeyond : showWithin;
                var show = rank < maxShown && FlatSqrDistance(mushroom.transform.position, from) <= limit * limit;
                if (show != shown)
                {
                    mushroom.gameObject.SetActive(show);
                }
            }
        }

        private static float FlatSqrDistance(Vector3 a, Vector3 b)
        {
            var offset = a - b;
            offset.y = 0f;
            return offset.sqrMagnitude;
        }

        /// <summary>A point at <paramref name="distance"/> from <paramref name="source"/>, inside the area, clear of trunks and rocks when one is found.</summary>
        private Vector2 HomeBeside(Vector3 source, float distance, System.Random random)
        {
            var home = Vector2.zero;
            for (var attempt = 0; attempt < HomeTries; attempt++)
            {
                var angle = (float)random.NextDouble() * 2f * Mathf.PI;
                home = new Vector2(
                    Mathf.Clamp(source.x + Mathf.Cos(angle) * distance, area.xMin, area.xMax),
                    Mathf.Clamp(source.z + Mathf.Sin(angle) * distance, area.yMin, area.yMax));
                var ground = new Vector3(home.x, source.y, home.y);
                if (!ForestResourceSpots.TouchesTrunkOrRock(ground))
                {
                    break;
                }
            }

            return home;
        }

        private void OnDestroy()
        {
            foreach (var mushroom in mushrooms)
            {
                if (mushroom != null)
                {
                    mushroom.Felled -= OnFelled;
                }
            }
        }

        /// <summary>A felled creature lets go of its water if it drank: a dry one leaves nothing.</summary>
        private void OnFelled(WildMushroom mushroom)
        {
            var drop = species[mushroom].WetDrop;
            if (mushroom.IsWet && drop != null)
            {
                loot(drop, mushroom.transform.position);
            }
        }

        private void SpawnOne(CreatureDefinition creature, Vector2 home, Transform threat, Func<Vector3, float> groundHeight, System.Random random)
        {
            var mushroomObject = new GameObject($"Mushroom ({creature.name})");
            mushroomObject.transform.SetParent(transform, false);
            var position = new Vector3(home.x, 0f, home.y);
            position.y = groundHeight(position);
            mushroomObject.transform.position = position;

            var mushroom = mushroomObject.AddComponent<WildMushroom>();
            mushroom.InitializeWithModel(creature.Profile, threat, area, creature.FleeSpeed, creature.Model);
            mushroom.FollowGround(groundHeight);
            mushroom.EnableWander(new MushroomWander(
                position,
                creature.WanderRadius,
                creature.WanderSpeed,
                creature.WanderPauseSeconds,
                new System.Random(random.Next())));

            mushroom.EnableHealth(new CreatureHealth(creature.MaxHealth));
            mushroom.Felled += OnFelled;
            if (creature.SoaksUp != null)
            {
                var look = new WetLook(
                    mushroom.Model.GetComponentsInChildren<Renderer>(),
                    creature.WetPartMaterial,
                    creature.WetTint,
                    creature.WetSmoothness);
                mushroom.EnableSoak(
                    new WildSoak(creature.SoakReach, creature.SoakSeconds, creature.SoakArrivalDistance, creature.SoaksUp),
                    sources,
                    creature.WanderSpeed,
                    look,
                    playerView,
                    creature.SoaksWhenPlayerWithin,
                    creature.SoaksWhenSeenWithinDegrees);
            }

            mushrooms.Add(mushroom);
            species.Add(mushroom, creature);
        }
    }
}
