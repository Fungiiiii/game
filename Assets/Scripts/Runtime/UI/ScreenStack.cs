using System;
using System.Collections.Generic;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The screens open over the game, the last opened on top. Back, on Escape, closes the
    /// top screen, and opens the pause only when nothing is open, as in Minecraft: the pause
    /// never opens over another screen. A screen opened from another, a confirmation for
    /// instance, closes first and leaves the one under it open.
    ///
    /// Opening a screen is all a new one needs: the inventory is toggled with Tab, a
    /// crafting table would be opened from its interaction.
    /// </summary>
    public sealed class ScreenStack
    {
        private readonly List<IGameScreen> open = new List<IGameScreen>();
        private readonly Dictionary<IGameScreen, Action> closers = new Dictionary<IGameScreen, Action>();
        private readonly IGameScreen backScreen;

        /// <param name="whenNothingIsOpen">The screen Back opens when no screen is open: the pause menu.</param>
        public ScreenStack(IGameScreen whenNothingIsOpen)
        {
            backScreen = whenNothingIsOpen ?? throw new ArgumentNullException(nameof(whenNothingIsOpen));
        }

        /// <summary>Raised whenever a screen opens or closes.</summary>
        public event Action Changed;

        public bool IsAnyOpen => open.Count > 0;

        /// <summary>The screen on top, null when none is open.</summary>
        public IGameScreen Top => IsAnyOpen ? open[open.Count - 1] : null;

        public bool IsOpen(IGameScreen screen) => open.Contains(screen);

        /// <summary>Opens <paramref name="screen"/> on top. Nothing happens if it is already on top.</summary>
        public void Open(IGameScreen screen)
        {
            if (screen == null)
            {
                throw new ArgumentNullException(nameof(screen));
            }

            if (Top == screen)
            {
                return;
            }

            if (!open.Remove(screen))
            {
                Action closer = () => Close(screen);
                closers.Add(screen, closer);
                screen.CloseRequested += closer;
            }

            open.Add(screen);
            screen.Open();
            Changed?.Invoke();
        }

        /// <summary>Closes <paramref name="screen"/>, wherever it stands. Nothing happens if it is not open.</summary>
        public void Close(IGameScreen screen)
        {
            if (!open.Remove(screen))
            {
                return;
            }

            screen.CloseRequested -= closers[screen];
            closers.Remove(screen);
            screen.Close();
            Changed?.Invoke();
        }

        /// <summary>Opens <paramref name="screen"/> when nothing is open, closes it when it is on top, else does nothing.</summary>
        public void Toggle(IGameScreen screen)
        {
            if (Top == screen)
            {
                Close(screen);
            }
            else if (!IsAnyOpen)
            {
                Open(screen);
            }
        }

        /// <summary>Closes every screen, the top first, for a reset.</summary>
        public void CloseAll()
        {
            while (IsAnyOpen)
            {
                Close(Top);
            }
        }

        /// <summary>Escape: closes the screen on top, or opens the pause when none is open.</summary>
        public void Back()
        {
            if (IsAnyOpen)
            {
                Close(Top);
            }
            else
            {
                Open(backScreen);
            }
        }
    }
}
