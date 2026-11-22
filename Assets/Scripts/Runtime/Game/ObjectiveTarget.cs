using System;
using System.Collections.Generic;
using Fungiiiii.Champimaison;
using Fungiiiii.Crafting;
using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Game
{
    /// <summary>
    /// Where the compass's mark points for the objective now, as in other games: the seed at
    /// the foot of the giant tree; the nearest mole hole to plant it in; the Champimaison
    /// while it waits for what the player carries, or for its thread before the net; the
    /// nearest full fern while water is missing. Nothing when there is nowhere in
    /// particular to go: sticks and mushrooms are all around.
    /// </summary>
    internal sealed class ObjectiveTarget
    {
        private readonly GameObjectives objectives;
        private readonly Transform player;
        private readonly Transform seed;
        private readonly MoleHoles moleHoles;
        private readonly IReadOnlyList<RefillingPlant> ferns;
        private readonly PlayerInventory inventory;
        private readonly CraftingRecipe net;

        /// <param name="seedOnTheGround">The seed lying where it fell; gone once picked up.</param>
        /// <param name="netRecipe">The net's recipe: before it, the Champimaison's thread is missing.</param>
        public ObjectiveTarget(
            GameObjectives gameObjectives,
            Transform playerTransform,
            Transform seedOnTheGround,
            MoleHoles holes,
            IReadOnlyList<RefillingPlant> waterFerns,
            PlayerInventory playerInventory,
            CraftingRecipe netRecipe)
        {
            objectives = gameObjectives ?? throw new ArgumentNullException(nameof(gameObjectives));
            player = playerTransform != null ? playerTransform : throw new ArgumentNullException(nameof(playerTransform));
            seed = seedOnTheGround;
            moleHoles = holes != null ? holes : throw new ArgumentNullException(nameof(holes));
            ferns = waterFerns ?? throw new ArgumentNullException(nameof(waterFerns));
            inventory = playerInventory ?? throw new ArgumentNullException(nameof(playerInventory));
            net = netRecipe != null ? netRecipe : throw new ArgumentNullException(nameof(netRecipe));
        }

        /// <summary>Where to go now, or null when nowhere in particular.</summary>
        public Vector3? Current()
        {
            var champimaison = moleHoles.Champimaison;
            switch (objectives.Current)
            {
                case Objective.FindSeed:
                    return seed != null ? seed.position : (Vector3?)null;
                case Objective.PlantSeed:
                    return NearestHole();
                case Objective.AwakenSeed:
                case Objective.GrowChampimaison:
                    return IsWaterMissing(champimaison) ? NearestFern() : champimaison.transform.position;
                case Objective.CraftNet:
                    return Crafter.CanCraft(inventory, net) ? (Vector3?)null : champimaison.transform.position;
                case Objective.EnterChampimaison:
                    return champimaison.transform.position;
                default:
                    return null;
            }
        }

        private bool IsWaterMissing(FungalHole champimaison)
        {
            if (ferns.Count == 0)
            {
                return false;
            }

            var water = ferns[0].Yield;
            var progress = champimaison.Network.Progress;
            for (var line = 0; line < progress.LineCount; line++)
            {
                var required = progress.Required(line);
                if (required.Item == water && progress.Deposited(line) + inventory.Count(water.Id) < required.Amount)
                {
                    return true;
                }
            }

            return false;
        }

        private Vector3? NearestHole()
        {
            Vector3? nearest = null;
            var nearestSquared = float.MaxValue;
            foreach (var hole in moleHoles.Holes)
            {
                var squared = FlatSqrDistance(hole.transform.position);
                if (squared < nearestSquared)
                {
                    nearest = hole.transform.position;
                    nearestSquared = squared;
                }
            }

            return nearest;
        }

        /// <summary>The nearest full fern; the nearest fern when none is full.</summary>
        private Vector3? NearestFern()
        {
            Vector3? nearest = null;
            var nearestSquared = float.MaxValue;
            var foundFull = false;
            foreach (var fern in ferns)
            {
                var squared = FlatSqrDistance(fern.Position);
                if ((fern.IsFull && !foundFull) || (fern.IsFull == foundFull && squared < nearestSquared))
                {
                    nearest = fern.Position;
                    nearestSquared = squared;
                    foundFull |= fern.IsFull;
                }
            }

            return nearest;
        }

        private float FlatSqrDistance(Vector3 point)
        {
            var offset = point - player.position;
            offset.y = 0f;
            return offset.sqrMagnitude;
        }
    }
}
