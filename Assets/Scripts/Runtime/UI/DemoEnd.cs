using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The end of the demo, inside the Champimaison: "End of the demo, thanks for playing"
    /// and a plank back to the home screen. The mouse is free again.
    /// </summary>
    internal sealed class DemoEnd : MonoBehaviour
    {
        [SerializeField]
        private PlankButton homeButton;

        [Tooltip("Path of the home screen's scene, which must be in the build settings.")]
        [SerializeField]
        private string homeScenePath = "Assets/Scenes/Menu/MainMenu.unity";

        public PlankButton HomeButton => homeButton;

        private void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            homeButton.Button.onClick.AddListener(GoHome);
            homeButton.Button.Select();
        }

        private void OnDestroy()
        {
            if (homeButton != null)
            {
                homeButton.Button.onClick.RemoveListener(GoHome);
            }
        }

        private void GoHome()
        {
            SceneManager.LoadScene(homeScenePath, LoadSceneMode.Single);
        }
    }
}
