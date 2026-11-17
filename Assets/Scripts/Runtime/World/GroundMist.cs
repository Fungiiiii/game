using UnityEngine;

namespace Fungiiiii.Runtime.World
{
    /// <summary>
    /// Gives the ground mist shader the relief of a Terrain, so the mist can hug the
    /// ground. It sits on the mist quad, child of the camera.
    /// </summary>
    [ExecuteAlways]
    internal sealed class GroundMist : MonoBehaviour
    {
        private static readonly int HeightmapId = Shader.PropertyToID("_MistHeightmap");
        private static readonly int TerrainOriginId = Shader.PropertyToID("_MistTerrainOrigin");
        private static readonly int TerrainSizeId = Shader.PropertyToID("_MistTerrainSize");

        [SerializeField, Tooltip("The Terrain the mist lies on.")]
        private Terrain terrain;

        private void OnEnable()
        {
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
    }
}
