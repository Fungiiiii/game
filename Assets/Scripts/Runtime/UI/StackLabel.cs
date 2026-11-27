namespace Fungiiiii.UI
{
    /// <summary>The number shown on a stack of items.</summary>
    internal static class StackLabel
    {
        /// <summary>A single item shows no number, as in most games.</summary>
        public static string For(int quantity)
        {
            return quantity > 1 ? quantity.ToString() : string.Empty;
        }
    }
}
