using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// Colour of the impact circle for a capture chance, so the player reads at a glance
    /// what is in front of them. Pure red means impossible (0 %). Any chance above zero
    /// starts at orange, low but possible, and runs through yellow at 50 % to the
    /// greenest green at 100 %.
    ///
    /// The chance itself is not computed here: it is whatever the capture rules give for
    /// this mushroom and this tool.
    /// </summary>
    public static class NetChanceColors
    {
        public static readonly Color Impossible = new Color(0.9f, 0.08f, 0.06f);
        public static readonly Color Low = new Color(1f, 0.5f, 0.05f);
        public static readonly Color Even = new Color(1f, 0.9f, 0.1f);
        public static readonly Color Certain = new Color(0.05f, 0.85f, 0.15f);

        private const float EvenChance = 0.5f;

        public static Color Evaluate(float chance)
        {
            // NaN falls here too: an unknown chance is shown as impossible.
            if (!(chance > 0f))
            {
                return Impossible;
            }

            chance = Mathf.Clamp01(chance);

            return chance < EvenChance
                ? Color.Lerp(Low, Even, chance / EvenChance)
                : Color.Lerp(Even, Certain, (chance - EvenChance) / (1f - EvenChance));
        }
    }
}
