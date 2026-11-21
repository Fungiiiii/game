using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Gathering
{
    /// <summary>A player as the spawner sees it: where they stand and where they look.</summary>
    public readonly struct ResourceViewer
    {
        public ResourceViewer(Vector3 position, Vector3 forward, float halfViewAngleDegrees)
        {
            Position = position;
            Forward = forward;
            HalfViewAngleDegrees = halfViewAngleDegrees;
        }

        public Vector3 Position { get; }

        /// <summary>Direction of the view. Only its part along the ground counts.</summary>
        public Vector3 Forward { get; }

        /// <summary>Half the width of what the player sees, around <see cref="Forward"/>, on the ground.</summary>
        public float HalfViewAngleDegrees { get; }
    }

    /// <summary>
    /// Keeps a few resources of one kind around each player, never on the whole map.
    /// Every tick it fills free spots near a player who has fewer than the target number
    /// within the bubble radius: never close to a player, and never in any player's field
    /// of view up to the sight distance, where the forest's fog ends: it does not hide a
    /// resource that pops in closer.
    /// Resources far from every player go away and free their spot, so the map does not
    /// fill up behind them.
    ///
    /// Distances are measured on the ground: a spot up a slope is not further away.
    /// </summary>
    public sealed class ResourceSpawner
    {
        /// <summary>Added to a player's half field of view, so nothing pops in at its edge while they turn.</summary>
        public const float SightMarginDegrees = 10f;

        private readonly IReadOnlyList<Vector3> spots;
        private readonly Settings settings;
        private readonly System.Random random;
        private readonly bool[] occupied;
        private readonly List<int> alive = new List<int>();
        private float sinceTick;

        public ResourceSpawner(IReadOnlyList<Vector3> spots, Settings settings, System.Random random)
        {
            this.spots = spots ?? throw new ArgumentNullException(nameof(spots));
            this.settings = settings;
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            occupied = new bool[spots.Count];
        }

        /// <summary>How many resources are on the map right now.</summary>
        public int AliveCount => alive.Count;

        public bool IsOccupied(int spot) => occupied[spot];

        /// <summary>
        /// Fills the bubble of every player at once, closer than a tick would and even in
        /// view: used while the screen is black, when nothing can be seen popping in.
        /// </summary>
        /// <param name="spawned">Receives the spots that were filled.</param>
        public void Fill(IReadOnlyList<ResourceViewer> viewers, List<int> spawned)
        {
            foreach (var viewer in viewers)
            {
                while (CountNear(viewer) < settings.TargetNearPlayer && alive.Count < settings.MaxAlive)
                {
                    var spot = DrawSpot(viewer, viewers, settings.StartMinPlayerDistance, ignoreSight: true);
                    if (spot < 0)
                    {
                        break;
                    }

                    Occupy(spot);
                    spawned.Add(spot);
                }
            }
        }

        /// <summary>
        /// Advances the clock. On each tick, resources far from every player go away, then
        /// each player whose bubble is short gets up to the per-tick count.
        /// </summary>
        /// <param name="spawned">Receives the spots that were filled.</param>
        /// <param name="despawned">Receives the spots whose resource went away.</param>
        public void Tick(float deltaTime, IReadOnlyList<ResourceViewer> viewers, List<int> spawned, List<int> despawned)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            sinceTick += deltaTime;
            while (sinceTick >= settings.TickSeconds)
            {
                sinceTick -= settings.TickSeconds;
                DespawnForgotten(viewers, despawned);
                SpawnOneTick(viewers, spawned);
            }
        }

        /// <summary>Frees the spot of a gathered resource. Does nothing if it was already free.</summary>
        public void Release(int spot)
        {
            if (!occupied[spot])
            {
                return;
            }

            occupied[spot] = false;
            alive.Remove(spot);
        }

        /// <summary>Empties the map and restarts the clock.</summary>
        public void Clear()
        {
            foreach (var spot in alive)
            {
                occupied[spot] = false;
            }

            alive.Clear();
            sinceTick = 0f;
        }

        /// <summary>Whether a point is in a player's field of view, margin included, and within sight, on the ground.</summary>
        public static bool CanSee(ResourceViewer viewer, Vector3 point, float sightDistance)
        {
            var flat = Flat(point - viewer.Position);
            if (flat.sqrMagnitude > sightDistance * sightDistance)
            {
                return false;
            }

            var forward = Flat(viewer.Forward);
            if (forward.sqrMagnitude <= 0f || flat.sqrMagnitude <= 0f)
            {
                // Looking straight down or up, or standing on it: assume it is seen.
                return true;
            }

            return Vector3.Angle(forward, flat) <= viewer.HalfViewAngleDegrees + SightMarginDegrees;
        }

        private void DespawnForgotten(IReadOnlyList<ResourceViewer> viewers, List<int> despawned)
        {
            var despawnSquared = settings.DespawnDistance * settings.DespawnDistance;

            for (var index = alive.Count - 1; index >= 0; index--)
            {
                var spot = alive[index];
                var remembered = false;
                foreach (var viewer in viewers)
                {
                    if (Flat(spots[spot] - viewer.Position).sqrMagnitude <= despawnSquared)
                    {
                        remembered = true;
                        break;
                    }
                }

                if (!remembered)
                {
                    occupied[spot] = false;
                    alive.RemoveAt(index);
                    despawned.Add(spot);
                }
            }
        }

        private void SpawnOneTick(IReadOnlyList<ResourceViewer> viewers, List<int> spawned)
        {
            foreach (var viewer in viewers)
            {
                for (var count = 0; count < settings.SpawnsPerTick; count++)
                {
                    if (alive.Count >= settings.MaxAlive || CountNear(viewer) >= settings.TargetNearPlayer)
                    {
                        break;
                    }

                    var spot = DrawSpot(viewer, viewers, settings.MinPlayerDistance, ignoreSight: false);
                    if (spot < 0)
                    {
                        break;
                    }

                    Occupy(spot);
                    spawned.Add(spot);
                }
            }
        }

        private int CountNear(ResourceViewer viewer)
        {
            var radiusSquared = settings.Radius * settings.Radius;
            var count = 0;
            foreach (var spot in alive)
            {
                if (Flat(spots[spot] - viewer.Position).sqrMagnitude <= radiusSquared)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// A free spot drawn at random among those in the bubble of <paramref name="owner"/>,
        /// no closer than <paramref name="minDistance"/> to any player and, unless
        /// <paramref name="ignoreSight"/>, out of every player's view. -1 when there is none.
        /// </summary>
        private int DrawSpot(ResourceViewer owner, IReadOnlyList<ResourceViewer> viewers, float minDistance, bool ignoreSight)
        {
            var radiusSquared = settings.Radius * settings.Radius;
            var minSquared = minDistance * minDistance;
            var chosen = -1;
            var candidates = 0;

            for (var spot = 0; spot < spots.Count; spot++)
            {
                if (occupied[spot])
                {
                    continue;
                }

                var point = spots[spot];
                if (Flat(point - owner.Position).sqrMagnitude > radiusSquared || !IsAllowed(point, viewers, minSquared, ignoreSight))
                {
                    continue;
                }

                // Reservoir sampling: every candidate has the same chance, without a list.
                candidates++;
                if (random.Next(candidates) == 0)
                {
                    chosen = spot;
                }
            }

            return chosen;
        }

        private bool IsAllowed(Vector3 point, IReadOnlyList<ResourceViewer> viewers, float minSquared, bool ignoreSight)
        {
            var sightDistance = settings.SightDistance;
            foreach (var viewer in viewers)
            {
                if (Flat(point - viewer.Position).sqrMagnitude < minSquared)
                {
                    return false;
                }

                if (!ignoreSight && CanSee(viewer, point, sightDistance))
                {
                    return false;
                }
            }

            return true;
        }

        private void Occupy(int spot)
        {
            occupied[spot] = true;
            alive.Add(spot);
        }

        private static Vector3 Flat(Vector3 vector)
        {
            vector.y = 0f;
            return vector;
        }

        /// <summary>How many resources of a kind each player has around them, and how they come and go.</summary>
        public readonly struct Settings
        {
            public Settings(
                float tickSeconds,
                int spawnsPerTick,
                int targetNearPlayer,
                float radius,
                float minPlayerDistance,
                float startMinPlayerDistance,
                float despawnDistance,
                float sightDistance,
                int maxAlive)
            {
                if (float.IsNaN(tickSeconds) || tickSeconds <= 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(tickSeconds));
                }

                if (spawnsPerTick < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(spawnsPerTick));
                }

                if (targetNearPlayer < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(targetNearPlayer));
                }

                if (float.IsNaN(radius) || radius <= 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(radius));
                }

                if (float.IsNaN(minPlayerDistance) || minPlayerDistance < 0f || minPlayerDistance > radius)
                {
                    throw new ArgumentOutOfRangeException(nameof(minPlayerDistance));
                }

                if (float.IsNaN(startMinPlayerDistance) || startMinPlayerDistance < 0f || startMinPlayerDistance > radius)
                {
                    throw new ArgumentOutOfRangeException(nameof(startMinPlayerDistance));
                }

                // Despawning inside the bubble would remove what was just spawned there.
                if (float.IsNaN(despawnDistance) || despawnDistance < radius)
                {
                    throw new ArgumentOutOfRangeException(nameof(despawnDistance));
                }

                if (float.IsNaN(sightDistance) || sightDistance < 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(sightDistance));
                }

                if (maxAlive < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(maxAlive));
                }

                TickSeconds = tickSeconds;
                SpawnsPerTick = spawnsPerTick;
                TargetNearPlayer = targetNearPlayer;
                Radius = radius;
                MinPlayerDistance = minPlayerDistance;
                StartMinPlayerDistance = startMinPlayerDistance;
                DespawnDistance = despawnDistance;
                SightDistance = sightDistance;
                MaxAlive = maxAlive;
            }

            /// <summary>Seconds between two ticks.</summary>
            public float TickSeconds { get; }

            /// <summary>Most resources that appear around one player on one tick.</summary>
            public int SpawnsPerTick { get; }

            /// <summary>How many resources each player should have within <see cref="Radius"/>.</summary>
            public int TargetNearPlayer { get; }

            /// <summary>Radius of a player's bubble, in metres on the ground.</summary>
            public float Radius { get; }

            /// <summary>Nothing appears closer to any player than this on a tick, in metres.</summary>
            public float MinPlayerDistance { get; }

            /// <summary>Nothing appears closer to any player than this when the bubbles are filled at once.</summary>
            public float StartMinPlayerDistance { get; }

            /// <summary>A resource further than this from every player goes away. At least the bubble radius.</summary>
            public float DespawnDistance { get; }

            /// <summary>Beyond this a player sees nothing, in metres on the ground: where the fog ends.</summary>
            public float SightDistance { get; }

            /// <summary>Most resources of the kind on the map at once, all players together.</summary>
            public int MaxAlive { get; }
        }
    }
}
