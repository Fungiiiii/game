using Fungiiiii.Controls;
using Fungiiiii.Inventory;
using UnityEngine;

namespace Fungiiiii.Items
{
    /// <summary>
    /// The player's hands: they hold the item of the selected quick bar slot and give it the
    /// use controls, the primary on a press and the secondary every frame. They are the only
    /// reader of those controls, so two items never answer the same click.
    ///
    /// An item does something in hand when its scene registers a behaviour for it
    /// (<see cref="Register"/>), or when its definition has a held prefab, made under the
    /// hand anchor the first time it is held.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerHands : MonoBehaviour
    {
        private HeldItemSwitch held;
        private IItemSelection selection;
        private PlayerControls controls;
        private Transform handAnchor;
        private bool inputEnabled = true;

        /// <summary>The item in hand, null with empty hands.</summary>
        public ItemDefinition ItemInHand => Held.CurrentItem;

        /// <summary>What the item in hand does, null when it does nothing.</summary>
        public IHeldItem Behaviour => Held.Current;

        /// <summary>False, the item in hand gets no control and stops what it was doing, for a menu.</summary>
        public bool InputEnabled
        {
            get => inputEnabled;
            set
            {
                inputEnabled = value;
                if (!value)
                {
                    Held.Cancel();
                }
            }
        }

        private HeldItemSwitch Held => held ??= new HeldItemSwitch(MakeFromPrefab);

        /// <param name="itemSelection">Tells which slot's item is in hand.</param>
        /// <param name="playerControls">Gives the use controls.</param>
        /// <param name="anchor">Where held prefabs are made: the first-person camera.</param>
        public void Initialize(IItemSelection itemSelection, PlayerControls playerControls, Transform anchor)
        {
            selection = itemSelection;
            controls = playerControls;
            handAnchor = anchor;
            selection.SelectedSlotChanged += TakeSelectedItem;
            TakeSelectedItem();
        }

        /// <summary>What <paramref name="item"/> does in hand, for a tool its scene builds. Register before <see cref="Initialize"/>.</summary>
        public void Register(ItemDefinition item, IHeldItem behaviour)
        {
            Held.Register(item, behaviour);
        }

        private void Update()
        {
            if (!inputEnabled || controls == null)
            {
                return;
            }

            var player = controls.Player;
            if (player.UsePrimary.WasPressedThisFrame())
            {
                Held.UsePrimary();
            }

            Held.HoldSecondary(player.UseSecondary.IsPressed());
        }

        private void OnDestroy()
        {
            if (selection != null)
            {
                selection.SelectedSlotChanged -= TakeSelectedItem;
            }
        }

        private void TakeSelectedItem()
        {
            var slot = selection.SelectedSlot;
            Held.Hold(slot.IsEmpty ? null : slot.Item);
        }

        private IHeldItem MakeFromPrefab(ItemDefinition item)
        {
            var instance = Instantiate(item.HeldPrefab, handAnchor, false);
            instance.name = item.HeldPrefab.name;
            if (instance.TryGetComponent<IHeldItem>(out var behaviour))
            {
                return behaviour;
            }

            Debug.LogError($"The held prefab of '{item.Id}' has no component that implements IHeldItem.", item);
            Destroy(instance);
            return null;
        }
    }
}
