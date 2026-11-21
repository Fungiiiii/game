using System.Linq;
using Fungiiiii.Editor.World;
using Fungiiiii.Runtime.World;
using Fungiiiii.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace Fungiiiii.Editor.Playtest
{
    /// <summary>
    /// Writes the MainMenu scene: a small forest on a gentle slope, generated with the
    /// tools of the Mystic Forest (scatter layers, groves, grass), under the same light,
    /// sky, fog and post-processing, which are copied from the forest's scene.
    /// Running it again rewrites the scene and its Terrain data in place.
    /// </summary>
    internal static class MainMenuSceneBuilder
    {
        internal const string ScenePath = "Assets/Scenes/Menu/MainMenu.unity";

        private const string SceneFolder = "Assets/Scenes/Menu";
        private const string TerrainDataPath = SceneFolder + "/MainMenuTerrainData.asset";
        private const string ForestScenePath = "Assets/Scenes/Biomes/MysticForest.unity";
        private const string MainMenuPrefabPath = "Assets/Prefabs/UI/MainMenu.prefab";
        private const string AmbiencePath = "Assets/Audio/MysticForest/AMB_MysticForest_ZoneA.wav";
        private const string GrassTuftPath = "Assets/Art/Models/Environment/GrassTuft.fbx";
        private const string PrefabFolder = "Assets/Prefabs/Environment/";
        private const string EditorOnlyTag = "EditorOnly";

        /// <summary>The Terrain is a square of this side, centred on the origin, with 2 m facets like the forest's.</summary>
        private const float TerrainSide = 256f;
        private const float TerrainHeight = 60f;
        private const int HeightmapResolution = 129;
        private const int DetailResolution = 128;
        private const int AlphamapResolution = 256;

        /// <summary>The slope climbs northwards (+Z) by this much between its foot and its top, in metres.</summary>
        private const float SlopeRise = 24f;
        private const float SlopeFootZ = -80f;
        private const float SlopeTopZ = 80f;
        private const float BaseHeight = 9f;
        private const float HillAmplitude = 6f;
        private const float HillWavelength = 75f;
        private const float GroundRoughness = 0.5f;
        private const int ReliefSeed = 4107;

        /// <summary>Moss patches, as in the forest: grass grows on green, the rest is forest floor.</summary>
        private const int ForestFloorLayer = 0;
        private const int MossLayer = 1;
        private const float MossPatchSize = 22f;

        /// <summary>The camera stands at the foot of the slope and looks up it.</summary>
        private static readonly Vector2 CameraGround = new Vector2(14f, -96f);
        private static readonly Vector2 CameraLooksAt = new Vector2(-10f, 40f);
        private const float EyeHeight = 1.7f;
        private const float LookAtHeightAboveGround = 5f;

        /// <summary>Open ground in front of the camera, so no trunk hides the view.</summary>
        private static readonly (Vector2 Centre, float Radius)[] ViewClearings =
        {
            (new Vector2(13f, -90f), 11f),
            (new Vector2(8f, -70f), 9f),
            (new Vector2(3f, -52f), 7f),
        };

        private const float AmbienceVolume = 0.6f;
        private const float FogEndDistance = 150f;

        [MenuItem("Fungiiiii/Playtest/2. Build Main Menu Scene")]
        internal static void Build()
        {
            var forest = EditorSceneManager.OpenScene(ForestScenePath, OpenSceneMode.Single);
            var look = SceneLook.Capture();
            var forestTerrain = FindInScene<Terrain>(forest);
            var forestCamera = FindInScene<Camera>(forest);
            var sun = FindInScene<Light>(forest);
            var postProcess = forest.GetRootGameObjects().FirstOrDefault(root => root.name == "PostProcess");

            if (forestTerrain == null || forestCamera == null || sun == null || postProcess == null)
            {
                throw new System.InvalidOperationException(
                    "The Mystic Forest scene no longer has the Terrain, camera, light or PostProcess object the menu copies.");
            }

            var menu = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            CopyInto(menu, sun.gameObject);
            CopyInto(menu, postProcess);
            var cameraObject = CopyInto(menu, forestCamera.gameObject);

            SceneManager.SetActiveScene(menu);
            look.Apply();
            RenderSettings.fogEndDistance = Mathf.Min(RenderSettings.fogEndDistance, FogEndDistance);
            RenderSettings.sun = menu.GetRootGameObjects().Select(root => root.GetComponent<Light>()).First(light => light != null);

            var terrain = CreateTerrain(forestTerrain);
            EditorSceneManager.CloseScene(forest, true);

            AddClearings();
            AddForestLayers(terrain);
            TerrainForestGenerator.Generate(terrain);

            PlaceCamera(cameraObject, terrain);
            AddAmbience();
            AddMenu();

            EditorSceneManager.MarkSceneDirty(menu);
            EditorSceneManager.SaveScene(menu, ScenePath);
            AssetDatabase.SaveAssets();
        }

        private static Terrain CreateTerrain(Terrain forestTerrain)
        {
            if (!AssetDatabase.IsValidFolder(SceneFolder))
            {
                AssetDatabase.CreateFolder("Assets/Scenes", "Menu");
            }

            // Reused when it exists, so its GUID never changes.
            var data = AssetDatabase.LoadAssetAtPath<TerrainData>(TerrainDataPath);
            if (data == null)
            {
                data = new TerrainData();
                AssetDatabase.CreateAsset(data, TerrainDataPath);
            }

            // The resolution first: setting it resets the size.
            data.heightmapResolution = HeightmapResolution;
            data.size = new Vector3(TerrainSide, TerrainHeight, TerrainSide);
            data.alphamapResolution = AlphamapResolution;
            data.baseMapResolution = AlphamapResolution;
            data.SetDetailResolution(DetailResolution, 16);
            data.terrainLayers = forestTerrain.terrainData.terrainLayers;
            data.SetHeights(0, 0, Heights());
            data.SetAlphamaps(0, 0, Alphamaps(data.alphamapLayers));
            EditorUtility.SetDirty(data);

            var terrainObject = Terrain.CreateTerrainGameObject(data);
            terrainObject.name = "Terrain";
            terrainObject.transform.position = new Vector3(-TerrainSide * 0.5f, 0f, -TerrainSide * 0.5f);

            var terrain = terrainObject.GetComponent<Terrain>();
            terrain.materialTemplate = forestTerrain.materialTemplate;
            terrain.treeDistance = forestTerrain.treeDistance;
            terrain.treeBillboardDistance = forestTerrain.treeBillboardDistance;
            terrain.treeCrossFadeLength = forestTerrain.treeCrossFadeLength;
            terrain.treeMaximumFullLODCount = forestTerrain.treeMaximumFullLODCount;
            terrain.detailObjectDistance = forestTerrain.detailObjectDistance;
            terrain.detailObjectDensity = forestTerrain.detailObjectDensity;
            terrain.heightmapPixelError = forestTerrain.heightmapPixelError;
            terrain.basemapDistance = forestTerrain.basemapDistance;
            terrain.shadowCastingMode = forestTerrain.shadowCastingMode;
            terrain.drawInstanced = forestTerrain.drawInstanced;
            return terrain;
        }

        private static float[,] Heights()
        {
            var heights = new float[HeightmapResolution, HeightmapResolution];
            var cell = TerrainSide / (HeightmapResolution - 1);

            for (var z = 0; z < HeightmapResolution; z++)
            {
                for (var x = 0; x < HeightmapResolution; x++)
                {
                    heights[z, x] = GroundHeightAt(x * cell - TerrainSide * 0.5f, z * cell - TerrainSide * 0.5f) / TerrainHeight;
                }
            }

            return heights;
        }

        /// <summary>A gentle slope climbing north, soft hills across it, and a faint roughness for the facets.</summary>
        private static float GroundHeightAt(float worldX, float worldZ)
        {
            var position = new Vector2(worldX, worldZ);
            var climb = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(SlopeFootZ, SlopeTopZ, worldZ)) * SlopeRise;
            var hills = (ValueNoise.Fbm(position / HillWavelength, 2, ReliefSeed) - 0.5f) * HillAmplitude;
            var roughness = (ValueNoise.Sample(position / 9f, ReliefSeed + 1) - 0.5f) * GroundRoughness;
            return BaseHeight + climb + hills + roughness;
        }

        private static float[,,] Alphamaps(int layerCount)
        {
            var maps = new float[AlphamapResolution, AlphamapResolution, layerCount];
            var cell = TerrainSide / AlphamapResolution;

            for (var z = 0; z < AlphamapResolution; z++)
            {
                for (var x = 0; x < AlphamapResolution; x++)
                {
                    var position = new Vector2((x + 0.5f) * cell - TerrainSide * 0.5f, (z + 0.5f) * cell - TerrainSide * 0.5f);
                    var moss = ValueNoise.Fbm(position / MossPatchSize, 2, ReliefSeed + 2) > 0.47f;
                    maps[z, x, moss ? MossLayer : ForestFloorLayer] = 1f;
                }
            }

            return maps;
        }

        private static void AddClearings()
        {
            var clearings = new GameObject("Clearings") { tag = EditorOnlyTag };
            foreach (var (centre, radius) in ViewClearings)
            {
                var clearing = new GameObject("ViewClearing") { tag = EditorOnlyTag };
                clearing.transform.SetParent(clearings.transform, false);
                clearing.transform.position = new Vector3(centre.x, 0f, centre.y);
                clearing.AddComponent<ScatterExclusionZone>().Radius = radius;
            }
        }

        /// <summary>
        /// The layers in priority order, as in the forest: dominant trees first, undergrowth
        /// after. Warm broadleaves on the slope, dark pines along its top.
        /// </summary>
        private static void AddForestLayers(Terrain terrain)
        {
            var whole = new Vector2(TerrainSide, TerrainSide);
            var top = new Vector2(TerrainSide, 110f);

            AddLayer(terrain, "Menu_TreeBroad", "PF_TreeBroad", Vector2.zero, whole, 9f, 0.75f, 1.35f, 51001, 25f, 38f, 0.6f, 0.15f);
            AddLayer(terrain, "Menu_TreePine", "PF_TreePine", new Vector2(0f, 73f), top, 6.5f, 0.8f, 1.45f, 51002, 25f, 30f, 0.7f, 0.15f);
            AddLayer(terrain, "Menu_Tree", "PF_Tree", Vector2.zero, whole, 17f, 0.8f, 1.3f, 51003, 25f, 0f, 0f, 0.15f);
            AddLayer(terrain, "Menu_Bush", "PF_Bush", Vector2.zero, whole, 6f, 0.7f, 1.3f, 51004, 30f, 25f, 0.5f, 0f);
            AddLayer(terrain, "Menu_Mushrooms", "PF_MushroomCluster", Vector2.zero, whole, 8f, 0.8f, 1.7f, 51005, 30f, 28f, 0.35f, 0f);
            AddLayer(terrain, "Menu_Fern", "PF_Fern", Vector2.zero, whole, 3f, 0.8f, 1.6f, 51006, 30f, 22f, 0.5f, 0f);
            AddLayer(terrain, "Menu_Rocks", "PF_Rock_Medium", Vector2.zero, whole, 16f, 0.8f, 1.6f, 51007, 90f, 0f, 0f, 0f);

            var grassObject = new GameObject("Grass") { tag = EditorOnlyTag };
            grassObject.transform.SetParent(terrain.transform, false);
            grassObject.AddComponent<TerrainGrass>().Configure(
                PlaytestSetup.Load<GameObject>(GrassTuftPath), 51101, 14f, 0.68f, 12, 30f);
        }

        private static void AddLayer(
            Terrain terrain,
            string layerName,
            string prefabName,
            Vector2 centre,
            Vector2 area,
            float minDistance,
            float minScale,
            float maxScale,
            int seed,
            float maxSlope,
            float groveSize,
            float groveCoverage,
            float heightVariation)
        {
            var layerObject = new GameObject(layerName) { tag = EditorOnlyTag };
            layerObject.transform.SetParent(terrain.transform, false);
            layerObject.transform.position = new Vector3(centre.x, 0f, centre.y);

            var layer = layerObject.AddComponent<ForestScatter>();
            layer.Configure(area, minDistance, minScale, maxScale, true, seed);
            layer.ConfigureLimits(Vector2.zero, maxSlope);
            layer.ConfigureGroves(groveSize, groveCoverage);
            layer.ConfigureHeightVariation(heightVariation);

            var serialized = new SerializedObject(layer);
            serialized.FindProperty("prefab").objectReferenceValue =
                PlaytestSetup.Load<GameObject>(PrefabFolder + prefabName + ".prefab");
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void PlaceCamera(GameObject cameraObject, Terrain terrain)
        {
            // The forest's camera carries its ground mist, which needs the forest's Terrain.
            var mist = cameraObject.GetComponentInChildren<GroundMist>(true);
            if (mist != null)
            {
                Object.DestroyImmediate(mist.gameObject);
            }

            var eye = OnGround(terrain, CameraGround) + Vector3.up * EyeHeight;
            var target = OnGround(terrain, CameraLooksAt) + Vector3.up * LookAtHeightAboveGround;
            cameraObject.name = "Main Camera";
            cameraObject.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(target - eye, Vector3.up));
            cameraObject.AddComponent<MenuCameraDrift>();

            if (cameraObject.GetComponent<AudioListener>() == null)
            {
                cameraObject.AddComponent<AudioListener>();
            }
        }

        private static Vector3 OnGround(Terrain terrain, Vector2 ground)
        {
            var point = new Vector3(ground.x, 0f, ground.y);
            point.y = terrain.SampleHeight(point) + terrain.transform.position.y;
            return point;
        }

        private static void AddAmbience()
        {
            var source = new GameObject("Ambience").AddComponent<AudioSource>();
            source.clip = PlaytestSetup.Load<AudioClip>(AmbiencePath);
            source.loop = true;
            source.playOnAwake = true;
            source.spatialBlend = 0f;
            source.volume = AmbienceVolume;
        }

        private static void AddMenu()
        {
            var menu = (GameObject)PrefabUtility.InstantiatePrefab(PlaytestSetup.Load<GameObject>(MainMenuPrefabPath));
            var serialized = new SerializedObject(menu.GetComponent<MainMenu>());
            serialized.FindProperty("gameScenePath").stringValue = PlaytestSetup.GameScenePath;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _ = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private static T FindInScene<T>(Scene scene)
            where T : Component
        {
            return scene.GetRootGameObjects()
                .Select(root => root.GetComponentInChildren<T>(true))
                .FirstOrDefault(component => component != null);
        }

        /// <summary>A copy of a scene object, at the root of another scene, in the same pose.</summary>
        private static GameObject CopyInto(Scene scene, GameObject source)
        {
            var copy = Object.Instantiate(source);
            copy.name = source.name;
            copy.transform.SetParent(null, true);
            SceneManager.MoveGameObjectToScene(copy, scene);
            return copy;
        }

        /// <summary>Sky, fog and ambient light of the active scene: they are settings of a scene, not objects.</summary>
        private readonly struct SceneLook
        {
            private readonly Material skybox;
            private readonly bool fog;
            private readonly FogMode fogMode;
            private readonly Color fogColor;
            private readonly float fogDensity;
            private readonly float fogStart;
            private readonly float fogEnd;
            private readonly UnityEngine.Rendering.AmbientMode ambientMode;
            private readonly Color ambientSky;
            private readonly Color ambientEquator;
            private readonly Color ambientGround;
            private readonly float ambientIntensity;
            private readonly float reflectionIntensity;
            private readonly Color subtractiveShadowColor;

            private SceneLook(bool _)
            {
                skybox = RenderSettings.skybox;
                fog = RenderSettings.fog;
                fogMode = RenderSettings.fogMode;
                fogColor = RenderSettings.fogColor;
                fogDensity = RenderSettings.fogDensity;
                fogStart = RenderSettings.fogStartDistance;
                fogEnd = RenderSettings.fogEndDistance;
                ambientMode = RenderSettings.ambientMode;
                ambientSky = RenderSettings.ambientSkyColor;
                ambientEquator = RenderSettings.ambientEquatorColor;
                ambientGround = RenderSettings.ambientGroundColor;
                ambientIntensity = RenderSettings.ambientIntensity;
                reflectionIntensity = RenderSettings.reflectionIntensity;
                subtractiveShadowColor = RenderSettings.subtractiveShadowColor;
            }

            public static SceneLook Capture() => new SceneLook(true);

            public void Apply()
            {
                RenderSettings.skybox = skybox;
                RenderSettings.fog = fog;
                RenderSettings.fogMode = fogMode;
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogDensity = fogDensity;
                RenderSettings.fogStartDistance = fogStart;
                RenderSettings.fogEndDistance = fogEnd;
                RenderSettings.ambientMode = ambientMode;
                RenderSettings.ambientSkyColor = ambientSky;
                RenderSettings.ambientEquatorColor = ambientEquator;
                RenderSettings.ambientGroundColor = ambientGround;
                RenderSettings.ambientIntensity = ambientIntensity;
                RenderSettings.reflectionIntensity = reflectionIntensity;
                RenderSettings.subtractiveShadowColor = subtractiveShadowColor;
            }
        }
    }
}
