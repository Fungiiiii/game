#if UNITY_EDITOR
using System.Collections;
using Fungiiiii.Demo;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class MainMenuPlayModeTests
    {
        private const string ScenePath = "Assets/Scenes/Menu/MainMenu.unity";

        /// <summary>Loading the string tables takes a few frames; the game's forest a few seconds.</summary>
        private const float TextTimeoutSeconds = 10f;
        private const float GameTimeoutSeconds = 90f;

        private MainMenu menu;

        [UnitySetUp]
        public IEnumerator LoadMenu()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return LocalizationSettings.InitializationOperation;

            var elapsed = 0f;
            while (elapsed < TextTimeoutSeconds && (menu == null || menu.PlayButton == null))
            {
                menu = Object.FindFirstObjectByType<MainMenu>();
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(menu, Is.Not.Null, "The scene has no MainMenu.");
            Assert.That(menu.PlayButton, Is.Not.Null, "The menu was not built in time.");
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator TheHomeScreen_ShowsTheTitleAndTwoButtons_InFrench()
        {
            yield return SelectLocale("fr");

            yield return WaitForText(TextOf(menu.PlayButton), "Jouer");
            Assert.That(TextOf(menu.QuitButton).text, Is.EqualTo("Quitter"));
            Assert.That(GameObject.Find("Title").GetComponent<Text>().text, Is.EqualTo("Fungiiiii"));
        }

        [UnityTest]
        public IEnumerator ChangingTheLanguage_ChangesTheButtonsAtOnce()
        {
            yield return SelectLocale("fr");
            yield return WaitForText(TextOf(menu.PlayButton), "Jouer");

            yield return SelectLocale("en");

            yield return WaitForText(TextOf(menu.PlayButton), "Play");
            Assert.That(TextOf(menu.QuitButton).text, Is.EqualTo("Quit"));
        }

        [UnityTest]
        public IEnumerator PlayIsSelectedFirst_AndTheSelectedPlankStandsOut()
        {
            yield return new WaitForSecondsRealtime(0.5f);

            Assert.That(EventSystem.current.currentSelectedGameObject, Is.SameAs(menu.PlayButton.gameObject));
            Assert.That(menu.PlayButton.GetComponentInChildren<PlankGraphic>().Highlight, Is.EqualTo(1f).Within(0.01f));
            Assert.That(menu.QuitButton.GetComponentInChildren<PlankGraphic>().Highlight, Is.Zero);

            EventSystem.current.SetSelectedGameObject(menu.QuitButton.gameObject);
            yield return new WaitForSecondsRealtime(0.5f);

            Assert.That(menu.PlayButton.GetComponentInChildren<PlankGraphic>().Highlight, Is.Zero);
            Assert.That(menu.QuitButton.GetComponentInChildren<PlankGraphic>().Highlight, Is.EqualTo(1f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator TheHomeScreen_ShowsASmallForestOnASlope()
        {
            yield return null;
            var terrain = Terrain.activeTerrain;
            Assert.That(terrain, Is.Not.Null, "The menu has no Terrain.");
            Assert.That(terrain.terrainData.treeInstanceCount, Is.GreaterThan(100), "The menu has no forest.");

            var foot = terrain.SampleHeight(new Vector3(0f, 0f, -80f));
            var top = terrain.SampleHeight(new Vector3(0f, 0f, 80f));
            Assert.That(top - foot, Is.GreaterThan(10f), "The ground does not climb.");

            var view = Camera.main;
            Assert.That(view, Is.Not.Null);
            Assert.That(view.GetComponent<MenuCameraDrift>(), Is.Not.Null, "The camera does not float.");
            var ground = terrain.SampleHeight(view.transform.position) + terrain.GetPosition().y;
            Assert.That(view.transform.position.y, Is.GreaterThan(ground + 1f), "The camera is in the ground.");
        }

        [UnityTest]
        public IEnumerator Play_LoadsTheGame()
        {
            menu.Play();
            Assert.That(menu.IsLoadingGame, Is.True);

            DemoLoopBootstrap demo = null;
            var elapsed = 0f;
            while (elapsed < GameTimeoutSeconds && (demo == null || !demo.IsReady))
            {
                demo = Object.FindFirstObjectByType<DemoLoopBootstrap>();
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(demo, Is.Not.Null, "Play did not load the game.");
            Assert.That(demo.IsReady, Is.True, "The game was not ready in time.");
            Assert.That(Object.FindFirstObjectByType<MainMenu>(), Is.Null, "The home screen is still there.");
        }

        private static Text TextOf(PlankButton button)
        {
            return button.Label.GetComponent<Text>();
        }

        private static IEnumerator SelectLocale(string code)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(code);
            yield return null;
        }

        private static IEnumerator WaitForText(Text label, string expected)
        {
            var elapsed = 0f;
            while (elapsed < TextTimeoutSeconds && label.text != expected)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(label.text, Is.EqualTo(expected));
        }
    }
}
#endif
