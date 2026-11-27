using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Interaction
{
    /// <summary>
    /// Something the interact control acts on: a resource to gather, the Champimaison hole.
    /// What it can also do when the control is held (<see cref="IHoldInteractable"/>) or a
    /// text shown instead of the key (<see cref="ILabelledInteractable"/>) are interfaces of
    /// their own, implemented only by what needs them.
    /// </summary>
    internal interface IInteractable
    {
        /// <summary>Where it is, how close the player must be, and whether it can be aimed at now.</summary>
        InteractionCandidate Candidate { get; }

        /// <summary>Where the key to press is shown above it, in world space.</summary>
        Vector3 PromptPoint { get; }

        /// <summary>What a press of the interact control does with it, a key of the UI string table: <c>hint.gather</c>. Null when a press does nothing now.</summary>
        string HintKey { get; }

        /// <summary>Shows or hides that the player is aiming at it.</summary>
        void SetTargeted(bool targeted);

        /// <summary>Uses it with the player's inventory. Returns false when nothing happened.</summary>
        bool Interact(PlayerInventory inventory);
    }
}
