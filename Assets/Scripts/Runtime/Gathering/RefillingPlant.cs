using Fungiiiii.Companions;
using Fungiiiii.Gathering;
using Fungiiiii.Interaction;
using Fungiiiii.Inventory;
using Fungiiiii.Outlines;
using UnityEngine;

namespace Fungiiiii.Gathering
{
    /// <summary>
    /// A plant that fills up by chance and that only a companion can empty: the water fern.
    /// It sits on the root of the plant's prefab, which tells it its model and what drips
    /// from it. Full, it looks wet, as wet surfaces do in other games: darker, with a light
    /// sheen, and now and then a drop falls from it. It is then outlined like what can be
    /// gathered and, aimed at, shows a short text instead of a key: the player cannot take
    /// it themselves, and the interact control does nothing with it.
    /// </summary>
    internal sealed class RefillingPlant : MonoBehaviour, IInteractable, ILabelledInteractable, IAbsorbable
    {
        /// <summary>The text floats this far above the top of the model.</summary>
        private const float PromptAboveTop = 0.25f;

        [Tooltip("The plant itself, outlined while full.")]
        [SerializeField]
        private GameObject model;

        [Tooltip("What drips from it while it is full: drops falling now and then.")]
        [SerializeField]
        private ParticleSystem drip;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");

        private RefillingPlantDefinition definition;
        private ChanceRefill refill;
        private ItemOutline outline;
        private Renderer[] renderers;
        private Renderer[] modelRenderers;
        private MaterialPropertyBlock wetBlock;

        /// <summary>The plant itself, which the outline is made around.</summary>
        public GameObject Model => model;

        public RefillingPlantDefinition Definition => definition;

        public bool IsFull => refill.IsFull;

        public Vector3 Position => transform.position;

        public ItemDefinition Yield => definition.Yield;

        public bool CanBeAbsorbed => refill.IsFull;

        /// <summary>Aimed at only while full: there is nothing to see on an empty one.</summary>
        public InteractionCandidate Candidate =>
            new InteractionCandidate(ModelBounds().center, definition.Reach, refill.IsFull, ModelBounds());

        public Vector3 PromptPoint
        {
            get
            {
                var point = transform.position;
                point.y = Mathf.Max(point.y, ModelBounds().max.y) + PromptAboveTop;
                return point;
            }
        }

        /// <summary>A press does nothing: only a companion can take what it holds.</summary>
        public string HintKey => null;

        public string LabelKey => definition.LabelKey;

        /// <summary>Whether it drips, while full.</summary>
        public bool IsDripping => drip.emission.enabled;

        private void Awake()
        {
            // The model's own renderers, taken before an outline adds its own under them.
            modelRenderers = model.GetComponentsInChildren<Renderer>(true);
        }

        public void Initialize(RefillingPlantDefinition plant, ChanceRefill chanceRefill, ItemOutline itemOutline)
        {
            definition = plant;
            refill = chanceRefill;
            outline = itemOutline;
            renderers = model.GetComponentsInChildren<Renderer>(true);
            wetBlock = new MaterialPropertyBlock();
            drip.Play();
            Show();
        }

        /// <summary>Counts time towards filling up, called by the plants of the level.</summary>
        public void Tick(float deltaTime)
        {
            if (refill.Tick(deltaTime))
            {
                Show();
            }
        }

        /// <summary>Starts over, full by chance, for the start of a game.</summary>
        public void Restart(float fullChance)
        {
            refill.Restart(fullChance);
            Show();
        }

        public void SetTargeted(bool targeted)
        {
            outline.SetTargeted(targeted && refill.IsFull);
        }

        public bool Interact(PlayerInventory inventory)
        {
            return false;
        }

        public bool Absorb()
        {
            if (!refill.TryTake())
            {
                return false;
            }

            Show();
            return true;
        }

        private void Show()
        {
            var full = refill.IsFull;
            var emission = drip.emission;
            emission.enabled = full;

            // Full, darker with a light sheen; empty, back to the material's own look.
            foreach (var modelRenderer in modelRenderers)
            {
                if (full)
                {
                    wetBlock.SetColor(BaseColorId, definition.FullTint);
                    wetBlock.SetFloat(SmoothnessId, definition.FullSmoothness);
                    modelRenderer.SetPropertyBlock(wetBlock);
                }
                else
                {
                    modelRenderer.SetPropertyBlock(null);
                }
            }

            outline.SetVisible(refill.IsFull);
            if (!refill.IsFull)
            {
                outline.SetTargeted(false);
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

        /// <summary>What is missing on the prefab, or null when it is sound.</summary>
        public string FindProblem()
        {
            return model == null || drip == null
                ? $"The plant prefab '{name}' lacks its model or what drips from it."
                : null;
        }
    }
}
