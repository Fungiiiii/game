using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fungiiiii.Controls
{
    /// <summary>
    /// The controls of the player on this machine, carried by the player: every system that
    /// reads the player's input asks this component for its action instead of building one,
    /// so that a single asset holds the bindings (ADR-0003) and opening a menu stops them all.
    ///
    /// Switched off on the copies of the other players, like the input reader: no action
    /// responds there.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerControls : MonoBehaviour
    {
        private ControlMaps maps;

        /// <summary>Raised when the player switches between keyboard and mouse and a gamepad.</summary>
        public event Action SchemeChanged;

        public GameControls.PlayerActions Player => Maps.Player;

        public GameControls.ScreensActions Screens => Maps.Screens;

        public GameControls.UIActions UI => Maps.UI;

        public InputControlScheme CurrentScheme => Maps.CurrentScheme;

        /// <summary>The device used last, or null while none has been used.</summary>
        public InputDevice CurrentDevice => Maps.CurrentDevice;

        public bool IsMenuOpen => Maps.IsMenuOpen;

        // Built on first use: other components of the player may ask for an action in their own Awake.
        private ControlMaps Maps
        {
            get
            {
                if (maps == null)
                {
                    maps = new ControlMaps();
                    maps.SchemeChanged += OnSchemeChanged;
                }

                return maps;
            }
        }

        /// <summary>A menu takes the controls: the Player map stops and the UI map takes over.</summary>
        public void SetMenuOpen(bool open)
        {
            Maps.SetMenuOpen(open);
        }

        private void OnEnable()
        {
            Maps.SetEnabled(true);
        }

        private void OnDisable()
        {
            maps?.SetEnabled(false);
        }

        private void OnDestroy()
        {
            if (maps == null)
            {
                return;
            }

            // Kept, disposed: whoever still unsubscribes while the scene unloads must not build new actions.
            maps.SchemeChanged -= OnSchemeChanged;
            maps.Dispose();
        }

        private void OnSchemeChanged()
        {
            SchemeChanged?.Invoke();
        }
    }
}
