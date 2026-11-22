using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The end of the demo, inside the Champimaison: "End of the demo, thanks for playing"
    /// and two planks, back to the home screen or out of the game. The mouse is free again.
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

        public PlankButton QuitButton => quitButton;

        private void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            homeButton.Button.onClick.AddListener(GoHome);
            quitButton.Button.onClick.AddListener(GameExit.Quit);
            homeButton.Button.Select();
        }

        private void OnDestroy()
        {
            if (homeButton != null)
            {
                homeButton.Button.onClick.RemoveListener(GoHome);
            }

            if (quitButton != null)
            {
                quitButton.Button.onClick.RemoveListener(GameExit.Quit);
            }
        }

        private void GoHome()
        {
            SceneManager.LoadScene(homeScenePath, LoadSceneMode.Single);
        }
    }
}
