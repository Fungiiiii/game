using System.Collections.Generic;
using Fungiiiii.Capture;
using Fungiiiii.Controls;
using Fungiiiii.Runtime.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Teaches squeezing a wild creature by hand: while the player, standing, looks at a full
    /// one a few metres away (the one that drips), the crouch key shows beside it with "Hold:
    /// sneak up". Once crouched behind it, the creature's own key says "Squeeze". Gone after
    /// the first squeeze: the player has learned it. Nothing shows while a menu or the
    /// opening shot takes the view.
    /// </summary>
    internal sealed class SneakHint : MonoBehaviour
    {
        [Tooltip("The crouch key and what holding it does.")]
        [SerializeField]
        private KeyPrompt prompt;

        [Tooltip("Closest the creature may be for the hint, in metres: closer, the player is about to be noticed.")]
        [SerializeField, Min(0f)]
        private float fromDistance = 4f;

        [Tooltip("Furthest the creature may be for the hint, in metres.")]
        [SerializeField, Min(1f)]
        private float toDistance = 10f;

        [Tooltip("How far off the middle of the view the creature may be, in degrees: the player looks at it.")]
        [SerializeField, Range(1f, 30f)]
        private float lookedAtWithinDegrees = 10f;

        [Tooltip("Right of the creature, on the screen, in canvas units: the key does not cover it.")]
        [SerializeField]
        private float besideCreature = 60f;

        private readonly List<string> keyPaths = new List<string>();
        private readonly Dictionary<Transform, float> heights = new Dictionary<Transform, float>();
        private FirstPersonCamera look;
        private IReadOnlyList<WildMushroom> wild;
        private PlayerMotor motor;
        private PlayerControls controls;
        private CreaturePopulation population;
        private InputDevice glyphDevice;
        private Glyph? keyGlyph;

        /// <summary>Whether the crouch key shows by a creature.</summary>
        public bool IsShown => prompt.IsKeyVisible;

        /// <summary>What holding the key does, an entry of the UI string table; empty when hidden.</summary>
        public string ActionKey => prompt.HoldActionKey;

        /// <summary>The key shown, as it reads; empty when hidden.</summary>
        public string KeyLabel => prompt.Label;

        /// <summary>Whether the player has squeezed a creature: the hint shows no more.</summary>
        public bool IsLearned { get; private set; }

        /// <summary>What is missing for it to work, or null when nothing is.</summary>
        public string FindProblem()
        {
            return prompt == null ? $"{name} has no key to show." : null;
        }

        /// <param name="playerLook">The player's view: the hint shows for the creature looked at.</param>
        /// <param name="creatures">The wild creatures, and who tells when one is squeezed.</param>
        /// <param name="playerMotor">Whether the player crouches: crouched, the hint has done its work.</param>
        /// <param name="playerControls">Draws the crouch control as on the device used last.</param>
        public void Bind(FirstPersonCamera playerLook, CreaturePopulation creatures, PlayerMotor playerMotor, PlayerControls playerControls)
        {
            look = playerLook;
            population = creatures;
            wild = creatures.Mushrooms;
            motor = playerMotor;
            controls = playerControls;
            population.Squeezed += OnSqueezed;
        }

        private void OnDestroy()
        {
            if (population != null)
            {
                population.Squeezed -= OnSqueezed;
            }
        }

        private void OnSqueezed()
        {
            IsLearned = true;
            prompt.Hide();
        }

        private void LateUpdate()
        {
            if (IsLearned || look == null || !look.isActiveAndEnabled || motor.IsCrouching)
            {
                prompt.Hide();
                return;
            }

            var view = look.Camera;
            var creature = LookedAtFull(view);
            if (creature == null)
            {
                prompt.Hide();
                return;
            }

            var screenPoint = view.WorldToScreenPoint(creature.position + Vector3.up * (HeightOf(creature) * 0.5f));
            if (screenPoint.z <= 0f)
            {
                prompt.Hide();
                return;
            }

            screenPoint.x += besideCreature * Screen.height / 1080f;
            prompt.Show(screenPoint, CrouchGlyph());
            prompt.SetMarks(false, false, 0f);
            prompt.SetActions(null, UiText.HintSneak);
        }

        /// <summary>The full creature looked at, within the distances, nearest the middle of the view; null when none is.</summary>
        private Transform LookedAtFull(Camera view)
        {
            Transform best = null;
            var bestAngle = lookedAtWithinDegrees;
            var eyes = view.transform.position;
            foreach (var mushroom in wild)
            {
                if (mushroom == null || !mushroom.isActiveAndEnabled || !mushroom.IsFree || !mushroom.IsUnaware || !mushroom.IsDripping)
                {
                    continue;
                }

                var creature = mushroom.transform;
                var middle = creature.position + Vector3.up * (HeightOf(creature) * 0.5f);
                var toMiddle = middle - eyes;
                var distance = toMiddle.magnitude;
                var angle = Vector3.Angle(view.transform.forward, toMiddle);
                if (distance < fromDistance || distance > toDistance || angle > bestAngle)
                {
                    continue;
                }

                if (Physics.Linecast(eyes, middle, out var hit, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && !hit.transform.IsChildOf(creature))
                {
                    continue;
                }

                best = creature;
                bestAngle = angle;
            }

            return best;
        }

        /// <summary>The crouch control as the key hints draw it, on the device used last; worked out again only when that device changes.</summary>
        private Glyph CrouchGlyph()
        {
            var device = controls.CurrentDevice;
            if (keyGlyph.HasValue && glyphDevice == device)
            {
                return keyGlyph.Value;
            }

            glyphDevice = device;
            keyPaths.Clear();
            HintInput.Of(controls.Player.Crouch).CollectPaths(controls.CurrentScheme.bindingGroup, keyPaths);
            keyGlyph = InputGlyphs.Describe(keyPaths, device is DualShockGamepad);
            return keyGlyph.Value;
        }

        /// <summary>Height of its model above its feet, worked out once.</summary>
        private float HeightOf(Transform creature)
        {
            if (heights.TryGetValue(creature, out var height))
            {
                return height;
            }

            height = ModelHeight.Of(creature);
            heights[creature] = height;
            return height;
        }
    }
}
