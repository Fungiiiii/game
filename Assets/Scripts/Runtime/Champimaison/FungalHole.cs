using System;
using System.Collections.Generic;
using Fungiiiii.Interaction;
using Fungiiiii.Inventory;
using Fungiiiii.Runtime.World;
using Fungiiiii.UI;
using UnityEngine;

namespace Fungiiiii.Champimaison
{
    /// <summary>
    /// The Champimaison, once planted: a dark hole in the ground and the fungal network
    /// around it, drawn as cream filaments that spread further at each stage. All
    /// placeholders. Holding the interact control drops into it what the next stage lacks
    /// (<see cref="FungalNetwork"/>); a short press collects the mycelium thread it
    /// produces once awake, or enters it once the last stage opens it. Its keys are shown
    /// in its wooden panel (<see cref="RequirementPanel"/>), not over it.
    /// </summary>
    internal sealed class FungalHole : MonoBehaviour, IInteractable, IHoldInteractable, IPanelledInteractable
    {
        private const float HoleRadius = 1f;
        private const float FilamentLift = 0.04f;
        private const float FilamentWidth = 0.05f;
        private const int FilamentPointCount = 10;
        private const int FilamentSeed = 20716;

        /// <summary>Where a key over it would float, this high above the centre of the hole: none does, its panel shows the keys.</summary>
        private const float PromptHeight = 0.9f;

        /// <summary>The bottom of the wooden panel, its pointer, floats this high above the centre of the hole.</summary>
        private const float PanelHeight = 1.2f;

        /// <summary>
        /// Top of the box the crosshair targets it in, above the centre of the hole: over the
        /// hole and up through its panel, so that looking at the panel aims at it.
        /// </summary>
        private const float AimBoxHeight = 2.5f;

        private static readonly int[] FilamentsPerStage = { 7, 12 };
        private static readonly float[] StageReach = { 3f, 6f };
        private static readonly Color HoleColor = new Color(0.05f, 0.04f, 0.035f);

        private readonly List<GameObject[]> stageFilaments = new List<GameObject[]>();

        private FungalNetwork network;
        private ThreadProduction thread;
        private ItemDefinition threadItem;
        private PlayerInventory inventory;
        private GroundRing ring;
        private Material holeMaterial;
        private float reach;

        /// <summary>Raised when the player enters the Champimaison.</summary>
        public event Action Entered;

        public FungalNetwork Network => network;

        public ThreadProduction Thread => thread;

        /// <summary>What its thread production gives.</summary>
        public ItemDefinition ThreadItem => threadItem;

        public bool CanBeEntered => network.HasUnlocked(TierUnlock.Entrance);

        public Vector3 PromptPoint => transform.position + Vector3.up * PromptHeight;

        /// <summary>Where the panel of what the next stage costs is shown.</summary>
        public Vector3 PanelPoint => transform.position + Vector3.up * PanelHeight;

        public float Reach => reach;

        public string HintKey => CanBeEntered ? UiText.HintEnter
            : thread.Stored > 0 ? UiText.HintCollectThread
            : null;

        public string HoldHintKey
        {
            get
            {
                if (network.IsComplete || !network.Progress.CanTakeFrom(inventory))
                {
                    return null;
                }

                if (!network.Progress.WouldComplete(inventory))
                {
                    return UiText.HintDeposit;
                }

                return network.Stage == 0 ? UiText.HintAwaken : UiText.HintUpgrade;
            }
        }

        /// <summary>Aimed at through its panel: whoever reads the panel can use its keys.</summary>
        public InteractionCandidate Candidate => new InteractionCandidate(
            PanelPoint,
            reach,
            true,
            new Bounds(
                transform.position + Vector3.up * (AimBoxHeight * 0.5f),
                new Vector3(HoleRadius * 2f, AimBoxHeight, HoleRadius * 2f)));

        public void Initialize(
            FungalNetwork fungalNetwork,
            ThreadProduction threadProduction,
            ItemDefinition producedThread,
            PlayerInventory playerInventory,
            float interactionReach,
            Material filamentMaterial,
            Material holeRingMaterial,
            Func<Vector3, float> groundHeight)
        {
            network = fungalNetwork;
            thread = threadProduction;
            threadItem = producedThread;
            inventory = playerInventory;
            reach = interactionReach + HoleRadius;
            holeMaterial = Primitives.CreateUnlitMaterial("Champimaison Hole Placeholder", HoleColor);
            Primitives.Create(
                PrimitiveType.Cylinder,
                "Hole",
                transform,
                new Vector3(0f, 0.01f, 0f),
                new Vector3(HoleRadius * 2f, 0.01f, HoleRadius * 2f),
                holeMaterial,
                keepCollider: false);
            ring = new GroundRing(transform, transform.position, HoleRadius + 0.15f, holeRingMaterial, groundHeight);
            BuildFilaments(filamentMaterial, groundHeight);
            network.Grew += HandleGrew;
            ShowStage(network.Stage);
        }

        public void SetTargeted(bool targeted)
        {
            ring.SetTargeted(targeted);
        }

        /// <summary>A short press: enters once the Champimaison is open, otherwise collects its thread.</summary>
        public bool Interact(PlayerInventory playerInventory)
        {
            if (CanBeEntered)
            {
                Entered?.Invoke();
                return true;
            }

            return threadItem != null && thread.Collect(playerInventory, threadItem) > 0;
        }

        /// <summary>Holding the control: drops what the next stage lacks.</summary>
        public bool InteractHold(PlayerInventory playerInventory)
        {
            return network.Deposit(playerInventory) > 0;
        }

        private void Update()
        {
            thread.Tick(Time.deltaTime);
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
            if (network.HasUnlocked(TierUnlock.MyceliumThread))
            {
                thread.Start();
            }
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
        }

        private void BuildFilaments(Material material, Func<Vector3, float> groundHeight)
        {
            // Fixed seed: the network looks the same every run.
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
            Func<Vector3, float> groundHeight)
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
