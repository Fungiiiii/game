using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>The interface's prefabs, as the scenes hold them, for tests that need one alone.</summary>
    internal static class UiPrefabs
    {
        public static T Instantiate<T>(string prefabName)
            where T : Component
        {
#if UNITY_EDITOR
            var path = $"Assets/Prefabs/UI/{prefabName}.prefab";
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.That(prefab, Is.Not.Null, $"{path} has no {typeof(T).Name}.");
            return Object.Instantiate(prefab);
#else
            Assert.Ignore("The interface's prefabs are read from the editor's assets.");
            return null;
#endif
        }

        /// <summary>The event system a scene holds, for a prefab tested without its scene.</summary>
        public static GameObject CreateEventSystem()
        {
            return new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }
    }
}
