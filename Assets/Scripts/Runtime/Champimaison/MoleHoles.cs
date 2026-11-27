using System;
using System.Collections.Generic;
using Fungiiiii.Interaction;
using Fungiiiii.Inventory;
using Fungiiiii.Items;
using Fungiiiii.Outlines;
using Fungiiiii.Runtime.Champimaison;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fungiiiii.Champimaison
{
    /// <summary>
    /// The mole holes of the level, where the Champimaison can be planted: one on each
    /// Champimaison site of the forest, read when the level is built, the forest scene left
    /// as it is. Planting takes the seed from the hand and builds the Champimaison in that
    /// hole; the other holes stay in the decor, without outline or use, for the
    /// Champimaisons to come.
    /// </summary>
    internal sealed class MoleHoles : MonoBehaviour
    {
        [Tooltip("Model of a mole hole, its pivot on the ground.")]
        [SerializeField]
        private GameObject model;

        [Tooltip("The item planted in a hole.")]
        [SerializeField]
        private ItemDefinition seed;

        [SerializeField]
        private FungalHoleSpawner champimaison;

        [Tooltip("Furthest ground distance from which the player uses a hole, in metres.")]
        [SerializeField, Min(0.1f)]
        private float reach = 2f;

        private readonly List<MoleHole> holes = new List<MoleHole>();

        private Func<Vector3, float> groundHeight;
        private ICollection<IInteractable> interactables;
        private IItemSelection hand;
        private PlayerInventory inventory;

        /// <summary>Raised once, when the seed is planted.</summary>
        public event Action<FungalHole> Planted;

        public IReadOnlyList<MoleHole> Holes => holes;

        /// <summary>The planted Champimaison; null until the seed is planted.</summary>
        public FungalHole Champimaison { get; private set; }

        public bool IsPlanted => Champimaison != null;

        public ItemDefinition Seed => seed;

        public string FindProblem()
        {
            if (model == null || seed == null || champimaison == null)
            {
                return $"{name} lacks its model, its seed or the Champimaison spawner.";
            }

            return champimaison.FindProblem();
        }

        /// <param name="usable">What the interact control can act on: the holes join it, and the Champimaison once planted.</param>
        /// <param name="selection">The slot whose item is in hand.</param>
        public void Populate(
            Scene forest,
            Func<Vector3, float> ground,
            ItemOutlineFactory outlines,
            ICollection<IInteractable> usable,
            IItemSelection selection,
            PlayerInventory playerInventory,
            System.Random random)
        {
            groundHeight = ground;
            interactables = usable;
            hand = selection;
            inventory = playerInventory;

            foreach (var root in forest.GetRootGameObjects())
            {
                foreach (var site in root.GetComponentsInChildren<ChampimaisonSite>(true))
                {
                    Dig(site.transform.position, outlines, random);
                }
            }

            if (holes.Count == 0)
            {
                Debug.LogWarning($"{name} found no Champimaison site in {forest.name}: the seed cannot be planted.", this);
            }
        }

        public PlantingPrompt PromptFor()
        {
            var held = hand.SelectedSlot;
            return PlantingRules.PromptFor(
                IsPlanted,
                !held.IsEmpty && held.Item == seed,
                inventory.Count(seed.Id) > 0);
        }

        /// <summary>Plants the seed held in hand in <paramref name="hole"/>. Refused without the seed in hand, or once planted.</summary>
        public bool TryPlant(MoleHole hole, PlayerInventory playerInventory)
        {
            var held = hand.SelectedSlot;
            if (IsPlanted || held.IsEmpty || held.Item != seed || playerInventory.TakeFromSlot(held.Index, 1, out _) == 0)
            {
                return false;
            }

            Champimaison = champimaison.Spawn(hole.transform.position, groundHeight, playerInventory);
            foreach (var other in holes)
            {
                other.Retire();
                interactables.Remove(other);
            }

            // The Champimaison grows out of this hole: it is no longer drawn.
            hole.gameObject.SetActive(false);
            interactables.Add(Champimaison);
            Planted?.Invoke(Champimaison);
            return true;
        }

        private void Dig(Vector3 position, ItemOutlineFactory outlines, System.Random random)
        {
            position.y = groundHeight(position);
            var yaw = Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);
            var holeObject = Instantiate(model, position, yaw, transform);
            holeObject.name = "Mole Hole";
            var hole = holeObject.AddComponent<MoleHole>();
            hole.Initialize(this, outlines.Create(holeObject), reach);
            holes.Add(hole);
            interactables.Add(hole);
        }
    }
}
