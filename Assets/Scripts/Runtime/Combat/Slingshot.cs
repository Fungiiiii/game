using System;
using System.Collections.Generic;
using Fungiiiii.Capture;
using Fungiiiii.Inventory;
using Fungiiiii.Items;
using Fungiiiii.Runtime.World;
using Fungiiiii.UI;
using UnityEngine;

namespace Fungiiiii.Combat
{
    /// <summary>
    /// The slingshot in hand. Holding the secondary control aims: the view narrows and dots
    /// show the arc, as with the net. The primary control shoots a pebble from the
    /// inventory; without one, nothing leaves. A pebble flies along a ballistic arc and is
    /// tested, each frame, along the stretch it just flew: the decor stops it, a mushroom
    /// close enough to that stretch is hit. A tool used from the hand, as every tool.
    /// The model in hand, the pebbles and the dots are placeholders.
    /// </summary>
    internal sealed class Slingshot : MonoBehaviour, IHeldItem, IHeldItemHints
    {
        private static readonly Vector3 ShotOriginOffset = new Vector3(0.15f, -0.12f, 0.5f);
        private static readonly Vector3 InHandPosition = new Vector3(0.28f, -0.3f, 0.55f);
        private static readonly Color WoodColor = new Color(0.54f, 0.37f, 0.26f);
        private static readonly Color PebbleColor = new Color(0.49f, 0.51f, 0.55f);
        private static readonly Color DotColor = Color.white;
        private const float PebbleSize = 0.08f;

        /// <summary>A mushroom's centre stands this high above its feet, in metres.</summary>
        private const float TargetHeight = 0.3f;

        private readonly List<Shot> shots = new List<Shot>();
        private readonly Stack<Transform> idlePebbles = new Stack<Transform>();
        private readonly List<Vector3> dotPositions = new List<Vector3>(NetPreviewDots.MaxDots);
        private readonly RaycastHit[] castHits = new RaycastHit[8];

        private SlingshotSettings settings;
        private Camera view;
        private Collider ownCollider;
        private IReadOnlyList<WildMushroom> targets;
        private PlayerInventory inventory;
        private ItemDefinition ammo;
        private AimZoom zoom;
        private NetSegmentCast segmentCast;
        private Transform inHand;
        private Transform[] dots;
        private Material woodMaterial;
        private Material pebbleMaterial;
        private Material dotMaterial;
        private Transform visualsRoot;
        private bool equipped;
        private bool aiming;
        private float cooldown;
        private string shownPrimary;
        private string shownSecondary;

        public event Action HintsChanged;

        /// <summary>Raised when a pebble hits a mushroom.</summary>
        public event Action<WildMushroom> Hit;

        public bool IsEquipped => equipped;

        public bool IsAiming => aiming;

        public int ShotsInFlight => shots.Count;

        public string PrimaryHintKey => !equipped ? null : HasAmmo ? UiText.HintShoot : UiText.HintNoPebble;

        public string SecondaryHintKey => !equipped ? null : UiText.HintAim;

        private bool HasAmmo => inventory.Count(ammo.Id) > 0;

        public void Initialize(
            SlingshotSettings slingshotSettings,
            Camera viewCamera,
            Collider playerCollider,
            IReadOnlyList<WildMushroom> mushrooms,
            PlayerInventory playerInventory,
            ItemDefinition pebble)
        {
            settings = slingshotSettings;
            view = viewCamera;
            ownCollider = playerCollider;
            targets = mushrooms;
            inventory = playerInventory;
            ammo = pebble;
            if (!view.TryGetComponent(out zoom))
            {
                zoom = view.gameObject.AddComponent<AimZoom>();
            }

            segmentCast = CastSegment;
            inventory.Changed += NotifyHintsIfChanged;
            BuildVisuals();
            SetEquipped(false);
        }

        public void Equip()
        {
            SetEquipped(true);
        }

        public void Unequip()
        {
            SetEquipped(false);
        }

        public void UsePrimary()
        {
            if (view != null)
            {
                TryShoot(view.transform.TransformPoint(ShotOriginOffset), view.transform.forward);
            }
        }

        /// <summary>Called every frame the hands give the controls; not under a menu.</summary>
        public void HoldSecondary(bool held)
        {
            SetAiming(held && equipped);
        }

        /// <summary>A menu took the controls: the aim drops until the hands give them back.</summary>
        public void Cancel()
        {
            SetAiming(false);
        }

        /// <summary>Shoots a pebble from <paramref name="origin"/> along <paramref name="direction"/>; also called by tests.</summary>
        /// <returns>False when not in hand, without a pebble, or before the slingshot is ready again.</returns>
        public bool TryShoot(Vector3 origin, Vector3 direction)
        {
            if (!equipped || cooldown > 0f || !HasAmmo)
            {
                return false;
            }

            inventory.Remove(ammo.Id, 1);
            cooldown = settings.SecondsBetweenShots;
            var flight = NetTrajectory.Launch(origin, direction, settings.ShotSpeed, settings.Gravity);
            var pebble = idlePebbles.Count > 0 ? idlePebbles.Pop() : CreatePebble();
            pebble.position = origin;
            pebble.gameObject.SetActive(true);
            shots.Add(new Shot(flight, pebble));
            return true;
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.Changed -= NotifyHintsIfChanged;
            }

            if (visualsRoot != null)
            {
                Destroy(visualsRoot.gameObject);
            }

            DestroyMaterial(woodMaterial);
            DestroyMaterial(pebbleMaterial);
            DestroyMaterial(dotMaterial);
        }

        private void Update()
        {
            if (settings == null)
            {
                return;
            }

            cooldown = Mathf.Max(0f, cooldown - Time.deltaTime);
            for (var index = shots.Count - 1; index >= 0; index--)
            {
                if (!Advance(shots[index], Time.deltaTime))
                {
                    Retire(index);
                }
            }

            if (aiming)
            {
                ShowArc();
            }
        }

        /// <returns>False once the pebble stopped: it hit something, or flew too long.</returns>
        private bool Advance(Shot shot, float deltaTime)
        {
            var from = shot.Flight.PositionAt(shot.Seconds);
            shot.Seconds += deltaTime;
            var to = shot.Flight.PositionAt(shot.Seconds);

            var target = NearestTargetAlong(from, to, out var targetDistance);
            var hitsDecor = CastSegment(from, to, out var decorPoint);
            if (target != null && (!hitsDecor || targetDistance <= Vector3.Distance(from, decorPoint)))
            {
                if (target.Hit(settings.Damage, from))
                {
                    Hit?.Invoke(target);
                }

                return false;
            }

            if (hitsDecor || shot.Seconds >= settings.MaxFlightSeconds)
            {
                return false;
            }

            shot.Pebble.position = to;
            return true;
        }

        /// <summary>The mushroom whose centre is nearest to the stretch flown, within the hit radius.</summary>
        private WildMushroom NearestTargetAlong(Vector3 from, Vector3 to, out float distanceAlong)
        {
            WildMushroom nearest = null;
            distanceAlong = float.PositiveInfinity;
            var segment = to - from;
            var length = segment.magnitude;
            if (length <= 0f)
            {
                return null;
            }

            var direction = segment / length;
            foreach (var mushroom in targets)
            {
                if (mushroom == null || !mushroom.IsAvailable || !mushroom.isActiveAndEnabled)
                {
                    continue;
                }

                var centre = mushroom.transform.position + Vector3.up * TargetHeight;
                var along = Mathf.Clamp(Vector3.Dot(centre - from, direction), 0f, length);
                var closest = from + direction * along;
                if ((centre - closest).sqrMagnitude <= settings.HitRadius * settings.HitRadius && along < distanceAlong)
                {
                    nearest = mushroom;
                    distanceAlong = along;
                }
            }

            return nearest;
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

            var count = Physics.RaycastNonAlloc(from, offset / distance, castHits, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            var closest = float.PositiveInfinity;
            for (var index = 0; index < count; index++)
            {
                var hit = castHits[index];

                // A mushroom is not decor: it is hit by its distance to the flight, not by its collider.
                if (hit.collider == ownCollider || hit.distance >= closest || hit.collider.GetComponentInParent<WildMushroom>() != null)
                {
                    continue;
                }

                closest = hit.distance;
                hitPoint = hit.point;
            }

            return closest < float.PositiveInfinity;
        }

        private void Retire(int index)
        {
            var pebble = shots[index].Pebble;
            pebble.gameObject.SetActive(false);
            idlePebbles.Push(pebble);
            shots.RemoveAt(index);
        }

        private void ShowArc()
        {
            var origin = view.transform.TransformPoint(ShotOriginOffset);
            var flight = NetTrajectory.Launch(origin, view.transform.forward, settings.ShotSpeed, settings.Gravity);
            var landed = flight.TryFindLanding(NetPreviewDots.TimeStep, settings.MaxFlightSeconds, segmentCast, out var landing);
            NetPreviewDots.Place(flight, landed ? landing.FlightSeconds : settings.MaxFlightSeconds, dotPositions);
            for (var index = 0; index < dots.Length; index++)
            {
                var shown = index < dotPositions.Count;
                dots[index].gameObject.SetActive(shown);
                if (shown)
                {
                    dots[index].position = dotPositions[index];
                }
            }
        }

        private void SetEquipped(bool isEquipped)
        {
            equipped = isEquipped;
            if (inHand != null)
            {
                inHand.gameObject.SetActive(isEquipped);
            }

            if (!isEquipped)
            {
                SetAiming(false);
            }

            NotifyHintsIfChanged();
        }

        private void SetAiming(bool isAiming)
        {
            if (aiming == isAiming && zoom != null)
            {
                return;
            }

            aiming = isAiming;
            if (zoom != null)
            {
                zoom.SetAiming(aiming, settings.AimFieldOfView);
            }

            if (!aiming && dots != null)
            {
                foreach (var dot in dots)
                {
                    dot.gameObject.SetActive(false);
                }
            }

            NotifyHintsIfChanged();
        }

        private void NotifyHintsIfChanged()
        {
            if (inventory == null)
            {
                return;
            }

            var primary = PrimaryHintKey;
            var secondary = SecondaryHintKey;
            if (primary == shownPrimary && secondary == shownSecondary)
            {
                return;
            }

            shownPrimary = primary;
            shownSecondary = secondary;
            HintsChanged?.Invoke();
        }

        private void BuildVisuals()
        {
            woodMaterial = Primitives.CreateMaterial("Slingshot Placeholder", WoodColor);
            pebbleMaterial = Primitives.CreateMaterial("Slingshot Pebble Placeholder", PebbleColor);
            dotMaterial = Primitives.CreateUnlitMaterial("Slingshot Aim Dot", DotColor);
            visualsRoot = new GameObject("Slingshot Visuals").transform;

            // A Y of three sticks, held in front of the view.
            inHand = new GameObject("Slingshot In Hand").transform;
            inHand.SetParent(view.transform, false);
            inHand.localPosition = InHandPosition;
            inHand.localRotation = Quaternion.Euler(-15f, 0f, 0f);
            Stick(inHand, new Vector3(0f, -0.07f, 0f), new Vector3(0.03f, 0.07f, 0.03f), 0f);
            Stick(inHand, new Vector3(-0.035f, 0.03f, 0f), new Vector3(0.025f, 0.045f, 0.025f), 30f);
            Stick(inHand, new Vector3(0.035f, 0.03f, 0f), new Vector3(0.025f, 0.045f, 0.025f), -30f);

            dots = new Transform[NetPreviewDots.MaxDots];
            for (var index = 0; index < dots.Length; index++)
            {
                var dot = Primitives.Create(PrimitiveType.Sphere, "Aim Dot", visualsRoot, Vector3.zero, Vector3.one * 0.05f, dotMaterial, keepCollider: false);
                dot.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                dot.SetActive(false);
                dots[index] = dot.transform;
            }
        }

        private void Stick(Transform parent, Vector3 position, Vector3 scale, float roll)
        {
            var stick = Primitives.Create(PrimitiveType.Cylinder, "Slingshot Stick", parent, position, scale, woodMaterial, keepCollider: false);
            stick.transform.localRotation = Quaternion.Euler(0f, 0f, roll);
            stick.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private Transform CreatePebble()
        {
            var pebble = Primitives.Create(PrimitiveType.Sphere, "Slingshot Pebble", visualsRoot, Vector3.zero, Vector3.one * PebbleSize, pebbleMaterial, keepCollider: false);
            return pebble.transform;
        }

        private static void DestroyMaterial(Material material)
        {
            if (material != null)
            {
                Destroy(material);
            }
        }

        /// <summary>A pebble in flight.</summary>
        private sealed class Shot
        {
            public Shot(NetTrajectory flight, Transform pebble)
            {
                Flight = flight;
                Pebble = pebble;
            }

            public NetTrajectory Flight { get; }

            public Transform Pebble { get; }

            public float Seconds { get; set; }
        }
    }
}
