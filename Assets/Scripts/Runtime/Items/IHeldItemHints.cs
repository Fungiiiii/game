using System;

namespace Fungiiiii.Items
{
    /// <summary>
    /// What the use controls do with an item in hand right now, for the key hints: the net
    /// says « Lancer le filet » and « Viser », then « Lancer » and « Relâcher : ne plus
    /// viser » while aiming. An item without it shows no hint. <see cref="PlayerHands"/>
    /// pairs the texts with the controls it reads.
    /// </summary>
    public interface IHeldItemHints
    {
        /// <summary>Raised when what the controls do changes.</summary>
        event Action HintsChanged;

        /// <summary>What the primary control does, a key of the UI string table; null when it does nothing now.</summary>
        string PrimaryHintKey { get; }

        /// <summary>What the secondary control does; null when it does nothing now.</summary>
        string SecondaryHintKey { get; }
    }
}
