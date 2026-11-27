using Fungiiiii.Interaction;
using Fungiiiii.Inventory;
using Fungiiiii.Outlines;
using Fungiiiii.UI;
using UnityEngine;

namespace Fungiiiii.Champimaison
{
    /// <summary>
    /// A small hole of earth where the Champimaison can be planted. Outlined while no seed
    /// is planted; aimed at, it says what is missing or shows the key to plant, as
    /// <see cref="PlantingRules"/> decides. <see cref="MoleHoles"/> does the planting.
    /// </summary>
    internal sealed class MoleHole : MonoBehaviour, IInteractable, ILabelledInteractable
    {
        private const float PromptHeight = 0.6f;

        private MoleHoles owner;
        private ItemOutline outline;
        private float reach;

        public PlantingPrompt Prompt => owner.PromptFor();

        public InteractionCandidate Candidate =>
            new InteractionCandidate(transform.position, reach, Prompt != PlantingPrompt.None);

        public Vector3 PromptPoint => transform.position + Vector3.up * PromptHeight;

        public string HintKey => Prompt == PlantingPrompt.Plant ? UiText.HintPlant : null;

        public string LabelKey => Prompt == PlantingPrompt.NeedsSeed ? UiText.LabelNeedsSeed
            : Prompt == PlantingPrompt.HoldSeed ? UiText.LabelHoldSeed
            : null;

        public bool IsOutlined => outline.IsVisible;

        public void Initialize(MoleHoles holes, ItemOutline itemOutline, float interactionReach)
        {
            owner = holes;
            outline = itemOutline;
            reach = interactionReach;
        }

        public void SetTargeted(bool targeted)
        {
            outline.SetTargeted(targeted);
        }

        public bool Interact(PlayerInventory inventory)
        {
            return owner.TryPlant(this, inventory);
        }

        /// <summary>The seed is planted: the hole stays in the decor, without outline.</summary>
        public void Retire()
        {
            outline.SetTargeted(false);
            outline.SetVisible(false);
        }
    }
}
