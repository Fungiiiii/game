using UnityEngine;

namespace Fungiiiii.Runtime.World
{
    /// <summary>
    /// Makes the object fall from above to where it stands now, turning as it falls, as a
    /// stick dropped by the leaves, instead of popping in.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FallIn : MonoBehaviour
    {
        /// <summary>How far it turns over a whole fall, in degrees, around a level axis.</summary>
        private const float TumbleDegrees = 300f;

        private Vector3 restPosition;
        private Quaternion restRotation;
        private Vector3 tumbleAxis;
        private float height;
        private float elapsed;
        private float duration;

        public bool IsPlaying => enabled;

        /// <summary>Falls from <paramref name="fromHeight"/> metres above the pose the object has now, onto it.</summary>
        /// <param name="tumbleSeed">Turns the axis the object tumbles around, so that two do not fall alike.</param>
        public void Play(float fromHeight, float tumbleSeed)
        {
            restPosition = transform.position;
            restRotation = transform.rotation;
            tumbleAxis = Quaternion.Euler(0f, tumbleSeed * 360f, 0f) * Vector3.right;
            height = Mathf.Max(0f, fromHeight);
            duration = ArrivalCurves.FallSeconds(height);
            elapsed = 0f;
            enabled = duration > 0f;
            Apply();
        }

        private void Awake()
        {
            enabled = false;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            Apply();
            if (elapsed >= duration)
            {
                enabled = false;
            }
        }

        private void Apply()
        {
            var left = duration > 0f ? 1f - Mathf.Clamp01(elapsed / duration) : 0f;
            transform.SetPositionAndRotation(
                restPosition + Vector3.up * ArrivalCurves.HeightAbove(elapsed, height),
                Quaternion.AngleAxis(TumbleDegrees * left, tumbleAxis) * restRotation);
        }
    }
}
