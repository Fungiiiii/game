namespace Fungiiiii.Demo
{
    /// <summary>
    /// Something aimed at that shows a short text over it instead of the key to press,
    /// because the player cannot use it themselves: the full water fern reads « Il faut
    /// un Spongeus ».
    /// </summary>
    internal interface ILabelledInteractable
    {
        /// <summary>The text, a key of the UI string table; null to show the key as usual.</summary>
        string LabelKey { get; }
    }
}
