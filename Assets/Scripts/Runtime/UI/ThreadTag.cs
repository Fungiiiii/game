using Fungiiiii.Champimaison;
using Fungiiiii.Interaction;
using Fungiiiii.Runtime.Player;
using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The thread waiting in the Champimaison, on a line of its own under its panel: the key
    /// and "Collect the thread", gently pulsing so that the player notices it, as long as
    /// thread waits and the player is near. Nothing shows while a menu or the opening shot
    /// takes the view.
    /// </summary>
    internal sealed class ThreadTag : MonoBehaviour
    {
        [Tooltip("The key and what a press does.")]
        [SerializeField]
        private KeyPrompt prompt;

        [Tooltip("Furthest from the Champimaison it shows, in metres.")]
        [SerializeField, Min(1f)]
        private float showWithin = 12f;

        [Tooltip("Height above the Champimaison's foot, in metres: under its panel.")]
        [SerializeField, Min(0f)]
        private float height = 0.6f;

        private FungalHole hole;
        private FirstPersonCamera look;
        private PlayerInteractor interactor;

        /// <summary>Whether the key and its line are shown.</summary>
        public bool IsShown => prompt.IsKeyVisible;

        /// <summary>The line shown right of the key, an entry of the UI string table; empty when hidden.</summary>
        public string ActionKey => prompt.PressActionKey;

        /// <summary>Whether it pulses.</summary>
        public bool IsPulsing => prompt.IsKeyVisible && prompt.Pulses;

        /// <summary>What is missing for it to work, or null when nothing is.</summary>
        public string FindProblem()
        {
            return prompt == null ? $"{name} has no key to show." : null;
        }

        /// <param name="champimaison">The planted Champimaison, whose thread it tells about.</param>
        /// <param name="playerLook">The player's view: nothing shows while a menu or the opening shot takes it.</param>
        /// <param name="playerInteractor">Draws the interact control as on the device used last.</param>
        public void Bind(FungalHole champimaison, FirstPersonCamera playerLook, PlayerInteractor playerInteractor)
        {
            hole = champimaison;
            look = playerLook;
            interactor = playerInteractor;
            prompt.Pulses = true;
        }

        private void LateUpdate()
        {
            if (hole == null || look == null || !look.isActiveAndEnabled || hole.CanBeEntered || hole.Thread.Stored <= 0)
            {
                prompt.Hide();
                return;
            }

            var view = look.Camera;
            var point = hole.transform.position + Vector3.up * height;
            var screenPoint = view.WorldToScreenPoint(point);
            if (screenPoint.z <= 0f || Vector3.Distance(view.transform.position, point) > showWithin)
            {
                prompt.Hide();
                return;
            }

            prompt.Show(screenPoint, interactor.InteractGlyph);
            prompt.SetMarks(true, false, 0f);
            prompt.SetActions(UiText.HintCollectThread, null);
        }
    }
}
