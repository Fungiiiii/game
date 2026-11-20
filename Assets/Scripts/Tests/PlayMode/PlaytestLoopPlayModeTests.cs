using System.Collections;
using System.Linq;
using Fungiiiii.Demo;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Fungiiiii.Tests.PlayMode
{
    /// <summary>
    /// The whole way a playtester goes: home screen, Play, the game in the forest, pause,
    /// back home. It loads the scenes as a build does, by path through the build settings,
    /// and uses nothing of the Editor.
    /// </summary>
    public sealed class PlaytestLoopPlayModeTests
    {
        private const string MenuScenePath = "Assets/Scenes/Menu/MainMenu.unity";
        private const float MenuTimeoutSeconds = 30f;
        private const float GameTimeoutSeconds = 120f;

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator FromTheHomeScreen_PlayReachesTheGame_AndPauseComesBackHome()
        {
            SceneManager.LoadScene(MenuScenePath, LoadSceneMode.Single);
            MainMenu menu = null;
            yield return WaitFor(MenuTimeoutSeconds, () => (menu = Object.FindFirstObjectByType<MainMenu>()) != null && menu.PlayButton != null);
            Assert.That(menu, Is.Not.Null, "The home screen did not load.");
            Assert.That(menu.PlayButton, Is.Not.Null, "The home screen was not built.");

            // The texts come from the string tables: empty labels mean they did not load.
            var playLabel = menu.PlayButton.Label.GetComponent<Text>();
            yield return WaitFor(MenuTimeoutSeconds, () => !string.IsNullOrEmpty(playLabel.text));
            Assert.That(playLabel.text, Is.Not.Empty, "The Play button has no text.");
            Assert.That(playLabel.text, Does.Not.Contain("No translation"), "The Play button's text is missing from the table.");

            menu.Play();
            DemoLoopBootstrap demo = null;
            yield return WaitFor(GameTimeoutSeconds, () => (demo = Object.FindFirstObjectByType<DemoLoopBootstrap>()) != null && demo.IsReady);
            Assert.That(demo, Is.Not.Null, "Play did not load the game.");
            Assert.That(demo.IsReady, Is.True, "The game was not ready in time.");

            Assert.That(demo.Gatherables.Count(gatherable => gatherable.IsAvailable), Is.GreaterThan(50), "The map has almost nothing to gather.");
            var outline = demo.Gatherables
                .First(gatherable => gatherable.IsAvailable)
                .GetComponentsInChildren<MeshRenderer>()
                .FirstOrDefault(renderer => renderer.name == "Outline");
            Assert.That(outline, Is.Not.Null, "What can be gathered has no outline.");
            Assert.That(outline.sharedMaterial.shader.isSupported, Is.True, "The outline shader does not run here.");

            demo.Pause.SetPaused(true);
            yield return null;
            Assert.That(Time.timeScale, Is.Zero);
            demo.Pause.HomeButton.Button.onClick.Invoke();

            menu = null;
            yield return WaitFor(MenuTimeoutSeconds, () => (menu = Object.FindFirstObjectByType<MainMenu>()) != null && menu.PlayButton != null);
            Assert.That(menu, Is.Not.Null, "Home did not come back to the home screen.");
            Assert.That(Time.timeScale, Is.EqualTo(1f), "Time is still stopped on the home screen.");
        }

        private static IEnumerator WaitFor(float timeoutSeconds, System.Func<bool> condition)
        {
            var elapsed = 0f;
            while (elapsed < timeoutSeconds && !condition())
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}
