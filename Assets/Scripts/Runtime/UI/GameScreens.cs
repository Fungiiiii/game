using System.Linq;
using Fungiiiii.Controls;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The screens over the game and the key hints. Escape, or B, closes the screen on top,
    /// or pauses when none is open; Tab opens and closes the inventory. While a screen is
    /// open the menus have the controls, and the inventory, which shows its own quick bar,
    /// hides the game's one. The hints are those of the screen on top, or of the game.
    /// </summary>
    internal sealed class GameScreens : MonoBehaviour
    {
        [SerializeField]
        private Hotbar hotbar;

        [SerializeField]
        private InventoryScreen inventory;

        [SerializeField]
        private PauseMenu pauseMenu;

        [SerializeField]
        private ControlHintsPanel hints;

        private PlayerControls controls;
        private ScreenStack stack;
        private IHintSource[] gameHints;
        private InputAction menuAction;
        private InputAction cancelAction;
        private InputAction inventoryAction;

        /// <summary>The screens open over the game, null until bound.</summary>
        public ScreenStack Stack => stack;

        public InventoryScreen Inventory => inventory;

        public PauseMenu Pause => pauseMenu;

        public ControlHintsPanel Hints => hints;

        public bool IsInventoryOpen => inventory.gameObject.activeSelf;

        /// <summary>What is missing, or null when nothing is.</summary>
        public string FindProblem()
        {
            return hotbar == null || inventory == null || pauseMenu == null || hints == null
                ? "a piece of the interface is missing: quick bar, inventory, pause menu or key hints."
                : null;
        }

        /// <summary>
        /// Gives the screens the player's controls. In game the hints are those of
        /// <paramref name="inGame"/>, in order, then switching items and the inventory;
        /// switching is left out first when there are too many.
        /// </summary>
        public void Bind(PlayerControls playerControls, params IHintSource[] inGame)
        {
            controls = playerControls;
            inventory.SetControls(new InventoryScreenControls(
                controls.UI.Navigate,
                controls.UI.TakeHalf,
                controls.UI.GatherAll,
                controls.UI.Drop,
                controls.Screens.Inventory,
                controls.UI.Cancel));

            var actions = controls.Player;
            gameHints = inGame.Append(new FixedHints(
                new ControlHint(UiText.HintSwitchItem, HintInput.Of(actions.HotbarNext, actions.HotbarPrevious), optional: true),
                new ControlHint(UiText.HintInventory, HintInput.Of(controls.Screens.Inventory)))).ToArray();
            hints.Initialize(controls);

            stack = new ScreenStack(pauseMenu);
            stack.Changed += Apply;
            menuAction = controls.Screens.Menu;
            menuAction.performed += OnBack;
            // B on a gamepad goes back too; it only acts in a screen, where the UI map is on.
            cancelAction = controls.UI.Cancel;
            cancelAction.performed += OnBack;
            inventoryAction = controls.Screens.Inventory;
            inventoryAction.performed += OnInventory;
            Apply();
        }

        /// <summary>Opens or closes the inventory, as Tab does.</summary>
        public void SetInventoryOpen(bool open)
        {
            if (open)
            {
                stack.Open(inventory);
            }
            else
            {
                stack.Close(inventory);
            }
        }

        private void OnDestroy()
        {
            if (stack == null)
            {
                return;
            }

            stack.Changed -= Apply;
            menuAction.performed -= OnBack;
            cancelAction.performed -= OnBack;
            inventoryAction.performed -= OnInventory;
        }

        private void OnBack(InputAction.CallbackContext _)
        {
            stack.Back();
        }

        /// <summary>Tab: opens the inventory when no screen is open, closes it when it is on top.</summary>
        private void OnInventory(InputAction.CallbackContext _)
        {
            stack.Toggle(inventory);
        }

        private void Apply()
        {
            hotbar.SetVisible(!stack.IsOpen(inventory));
            // Every system that stops under a menu listens to the controls itself.
            controls.SetMenuOpen(stack.IsAnyOpen);
            ShowHints();
        }

        /// <summary>The hints of the screen on top, or of the game when none is open; the pause shows none.</summary>
        private void ShowHints()
        {
            if (!stack.IsAnyOpen)
            {
                hints.ShowFrom(gameHints);
            }
            else if (stack.Top is IHintSource screenHints)
            {
                hints.ShowFrom(screenHints);
            }
            else
            {
                hints.ShowFrom();
            }
        }
    }
}
