using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fungiiiii.Controls
{
    /// <summary>
    /// The game's actions for one local player: which maps are on, and the control scheme
    /// used last.
    ///
    /// In game, the Player map drives the character; while a menu is open, the UI map
    /// takes over and the Player map stops, so a click in a menu never throws the net.
    /// The Screens map, which opens and closes the menus, stays on in both.
    ///
    /// The scheme follows the last device that triggered one of the actions, sticks
    /// included: the key prompts show the keyboard or the gamepad accordingly.
    /// </summary>
    public sealed class ControlMaps : IDisposable
    {
        private readonly GameControls controls = new GameControls();
        private bool isEnabled;
        private bool isMenuOpen;

        public ControlMaps()
        {
            CurrentScheme = controls.KeyboardMouseScheme;
            foreach (var map in controls.asset.actionMaps)
            {
                map.actionTriggered += OnActionTriggered;
            }
        }

        /// <summary>Raised when the control scheme in use changes.</summary>
        public event Action SchemeChanged;

        public GameControls.PlayerActions Player => controls.Player;

        public GameControls.ScreensActions Screens => controls.Screens;

        public GameControls.UIActions UI => controls.UI;

        public InputControlScheme KeyboardMouseScheme => controls.KeyboardMouseScheme;

        public InputControlScheme GamepadScheme => controls.GamepadScheme;

        /// <summary>The scheme of the device used last; keyboard and mouse until another is used.</summary>
        public InputControlScheme CurrentScheme { get; private set; }

        /// <summary>The device used last, or null while none has been used.</summary>
        public InputDevice CurrentDevice { get; private set; }

        public bool IsMenuOpen => isMenuOpen;

        /// <summary>Off, no action responds; on again, the maps are those of the menu state.</summary>
        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            Apply();
        }

        public void SetMenuOpen(bool open)
        {
            isMenuOpen = open;
            Apply();
        }

        public void Dispose()
        {
            foreach (var map in controls.asset.actionMaps)
            {
                map.actionTriggered -= OnActionTriggered;
            }

            // The generated Dispose destroys the asset with Destroy, which Unity refuses outside Play mode.
            if (Application.isPlaying)
            {
                controls.Dispose();
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(controls.asset);
            }
        }

        private void Apply()
        {
            SetMap(controls.Screens.Get(), isEnabled);
            SetMap(controls.Player.Get(), isEnabled && !isMenuOpen);
            SetMap(controls.UI.Get(), isEnabled && isMenuOpen);
        }

        private static void SetMap(InputActionMap map, bool on)
        {
            if (on)
            {
                map.Enable();
            }
            else
            {
                map.Disable();
            }
        }

        private void OnActionTriggered(InputAction.CallbackContext context)
        {
            var device = context.control?.device;
            if (device == null || device == CurrentDevice)
            {
                return;
            }

            CurrentDevice = device;
            foreach (var scheme in controls.controlSchemes)
            {
                // Not FindControlSchemeForDevice: it wants every device of the scheme, and the
                // keyboard alone would never lead back to Keyboard&Mouse.
                if (!scheme.SupportsDevice(device))
                {
                    continue;
                }

                if (scheme.name != CurrentScheme.name)
                {
                    CurrentScheme = scheme;
                    SchemeChanged?.Invoke();
                }

                return;
            }
        }
    }
}
