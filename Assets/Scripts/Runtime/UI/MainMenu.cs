using System.Collections;
using UnityEngine;
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
        [SerializeField]
        private PlankButton playButton;

        [SerializeField]
        private PlankButton quitButton;

        [Tooltip("Black over the whole screen, faded out when the menu appears and in when Play is pressed.")]
        [SerializeField]
        private Image fade;

        [Tooltip("Path of the scene Play loads. It has to be in the build settings.")]
        [SerializeField]
        private string gameScenePath = "Assets/Scenes/Prototype/DemoLoopScene.unity";

        [Tooltip("Seconds of the fade from black when the menu appears, and to black when Play is pressed.")]
        [SerializeField, Min(0f)]
        private float fadeSeconds = 0.6f;

        public PlankButton PlayButton => playButton;

        public PlankButton QuitButton => quitButton;

        /// <summary>True from the moment Play is pressed.</summary>
        public bool IsLoadingGame { get; private set; }

        private void Awake()
        {
            playButton.Button.onClick.AddListener(Play);
            quitButton.Button.onClick.AddListener(GameExit.Quit);
        }

        private void Start()
        {
            // Coming back from a paused game: time runs again and the pointer is free.
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

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
            playButton.Button.interactable = false;
            quitButton.Button.interactable = false;
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
