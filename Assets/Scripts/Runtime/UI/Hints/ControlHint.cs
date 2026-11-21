using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The controls a hint shows: the bindings of actions, read in the scheme in use, so a
    /// remapped key shows as remapped; or fixed controls, for what no action of the game
    /// carries, like the mouse buttons of the inventory, which the event system reads.
    /// </summary>
    public readonly struct HintInput
    {
        private readonly InputAction[] actions;
        private readonly string[] paths;

        private HintInput(InputAction[] actions, string[] paths)
        {
            this.actions = actions;
            this.paths = paths;
        }

        /// <summary>The controls bound to these actions, one per action.</summary>
        public static HintInput Of(params InputAction[] boundActions) => new HintInput(boundActions, null);

        /// <summary>These controls, whatever the bindings.</summary>
        public static HintInput At(params string[] controlPaths) => new HintInput(null, controlPaths);

        /// <summary>Adds to <paramref name="into"/> the controls of this input in <paramref name="bindingGroup"/>.</summary>
        public void CollectPaths(string bindingGroup, List<string> into)
        {
            if (paths != null)
            {
                into.AddRange(paths);
                return;
            }

            if (actions == null)
            {
                return;
            }

            var inGroup = InputBinding.MaskByGroup(bindingGroup);
            foreach (var action in actions)
            {
                foreach (var binding in action.bindings)
                {
                    // Composites (WASD) are never shown in a hint.
                    if (binding.isComposite || binding.isPartOfComposite || !inGroup.Matches(binding))
                    {
                        continue;
                    }

                    into.Add(binding.effectivePath);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// One line of the key hints: the controls to use, drawn for the device in hand, and
    /// what they do, a key of the UI string table.
    /// </summary>
    public readonly struct ControlHint
    {
        public const string GamepadScheme = "Gamepad";

        /// <param name="optional">Left out first when there are too many hints to show.</param>
        public ControlHint(string labelKey, HintInput input, bool optional = false)
            : this(labelKey, input, input, twiceWithMouse: false, optional)
        {
        }

        /// <param name="twiceWithMouse">Pressed twice with the mouse: a double click.</param>
        public ControlHint(string labelKey, HintInput keyboardMouse, HintInput gamepad, bool twiceWithMouse = false, bool optional = false)
        {
            if (string.IsNullOrEmpty(labelKey))
            {
                throw new ArgumentException("A hint says what it does.", nameof(labelKey));
            }

            LabelKey = labelKey;
            KeyboardMouse = keyboardMouse;
            Gamepad = gamepad;
            TwiceWithMouse = twiceWithMouse;
            IsOptional = optional;
        }

        public string LabelKey { get; }

        public HintInput KeyboardMouse { get; }

        public HintInput Gamepad { get; }

        public bool TwiceWithMouse { get; }

        public bool IsOptional { get; }

        /// <summary>How the controls are drawn with <paramref name="scheme"/>, a PlayStation pad or not.</summary>
        public Glyph GlyphFor(InputControlScheme scheme, bool playStation)
        {
            var paths = new List<string>();
            (IsGamepad(scheme) ? Gamepad : KeyboardMouse).CollectPaths(scheme.bindingGroup, paths);
            return InputGlyphs.Describe(paths, playStation);
        }

        /// <summary>Whether the control is pressed twice with <paramref name="scheme"/>.</summary>
        public bool IsTwiceWith(InputControlScheme scheme) => TwiceWithMouse && !IsGamepad(scheme);

        private static bool IsGamepad(InputControlScheme scheme) => scheme.name == GamepadScheme;
    }

    /// <summary>
    /// Something that tells the key hints what the player can do with it now: the item in
    /// hand, the target of the interact control, an open screen. The hints panel shows the
    /// hints of the sources in charge; adding a hint never changes the panel.
    /// </summary>
    public interface IHintSource
    {
        /// <summary>Raised when the hints change, for instance when aiming starts.</summary>
        event Action HintsChanged;

        IReadOnlyList<ControlHint> Hints { get; }
    }

    /// <summary>Hints that never change, such as opening the inventory.</summary>
    public sealed class FixedHints : IHintSource
    {
        private readonly ControlHint[] hints;

        public FixedHints(params ControlHint[] fixedHints)
        {
            hints = fixedHints;
        }

        event Action IHintSource.HintsChanged
        {
            add { }
            remove { }
        }

        public IReadOnlyList<ControlHint> Hints => hints;
    }
}
