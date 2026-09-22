#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Fungiiiii.UI;
using UnityEngine.InputSystem;
#endif
using UnityEngine;

namespace Fungiiiii.Survival
{
    /// <summary>Drives the simulation and connects the development prototype view.</summary>
    [DisallowMultipleComponent]
    public sealed class PoisonPrototypeController : MonoBehaviour
    {
        private const float DemoMaxHealth = 100f;
        private const float DemoApplicationAmount = 10f;

        private PoisonSimulation simulation;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private PoisonPrototypeView view;
#endif

        public PoisonState Poison => simulation.Poison;
        public float MaxHealth => simulation.MaxHealth;
        public float CurrentHealth => simulation.CurrentHealth;
        public bool IsDead => simulation.IsDead;
        public bool IsPaused => simulation.IsPaused;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool IsPoisonVisible => view != null && view.IsPoisonVisible;
        public float PoisonPercentage => view == null ? 0f : view.PoisonPercentage;
#else
        public bool IsPoisonVisible => false;
        public float PoisonPercentage => 0f;
#endif

        private void Awake()
        {
            simulation = new PoisonSimulation(DemoMaxHealth);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            view = gameObject.AddComponent<PoisonPrototypeView>();
            view.Initialize(this);
#endif
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Keyboard keyboard = Keyboard.current;
            if (view != null && view.IsHelpVisible)
            {
                keyboard = null;
            }
            if (keyboard != null)
            {
                if (!IsDead && keyboard.pKey.wasPressedThisFrame)
                {
                    ApplyPoison(DemoApplicationAmount);
                }

                if (!IsDead && keyboard.oKey.wasPressedThisFrame)
                {
                    RemovePoison(DemoApplicationAmount);
                }

                if (keyboard.rKey.wasPressedThisFrame)
                {
                    ResetPrototype();
                }

                if (keyboard.spaceKey.wasPressedThisFrame)
                {
                    TogglePause();
                }
            }
#endif

            Simulate(Time.deltaTime);
        }

        public void ApplyPoison(float amount) => Poison.Apply(amount);
        public void RemovePoison(float amount) => Poison.Remove(amount);
        public void SetPoison(float intensity) => Poison.SetIntensity(intensity);
        public void ClearPoison() => Poison.Clear();
        public void TogglePause() => simulation.TogglePause();
        public void ResetPrototype() => simulation.Reset();
        public void Simulate(float deltaTime) => simulation.Simulate(deltaTime);
    }
}
