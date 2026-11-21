using Fungiiiii.Controls;
using UnityEngine;

namespace Fungiiiii.Runtime.Player
{
    /// <summary>
    /// Exposes the player movement input to the rest of the player components, read from
    /// the Player map of the game's actions (<see cref="PlayerControls"/>). While a menu
    /// is open that map is off, so the player stands still.
    /// </summary>
    [RequireComponent(typeof(PlayerControls))]
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        private PlayerControls controls;

        /// <summary>
        /// Current move input on the horizontal plane. Zero while the component is disabled.
        /// </summary>
        public Vector2 MoveInput => enabled ? controls.Player.Move.ReadValue<Vector2>() : Vector2.zero;

        /// <summary>True while the sprint control is held down.</summary>
        public bool IsSprintHeld => enabled && controls.Player.Sprint.IsPressed();

        /// <summary>True while the crouch control is held down.</summary>
        public bool IsCrouchHeld => enabled && controls.Player.Crouch.IsPressed();

        /// <summary>True only on the frame the dodge control was pressed.</summary>
        public bool WasDodgePressedThisFrame => enabled && controls.Player.Dodge.WasPressedThisFrame();

        private void Awake()
        {
            controls = GetComponent<PlayerControls>();
        }
    }
}
