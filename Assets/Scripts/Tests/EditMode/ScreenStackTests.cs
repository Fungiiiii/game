using System;
using System.Collections.Generic;
using Fungiiiii.UI;
using NUnit.Framework;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class ScreenStackTests
    {
        private FakeScreen pause;
        private FakeScreen inventory;
        private FakeScreen craft;
        private ScreenStack screens;
        private int changes;

        [SetUp]
        public void SetUp()
        {
            pause = new FakeScreen();
            inventory = new FakeScreen();
            craft = new FakeScreen();
            screens = new ScreenStack(pause);
            changes = 0;
            screens.Changed += () => changes++;
        }

        [Test]
        public void Back_WithNothingOpen_OpensThePause()
        {
            screens.Back();

            Assert.That(screens.Top, Is.SameAs(pause));
            Assert.That(pause.IsOpen, Is.True);
            Assert.That(screens.IsAnyOpen, Is.True);
        }

        [Test]
        public void Back_WithAScreenOpen_ClosesIt_AndDoesNotOpenThePause()
        {
            screens.Open(inventory);

            screens.Back();

            Assert.That(inventory.IsOpen, Is.False);
            Assert.That(pause.IsOpen, Is.False);
            Assert.That(screens.IsAnyOpen, Is.False);
        }

        [Test]
        public void Back_OnThePause_ResumesTheGame()
        {
            screens.Back();

            screens.Back();

            Assert.That(pause.IsOpen, Is.False);
            Assert.That(screens.IsAnyOpen, Is.False);
        }

        [Test]
        public void Toggle_OpensAScreen_WhenNothingIsOpen_AndClosesItAgain()
        {
            screens.Toggle(inventory);
            Assert.That(inventory.IsOpen, Is.True);

            screens.Toggle(inventory);
            Assert.That(inventory.IsOpen, Is.False);
        }

        [Test]
        public void Toggle_DoesNothing_WhileAnotherScreenIsOpen()
        {
            screens.Back();

            screens.Toggle(inventory);

            Assert.That(inventory.IsOpen, Is.False, "The inventory opened over the pause.");
            Assert.That(screens.Top, Is.SameAs(pause));
        }

        [Test]
        public void AScreenOpenedFromAnother_ClosesFirst_ThenTheOneUnder()
        {
            screens.Open(craft);
            screens.Open(inventory);

            screens.Back();
            Assert.That(inventory.IsOpen, Is.False);
            Assert.That(craft.IsOpen, Is.True);
            Assert.That(screens.Top, Is.SameAs(craft));

            screens.Back();
            Assert.That(craft.IsOpen, Is.False);
            Assert.That(screens.IsAnyOpen, Is.False);
        }

        [Test]
        public void OpeningTheScreenOnTop_AgainDoesNothing()
        {
            screens.Open(inventory);

            screens.Open(inventory);

            Assert.That(inventory.OpenCount, Is.EqualTo(1));
            Assert.That(changes, Is.EqualTo(1));
        }

        [Test]
        public void AScreenThatAsksToClose_IsClosed()
        {
            screens.Back();

            pause.RequestClose();

            Assert.That(pause.IsOpen, Is.False);
            Assert.That(screens.IsAnyOpen, Is.False);
        }

        [Test]
        public void AClosedScreenAskingToClose_ChangesNothing()
        {
            screens.Open(inventory);
            pause.RequestClose();

            Assert.That(inventory.IsOpen, Is.True);
            Assert.That(changes, Is.EqualTo(1));
        }

        [Test]
        public void Close_OfAScreenThatIsNotOpen_DoesNothing()
        {
            screens.Close(inventory);

            Assert.That(changes, Is.Zero);
            Assert.That(inventory.CloseCount, Is.Zero);
        }

        [Test]
        public void Changed_IsRaisedOnEveryOpeningAndClosing()
        {
            screens.Toggle(inventory);
            screens.Back();

            Assert.That(changes, Is.EqualTo(2));
        }

        [Test]
        public void IsOpen_TellsWhetherAScreenIsInTheStack()
        {
            screens.Open(craft);
            screens.Open(inventory);

            Assert.That(screens.IsOpen(craft), Is.True);
            Assert.That(screens.IsOpen(inventory), Is.True);
            Assert.That(screens.IsOpen(pause), Is.False);
        }

        private sealed class FakeScreen : IGameScreen
        {
            public event Action CloseRequested;

            public bool IsOpen { get; private set; }

            public int OpenCount { get; private set; }

            public int CloseCount { get; private set; }

            public void Open()
            {
                IsOpen = true;
                OpenCount++;
            }

            public void Close()
            {
                IsOpen = false;
                CloseCount++;
            }

            public void RequestClose() => CloseRequested?.Invoke();
        }
    }
}
