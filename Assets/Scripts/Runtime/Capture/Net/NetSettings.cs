using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>The tuning of the capture net, as data: the values of the validated net prototype.</summary>
    [CreateAssetMenu(fileName = "NetSettings", menuName = "Fungiiiii/Net Settings")]
    public sealed class NetSettings : ScriptableObject
    {
        [SerializeField, Min(0.1f)]
        private float launchSpeed = 11f;

        [SerializeField, Min(0f)]
        private float gravity = 9.81f;

        [SerializeField, Min(0.1f)]
        private float impactRadius = 1.5f;

        [SerializeField, Min(0f)]
        private float scareRadius = 3f;

        [SerializeField, Min(0f)]
        private float holdSeconds = 0.5f;

        [SerializeField, Min(0.1f)]
        private float returnSeconds = 1f;

        internal NetThrowSettings ToThrowSettings()
        {
            return new NetThrowSettings(launchSpeed, gravity, impactRadius, scareRadius, holdSeconds, returnSeconds, NetCaptureRates.Default);
        }
    }
}
