using UnityEngine;

namespace Fungiiiii.Runtime.Champimaison
{
    /// <summary>
    /// Marks a spot of the world where a Champimaison can be planted later.
    ///
    /// It holds no behaviour. The ground is made ready in the editor by a
    /// ScatterExclusionZone child with FlattenGround on: level, painted as a
    /// clearing, kept free of trees. That child is EditorOnly and is stripped from
    /// builds; this marker stays, so the game can find the sites.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ChampimaisonSite : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("Radius of level ground the Champimaison can use, in metres.")]
        private float radius = 5f;

        public float Radius => radius;

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.85f, 0.31f, 0.24f, 0.9f);
            Gizmos.DrawWireCube(transform.position + Vector3.up, new Vector3(radius * 2f, 2f, radius * 2f));
        }
    }
}
