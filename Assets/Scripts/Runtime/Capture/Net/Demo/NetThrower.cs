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
    /// zone, and plays the hold, the capture and the return with placeholders. The net
    /// and the impact circle are rings of points (<see cref="NetRim"/>) that drape on
    /// the ground point by point, ready for a net model to bind to. A rope
    /// links the hand to the top of the net for as long as the net is out, following
    /// the arc the net flew.
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

        /// <summary>The net: a centre and a ring of points, this wide open, this small closed on a catch.</summary>
        private const float NetRadius = 0.6f;
        private const float ClosedNetRadius = 0.15f;
        private const int NetRimPointCount = 16;
        private const float NetLineWidth = 0.02f;
        private const float ImpactLineWidth = 0.04f;

        /// <summary>Rings of cords between the centre and the rim of the placeholder net.</summary>
        private const int NetInnerRingCount = 3;

        /// <summary>
        /// Draping: each point looks down from this high above the landing point, so it can
        /// climb onto a mushroom cap or a slope, and lies this far above what it finds.
        /// </summary>
        private const float DrapeProbeHeight = 1.5f;
        private const float DrapeLift = 0.03f;

        /// <summary>Share of the return during which a net with a catch closes up.</summary>
        private const float ClosingShareOfReturn = 0.25f;

        private static readonly Vector3 ThrowOriginOffset = new Vector3(0.15f, -0.15f, 0.5f);
        private static readonly Vector3 HandNetLocalPosition = new Vector3(0.3f, -0.28f, 0.6f);

        private static readonly Color NetColor = new Color(0.82f, 0.74f, 0.52f);
        private static readonly Color PreviewDotColor = new Color(1f, 1f, 1f);
        private static readonly Color NoTargetColor = new Color(0.85f, 0.85f, 0.85f);
        private static readonly Color RopeColor = new Color(0.55f, 0.45f, 0.3f);

        /// <summary>Rope placeholder between the hand and the net.</summary>
        private const int RopePointCount = 12;
        private const float RopeWidth = 0.012f;


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
        private NetGroundProbe groundProbe;
        private InputAction throwAction;

        private NetTrajectory flight;
        private float flightSeconds;
        private float ropeArcSeconds;
        private float landedRopeBulge;
        private Vector3 previousFlightPoint;
        private Vector3 landingPoint;
        private NetDemoMushroom heldTarget;
        private NetDemoMushroom caughtTarget;

        private Transform netVisual;
        private NetRim netRim;
        private Transform handNet;
        private NetRim impactRim;
        private Transform[] previewDots;
        private Material netMaterial;
        private Material dotMaterial;
        private Material circleMaterial;
        private Material ropeMaterial;
        private LineRenderer rope;

        public NetCycle Cycle { get; private set; }

        /// <summary>Raised when the net is back in hand with a catch, with the mushroom caught.</summary>
        public event System.Action<NetDemoMushroom> CatchReturned;

        /// <summary>
        /// False ignores the ThrowNet input, for instance while a menu uses the mouse.
        /// The net in flight carries on. <see cref="TryThrow"/> is not affected.
        /// </summary>
        public bool ThrowInputEnabled { get; set; } = true;

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
            groundProbe = ProbeGround;

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
            netRim.ShowFlat(origin, NetRadius);
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
            DestroyMaterial(ropeMaterial);
        }

        private void Update()
        {
            if (Cycle == null || aimCamera == null)
            {
                return;
            }

            if (ThrowInputEnabled && throwAction.WasPressedThisFrame())
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
            netRim.ShowFlat(nextPoint, NetRadius);
        }

        private void Land(Vector3 point)
        {
            landingPoint = point;
            netVisual.position = point;
            netRim.ShowDraped(point, NetRadius, groundProbe, DrapeProbeHeight, DrapeLift);
            FreezeRopeArc();

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

            var radius = NetRadius;
            if (Cycle.IsCarryingCatch)
            {
                var closing = Mathf.Clamp01(progress / ClosingShareOfReturn);
                radius = Mathf.Lerp(NetRadius, ClosedNetRadius, closing);

                if (caughtTarget != null)
                {
                    caughtTarget.CarryInNet(netVisual.position, progress);
                }
            }

            netRim.ShowFlat(netVisual.position, radius);
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

            impactRim.gameObject.SetActive(hasLanding);
            if (!hasLanding)
            {
                return;
            }

            impactRim.ShowDraped(landing.Point, settings.ImpactRadius, groundProbe, DrapeProbeHeight, DrapeLift);

            CollectCandidates();
            circleMaterial.color = resolver.TryPreview(landing.Point, candidates, out _, out var chance)
                ? NetChanceColors.Evaluate(chance)
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

            return distance > 0f && NearestHit(from, offset / distance, distance, ref hitPoint);
        }

        private bool ProbeGround(Vector3 from, float maxDistance, out Vector3 groundPoint)
        {
            groundPoint = from;
            return NearestHit(from, Vector3.down, maxDistance, ref groundPoint);
        }

        /// <summary>Nearest hit along a ray, ignoring the thrower's own capsule.</summary>
        private bool NearestHit(Vector3 origin, Vector3 direction, float distance, ref Vector3 hitPoint)
        {
            var count = Physics.RaycastNonAlloc(
                origin,
                direction,
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
                var caught = caughtTarget;
                caughtTarget = null;
                caught.CarryInNet(netVisual.position, 1f);
                CatchReturned?.Invoke(caught);
            }
        }

        private void ShowPhase()
        {
            var ready = Cycle.Phase == NetPhase.Ready;

            handNet.gameObject.SetActive(ready);
            netRim.gameObject.SetActive(!ready);

            foreach (var dot in previewDots)
            {
                dot.gameObject.SetActive(ready);
            }

            if (!ready)
            {
                impactRim.gameObject.SetActive(false);
            }

            rope.enabled = !ready;
            if (!ready)
            {
                UpdateRope();
            }
        }

        /// <summary>
        /// Draws the rope from the hand to the top of the net. In flight and while the
        /// net holds a mushroom, it follows the arc the net flew, moved so that it starts
        /// at the hand and ends on the net. On the way back, that arc flattens out as the
        /// net comes in.
        /// </summary>
        private void UpdateRope()
        {
            var hand = HandPosition();
            var netTop = NetTop();

            if (Cycle.Phase == NetPhase.Returning)
            {
                var bulge = landedRopeBulge * (1f - Cycle.ReturnProgress);
                for (var index = 0; index < RopePointCount; index++)
                {
                    var along = index / (float)(RopePointCount - 1);
                    var rise = 4f * along * (1f - along) * bulge;
                    rope.SetPosition(index, Vector3.Lerp(hand, netTop, along) + Vector3.up * rise);
                }

                return;
            }

            var arcSeconds = Cycle.Phase == NetPhase.Flying ? flightSeconds : ropeArcSeconds;
            for (var index = 0; index < RopePointCount; index++)
            {
                var along = index / (float)(RopePointCount - 1);
                rope.SetPosition(index, RopePointOnArc(along, arcSeconds, hand, netTop));
            }
        }

        /// <summary>
        /// Keeps the arc the rope follows once the net has landed, and how far it rises
        /// above the straight line, so the return can flatten it.
        /// </summary>
        private void FreezeRopeArc()
        {
            ropeArcSeconds = flightSeconds;

            var hand = HandPosition();
            var netTop = NetTop();
            var middle = RopePointOnArc(0.5f, ropeArcSeconds, hand, netTop);
            landedRopeBulge = Mathf.Max(0f, middle.y - Vector3.Lerp(hand, netTop, 0.5f).y);
        }

        /// <summary>
        /// Point of the flown arc at <paramref name="along"/> (0 at the hand, 1 on the net),
        /// shifted so that its ends meet the hand and the top of the net.
        /// </summary>
        private Vector3 RopePointOnArc(float along, float arcSeconds, Vector3 hand, Vector3 netTop)
        {
            var arcEnd = flight.PositionAt(arcSeconds);
            return flight.PositionAt(along * arcSeconds)
                + (hand - flight.Origin) * (1f - along)
                + (netTop - arcEnd) * along;
        }

        private Vector3 HandPosition()
        {
            return aimCamera.transform.TransformPoint(HandNetLocalPosition);
        }

        /// <summary>Where the rope ties on: the centre point of the net, on top of it.</summary>
        private Vector3 NetTop()
        {
            return netRim.Centre.position;
        }

        private void BuildVisuals()
        {
            netMaterial = NetDemoPrimitives.CreateMaterial("Net Placeholder", NetColor);
            dotMaterial = NetDemoPrimitives.CreateUnlitMaterial("Net Preview Dot", PreviewDotColor);
            circleMaterial = NetDemoPrimitives.CreateUnlitMaterial("Net Impact Circle", NoTargetColor);

            var root = new GameObject("Net Visuals").transform;

            // The net's position, carried by an empty object; its shape is the rim of points.
            netVisual = new GameObject("Net").transform;
            netVisual.SetParent(root, false);
            netRim = CreateRim("Net Rim", root, netMaterial, NetLineWidth, withSpokes: true, NetInnerRingCount);

            handNet = NetDemoPrimitives.Create(
                PrimitiveType.Cylinder,
                "Net In Hand",
                aimCamera.transform,
                HandNetLocalPosition,
                new Vector3(0.25f, 0.01f, 0.25f),
                netMaterial,
                keepCollider: false).transform;
            handNet.localRotation = Quaternion.Euler(-60f, 0f, 0f);

            impactRim = CreateRim("Impact Circle", root, circleMaterial, ImpactLineWidth, withSpokes: false, innerRingCount: 0);

            ropeMaterial = NetDemoPrimitives.CreateUnlitMaterial("Net Rope", RopeColor);
            var ropeObject = new GameObject("Net Rope");
            ropeObject.transform.SetParent(root, false);
            rope = ropeObject.AddComponent<LineRenderer>();
            rope.useWorldSpace = true;
            rope.positionCount = RopePointCount;
            rope.startWidth = RopeWidth;
            rope.endWidth = RopeWidth;
            rope.sharedMaterial = ropeMaterial;
            rope.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rope.enabled = false;

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

        private static NetRim CreateRim(
            string objectName,
            Transform parent,
            Material material,
            float width,
            bool withSpokes,
            int innerRingCount)
        {
            var rimObject = new GameObject(objectName);
            rimObject.transform.SetParent(parent, false);

            var rim = rimObject.AddComponent<NetRim>();
            rim.Build(NetRimPointCount, material, width, withSpokes, innerRingCount);
            return rim;
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
