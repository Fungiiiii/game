using System.Collections.Generic;
using Fungiiiii.Items;
using UnityEngine;

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
    /// The primary action throws the net; the secondary one, held, aims: only then are the
    /// arc and the impact circle shown, the view narrows and turns slower. A net can be
    /// thrown without aiming, just without a preview. It is the net's behaviour in hand
    /// (<see cref="IHeldItem"/>): <see cref="PlayerHands"/> gives it both actions, and the
    /// shared <see cref="AimZoom"/> of the camera narrows the view.
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class NetThrower : MonoBehaviour, IHeldItem
    {
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

        /// <summary>Colour of the placeholder net, also its colour as an inventory item.</summary>
        internal static readonly Color NetColor = new Color(0.82f, 0.74f, 0.52f);
        private static readonly Color PreviewDotColor = new Color(1f, 1f, 1f);
        private static readonly Color NoTargetColor = new Color(0.85f, 0.85f, 0.85f);
        private static readonly Color RopeColor = new Color(0.55f, 0.45f, 0.3f);

        /// <summary>While aiming, the view narrows to this, in degrees.</summary>
        private const float AimFieldOfView = 45f;

        /// <summary>Rope placeholder between the hand and the net.</summary>
        private const int RopePointCount = 12;
        private const float RopeWidth = 0.012f;


        private readonly List<NetCandidate> candidates = new List<NetCandidate>();
        private readonly List<NetDemoMushroom> candidateOwners = new List<NetDemoMushroom>();
        private readonly List<int> scaredIndices = new List<int>();
        private readonly RaycastHit[] castHits = new RaycastHit[CastBufferSize];
        private readonly List<Vector3> dotPositions = new List<Vector3>(NetPreviewDots.MaxDots);

        private NetThrowSettings settings;
        private Camera aimCamera;
        private Collider ownCollider;
        private IReadOnlyList<NetDemoMushroom> mushrooms;
        private NetLandingResolver resolver;
        private NetSegmentCast segmentCast;
        private NetGroundProbe groundProbe;
        private AimZoom zoom;
        private bool secondaryHeld;
        private bool controlsTaken;

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
        private bool isEquipped;

        public NetCycle Cycle { get; private set; }

        /// <summary>Raised when the net is back in hand with a catch, with the mushroom caught.</summary>
        public event System.Action<NetDemoMushroom> CatchReturned;

        /// <summary>Aims as if the secondary action were held: for tests, which have no device.</summary>
        public bool HoldAim { get; set; }

        /// <summary>True while the secondary action aims the net in hand.</summary>
        public bool IsAiming { get; private set; }

        /// <summary>How many dots of the arc are shown.</summary>
        public int VisibleDotCount
        {
            get
            {
                var count = 0;
                foreach (var dot in previewDots)
                {
                    count += dot.gameObject.activeSelf ? 1 : 0;
                }

                return count;
            }
        }

        public bool IsImpactCircleShown => impactRim != null && impactRim.gameObject.activeSelf;

        /// <summary>Where the aimed net would land, while <see cref="IsImpactCircleShown"/>.</summary>
        public Vector3 PreviewLandingPoint { get; private set; }

        /// <summary>The mushroom the aimed net would catch, or null.</summary>
        public NetDemoMushroom PreviewTarget { get; private set; }

        /// <summary>The chance the aimed net would catch <see cref="PreviewTarget"/>, from 0 to 1.</summary>
        public float PreviewChance { get; private set; }

        /// <summary>
        /// Whether the net is the item in hand (<see cref="Equip"/>). Put away, the net in
        /// hand and the aim are hidden and the net cannot be thrown; a net already out
        /// still comes back.
        /// </summary>
        public bool IsEquipped => isEquipped;

        /// <summary>True while the net in hand is drawn.</summary>
        public bool IsNetInHandShown => handNet != null && handNet.gameObject.activeSelf;

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
            if (!aimCamera.TryGetComponent(out zoom))
            {
                zoom = aimCamera.gameObject.AddComponent<AimZoom>();
            }
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
        /// Called by the primary action, and by tests to throw without a device.
        /// </summary>
        /// <returns>False while the net is not back in hand, or not equipped.</returns>
        public bool TryThrow(Vector3 origin, Vector3 direction)
        {
            if (Cycle == null || !isEquipped || !Cycle.TryThrow())
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

        private void OnDestroy()
        {
            if (Cycle != null)
            {
                Cycle.CaptureSucceeded -= HandleCaptureSucceeded;
                Cycle.CaptureFailed -= HandleCaptureFailed;
                Cycle.Returned -= HandleReturned;
            }

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

            IsAiming = isEquipped && !controlsTaken && (HoldAim || secondaryHeld);
            zoom.SetAiming(IsAiming, AimFieldOfView);

            Cycle.Tick(Time.deltaTime);
            PreviewTarget = null;
            PreviewChance = 0f;

            switch (Cycle.Phase)
            {
                case NetPhase.Ready:
                    if (IsAiming)
                    {
                        UpdatePreview();
                    }

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

        private void DropAim()
        {
            IsAiming = false;
            PreviewTarget = null;
            PreviewChance = 0f;
            if (zoom != null)
            {
                zoom.Drop();
            }

            if (Cycle != null)
            {
                ShowPhase();
            }
        }

        public void Equip()
        {
            SetEquipped(true);
        }

        public void Unequip()
        {
            SetEquipped(false);
        }

        /// <summary>Throws the net from the hand where the view points.</summary>
        public void UsePrimary()
        {
            controlsTaken = false;
            if (aimCamera != null)
            {
                TryThrow(ThrowOrigin(), aimCamera.transform.forward);
            }
        }

        /// <summary>Held, it aims: the arc and the impact circle show, the view narrows.</summary>
        public void HoldSecondary(bool held)
        {
            controlsTaken = false;
            secondaryHeld = held;
        }

        /// <summary>Drops the aim and widens the view at once. The net in flight carries on.</summary>
        public void Cancel()
        {
            // Until the hands give the controls back, not even HoldAim aims.
            controlsTaken = true;
            secondaryHeld = false;
            DropAim();
        }

        private void SetEquipped(bool equipped)
        {
            isEquipped = equipped;
            if (!equipped)
            {
                DropAim();
            }

            if (Cycle != null)
            {
                ShowPhase();
            }
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

            var hasLanding = preview.TryFindLanding(NetPreviewDots.TimeStep, MaxFlightSeconds, segmentCast, out var landing);
            var previewSeconds = hasLanding ? landing.FlightSeconds : MaxFlightSeconds;

            NetPreviewDots.Place(preview, previewSeconds, dotPositions);
            for (var index = 0; index < previewDots.Length; index++)
            {
                var shown = index < dotPositions.Count;
                previewDots[index].gameObject.SetActive(shown);
                if (shown)
                {
                    previewDots[index].position = dotPositions[index];
                }
            }

            impactRim.gameObject.SetActive(hasLanding);
            if (!hasLanding)
            {
                return;
            }

            PreviewLandingPoint = landing.Point;
            impactRim.ShowDraped(landing.Point, settings.ImpactRadius, groundProbe, DrapeProbeHeight, DrapeLift);

            CollectCandidates();
            if (resolver.TryPreview(landing.Point, candidates, out var targetIndex, out var chance))
            {
                PreviewTarget = candidateOwners[targetIndex];
                PreviewChance = chance;
                circleMaterial.color = NetChanceColors.Evaluate(chance);
            }
            else
            {
                circleMaterial.color = NoTargetColor;
            }
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
            var inHand = ready && isEquipped;

            handNet.gameObject.SetActive(inHand);
            netRim.gameObject.SetActive(!ready);

            // The dots and the circle are placed by the preview, and shown only while aiming.
            if (!inHand || !IsAiming)
            {
                foreach (var dot in previewDots)
                {
                    dot.gameObject.SetActive(false);
                }

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

            previewDots = new Transform[NetPreviewDots.MaxDots];
            for (var index = 0; index < previewDots.Length; index++)
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
