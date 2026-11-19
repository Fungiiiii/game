using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fungiiiii.Capture
{
    /// <summary>Tuning of the capture net, set by the scene that owns it.</summary>
    internal readonly struct NetThrowSettings
    {
        public NetThrowSettings(
            float launchSpeed,
            float gravity,
            float impactRadius,
            float scareRadius,
            float holdSeconds,
            float returnSeconds,
            NetCaptureRates rates)
        {
            LaunchSpeed = launchSpeed;
            Gravity = gravity;
            ImpactRadius = impactRadius;
            ScareRadius = scareRadius;
            HoldSeconds = holdSeconds;
            ReturnSeconds = returnSeconds;
            Rates = rates;
        }

        public float LaunchSpeed { get; }
        public float Gravity { get; }
        public float ImpactRadius { get; }
        public float ScareRadius { get; }
        public float HoldSeconds { get; }
        public float ReturnSeconds { get; }
        public NetCaptureRates Rates { get; }
    }

    /// <summary>
    /// Throws the capture net from the first-person view, previews its arc and impact
    /// zone, and plays the hold, the capture and the return with placeholders.
    ///
    /// The rules live in plain C# (<see cref="NetCycle"/>, <see cref="NetLandingResolver"/>,
    /// <see cref="NetTrajectory"/>); this component only feeds them input, physics casts
    /// and mushroom positions, and shows the result. Subscribe to <see cref="Cycle"/> for
    /// the Thrown, Landed, CaptureSucceeded and CaptureFailed events.
    ///
    /// ThrowNet is built in code, like Dodge in PlayerInputReader, and must be added to the
    /// shared InputSystem_Actions asset before production.
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class NetThrower : MonoBehaviour
    {
        private const int PreviewDotCount = 30;
        private const float PreviewTimeStep = 0.04f;
        private const float MaxFlightSeconds = 3f;
        private const int CastBufferSize = 8;

        /// <summary>Net placeholder: a flat disc this wide in flight and on the ground.</summary>
        private const float NetDiameter = 1.2f;
        private const float NetThickness = 0.04f;
        private const float ClosedNetSize = 0.45f;

        /// <summary>Share of the return during which a net with a catch closes up.</summary>
        private const float ClosingShareOfReturn = 0.25f;

        private static readonly Vector3 ThrowOriginOffset = new Vector3(0.15f, -0.15f, 0.5f);
        private static readonly Vector3 HandNetLocalPosition = new Vector3(0.3f, -0.28f, 0.6f);

        private static readonly Color NetColor = new Color(0.82f, 0.74f, 0.52f);
        private static readonly Color PreviewDotColor = new Color(1f, 1f, 1f);
        private static readonly Color NoTargetColor = new Color(0.85f, 0.85f, 0.85f);
        private static readonly Color LowChanceColor = new Color(0.9f, 0.15f, 0.1f);
        private static readonly Color MediumChanceColor = new Color(1f, 0.55f, 0.05f);
        private static readonly Color HighChanceColor = new Color(0.2f, 0.85f, 0.25f);

        private readonly List<NetCandidate> candidates = new List<NetCandidate>();
        private readonly List<NetDemoMushroom> candidateOwners = new List<NetDemoMushroom>();
        private readonly List<int> scaredIndices = new List<int>();
        private readonly RaycastHit[] castHits = new RaycastHit[CastBufferSize];

        private NetThrowSettings settings;
        private Camera aimCamera;
        private Collider ownCollider;
        private IReadOnlyList<NetDemoMushroom> mushrooms;
        private NetLandingResolver resolver;
        private NetSegmentCast segmentCast;
        private InputAction throwAction;

        private NetTrajectory flight;
        private float flightSeconds;
        private Vector3 previousFlightPoint;
        private Vector3 landingPoint;
        private NetDemoMushroom heldTarget;
        private NetDemoMushroom caughtTarget;

        private Transform netVisual;
        private Transform handNet;
        private Transform impactCircle;
        private Transform[] previewDots;
        private Material netMaterial;
        private Material dotMaterial;
        private Material circleMaterial;

        public NetCycle Cycle { get; private set; }

        /// <summary>Where the net is now: in flight, on the ground or on its way back.</summary>
        public Vector3 NetPosition => netVisual != null ? netVisual.position : transform.position;

        public void Initialize(
            NetThrowSettings netSettings,
            Camera viewCamera,
            Collider playerCollider,
            IReadOnlyList<NetDemoMushroom> targets,
            System.Random random)
        {
            settings = netSettings;
            aimCamera = viewCamera;
            ownCollider = playerCollider;
            mushrooms = targets;
            resolver = new NetLandingResolver(settings.ImpactRadius, settings.ScareRadius, settings.Rates, random);
            segmentCast = CastSegment;

            Cycle = new NetCycle(settings.HoldSeconds, settings.ReturnSeconds);
            Cycle.CaptureSucceeded += HandleCaptureSucceeded;
            Cycle.CaptureFailed += HandleCaptureFailed;
            Cycle.Returned += HandleReturned;

            BuildVisuals();
            ShowPhase();
        }

        /// <summary>
        /// Throws the net from <paramref name="origin"/> along <paramref name="direction"/>.
        /// Called by the ThrowNet input, and by tests to throw without a device.
        /// </summary>
        /// <returns>False while the net is not back in hand.</returns>
        public bool TryThrow(Vector3 origin, Vector3 direction)
        {
            if (Cycle == null || !Cycle.TryThrow())
            {
                return false;
            }

            flight = NetTrajectory.Launch(origin, direction, settings.LaunchSpeed, settings.Gravity);
            flightSeconds = 0f;
            previousFlightPoint = origin;
            netVisual.position = origin;
            ShowPhase();
            return true;
        }

        /// <summary>Puts the net back in hand at once, for a demo reset.</summary>
        public void ResetNet()
        {
            heldTarget = null;
            caughtTarget = null;
            Cycle?.Reset();
            ShowPhase();
        }

        private void Awake()
        {
            throwAction = new InputAction("ThrowNet", InputActionType.Button);
            throwAction.AddBinding("<Mouse>/leftButton");
            throwAction.AddBinding("<Gamepad>/rightTrigger");
        }

        private void OnEnable()
        {
            throwAction?.Enable();
        }

        private void OnDisable()
        {
            throwAction?.Disable();
        }

        private void OnDestroy()
        {
            if (Cycle != null)
            {
                Cycle.CaptureSucceeded -= HandleCaptureSucceeded;
                Cycle.CaptureFailed -= HandleCaptureFailed;
                Cycle.Returned -= HandleReturned;
            }

            throwAction?.Dispose();
            throwAction = null;

            DestroyMaterial(netMaterial);
            DestroyMaterial(dotMaterial);
            DestroyMaterial(circleMaterial);
        }

        private void Update()
        {
            if (Cycle == null || aimCamera == null)
            {
                return;
            }

            if (throwAction.WasPressedThisFrame())
            {
                TryThrow(ThrowOrigin(), aimCamera.transform.forward);
            }

            Cycle.Tick(Time.deltaTime);

            switch (Cycle.Phase)
            {
                case NetPhase.Ready:
                    UpdatePreview();
                    break;
                case NetPhase.Flying:
                    AdvanceFlight(Time.deltaTime);
                    break;
                case NetPhase.Holding:
                    netVisual.position = landingPoint;
                    break;
                case NetPhase.Returning:
                    UpdateReturn();
                    break;
            }

            ShowPhase();
        }

        private Vector3 ThrowOrigin()
        {
            return aimCamera.transform.TransformPoint(ThrowOriginOffset);
        }

        private void AdvanceFlight(float deltaTime)
        {
            flightSeconds += deltaTime;
            var nextPoint = flight.PositionAt(flightSeconds);

            if (CastSegment(previousFlightPoint, nextPoint, out var hitPoint))
            {
                Land(hitPoint);
                return;
            }

            if (flightSeconds >= MaxFlightSeconds)
            {
                // Nothing to land on (thrown over the edge): the net comes back empty.
                Land(nextPoint);
                return;
            }

            previousFlightPoint = nextPoint;
            netVisual.position = nextPoint;
        }

        private void Land(Vector3 point)
        {
            landingPoint = point;
            netVisual.position = point;

            CollectCandidates();
            var result = resolver.Resolve(point, candidates, scaredIndices);

            if (result.Outcome == NetLandingOutcome.Miss)
            {
                foreach (var index in scaredIndices)
                {
                    candidateOwners[index].Scare(point);
                }
            }
            else
            {
                heldTarget = candidateOwners[result.TargetIndex];
                heldTarget.SetHeld(true);
            }

            Cycle.Land(result.Outcome);
        }

        private void UpdateReturn()
        {
            var progress = Cycle.ReturnProgress;
            var handPosition = aimCamera.transform.TransformPoint(HandNetLocalPosition);
            netVisual.position = Vector3.Lerp(landingPoint, handPosition, progress);

            if (Cycle.IsCarryingCatch)
            {
                var closing = Mathf.Clamp01(progress / ClosingShareOfReturn);
                netVisual.localScale = Vector3.Lerp(OpenNetScale(), Vector3.one * ClosedNetSize, closing);

                if (caughtTarget != null)
                {
                    caughtTarget.CarryInNet(netVisual.position, progress);
                }
            }
        }

        private void UpdatePreview()
        {
            var preview = NetTrajectory.Launch(
                ThrowOrigin(),
                aimCamera.transform.forward,
                settings.LaunchSpeed,
                settings.Gravity);

            var hasLanding = preview.TryFindLanding(PreviewTimeStep, MaxFlightSeconds, segmentCast, out var landing);
            var previewSeconds = hasLanding ? landing.FlightSeconds : MaxFlightSeconds;

            for (var index = 0; index < previewDots.Length; index++)
            {
                var time = previewSeconds * (index + 1) / (previewDots.Length + 1);
                previewDots[index].position = preview.PositionAt(time);
            }

            impactCircle.gameObject.SetActive(hasLanding);
            if (!hasLanding)
            {
                return;
            }

            impactCircle.position = landing.Point + Vector3.up * 0.02f;

            CollectCandidates();
            circleMaterial.color = resolver.TryPreview(landing.Point, candidates, out _, out var chance)
                ? ChanceColor(NetCaptureRates.Classify(chance))
                : NoTargetColor;
        }

        private void CollectCandidates()
        {
            candidates.Clear();
            candidateOwners.Clear();

            foreach (var mushroom in mushrooms)
            {
                if (mushroom == null || !mushroom.IsAvailable)
                {
                    continue;
                }

                candidates.Add(new NetCandidate(mushroom.transform.position, mushroom.Profile, mushroom.IsUnaware));
                candidateOwners.Add(mushroom);
            }
        }

        private bool CastSegment(Vector3 from, Vector3 to, out Vector3 hitPoint)
        {
            var offset = to - from;
            var distance = offset.magnitude;
            hitPoint = to;

            if (distance <= 0f)
            {
                return false;
            }

            var count = Physics.RaycastNonAlloc(
                from,
                offset / distance,
                castHits,
                distance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            var closest = float.PositiveInfinity;
            for (var index = 0; index < count; index++)
            {
                var hit = castHits[index];

                // The arc of a net thrown downwards can cross the thrower's own capsule.
                if (hit.collider == ownCollider || hit.distance >= closest)
                {
                    continue;
                }

                closest = hit.distance;
                hitPoint = hit.point;
            }

            return closest < float.PositiveInfinity;
        }

        private void HandleCaptureSucceeded()
        {
            if (heldTarget == null)
            {
                return;
            }

            heldTarget.ResolveCaptureAttempt(true, landingPoint);
            caughtTarget = heldTarget;
            heldTarget = null;
        }

        private void HandleCaptureFailed()
        {
            if (heldTarget == null)
            {
                return;
            }

            heldTarget.ResolveCaptureAttempt(false, landingPoint);
            heldTarget = null;
        }

        private void HandleReturned()
        {
            if (caughtTarget != null)
            {
                caughtTarget.CarryInNet(netVisual.position, 1f);
                caughtTarget = null;
            }
        }

        private void ShowPhase()
        {
            var ready = Cycle.Phase == NetPhase.Ready;

            handNet.gameObject.SetActive(ready);
            netVisual.gameObject.SetActive(!ready);

            if (ready || !Cycle.IsCarryingCatch)
            {
                netVisual.localScale = OpenNetScale();
            }

            foreach (var dot in previewDots)
            {
                dot.gameObject.SetActive(ready);
            }

            if (!ready)
            {
                impactCircle.gameObject.SetActive(false);
            }
        }

        private void BuildVisuals()
        {
            netMaterial = NetDemoPrimitives.CreateMaterial("Net Placeholder", NetColor);
            dotMaterial = NetDemoPrimitives.CreateUnlitMaterial("Net Preview Dot", PreviewDotColor);
            circleMaterial = NetDemoPrimitives.CreateUnlitMaterial("Net Impact Circle", NoTargetColor);

            var root = new GameObject("Net Visuals").transform;

            netVisual = NetDemoPrimitives.Create(
                PrimitiveType.Cylinder,
                "Net",
                root,
                Vector3.zero,
                OpenNetScale(),
                netMaterial,
                keepCollider: false).transform;

            handNet = NetDemoPrimitives.Create(
                PrimitiveType.Cylinder,
                "Net In Hand",
                aimCamera.transform,
                HandNetLocalPosition,
                new Vector3(0.25f, 0.01f, 0.25f),
                netMaterial,
                keepCollider: false).transform;
            handNet.localRotation = Quaternion.Euler(-60f, 0f, 0f);

            impactCircle = NetDemoPrimitives.Create(
                PrimitiveType.Cylinder,
                "Impact Circle",
                root,
                Vector3.zero,
                new Vector3(settings.ImpactRadius * 2f, 0.005f, settings.ImpactRadius * 2f),
                circleMaterial,
                keepCollider: false).transform;
            impactCircle.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            previewDots = new Transform[PreviewDotCount];
            for (var index = 0; index < PreviewDotCount; index++)
            {
                var dot = NetDemoPrimitives.Create(
                    PrimitiveType.Sphere,
                    "Preview Dot",
                    root,
                    Vector3.zero,
                    Vector3.one * 0.06f,
                    dotMaterial,
                    keepCollider: false);
                dot.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                previewDots[index] = dot.transform;
            }
        }

        private static Vector3 OpenNetScale()
        {
            return new Vector3(NetDiameter, NetThickness, NetDiameter);
        }

        private static Color ChanceColor(NetChanceBand band)
        {
            switch (band)
            {
                case NetChanceBand.High:
                    return HighChanceColor;
                case NetChanceBand.Medium:
                    return MediumChanceColor;
                default:
                    return LowChanceColor;
            }
        }

        private static void DestroyMaterial(Material material)
        {
            if (material != null)
            {
                Destroy(material);
            }
        }
    }
}
