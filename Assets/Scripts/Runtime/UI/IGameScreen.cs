using System;

namespace Fungiiiii.UI
{
    /// <summary>
    /// A screen that opens over the game and takes the controls while it is open: the
    /// inventory and the pause menu today, a crafting table or the Champimaison's upgrades
    /// tomorrow. <see cref="ScreenStack"/> opens and closes it; it never does so itself.
    /// </summary>
    public interface IGameScreen
    {
        /// <summary>Raised when the screen wants to be closed, by a button of its own for instance.</summary>
        event Action CloseRequested;

        /// <summary>Shows the screen.</summary>
        void Open();

        /// <summary>Hides the screen.</summary>
        void Close();
    }
}
