using Fungiiiii.Runtime.Player;
using Fungiiiii.Survival;
using UnityEngine;

namespace Fungiiiii.Demo
{
    /// <summary>
    /// Connects the stamina rules (<see cref="StaminaRegulator"/>) to the player motor:
    /// sprinting and dodging spend stamina, and exhaustion forbids both.
    /// </summary>
    internal sealed class StaminaDriver : MonoBehaviour
    {
        private PlayerMotor motor;
        private StaminaRegulator stamina;

        public void Initialize(PlayerMotor playerMotor, StaminaRegulator regulator)
        {
            motor = playerMotor;
            stamina = regulator;
            motor.Dodged += HandleDodged;
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

        private void OnDestroy()
        {
            if (motor != null)
            {
                motor.Dodged -= HandleDodged;
            }
        }

        private void HandleDodged()
        {
            stamina.TryDodge();
            ApplyLimits();
        }

        private void ApplyLimits()
        {
            motor.SprintAllowed = stamina.CanSprint;
            motor.DodgeAllowed = stamina.CanDodge;
        }
    }
}
