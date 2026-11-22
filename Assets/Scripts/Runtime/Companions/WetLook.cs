using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Companions
{
    /// <summary>
    /// The part of a model that looks wet as it soaks up water (the Spongeus's cap): every
    /// material slot drawn with that part's material is multiplied by a tint and given a
    /// sheen, through property blocks, so that no material is copied.
    /// </summary>
    internal sealed class WetLook
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");

        private readonly List<(Renderer Renderer, int Slot)> parts = new List<(Renderer, int)>();
        private readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        private readonly Color wetTint;
        private readonly float drySmoothness;
        private readonly float wetSmoothness;
        private float shown = -1f;

        public WetLook(IEnumerable<Renderer> renderers, Material wetPart, Color tint, float smoothness)
        {
            wetTint = tint;
            wetSmoothness = smoothness;
            if (wetPart == null)
            {
                return;
            }

            drySmoothness = wetPart.HasProperty(SmoothnessId) ? wetPart.GetFloat(SmoothnessId) : 0f;
            foreach (var renderer in renderers)
            {
                var materials = renderer.sharedMaterials;
                for (var slot = 0; slot < materials.Length; slot++)
                {
                    if (materials[slot] == wetPart)
                    {
                        parts.Add((renderer, slot));
                    }
                }
            }
        }

        /// <summary>False when no part of the model uses the wet part's material: nothing will show.</summary>
        public bool HasParts => parts.Count > 0;

        /// <summary>How wet the part is drawn, from 0 (its colour) to 1 (soaked).</summary>
        public float Shown => Mathf.Max(0f, shown);

        public void Show(float wetness)
        {
            if (Mathf.Approximately(wetness, shown))
            {
                return;
            }

            shown = wetness;
            block.SetColor(BaseColorId, Color.Lerp(Color.white, wetTint, wetness));
            block.SetFloat(SmoothnessId, Mathf.Lerp(drySmoothness, wetSmoothness, wetness));
            foreach (var (renderer, slot) in parts)
            {
                if (renderer != null)
                {
                    renderer.SetPropertyBlock(block, slot);
                }
            }
        }
    }
}
