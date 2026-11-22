using System;
using System.Collections.Generic;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Champimaison
{
    /// <summary>What a Champimaison stage gives once it is reached.</summary>
    public enum TierUnlock
    {
        None,

        /// <summary>The Champimaison produces mycelium thread.</summary>
        MyceliumThread,

        /// <summary>The player can enter the Champimaison.</summary>
        Entrance,
    }

    /// <summary>One stage of the Champimaison: what it costs and what it gives.</summary>
    [Serializable]
    public struct ChampimaisonTier
    {
        [Tooltip("Dropped into the Champimaison, a little at a time if need be.")]
        [SerializeField]
        private ItemAmount[] cost;

        [SerializeField]
        private TierUnlock unlock;

        public ChampimaisonTier(ItemAmount[] cost, TierUnlock unlock)
        {
            this.cost = cost;
            this.unlock = unlock;
        }

        public IReadOnlyList<ItemAmount> Cost => cost ?? Array.Empty<ItemAmount>();

        public TierUnlock Unlock => unlock;
    }

    /// <summary>
    /// The Champimaison's stages in order, and the mycelium thread it produces once awake.
    /// </summary>
    [CreateAssetMenu(fileName = "Champimaison", menuName = "Fungiiiii/Champimaison Definition")]
    public sealed class ChampimaisonDefinition : ScriptableObject
    {
        [Tooltip("In order: the first one wakes the seed up.")]
        [SerializeField]
        private ChampimaisonTier[] tiers = new ChampimaisonTier[0];

        [Header("Mycelium thread, once a stage unlocks it")]
        [SerializeField]
        private ItemDefinition threadItem;

        [Tooltip("Seconds to produce one thread.")]
        [SerializeField, Min(0.1f)]
        private float secondsPerThread = 20f;

        [Tooltip("Most threads waiting to be collected.")]
        [SerializeField, Min(1)]
        private int maxStoredThread = 3;

        public IReadOnlyList<ChampimaisonTier> Tiers => tiers;

        public ItemDefinition ThreadItem => threadItem;

        public float SecondsPerThread => secondsPerThread;

        public int MaxStoredThread => maxStoredThread;

        /// <summary>A definition that exists only at runtime, for tests.</summary>
        public static ChampimaisonDefinition Create(ItemDefinition thread, float secondsPerThread, int maxStoredThread, params ChampimaisonTier[] tiers)
        {
            var definition = CreateInstance<ChampimaisonDefinition>();
            definition.threadItem = thread;
            definition.secondsPerThread = secondsPerThread;
            definition.maxStoredThread = maxStoredThread;
            definition.tiers = tiers;
            return definition;
        }

        public string FindProblem()
        {
            if (tiers.Length == 0)
            {
                return $"'{name}' has no stage.";
            }

            for (var index = 0; index < tiers.Length; index++)
            {
                var cost = tiers[index].Cost;
                if (cost.Count == 0)
                {
                    return $"Stage {index + 1} of '{name}' costs nothing.";
                }

                foreach (var line in cost)
                {
                    if (line.Item == null || line.Amount <= 0)
                    {
                        return $"Stage {index + 1} of '{name}' has an empty line.";
                    }
                }

                if (tiers[index].Unlock == TierUnlock.MyceliumThread && threadItem == null)
                {
                    return $"'{name}' produces thread but has no thread item.";
                }
            }

            return null;
        }
    }
}
