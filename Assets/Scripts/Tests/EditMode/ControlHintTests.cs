using Fungiiiii.Controls;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class ControlHintTests
    {
        private GameControls controls;

        [SetUp]
        public void SetUp()
        {
            controls = new GameControls();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(controls.asset);
        }

        [Test]
        public void AnAction_ShowsItsBindingInTheSchemeInUse()
        {
            var hint = new ControlHint("hint.gather", HintInput.Of(controls.Player.Interact));

            Assert.That(hint.GlyphFor(controls.KeyboardMouseScheme, false), Is.EqualTo(new Glyph(GlyphKind.Key, "E")));
            Assert.That(hint.GlyphFor(controls.GamepadScheme, false), Is.EqualTo(new Glyph(GlyphKind.Face, "Y")));
            Assert.That(hint.GlyphFor(controls.GamepadScheme, true), Is.EqualTo(new Glyph(GlyphKind.Face, null, FaceShape.Triangle)));
        }

        [Test]
        public void ARemappedAction_ShowsItsNewKey()
        {
            controls.Player.Interact.ApplyBindingOverride(0, "<Keyboard>/f");
            var hint = new ControlHint("hint.gather", HintInput.Of(controls.Player.Interact));

            Assert.That(hint.GlyphFor(controls.KeyboardMouseScheme, false), Is.EqualTo(new Glyph(GlyphKind.Key, "F")));
        }

        [Test]
        public void TwoActions_AreShownTogether()
        {
            var hint = new ControlHint("hint.switch_item", HintInput.Of(controls.Player.HotbarNext, controls.Player.HotbarPrevious));

            Assert.That(hint.GlyphFor(controls.KeyboardMouseScheme, false).Kind, Is.EqualTo(GlyphKind.MouseWheel));
            Assert.That(hint.GlyphFor(controls.GamepadScheme, false), Is.EqualTo(new Glyph(GlyphKind.Pill, "LB · RB")));
        }

        [Test]
        public void EachScheme_CanHaveItsOwnControls_AndTheDoubleClickIsForTheMouseOnly()
        {
            var hint = new ControlHint(
                "hint.gather_all",
                HintInput.At("<Mouse>/leftButton"),
                HintInput.Of(controls.UI.GatherAll),
                twiceWithMouse: true);

            Assert.That(hint.GlyphFor(controls.KeyboardMouseScheme, false).Kind, Is.EqualTo(GlyphKind.MouseLeft));
            Assert.That(hint.IsTwiceWith(controls.KeyboardMouseScheme), Is.True);
            Assert.That(hint.GlyphFor(controls.GamepadScheme, false), Is.EqualTo(new Glyph(GlyphKind.Face, "Y")));
            Assert.That(hint.IsTwiceWith(controls.GamepadScheme), Is.False);
        }

        [Test]
        public void AHint_SaysWhatItDoes()
        {
            Assert.Throws<System.ArgumentException>(() => new ControlHint(string.Empty, HintInput.At("<Keyboard>/e")));
        }

        [Test]
        public void FixedHints_GiveTheirHints()
        {
            IHintSource source = new FixedHints(new ControlHint("hint.inventory", HintInput.Of(controls.Screens.Inventory)));

            Assert.That(source.Hints.Count, Is.EqualTo(1));
            Assert.That(source.Hints[0].LabelKey, Is.EqualTo("hint.inventory"));
        }
    }
}
