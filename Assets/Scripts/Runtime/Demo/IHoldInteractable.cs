using Fungiiiii.Inventory;

namespace Fungiiiii.Demo
{
    /// <summary>
    /// Something the interact control also acts on when held, besides a short press: the
    /// Spongeus is squeezed with a short press and taken back when E is held. On such a
    /// target the short press is decided on the release; on any other, on the press.
    /// </summary>
    internal interface IHoldInteractable
    {
        /// <summary>What holding the control does, a key of the UI string table: « Maintenir : reprendre ».</summary>
        string HoldHintKey { get; }

        /// <summary>Holding the control did its work. Returns false when nothing happened.</summary>
        bool InteractHold(PlayerInventory inventory);
    }
}
