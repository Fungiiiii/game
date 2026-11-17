using UnityEngine;

namespace Fungiiiii.Runtime.World
{
    /// <summary>
    /// Gives the ground mist shader the relief of a Terrain, so the mist can hug the
    /// ground, and hides the mist quad while the camera is too far from the mist
    /// zone to see any: the shader runs on every pixel of the screen.
    /// It sits on the mist quad, child of the camera.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer))]
    internal sealed class GroundMist : MonoBehaviour
    {
        private static readonly int HeightmapId = Shader.PropertyToID("_MistHeightmap");
        private static readonly int TerrainOriginId = Shader.PropertyToID("_MistTerrainOrigin");
        private static readonly int TerrainSizeId = Shader.PropertyToID("_MistTerrainSize");
        private static readonly int ZoneCentreId = Shader.PropertyToID("_ZoneCentre");
        private static readonly int ZoneHalfSizeId = Shader.PropertyToID("_ZoneHalfSize");
        private static readonly int ZoneFadeId = Shader.PropertyToID("_ZoneFade");
        private static readonly int MaxDistanceId = Shader.PropertyToID("_MaxDistance");

        [SerializeField, Tooltip("The Terrain the mist lies on.")]
        private Terrain terrain;

        private MeshRenderer mistRenderer;

        private void OnEnable()
        {
            mistRenderer = GetComponent<MeshRenderer>();

            if (terrain == null)
            {
                Debug.LogError($"GroundMist '{name}' has no Terrain: the mist will not follow the ground.", this);
                return;
            }

            TerrainData data = terrain.terrainData;
            Shader.SetGlobalTexture(HeightmapId, data.heightmapTexture);
            Shader.SetGlobalVector(TerrainOriginId, terrain.transform.position);
            Shader.SetGlobalVector(TerrainSizeId, data.size);
        }

        private void LateUpdate()
        {
            // Read every frame, so the zone can be tuned on the material while playing.
            Material material = mistRenderer.sharedMaterial;
            Vector4 centre = material.GetVector(ZoneCentreId);
            Vector4 halfSize = material.GetVector(ZoneHalfSizeId);
            float reach = material.GetFloat(ZoneFadeId) + material.GetFloat(MaxDistanceId);

            Vector3 position = transform.position;
            mistRenderer.enabled = Mathf.Abs(position.x - centre.x) < halfSize.x + reach
                && Mathf.Abs(position.z - centre.y) < halfSize.y + reach;
        }
    }
}
