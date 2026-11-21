using UnityEngine;

namespace Fungiiiii.Runtime.World
{
    /// <summary>
    /// A picture lying in the world, as a dropped item in Minecraft or Terraria: it turns to
    /// face the camera every frame and floats gently up and down.
    /// </summary>
    public sealed class FacesCamera : MonoBehaviour
    {
        [SerializeField, Min(0f)]
        private float bobHeight = 0.05f;

        [Tooltip("Seconds of one float up and down.")]
        [SerializeField, Min(0.1f)]
        private float bobSeconds = 2f;

        private Transform view;
        private Vector3 rest;
        private float elapsed;

        /// <param name="phase">From 0 to 1: where in its float it starts, so that two pictures do not float alike.</param>
        public void Initialize(Transform viewPoint, float phase)
        {
            view = viewPoint;
            rest = transform.localPosition;
            elapsed = phase * bobSeconds;
        }

        private void LateUpdate()
        {
            if (view == null)
            {
                return;
            }

            elapsed += Time.deltaTime;
            transform.localPosition = rest + Vector3.up * (bobHeight * Mathf.Sin(elapsed / bobSeconds * 2f * Mathf.PI));

            var away = transform.position - view.position;
            if (away.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(away);
            }
        }
    }
}
