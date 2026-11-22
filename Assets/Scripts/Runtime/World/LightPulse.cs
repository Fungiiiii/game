using UnityEngine;

namespace Fungiiiii.Runtime.World
{
    /// <summary>
    /// A light that breathes: its intensity goes softly up and down, to draw the eye to
    /// something, such as the seed waiting at the giant tree.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public sealed class LightPulse : MonoBehaviour
    {
        [SerializeField, Min(0f)]
        private float lowIntensity = 1.2f;

        [SerializeField, Min(0f)]
        private float highIntensity = 3f;

        [Tooltip("Seconds for one breath, up and down.")]
        [SerializeField, Min(0.1f)]
        private float periodSeconds = 2f;

        private Light glow;

        private void Awake()
        {
            glow = GetComponent<Light>();
        }

        private void Update()
        {
            var breath = 0.5f - 0.5f * Mathf.Cos(Time.time * 2f * Mathf.PI / periodSeconds);
            glow.intensity = Mathf.Lerp(lowIntensity, highIntensity, breath);
        }
    }
}
