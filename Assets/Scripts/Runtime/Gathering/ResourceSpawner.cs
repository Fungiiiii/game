using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Gathering
{
    /// <summary>
    /// Decides where resources appear over a map: every tick it fills a few free spots,
    /// drawn at random and never close to the player, until a maximum is alive at once.
    /// A gathered resource frees its spot; the next one appears wherever the draw falls,
    /// not necessarily at the same place.
    /// </summary>
    public sealed class ResourceSpawner
    {
        /// <summary>Draws tried per spawn before giving up until the next tick.</summary>
        private const int DrawsPerSpawn = 24;

        private readonly IReadOnlyList<Vector3> spots;
        private readonly Settings settings;
        private readonly System.Random random;
        private readonly List<int> free;
        private readonly int[] freeIndex;
        private float sinceTick;

        public ResourceSpawner(IReadOnlyList<Vector3> spots, Settings settings, System.Random random)
        {
            this.spots = spots ?? throw new ArgumentNullException(nameof(spots));
            this.settings = settings;
            this.random = random ?? throw new ArgumentNullException(nameof(random));

            free = new List<int>(spots.Count);
            freeIndex = new int[spots.Count];
            for (var spot = 0; spot < spots.Count; spot++)
            {
                freeIndex[spot] = spot;
                free.Add(spot);
            }
        }

        /// <summary>How many resources are on the map right now.</summary>
        public int AliveCount => spots.Count - free.Count;

        public bool IsOccupied(int spot) => freeIndex[spot] < 0;

        /// <summary>
        /// Fills the map up to the maximum at once, wherever the player is: used before the
        /// game starts, when nothing can be seen popping in.
        /// </summary>
        /// <param name="spawned">Receives the spots that were filled.</param>
        public void Fill(List<int> spawned)
        {
            while (AliveCount < settings.MaxAlive && free.Count > 0)
            {
                var spot = free[random.Next(free.Count)];
                Occupy(spot);
                spawned.Add(spot);
            }
        }

        /// <summary>
        /// Advances the clock. On each tick, fills free spots far enough from the player,
        /// up to the per-tick count and the maximum alive.
        /// </summary>
        /// <param name="spawned">Receives the spots that were filled.</param>
        public void Tick(float deltaTime, Vector3 playerPosition, List<int> spawned)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            sinceTick += deltaTime;
            while (sinceTick >= settings.TickSeconds)
            {
                sinceTick -= settings.TickSeconds;
                SpawnOneTick(playerPosition, spawned);
            }
        }

        /// <summary>Frees the spot of a gathered resource. Does nothing if it was already free.</summary>
        public void Release(int spot)
        {
            if (freeIndex[spot] >= 0)
            {
                return;
            }

            freeIndex[spot] = free.Count;
            free.Add(spot);
        }

        /// <summary>Empties the map and restarts the clock.</summary>
        public void Clear()
        {
            free.Clear();
            for (var spot = 0; spot < spots.Count; spot++)
            {
                freeIndex[spot] = spot;
                free.Add(spot);
            }

            sinceTick = 0f;
        }

        private void SpawnOneTick(Vector3 playerPosition, List<int> spawned)
        {
            for (var count = 0; count < settings.SpawnsPerTick; count++)
            {
                if (AliveCount >= settings.MaxAlive || free.Count == 0)
                {
                    return;
                }

                var spot = DrawFarFreeSpot(playerPosition);
                if (spot < 0)
                {
                    return;
                }

                Occupy(spot);
                spawned.Add(spot);
            }
        }

        private int DrawFarFreeSpot(Vector3 playerPosition)
        {
            var minSquared = settings.MinPlayerDistance * settings.MinPlayerDistance;

            for (var draw = 0; draw < DrawsPerSpawn; draw++)
            {
                var spot = free[random.Next(free.Count)];

                // Measured on the ground: a spot up a slope is not further away.
                var flat = spots[spot] - playerPosition;
                flat.y = 0f;
                if (flat.sqrMagnitude >= minSquared)
                {
                    return spot;
                }
            }

            return -1;
        }

        private void Occupy(int spot)
        {
            var index = freeIndex[spot];
            var last = free[free.Count - 1];
            free[index] = last;
            freeIndex[last] = index;
            free.RemoveAt(free.Count - 1);
            freeIndex[spot] = -1;
        }

        /// <summary>How fast and how much a kind of resource appears.</summary>
        public readonly struct Settings
        {
            public Settings(float tickSeconds, int spawnsPerTick, int maxAlive, float minPlayerDistance)
            {
                if (float.IsNaN(tickSeconds) || tickSeconds <= 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(tickSeconds));
                }

                if (spawnsPerTick < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(spawnsPerTick));
                }

                if (maxAlive < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(maxAlive));
                }

                if (float.IsNaN(minPlayerDistance) || minPlayerDistance < 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(minPlayerDistance));
                }

                TickSeconds = tickSeconds;
                SpawnsPerTick = spawnsPerTick;
                MaxAlive = maxAlive;
                MinPlayerDistance = minPlayerDistance;
            }

            /// <summary>Seconds between two ticks.</summary>
            public float TickSeconds { get; }

            /// <summary>Most resources that appear on one tick.</summary>
            public int SpawnsPerTick { get; }

            /// <summary>Most resources on the map at once.</summary>
            public int MaxAlive { get; }

            /// <summary>Nothing appears closer to the player than this, in metres on the ground.</summary>
            public float MinPlayerDistance { get; }
        }
    }
}
