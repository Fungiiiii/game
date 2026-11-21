using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Companions
{
    /// <summary>
    /// Something a companion can soak up, like the drops of a water fern. A companion only
    /// goes for what yields the item it carries.
    /// </summary>
    public interface IAbsorbable
    {
        /// <summary>Where it stands, on the ground.</summary>
        Vector3 Position { get; }

        /// <summary>What it gives: water for the fern.</summary>
        ItemDefinition Yield { get; }

        /// <summary>Whether there is something to soak up now.</summary>
        bool CanBeAbsorbed { get; }

        /// <summary>Takes what it holds. Returns false when there was nothing left.</summary>
        bool Absorb();
    }
}
