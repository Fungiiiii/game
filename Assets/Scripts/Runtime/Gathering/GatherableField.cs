using System;
using System.Collections.Generic;
using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using Fungiiiii.Outlines;
using UnityEngine;

namespace Fungiiiii.Gathering
{
    /// <summary>Where one gatherable can stand, and how it is posed there.</summary>
    internal readonly struct GatherableSpot
    {
        /// <param name="dropHeight">From how high it falls when it appears, under a tree; 0 to grow out of the ground.</param>
        public GatherableSpot(Vector3 position, Quaternion rotation, Vector3 scale, float dropHeight = 0f)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
            DropHeight = dropHeight;
        }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }

        public Vector3 Scale { get; }

        /// <summary>From how high, in metres, it falls when it appears; 0 when it grows out of the ground.</summary>
        public float DropHeight { get; }
    }

    /// <summary>
    /// A player as the fields see them: the body gives the position, because the camera is
    /// only moved to it in LateUpdate; the camera gives where they look.
    /// </summary>
    internal readonly struct FieldPlayer
    {
        public FieldPlayer(Transform body, Camera view)
        {
            Body = body;
            View = view;
        }

        public Transform Body { get; }

        public Camera View { get; }

        public ResourceViewer ToViewer()
        {
            return new ResourceViewer(Body.position, View.transform.forward);
        }
    }

    /// <summary>
    /// One kind of resource around the players: it runs the <see cref="ResourceSpawner"/>
    /// of the kind and shows a <see cref="Gatherable"/> on every spot the spawner fills,
    /// in one of its model variants drawn at random. Gathered or forgotten objects are kept
    /// and shown again later, instead of being destroyed and rebuilt.
    /// </summary>
    internal sealed class GatherableField : MonoBehaviour
    {
        private readonly List<Gatherable> gatherables = new List<Gatherable>();
        private readonly Dictionary<Gatherable, int> variantOf = new Dictionary<Gatherable, int>();
        private readonly Dictionary<int, Gatherable> shown = new Dictionary<int, Gatherable>();
        private readonly List<int> spawned = new List<int>();
        private readonly List<int> despawned = new List<int>();
        private readonly List<ResourceViewer> viewers = new List<ResourceViewer>();

        private ItemDefinition item;
        private IReadOnlyList<GameObject> variants;
        private Stack<Gatherable>[] hidden;
        private IReadOnlyList<GatherableSpot> spots;
        private ResourceSpawner spawner;
        private System.Random variantRandom;
        private IReadOnlyList<FieldPlayer> players;
        private float reach;
        private ItemOutlineFactory outlines;
        private Action<Gatherable> created;

        /// <summary>Every gatherable built so far, on the map or waiting for a spot.</summary>
        public IReadOnlyList<Gatherable> Gatherables => gatherables;

        /// <param name="gatherablePrefabs">Model variants: each appearance draws one.</param>
        /// <param name="onCreated">Called once for each gatherable the field builds.</param>
        public void Initialize(
            ItemDefinition gatheredItem,
            IReadOnlyList<GameObject> gatherablePrefabs,
            IReadOnlyList<GatherableSpot> fieldSpots,
            ResourceSpawner.Settings settings,
            System.Random random,
            IReadOnlyList<FieldPlayer> fieldPlayers,
            float interactionReach,
            ItemOutlineFactory outlineFactory,
            Action<Gatherable> onCreated)
        {
            if (gatherablePrefabs == null || gatherablePrefabs.Count == 0)
            {
                throw new ArgumentException("A field needs at least one model.", nameof(gatherablePrefabs));
            }

            item = gatheredItem;
            variants = gatherablePrefabs;
            spots = fieldSpots;
            players = fieldPlayers;
            reach = interactionReach;
            outlines = outlineFactory;
            created = onCreated;

            hidden = new Stack<Gatherable>[variants.Count];
            for (var variant = 0; variant < hidden.Length; variant++)
            {
                hidden[variant] = new Stack<Gatherable>();
            }

            var positions = new Vector3[fieldSpots.Count];
            for (var index = 0; index < positions.Length; index++)
            {
                positions[index] = fieldSpots[index].Position;
            }

            // The variant draw has its own stream: adding a variant does not move any spot.
            spawner = new ResourceSpawner(positions, settings, new System.Random(random.Next()));
            variantRandom = new System.Random(random.Next());
            Fill();
        }

        private void Update()
        {
            if (spawner == null)
            {
                return;
            }

            spawned.Clear();
            despawned.Clear();
            spawner.Tick(Time.deltaTime, Viewers(), spawned, despawned);

            foreach (var spot in despawned)
            {
                Hide(spot);
            }

            ShowSpawned(animate: true);
        }

        private void Fill()
        {
            spawned.Clear();
            spawner.Fill(Viewers(), spawned);
            // The screen is black: everything is simply there when it lights up.
            ShowSpawned(animate: false);
        }

        private List<ResourceViewer> Viewers()
        {
            viewers.Clear();
            foreach (var player in players)
            {
                if (player.Body != null && player.View != null)
                {
                    viewers.Add(player.ToViewer());
                }
            }

            return viewers;
        }

        /// <param name="animate">Whether what appears grows or falls in, rather than being there at once.</param>
        private void ShowSpawned(bool animate)
        {
            foreach (var spot in spawned)
            {
                var pose = spots[spot];
                var variant = variantRandom.Next(variants.Count);
                var gatherable = hidden[variant].Count > 0 ? hidden[variant].Pop() : Create(variant);
                gatherable.Appear(spot, pose);
                if (animate)
                {
                    // The golden ratio spreads the tumble axes of neighbouring spots apart.
                    gatherable.Arrive(pose.DropHeight, Mathf.Repeat(spot * 0.618034f, 1f));
                }

                shown[spot] = gatherable;
            }
        }

        /// <summary>Hides what stands on a spot without giving anything.</summary>
        private void Hide(int spot)
        {
            if (!shown.TryGetValue(spot, out var gatherable))
            {
                return;
            }

            shown.Remove(spot);
            gatherable.Disappear();
            hidden[variantOf[gatherable]].Push(gatherable);
        }

        private Gatherable Create(int variant)
        {
            var prefab = variants[variant];
            var gatherableObject = new GameObject($"Gatherable ({item.Id})");
            gatherableObject.transform.SetParent(transform, false);

            var visual = Instantiate(prefab, gatherableObject.transform);
            visual.name = prefab.name;
            visual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            var gatherable = gatherableObject.AddComponent<Gatherable>();
            gatherable.Initialize(item, outlines.Create(visual), reach);
            gatherable.Gathered += HandleGathered;
            gatherables.Add(gatherable);
            variantOf.Add(gatherable, variant);
            created?.Invoke(gatherable);
            return gatherable;
        }

        private void HandleGathered(Gatherable gatherable)
        {
            spawner.Release(gatherable.Spot);
            shown.Remove(gatherable.Spot);
            hidden[variantOf[gatherable]].Push(gatherable);
        }
    }
}
