using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The home screen: the game's name and two wooden planks, Play and Quit, over the
    /// small forest of the MainMenu scene. Play fades to black and loads the game.
    /// </summary>
    internal sealed class MainMenu : MonoBehaviour
    {
        /// <summary>Above everything of the scene, below nothing: the menu is the only UI here.</summary>
        private const int CanvasSortingOrder = 100;

        private static readonly Vector2 TitlePosition = new Vector2(144f, -150f);
        private static readonly Vector2 FirstButtonPosition = new Vector2(144f, -450f);
        private static readonly Vector2 ButtonSlide = new Vector2(18f, 0f);

        [Tooltip("Font of the title and of the buttons.")]
        [SerializeField]
        private Font font;

        [Tooltip("Path of the scene Play loads. It has to be in the build settings.")]
        [SerializeField]
        private string gameScenePath = "Assets/Scenes/Prototype/DemoLoopScene.unity";

        [Tooltip("Seconds of the fade from black when the menu appears, and to black when Play is pressed.")]
        [SerializeField, Min(0f)]
        private float fadeSeconds = 0.6f;

        private Image fade;

        public PlankButton PlayButton { get; private set; }

        public PlankButton QuitButton { get; private set; }

        /// <summary>True from the moment Play is pressed.</summary>
        public bool IsLoadingGame { get; private set; }

        private void Start()
        {
            if (font == null)
            {
                Debug.LogError("MainMenu has no font.", this);
                enabled = false;
                return;
            }

            // Coming back from a paused game: time runs again and the pointer is free.
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var canvas = WoodMenu.CreateCanvas("Main Menu Canvas", CanvasSortingOrder);
            var root = (RectTransform)canvas.transform;
            var topLeft = new Vector2(0f, 1f);

            WoodMenu.CreateTitle(root, font, UiText.GameTitle, TitlePosition);
            PlayButton = WoodMenu.CreateButton(
                root, font, UiText.MenuPlay, topLeft, FirstButtonPosition, ButtonSlide, 0.74f, Play);
            QuitButton = WoodMenu.CreateButton(
                root,
                font,
                UiText.MenuQuit,
                topLeft,
                FirstButtonPosition + Vector2.down * (WoodMenu.ButtonSize.y + WoodMenu.ButtonSpacing),
                ButtonSlide,
                0.82f,
                GameExit.Quit);

            fade = WoodMenu.CreateVeil(root, "Fade", Color.black);
            EventSystem.current.SetSelectedGameObject(PlayButton.gameObject);
            StartCoroutine(FadeTo(0f));
        }

        /// <summary>Fades to black, then loads the game.</summary>
        public void Play()
        {
            if (IsLoadingGame)
            {
                return;
            }

            IsLoadingGame = true;
            PlayButton.Button.interactable = false;
            QuitButton.Button.interactable = false;
            StartCoroutine(LoadGame());
        }

        private IEnumerator LoadGame()
        {
            yield return FadeTo(1f);
            yield return SceneManager.LoadSceneAsync(gameScenePath, LoadSceneMode.Single);
        }

        private IEnumerator FadeTo(float alpha)
        {
            var from = fade.color.a;
            var elapsed = 0f;

            while (elapsed < fadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                SetFade(Mathf.Lerp(from, alpha, elapsed / fadeSeconds));
                yield return null;
            }

            SetFade(alpha);
        }

        private void SetFade(float alpha)
        {
            fade.color = new Color(0f, 0f, 0f, alpha);
        }
    }
}
