using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Demo
{
    /// <summary>Something the interact control acts on: a resource to gather, the Champimaison hole.</summary>
    internal interface IInteractable
    {
        /// <summary>Where it is, how close the player must be, and whether it can be used now.</summary>
        InteractionCandidate Candidate { get; }

        /// <summary>Where the key to press is shown above it, in world space.</summary>
        Vector3 PromptPoint { get; }

        /// <summary>What the interact control does with it, a key of the UI string table: « Ramasser ».</summary>
        string HintKey { get; }

        /// <summary>Shows or hides that the player is aiming at it.</summary>
        void SetTargeted(bool targeted);

        /// <summary>Uses it with the player's inventory. Returns false when nothing happened.</summary>
        bool Interact(PlayerInventory inventory);
    }
}
