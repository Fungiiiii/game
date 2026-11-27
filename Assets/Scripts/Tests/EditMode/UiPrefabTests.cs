using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class UiPrefabTests
    {
        private const string Folder = "Assets/Prefabs/UI";

        private static readonly Regex GameObjectOf = new Regex(@"m_GameObject: \{fileID: (-?\d+)\}");
        private static readonly Regex ScriptOf = new Regex(@"m_EditorClassIdentifier: (\S*)");

        /// <summary>
        /// Read from the files: a graphic whose class requires a CanvasRenderer gets one in
        /// memory when its prefab loads, so a loaded prefab hides a file saved without it,
        /// and a graphic without one is never drawn, with no error.
        /// </summary>
        [Test]
        public void EveryGraphicOfTheInterface_IsSavedWithTheCanvasRendererThatDrawsIt()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { Folder });
            Assert.That(guids, Is.Not.Empty, $"No prefab in {Folder}.");

            var undrawn = new List<string>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                undrawn.AddRange(UndrawnGraphics(path, File.ReadAllText(path)));
            }

            Assert.That(undrawn, Is.Empty);
        }

        private static IEnumerable<string> UndrawnGraphics(string path, string yaml)
        {
            var graphics = new Dictionary<string, string>();
            var renderers = new HashSet<string>();
            foreach (var document in yaml.Split(new[] { "\n--- " }, System.StringSplitOptions.None))
            {
                var owner = GameObjectOf.Match(document);
                if (!owner.Success)
                {
                    continue;
                }

                if (document.Contains("\nCanvasRenderer:"))
                {
                    renderers.Add(owner.Groups[1].Value);
                }
                else if (document.Contains("\nMonoBehaviour:") && document.Contains("m_RaycastTarget:"))
                {
                    // Every Graphic, and only a Graphic, serializes m_RaycastTarget.
                    graphics[owner.Groups[1].Value] = ScriptOf.Match(document).Groups[1].Value;
                }
            }

            foreach (var graphic in graphics)
            {
                if (!renderers.Contains(graphic.Key))
                {
                    yield return $"{path}: {graphic.Value} on object {graphic.Key}";
                }
            }
        }
    }
}
