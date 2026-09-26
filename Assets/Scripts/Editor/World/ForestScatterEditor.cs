using Fungiiiii.Runtime.World;
using UnityEditor;
using UnityEngine;

namespace Fungiiiii.Editor.World
{
    /// <summary>
    /// Adds "Generate forest" / "Clear forest" buttons to the ForestScatter inspector.
    /// A layer writes into the Terrain it sits under. Every action is a single Undo step.
    /// </summary>
    [CustomEditor(typeof(ForestScatter))]
    internal sealed class ForestScatterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var scatter = (ForestScatter)target;
            Terrain terrain = scatter.GetComponentInParent<Terrain>();

            EditorGUILayout.Space();
            if (terrain == null)
            {
                EditorGUILayout.HelpBox("Place this layer under a Terrain: its instances are written into the Terrain's trees.", MessageType.Warning);
                return;
            }

            if (scatter.Prefab == null)
            {
                EditorGUILayout.HelpBox("Assign a prefab with an LODGroup on its root, such as PF_Tree.", MessageType.Warning);
            }

            EditorGUILayout.HelpBox("Generate rewrites every layer under this Terrain at once.", MessageType.Info);

            using (new EditorGUI.DisabledScope(scatter.Prefab == null))
            {
                if (GUILayout.Button("Generate forest", GUILayout.Height(28)))
                {
                    RunAsUndoGroup("Generate forest", () => Log(terrain, TerrainForestGenerator.Generate(terrain)));
                }

                if (GUILayout.Button("New seed + Generate forest"))
                {
                    ReseedAndGenerate(scatter, Random.Range(1, 100000));
                }
            }

            if (GUILayout.Button("Clear forest"))
            {
                RunAsUndoGroup("Clear forest", () => TerrainForestGenerator.Clear(terrain));
            }
        }

        /// <summary>
        /// Changes the seed and regenerates as one undo step: a single Ctrl+Z
        /// restores both the previous seed and the previous forest.
        /// </summary>
        internal static void ReseedAndGenerate(ForestScatter scatter, int newSeed)
        {
            Terrain terrain = scatter.GetComponentInParent<Terrain>();
            RunAsUndoGroup("Reseed and generate forest", () =>
            {
                var serialized = new SerializedObject(scatter);
                serialized.FindProperty("seed").intValue = newSeed;
                serialized.ApplyModifiedProperties();
                Log(terrain, TerrainForestGenerator.Generate(terrain));
            });
        }

        private static void Log(Terrain terrain, int count)
        {
            Debug.Log($"[ForestScatter] Generated {count} trees on {terrain.name}.", terrain);
        }

        private static void RunAsUndoGroup(string name, System.Action action)
        {
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(name);
            action();
            Undo.CollapseUndoOperations(group);
        }
    }
}
