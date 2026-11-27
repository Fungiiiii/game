using System;
using System.Collections.Generic;
using Fungiiiii.Controls;
using Fungiiiii.Inventory;
using Fungiiiii.UI;
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
    public sealed class PlayerHands : MonoBehaviour, IHintSource
    {
        private readonly List<ControlHint> hints = new List<ControlHint>();
        private HeldItemSwitch held;
        private IHeldItemHints watched;
        private IItemSelection selection;
        private PlayerControls controls;
        private Transform handAnchor;

        /// <summary>The item in hand, null with empty hands.</summary>
        public ItemDefinition ItemInHand => Held.CurrentItem;

        /// <summary>What the item in hand does, null when it does nothing.</summary>
        public IHeldItem Behaviour => Held.Current;

        /// <summary>Raised when what the use controls do changes.</summary>
        public event Action HintsChanged;

        /// <summary>What the use controls do with the item in hand, when it says so (<see cref="IHeldItemHints"/>).</summary>
        public IReadOnlyList<ControlHint> Hints => hints;

        private HeldItemSwitch Held => held ??= new HeldItemSwitch(MakeFromPrefab);

        /// <param name="itemSelection">Tells which slot's item is in hand.</param>
        /// <param name="playerControls">Gives the use controls.</param>
        /// <param name="anchor">Where held prefabs are made: the first-person camera.</param>
        public void Initialize(IItemSelection itemSelection, PlayerControls playerControls, Transform anchor)
        {
            selection = itemSelection;
            controls = playerControls;
            controls.MenuOpenChanged += OnMenuOpenChanged;
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
            // Under a menu the item gets no control: it was cancelled when the menu opened.
            if (controls == null || controls.IsMenuOpen)
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

            if (controls != null)
            {
                controls.MenuOpenChanged -= OnMenuOpenChanged;
            }

            if (watched != null)
            {
                watched.HintsChanged -= RefreshHints;
            }
        }

        private void OnMenuOpenChanged(bool open)
        {
            if (open)
            {
                Held.Cancel();
            }
        }

        private void TakeSelectedItem()
        {
            var slot = selection.SelectedSlot;
            Held.Hold(slot.IsEmpty ? null : slot.Item);
            Watch(Held.Current as IHeldItemHints);
        }

        private void Watch(IHeldItemHints next)
        {
            if (next != watched)
            {
                if (watched != null)
                {
                    watched.HintsChanged -= RefreshHints;
                }

                watched = next;
                if (watched != null)
                {
                    watched.HintsChanged += RefreshHints;
                }
            }

            RefreshHints();
        }

        private void RefreshHints()
        {
            hints.Clear();
            if (watched != null && controls != null)
            {
                AddHint(watched.PrimaryHintKey, controls.Player.UsePrimary);
                AddHint(watched.SecondaryHintKey, controls.Player.UseSecondary);
            }

            HintsChanged?.Invoke();
        }

        private void AddHint(string labelKey, UnityEngine.InputSystem.InputAction control)
        {
            if (labelKey != null)
            {
                hints.Add(new ControlHint(labelKey, HintInput.Of(control)));
            }
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
