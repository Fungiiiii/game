using System;
using System.Collections.Generic;
using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Demo
{
    /// <summary>Where one gatherable can stand, and how it is posed there.</summary>
    internal readonly struct GatherableSpot
    {
        public GatherableSpot(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }

        public Vector3 Scale { get; }
    }

    /// <summary>
    /// One kind of resource over the whole map: it runs the <see cref="ResourceSpawner"/>
    /// of the kind and shows a <see cref="Gatherable"/> on every spot the spawner fills.
    /// Gathered objects are kept and shown again on the next spot, instead of being
    /// destroyed and rebuilt.
    /// </summary>
    internal sealed class GatherableField : MonoBehaviour
    {
        private readonly List<Gatherable> gatherables = new List<Gatherable>();
        private readonly Stack<Gatherable> hidden = new Stack<Gatherable>();
        private readonly List<int> spawned = new List<int>();

        private InventoryItemDefinition item;
        private GameObject prefab;
        private IReadOnlyList<GatherableSpot> spots;
        private ResourceSpawner spawner;
        private Transform player;
        private float reach;
        private ItemOutlineFactory outlines;
        private Action<Gatherable> created;

        /// <summary>Every gatherable built so far, on the map or waiting for a spot.</summary>
        public IReadOnlyList<Gatherable> Gatherables => gatherables;

        /// <param name="onCreated">Called once for each gatherable the field builds.</param>
        public void Initialize(
            InventoryItemDefinition gatheredItem,
            GameObject gatherablePrefab,
            IReadOnlyList<GatherableSpot> fieldSpots,
            ResourceSpawner.Settings settings,
            System.Random random,
            Transform playerTransform,
            float interactionReach,
            ItemOutlineFactory outlineFactory,
            Action<Gatherable> onCreated)
        {
            item = gatheredItem;
            prefab = gatherablePrefab;
            spots = fieldSpots;
            player = playerTransform;
            reach = interactionReach;
            outlines = outlineFactory;
            created = onCreated;

            var positions = new Vector3[fieldSpots.Count];
            for (var index = 0; index < positions.Length; index++)
            {
                positions[index] = fieldSpots[index].Position;
            }

            spawner = new ResourceSpawner(positions, settings, random);
            Fill();
        }

        /// <summary>Everything gone, then the map filled again, for a demo reset.</summary>
        public void ResetField()
        {
            foreach (var gatherable in gatherables)
            {
                if (gatherable.IsAvailable)
                {
                    gatherable.Disappear();
                    hidden.Push(gatherable);
                }
            }

            spawner.Clear();
            Fill();
        }

        private void Update()
        {
            if (spawner == null || player == null)
            {
                return;
            }

            spawned.Clear();
            spawner.Tick(Time.deltaTime, player.position, spawned);
            ShowSpawned();
        }

        private void Fill()
        {
            spawned.Clear();
            spawner.Fill(spawned);
            ShowSpawned();
        }

        private void ShowSpawned()
        {
            foreach (var spot in spawned)
            {
                var pose = spots[spot];
                var gatherable = hidden.Count > 0 ? hidden.Pop() : Create();
                gatherable.Appear(spot, pose.Position, pose.Rotation, pose.Scale);
            }
        }

        private Gatherable Create()
        {
            var gatherableObject = new GameObject($"Gatherable ({item.Id})");
            gatherableObject.transform.SetParent(transform, false);

            var visual = Instantiate(prefab, gatherableObject.transform);
            visual.name = prefab.name;
            visual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            var gatherable = gatherableObject.AddComponent<Gatherable>();
            gatherable.Initialize(item, outlines.Create(visual), reach);
            gatherable.Gathered += HandleGathered;
            gatherables.Add(gatherable);
            created?.Invoke(gatherable);
            return gatherable;
        }

        private void HandleGathered(Gatherable gatherable)
        {
            spawner.Release(gatherable.Spot);
            hidden.Push(gatherable);
        }
    }
}
