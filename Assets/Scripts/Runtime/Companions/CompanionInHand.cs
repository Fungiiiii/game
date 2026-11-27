using System;
using Fungiiiii.Companions;
using Fungiiiii.Items;
using Fungiiiii.UI;

namespace Fungiiiii.Companions
{
    /// <summary>
    /// What a companion's item does in hand: the primary control (left click, right
    /// trigger) takes it out to follow the player. Nothing is drawn in hand. While a
    /// companion is out, the control does nothing and its hint goes.
    /// </summary>
    internal sealed class CompanionInHand : IHeldItem, IHeldItemHints
    {
        private readonly CompanionKeeper keeper;
        private readonly CompanionDefinition companion;
        private bool equipped;

        public CompanionInHand(CompanionKeeper companionKeeper, CompanionDefinition definition)
        {
            keeper = companionKeeper != null ? companionKeeper : throw new ArgumentNullException(nameof(companionKeeper));
            companion = definition != null ? definition : throw new ArgumentNullException(nameof(definition));
        }

        public event Action HintsChanged;

        public string PrimaryHintKey => keeper.Active == null ? UiText.HintRelease : null;

        public string SecondaryHintKey => null;

        public void Equip()
        {
            if (!equipped)
            {
                equipped = true;
                keeper.ActiveChanged += OnActiveChanged;
            }
        }

        public void Unequip()
        {
            if (equipped)
            {
                equipped = false;
                keeper.ActiveChanged -= OnActiveChanged;
            }
        }

        public void UsePrimary()
        {
            keeper.Release(companion);
        }

        public void HoldSecondary(bool held)
        {
        }

        public void Cancel()
        {
        }

        private void OnActiveChanged()
        {
            HintsChanged?.Invoke();
        }
    }
}
