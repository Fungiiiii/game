using UnityEngine;

namespace Fungiiiii.Runtime.World
{
    /// <summary>
    /// A circle on the ground where <see cref="ForestScatter"/> never places anything
    /// (spawn clearing, path, point of interest). Only the X/Z position matters.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScatterExclusionZone : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("Radius of the empty area, in metres.")]
        private float radius = 8f;

        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0f, value);
        }

        public ExclusionCircle ToCircle() => new ExclusionCircle(transform.position, radius);

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.35f, 0.25f, 0.9f);
            Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, new Vector3(1f, 0.001f, 1f));
            Gizmos.DrawWireSphere(Vector3.zero, radius);
        }
    }

    /// <summary>
    /// Plain-data circle on the X/Z plane.
    /// </summary>
    public readonly struct ExclusionCircle
    {
        public ExclusionCircle(Vector3 center, float radius)
        {
            Center = new Vector2(center.x, center.z);
            Radius = radius;
        }

        public Vector2 Center { get; }
        public float Radius { get; }

        public bool Contains(Vector3 worldPosition)
        {
            return (new Vector2(worldPosition.x, worldPosition.z) - Center).sqrMagnitude < Radius * Radius;
        }
    }
}
