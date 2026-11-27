using System;
using System.Collections.Generic;
using System.Linq;
using Fungiiiii.Companions;
using Fungiiiii.Gathering;
using Fungiiiii.Interaction;
using Fungiiiii.Outlines;
using Fungiiiii.Runtime.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fungiiiii.Gathering
{
    /// <summary>
    /// The plants of the level that fill up by chance, such as the water ferns of the mist:
    /// which kind grows where, set in the Inspector. They are scattered once, when the level
    /// is built, out of the paths and away from trunks and rocks, and stay where they are;
    /// this counts time for all of them. The decor each kind could be mistaken for (the decor
    /// fern) is taken out of the level first, so that every plant of its look is one that
    /// fills. A new kind is a <see cref="RefillingPlantDefinition"/> and a line here, no code.
    /// </summary>
    internal sealed class RefillingPlants : MonoBehaviour
    {
        /// <summary>Plants go 2 cm into the ground, so none floats on a slope.</summary>
        private const float Sink = 0.02f;

        [Serializable]
        private struct Growth
        {
            public RefillingPlantDefinition plant;

            [Tooltip("Ground rectangle (x, z, width, depth) where it grows: the water fern grows in the mist.")]
            public Rect area;
        }

        [SerializeField]
        private Growth[] growths = new Growth[0];

        [Tooltip("Where the player comes from (x, z): the plants nearest to it are the first met, full at the start.")]
        [SerializeField]
        private Vector2 firstMetFrom = new Vector2(25f, -105f);

        [Tooltip("How many plants are the first met, full at the start.")]
        [SerializeField, Min(0)]
        private int firstMetCount = 3;

        private readonly List<RefillingPlant> plants = new List<RefillingPlant>();
        private readonly List<RefillingPlant> firstMet = new List<RefillingPlant>();
        private readonly List<IAbsorbable> absorbable = new List<IAbsorbable>();

        /// <summary>Every plant of the level.</summary>
        public IReadOnlyList<RefillingPlant> Plants => plants;

        /// <summary>The same plants, as what companions can soak up.</summary>
        public IReadOnlyList<IAbsorbable> Absorbable => absorbable;

        /// <summary>The plants nearest to where the player comes from: full at the start, so a creature is seen drinking at them.</summary>
        public IReadOnlyList<RefillingPlant> FirstMet => firstMet;

        /// <summary>What is missing for the component to work, or null when it is sound.</summary>
        public string FindProblem()
        {
            foreach (var growth in growths)
            {
                if (growth.plant == null)
                {
                    return $"{name} has a line without plant.";
                }

                if (growth.plant.FindProblem() is { } problem)
                {
                    return problem;
                }

                if (!growth.plant.Prefab.TryGetComponent<RefillingPlant>(out var prefabPlant))
                {
                    return $"The prefab of '{growth.plant.name}' has no RefillingPlant on its root.";
                }

                if (prefabPlant.FindProblem() is { } prefabProblem)
                {
                    return prefabProblem;
                }
            }

            return null;
        }

        /// <param name="forest">The level's scene: its hand-placed look-alikes are hidden.</param>
        /// <param name="session">The ground they grow on, for this session: its look-alikes are taken out of it.</param>
        /// <param name="usable">What the interact control can act on: every plant joins it.</param>
        public void Populate(Scene forest, SessionTerrain session, ItemOutlineFactory outlines, ICollection<IInteractable> usable, System.Random random)
        {
            var terrain = session.Terrain;
            foreach (var growth in growths)
            {
                foreach (var lookAlike in growth.plant.LookAlikes)
                {
                    session.TakeTrees(lookAlike);
                    ForestResourceSpots.TakeLandmarks(forest, lookAlike.name);
                }
            }

            var paths = new List<ExclusionCircle>();
            foreach (var path in FindObjectsByType<ScatterExclusionPath>(FindObjectsSortMode.InstanceID))
            {
                paths.AddRange(path.ToCircles());
            }

            var spots = new List<GatherableSpot>();
            foreach (var growth in growths)
            {
                spots.Clear();
                var plant = growth.plant;
                ForestResourceSpots.ScatterMushroomSpots(
                    terrain,
                    growth.area,
                    plant.Spacing,
                    plant.MaxSlope,
                    paths,
                    new MushroomPose(plant.MinScale, plant.MaxScale, Sink),
                    new System.Random(random.Next()),
                    spots);

                foreach (var spot in spots)
                {
                    if (!ForestResourceSpots.TouchesTrunkOrRock(spot.Position))
                    {
                        Grow(plant, spot, outlines, usable, random);
                    }
                }

                if (spots.Count == 0)
                {
                    Debug.LogWarning($"{name} found no spot for {plant.name}: none will grow.", this);
                }
            }

            PickFirstMet();
        }

        private void PickFirstMet()
        {
            var from = new Vector3(firstMetFrom.x, 0f, firstMetFrom.y);
            firstMet.AddRange(plants
                .OrderBy(plant => Vector3.ProjectOnPlane(plant.Position - from, Vector3.up).sqrMagnitude)
                .Take(firstMetCount));
            foreach (var plant in firstMet)
            {
                plant.Restart(1f);
            }
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            foreach (var plant in plants)
            {
                plant.Tick(deltaTime);
            }
        }

        private void Grow(RefillingPlantDefinition definition, GatherableSpot spot, ItemOutlineFactory outlines, ICollection<IInteractable> usable, System.Random random)
        {
            var instance = Instantiate(definition.Prefab, spot.Position, spot.Rotation, transform);
            instance.name = definition.Prefab.name;
            instance.transform.localScale = spot.Scale;

            var plant = instance.GetComponent<RefillingPlant>();
            var refill = new ChanceRefill(definition.RefillIntervalSeconds, definition.RefillChance, new System.Random(random.Next()));
            plant.Initialize(definition, refill, outlines.Create(plant.Model));
            plant.Restart(definition.FullAtStart);

            plants.Add(plant);
            absorbable.Add(plant);
            usable.Add(plant);
        }
    }
}
