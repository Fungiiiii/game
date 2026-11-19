using Fungiiiii.Gathering;
using Fungiiiii.Inventory;

namespace Fungiiiii.Demo
{
    /// <summary>Something the interact control acts on: a resource to gather, the Champimaison hole.</summary>
    internal interface IDemoInteractable
    {
        /// <summary>Where it is, how close the player must be, and whether it can be used now.</summary>
        InteractionCandidate Candidate { get; }

        /// <summary>Shows or hides that the player is aiming at it.</summary>
        void SetTargeted(bool targeted);

        /// <summary>Uses it with the player's inventory. Returns false when nothing happened.</summary>
        bool Interact(PlayerInventory inventory);
    }
}
