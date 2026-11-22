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
    /// while the view narrows on it. Then the view goes back to the player, who takes the
    /// controls. Any button skips it. The interface stays hidden meanwhile.
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

        [Tooltip("Field of view at the end of the shot, narrowed on the seed, in degrees.")]
        [SerializeField, Range(10f, 60f)]
        private float endFieldOfView = 30f;

        [Tooltip("The interface hidden during the shot.")]
        [SerializeField]
        private Canvas[] hud = new Canvas[0];

        private bool[] hudShown;
        private Camera view;
        private FirstPersonCamera look;
        private PlayerControls controls;
        private FallIn fall;
        private float baseFieldOfView;
        private IDisposable skipListener;
        private Coroutine shot;

        /// <summary>Whether the shot is on: the player has no control.</summary>
        public bool IsPlaying { get; private set; }

        /// <summary>Raised once the view is back with the player, the shot played or skipped.</summary>
        public event Action Finished;

        /// <summary>What is missing for the shot, or null when nothing is.</summary>
        public string FindProblem()
        {
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

            fall = seed.gameObject.AddComponent<FallIn>();
            skipListener = InputSystem.onAnyButtonPress.CallOnce(_ => Skip());
            shot = StartCoroutine(Shoot(seed, landing));
        }

        /// <summary>Ends the shot at once: the seed lies on the ground, the player has the view and the controls.</summary>
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

            End();
        }

        private void End()
        {
            IsPlaying = false;
            shot = null;
            skipListener?.Dispose();
            skipListener = null;
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
