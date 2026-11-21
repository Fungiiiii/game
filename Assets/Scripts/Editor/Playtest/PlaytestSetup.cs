using System.Linq;
using Fungiiiii.Demo;
using Fungiiiii.Editor.World;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Fungiiiii.Editor.Playtest
{
    /// <summary>
    /// Writes what the playtest needs through the Editor API, so its scenes are never
    /// edited by hand: every step can be run again from the Fungiiiii/Playtest menu, or
    /// headlessly with -executeMethod.
    /// </summary>
    internal static class PlaytestSetup
    {
        internal const string DemoLoopScenePath = "Assets/Scenes/Prototype/DemoLoopScene.unity";

        private const string ForestScenePath = "Assets/Scenes/Biomes/MysticForest.unity";
        private const string OutlineShaderPath = "Assets/Art/Shaders/ItemOutline.shader";
        private const string FontPath = "Assets/Art/Fonts/Fredoka_SemiBold.ttf";
        private const string PlayerPath = "Builds/Playtest/Fungiiiii.exe";
        private const string GraphicsSettingsPath = "ProjectSettings/GraphicsSettings.asset";

        /// <summary>
        /// The placeholders of the net and of the mushrooms' state discs build their
        /// materials from this shader, found by name. No asset uses it, so a build would
        /// leave it out and the placeholders would fall back to a lit material.
        /// </summary>
        private const string PlaceholderShaderName = "Universal Render Pipeline/Unlit";

        private static readonly string[] StickTreePrefabPaths =
        {
            "Assets/Prefabs/Environment/PF_Tree.prefab",
            "Assets/Prefabs/Environment/PF_TreeBroad.prefab",
            "Assets/Prefabs/Environment/PF_TreePine.prefab",
        };

        /// <summary>The home screen comes first: it is the scene a build starts on.</summary>
        private static readonly string[] BuildScenePaths =
        {
            MainMenuSceneBuilder.ScenePath,
            DemoLoopScenePath,
            ForestScenePath,
        };

        /// <summary>Gives the demo loop the assets its gathering and its pause menu need.</summary>
        [MenuItem("Fungiiiii/Playtest/3. Wire Demo Loop Scene")]
        internal static void WireDemoLoopScene()
        {
            var scene = EditorSceneManager.OpenScene(DemoLoopScenePath, OpenSceneMode.Single);
            var bootstrap = Object.FindFirstObjectByType<DemoLoopBootstrap>();
            if (bootstrap == null)
            {
                throw new System.InvalidOperationException($"{DemoLoopScenePath} has no DemoLoopBootstrap.");
            }

            var serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("outlineShader").objectReferenceValue = Load<Shader>(OutlineShaderPath);
            serialized.FindProperty("menuFont").objectReferenceValue = Load<Font>(FontPath);
            serialized.FindProperty("mainMenuScenePath").stringValue = MainMenuSceneBuilder.ScenePath;

            var trees = serialized.FindProperty("stickTreePrefabs");
            trees.arraySize = StickTreePrefabPaths.Length;
            for (var index = 0; index < StickTreePrefabPaths.Length; index++)
            {
                trees.GetArrayElementAtIndex(index).objectReferenceValue = Load<GameObject>(StickTreePrefabPaths[index]);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        /// <summary>The scenes of the build, in order. Menus load one another by path: all three must be listed.</summary>
        [MenuItem("Fungiiiii/Playtest/4. Set Build Scenes")]
        internal static void SetBuildScenes()
        {
            EditorBuildSettings.scenes = BuildScenePaths
                .Select(path => new EditorBuildSettingsScene(path, true))
                .ToArray();
            AssetDatabase.SaveAssets();
        }

        /// <summary>Adds the placeholders' shader to the shaders every build includes.</summary>
        [MenuItem("Fungiiiii/Playtest/5. Include Placeholder Shader")]
        internal static void IncludePlaceholderShader()
        {
            var shader = Shader.Find(PlaceholderShaderName);
            if (shader == null)
            {
                throw new System.InvalidOperationException($"Shader not found: {PlaceholderShaderName}");
            }

            var graphicsSettings = AssetDatabase.LoadAllAssetsAtPath(GraphicsSettingsPath).FirstOrDefault();
            if (graphicsSettings == null)
            {
                throw new System.InvalidOperationException($"Missing asset: {GraphicsSettingsPath}");
            }

            var serialized = new SerializedObject(graphicsSettings);
            var included = serialized.FindProperty("m_AlwaysIncludedShaders");
            for (var index = 0; index < included.arraySize; index++)
            {
                if (included.GetArrayElementAtIndex(index).objectReferenceValue == shader)
                {
                    return;
                }
            }

            included.arraySize++;
            included.GetArrayElementAtIndex(included.arraySize - 1).objectReferenceValue = shader;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Rewrites the Mystic Forest's trees, rocks and grass from its scatter layers, as
        /// the "Generate forest" button of a layer does, and saves the scene and its
        /// Terrain data. Run it after changing a layer or the generator. Not part of Set
        /// Up Everything: it changes Hugo's forest.
        /// </summary>
        [MenuItem("Fungiiiii/Playtest/6. Regenerate Mystic Forest")]
        internal static void RegenerateMysticForest()
        {
            var scene = EditorSceneManager.OpenScene(ForestScenePath, OpenSceneMode.Single);
            var terrain = Object.FindFirstObjectByType<Terrain>();
            if (terrain == null)
            {
                throw new System.InvalidOperationException($"{ForestScenePath} has no Terrain.");
            }

            var written = TerrainForestGenerator.Generate(terrain);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"Mystic Forest regenerated: {written} trees, rocks and plants.");
        }

        [MenuItem("Fungiiiii/Playtest/Set Up Everything")]
        internal static void SetUpEverything()
        {
            PlaytestLocalization.SetUp();
            MainMenuSceneBuilder.Build();
            WireDemoLoopScene();
            SetBuildScenes();
            IncludePlaceholderShader();
        }

        /// <summary>Builds the Windows executable of the playtest into Builds/Playtest, which is not version controlled.</summary>
        [MenuItem("Fungiiiii/Playtest/Build Windows Player")]
        internal static void BuildWindowsPlayer()
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = BuildScenePaths,
                locationPathName = PlayerPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            });

            var summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new System.InvalidOperationException(
                    $"The playtest build failed: {summary.result}, {summary.totalErrors} error(s).");
            }

            Debug.Log($"Playtest built: {summary.outputPath} ({summary.totalSize / (1024 * 1024)} MB).");
        }

        internal static T Load<T>(string path)
            where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new System.InvalidOperationException($"Missing asset: {path}");
            }

            return asset;
        }
    }
}
