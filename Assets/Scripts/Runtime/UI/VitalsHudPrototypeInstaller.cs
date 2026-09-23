using Fungiiiii.Survival;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Creates the HUD prototype only in the dedicated prototype scene.
    /// </summary>
    internal static class VitalsHudPrototypeInstaller
    {
        private const string PrototypeScenePath = "Assets/Scenes/Prototype/HUDScene.unity";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneLoadHandler()
        {
            SceneManager.sceneLoaded -= InstallInPrototypeScene;
            SceneManager.sceneLoaded += InstallInPrototypeScene;
        }

        private static void InstallInPrototypeScene(Scene scene, LoadSceneMode mode)
        {
            if (scene.path != PrototypeScenePath)
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
        private const float PoisonStepPercentage = 10f;

        private PlayerVitals _model;

        public void Initialize(PlayerVitals model)
        {
            _model = model;
        }

        private void Update()
        {
            if (_model == null)
            {
                return;
            }

            _model.Tick(Time.deltaTime);

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.hKey.wasPressedThisFrame)
            {
                _model.SetHealth(_model.Current.Health - 20f);
            }

            if (keyboard.sKey.wasPressedThisFrame)
            {
                _model.SetStamina(_model.Current.Stamina - 20f);
            }

            if (keyboard.pKey.wasPressedThisFrame)
            {
                _model.Poison.Apply(PoisonStepPercentage);
            }

            if (keyboard.oKey.wasPressedThisFrame)
            {
                _model.Poison.Remove(PoisonStepPercentage);
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                _model.Reset();
            }
        }
    }
}
