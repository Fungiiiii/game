using System.Collections.Generic;
using Fungiiiii.Capture;
using Fungiiiii.Champimaison;
using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Demo
{
    /// <summary>
    /// The Champimaison hole of the demo: a dark disc in the ground where the interact
    /// control drops wood and spores, and the fungal network around it, drawn as cream
    /// filaments on the ground that spread further at each stage. All placeholders.
    /// The rules are in <see cref="FungalNetwork"/>.
    /// </summary>
    internal sealed class FungalHole : MonoBehaviour, IDemoInteractable
    {
        private const float HoleRadius = 1f;
        private const float FilamentLift = 0.04f;
        private const float FilamentWidth = 0.05f;
        private const int FilamentPointCount = 10;
        private const float RefusedFlashSeconds = 0.5f;
        private const int FilamentSeed = 20716;

        /// <summary>Filaments per stage, and how far each stage reaches from the hole edge.</summary>
        private static readonly int[] FilamentsPerStage = { 7, 12 };
        private static readonly float[] StageReach = { 3f, 6f };

        private static readonly Color HoleColor = new Color(0.05f, 0.04f, 0.035f);

        private readonly List<GameObject[]> stageFilaments = new List<GameObject[]>();

        private FungalNetwork network;
        private DemoGroundRing ring;
        private Material holeMaterial;
        private Material ringMaterial;
        private Material refusedMaterial;
        private float reach;
        private float refusedFlashLeft;

        public FungalNetwork Network => network;

        /// <summary>The key to press floats a metre above the centre of the hole.</summary>
        public Vector3 PromptPoint => transform.position + Vector3.up;

        public InteractionCandidate Candidate =>
            new InteractionCandidate(transform.position, reach, !network.IsComplete);

        public void Initialize(
            FungalNetwork fungalNetwork,
            float interactionReach,
            Material filamentMaterial,
            Material holeRingMaterial,
            Material refusedRingMaterial,
            System.Func<Vector3, float> groundHeight)
        {
            network = fungalNetwork;
            reach = interactionReach + HoleRadius;
            ringMaterial = holeRingMaterial;
            refusedMaterial = refusedRingMaterial;

            holeMaterial = NetDemoPrimitives.CreateUnlitMaterial("Champimaison Hole Placeholder", HoleColor);
            NetDemoPrimitives.Create(
                PrimitiveType.Cylinder,
                "Hole",
                transform,
                new Vector3(0f, 0.01f, 0f),
                new Vector3(HoleRadius * 2f, 0.01f, HoleRadius * 2f),
                holeMaterial,
                keepCollider: false);

            ring = new DemoGroundRing(transform, transform.position, HoleRadius + 0.15f, ringMaterial, groundHeight);
            BuildFilaments(filamentMaterial, groundHeight);

            network.Grew += HandleGrew;
            ShowStage(network.Stage);
        }

        public void SetTargeted(bool targeted)
        {
            ring.SetTargeted(targeted);
        }

        /// <summary>Drops the next stage's wood and spores. Refused, taking nothing, without enough of either.</summary>
        public bool Interact(PlayerInventory inventory)
        {
            if (network.TryGrow(inventory))
            {
                return true;
            }

            refusedFlashLeft = RefusedFlashSeconds;
            ring.SetMaterial(refusedMaterial);
            return false;
        }

        /// <summary>Back to an empty hole, for a demo reset.</summary>
        public void ResetNetwork()
        {
            network.Reset();
            ShowStage(0);
        }

        private void Update()
        {
            if (refusedFlashLeft <= 0f)
            {
                return;
            }

            refusedFlashLeft -= Time.deltaTime;
            if (refusedFlashLeft <= 0f)
            {
                ring.SetMaterial(ringMaterial);
            }
        }

        private void OnDestroy()
        {
            if (network != null)
            {
                network.Grew -= HandleGrew;
            }

            if (holeMaterial != null)
            {
                Destroy(holeMaterial);
            }
        }

        private void HandleGrew(int stage)
        {
            ShowStage(stage);
        }

        private void ShowStage(int stage)
        {
            for (var index = 0; index < stageFilaments.Count; index++)
            {
                foreach (var filament in stageFilaments[index])
                {
                    filament.SetActive(index < stage);
                }
            }

            ring.SetVisible(stage < network.StageCount);
        }

        private void BuildFilaments(Material material, System.Func<Vector3, float> groundHeight)
        {
            // Fixed seed: the network looks the same every run, and after a reset.
            var random = new System.Random(FilamentSeed);
            var centre = transform.position;

            for (var stage = 0; stage < FilamentsPerStage.Length; stage++)
            {
                var filaments = new GameObject[FilamentsPerStage[stage]];
                var length = StageReach[stage];

                for (var index = 0; index < filaments.Length; index++)
                {
                    var angle = (index + (float)random.NextDouble() * 0.6f) * Mathf.PI * 2f / filaments.Length;
                    filaments[index] = BuildFilament(centre, angle, length, material, random, groundHeight);
                }

                stageFilaments.Add(filaments);
            }
        }

        private GameObject BuildFilament(
            Vector3 centre,
            float angle,
            float length,
            Material material,
            System.Random random,
            System.Func<Vector3, float> groundHeight)
        {
            var filamentObject = new GameObject("Network Filament");
            filamentObject.transform.SetParent(transform, false);

            var line = filamentObject.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = true;
            line.positionCount = FilamentPointCount;
            line.widthMultiplier = FilamentWidth;
            line.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.2f);
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;

            // Normals and tangents, so a lit material shades the filament like the ground.
            line.generateLightingData = true;
            line.alignment = LineAlignment.TransformZ;
            filamentObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            var heading = angle;
            for (var index = 0; index < FilamentPointCount; index++)
            {
                var share = index / (float)(FilamentPointCount - 1);
                heading += ((float)random.NextDouble() - 0.5f) * 0.5f;
                var distance = HoleRadius + share * length;
                var point = centre + new Vector3(Mathf.Cos(heading) * distance, 0f, Mathf.Sin(heading) * distance);
                point.y = groundHeight(point) + FilamentLift;
                line.SetPosition(index, point);
            }

            return filamentObject;
        }
    }
}
