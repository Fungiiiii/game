using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Fungiiiii.UI
{
    public enum GlyphKind
    {
        /// <summary>A keyboard key: its name in a wooden key.</summary>
        Key,
        MouseLeft,
        MouseRight,
        MouseWheel,

        /// <summary>A face button of a pad: a letter, or a PlayStation shape.</summary>
        Face,

        /// <summary>A trigger, a shoulder or a small pad button: its name in a pill.</summary>
        Pill,

        /// <summary>The View button of an Xbox pad.</summary>
        View,
    }

    public enum FaceShape
    {
        Letter,
        Cross,
        Circle,
        Square,
        Triangle,
    }

    /// <summary>How one control is drawn in a key hint.</summary>
    public readonly struct Glyph : IEquatable<Glyph>
    {
        public Glyph(GlyphKind kind, string text = null, FaceShape face = FaceShape.Letter)
        {
            Kind = kind;
            Text = text;
            Face = face;
        }

        public GlyphKind Kind { get; }

        /// <summary>What is written on it, null when it is drawn.</summary>
        public string Text { get; }

        public FaceShape Face { get; }

        public bool Equals(Glyph other) => Kind == other.Kind && Text == other.Text && Face == other.Face;

        public override bool Equals(object obj) => obj is Glyph other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Kind, Text, Face);

        public override string ToString() => $"{Kind} {Text} {Face}";
    }

    /// <summary>
    /// Turns the controls of an action into what a key hint draws: a key, a mouse, a pad
    /// button named as on the pad in hand, Xbox or PlayStation.
    /// </summary>
    public static class InputGlyphs
    {
        private const InputControlPath.HumanReadableStringOptions KeyNameOptions =
            InputControlPath.HumanReadableStringOptions.OmitDevice | InputControlPath.HumanReadableStringOptions.UseShortNames;

        /// <param name="controlPaths">The paths of the controls, one per action, as in the bindings.</param>
        /// <param name="playStation">Whether the pad in hand is a PlayStation pad.</param>
        public static Glyph Describe(IReadOnlyList<string> controlPaths, bool playStation)
        {
            if (controlPaths == null || controlPaths.Count == 0)
            {
                return new Glyph(GlyphKind.Key, "?");
            }

            // The wheel up and down, or both shoulders, are one control to the player.
            if (AllAre(controlPaths, "Mouse", "scroll"))
            {
                return new Glyph(GlyphKind.MouseWheel);
            }

            if (controlPaths.Count == 2 && ControlOf(controlPaths[0]).EndsWith("Shoulder") && ControlOf(controlPaths[1]).EndsWith("Shoulder"))
            {
                return new Glyph(GlyphKind.Pill, playStation ? "L1 · R1" : "LB · RB");
            }

            return DescribeOne(controlPaths[0], playStation);
        }

        private static Glyph DescribeOne(string path, bool playStation)
        {
            var control = ControlOf(path);
            if (DeviceOf(path) == "Mouse")
            {
                switch (control)
                {
                    case "leftButton":
                        return new Glyph(GlyphKind.MouseLeft);
                    case "rightButton":
                        return new Glyph(GlyphKind.MouseRight);
                }

                if (control.StartsWith("scroll"))
                {
                    return new Glyph(GlyphKind.MouseWheel);
                }
            }

            if (DeviceOf(path) == "Gamepad")
            {
                switch (control)
                {
                    case "buttonSouth":
                        return Face("A", FaceShape.Cross, playStation);
                    case "buttonEast":
                        return Face("B", FaceShape.Circle, playStation);
                    case "buttonWest":
                        return Face("X", FaceShape.Square, playStation);
                    case "buttonNorth":
                        return Face("Y", FaceShape.Triangle, playStation);
                    case "rightTrigger":
                        return new Glyph(GlyphKind.Pill, playStation ? "R2" : "RT");
                    case "leftTrigger":
                        return new Glyph(GlyphKind.Pill, playStation ? "L2" : "LT");
                    case "rightShoulder":
                        return new Glyph(GlyphKind.Pill, playStation ? "R1" : "RB");
                    case "leftShoulder":
                        return new Glyph(GlyphKind.Pill, playStation ? "L1" : "LB");
                    case "leftStickPress":
                        return new Glyph(GlyphKind.Pill, playStation ? "L3" : "LS");
                    case "rightStickPress":
                        return new Glyph(GlyphKind.Pill, playStation ? "R3" : "RS");
                    case "select":
                        return playStation ? new Glyph(GlyphKind.Pill, "Create") : new Glyph(GlyphKind.View);
                    case "start":
                        return new Glyph(GlyphKind.Pill, playStation ? "Options" : "Menu");
                }
            }

            return new Glyph(GlyphKind.Key, InputControlPath.ToHumanReadableString(path, KeyNameOptions));
        }

        private static Glyph Face(string letter, FaceShape shape, bool playStation)
        {
            return playStation ? new Glyph(GlyphKind.Face, null, shape) : new Glyph(GlyphKind.Face, letter);
        }

        private static bool AllAre(IReadOnlyList<string> paths, string device, string controlPrefix)
        {
            foreach (var path in paths)
            {
                if (DeviceOf(path) != device || !ControlOf(path).StartsWith(controlPrefix))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>"Gamepad" in "&lt;Gamepad&gt;/buttonNorth".</summary>
        private static string DeviceOf(string path)
        {
            var end = path.IndexOf('>');
            return path.StartsWith("<") && end > 1 ? path.Substring(1, end - 1) : string.Empty;
        }

        /// <summary>"buttonNorth" in "&lt;Gamepad&gt;/buttonNorth", "scroll/up" in "&lt;Mouse&gt;/scroll/up".</summary>
        private static string ControlOf(string path)
        {
            var slash = path.IndexOf('/');
            return slash >= 0 ? path.Substring(slash + 1) : path;
        }
    }
}
