using Fungiiiii.Controls;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The end of the demo, inside the Champimaison: "End of the demo, thanks for playing"
    /// and two planks, back to the home screen or out of the game. The mouse is free again.
    /// Escape, or Start or B on a gamepad, goes back to the home screen as on the other
    /// screens; only the plank quits, so that the game is not closed by mistake.
    /// </summary>
    internal sealed class DemoEnd : MonoBehaviour
    {
        [SerializeField]
        private PlankButton homeButton;

        [SerializeField]
        private PlankButton quitButton;

        [Tooltip("Path of the home screen's scene, which must be in the build settings.")]
        [SerializeField]
        private string homeScenePath = "Assets/Scenes/Menu/MainMenu.unity";

        public PlankButton HomeButton => homeButton;

        private GameControls controls;
        private bool leaving;

        public PlankButton QuitButton => quitButton;

        private void Start()
        {
            controls = new GameControls();
            controls.Screens.Menu.performed += OnBack;
            controls.UI.Cancel.performed += OnBack;
            controls.Screens.Enable();
            controls.UI.Enable();
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            homeButton.Button.onClick.AddListener(GoHome);
            quitButton.Button.onClick.AddListener(GameExit.Quit);
            homeButton.Button.Select();
        }

        private void OnDestroy()
        {
            if (controls != null)
            {
                controls.Screens.Menu.performed -= OnBack;
                controls.UI.Cancel.performed -= OnBack;
                // Disposed while on, its actions would stay in the input system.
                controls.Disable();
                controls.Dispose();
            }

            if (homeButton != null)
            {
                homeButton.Button.onClick.RemoveListener(GoHome);
            }

            if (quitButton != null)
            {
                quitButton.Button.onClick.RemoveListener(GameExit.Quit);
            }
        }

        private void OnBack(InputAction.CallbackContext _)
        {
            GoHome();
        }

        private void GoHome()
        {
            // A click and a key in the same frame load the home screen once.
            if (leaving)
            {
                return;
            }

            leaving = true;
            SceneManager.LoadScene(homeScenePath, LoadSceneMode.Single);
        }
    }
}
