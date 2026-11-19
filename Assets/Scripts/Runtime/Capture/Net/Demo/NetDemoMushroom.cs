using System;
using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// Placeholder mushroom of the net prototype, built from primitives. It perceives the
    /// player through the existing capture state machine, flees on the ground plane, and
    /// can be held, captured and shrunk by the net.
    ///
    /// A net that misses it or fails to catch it leaves a threat at the impact point: the
    /// mushroom flees from the closer of the player and that point. Without it, a net
    /// thrown from beyond the calm distance would make the mushroom flee for one frame only.
    /// </summary>
    internal sealed class NetDemoMushroom : MonoBehaviour
    {
        private static readonly Color IdleColor = new Color(0.45f, 0.45f, 0.55f);
        private static readonly Color AlertColor = new Color(1f, 0.75f, 0.1f);
        private static readonly Color FleeColor = new Color(0.95f, 0.12f, 0.08f);
        private static readonly Color CapturedColor = new Color(0.15f, 0.85f, 0.35f);

        /// <summary>How far the colours move towards white while the net holds the mushroom.</summary>
        private const float HighlightWhiteness = 0.55f;

        private MushroomCaptureStateMachine stateMachine;
        private Transform player;
        private Rect arena;
        private float fleeSpeed;
        private Vector3 spawnPosition;
        private Vector3 baseScale;
        private Color capColor;
        private Material capMaterial;
        private Material markerMaterial;
        private Material stemMaterial;
        private Collider[] colliders;
        private Vector3? threatPoint;
        private bool isHeld;
        private bool isCaught;

        public NetTargetProfile Profile { get; private set; }

        public MushroomCaptureState State => stateMachine.CurrentState;

        /// <summary>True while the net may target this mushroom: present and not captured.</summary>
        public bool IsAvailable => !isCaught && stateMachine.CurrentState != MushroomCaptureState.Captured;

        /// <summary>True if the mushroom has not noticed the player.</summary>
        public bool IsUnaware => stateMachine.CurrentState == MushroomCaptureState.Idle;

        public bool IsHeld => isHeld;

        /// <summary>
        /// Sets the mushroom up. Called once by the demo right after AddComponent.
        /// </summary>
        /// <param name="arenaBounds">Ground rectangle (x, z) the mushroom cannot flee out of.</param>
        public void Initialize(
            NetTargetProfile profile,
            Transform observedPlayer,
            Rect arenaBounds,
            float mushroomFleeSpeed,
            float size,
            Color profileCapColor)
        {
            Profile = profile;
            player = observedPlayer;
            arena = arenaBounds;
            fleeSpeed = mushroomFleeSpeed;
            spawnPosition = transform.position;
            baseScale = Vector3.one * size;
            capColor = profileCapColor;
            transform.localScale = baseScale;

            stateMachine = new MushroomCaptureStateMachine(MushroomCaptureStateMachine.Configuration.Default);
            stateMachine.StateChanged += HandleStateChanged;

            BuildVisuals(profile == NetTargetProfile.CommonWeakened);
            ApplyColors();
        }

        /// <summary>Freezes or releases the mushroom while the net is on it, and highlights it.</summary>
        public void SetHeld(bool held)
        {
            isHeld = held;
            ApplyColors();
        }

        /// <summary>
        /// Applies the revealed capture outcome: captured and carried away by the net on
        /// success, fleeing from <paramref name="netPoint"/> on failure.
        /// </summary>
        public void ResolveCaptureAttempt(bool succeeded, Vector3 netPoint)
        {
            isHeld = false;

            if (!succeeded)
            {
                threatPoint = netPoint;
            }

            stateMachine.ResolveCaptureAttempt(succeeded);
            isCaught = succeeded;

            // A caught mushroom flies back inside the net: it must not bump into the player.
            SetCollidersEnabled(!succeeded);
            ApplyColors();
        }

        /// <summary>Makes the mushroom flee from a net that landed close by without catching it.</summary>
        public void Scare(Vector3 netPoint)
        {
            // A net landing next to it is, for the mushroom, a capture attempt that failed.
            threatPoint = netPoint;
            stateMachine.ResolveCaptureAttempt(false);
        }

        /// <summary>
        /// Moves the caught mushroom with the returning net and shrinks it:
        /// 0 is full size, 1 has vanished.
        /// </summary>
        public void CarryInNet(Vector3 netPosition, float shrinkProgress)
        {
            var remaining = 1f - Mathf.Clamp01(shrinkProgress);
            transform.position = netPosition;
            transform.localScale = baseScale * remaining;

            if (remaining <= 0f)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>Back to its spawn point, unaware, for a demo reset.</summary>
        public void ResetToSpawn()
        {
            isHeld = false;
            isCaught = false;
            threatPoint = null;
            transform.position = spawnPosition;
            transform.rotation = Quaternion.identity;
            transform.localScale = baseScale;
            gameObject.SetActive(true);
            SetCollidersEnabled(true);
            stateMachine.Reset();
            ApplyColors();
        }

        private void Update()
        {
            if (stateMachine == null || isHeld || isCaught || player == null)
            {
                return;
            }

            var threat = ClosestThreat(out var distance);
            stateMachine.Tick(distance, Time.deltaTime);

            if (stateMachine.CurrentState == MushroomCaptureState.Flee)
            {
                FleeFrom(threat);
            }
            else if (stateMachine.CurrentState == MushroomCaptureState.Idle)
            {
                // Calmed down: the net it fled from is forgotten.
                threatPoint = null;
            }
        }

        private Vector3 ClosestThreat(out float distance)
        {
            var threat = player.position;
            distance = NetImpact.HorizontalDistance(transform.position, threat);

            if (threatPoint.HasValue)
            {
                var netDistance = NetImpact.HorizontalDistance(transform.position, threatPoint.Value);
                if (netDistance < distance)
                {
                    threat = threatPoint.Value;
                    distance = netDistance;
                }
            }

            return threat;
        }

        private void OnDestroy()
        {
            if (stateMachine != null)
            {
                stateMachine.StateChanged -= HandleStateChanged;
            }

            if (capMaterial != null)
            {
                Destroy(capMaterial);
            }

            if (markerMaterial != null)
            {
                Destroy(markerMaterial);
            }

            if (stemMaterial != null)
            {
                Destroy(stemMaterial);
            }
        }

        private void HandleStateChanged(MushroomCaptureState previous, MushroomCaptureState next)
        {
            ApplyColors();
        }

        private void SetCollidersEnabled(bool enabledState)
        {
            foreach (var part in colliders)
            {
                part.enabled = enabledState;
            }
        }

        private void FleeFrom(Vector3 threat)
        {
            var away = transform.position - threat;
            away.y = 0f;

            if (away.sqrMagnitude < 0.001f)
            {
                away = Vector3.right;
            }

            var position = transform.position + away.normalized * (fleeSpeed * Time.deltaTime);
            position.x = Mathf.Clamp(position.x, arena.xMin, arena.xMax);
            position.z = Mathf.Clamp(position.z, arena.yMin, arena.yMax);
            transform.position = position;
        }

        private void BuildVisuals(bool weakened)
        {
            capMaterial = NetDemoPrimitives.CreateMaterial("Net Demo Mushroom Cap", capColor);
            markerMaterial = NetDemoPrimitives.CreateMaterial("Net Demo Mushroom State Marker", IdleColor);
            stemMaterial = NetDemoPrimitives.CreateMaterial("Net Demo Mushroom Stem", new Color(0.88f, 0.8f, 0.66f));

            // Colliders are kept: a net thrown at a mushroom lands on it instead of passing through.
            var stem = NetDemoPrimitives.Create(
                PrimitiveType.Cylinder,
                "Stem",
                transform,
                new Vector3(0f, 0.3f, 0f),
                new Vector3(0.25f, 0.3f, 0.25f),
                stemMaterial,
                keepCollider: true);

            var cap = NetDemoPrimitives.Create(
                PrimitiveType.Sphere,
                "Cap",
                transform,
                new Vector3(0f, 0.65f, 0f),
                new Vector3(0.8f, 0.35f, 0.8f),
                capMaterial,
                keepCollider: true);

            if (weakened)
            {
                // Placeholder cue for a weakened mushroom: a drooping cap.
                cap.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
                cap.transform.localPosition = new Vector3(0.08f, 0.58f, 0f);
            }

            NetDemoPrimitives.Create(
                PrimitiveType.Cylinder,
                "State Marker",
                transform,
                new Vector3(0f, 0.02f, 0f),
                new Vector3(1.1f, 0.01f, 1.1f),
                markerMaterial,
                keepCollider: false);

            // Collected by hand: the marker's collider is destroyed only at the end of the frame.
            colliders = new[] { stem.GetComponent<Collider>(), cap.GetComponent<Collider>() };
        }

        private void ApplyColors()
        {
            if (capMaterial == null || markerMaterial == null)
            {
                return;
            }

            var stateColor = GetStateColor(stateMachine.CurrentState);
            capMaterial.color = isHeld ? Highlight(capColor) : capColor;
            markerMaterial.color = isHeld ? Highlight(stateColor) : stateColor;
        }

        private static Color Highlight(Color color)
        {
            return Color.Lerp(color, Color.white, HighlightWhiteness);
        }

        private static Color GetStateColor(MushroomCaptureState state)
        {
            switch (state)
            {
                case MushroomCaptureState.Idle:
                    return IdleColor;
                case MushroomCaptureState.Alert:
                    return AlertColor;
                case MushroomCaptureState.Flee:
                    return FleeColor;
                case MushroomCaptureState.Captured:
                    return CapturedColor;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}
