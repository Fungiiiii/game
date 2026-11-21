using System.Collections.Generic;
using Fungiiiii.Controls;
using UnityEngine;
using UnityEngine.InputSystem.DualShock;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The key hints, bottom right of the screen, without a background (as on the key
    /// hints mockup): one line per hint, the control drawn for the device in hand
    /// and, right after it, what it does. The lines come from the sources in charge, in
    /// their order; with too many, optional ones are left out first.
    ///
    /// The panel never changes for a new hint: its source declares it.
    /// </summary>
    internal sealed class ControlHintsPanel : MonoBehaviour
    {
        public const int MaxHints = 4;

        [Tooltip("The lines' parent, bottom right.")]
        [SerializeField]
        private RectTransform list;

        [SerializeField]
        private HintRow rowPrefab;

        [Header("Controls, by how they are drawn")]
        [Tooltip("A keyboard key, as wide as its name.")]
        [SerializeField]
        private GlyphView keyPrefab;

        [Tooltip("A rounded button: gamepad shoulders, triggers, menu.")]
        [SerializeField]
        private GlyphView pillPrefab;

        [Tooltip("A gamepad face button: its letter, or its PlayStation shape.")]
        [SerializeField]
        private GlyphView facePrefab;

        [SerializeField]
        private GlyphView mousePrefab;

        [Tooltip("The Xbox View button.")]
        [SerializeField]
        private GlyphView viewPrefab;

        private readonly List<ControlHint> shown = new List<ControlHint>();
        private readonly List<Glyph> shownGlyphs = new List<Glyph>();
        private IHintSource[] sources = new IHintSource[0];
        private PlayerControls controls;

        /// <summary>The hints on screen, in order.</summary>
        public IReadOnlyList<ControlHint> Shown => shown;

        /// <summary>How each hint on screen is drawn, in order.</summary>
        public IReadOnlyList<Glyph> ShownGlyphs => shownGlyphs;

        public void Initialize(PlayerControls playerControls)
        {
            controls = playerControls;
            controls.SchemeChanged += Rebuild;
        }

        /// <summary>Shows the hints of these sources, in this order; none hides the panel's lines.</summary>
        public void ShowFrom(params IHintSource[] next)
        {
            foreach (var source in sources)
            {
                source.HintsChanged -= Rebuild;
            }

            sources = next ?? new IHintSource[0];
            foreach (var source in sources)
            {
                source.HintsChanged += Rebuild;
            }

            Rebuild();
        }

        private void OnDestroy()
        {
            foreach (var source in sources)
            {
                source.HintsChanged -= Rebuild;
            }

            if (controls != null)
            {
                controls.SchemeChanged -= Rebuild;
            }
        }

        private void Rebuild()
        {
            if (controls == null)
            {
                return;
            }

            Compose();
            for (var child = list.childCount - 1; child >= 0; child--)
            {
                Destroy(list.GetChild(child).gameObject);
            }

            var scheme = controls.CurrentScheme;
            var playStation = controls.CurrentDevice is DualShockGamepad;
            shownGlyphs.Clear();
            foreach (var hint in shown)
            {
                var glyph = hint.GlyphFor(scheme, playStation);
                shownGlyphs.Add(glyph);
                Instantiate(rowPrefab, list).Show(PrefabFor(glyph), glyph, hint.IsTwiceWith(scheme), hint.LabelKey);
            }
        }

        /// <summary>The hints of every source, optional ones left out first while there are too many.</summary>
        private void Compose()
        {
            shown.Clear();
            foreach (var source in sources)
            {
                shown.AddRange(source.Hints);
            }

            for (var index = shown.Count - 1; index >= 0 && shown.Count > MaxHints; index--)
            {
                if (shown[index].IsOptional)
                {
                    shown.RemoveAt(index);
                }
            }

            if (shown.Count > MaxHints)
            {
                shown.RemoveRange(MaxHints, shown.Count - MaxHints);
            }
        }

        private GlyphView PrefabFor(Glyph glyph)
        {
            switch (glyph.Kind)
            {
                case GlyphKind.MouseLeft:
                case GlyphKind.MouseRight:
                case GlyphKind.MouseWheel:
                    return mousePrefab;
                case GlyphKind.Face:
                    return facePrefab;
                case GlyphKind.View:
                    return viewPrefab;
                case GlyphKind.Pill:
                    return pillPrefab;
                default:
                    return keyPrefab;
            }
        }
    }
}
