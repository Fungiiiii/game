using System;
using System.Collections.Generic;
using Fungiiiii.Inventory;
using Fungiiiii.Runtime.World;
using UnityEngine;

namespace Fungiiiii.Champimaison
{
    /// <summary>
    /// Builds the Champimaison where the seed is planted, with its stages and its thread
    /// production from <see cref="ChampimaisonDefinition"/>.
    /// </summary>
    internal sealed class FungalHoleSpawner : MonoBehaviour
    {
        [SerializeField]
        private ChampimaisonDefinition definition;

        [Tooltip("Furthest ground distance from the hole's edge at which the player uses it, in metres.")]
        [SerializeField, Min(0.1f)]
        private float reach = 2f;

        [Tooltip("Colour of the fungal network and of the hole's ring: palette \"Body light\", a mycelium cream.")]
        [SerializeField]
        private Color networkColor = new Color32(0xF2, 0xE6, 0xCC, 0xFF);

        private readonly List<Material> materials = new List<Material>();

        public ChampimaisonDefinition Definition => definition;

        public string FindProblem()
        {
            return definition == null ? $"{name} has no Champimaison definition." : definition.FindProblem();
        }

        public FungalHole Spawn(Vector3 centre, Func<Vector3, float> groundHeight, PlayerInventory inventory)
        {
            centre.y = groundHeight(centre);
            var hole = new GameObject("Champimaison").AddComponent<FungalHole>();
            hole.transform.SetParent(transform, false);
            hole.transform.position = centre;

            // Lit filaments take the forest's light and fog, like the ground they lie on.
            var filaments = Keep(Primitives.CreateMaterial("Fungal Network", networkColor));
            var ring = Keep(Primitives.CreateUnlitMaterial("Champimaison Hole Ring", networkColor));
            hole.Initialize(
                new FungalNetwork(definition.Tiers),
                new ThreadProduction(definition.SecondsPerThread, definition.MaxStoredThread),
                definition.ThreadItem,
                inventory,
                reach,
                filaments,
                ring,
                groundHeight);
            return hole;
        }

        private void OnDestroy()
        {
            foreach (var material in materials)
            {
                Destroy(material);
            }
        }

        private Material Keep(Material material)
        {
            materials.Add(material);
            return material;
        }
    }
}
