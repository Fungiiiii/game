using System;
using System.Collections.Generic;
using Fungiiiii.Capture;
using Fungiiiii.Companions;
using Fungiiiii.Inventory;
using Fungiiiii.Runtime.Player;
using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The name of the creature the player looks at, on a small wooden panel above it,
    /// readable from afar and not only up close: the player learns what a Spongeus is before
    /// reaching it. Nothing shows through a trunk, nor while a menu or the opening shot
    /// takes the view.
    /// </summary>
    internal sealed class CreatureNameTag : MonoBehaviour
    {
        [Tooltip("The small wooden panel the name is written on.")]
        [SerializeField]
        private KeyPrompt tag;

        [Tooltip("Furthest from the player's eyes a name is read, in metres.")]
        [SerializeField, Min(1f)]
        private float readableWithin = 15f;

        [Tooltip("How far off the middle of the view a creature may be and still be looked at, in degrees.")]
        [SerializeField, Range(1f, 30f)]
        private float lookedAtWithinDegrees = 6f;

        [Tooltip("Gap between the top of the creature and its name, in metres.")]
        [SerializeField, Min(0f)]
        private float aboveTop = 0.25f;

        [Tooltip("Further up for the companion, whose key shows just above it, in metres.")]
        [SerializeField, Min(0f)]
        private float aboveCompanionKey = 0.35f;

        private readonly Dictionary<Transform, float> heights = new Dictionary<Transform, float>();
        private FirstPersonCamera look;
        private Camera view;
        private IReadOnlyList<WildMushroom> wild;
        private Func<WildMushroom, ItemDefinition> caughtItem;
        private CompanionKeeper companions;

        /// <summary>Whether a name is shown.</summary>
        public bool IsShown => tag.IsTextVisible;

        /// <summary>The name shown, an entry of the UI string table; empty when none is.</summary>
        public string ShownKey => tag.TextKey;

        /// <summary>What is missing for it to work, or null when nothing is.</summary>
        public string FindProblem()
        {
            return tag == null ? $"{name} has no panel to write the name on." : null;
        }

        /// <param name="playerLook">The player's view: no name while it is taken by a menu or the opening shot.</param>
        /// <param name="mushrooms">The wild creatures.</param>
        /// <param name="itemFor">The item a wild creature becomes once caught, which carries its name.</param>
        /// <param name="keeper">The companion out of the inventory, if any.</param>
        public void Bind(FirstPersonCamera playerLook, IReadOnlyList<WildMushroom> mushrooms, Func<WildMushroom, ItemDefinition> itemFor, CompanionKeeper keeper)
        {
            look = playerLook;
            view = playerLook.Camera;
            wild = mushrooms;
            caughtItem = itemFor;
            companions = keeper;
        }

        private void LateUpdate()
        {
            if (look == null || !look.isActiveAndEnabled)
            {
                tag.Hide();
                return;
            }

            Transform best = null;
            string bestName = null;
            var bestAngle = lookedAtWithinDegrees;
            foreach (var mushroom in wild)
            {
                if (mushroom != null && mushroom.isActiveAndEnabled && mushroom.IsFree)
                {
                    Consider(mushroom.transform, caughtItem(mushroom).NameKey, ref best, ref bestName, ref bestAngle);
                }
            }

            var companion = companions != null ? companions.Active : null;
            if (companion != null)
            {
                Consider(companion.transform, companion.Definition.Creature.CaughtItem.NameKey, ref best, ref bestName, ref bestAngle);
            }

            if (best == null)
            {
                tag.Hide();
                return;
            }

            var point = TagPoint(best);
            if (companion != null && best == companion.transform)
            {
                // The companion's key shows just above it: its name goes over the key.
                point += Vector3.up * aboveCompanionKey;
            }

            var screenPoint = view.WorldToScreenPoint(point);
            if (screenPoint.z <= 0f)
            {
                tag.Hide();
                return;
            }

            tag.ShowText(screenPoint, bestName);
        }

        private void Consider(Transform creature, string nameKey, ref Transform best, ref string bestName, ref float bestAngle)
        {
            if (string.IsNullOrEmpty(nameKey))
            {
                return;
            }

            var eyes = view.transform.position;
            var middle = creature.position + Vector3.up * (HeightOf(creature) * 0.5f);
            var toMiddle = middle - eyes;
            var distance = toMiddle.magnitude;
            if (distance > readableWithin || distance < 0.01f)
            {
                return;
            }

            var angle = Vector3.Angle(view.transform.forward, toMiddle);
            if (angle > bestAngle || IsHidden(eyes, middle, creature))
            {
                return;
            }

            best = creature;
            bestName = nameKey;
            bestAngle = angle;
        }

        /// <summary>Whether a trunk, a rock or the ground stands between the eyes and the creature.</summary>
        private static bool IsHidden(Vector3 eyes, Vector3 point, Transform creature)
        {
            return Physics.Linecast(eyes, point, out var hit, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
                && !hit.transform.IsChildOf(creature);
        }

        private Vector3 TagPoint(Transform creature)
        {
            return creature.position + Vector3.up * (HeightOf(creature) + aboveTop);
        }

        /// <summary>Height of its model above its feet, worked out once.</summary>
        private float HeightOf(Transform creature)
        {
            if (heights.TryGetValue(creature, out var height))
            {
                return height;
            }

            height = 0.5f;
            var top = float.MinValue;
            foreach (var renderer in creature.GetComponentsInChildren<Renderer>())
            {
                // What drips from it is not its body.
                if (!(renderer is ParticleSystemRenderer))
                {
                    top = Mathf.Max(top, renderer.bounds.max.y);
                }
            }

            if (top > float.MinValue)
            {
                height = Mathf.Max(0.1f, top - creature.position.y);
            }

            heights[creature] = height;
            return height;
        }
    }
}
