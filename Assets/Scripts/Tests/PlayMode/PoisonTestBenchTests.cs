#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using Fungiiiii.Survival;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Fungiiiii.Tests.PlayMode
{
    public sealed class PoisonTestBenchTests
    {
        private GameObject root;
        private Keyboard testKeyboard;
        private Keyboard previousKeyboard;

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (root != null) Object.Destroy(root);
            if (testKeyboard != null) InputSystem.RemoveDevice(testKeyboard);
            if (previousKeyboard != null && previousKeyboard.added) previousKeyboard.MakeCurrent();
            testKeyboard = null;
            previousKeyboard = null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator KeyboardShortcutsApplyRemovePauseResetAndRespectGuide()
        {
            PoisonPrototypeController controller = CreateController();
            controller.enabled = true;
            previousKeyboard = Keyboard.current;
            testKeyboard = InputSystem.AddDevice<Keyboard>();
            testKeyboard.MakeCurrent();
            yield return Press(Key.P);
            Assert.That(controller.Poison.Intensity, Is.EqualTo(10f));
            yield return Press(Key.O);
            Assert.That(controller.Poison.Intensity, Is.Zero);
            yield return Press(Key.Space);
            Assert.That(controller.IsPaused, Is.True);
            yield return Press(Key.R);
            Assert.That(controller.IsPaused, Is.False);
            Assert.That(controller.CurrentHealth, Is.EqualTo(100f));
            Click("HelpButton");
            yield return Press(Key.P);
            yield return Press(Key.R);
            yield return Press(Key.Space);
            Assert.That(controller.Poison.Intensity, Is.Zero);
            Assert.That(controller.IsPaused, Is.True);
            Click("CloseHelpButton");
            controller.SetPoison(100f);
            controller.Simulate(20f);
            yield return Press(Key.O);
            Assert.That(controller.Poison.Intensity, Is.EqualTo(100f));
            yield return Press(Key.R);
            Assert.That(controller.IsDead, Is.False);
        }

        private IEnumerator Press(Key key)
        {
            InputSystem.QueueStateEvent(testKeyboard, new KeyboardState(key));
            yield return null;
            InputSystem.QueueStateEvent(testKeyboard, new KeyboardState());
            yield return null;
        }

        [UnityTest]
        public IEnumerator ButtonsDriveVisibleHealthPoisonPauseAndReset()
        {
            PoisonPrototypeController controller = CreateController();
            Click("ApplyButton");
            yield return null;
            Assert.That(controller.Poison.Intensity, Is.EqualTo(10f));
            Click("RemoveButton");
            Assert.That(controller.Poison.Intensity, Is.Zero);
            Click("MaximumPoisonButton");
            controller.Simulate(2f);
            yield return null;
            Assert.That(Label("HealthValue"), Does.Contain(80f.ToString("0.0")));
            Assert.That(Label("IntensityValue"), Does.Contain("100"));
            Assert.That(Label("DamageValue"), Does.Contain(10f.ToString("0.00")));
            Click("PauseButton");
            controller.Simulate(2f);
            yield return null;
            Assert.That(controller.CurrentHealth, Is.EqualTo(80f));
            Assert.That(Label("StateLabel"), Is.EqualTo("PAUSED"));
            Click("CureButton");
            Assert.That(controller.Poison.Intensity, Is.Zero);
            Assert.That(controller.CurrentHealth, Is.EqualTo(80f));
            Click("ResetButton");
            yield return null;
            Assert.That(controller.CurrentHealth, Is.EqualTo(100f));
            Assert.That(controller.IsPaused, Is.False);
            Assert.That(controller.IsPoisonVisible, Is.False);
            Assert.That(Label("StateLabel"), Is.EqualTo("HEALTHY"));
        }

        [UnityTest]
        public IEnumerator DeathOffersRestartAndGuidePreservesPauseState()
        {
            PoisonPrototypeController controller = CreateController();
            PoisonPrototypeView view = root.GetComponent<PoisonPrototypeView>();
            Click("HelpButton");
            Assert.That(view.IsHelpVisible, Is.True);
            Assert.That(controller.IsPaused, Is.True);
            Click("CloseHelpButton");
            Assert.That(controller.IsPaused, Is.False);
            controller.TogglePause();
            Click("HelpButton");
            Click("CloseHelpButton");
            Assert.That(controller.IsPaused, Is.True);
            controller.TogglePause();
            controller.SetPoison(100f);
            controller.Simulate(20f);
            yield return null;
            Assert.That(view.IsDeathVisible, Is.True);
            Assert.That(controller.CurrentHealth, Is.Zero);
            Assert.That(Label("StateLabel"), Is.EqualTo("DEAD"));
            Click("RestartButton");
            yield return null;
            Assert.That(view.IsDeathVisible, Is.False);
            Assert.That(controller.CurrentHealth, Is.EqualTo(100f));
            Assert.That(controller.Poison.Intensity, Is.Zero);
        }

        [UnityTest]
        public IEnumerator PresenterRebindAndReenableRefreshFromAuthoritativeState()
        {
            root = new GameObject("PresenterTest");
            PoisonHudPresenter presenter = root.AddComponent<PoisonHudPresenter>();
            GameObject gauge = new GameObject("Gauge", typeof(RectTransform), typeof(Slider));
            gauge.transform.SetParent(root.transform);
            Slider slider = gauge.GetComponent<Slider>();
            PoisonState first = new PoisonState(100f);
            PoisonState second = new PoisonState(100f);
            presenter.Bind(first, slider, gauge);
            first.Apply(25f);
            Assert.That(slider.value, Is.EqualTo(25f));
            presenter.Bind(second, slider, gauge);
            first.Apply(25f);
            Assert.That(gauge.activeSelf, Is.False);
            presenter.enabled = false;
            second.Apply(75f);
            Assert.That(slider.value, Is.Zero);
            presenter.enabled = true;
            Assert.That(slider.value, Is.EqualTo(75f));
            Assert.That(gauge.activeSelf, Is.True);
            Object.Destroy(root);
            yield return null;
            Assert.DoesNotThrow(() => second.Clear());
        }

        private PoisonPrototypeController CreateController()
        {
            root = new GameObject("PoisonTest");
            PoisonPrototypeController controller = root.AddComponent<PoisonPrototypeController>();
            controller.enabled = false;
            return controller;
        }

        private void Click(string name)
        {
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
            {
                if (button.name != name) continue;
                Assert.That(button.gameObject.activeInHierarchy && button.interactable, Is.True, name);
                button.onClick.Invoke();
                return;
            }
            Assert.Fail($"Missing test bench button: {name}");
        }

        private string Label(string name)
        {
            foreach (Text label in root.GetComponentsInChildren<Text>(true))
                if (label.name == name) return label.text;
            Assert.Fail($"Missing diagnostic label: {name}");
            return string.Empty;
        }
    }
}
#endif
