using System.Collections;
using System.Collections.Generic;
using Fungiiiii.Capture;
using Fungiiiii.Champimaison;
using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using Fungiiiii.Runtime.Champimaison;
using Fungiiiii.Runtime.Player;
using Fungiiiii.Survival;
using Fungiiiii.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Fungiiiii.Demo
{
    /// <summary>
    /// Entry point of the demo loop (explore, gather, capture, feed the network), placed
    /// alone in DemoLoopScene. It loads Hugo's MysticForest additively without changing
    /// it: the forest's test player is switched off and its Main Camera, which carries the
    /// zone audio, the spores and the ground mist, becomes the first-person view of the
    /// real Player prefab, used offline as in NetThrowScene.
    ///
    /// Everything else is built at runtime: HUD, empty inventory, resources to gather,
    /// wandering Spongeus and Champoule, the capture net and the Champimaison hole.
    /// </summary>
    internal sealed class DemoLoopBootstrap : MonoBehaviour
    {
        private const string ForestScenePath = "Assets/Scenes/Biomes/MysticForest.unity";

        /// <summary>Hugo's hand-placed decor mushrooms are prefab instances under this root.</summary>
        private const string LandmarksName = "Landmarks";
        private const string DecorMushroomNamePrefix = "PF_MushroomCluster";

        /// <summary>The Player prefab's pivot is the centre of its 2 m capsule.</summary>
        private const float PlayerPivotAboveGround = 1.1f;

        private const float AimConeDegrees = 25f;
        private const float SporeRingRadius = 0.6f;
        private const float WoodRingRadius = 0.6f;

        private static readonly Color RefusedColor = new Color(1f, 0.45f, 0.1f);

        [Header("Assets")]
        [Tooltip("The player prefab, used offline: no network session is started in this scene.")]
        [SerializeField]
        private GameObject playerPrefab;

        [SerializeField]
        private GameObject spongeusModel;

        [SerializeField]
        private GameObject champouleModel;

        [Tooltip("Decor mushroom of the forest, placed again by the demo as extra spore spots.")]
        [SerializeField]
        private GameObject decorMushroomPrefab;

        [Tooltip("Stick lying on the ground: the wood to gather, until a wood mushroom exists.")]
        [SerializeField]
        private GameObject stickPrefab;

        [Header("Stamina (RG_SU_03, RG_SU_04), in % of the maximum")]
        [SerializeField, Min(0f)]
        private float sprintCostPerSecond = 15f;

        [SerializeField, Min(0f)]
        private float dodgeCost = 20f;

        [SerializeField, Min(0f)]
        private float regenerationPerSecond = 20f;

        [SerializeField, Min(0f)]
        private float regenerationDelaySeconds = 1f;

        [SerializeField, Min(0f)]
        private float recoveryThreshold = 20f;

        [Header("Gathering: starting values of the design note, without source")]
        [SerializeField, Min(0.1f)]
        private float gatherReach = 2f;

        [SerializeField, Min(0f)]
        private float minRespawnSeconds = 30f;

        [SerializeField, Min(0f)]
        private float maxRespawnSeconds = 60f;

        [Tooltip("Cyan of the highlight. PLACEHOLDER: the charter's exact cyan was not found.")]
        [SerializeField]
        private Color highlightColor = new Color(0.2f, 0.95f, 1f);

        [Tooltip("Colour of the fungal network and of the hole's ring: palette \"Body light\", a mycelium cream.")]
        [SerializeField]
        private Color networkColor = new Color32(0xF2, 0xE6, 0xCC, 0xFF);

        [Header("Fungal network: starting values of the design note, without source")]
        [SerializeField, Min(0)]
        private int stageOneWood = 2;

        [SerializeField, Min(0)]
        private int stageOneSpores = 2;

        [SerializeField, Min(0)]
        private int stageTwoWood = 4;

        [SerializeField, Min(0)]
        private int stageTwoSpores = 4;

        [Header("Mushrooms")]
        [SerializeField, Min(0f)]
        private float wanderRadius = 4f;

        [SerializeField, Min(0f)]
        private float wanderSpeed = 0.8f;

        [SerializeField, Min(0f)]
        private float wanderPauseSeconds = 1.5f;

        [SerializeField, Min(0f)]
        private float fleeSpeed = 4f;

        [Header("Net: values of the validated net prototype")]
        [SerializeField, Min(0.1f)]
        private float launchSpeed = 11f;

        [SerializeField, Min(0f)]
        private float netGravity = 9.81f;

        [SerializeField, Min(0.1f)]
        private float impactRadius = 1.5f;

        [SerializeField, Min(0f)]
        private float scareRadius = 3f;

        [SerializeField, Min(0f)]
        private float holdSeconds = 0.5f;

        [SerializeField, Min(0.1f)]
        private float returnSeconds = 1f;

        [Tooltip("Seed of every random draw of the demo. 0 picks a different seed every run.")]
        [SerializeField]
        private int randomSeed;

        // Demo spots, in world coordinates of the forest: the start clearing is centred on
        // (0, -235) with a 16 m radius, and the player spawns at its south edge.
        private static readonly Vector2[] ExtraSporeSpots =
        {
            new Vector2(-9f, -240f), new Vector2(10f, -242f), new Vector2(-11f, -229f),
            new Vector2(12f, -228f), new Vector2(-4f, -225f),
        };

        private static readonly Vector2[] WoodSpots =
        {
            new Vector2(-5f, -238f), new Vector2(6f, -237f), new Vector2(-12f, -234f),
            new Vector2(9f, -226f), new Vector2(3f, -222f),
        };

        /// <summary>Spots next to the hole, relative to its centre.</summary>
        private static readonly Vector2[] SporeSpotsAroundHole = { new Vector2(-4f, -4f), new Vector2(4f, 4f) };
        private static readonly Vector2[] WoodSpotsAroundHole = { new Vector2(-2f, 5f), new Vector2(3f, -5f) };

        private static readonly Vector2[] SpongeusHomes = { new Vector2(4f, -228f), new Vector2(-7f, -224f) };
        private static readonly Vector2[] ChampouleHomes = { new Vector2(10f, -236f), new Vector2(-3f, -217f) };

        /// <summary>Mushrooms cannot flee out of this ground rectangle (x, z) around the spawn and the hole.</summary>
        private static readonly Rect MushroomArea = new Rect(-30f, -260f, 80f, 65f);

        private readonly List<IDemoInteractable> interactables = new List<IDemoInteractable>();
        private readonly List<Gatherable> gatherables = new List<Gatherable>();
        private readonly List<NetDemoMushroom> mushrooms = new List<NetDemoMushroom>();
        private readonly Dictionary<NetDemoMushroom, InventoryItemDefinition> mushroomItems =
            new Dictionary<NetDemoMushroom, InventoryItemDefinition>();
        private readonly List<Material> materials = new List<Material>();

        private InputAction inventoryAction;
        private InputAction resetAction;
        private System.Random random;
        private Terrain terrain;
        private Vector3 spawnPosition;
        private float spawnYaw;
        private GameObject player;
        private CharacterController playerController;
        private PrototypeFirstPersonCamera firstPersonCamera;
        private PlayerVitals vitals;
        private StaminaRegulator stamina;
        private InventoryDemoController inventoryView;
        private PlayerInteractor interactor;
        private FungalHole hole;

        /// <summary>True once the forest is loaded and the loop is built.</summary>
        public bool IsReady { get; private set; }

        public PlayerInventory Inventory => inventoryView != null ? inventoryView.Inventory : null;

        public NetThrower Thrower { get; private set; }

        public IReadOnlyList<NetDemoMushroom> Mushrooms => mushrooms;

        public IReadOnlyList<Gatherable> Gatherables => gatherables;

        public FungalHole Hole => hole;

        public bool IsInventoryOpen => inventoryView != null && inventoryView.gameObject.activeSelf;

        /// <summary>The inventory item a mushroom becomes once caught.</summary>
        public InventoryItemDefinition ItemFor(NetDemoMushroom mushroom) => mushroomItems[mushroom];

        private void Awake()
        {
            // Inventory and reset are demo controls built in code, like Dodge in PlayerInputReader.
            inventoryAction = new InputAction("ToggleInventory", InputActionType.Button);
            inventoryAction.AddBinding("<Keyboard>/tab");
            inventoryAction.AddBinding("<Gamepad>/select");
            inventoryAction.performed += OnInventoryPerformed;

            // Development shortcut, not a player-facing control.
            resetAction = new InputAction("ResetDemoLoop", InputActionType.Button);
            resetAction.AddBinding("<Keyboard>/r");
            resetAction.performed += OnResetPerformed;
        }

        private void OnEnable()
        {
            inventoryAction.Enable();
            resetAction.Enable();
        }

        private void OnDisable()
        {
            inventoryAction.Disable();
            resetAction.Disable();
        }

        private void OnDestroy()
        {
            inventoryAction.performed -= OnInventoryPerformed;
            resetAction.performed -= OnResetPerformed;
            inventoryAction.Dispose();
            resetAction.Dispose();

            if (Thrower != null)
            {
                Thrower.CatchReturned -= HandleCatchReturned;
            }

            foreach (var material in materials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
        }

        private IEnumerator Start()
        {
            if (playerPrefab == null || spongeusModel == null || champouleModel == null || decorMushroomPrefab == null ||
                stickPrefab == null)
            {
                Debug.LogError("DemoLoopBootstrap is missing one of its assets.", this);
                enabled = false;
                yield break;
            }

            random = randomSeed != 0 ? new System.Random(randomSeed) : new System.Random();

            yield return LoadForest();

            var forest = SceneManager.GetSceneByPath(ForestScenePath);
            if (!forest.IsValid() || !forest.isLoaded)
            {
                Debug.LogError($"DemoLoopBootstrap could not load {ForestScenePath}.", this);
                enabled = false;
                yield break;
            }

            // The forest's fog, sky and ambient light belong to it: it has to be the active scene.
            SceneManager.SetActiveScene(forest);
            terrain = Terrain.activeTerrain;

            var forestCamera = TakeOverForestPlayer();
            if (forestCamera == null)
            {
                Debug.LogError("DemoLoopBootstrap found no FirstPersonTestPlayer with a camera in the forest.", this);
                enabled = false;
                yield break;
            }

            var cyan = CreateUnlit("Demo Highlight", highlightColor);
            var refused = CreateUnlit("Demo Refused", RefusedColor);

            SpawnPlayer(forestCamera);
            BuildHudAndInventory();
            BuildHole(refused);
            BuildGatherables(forest, cyan);
            SpawnMushrooms();
            SetUpThrower();

            interactor = player.AddComponent<PlayerInteractor>();
            interactor.Initialize(firstPersonCamera.Camera, Inventory, interactables, AimConeDegrees);

            IsReady = true;
        }

        private static IEnumerator LoadForest()
        {
            if (SceneManager.GetSceneByPath(ForestScenePath).isLoaded)
            {
                yield break;
            }

#if UNITY_EDITOR
            // The demo is played from the Editor: the forest need not be in the build settings.
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                ForestScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
#else
            yield return SceneManager.LoadSceneAsync(ForestScenePath, LoadSceneMode.Additive);
#endif
        }

        /// <summary>
        /// Switches the forest's test player off and keeps its camera, detached, for the
        /// real player. Its pose becomes the spawn.
        /// </summary>
        private Camera TakeOverForestPlayer()
        {
            var testPlayer = FindFirstObjectByType<FirstPersonTestPlayer>();
            if (testPlayer == null)
            {
                return null;
            }

            var forestCamera = testPlayer.GetComponentInChildren<Camera>();
            if (forestCamera == null)
            {
                return null;
            }

            spawnPosition = testPlayer.transform.position;
            spawnYaw = testPlayer.transform.eulerAngles.y;

            forestCamera.transform.SetParent(null, true);
            testPlayer.gameObject.SetActive(false);
            return forestCamera;
        }

        private void SpawnPlayer(Camera forestCamera)
        {
            spawnPosition.y = GroundHeight(spawnPosition) + PlayerPivotAboveGround;
            player = Instantiate(playerPrefab, spawnPosition, Quaternion.Euler(0f, spawnYaw, 0f));
            player.name = "Player";
            playerController = player.GetComponent<CharacterController>();

            // First person: the body is not drawn for its own camera, but still casts its shadow.
            foreach (var bodyRenderer in player.GetComponentsInChildren<Renderer>())
            {
                bodyRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }

            firstPersonCamera = forestCamera.gameObject.AddComponent<PrototypeFirstPersonCamera>();
            firstPersonCamera.Initialize(player.GetComponent<PlayerMotor>(), spawnYaw);
        }

        private void BuildHudAndInventory()
        {
            vitals = new PlayerVitals(100f, 100f);
            stamina = new StaminaRegulator(vitals, new StaminaRegulator.Settings(
                sprintCostPerSecond,
                dodgeCost,
                regenerationPerSecond,
                regenerationDelaySeconds,
                recoveryThreshold));

            var hud = new GameObject("Vitals HUD").AddComponent<VitalsHud>();
            hud.Initialize(vitals);

            player.AddComponent<StaminaDriver>().Initialize(player.GetComponent<PlayerMotor>(), stamina);

            // Built inactive so that Awake does not seed sample items: the inventory starts empty.
            var inventoryObject = new GameObject("Inventory");
            inventoryObject.SetActive(false);
            inventoryView = inventoryObject.AddComponent<InventoryDemoController>();
            inventoryView.SeedsDemoItems = false;
            inventoryObject.SetActive(true);
            inventoryObject.SetActive(false);
        }

        private void BuildHole(Material refused)
        {
            var site = NearestChampimaisonSite();
            var centre = site != null ? site.transform.position : spawnPosition + Vector3.forward * 10f;
            centre.y = GroundHeight(centre);

            var network = new FungalNetwork(
                new[]
                {
                    new NetworkStageCost(stageOneWood, stageOneSpores),
                    new NetworkStageCost(stageTwoWood, stageTwoSpores),
                },
                DemoItems.WoodId,
                DemoItems.SporeId);

            var holeObject = new GameObject("Champimaison Hole");
            holeObject.transform.position = centre;
            hole = holeObject.AddComponent<FungalHole>();
            // Lit filaments take the forest's light and fog, like the ground they lie on.
            var filaments = CreateLit("Fungal Network", networkColor);
            var holeRing = CreateUnlit("Champimaison Hole Ring", networkColor);
            hole.Initialize(network, gatherReach, filaments, holeRing, refused, GroundHeight);
            interactables.Add(hole);
        }

        private ChampimaisonSite NearestChampimaisonSite()
        {
            ChampimaisonSite nearest = null;
            var nearestDistance = float.PositiveInfinity;

            foreach (var site in FindObjectsByType<ChampimaisonSite>(FindObjectsSortMode.None))
            {
                var distance = Vector3.Distance(site.transform.position, spawnPosition);
                if (distance < nearestDistance)
                {
                    nearest = site;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private void BuildGatherables(Scene forest, Material cyan)
        {
            // Hugo's hand-placed decor mushrooms: the demo hides and shows them, nothing else.
            foreach (var root in forest.GetRootGameObjects())
            {
                if (root.name != LandmarksName)
                {
                    continue;
                }

                foreach (Transform child in root.transform)
                {
                    if (child.name.StartsWith(DecorMushroomNamePrefix))
                    {
                        AddGatherable("Spore Spot (forest decor)", child.position, child.gameObject, DemoItems.Spore, SporeRingRadius);
                    }
                }
            }

            var holeCentre = hole.transform.position;

            foreach (var spot in ExtraSporeSpots)
            {
                AddDecorMushroom(OnGround(spot));
            }

            foreach (var offset in SporeSpotsAroundHole)
            {
                AddDecorMushroom(OnGround(new Vector2(holeCentre.x + offset.x, holeCentre.z + offset.y)));
            }

            foreach (var spot in WoodSpots)
            {
                AddStick(OnGround(spot));
            }

            foreach (var offset in WoodSpotsAroundHole)
            {
                AddStick(OnGround(new Vector2(holeCentre.x + offset.x, holeCentre.z + offset.y)));
            }

            void AddDecorMushroom(Vector3 position)
            {
                var yaw = (float)random.NextDouble() * 360f;
                var decor = Instantiate(decorMushroomPrefab, position, Quaternion.Euler(0f, yaw, 0f));
                decor.name = "Spore Mushroom";
                AddGatherable("Spore Spot (demo)", position, decor, DemoItems.Spore, SporeRingRadius);
            }

            void AddStick(Vector3 position)
            {
                // Hugo's stick, lying on the ground: temporary, until a wood mushroom exists (RG_CR_01).
                var spotObject = new GameObject("Wood Spot");
                spotObject.transform.position = position;
                var yaw = (float)random.NextDouble() * 360f;
                var stick = Instantiate(stickPrefab, position, Quaternion.Euler(0f, yaw, 0f), spotObject.transform);
                stick.name = "Stick";

                var gatherable = spotObject.AddComponent<Gatherable>();
                gatherable.Initialize(
                    DemoItems.Wood,
                    stick,
                    NewResource(),
                    gatherReach,
                    WoodRingRadius,
                    cyan,
                    GroundHeight);
                Register(gatherable);
            }

            void AddGatherable(string name, Vector3 position, GameObject visual, InventoryItemDefinition item, float ringRadius)
            {
                var spotObject = new GameObject(name);
                spotObject.transform.position = position;
                var gatherable = spotObject.AddComponent<Gatherable>();
                gatherable.Initialize(item, visual, NewResource(), gatherReach, ringRadius, cyan, GroundHeight);
                Register(gatherable);
            }

            void Register(Gatherable gatherable)
            {
                gatherables.Add(gatherable);
                interactables.Add(gatherable);
            }
        }

        private RespawningResource NewResource()
        {
            return new RespawningResource(minRespawnSeconds, Mathf.Max(minRespawnSeconds, maxRespawnSeconds), random);
        }

        private void SpawnMushrooms()
        {
            foreach (var home in SpongeusHomes)
            {
                SpawnMushroom("Spongeus", home, spongeusModel, NetTargetProfile.Common, DemoItems.Spongeus);
            }

            foreach (var home in ChampouleHomes)
            {
                SpawnMushroom("Champoule", home, champouleModel, NetTargetProfile.Rare, DemoItems.Champoule);
            }
        }

        private void SpawnMushroom(
            string species,
            Vector2 home,
            GameObject model,
            NetTargetProfile profile,
            InventoryItemDefinition item)
        {
            var mushroomObject = new GameObject($"Mushroom ({species})");
            mushroomObject.transform.position = OnGround(home);

            var mushroom = mushroomObject.AddComponent<NetDemoMushroom>();
            mushroom.InitializeWithModel(profile, player.transform, MushroomArea, fleeSpeed, model);
            mushroom.FollowGround(GroundHeight);
            mushroom.EnableWander(new MushroomWander(
                mushroomObject.transform.position,
                wanderRadius,
                wanderSpeed,
                wanderPauseSeconds,
                new System.Random(random.Next())));

            mushrooms.Add(mushroom);
            mushroomItems.Add(mushroom, item);
        }

        private void SetUpThrower()
        {
            var settings = new NetThrowSettings(
                launchSpeed,
                netGravity,
                impactRadius,
                scareRadius,
                holdSeconds,
                returnSeconds,
                NetCaptureRates.Default);

            Thrower = player.AddComponent<NetThrower>();
            Thrower.Initialize(settings, firstPersonCamera.Camera, playerController, mushrooms, new System.Random(random.Next()));
            Thrower.CatchReturned += HandleCatchReturned;
        }

        private void HandleCatchReturned(NetDemoMushroom caught)
        {
            // RG_CA_03: the caught mushroom goes into the inventory as it is.
            if (Inventory.Add(mushroomItems[caught], 1) == 0)
            {
                Debug.LogWarning($"The inventory is full: {caught.name} is lost.", this);
            }
        }

        private void OnInventoryPerformed(InputAction.CallbackContext _)
        {
            if (IsReady)
            {
                SetInventoryOpen(!IsInventoryOpen);
            }
        }

        /// <summary>
        /// Opens or closes the inventory. While it is open the mouse is free for it: the
        /// view does not turn, and neither the net nor the interact control respond.
        /// </summary>
        public void SetInventoryOpen(bool open)
        {
            inventoryView.gameObject.SetActive(open);
            firstPersonCamera.enabled = !open;
            Thrower.ThrowInputEnabled = !open;
            interactor.enabled = !open;
        }

        private void OnResetPerformed(InputAction.CallbackContext _)
        {
            if (IsReady)
            {
                ResetDemo();
            }
        }

        /// <summary>Everything back to the start: player, gauges, empty inventory, resources, mushrooms, hole.</summary>
        public void ResetDemo()
        {
            SetInventoryOpen(false);
            Thrower.ResetNet();

            foreach (var mushroom in mushrooms)
            {
                mushroom.ResetToSpawn();
            }

            foreach (var gatherable in gatherables)
            {
                gatherable.ResetResource();
            }

            hole.ResetNetwork();
            EmptyInventory();
            vitals.Reset();
            stamina.Reset();

            // A CharacterController keeps its own cached position and would snap back to
            // it on the next Move, so it has to be off while the transform is teleported.
            playerController.enabled = false;
            player.transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0f, spawnYaw, 0f));
            playerController.enabled = true;
            firstPersonCamera.SetView(spawnYaw, 0f);
        }

        private void EmptyInventory()
        {
            var inventory = Inventory;
            for (var index = 0; index < inventory.SlotCount; index++)
            {
                var slot = inventory.GetSlot(index);
                if (!slot.IsEmpty)
                {
                    inventory.TakeFromSlot(index, slot.Quantity, out _);
                }
            }
        }

        private float GroundHeight(Vector3 point)
        {
            return terrain != null ? terrain.SampleHeight(point) + terrain.GetPosition().y : point.y;
        }

        private Vector3 OnGround(Vector2 groundPoint)
        {
            var point = new Vector3(groundPoint.x, 0f, groundPoint.y);
            point.y = GroundHeight(point);
            return point;
        }

        private Material CreateUnlit(string materialName, Color color)
        {
            var material = NetDemoPrimitives.CreateUnlitMaterial(materialName, color);
            materials.Add(material);
            return material;
        }

        private Material CreateLit(string materialName, Color color)
        {
            var material = NetDemoPrimitives.CreateMaterial(materialName, color);
            materials.Add(material);
            return material;
        }
    }
}
