using Fungiiiii.Runtime.World;
using UnityEditor;
using UnityEngine;

namespace Fungiiiii.Editor.World
{
    /// <summary>
    /// Adds "Generate relief" buttons to the TerrainRelief inspector. The forest is
    /// regenerated right after, because tree heights and slope filters depend on
    /// the ground. Each action is a single undo step.
    /// </summary>
    [CustomEditor(typeof(TerrainRelief))]
    internal sealed class TerrainReliefEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var relief = (TerrainRelief)target;
            Terrain terrain = relief.GetComponentInParent<Terrain>();

            EditorGUILayout.Space();
            if (terrain == null)
            {
                EditorGUILayout.HelpBox("Place this object under a Terrain: the relief is written into its heightmap.", MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox("Generate relief rewrites the heightmap and the ground layers, then regenerates the forest.", MessageType.Info);

            if (GUILayout.Button("Generate relief + forest", GUILayout.Height(28)))
            {
                Run(relief, terrain, "Generate relief");
            }

            if (GUILayout.Button("New seed + Generate relief + forest"))
            {
                var serialized = new SerializedObject(relief);
                Undo.IncrementCurrentGroup();
                int group = Undo.GetCurrentGroup();
                serialized.FindProperty("settings.seed").intValue = Random.Range(1, 100000);
                serialized.ApplyModifiedProperties();
                Generate(relief, terrain);
                Undo.CollapseUndoOperations(group);
            }
        }

        private static void Run(TerrainRelief relief, Terrain terrain, string name)
        {
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(name);
            Generate(relief, terrain);
            Undo.CollapseUndoOperations(group);
        }

        private static void Generate(TerrainRelief relief, Terrain terrain)
        {
            TerrainReliefTool.Apply(relief);
            if (terrain.GetComponentInChildren<ForestScatter>() != null)
            {
                int count = TerrainForestGenerator.Generate(terrain);
                Debug.Log($"[TerrainRelief] Relief generated, {count} trees regenerated on {terrain.name}.", terrain);
            }
        }
    }
}
