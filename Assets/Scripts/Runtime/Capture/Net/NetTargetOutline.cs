using System.Collections.Generic;
using Fungiiiii.Capture;
using Fungiiiii.Outlines;
using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// Outlines, while aiming, the mushroom the net would catch, in the colour of its
    /// chance: the same gradient as the impact circle, from red at 0 % to green at 100 %
    /// (playtest of 20/11, feedback #6). Every other mushroom stays without outline.
    /// </summary>
    internal sealed class NetTargetOutline : MonoBehaviour
    {
        private readonly Dictionary<WildMushroom, ItemOutline> outlines = new Dictionary<WildMushroom, ItemOutline>();

        private NetThrower thrower;
        private ItemOutlineFactory factory;
        private WildMushroom shown;

        /// <summary>The mushroom whose outline is on, or null.</summary>
        public WildMushroom Shown => shown;

        public Color Color => factory.ChanceColor;

        /// <summary>Builds the hidden outline of each mushroom that has a model.</summary>
        public void Initialize(NetThrower netThrower, IReadOnlyList<WildMushroom> mushrooms, ItemOutlineFactory outlineFactory)
        {
            thrower = netThrower;
            factory = outlineFactory;

            foreach (var mushroom in mushrooms)
            {
                if (mushroom.Model != null)
                {
                    outlines.Add(mushroom, factory.CreateChanceOutline(mushroom.Model));
                }
            }
        }

        private void LateUpdate()
        {
            if (thrower == null)
            {
                return;
            }

            var target = thrower.PreviewTarget;
            if (target != null && !outlines.ContainsKey(target))
            {
                target = null;
            }

            if (target != shown)
            {
                if (shown != null)
                {
                    outlines[shown].SetVisible(false);
                }

                shown = target;
                if (shown != null)
                {
                    outlines[shown].SetVisible(true);
                }
            }

            if (shown != null)
            {
                factory.SetChanceColor(NetChanceColors.Evaluate(thrower.PreviewChance));
            }
        }
    }
}
