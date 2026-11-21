using System;

namespace Fungiiiii.Controls
{
    /// <summary>
    /// Whether a menu has the controls. Each system of the game that must stop under a menu
    /// (the view, the item in hand, the interact control) listens to this itself: opening a
    /// menu is one call, and a new system never has to be added to a list somewhere else.
    /// </summary>
    public interface IMenuState
    {
        /// <summary>Raised when a menu opens (true) or the last one closes (false).</summary>
        event Action<bool> MenuOpenChanged;

        bool IsMenuOpen { get; }
    }
}
