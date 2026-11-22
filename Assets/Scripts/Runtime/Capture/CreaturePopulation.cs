using System;
using System.Collections.Generic;
using Fungiiiii.Combat;
using Fungiiiii.Companions;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// The mushrooms to catch in a level: which species live where, set in the Inspector.
    /// Each one wanders around its home and runs from the net; a new species is a
    /// <see cref="CreatureDefinition"/> and a line here, no code.
    /// </summary>
    internal sealed class CreaturePopulation : MonoBehaviour
    {
        [Serializable]
        private struct Spawn
        {
            public CreatureDefinition creature;

            [Tooltip("Homes, in world coordinates on the ground (x, z): one mushroom wanders around each.")]
            public Vector2[] homes;
        }

        [SerializeField]
        private Spawn[] spawns = new Spawn[0];

        [Tooltip("The mushrooms cannot flee out of this ground rectangle (x, z, width, depth).")]
        [SerializeField]
        private Rect area = new Rect(-30f, -260f, 80f, 65f);

        private readonly List<WildMushroom> mushrooms = new List<WildMushroom>();
        private readonly Dictionary<WildMushroom, CreatureDefinition> species = new Dictionary<WildMushroom, CreatureDefinition>();
        private IReadOnlyList<IAbsorbable> sources;
        private Action<ItemDefinition, Vector3> loot;

        public IReadOnlyList<WildMushroom> Mushrooms => mushrooms;

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
            }

            return null;
        }

        /// <param name="threat">What the mushrooms watch and run from: the player.</param>
        /// <param name="groundHeight">Height of the ground under a point.</param>
        /// <param name="absorbable">What a creature that soaks something up may soak up: the water ferns.</param>
        /// <param name="dropLoot">Puts on the ground, at a point, what a felled creature lets go of.</param>
        public void Populate(
            Transform threat,
            Func<Vector3, float> groundHeight,
            System.Random random,
            IReadOnlyList<IAbsorbable> absorbable,
            Action<ItemDefinition, Vector3> dropLoot)
        {
            sources = absorbable ?? throw new ArgumentNullException(nameof(absorbable));
            loot = dropLoot ?? throw new ArgumentNullException(nameof(dropLoot));
            foreach (var spawn in spawns)
            {
                foreach (var home in spawn.homes)
                {
                    SpawnOne(spawn.creature, home, threat, groundHeight, random);
                }
            }
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
                    look);
            }

            mushrooms.Add(mushroom);
            species.Add(mushroom, creature);
        }
    }
}
