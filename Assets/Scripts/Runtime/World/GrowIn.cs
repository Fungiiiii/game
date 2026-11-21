using UnityEngine;

namespace Fungiiiii.Runtime.World
{
    /// <summary>
    /// Makes the object grow from nothing to its scale when it appears in view, instead of
    /// popping in. Only the scale changes: a collider under it would grow with it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GrowIn : MonoBehaviour
    {
        private Vector3 restScale;
        private float elapsed;
        private float duration;

        public bool IsPlaying => enabled;

        /// <summary>Grows from nothing to the scale the object has now, over <paramref name="seconds"/>.</summary>
        public void Play(float seconds)
        {
            restScale = transform.localScale;
            duration = Mathf.Max(0.01f, seconds);
            elapsed = 0f;
            enabled = true;
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
            transform.localScale = restScale * ArrivalCurves.Grow(elapsed, duration);
        }
    }
}
