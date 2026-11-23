using System;
using System.Collections.Generic;
using Fungiiiii.Combat;
using Fungiiiii.Companions;
using Fungiiiii.Runtime.World;
using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// A wild mushroom to catch with the net. It perceives the player through the capture
    /// state machine, flees on the ground plane, and can be held, captured and shrunk by
    /// the net. It shows its species' model, or shapes built from primitives on the net's
    /// bench.
    ///
    /// A net that misses it or fails to catch it leaves a threat at the impact point: the
    /// mushroom flees from the closer of the player and that point. Without it, a net
    /// thrown from beyond the calm distance would make the mushroom flee for one frame only.
    ///
    /// In a level: <see cref="InitializeWithModel"/> shows the species' model, <see cref="EnableWander"/> makes it
    /// stroll around its spawn while unaware, <see cref="FollowGround"/> keeps it on
    /// uneven ground, <see cref="EnableSoak"/> sends it to drink at the water ferns while
    /// calm (<see cref="WildSoak"/>), and <see cref="EnableHealth"/> lets the slingshot fell it.
    /// </summary>
    internal sealed class WildMushroom : MonoBehaviour
    {
        private static readonly Color IdleColor = new Color(0.45f, 0.45f, 0.55f);
        private static readonly Color AlertColor = new Color(1f, 0.75f, 0.1f);
        private static readonly Color FleeColor = new Color(0.95f, 0.12f, 0.08f);
        private static readonly Color CapturedColor = new Color(0.15f, 0.85f, 0.35f);

        /// <summary>How far the colours move towards white while the net holds the mushroom.</summary>
        private const float HighlightWhiteness = 0.55f;

        /// <summary>Height of its middle above its feet, where the player looks at it, in metres.</summary>
        private const float SeenHeight = 0.3f;

        /// <summary>A model is brightened, not whitened: its colours come from a palette texture.</summary>
        private static readonly Color ModelHighlightTint = new Color(1.6f, 1.6f, 1.6f, 1f);

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private Renderer[] modelRenderers;
        private MaterialPropertyBlock highlightBlock;
        private MushroomWander wander;
        private System.Func<Vector3, float> groundHeight;
        private Quaternion spawnRotation = Quaternion.identity;

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
        private bool isFelled;
        private WildSoak soak;
        private IReadOnlyList<IAbsorbable> soakSources;
        private float soakSpeed;
        private float soakWatchedWithin;
        private float soakSeenWithinDegrees;
        private Transform watcher;
        private Func<bool> playerCrouching;
        private float seesWithinDegrees = 180f;
        private WetLook wetLook;
        private CreatureHealth health;

        /// <summary>Raised once, when the slingshot fells it.</summary>
        public event Action<WildMushroom> Felled;

        public NetTargetProfile Profile { get; private set; }

        /// <summary>The model shown as its body, or null while it is drawn with primitives.</summary>
        public GameObject Model { get; private set; }

        public MushroomCaptureState State => stateMachine.CurrentState;

        /// <summary>True while the net may target this mushroom: present and not captured.</summary>
        public bool IsAvailable => isActiveAndEnabled && !isCaught && !isFelled && stateMachine.CurrentState != MushroomCaptureState.Captured;

        /// <summary>Neither felled, caught nor held in a net: it may be hidden while the player is far, and shown again.</summary>
        public bool IsFree => !isCaught && !isFelled && !isHeld;

        /// <summary>It drank at a water fern: felled, it lets go of its water.</summary>
        public bool IsWet => soak != null && soak.IsWet;

        /// <summary>Soaked through, drops fall from it.</summary>
        public bool IsDripping => wetLook != null && wetLook.IsDripping;

        /// <summary>The player, crouched, is behind it, out of what it sees: it does not notice them.</summary>
        public bool IsSneakedUpOn
        {
            get
            {
                if (playerCrouching == null || player == null || !playerCrouching())
                {
                    return false;
                }

                var toPlayer = player.position - transform.position;
                toPlayer.y = 0f;
                var facing = transform.forward;
                facing.y = 0f;
                return toPlayer.sqrMagnitude > 0.0001f && Vector3.Angle(facing, toPlayer) > seesWithinDegrees;
            }
        }

        public bool IsFelled => isFelled;

        /// <summary>What it soaks up, and whether it is busy doing so; null when it does not.</summary>
        public WildSoak Soak => soak;

        /// <summary>Null when the slingshot cannot hit it.</summary>
        public CreatureHealth Health => health;

        /// <summary>True if the mushroom has not noticed the player.</summary>
        public bool IsUnaware => stateMachine.CurrentState == MushroomCaptureState.Idle;

        public bool IsHeld => isHeld;

        /// <summary>
        /// Sets the mushroom up. Called once, right after AddComponent.
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
            capColor = profileCapColor;
            Setup(profile, observedPlayer, arenaBounds, mushroomFleeSpeed, size);
            BuildVisuals(profile == NetTargetProfile.CommonWeakened);
            ApplyColors();
        }

        /// <summary>
        /// Sets the mushroom up with an existing model as its body, at scale 1, instead of
        /// primitives. A capsule fitted to the model takes the net.
        /// </summary>
        public void InitializeWithModel(
            NetTargetProfile profile,
            Transform observedPlayer,
            Rect arenaBounds,
            float mushroomFleeSpeed,
            GameObject modelPrefab)
        {
            if (modelPrefab == null)
            {
                throw new ArgumentNullException(nameof(modelPrefab));
            }

            Setup(profile, observedPlayer, arenaBounds, mushroomFleeSpeed, 1f);
            BuildModelVisuals(modelPrefab);
            ApplyColors();
        }

        /// <summary>Makes the mushroom stroll while it is unaware, instead of standing still.</summary>
        public void EnableWander(MushroomWander stroll)
        {
            wander = stroll;
        }

        /// <param name="sources">What it may soak up.</param>
        /// <param name="speed">How fast it walks to a source, in metres per second.</param>
        /// <param name="look">The part of its model that shows it wet; null when it shows nothing.</param>
        /// <param name="view">The player's view: it starts to soak up only when seen.</param>
        /// <param name="watchedWithin">It soaks up only while the player is this close, in metres: what nobody sees does not happen.</param>
        /// <param name="seenWithinDegrees">It starts to soak up only within this angle of the middle of the view.</param>
        /// <summary>A crouched player behind it, out of what it sees, goes unnoticed.</summary>
        /// <param name="crouching">Whether the player crouches.</param>
        /// <param name="seesWithin">It notices the player within this angle of where it faces, in degrees.</param>
        public void EnableSneaking(Func<bool> crouching, float seesWithin)
        {
            playerCrouching = crouching ?? throw new ArgumentNullException(nameof(crouching));
            seesWithinDegrees = seesWithin;
        }

        /// <summary>Squeezed by hand: it gives up what it soaked up, and runs from <paramref name="by"/>. False when it was dry.</summary>
        public bool Squeeze(Vector3 by)
        {
            if (soak == null || !soak.Squeeze())
            {
                return false;
            }

            Scare(by);
            return true;
        }

        public void EnableSoak(WildSoak soaking, IReadOnlyList<IAbsorbable> sources, float speed, WetLook look, Transform view, float watchedWithin, float seenWithinDegrees)
        {
            watcher = view != null ? view : throw new ArgumentNullException(nameof(view));
            soakSeenWithinDegrees = seenWithinDegrees;
            soak = soaking ?? throw new ArgumentNullException(nameof(soaking));
            soakSources = sources ?? throw new ArgumentNullException(nameof(sources));
            soakSpeed = speed;
            soakWatchedWithin = watchedWithin;
            wetLook = look;
            wetLook?.Show(0f);
        }

        public void EnableHealth(CreatureHealth creatureHealth)
        {
            health = creatureHealth ?? throw new ArgumentNullException(nameof(creatureHealth));
        }

        /// <summary>
        /// A hit, from the slingshot: it runs from where the shot came from, and falls once
        /// its health runs out.
        /// </summary>
        /// <returns>False when it cannot be hit: no health, caught, or already felled.</returns>
        public bool Hit(float damage, Vector3 from)
        {
            if (health == null || !IsAvailable)
            {
                return false;
            }

            Scare(from);
            if (health.Damage(damage))
            {
                isFelled = true;
                SetCollidersEnabled(false);
                gameObject.SetActive(false);
                Felled?.Invoke(this);
            }

            return true;
        }

        /// <summary>Keeps the mushroom on the ground: <paramref name="heightAt"/> gives the ground height under a point.</summary>
        public void FollowGround(System.Func<Vector3, float> heightAt)
        {
            groundHeight = heightAt;
            transform.position = OnGround(transform.position);
            spawnPosition = transform.position;
        }

        private void Setup(
            NetTargetProfile profile,
            Transform observedPlayer,
            Rect arenaBounds,
            float mushroomFleeSpeed,
            float size)
        {
            Profile = profile;
            player = observedPlayer;
            arena = arenaBounds;
            fleeSpeed = mushroomFleeSpeed;
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
            baseScale = Vector3.one * size;
            transform.localScale = baseScale;

            stateMachine = new MushroomCaptureStateMachine(MushroomCaptureStateMachine.Configuration.Default);
            stateMachine.StateChanged += HandleStateChanged;
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

        /// <summary>Back to its spawn point, unaware, for a reset.</summary>
        public void ResetToSpawn()
        {
            isHeld = false;
            isCaught = false;
            isFelled = false;
            threatPoint = null;
            transform.position = spawnPosition;
            transform.rotation = spawnRotation;
            transform.localScale = baseScale;
            gameObject.SetActive(true);
            SetCollidersEnabled(true);
            stateMachine.Reset();
            wander?.Restart();
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
                soak?.Interrupt();
                FleeFrom(threat);
            }
            else if (stateMachine.CurrentState == MushroomCaptureState.Idle)
            {
                // Calmed down: the net it fled from is forgotten.
                threatPoint = null;

                // It drinks under the player's eyes, not at the other end of the forest: the
                // player learns that a Champonge fills up by seeing one do it. It sets off only
                // while looked at, then carries on while the player stays close.
                var near = NetImpact.HorizontalDistance(transform.position, player.position) <= soakWatchedWithin;
                if (!near)
                {
                    soak?.Interrupt();
                }

                var drinks = soak != null && near && (soak.IsBusy || IsSeen());
                var soakStep = drinks ? soak.Step(transform.position, soakSources, soakSpeed, Time.deltaTime) : null;
                if (soakStep.HasValue)
                {
                    MoveTo(soakStep.Value);
                }
                else if (wander != null)
                {
                    MoveTo(wander.Step(transform.position, Time.deltaTime));
                }
            }

            if (soak != null)
            {
                wetLook?.Show(soak.Wetness);
            }
        }

        /// <summary>Whether it stands near the middle of the player's view.</summary>
        private bool IsSeen()
        {
            var toModel = transform.position + Vector3.up * SeenHeight - watcher.position;
            return Vector3.Angle(watcher.forward, toModel) <= soakSeenWithinDegrees;
        }

        private Vector3 ClosestThreat(out float distance)
        {
            var threat = player.position;
            // A crouched player behind it is not noticed, however close.
            distance = IsSneakedUpOn ? float.MaxValue : NetImpact.HorizontalDistance(transform.position, threat);

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

            MoveTo(transform.position + away.normalized * (fleeSpeed * Time.deltaTime));
        }

        private void MoveTo(Vector3 position)
        {
            position.x = Mathf.Clamp(position.x, arena.xMin, arena.xMax);
            position.z = Mathf.Clamp(position.z, arena.yMin, arena.yMax);
            position = OnGround(position);

            // A model shows which way it goes; the round primitive body does not need to.
            var step = position - transform.position;
            step.y = 0f;
            if (modelRenderers != null && step.sqrMagnitude > 0.000001f)
            {
                transform.rotation = Quaternion.LookRotation(step, Vector3.up);
            }

            transform.position = position;
        }

        private Vector3 OnGround(Vector3 position)
        {
            if (groundHeight != null)
            {
                position.y = groundHeight(position);
            }

            return position;
        }

        private void BuildVisuals(bool weakened)
        {
            capMaterial = Primitives.CreateMaterial("Wild Mushroom Cap", capColor);
            markerMaterial = Primitives.CreateMaterial("Wild Mushroom State Marker", IdleColor);
            stemMaterial = Primitives.CreateMaterial("Wild Mushroom Stem", new Color(0.88f, 0.8f, 0.66f));

            // Colliders are kept: a net thrown at a mushroom lands on it instead of passing through.
            var stem = Primitives.Create(
                PrimitiveType.Cylinder,
                "Stem",
                transform,
                new Vector3(0f, 0.3f, 0f),
                new Vector3(0.25f, 0.3f, 0.25f),
                stemMaterial,
                keepCollider: true);

            var cap = Primitives.Create(
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

            Primitives.Create(
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

        private void BuildModelVisuals(GameObject modelPrefab)
        {
            var model = Instantiate(modelPrefab, transform, false);
            model.name = "Model";
            Model = model;
            modelRenderers = model.GetComponentsInChildren<Renderer>();
            highlightBlock = new MaterialPropertyBlock();

            // The model's own colliders, if any, are replaced by one capsule fitted to it.
            foreach (var modelCollider in model.GetComponentsInChildren<Collider>())
            {
                Destroy(modelCollider);
            }

            var bounds = new Bounds(transform.position, Vector3.zero);
            foreach (var modelRenderer in modelRenderers)
            {
                bounds.Encapsulate(modelRenderer.bounds);
            }

            var body = gameObject.AddComponent<CapsuleCollider>();
            body.center = bounds.center - transform.position;
            body.radius = Mathf.Max(0.05f, Mathf.Max(bounds.extents.x, bounds.extents.z));
            body.height = Mathf.Max(body.radius * 2f, bounds.size.y);

            markerMaterial = Primitives.CreateMaterial("Wild Mushroom State Marker", IdleColor);
            var markerDiameter = body.radius * 2.6f;
            Primitives.Create(
                PrimitiveType.Cylinder,
                "State Marker",
                transform,
                new Vector3(0f, 0.02f, 0f),
                new Vector3(markerDiameter, 0.01f, markerDiameter),
                markerMaterial,
                keepCollider: false);

            colliders = new Collider[] { body };
        }

        private void ApplyColors()
        {
            if (markerMaterial == null)
            {
                return;
            }

            var stateColor = GetStateColor(stateMachine.CurrentState);
            markerMaterial.color = isHeld ? Highlight(stateColor) : stateColor;

            if (capMaterial != null)
            {
                capMaterial.color = isHeld ? Highlight(capColor) : capColor;
            }

            if (modelRenderers != null)
            {
                highlightBlock.Clear();
                if (isHeld)
                {
                    highlightBlock.SetColor(BaseColorId, ModelHighlightTint);
                }

                foreach (var modelRenderer in modelRenderers)
                {
                    modelRenderer.SetPropertyBlock(isHeld ? highlightBlock : null);
                }
            }
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
