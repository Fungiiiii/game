using Fungiiiii.UI;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class InputGlyphsTests
    {
        private static Glyph Xbox(params string[] paths) => InputGlyphs.Describe(paths, playStation: false);

        private static Glyph PlayStation(params string[] paths) => InputGlyphs.Describe(paths, playStation: true);

        [Test]
        public void TheMouseButtons_AreAMouseWithThatButtonLit()
        {
            Assert.That(Xbox("<Mouse>/leftButton").Kind, Is.EqualTo(GlyphKind.MouseLeft));
            Assert.That(Xbox("<Mouse>/rightButton").Kind, Is.EqualTo(GlyphKind.MouseRight));
        }

        [Test]
        public void TheWheelUpAndDown_AreOneWheel()
        {
            Assert.That(Xbox("<Mouse>/scroll/down", "<Mouse>/scroll/up").Kind, Is.EqualTo(GlyphKind.MouseWheel));
        }

        [Test]
        public void AKey_IsItsShortName()
        {
            Assert.That(Xbox("<Keyboard>/tab"), Is.EqualTo(new Glyph(GlyphKind.Key, "Tab")));
            Assert.That(Xbox("<Keyboard>/e"), Is.EqualTo(new Glyph(GlyphKind.Key, "E")));
        }

        [Test]
        public void TheFaceButtons_AreLettersOnXbox_AndShapesOnPlayStation()
        {
            Assert.That(Xbox("<Gamepad>/buttonSouth"), Is.EqualTo(new Glyph(GlyphKind.Face, "A")));
            Assert.That(Xbox("<Gamepad>/buttonNorth"), Is.EqualTo(new Glyph(GlyphKind.Face, "Y")));
            Assert.That(PlayStation("<Gamepad>/buttonSouth"), Is.EqualTo(new Glyph(GlyphKind.Face, null, FaceShape.Cross)));
            Assert.That(PlayStation("<Gamepad>/buttonEast"), Is.EqualTo(new Glyph(GlyphKind.Face, null, FaceShape.Circle)));
            Assert.That(PlayStation("<Gamepad>/buttonWest"), Is.EqualTo(new Glyph(GlyphKind.Face, null, FaceShape.Square)));
            Assert.That(PlayStation("<Gamepad>/buttonNorth"), Is.EqualTo(new Glyph(GlyphKind.Face, null, FaceShape.Triangle)));
        }

        [Test]
        public void TheTriggersAndShoulders_AreNamedAsOnThePad()
        {
            Assert.That(Xbox("<Gamepad>/rightTrigger"), Is.EqualTo(new Glyph(GlyphKind.Pill, "RT")));
            Assert.That(PlayStation("<Gamepad>/leftTrigger"), Is.EqualTo(new Glyph(GlyphKind.Pill, "L2")));
            Assert.That(Xbox("<Gamepad>/rightShoulder", "<Gamepad>/leftShoulder"), Is.EqualTo(new Glyph(GlyphKind.Pill, "LB · RB")));
            Assert.That(PlayStation("<Gamepad>/rightShoulder", "<Gamepad>/leftShoulder"), Is.EqualTo(new Glyph(GlyphKind.Pill, "L1 · R1")));
        }

        [Test]
        public void Select_IsTheViewButtonOnXbox_AndCreateOnPlayStation()
        {
            Assert.That(Xbox("<Gamepad>/select").Kind, Is.EqualTo(GlyphKind.View));
            Assert.That(PlayStation("<Gamepad>/select"), Is.EqualTo(new Glyph(GlyphKind.Pill, "Create")));
        }

        [Test]
        public void NoControl_IsAQuestionMark()
        {
            Assert.That(Xbox(), Is.EqualTo(new Glyph(GlyphKind.Key, "?")));
        }
    }
}
