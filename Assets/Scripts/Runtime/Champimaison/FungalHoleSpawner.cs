using System;
using System.Collections.Generic;
using Fungiiiii.Capture;
using Fungiiiii.Champimaison;
using Fungiiiii.Runtime.Champimaison;
using Fungiiiii.Runtime.World;
using UnityEngine;

namespace Fungiiiii.Champimaison
{
    /// <summary>
    /// Builds the Champimaison hole and its fungal network at the forest's site nearest to
    /// the player, when the game starts. Left inactive for the playtest.
    /// </summary>
    internal sealed class FungalHoleSpawner : MonoBehaviour
    {
        /// <summary>How close the player must be to use the hole, in metres.</summary>
        private const float Reach = 2f;

        private static readonly Color RefusedColor = new Color(1f, 0.45f, 0.1f);

        [Tooltip("Colour of the fungal network and of the hole's ring: palette \"Body light\", a mycelium cream.")]
        [SerializeField]
        private Color networkColor = new Color32(0xF2, 0xE6, 0xCC, 0xFF);

        [Header("Fungal network: starting values of the design note, without source")]
        [SerializeField, Min(0)]
        private int stageOneWood = 2;

        [SerializeField, Min(0)]
        private int stageOneSpores = 2;

        [SerializeField, Min(0)]
        private int stageTwoWood = 4;

        [SerializeField, Min(0)]
        private int stageTwoSpores = 4;

        private readonly List<Material> materials = new List<Material>();

        /// <summary>The hole, at the site nearest to <paramref name="player"/>, or 10 m north of it without a site.</summary>
        public FungalHole Spawn(Vector3 player, Func<Vector3, float> groundHeight, string woodId, string sporeId)
        {
            var site = NearestSite(player);
            var centre = site != null ? site.transform.position : player + Vector3.forward * 10f;
            centre.y = groundHeight(centre);

            var network = new FungalNetwork(
                new[]
                {
                    new NetworkStageCost(stageOneWood, stageOneSpores),
                    new NetworkStageCost(stageTwoWood, stageTwoSpores),
                },
                woodId,
                sporeId);

            var hole = new GameObject("Champimaison Hole").AddComponent<FungalHole>();
            hole.transform.SetParent(transform, false);
            hole.transform.position = centre;
            // Lit filaments take the forest's light and fog, like the ground they lie on.
            var filaments = Keep(Primitives.CreateMaterial("Fungal Network", networkColor));
            var ring = Keep(Primitives.CreateUnlitMaterial("Champimaison Hole Ring", networkColor));
            var refused = Keep(Primitives.CreateUnlitMaterial("Champimaison Refused", RefusedColor));
            hole.Initialize(network, Reach, filaments, ring, refused, groundHeight);
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

        private static ChampimaisonSite NearestSite(Vector3 player)
        {
            ChampimaisonSite nearest = null;
            var nearestDistance = float.PositiveInfinity;

            foreach (var site in FindObjectsByType<ChampimaisonSite>(FindObjectsSortMode.None))
            {
                var distance = Vector3.Distance(site.transform.position, player);
                if (distance < nearestDistance)
                {
                    nearest = site;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }
    }
}
