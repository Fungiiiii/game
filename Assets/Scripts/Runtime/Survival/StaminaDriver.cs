using Fungiiiii.Runtime.Player;
using Fungiiiii.Survival;
using UnityEngine;

namespace Fungiiiii.Survival
{
    /// <summary>
    /// Connects the stamina rules (<see cref="StaminaRegulator"/>) to the player motor:
    /// sprinting spends stamina, and exhaustion forbids it.
    /// </summary>
    internal sealed class StaminaDriver : MonoBehaviour
    {
        private PlayerMotor motor;
        private StaminaRegulator stamina;

        public void Initialize(PlayerMotor playerMotor, StaminaRegulator regulator)
        {
            motor = playerMotor;
            stamina = regulator;
            ApplyLimits();
        }

        private void Update()
        {
            if (motor == null)
            {
                return;
            }

            stamina.Tick(Time.deltaTime, motor.IsSprintMoving);
            ApplyLimits();
        }

        private void ApplyLimits()
        {
            motor.SprintAllowed = stamina.CanSprint;
        }
    }
}
