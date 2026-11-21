using Fungiiiii.Controls;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class ControlMapsTests : InputTestFixture
    {
        private ControlMaps maps;

        public override void Setup()
        {
            base.Setup();
            maps = new ControlMaps();
            maps.SetEnabled(true);
        }

        public override void TearDown()
        {
            maps.Dispose();
            base.TearDown();
        }

        [Test]
        public void InGame_ThePlayerAndScreenMapsAreOn_NotTheUiMap()
        {
            Assert.That(maps.Player.enabled, Is.True);
            Assert.That(maps.Screens.enabled, Is.True);
            Assert.That(maps.UI.enabled, Is.False);
        }

        [Test]
        public void AnOpenMenu_TurnsThePlayerMapOff_AndTheUiMapOn()
        {
            maps.SetMenuOpen(true);

            Assert.That(maps.Player.enabled, Is.False);
            Assert.That(maps.Screens.enabled, Is.True);
            Assert.That(maps.UI.enabled, Is.True);
        }

        [Test]
        public void AClosedMenu_GivesThePlayerMapBack()
        {
            maps.SetMenuOpen(true);

            maps.SetMenuOpen(false);

            Assert.That(maps.Player.enabled, Is.True);
            Assert.That(maps.UI.enabled, Is.False);
        }

        [Test]
        public void Disabled_EveryMapIsOff_AndEnabledAgain_TheOpenMenuIsKept()
        {
            maps.SetMenuOpen(true);

            maps.SetEnabled(false);

            Assert.That(maps.Player.enabled, Is.False);
            Assert.That(maps.Screens.enabled, Is.False);
            Assert.That(maps.UI.enabled, Is.False);

            maps.SetEnabled(true);

            Assert.That(maps.Player.enabled, Is.False);
            Assert.That(maps.UI.enabled, Is.True);
        }

        [Test]
        public void TheSchemeStartsOnKeyboardAndMouse()
        {
            Assert.That(maps.CurrentScheme.name, Is.EqualTo(maps.KeyboardMouseScheme.name));
        }

        [Test]
        public void UsingAGamepad_SwitchesToTheGamepadScheme_AndSaysSo()
        {
            var gamepad = InputSystem.AddDevice<Gamepad>();
            var changes = 0;
            maps.SchemeChanged += () => changes++;

            Press(gamepad.buttonNorth);

            Assert.That(maps.CurrentScheme.name, Is.EqualTo(maps.GamepadScheme.name));
            Assert.That(maps.CurrentDevice, Is.SameAs(gamepad));
            Assert.That(changes, Is.EqualTo(1));
        }

        [Test]
        public void UsingTheKeyboardAgain_SwitchesBack()
        {
            var gamepad = InputSystem.AddDevice<Gamepad>();
            var keyboard = InputSystem.AddDevice<Keyboard>();
            Press(gamepad.buttonNorth);
            Release(gamepad.buttonNorth);

            Press(keyboard.eKey);

            Assert.That(maps.CurrentScheme.name, Is.EqualTo(maps.KeyboardMouseScheme.name));
            Assert.That(maps.CurrentDevice, Is.SameAs(keyboard));
        }

        [Test]
        public void TheSameScheme_DoesNotSayItChangedAgain()
        {
            var gamepad = InputSystem.AddDevice<Gamepad>();
            Press(gamepad.buttonNorth);
            Release(gamepad.buttonNorth);
            var changes = 0;
            maps.SchemeChanged += () => changes++;

            Press(gamepad.buttonWest);

            Assert.That(changes, Is.EqualTo(0));
        }

        [Test]
        public void TheGamepadSticks_AreEnoughToSwitch()
        {
            var gamepad = InputSystem.AddDevice<Gamepad>();

            Set(gamepad.rightStick, new UnityEngine.Vector2(0.8f, 0f));

            Assert.That(maps.CurrentScheme.name, Is.EqualTo(maps.GamepadScheme.name));
        }
    }
}
