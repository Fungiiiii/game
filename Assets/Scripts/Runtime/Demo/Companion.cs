using System;
using System.Collections.Generic;
using Fungiiiii.Companions;
using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using Fungiiiii.UI;
using UnityEngine;

namespace Fungiiiii.Demo
{
    /// <summary>
    /// A caught creature out of the inventory, following the player: the Spongeus. What it
    /// does is its <see cref="CompanionBrain"/>; this puts it on the ground, turns it where
    /// it walks, and makes the part of its model that shows it full (the Spongeus's cap) look
    /// wet as it fills: darker, with a light sheen, as the water fern.
    /// A short press of E squeezes it for what it carries; holding E takes it back.
    /// </summary>
    internal sealed class Companion : MonoBehaviour, IInteractable, IHoldInteractable
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");

        private const float AimHeight = 0.3f;

        /// <summary>The key to press floats this far above the top of the model.</summary>
        private const float PromptAboveTop = 0.25f;

        private readonly List<(Renderer Renderer, int Slot)> fullParts = new List<(Renderer, int)>();
        private CompanionDefinition definition;
        private CompanionBrain brain;
        private Transform player;
        private Transform view;
        private Func<Vector3, float> groundHeight;
        private IReadOnlyList<IAbsorbable> sources;
        private ItemOutline outline;
        private Renderer[] renderers;
        private MaterialPropertyBlock tintBlock;
        private float shownFullness = -1f;
        private float drySmoothness;

        /// <summary>Raised once it went back into the inventory.</summary>
        public event Action<Companion> TakenBack;

        public CompanionDefinition Definition => definition;

        /// <summary>What it is busy with, and whether it is full.</summary>
        public CompanionBrain Brain => brain;

        /// <summary>How dark its full part is drawn, from 0 (its colour) to 1 (full).</summary>
        public float ShownFullness => Mathf.Max(0f, shownFullness);

        public InteractionCandidate Candidate =>
            new InteractionCandidate(transform.position + Vector3.up * AimHeight, definition.Reach, true, ModelBounds());

        public Vector3 PromptPoint
        {
            get
            {
                var point = transform.position;
                point.y = Mathf.Max(point.y, ModelBounds().max.y) + PromptAboveTop;
                return point;
            }
        }

        /// <summary>A short press squeezes it, once it is full.</summary>
        public string HintKey => brain.IsFull ? UiText.HintSqueeze : null;

        public string HoldHintKey => UiText.HintTakeBack;

        /// <param name="followed">The player it follows.</param>
        /// <param name="viewPoint">The player's view: catching up puts it behind it, out of sight.</param>
        /// <param name="ground">Height of the ground under a point.</param>
        /// <param name="absorbable">What it could soak up.</param>
        public void Initialize(
            CompanionDefinition companion,
            Transform followed,
            Transform viewPoint,
            Func<Vector3, float> ground,
            IReadOnlyList<IAbsorbable> absorbable,
            ItemOutline itemOutline)
        {
            definition = companion;
            brain = new CompanionBrain(companion.ToTuning(), companion.CarriedItem);
            player = followed;
            view = viewPoint;
            groundHeight = ground;
            sources = absorbable;
            outline = itemOutline;
            renderers = GetComponentsInChildren<Renderer>(true);
            tintBlock = new MaterialPropertyBlock();
            FindFullParts();
            ShowFullness(0f);
        }

        private void Update()
        {
            var position = transform.position;
            var step = brain.Step(position, player.position, view.forward, sources, Time.deltaTime);
            var next = step.Position;
            next.y = groundHeight(next);

            if (step.Moved)
            {
                var direction = new Vector3(next.x - position.x, 0f, next.z - position.z);
                if (direction.sqrMagnitude > 0.000001f)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }
            }

            transform.position = next;
            UpdateFullness();
        }

        public void SetTargeted(bool targeted)
        {
            outline.SetTargeted(targeted);
        }

        /// <summary>Squeezes it: what it carries goes into the inventory. Refused while empty, or when nothing fits.</summary>
        public bool Interact(PlayerInventory inventory)
        {
            if (!brain.IsFull || inventory.Collect(definition.CarriedItem, definition.CarriedQuantity) == 0)
            {
                return false;
            }

            brain.TryEmpty();
            return true;
        }

        /// <summary>
        /// Takes it back into the inventory, squeezed first when full so that nothing is
        /// lost. Refused when what it carries or itself does not fit.
        /// </summary>
        public bool InteractHold(PlayerInventory inventory)
        {
            if (brain.IsFull && !Interact(inventory))
            {
                return false;
            }

            if (inventory.Collect(definition.Item, 1) == 0)
            {
                return false;
            }

            TakenBack?.Invoke(this);
            return true;
        }

        /// <summary>Full, its part darkens as it soaks up; squeezed, it fades back to its colour.</summary>
        private void UpdateFullness()
        {
            var target = brain.Fullness;
            var shown = target >= shownFullness
                ? target
                : definition.DrySeconds > 0f
                    ? Mathf.Max(target, shownFullness - Time.deltaTime / definition.DrySeconds)
                    : target;
            ShowFullness(shown);
        }

        private void ShowFullness(float fullness)
        {
            if (Mathf.Approximately(fullness, shownFullness))
            {
                return;
            }

            shownFullness = fullness;
            tintBlock.SetColor(BaseColorId, Color.Lerp(Color.white, definition.FullTint, fullness));
            tintBlock.SetFloat(SmoothnessId, Mathf.Lerp(drySmoothness, definition.FullSmoothness, fullness));
            foreach (var (partRenderer, slot) in fullParts)
            {
                partRenderer.SetPropertyBlock(tintBlock, slot);
            }
        }

        /// <summary>The material slots of the model drawn with the full part's material.</summary>
        private void FindFullParts()
        {
            fullParts.Clear();
            var material = definition.FullPartMaterial;
            drySmoothness = material.HasProperty(SmoothnessId) ? material.GetFloat(SmoothnessId) : 0f;
            foreach (var modelRenderer in renderers)
            {
                var materials = modelRenderer.sharedMaterials;
                for (var slot = 0; slot < materials.Length; slot++)
                {
                    if (materials[slot] == definition.FullPartMaterial)
                    {
                        fullParts.Add((modelRenderer, slot));
                    }
                }
            }

            if (fullParts.Count == 0)
            {
                Debug.LogWarning($"No part of {name} uses {definition.FullPartMaterial.name}: it will not look full.", this);
            }
        }

        private Bounds ModelBounds()
        {
            var bounds = new Bounds(transform.position, Vector3.zero);
            foreach (var modelRenderer in renderers)
            {
                bounds.Encapsulate(modelRenderer.bounds);
            }

            return bounds;
        }
    }
}
