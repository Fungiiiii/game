using UnityEngine;

namespace Fungiiiii.Gathering
{
    /// <summary>
    /// How every kind of resource comes and goes around the players: the bubble kept
    /// around each of them, the tick, and how far from them things appear and go away.
    /// One asset for the game; each kind says how many of it (<see cref="GatherableDefinition"/>).
    /// </summary>
    [CreateAssetMenu(fileName = "GatheringSettings", menuName = "Fungiiiii/Gathering Settings")]
    public sealed class GatheringSettings : ScriptableObject
    {
        [Tooltip("Furthest ground distance from which the player gathers, in metres.")]
        [SerializeField, Min(0.1f)]
        private float reach = 2f;

        [Tooltip("Seconds between two ticks: on each one, resources appear on free spots around the players.")]
        [SerializeField, Min(0.1f)]
        private float tickSeconds = 5f;

        [Tooltip("Radius of the bubble around each player where resources are kept, in metres.")]
        [SerializeField, Min(1f)]
        private float bubbleRadius = 50f;

        [Tooltip("On a tick, nothing appears closer to a player than this, in metres.")]
        [SerializeField, Min(0f)]
        private float minPlayerDistance = 20f;

        [Tooltip("At the start and after a reset, while the screen is black, the bubble fills from this distance, in metres.")]
        [SerializeField, Min(0f)]
        private float startMinPlayerDistance = 8f;

        [Tooltip("What is further than this from every player goes away, in metres.")]
        [SerializeField, Min(1f)]
        private float despawnDistance = 75f;

        [Tooltip("How many times as likely a spot straight ahead of the player is used as one behind: walking on, one keeps finding some.")]
        [SerializeField, Min(1f)]
        private float aheadWeight = 4f;

        public float Reach => reach;

        /// <summary>The spawner's settings for a kind with these counts.</summary>
        public ResourceSpawner.Settings For(GatherableDefinition kind)
        {
            return new ResourceSpawner.Settings(
                tickSeconds,
                kind.PerTick,
                kind.NearPlayer,
                bubbleRadius,
                Mathf.Min(minPlayerDistance, bubbleRadius),
                Mathf.Min(startMinPlayerDistance, bubbleRadius),
                Mathf.Max(despawnDistance, bubbleRadius),
                aheadWeight,
                kind.MaxAlive);
        }
    }
}
