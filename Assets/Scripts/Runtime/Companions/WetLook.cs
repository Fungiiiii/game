using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Companions
{
    /// <summary>
    /// The part of a model that looks wet as it soaks up water (the Champonge's cap): every
    /// material slot drawn with that part's material goes from its own colour to that colour
    /// multiplied by a tint, and takes a sheen, through property blocks, so that no material
    /// is copied. Soaked through, drops fall from it, as from a full fern.
    /// </summary>
    internal sealed class WetLook
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");

        /// <summary>Wetness from which drops fall.</summary>
        private const float SoakedThrough = 0.999f;

        private readonly List<(Renderer Renderer, int Slot)> parts = new List<(Renderer, int)>();
        private readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        private readonly Color dryColor = Color.white;
        private readonly Color wetColor;
        private readonly float drySmoothness;
        private readonly float wetSmoothness;
        private readonly ParticleSystem drip;
        private float shown = -1f;

        /// <param name="model">The model the part belongs to: what drips hangs under it.</param>
        /// <param name="dripPrefab">What drips while soaked through; null for nothing.</param>
        public WetLook(Transform model, IEnumerable<Renderer> renderers, Material wetPart, Color tint, float smoothness, ParticleSystem dripPrefab)
        {
            wetColor = tint;
            wetSmoothness = smoothness;
            if (wetPart == null)
            {
                return;
            }

            if (wetPart.HasProperty(BaseColorId))
            {
                dryColor = wetPart.GetColor(BaseColorId);
            }

            wetColor = dryColor * tint;
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

            if (dripPrefab != null && model != null && parts.Count > 0)
            {
                drip = Object.Instantiate(dripPrefab, model);
                drip.name = "Drip";
                PlaceUnder(drip, parts[0].Renderer.bounds, model);
            }
        }

        /// <summary>False when no part of the model uses the wet part's material: nothing will show.</summary>
        public bool HasParts => parts.Count > 0;

        /// <summary>How wet the part is drawn, from 0 (its colour) to 1 (soaked).</summary>
        public float Shown => Mathf.Max(0f, shown);

        /// <summary>Whether drops fall from it.</summary>
        public bool IsDripping => drip != null && drip.emission.enabled;

        public void Show(float wetness)
        {
            if (Mathf.Approximately(wetness, shown))
            {
                return;
            }

            shown = wetness;
            block.SetColor(BaseColorId, Color.Lerp(dryColor, wetColor, wetness));
            block.SetFloat(SmoothnessId, Mathf.Lerp(drySmoothness, wetSmoothness, wetness));
            foreach (var (renderer, slot) in parts)
            {
                if (renderer != null)
                {
                    renderer.SetPropertyBlock(block, slot);
                }
            }

            SetDripping(wetness >= SoakedThrough);
        }

        private void SetDripping(bool dripping)
        {
            if (drip == null)
            {
                return;
            }

            // As on the fern: it keeps playing, and only lets drops go while soaked; the drops
            // already falling finish their fall.
            var emission = drip.emission;
            emission.enabled = dripping;
        }

        /// <summary>Under the wet part's rim, as wide as it: the drops fall from its edge.</summary>
        private static void PlaceUnder(ParticleSystem drip, Bounds part, Transform model)
        {
            drip.transform.position = new Vector3(part.center.x, part.min.y + part.size.y * 0.3f, part.center.z);
            var shape = drip.shape;
            var scale = Mathf.Max(model.lossyScale.x, 0.0001f);
            shape.radius = Mathf.Max(part.extents.x, part.extents.z) * 0.8f / scale;
            var emission = drip.emission;
            emission.enabled = false;
            drip.Play();
        }
    }
}
