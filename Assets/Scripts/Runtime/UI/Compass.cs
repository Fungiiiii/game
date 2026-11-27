using System;
using Fungiiiii.Runtime.Player;
using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Top centre during play, as in other games: a band where the cardinal points slide as
    /// the player turns, and a yellow "!" where the objective lies. The mark sticks to the
    /// band's edge when the objective is behind, so that the player always knows which way
    /// to turn. Hidden while a menu or the opening shot takes the view.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    internal sealed class Compass : MonoBehaviour
    {
        [Tooltip("The band: its width shows the angle below, either side of the middle.")]
        [SerializeField]
        private RectTransform band;

        [SerializeField]
        private RectTransform north;

        [SerializeField]
        private RectTransform east;

        [SerializeField]
        private RectTransform south;

        [SerializeField]
        private RectTransform west;

        [Tooltip("The yellow \"!\" where the objective lies.")]
        [SerializeField]
        private RectTransform objectiveMark;

        [Tooltip("Degrees shown on the band either side of the middle.")]
        [SerializeField, Range(30f, 180f)]
        private float halfSpanDegrees = 90f;

        private FirstPersonCamera look;
        private Func<Vector3?> objective;
        private Canvas canvas;

        /// <summary>Whether it is drawn.</summary>
        public bool IsShown => Canvas.enabled;

        /// <summary>Whether the objective's mark is on the band.</summary>
        public bool IsObjectiveShown => IsShown && objectiveMark.gameObject.activeSelf;

        /// <summary>Where the objective's mark stands on the band, from -1 (left edge) to 1 (right edge).</summary>
        public float ObjectivePosition => objectiveMark.anchoredPosition.x / HalfWidth;

        private float HalfWidth => Mathf.Max(1f, band.rect.width * 0.5f);

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

        /// <summary>What is missing for it to work, or null when nothing is.</summary>
        public string FindProblem()
        {
            return band == null || north == null || east == null || south == null || west == null || objectiveMark == null
                ? $"{name} lacks its band, a cardinal point or the objective's mark."
                : null;
        }

        /// <param name="playerLook">The player's view, whose heading the band follows.</param>
        /// <param name="objectiveTarget">Where the objective lies now, or null when nowhere in particular.</param>
        public void Bind(FirstPersonCamera playerLook, Func<Vector3?> objectiveTarget)
        {
            look = playerLook;
            objective = objectiveTarget;
        }

        private void LateUpdate()
        {
            var shown = look != null && look.isActiveAndEnabled;
            if (Canvas.enabled != shown)
            {
                Canvas.enabled = shown;
            }

            if (!shown)
            {
                return;
            }

            var view = look.transform;
            var heading = view.eulerAngles.y;
            PlacePoint(north, heading, 0f);
            PlacePoint(east, heading, 90f);
            PlacePoint(south, heading, 180f);
            PlacePoint(west, heading, 270f);

            var target = objective?.Invoke();
            var hasTarget = target.HasValue;
            if (objectiveMark.gameObject.activeSelf != hasTarget)
            {
                objectiveMark.gameObject.SetActive(hasTarget);
            }

            if (hasTarget)
            {
                var toTarget = target.Value - view.position;
                var bearing = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
                var offset = Mathf.Clamp(Mathf.DeltaAngle(heading, bearing) / halfSpanDegrees, -1f, 1f);
                objectiveMark.anchoredPosition = new Vector2(offset * HalfWidth, objectiveMark.anchoredPosition.y);
            }
        }

        private void PlacePoint(RectTransform point, float heading, float bearing)
        {
            var offset = Mathf.DeltaAngle(heading, bearing) / halfSpanDegrees;
            var visible = Mathf.Abs(offset) <= 1f;
            if (point.gameObject.activeSelf != visible)
            {
                point.gameObject.SetActive(visible);
            }

            if (visible)
            {
                point.anchoredPosition = new Vector2(offset * HalfWidth, point.anchoredPosition.y);
            }
        }
    }
}
