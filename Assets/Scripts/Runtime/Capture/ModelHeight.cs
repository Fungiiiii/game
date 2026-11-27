using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>How high a creature's model reaches above its feet: the top of its renderers, what drips from it left out.</summary>
    internal static class ModelHeight
    {
        /// <summary>Metres from <paramref name="creature"/>'s position to the top of its model; <paramref name="fallback"/> without a model.</summary>
        public static float Of(Transform creature, float fallback = 0.5f)
        {
            var top = float.MinValue;
            foreach (var renderer in creature.GetComponentsInChildren<Renderer>())
            {
                // What drips from it is not its body.
                if (!(renderer is ParticleSystemRenderer))
                {
                    top = Mathf.Max(top, renderer.bounds.max.y);
                }
            }

            return top > float.MinValue ? Mathf.Max(0.1f, top - creature.position.y) : fallback;
        }
    }
}
