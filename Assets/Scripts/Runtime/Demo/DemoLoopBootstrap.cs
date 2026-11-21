using System.Collections;
using System.Collections.Generic;
using Fungiiiii.Capture;
using Fungiiiii.Champimaison;
using Fungiiiii.Controls;
using Fungiiiii.Gathering;
using Fungiiiii.Inventory;
using Fungiiiii.Items;
using Fungiiiii.Runtime.Champimaison;
using Fungiiiii.Runtime.Player;
using Fungiiiii.Runtime.World;
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
    /// Everything else is built at runtime: HUD, inventory holding only the capture net,
    /// the quick bar, wandering Spongeus and Champoule, and the sticks and mushrooms to
    /// gather, which a tick keeps around the player. The Champimaison hole is left out of
    /// the playtest.
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

        /// <summary>Spore mushrooms go 2 cm into the ground, so none floats on a slope.</summary>
        private const float MushroomSink = 0.02f;

        // A mushroom spot is dropped if this capsule, above the ground, touches a trunk or
        // a rock. Up to a 30° slope it does not touch the ground itself.
        private const float ObstacleCapsuleBottom = 0.9f;
        private const float ObstacleCapsuleTop = 2f;
        private const float ObstacleCapsuleRadius = 0.45f;

        private static readonly Color RefusedColor = new Color(1f, 0.45f, 0.1f);

        [Header("Assets")]
        [Tooltip("The player prefab, used offline: no network session is started in this scene.")]
        [SerializeField]
        private GameObject playerPrefab;

        [SerializeField]
        private GameObject spongeusModel;

        [SerializeField]
        private GameObject champouleModel;

        [Tooltip("Every item of the game: the demo takes its wood, spores, mushrooms and net from it.")]
        [SerializeField]
        private ItemCatalog itemCatalog;

        [Tooltip("Decor mushroom of the forest: the Terrain's instances of it become spore spots.")]
        [SerializeField]
        private GameObject decorMushroomPrefab;

        [Tooltip("Models of the spore mushrooms to gather: each one that appears draws one of them.")]
        [SerializeField]
        private GameObject[] sporeMushroomPrefabs;

        [Tooltip("Stick lying on the ground: the wood to gather, until a wood mushroom exists.")]
        [SerializeField]
        private GameObject stickPrefab;

        [Tooltip("Trees of the forest's Terrain under which sticks appear.")]
        [SerializeField]
        private GameObject[] stickTreePrefabs;

        [Tooltip("Shader of the outline around what can be gathered (Fungiiiii/ItemOutline).")]
        [SerializeField]
        private Shader outlineShader;

        [Header("Menus")]
        [Tooltip("Font of the pause menu's buttons.")]
        [SerializeField]
        private Font menuFont;

        [Tooltip("Path of the home screen's scene, which the pause menu goes back to.")]
        [SerializeField]
        private string mainMenuScenePath = "Assets/Scenes/Menu/MainMenu.unity";

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

        [Header("Gathering")]
        [SerializeField, Min(0.1f)]
        private float gatherReach = 2f;

        [Tooltip("Seconds between two ticks: on each one, sticks and mushrooms appear on free spots around the players.")]
        [SerializeField, Min(0.1f)]
        private float spawnTickSeconds = 5f;

        [Tooltip("Radius of the bubble around each player where sticks and mushrooms are kept, in metres.")]
        [SerializeField, Min(1f)]
        private float bubbleRadius = 50f;

        [Tooltip("On a tick, nothing appears closer to a player than this, in metres.")]
        [SerializeField, Min(0f)]
        private float spawnMinPlayerDistance = 20f;

        [Tooltip("At the start and after a reset, while the screen is black, the bubble fills from this distance, in metres.")]
        [SerializeField, Min(0f)]
        private float startMinPlayerDistance = 8f;

        [Tooltip("What is further than this from every player goes away, in metres.")]
        [SerializeField, Min(1f)]
        private float despawnDistance = 75f;

        [Tooltip("How many times as likely a spot straight ahead of the player is used as one behind: walking on, one keeps finding some.")]
        [SerializeField, Min(1f)]
        private float spawnAheadWeight = 4f;

        [SerializeField, Min(0)]
        private int sticksNearPlayer = 4;

        [Tooltip("Most sticks on the whole map, all players together.")]
        [SerializeField, Min(0)]
        private int maxSticks = 30;

        [Tooltip("Sticks that appear per tick and per player.")]
        [SerializeField, Min(0)]
        private int sticksPerTick = 1;

        [SerializeField, Min(0)]
        private int sporeMushroomsNearPlayer = 5;

        [Tooltip("Most spore mushrooms on the whole map, all players together.")]
        [SerializeField, Min(0)]
        private int maxSporeMushrooms = 40;

        [Tooltip("Spore mushrooms that appear per tick and per player.")]
        [SerializeField, Min(0)]
        private int sporeMushroomsPerTick = 1;

        [Tooltip("Spore mushrooms at the foot of a tree stand between these two distances from its trunk, in metres.")]
        [SerializeField]
        private Vector2 mushroomTrunkDistance = new Vector2(1f, 2.2f);

        [Tooltip("Spore mushrooms are also scattered over the playable area, this far apart, in metres.")]
        [SerializeField, Min(1f)]
        private float mushroomScatterSpacing = 15f;

        [Tooltip("No spore mushroom on ground steeper than this, in degrees.")]
        [SerializeField, Range(0f, 90f)]
        private float mushroomMaxSlope = 30f;

        [Tooltip("Size of a spore mushroom, drawn between these two scales.")]
        [SerializeField]
        private Vector2 mushroomScale = new Vector2(0.8f, 1.7f);

        [Tooltip("No spore mushroom closer than this to a stick spot, in metres.")]
        [SerializeField, Min(0f)]
        private float mushroomStickClearance = 1.5f;

        [Tooltip("No spore mushroom closer than this to another one, in metres.")]
        [SerializeField, Min(0f)]
        private float mushroomClearance = 1.2f;

        [Tooltip("Sticks lie between these two distances from a trunk, in metres.")]
        [SerializeField]
        private Vector2 stickTrunkDistance = new Vector2(1f, 2.5f);

        [Tooltip("No stick on ground steeper than this, in degrees.")]
        [SerializeField, Range(0f, 90f)]
        private float stickMaxSlope = 30f;

        [Tooltip("Ground rectangle (x, z, width, depth) where sticks appear: the walkable part of the forest.")]
        [SerializeField]
        private Rect playableArea = new Rect(-290f, -290f, 580f, 580f);

        [Header("Outline of what can be gathered")]
        [Tooltip("Palette Body light, a mycelium cream.")]
        [SerializeField]
        private Color outlineColor = new Color32(0xF2, 0xE6, 0xCC, 0xFF);

        [Tooltip("Width in pixels on a 1080p screen.")]
        [SerializeField, Range(0f, 8f)]
        private float outlineWidth = 2.5f;

        [Tooltip("Cyan of what is aimed at, as on the Ramassage artboard.")]
        [SerializeField]
        private Color targetedOutlineColor = new Color32(0x6F, 0xD6, 0xEA, 0xFF);

        [SerializeField, Range(0f, 8f)]
        private float targetedOutlineWidth = 4f;

        [Tooltip("The outline is at full strength up to the first distance and gone at the second, in metres.")]
        [SerializeField]
        private Vector2 outlineFade = new Vector2(16f, 30f);

        [Header("Champimaison")]
        [Tooltip("Off for the playtest: the hole and its fungal network are not built.")]
        [SerializeField]
        private bool buildHole;

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

        // Homes of the mushrooms to catch, in world coordinates of the forest: the start
        // clearing is centred on (0, -235) with a 16 m radius, and the player spawns at its
        // south edge.
        private static readonly Vector2[] SpongeusHomes = { new Vector2(4f, -228f), new Vector2(-7f, -224f) };
        private static readonly Vector2[] ChampouleHomes = { new Vector2(10f, -236f), new Vector2(-3f, -217f) };

        /// <summary>Mushrooms cannot flee out of this ground rectangle (x, z) around the spawn and the hole.</summary>
        private static readonly Rect MushroomArea = new Rect(-30f, -260f, 80f, 65f);

        private readonly List<IInteractable> interactables = new List<IInteractable>();
        private readonly List<Gatherable> gatherables = new List<Gatherable>();
        private readonly List<GatherableField> fields = new List<GatherableField>();
        private readonly List<NetDemoMushroom> mushrooms = new List<NetDemoMushroom>();
        private readonly Dictionary<NetDemoMushroom, ItemDefinition> mushroomItems =
            new Dictionary<NetDemoMushroom, ItemDefinition>();
        private readonly List<Material> materials = new List<Material>();
        private readonly List<GatherableSpot> mushroomSpots = new List<GatherableSpot>();
        private readonly List<GatherableSpot> stickSpots = new List<GatherableSpot>();

        private PlayerControls controls;
        private ItemDefinition woodItem;
        private ItemDefinition sporeItem;
        private ItemDefinition spongeusItem;
        private ItemDefinition champouleItem;
        private ItemDefinition netItem;
        private ScreenStack screens;
        private InputAction menuAction;
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
        private Backpack backpack;
        private PlayerInteractor interactor;
        private FungalHole hole;
        private ItemOutlineFactory outlines;
        private PauseMenu pauseMenu;
        private Hotbar hotbar;
        private PlayerHands hands;
        private NetTargetOutline targetOutline;
        private TerrainData sessionTerrainData;

        /// <summary>True once the forest is loaded and the loop is built.</summary>
        public bool IsReady { get; private set; }

        public PlayerInventory Inventory => backpack != null ? backpack.Inventory : null;

        /// <summary>The inventory screen, opened with Tab.</summary>
        public InventoryDemoController InventoryView => inventoryView;

        public NetThrower Thrower { get; private set; }

        public IReadOnlyList<NetDemoMushroom> Mushrooms => mushrooms;

        /// <summary>Every stick and mushroom built so far, on the map or waiting for a spot.</summary>
        public IReadOnlyList<Gatherable> Gatherables => gatherables;

        /// <summary>Every place a spore mushroom may appear, overlaps removed.</summary>
        public IReadOnlyList<GatherableSpot> MushroomSpots => mushroomSpots;

        /// <summary>Every place a stick may appear.</summary>
        public IReadOnlyList<GatherableSpot> StickSpots => stickSpots;

        /// <summary>Null while the hole is left out (see buildHole).</summary>
        public FungalHole Hole => hole;

        public bool IsInventoryOpen => inventoryView != null && inventoryView.gameObject.activeSelf;

        /// <summary>The screens open over the game: Escape closes the top one, or pauses.</summary>
        public ScreenStack Screens => screens;

        /// <summary>The pause menu, opened with Escape.</summary>
        public PauseMenu Pause => pauseMenu;

        /// <summary>The outline of the mushroom the aimed net would catch.</summary>
        public NetTargetOutline TargetOutline => targetOutline;

        /// <summary>The interact control, and the key it shows above its target.</summary>
        public PlayerInteractor Interactor => interactor;

        /// <summary>The player's first-person camera.</summary>
        public Camera ViewCamera => firstPersonCamera != null ? firstPersonCamera.Camera : null;

        /// <summary>The player's hands, holding the item of the quick bar's selected slot.</summary>
        public PlayerHands Hands => hands;

        /// <summary>The quick bar: the item of its selected slot is the one in hand.</summary>
        public Hotbar Hotbar => hotbar;

        /// <summary>The inventory item a mushroom becomes once caught.</summary>
        public ItemDefinition ItemFor(NetDemoMushroom mushroom) => mushroomItems[mushroom];

        private void OnDestroy()
        {
            if (inventoryAction != null)
            {
                inventoryAction.performed -= OnInventoryPerformed;
                resetAction.performed -= OnResetPerformed;
            }

            if (Thrower != null)
            {
                Thrower.CatchReturned -= HandleCatchReturned;
            }

            if (screens != null)
            {
                screens.Changed -= ApplyScreens;
            }

            if (menuAction != null)
            {
                menuAction.performed -= OnMenuPerformed;
            }

            foreach (var material in materials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }

            outlines?.Dispose();

            if (sessionTerrainData != null)
            {
                Destroy(sessionTerrainData);
            }
        }

        private IEnumerator Start()
        {
            if (playerPrefab == null || spongeusModel == null || champouleModel == null || decorMushroomPrefab == null ||
                sporeMushroomPrefabs == null || sporeMushroomPrefabs.Length == 0 || System.Array.IndexOf(sporeMushroomPrefabs, null) >= 0 ||
                stickPrefab == null || outlineShader == null || stickTreePrefabs == null || stickTreePrefabs.Length == 0 ||
                menuFont == null || itemCatalog == null)
            {
                Debug.LogError("DemoLoopBootstrap is missing one of its assets.", this);
                enabled = false;
                yield break;
            }

            if (!TakeItems())
            {
                enabled = false;
                yield break;
            }

            random =randomSeed != 0 ? new System.Random(randomSeed) : new System.Random();

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

            SpawnPlayer(forestCamera);
            BuildHudAndInventory();

            if (buildHole)
            {
                BuildHole(CreateUnlit("Demo Refused", RefusedColor));
            }

            BuildGatherables(forest);
            SpawnMushrooms();
            SetUpThrower();
            BuildHotbar();

            interactor = player.AddComponent<PlayerInteractor>();
            interactor.Initialize(firstPersonCamera.Camera, Inventory, interactables, AimConeDegrees, menuFont, controls);

            pauseMenu = new GameObject("Pause Menu").AddComponent<PauseMenu>();
            pauseMenu.Initialize(menuFont, mainMenuScenePath);

            screens = new ScreenStack(pauseMenu);
            screens.Changed += ApplyScreens;
            menuAction = controls.Screens.Menu;
            menuAction.performed += OnMenuPerformed;
            inventoryAction = controls.Screens.Inventory;
            inventoryAction.performed += OnInventoryPerformed;
            // Development shortcut, not a player-facing control.
            resetAction = controls.Player.ResetDemo;
            resetAction.performed += OnResetPerformed;

            IsReady = true;
        }

        /// <summary>The demo's items, from the catalog. False, with the reason logged, when the catalog cannot give them.</summary>
        private bool TakeItems()
        {
            var problems = itemCatalog.FindProblems();
            if (problems.Count > 0)
            {
                Debug.LogError($"The item catalog is not sound: {string.Join(" ", problems)}", itemCatalog);
                return false;
            }

            if (!itemCatalog.TryGet(DemoItems.WoodId, out woodItem) ||
                !itemCatalog.TryGet(DemoItems.SporeId, out sporeItem) ||
                !itemCatalog.TryGet(DemoItems.SpongeusId, out spongeusItem) ||
                !itemCatalog.TryGet(DemoItems.ChampouleId, out champouleItem) ||
                !itemCatalog.TryGet(DemoItems.NetId, out netItem))
            {
                Debug.LogError("The item catalog lacks one of the demo's items: wood, spore, spongeus, champoule or net.", itemCatalog);
                return false;
            }

            return true;
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
            controls = player.GetComponent<PlayerControls>();
            backpack = player.GetComponent<Backpack>();

            // First person: the body is not drawn for its own camera, but still casts its shadow.
            foreach (var bodyRenderer in player.GetComponentsInChildren<Renderer>())
            {
                bodyRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }

            firstPersonCamera = forestCamera.gameObject.AddComponent<PrototypeFirstPersonCamera>();
            firstPersonCamera.Initialize(player.GetComponent<PlayerMotor>(), spawnYaw, controls.Player.Look, controls.Player.UsePrimary);
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

            // Built inactive so that the player's inventory is bound before Awake builds the screen.
            var inventoryObject = new GameObject("Inventory");
            inventoryObject.SetActive(false);
            inventoryView = inventoryObject.AddComponent<InventoryDemoController>();
            inventoryView.Bind(backpack.Inventory);
            inventoryView.Font = menuFont;
            inventoryObject.SetActive(true);
            inventoryObject.SetActive(false);
            PutNetInHotbar();
        }

        /// <summary>The net starts in the quick bar's first slot.</summary>
        private void PutNetInHotbar()
        {
            Inventory.AddToSlot(InventoryDemoController.MainSlotCount, netItem, 1);
        }

        /// <summary>The quick bar shows the inventory's own quick bar slots.</summary>
        private void BuildHotbar()
        {
            hotbar = new GameObject("Hotbar").AddComponent<Hotbar>();
            var actions = controls.Player;
            hotbar.Initialize(
                Inventory,
                InventoryDemoController.MainSlotCount,
                InventoryDemoController.HotbarSlotCount,
                menuFont,
                actions.HotbarNext,
                actions.HotbarPrevious,
                new[] { actions.HotbarSlot1, actions.HotbarSlot2, actions.HotbarSlot3 });

            // The hands hold the item of the selected slot: the net does something there.
            hands = player.AddComponent<PlayerHands>();
            hands.Register(netItem, Thrower);
            hands.Initialize(hotbar, controls, firstPersonCamera.transform);
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

        /// <summary>
        /// Sticks and spore mushrooms around the player. Every decor mushroom of the forest
        /// becomes a spot, so none stays that cannot be gathered; the foot of the trees and
        /// a scatter over the forest add more, so that a few are always within reach.
        /// </summary>
        private void BuildGatherables(Scene forest)
        {
            outlines = new ItemOutlineFactory(outlineShader, new ItemOutlineFactory.Settings(
                outlineColor,
                outlineWidth,
                targetedOutlineColor,
                targetedOutlineWidth,
                outlineFade.x,
                Mathf.Max(outlineFade.x, outlineFade.y)));

            TakeHandPlacedMushrooms(forest, mushroomSpots);

            // The mushrooms the Terrain draws are taken out of a copy of its data, made for
            // this session: the forest's own asset is not changed.
            sessionTerrainData = ForestResourceSpots.TakeDecorMushrooms(terrain, decorMushroomPrefab, mushroomSpots);
            if (sessionTerrainData != null)
            {
                terrain.terrainData = sessionTerrainData;
                if (terrain.TryGetComponent<TerrainCollider>(out var terrainCollider))
                {
                    terrainCollider.terrainData = sessionTerrainData;
                }
            }

            ForestResourceSpots.FindStickSpots(
                terrain,
                stickTreePrefabs,
                playableArea,
                Mathf.Min(stickTrunkDistance.x, stickTrunkDistance.y),
                Mathf.Max(stickTrunkDistance.x, stickTrunkDistance.y),
                stickMaxSlope,
                new System.Random(random.Next()),
                stickSpots);

            AddMoreMushroomSpots();
            ForestResourceSpots.RemoveOverlaps(mushroomSpots, stickSpots, mushroomStickClearance, mushroomClearance, IsBlocked);

            var fieldPlayers = new[] { new FieldPlayer(player.transform, firstPersonCamera.Camera) };
            AddField("Spore Mushrooms", sporeItem, sporeMushroomPrefabs, mushroomSpots, sporeMushroomsNearPlayer, maxSporeMushrooms, sporeMushroomsPerTick, fieldPlayers);
            AddField("Sticks", woodItem, new[] { stickPrefab }, stickSpots, sticksNearPlayer, maxSticks, sticksPerTick, fieldPlayers);
        }

        /// <summary>
        /// Spore spots at the foot of the trees and scattered over the forest, out of its
        /// paths: with the decor mushrooms alone, some places had too few within reach.
        /// </summary>
        private void AddMoreMushroomSpots()
        {
            var pose = new MushroomPose(
                Mathf.Min(mushroomScale.x, mushroomScale.y),
                Mathf.Max(mushroomScale.x, mushroomScale.y),
                MushroomSink);

            ForestResourceSpots.FindTreeFootMushroomSpots(
                terrain,
                stickTreePrefabs,
                playableArea,
                Mathf.Min(mushroomTrunkDistance.x, mushroomTrunkDistance.y),
                Mathf.Max(mushroomTrunkDistance.x, mushroomTrunkDistance.y),
                mushroomMaxSlope,
                pose,
                new System.Random(random.Next()),
                mushroomSpots);

            var paths = new List<ExclusionCircle>();
            foreach (var path in FindObjectsByType<ScatterExclusionPath>(FindObjectsSortMode.InstanceID))
            {
                paths.AddRange(path.ToCircles());
            }

            ForestResourceSpots.ScatterMushroomSpots(
                terrain,
                playableArea,
                mushroomScatterSpacing,
                mushroomMaxSlope,
                paths,
                pose,
                new System.Random(random.Next()),
                mushroomSpots);
        }

        /// <summary>Whether a mushroom standing here would go through a trunk or a rock. Triggers are ignored.</summary>
        private static bool IsBlocked(Vector3 groundPoint)
        {
            return Physics.CheckCapsule(
                groundPoint + Vector3.up * ObstacleCapsuleBottom,
                groundPoint + Vector3.up * ObstacleCapsuleTop,
                ObstacleCapsuleRadius,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// Hugo's hand-placed decor mushrooms are scene objects: they are hidden, and their
        /// places become spots like those of the Terrain.
        /// </summary>
        private static void TakeHandPlacedMushrooms(Scene forest, List<GatherableSpot> spots)
        {
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
                        spots.Add(new GatherableSpot(child.position, child.rotation, child.lossyScale));
                        child.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void AddField(
            string fieldName,
            ItemDefinition item,
            GameObject[] variants,
            List<GatherableSpot> spots,
            int nearPlayer,
            int maxAlive,
            int perTick,
            IReadOnlyList<FieldPlayer> fieldPlayers)
        {
            if (spots.Count == 0)
            {
                Debug.LogWarning($"DemoLoopBootstrap found no spot for {fieldName}: none will appear.", this);
            }

            var radius = bubbleRadius;
            var settings = new ResourceSpawner.Settings(
                spawnTickSeconds,
                perTick,
                nearPlayer,
                radius,
                Mathf.Min(spawnMinPlayerDistance, radius),
                Mathf.Min(startMinPlayerDistance, radius),
                Mathf.Max(despawnDistance, radius),
                spawnAheadWeight,
                maxAlive);

            var field = new GameObject(fieldName).AddComponent<GatherableField>();
            field.Initialize(
                item,
                variants,
                spots,
                settings,
                new System.Random(random.Next()),
                fieldPlayers,
                gatherReach,
                outlines,
                Register);
            fields.Add(field);

            void Register(Gatherable gatherable)
            {
                gatherables.Add(gatherable);
                interactables.Add(gatherable);
            }
        }

        private void SpawnMushrooms()
        {
            foreach (var home in SpongeusHomes)
            {
                SpawnMushroom("Spongeus", home, spongeusModel, NetTargetProfile.Common, spongeusItem);
            }

            foreach (var home in ChampouleHomes)
            {
                SpawnMushroom("Champoule", home, champouleModel, NetTargetProfile.Rare, champouleItem);
            }
        }

        private void SpawnMushroom(
            string species,
            Vector2 home,
            GameObject model,
            NetTargetProfile profile,
            ItemDefinition item)
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
            Thrower.Initialize(
                settings,
                firstPersonCamera.Camera,
                playerController,
                mushrooms,
                new System.Random(random.Next()));
            Thrower.CatchReturned += HandleCatchReturned;

            targetOutline = new GameObject("Net Target Outline").AddComponent<NetTargetOutline>();
            targetOutline.Initialize(Thrower, mushrooms, outlines);
        }

        private void HandleCatchReturned(NetDemoMushroom caught)
        {
            // RG_CA_03: the caught mushroom goes into the inventory as it is.
            if (Inventory.Add(mushroomItems[caught], 1) == 0)
            {
                Debug.LogWarning($"The inventory is full: {caught.name} is lost.", this);
            }
        }

        /// <summary>Escape: closes the screen on top, or pauses when none is open.</summary>
        private void OnMenuPerformed(InputAction.CallbackContext _)
        {
            if (IsReady)
            {
                screens.Back();
            }
        }

        /// <summary>Tab: opens the inventory when no screen is open, closes it when it is on top.</summary>
        private void OnInventoryPerformed(InputAction.CallbackContext _)
        {
            if (IsReady)
            {
                screens.Toggle(inventoryView);
            }
        }

        /// <summary>Opens or closes the inventory, as Tab does.</summary>
        public void SetInventoryOpen(bool open)
        {
            if (open)
            {
                screens.Open(inventoryView);
            }
            else
            {
                screens.Close(inventoryView);
            }
        }

        /// <summary>
        /// While a screen is open the mouse is free for it: the view does not turn, and
        /// neither the hands, the quick bar nor the interact control respond. The inventory
        /// shows its own quick bar, so the game's one hides under it.
        /// </summary>
        private void ApplyScreens()
        {
            hotbar.SetVisible(!screens.IsOpen(inventoryView));
            SetGameplayInput(!screens.IsAnyOpen);
        }

        /// <summary>The view, the net, the quick bar and the interact control. Off, the pointer is free for a menu.</summary>
        private void SetGameplayInput(bool enabled)
        {
            controls.SetMenuOpen(!enabled);
            firstPersonCamera.enabled = enabled;
            hands.InputEnabled = enabled;
            hotbar.InputEnabled = enabled;
            interactor.enabled = enabled;
        }

        private void OnResetPerformed(InputAction.CallbackContext _)
        {
            if (IsReady && !screens.IsAnyOpen)
            {
                ResetDemo();
            }
        }

        /// <summary>Everything back to the start: player, gauges, inventory with only the net, resources, mushrooms.</summary>
        public void ResetDemo()
        {
            screens.CloseAll();
            Thrower.ResetNet();

            foreach (var mushroom in mushrooms)
            {
                mushroom.ResetToSpawn();
            }

            // The player goes back first: the fields fill their bubble around where they stand.
            // A CharacterController keeps its own cached position and would snap back to
            // it on the next Move, so it has to be off while the transform is teleported.
            playerController.enabled = false;
            player.transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0f, spawnYaw, 0f));
            playerController.enabled = true;
            firstPersonCamera.SetView(spawnYaw, 0f);

            foreach (var field in fields)
            {
                field.ResetField();
            }

            if (hole != null)
            {
                hole.ResetNetwork();
            }

            EmptyInventory();
            PutNetInHotbar();
            hotbar.Select(0);
            vitals.Reset();
            stamina.Reset();
        }

        private void EmptyInventory()
        {
            inventoryView.Hand.Clear();
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
