using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The pause menu of the game: Escape freezes the game under a dark veil and shows
    /// three wooden planks, Resume, Home and Quit. Escape again resumes.
    /// </summary>
    internal sealed class PauseMenu : MonoBehaviour
    {
        /// <summary>Above the HUD and the inventory.</summary>
        private const int CanvasSortingOrder = 200;

        /// <summary>The darkest tone of the forest's sky, at two thirds strength.</summary>
        private static readonly Color Veil = new Color32(0x1E, 0x26, 0x30, 0xA8);

        private static readonly Vector2 ButtonSlide = new Vector2(0f, 6f);

        private InputAction pauseAction;
        private GameObject canvasObject;
        private string homeScenePath;
        private float timeScaleBeforePause = 1f;

        /// <summary>Raised when the game is paused (true) or resumed (false).</summary>
        public event Action<bool> PausedChanged;

        public bool IsPaused { get; private set; }

        public PlankButton ResumeButton { get; private set; }

        public PlankButton HomeButton { get; private set; }

        public PlankButton QuitButton { get; private set; }

        /// <param name="homeScene">Path of the scene Home loads. It has to be in the build settings.</param>
        public void Initialize(Font font, string homeScene)
        {
            homeScenePath = homeScene;

            var canvas = WoodMenu.CreateCanvas("Pause Menu Canvas", CanvasSortingOrder);
            canvasObject = canvas.gameObject;
            canvasObject.transform.SetParent(transform, false);
            var root = (RectTransform)canvas.transform;

            // The veil catches the pointer: nothing under the pause menu can be clicked.
            WoodMenu.CreateVeil(root, "Veil", Veil).raycastTarget = true;

            var centre = new Vector2(0.5f, 0.5f);
            var step = Vector2.down * (WoodMenu.ButtonSize.y + WoodMenu.ButtonSpacing);
            var first = -step;
            ResumeButton = WoodMenu.CreateButton(root, font, UiText.PauseResume, centre, first, ButtonSlide, 0.74f, Resume);
            HomeButton = WoodMenu.CreateButton(root, font, UiText.PauseHome, centre, first + step, ButtonSlide, 0.82f, GoHome);
            QuitButton = WoodMenu.CreateButton(root, font, UiText.PauseQuit, centre, first + step * 2f, ButtonSlide, 0.68f, GameExit.Quit);

            canvasObject.SetActive(false);
        }

        public void SetPaused(bool paused)
        {
            if (paused == IsPaused || canvasObject == null)
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

            // Listeners run before the menu shows: the game lets go of the pointer first.
            PausedChanged?.Invoke(paused);
            canvasObject.SetActive(paused);

            if (paused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                EventSystem.current.SetSelectedGameObject(ResumeButton.gameObject);
            }
            else
            {
                // Otherwise the hidden plank stays selected, and selecting it again on the
                // next pause would change nothing: it would not stand out.
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private void Awake()
        {
            pauseAction = new InputAction("Pause", InputActionType.Button);
            pauseAction.AddBinding("<Keyboard>/escape");
            pauseAction.AddBinding("<Gamepad>/start");
            pauseAction.performed += OnPausePerformed;
        }

        private void OnEnable()
        {
            pauseAction.Enable();
        }

        private void OnDisable()
        {
            pauseAction.Disable();
        }

        private void OnDestroy()
        {
            pauseAction.performed -= OnPausePerformed;
            pauseAction.Dispose();

            // Leaving the scene while paused must not leave time stopped for the next one.
            if (IsPaused)
            {
                Time.timeScale = timeScaleBeforePause;
            }
        }

        private void OnPausePerformed(InputAction.CallbackContext _)
        {
            SetPaused(!IsPaused);
        }

        private void Resume()
        {
            SetPaused(false);
        }

        private void GoHome()
        {
            Time.timeScale = timeScaleBeforePause;
            SceneManager.LoadScene(homeScenePath, LoadSceneMode.Single);
        }
    }
}
