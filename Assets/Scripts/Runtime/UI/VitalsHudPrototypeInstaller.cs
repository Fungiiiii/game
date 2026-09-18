using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Creates the HUD prototype in the existing sample scene without changing the scene asset.
    /// </summary>
    internal static class VitalsHudPrototypeInstaller
    {
        private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallInSampleScene()
        {
            if (Application.isBatchMode || SceneManager.GetActiveScene().path != SampleScenePath)
            {
                return;
            }

            if (Object.FindFirstObjectByType<VitalsHud>() != null)
            {
                return;
            }

            var root = new GameObject("VitalsHudPrototype");
            var model = new PlayerVitals(100f, 100f);
            var hud = root.AddComponent<VitalsHud>();
            var demo = root.AddComponent<VitalsHudPrototypeControls>();

            hud.Initialize(model);
            demo.Initialize(model);
        }
    }

    /// <summary>
    /// Small keyboard-only driver for demonstrating the three HUD states.
    /// </summary>
    internal sealed class VitalsHudPrototypeControls : MonoBehaviour
    {
        private PlayerVitals _model;

        public void Initialize(PlayerVitals model)
        {
            _model = model;
        }

        private void Update()
        {
            if (_model == null || Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                _model.SetHealth(_model.Current.Health - 20f);
            }

            if (Keyboard.current.sKey.wasPressedThisFrame)
            {
                _model.SetStamina(_model.Current.Stamina - 20f);
            }

            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                _model.SetPoisoned(true);
            }

            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                _model.SetPoisoned(false);
            }

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                _model.SetHealth(_model.MaxHealth);
                _model.SetStamina(_model.MaxStamina);
                _model.SetPoisoned(false);
            }
        }
    }
}
