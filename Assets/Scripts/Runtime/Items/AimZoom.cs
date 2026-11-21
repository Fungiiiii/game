using Fungiiiii.Capture;
using UnityEngine;

namespace Fungiiiii.Items
{
    /// <summary>
    /// Narrows the view while an item aims, and slows the look by as much so that the aim
    /// stays as precise on screen: the net today, any item that aims later. It sits on the
    /// player's camera and is shared by every item.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public sealed class AimZoom : MonoBehaviour
    {
        private const float TransitionSeconds = 0.15f;

        private Camera view;
        private PrototypeFirstPersonCamera look;
        private float baseFieldOfView;
        private float aimFieldOfView;
        private float blend;
        private bool isAiming;

        /// <summary>The field of view outside any aim, in degrees.</summary>
        public float BaseFieldOfView => baseFieldOfView;

        /// <summary>Narrows the view to <paramref name="fieldOfView"/> over a moment while <paramref name="aiming"/>, widens it back otherwise.</summary>
        public void SetAiming(bool aiming, float fieldOfView)
        {
            isAiming = aiming;
            if (aiming)
            {
                aimFieldOfView = fieldOfView;
            }
        }

        private void Awake()
        {
            view = GetComponent<Camera>();
            TryGetComponent(out look);
            baseFieldOfView = view.fieldOfView;
            aimFieldOfView = baseFieldOfView;
        }

        private void Update()
        {
            var target = isAiming ? 1f : 0f;
            if (Mathf.Approximately(blend, target))
            {
                return;
            }

            // Real time: the view widens back under the pause too, which stops game time.
            blend = Mathf.MoveTowards(blend, target, Time.unscaledDeltaTime / TransitionSeconds);
            Apply();
        }

        private void Apply()
        {
            view.fieldOfView = Mathf.Lerp(baseFieldOfView, aimFieldOfView, blend);
            if (look != null)
            {
                look.LookScale = view.fieldOfView / baseFieldOfView;
            }
        }
    }
}
