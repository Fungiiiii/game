using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Fungiiiii.UI;
using UnityEngine.InputSystem;
#endif

namespace Fungiiiii.Survival
{
    /// <summary>Development scene driver; PlayerVitals remains the sole simulation owner.</summary>
    [DisallowMultipleComponent]
    public sealed class PoisonTestBenchController : MonoBehaviour
    {
        private const float DemoMaxHealth = 100f;
        private const float DemoMaxStamina = 100f;
        private const float DemoDose = 10f;

        private PlayerVitals vitals;
        private bool isPaused;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private PoisonPrototypeView view;
#endif

        public PlayerVitals Vitals => vitals;
        public PoisonState Poison => vitals.Poison;
        public float MaxHealth => vitals.MaxHealth;
        public float CurrentHealth => vitals.Current.Health;
        public float PoisonDamagePerSecond => vitals.PoisonDamagePerSecond;
        public bool IsDead => vitals.IsDead;
        public bool IsPaused => isPaused;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool IsPoisonVisible => view != null && view.IsPoisonVisible;
        public float PoisonPercentage => view == null ? 0f : view.PoisonPercentage;
#else
        public bool IsPoisonVisible => false;
        public float PoisonPercentage => 0f;
#endif

        private void Awake()
        {
            vitals = new PlayerVitals(DemoMaxHealth, DemoMaxStamina);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            view = gameObject.AddComponent<PoisonPrototypeView>();
            view.Initialize(this);
#endif
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.pKey.wasPressedThisFrame) HandleShortcut(Key.P);
                if (keyboard.oKey.wasPressedThisFrame) HandleShortcut(Key.O);
                if (keyboard.rKey.wasPressedThisFrame) HandleShortcut(Key.R);
                if (keyboard.spaceKey.wasPressedThisFrame) HandleShortcut(Key.Space);
            }
#endif
            Simulate(Time.deltaTime);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>Deterministic shortcut dispatcher, also testable without an OS keyboard.</summary>
        public void HandleShortcut(Key key)
        {
            if (view != null && view.IsHelpVisible) return;
            switch (key)
            {
                case Key.P when !IsDead:
                    ApplyPoison(DemoDose);
                    break;
                case Key.O when !IsDead:
                    RemovePoison(DemoDose);
                    break;
                case Key.R:
                    ResetPrototype();
                    break;
                case Key.Space:
                    TogglePause();
                    break;
            }
        }
#endif

        public void ApplyPoison(float amount) => Poison.Apply(amount);
        public void RemovePoison(float amount) => Poison.Remove(amount);
        public void SetPoison(float intensity) => Poison.SetIntensity(intensity);
        public void ClearPoison() => Poison.Clear();
        public void TogglePause() => isPaused = !isPaused;

        public void ResetPrototype()
        {
            isPaused = false;
            vitals.Reset();
        }

        public void Simulate(float deltaTime)
        {
            if (!isPaused) vitals.Tick(deltaTime);
        }
    }
}
