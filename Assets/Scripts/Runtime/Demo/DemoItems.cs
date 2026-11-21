using Fungiiiii.Capture;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Demo
{
    /// <summary>
    /// The items of the demo loop. Ids are gameplay keys, not player-facing names: the
    /// inventory shows a colour square and a count, no text. Colours are placeholders
    /// taken from the project palette.
    /// </summary>
    internal static class DemoItems
    {
        public const string WoodId = "wood";
        public const string SporeId = "spore";
        public const string NetId = "net";

        private const int ResourceStackSize = 16;
        private const int MushroomStackSize = 4;

        /// <summary>Palette "Wood", #8A5F42.</summary>
        public static readonly InventoryItemDefinition Wood =
            new InventoryItemDefinition(WoodId, new Color32(0x8A, 0x5F, 0x42, 0xFF), ResourceStackSize);

        /// <summary>Palette "Mushroom violet", #8E6CB8.</summary>
        public static readonly InventoryItemDefinition Spore =
            new InventoryItemDefinition(SporeId, new Color32(0x8E, 0x6C, 0xB8, 0xFF), ResourceStackSize);

        /// <summary>Palette "Sponge", #9CC3DA.</summary>
        public static readonly InventoryItemDefinition Spongeus =
            new InventoryItemDefinition("spongeus", new Color32(0x9C, 0xC3, 0xDA, 0xFF), MushroomStackSize);

        /// <summary>The capture net: one, the colour of its placeholder.</summary>
        public static readonly InventoryItemDefinition Net =
            new InventoryItemDefinition(NetId, NetThrower.NetColor, 1);

        /// <summary>Palette "Bulb on", #FFE9A3.</summary>
        public static readonly InventoryItemDefinition Champoule =
            new InventoryItemDefinition("champoule", new Color32(0xFF, 0xE9, 0xA3, 0xFF), MushroomStackSize);
    }
}
