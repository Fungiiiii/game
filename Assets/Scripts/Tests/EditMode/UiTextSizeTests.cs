using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.Tests.EditMode
{
    /// <summary>
    /// The interface has four text sizes (CLAUDE.md, "UI text sizes"): 60 for a screen's
    /// title, 36 for a block's title, 26 for body text, 22 for small text, at 1080p. The
    /// inventory's canvas is 720 units high, so there they are 40, 24, 17 and 15.
    /// </summary>
    public sealed class UiTextSizeTests
    {
        private const string UiFolder = "Assets/Prefabs/UI";

        private static readonly int[] At1080 = { 60, 36, 26, 22 };
        private static readonly int[] At720 = { 40, 24, 17, 15 };

        /// <summary>Prefabs without a canvas of their own, by the canvas they are shown in.</summary>
        private static readonly Dictionary<string, int[]> ShownIn = new Dictionary<string, int[]>
        {
            { "RecipeRow", At720 },
            { "IngredientRow", At720 },
            { "InventorySlot", At720 },
            { "HotbarSlot", At1080 },
            { "HintRow", At1080 },
            { "PickupLine", At1080 },
            { "TrackedRecipe", At1080 },
            { "TrackedIngredientRow", At1080 },
            { "RequirementRow", At1080 },
        };

        /// <summary>Letters drawn on a key belong to the key; the home screen's and the end's titles and the planks are display text.</summary>
        private static readonly HashSet<string> Exempt = new HashSet<string>
        {
            "HintKey/Text", "HintFace/Text", "HintPill/Text", "KeyPrompt/Letter", "RequirementPanel/Letter",
            "MainMenu/Title", "DemoEnd/Title", "PlankButton/Label",
        };

        [Test]
        public void EveryUiText_UsesOneOfTheFourSizes()
        {
            var wrong = new List<string>();
            foreach (var path in AssetDatabase.FindAssets("t:Prefab", new[] { UiFolder }).Select(AssetDatabase.GUIDToAssetPath))
            {
                var prefab = Path.GetFileNameWithoutExtension(path);
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var texts = root.GetComponentsInChildren<Text>(true)
                    .Where(text => PrefabUtility.GetCorrespondingObjectFromSource(text) == null)
                    .Where(text => !Exempt.Contains($"{prefab}/{text.name}"))
                    .ToArray();
                if (texts.Length == 0)
                {
                    continue;
                }

                var allowed = SizesFor(root, prefab);
                if (allowed == null)
                {
                    wrong.Add($"{prefab}: no canvas of its own; say in {nameof(ShownIn)} which canvas shows it.");
                    continue;
                }

                wrong.AddRange(texts
                    .Where(text => !allowed.Contains(text.fontSize))
                    .Select(text => $"{prefab}/{text.name}: {text.fontSize}, expected one of {string.Join(", ", allowed)}"));
            }

            Assert.That(wrong, Is.Empty, string.Join("\n", wrong));
        }

        private static int[] SizesFor(GameObject root, string prefab)
        {
            var scaler = root.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                return Mathf.Approximately(scaler.referenceResolution.y, 720f) ? At720 : At1080;
            }

            return ShownIn.TryGetValue(prefab, out var sizes) ? sizes : null;
        }
    }
}
