using System.Collections;
using System.Linq;
using Fungiiiii.Game;
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
            // Waited for: the scene of a previous test, and its menu, go away only once it is loaded.
            yield return SceneManager.LoadSceneAsync(MenuScenePath, LoadSceneMode.Single);
            var menu = Object.FindFirstObjectByType<MainMenu>();
            Assert.That(menu, Is.Not.Null, "The home screen did not load.");

            // The texts come from the string tables: empty labels mean they did not load.
            var playLabel = menu.PlayButton.Label.GetComponent<Text>();
            yield return WaitFor(MenuTimeoutSeconds, () => !string.IsNullOrEmpty(playLabel.text));
            Assert.That(playLabel.text, Is.Not.Empty, "The Play button has no text.");
            Assert.That(playLabel.text, Does.Not.Contain("No translation"), "The Play button's text is missing from the table.");

            menu.Play();
            GameBootstrap game = null;
            yield return WaitFor(GameTimeoutSeconds, () => (game = Object.FindFirstObjectByType<GameBootstrap>()) != null && game.IsReady);
            Assert.That(game, Is.Not.Null, "Play did not load the game.");
            Assert.That(game.IsReady, Is.True, "The game was not ready in time.");
            game.Intro.Skip();

            // Since the playtest of 22/11, 2 sticks and 3 mushrooms are kept around the player;
            // since the playtest loop, 4 pebbles too.
            Assert.That(game.Gatherables.Count(gatherable => gatherable.IsAvailable), Is.EqualTo(9), "Not everything to gather is around the player.");
            var outline = game.Gatherables
                .First(gatherable => gatherable.IsAvailable)
                .GetComponentsInChildren<MeshRenderer>()
                .FirstOrDefault(renderer => renderer.name == "Outline");
            Assert.That(outline, Is.Not.Null, "What can be gathered has no outline.");
            Assert.That(outline.sharedMaterial.shader.isSupported, Is.True, "The outline shader does not run here.");

            game.Screens.Open(game.Pause);
            yield return null;
            Assert.That(Time.timeScale, Is.Zero);
            game.Pause.HomeButton.Button.onClick.Invoke();

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
