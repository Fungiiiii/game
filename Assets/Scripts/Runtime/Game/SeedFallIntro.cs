using System;
using System.Collections;
using Fungiiiii.Controls;
using Fungiiiii.Runtime.Player;
using Fungiiiii.Runtime.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Fungiiiii.Game
{
    /// <summary>
    /// The opening shot, when a game starts: the player's camera stands before the giant
    /// tree, looks up at its leaves, and follows the seed falling from them to the ground
    /// while the view narrows on it. Then the view draws back from the seed to the player's
    /// eyes, still looking at it, so that the player sees it lies right before them, and
    /// they take the controls. Two presses of any button skip it: the first one only says so, so that a
    /// player who pressed by mistake does not lose the shot. The interface stays hidden
    /// meanwhile.
    /// </summary>
    internal sealed class SeedFallIntro : MonoBehaviour
    {
        [Tooltip("How high above the ground the seed falls from: the giant tree's leaves, in metres.")]
        [SerializeField, Min(1f)]
        private float fallHeight = 18f;

        [Tooltip("Ground distance from the seed to the camera, on the side the player comes from, in metres.")]
        [SerializeField, Min(1f)]
        private float cameraDistance = 7f;

        [Tooltip("Height of the camera above the ground, in metres.")]
        [SerializeField, Min(0.2f)]
        private float cameraHeight = 1.6f;

        [Tooltip("Seconds looking up at the leaves before the seed falls.")]
        [SerializeField, Min(0f)]
        private float secondsBeforeFall = 0.6f;

        [Tooltip("Seconds on the seed once it lies on the ground.")]
        [SerializeField, Min(0f)]
        private float secondsAfterLanding = 1.2f;

        [Tooltip("Seconds for the view to draw back from the seed to the player's eyes.")]
        [SerializeField, Min(0f)]
        private float secondsBackToPlayer = 1.6f;

        [Tooltip("Field of view at the end of the shot, narrowed on the seed, in degrees.")]
        [SerializeField, Range(10f, 60f)]
        private float endFieldOfView = 30f;

        [Tooltip("The interface hidden during the shot.")]
        [SerializeField]
        private Canvas[] hud = new Canvas[0];

        [Tooltip("Shown after a first press: a second one skips the shot.")]
        [SerializeField]
        private GameObject skipHint;

        [Tooltip("Seconds the second press has to come, after the first one.")]
        [SerializeField, Min(0.5f)]
        private float secondsToPressAgain = 3f;

        private bool[] hudShown;
        private Camera view;
        private FirstPersonCamera look;
        private PlayerControls controls;
        private FallIn fall;
        private float baseFieldOfView;
        private IDisposable skipListener;
        private Coroutine shot;
        private float hintLeft;

        /// <summary>Whether the shot is on: the player has no control.</summary>
        public bool IsPlaying { get; private set; }

        /// <summary>Whether a first press asked to skip: a second one does.</summary>
        public bool IsAskingToSkip => skipHint.activeSelf;

        /// <summary>Raised once the view is back with the player, the shot played or skipped.</summary>
        public event Action Finished;

        /// <summary>What is missing for the shot, or null when nothing is.</summary>
        public string FindProblem()
        {
            if (skipHint == null)
            {
                return $"{name} lacks the hint shown after a first press.";
            }

            foreach (var canvas in hud)
            {
                if (canvas == null)
                {
                    return $"{name} has an empty line in its interface to hide.";
                }
            }

            return null;
        }

        /// <param name="playerView">The player's camera, borrowed for the shot.</param>
        /// <param name="playerLook">What moves that camera with the player; off during the shot.</param>
        /// <param name="playerControls">Off during the shot.</param>
        /// <param name="seed">The seed, lying where it lands.</param>
        /// <param name="playerFrom">Where the player starts: the camera stands on that side of the seed.</param>
        /// <param name="groundHeight">Height of the ground under a point.</param>
        public void Play(Camera playerView, FirstPersonCamera playerLook, PlayerControls playerControls, Transform seed, Vector3 playerFrom, Func<Vector3, float> groundHeight)
        {
            view = playerView;
            look = playerLook;
            controls = playerControls;
            baseFieldOfView = view.fieldOfView;

            IsPlaying = true;
            look.enabled = false;
            controls.enabled = false;
            hudShown = new bool[hud.Length];
            for (var index = 0; index < hud.Length; index++)
            {
                hudShown[index] = hud[index].enabled;
                hud[index].enabled = false;
            }

            var landing = seed.position;
            var side = playerFrom - landing;
            side.y = 0f;
            side = side.sqrMagnitude > 0.01f ? side.normalized : Vector3.back;
            var standing = landing + side * cameraDistance;
            standing.y = groundHeight(standing) + cameraHeight;
            view.transform.position = standing;

            // The player will look at the seed when the view comes back to them.
            look.LookAt(landing);

            fall = seed.gameObject.AddComponent<FallIn>();
            skipHint.SetActive(false);
            skipListener = InputSystem.onAnyButtonPress.Call(_ => OnButtonPressed());
            shot = StartCoroutine(Shoot(seed, landing));
        }

        /// <summary>Ends the shot at once: the seed lies on the ground, the player has the view and the controls. A second press calls it; so do the tests.</summary>
        public void Skip()
        {
            if (!IsPlaying)
            {
                return;
            }

            if (shot != null)
            {
                StopCoroutine(shot);
            }

            fall.Finish();
            End();
        }

        /// <summary>A first press shows the hint; a second one while it shows skips the shot.</summary>
        private void OnButtonPressed()
        {
            if (!IsPlaying)
            {
                return;
            }

            if (skipHint.activeSelf)
            {
                Skip();
                return;
            }

            skipHint.SetActive(true);
            hintLeft = secondsToPressAgain;
        }

        private void Update()
        {
            if (!skipHint.activeSelf)
            {
                return;
            }

            hintLeft -= Time.unscaledDeltaTime;
            if (hintLeft <= 0f)
            {
                skipHint.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            skipListener?.Dispose();
        }

        private IEnumerator Shoot(Transform seed, Vector3 landing)
        {
            var top = landing + Vector3.up * fallHeight;
            var total = secondsBeforeFall + ArrivalCurves.FallSeconds(fallHeight) + secondsAfterLanding;
            var elapsed = 0f;
            var falling = false;
            while (elapsed < total)
            {
                if (!falling && elapsed >= secondsBeforeFall)
                {
                    falling = true;
                    fall.Play(fallHeight, 0.37f);
                }

                var target = falling ? seed.position : top;
                view.transform.rotation = Quaternion.LookRotation(target - view.transform.position);
                view.fieldOfView = Mathf.Lerp(baseFieldOfView, endFieldOfView, Mathf.SmoothStep(0f, 1f, elapsed / total));
                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return BackToPlayer();
            End();
        }

        /// <summary>From the seed to the player's eyes, the view widening back, the seed in sight.</summary>
        private IEnumerator BackToPlayer()
        {
            var fromPosition = view.transform.position;
            var fromRotation = view.transform.rotation;
            var fromFieldOfView = view.fieldOfView;
            var elapsed = 0f;
            while (elapsed < secondsBackToPlayer)
            {
                var progress = Mathf.SmoothStep(0f, 1f, elapsed / secondsBackToPlayer);
                var to = look.ViewPose;
                view.transform.SetPositionAndRotation(
                    Vector3.Lerp(fromPosition, to.position, progress),
                    Quaternion.Slerp(fromRotation, to.rotation, progress));
                view.fieldOfView = Mathf.Lerp(fromFieldOfView, baseFieldOfView, progress);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void End()
        {
            IsPlaying = false;
            shot = null;
            skipListener?.Dispose();
            skipListener = null;
            skipHint.SetActive(false);
            view.fieldOfView = baseFieldOfView;
            look.enabled = true;
            controls.enabled = true;
            for (var index = 0; index < hud.Length; index++)
            {
                hud[index].enabled = hudShown[index];
            }

            Finished?.Invoke();
        }
    }
}
