using System;
using Fungiiiii.Survival;
using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Presents poison snapshots on a HUD slider and hides the gauge at zero.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PoisonHudPresenter : MonoBehaviour
    {
        private PoisonState poisonState;
        private Slider poisonSlider;
        private GameObject poisonContainer;

        public bool IsPoisonVisible => poisonContainer != null && poisonContainer.activeSelf;

        public float PoisonPercentage => poisonSlider == null ? 0f : poisonSlider.value;

        public void Bind(PoisonState state, Slider slider, GameObject container)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (slider == null)
            {
                throw new ArgumentNullException(nameof(slider));
            }

            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            Unbind();

            poisonState = state;
            poisonSlider = slider;
            poisonContainer = container;
            poisonSlider.minValue = 0f;
            poisonSlider.maxValue = PoisonState.MaxIntensity;
            poisonState.Changed += Refresh;
            Refresh(poisonState.Current);
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void Unbind()
        {
            if (poisonState != null)
            {
                poisonState.Changed -= Refresh;
            }

            poisonState = null;
            poisonSlider = null;
            poisonContainer = null;
        }

        private void Refresh(PoisonSnapshot snapshot)
        {
            if (poisonSlider == null || poisonContainer == null)
            {
                return;
            }

            poisonSlider.value = snapshot.Intensity;
            poisonContainer.SetActive(snapshot.IsActive);
        }
    }
}
