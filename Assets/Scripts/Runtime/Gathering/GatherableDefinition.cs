using System.Collections.Generic;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Gathering
{
    /// <summary>
    /// One kind of resource to gather, as data: what it gives, what it looks like, and how
    /// many of it are kept around each player. Where it can stand comes from the level.
    /// </summary>
    [CreateAssetMenu(fileName = "Gatherable", menuName = "Fungiiiii/Gatherable Definition")]
    public sealed class GatherableDefinition : ScriptableObject
    {
        [Tooltip("What gathering one gives.")]
        [SerializeField]
        private ItemDefinition item;

        [Tooltip("Models: each one that appears draws one of them.")]
        [SerializeField]
        private GameObject[] variants = new GameObject[0];

        [Tooltip("How many are kept within the bubble of each player.")]
        [SerializeField, Min(0)]
        private int nearPlayer = 4;

        [Tooltip("Most on the whole map, all players together.")]
        [SerializeField, Min(0)]
        private int maxAlive = 30;

        [Tooltip("Most that appear per tick and per player.")]
        [SerializeField, Min(0)]
        private int perTick = 1;

        public ItemDefinition Item => item;

        public IReadOnlyList<GameObject> Variants => variants;

        public int NearPlayer => nearPlayer;

        public int MaxAlive => maxAlive;

        public int PerTick => perTick;

        /// <summary>What is missing for it to be used, or null when it is sound.</summary>
        public string FindProblem()
        {
            if (item == null)
            {
                return $"'{name}' gives no item.";
            }

            if (variants.Length == 0 || System.Array.IndexOf(variants, null) >= 0)
            {
                return $"'{name}' has an empty or missing model.";
            }

            return null;
        }
    }
}
