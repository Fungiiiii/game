using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Keeps the home screen's camera almost still: it floats a few centimetres around
    /// its pose and turns by a fraction of a degree, so the picture breathes while the
    /// wind in the trees does the moving.
    /// </summary>
    internal sealed class MenuCameraDrift : MonoBehaviour
    {
        private const float FullTurn = Mathf.PI * 2f;

        [Tooltip("How far the camera floats sideways and up, in metres.")]
        [SerializeField]
        private Vector2 floatDistance = new Vector2(0.22f, 0.07f);

        [Tooltip("How far the camera turns, in degrees.")]
        [SerializeField, Min(0f)]
        private float turnDegrees = 0.5f;

        [Tooltip("Seconds of one full sideways float.")]
        [SerializeField, Min(1f)]
        private float periodSeconds = 16f;

        private Vector3 restPosition;
        private Quaternion restRotation;

        private void Awake()
        {
            restPosition = transform.position;
            restRotation = transform.rotation;
        }

        private void Update()
        {
            var phase = Time.time / periodSeconds * FullTurn;

            // Two rhythms that never line up: the float does not read as a loop.
            var sideways = Mathf.Sin(phase);
            var upwards = Mathf.Sin(phase * 0.63f + 1.3f);

            var position = restPosition
                + restRotation * Vector3.right * (sideways * floatDistance.x)
                + Vector3.up * (upwards * floatDistance.y);
            var rotation = restRotation * Quaternion.Euler(upwards * turnDegrees * 0.5f, -sideways * turnDegrees, 0f);
            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
