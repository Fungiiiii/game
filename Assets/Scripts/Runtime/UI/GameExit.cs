using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>Leaves the game from a menu.</summary>
    internal static class GameExit
    {
        public static void Quit()
        {
#if UNITY_EDITOR
            // Application.Quit does nothing in the Editor: Quit stops the play session.
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
