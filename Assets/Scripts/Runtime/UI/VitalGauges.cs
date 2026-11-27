using Fungiiiii.Survival;
using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The player's vitals as gauges, top left: health and stamina always, poison while
    /// the player is poisoned. A placeholder look until the HUD is designed.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public sealed class VitalGauges : MonoBehaviour
    {
        [SerializeField]
        private Gauge health;

        [SerializeField]
        private Gauge stamina;

        [Tooltip("Shown only while the player is poisoned.")]
        [SerializeField]
        private Gauge poison;

        private PlayerVitals model;
        private Canvas canvas;

        public bool HealthGaugeVisible => health.IsVisible;

        public bool StaminaGaugeVisible => stamina.IsVisible;

        public bool PoisonGaugeVisible => poison.IsVisible;

        public float PoisonPercentage => poison.Value * PoisonState.MaxIntensity;

        /// <summary>Whether it is drawn. Hidden while the inventory is open, it keeps following the game.</summary>
        public bool IsVisible => Canvas.enabled;

        private Canvas Canvas
        {
            get
            {
                if (canvas == null)
                {
                    canvas = GetComponent<Canvas>();
                }

                return canvas;
            }
        }

        public void SetVisible(bool visible)
        {
            Canvas.enabled = visible;
        }

        /// <summary>Shows these vitals, and follows them as they change.</summary>
        public void Initialize(PlayerVitals vitals)
        {
            if (vitals == null)
            {
                throw new System.ArgumentNullException(nameof(vitals));
            }

            if (model != null)
            {
                model.Changed -= Refresh;
            }

            model = vitals;
            model.Changed += Refresh;
            Refresh(model.Current);
        }

        private void OnDestroy()
        {
            if (model != null)
            {
                model.Changed -= Refresh;
            }
        }

        private void Refresh(VitalsSnapshot snapshot)
        {
            health.Show(snapshot.HealthNormalized);
            stamina.Show(snapshot.StaminaNormalized);
            poison.Show(snapshot.PoisonNormalized);
            poison.SetVisible(snapshot.IsPoisoned);
        }
    }
}
