#if UNITY_EDITOR
using System.Collections;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Fungiiiii.Tests.PlayMode.PlaytestLoopGame;

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>The end of the demo, inside the Champimaison.</summary>
    public sealed class DemoEndPlayModeTests : InputTestFixture
    {
        private const string HomeScenePath = "Assets/Scenes/Menu/MainMenu.unity";

        [UnityTest]
        public IEnumerator Escape_AtTheEndOfTheDemo_GoesBackToTheHomeScreen()
        {
            // Since the playtest of 23/11: Escape did nothing there.
            var keyboard = InputSystem.AddDevice<Keyboard>();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(InteriorScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return WaitUntil(() => Object.FindFirstObjectByType<DemoEnd>() != null, 20f);
            Assert.That(Object.FindFirstObjectByType<DemoEnd>(), Is.Not.Null, "No end of the demo inside.");
            yield return null;

            Press(keyboard.escapeKey);
            yield return null;
            Release(keyboard.escapeKey);
            yield return WaitUntil(() => SceneManager.GetActiveScene().path == HomeScenePath, 20f);

            Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(HomeScenePath), "Escape did not go back to the home screen.");
        }
    }
}
#endif
