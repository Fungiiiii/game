using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The pause menu of the game: it freezes the game under a dark veil and shows three
    /// wooden planks, Resume, Home and Quit. It is the screen Escape opens when no other
    /// screen is open (<see cref="ScreenStack"/>); Escape again, or Resume, closes it.
    /// </summary>
    internal sealed class PauseMenu : MonoBehaviour, IGameScreen
    {
        [Tooltip("The veil and the planks, hidden while the game runs.")]
        [SerializeField]
        private GameObject content;

        [SerializeField]
        private PlankButton resumeButton;

        [SerializeField]
        private PlankButton homeButton;

        [SerializeField]
        private PlankButton quitButton;

        [Tooltip("Path of the scene Home loads. It has to be in the build settings.")]
        [SerializeField]
        private string homeScenePath = "Assets/Scenes/Menu/MainMenu.unity";

        private float timeScaleBeforePause = 1f;

        /// <summary>Raised by Resume: the screen stack closes the menu.</summary>
        public event Action CloseRequested;

        public bool IsPaused { get; private set; }

        public PlankButton ResumeButton => resumeButton;

        public PlankButton HomeButton => homeButton;

        public PlankButton QuitButton => quitButton;

        private void Awake()
        {
            resumeButton.Button.onClick.AddListener(Resume);
            homeButton.Button.onClick.AddListener(GoHome);
            quitButton.Button.onClick.AddListener(GameExit.Quit);
            content.SetActive(false);
        }

        /// <summary>Freezes the game and shows the menu.</summary>
        public void Open()
        {
            SetPaused(true);
        }

        /// <summary>Hides the menu and lets time run again.</summary>
        public void Close()
        {
            SetPaused(false);
        }

        private void SetPaused(bool paused)
        {
            if (paused == IsPaused)
            {
                return;
            }

            IsPaused = paused;

            if (paused)
            {
                timeScaleBeforePause = Time.timeScale;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = timeScaleBeforePause;
            }

            content.SetActive(paused);

            if (paused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
            }
            else
            {
                // Otherwise the hidden plank stays selected, and selecting it again on the
                // next pause would change nothing: it would not stand out.
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private void OnDestroy()
        {
            // Leaving the scene while paused must not leave time stopped for the next one.
            if (IsPaused)
            {
                Time.timeScale = timeScaleBeforePause;
            }
        }

        private void Resume()
        {
            CloseRequested?.Invoke();
        }

        private void GoHome()
        {
            Time.timeScale = timeScaleBeforePause;
            SceneManager.LoadScene(homeScenePath, LoadSceneMode.Single);
        }
    }
}
