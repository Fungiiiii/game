using Fungiiiii.Survival;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fungiiiii.Prototypes
{
    /// <summary>
    /// Small keyboard-only driver of HUDScene, to show the gauges' three states:
    /// H and S take health and stamina, P and O add and remove poison, R resets.
    /// </summary>
    internal sealed class VitalGaugesPrototypeControls : MonoBehaviour
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
