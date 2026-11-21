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
    /// The world and the interface are the scene's objects, set in the Inspector: the
    /// forest's sticks and spore mushrooms (<see cref="ForestGathering"/>), the mushrooms
    /// to catch (<see cref="CreaturePopulation"/>), tuned by data assets, and the screens,
    /// quick bar and key hints, instances of their prefabs. This hands them the player
    /// once it is spawned. The Champimaison hole is left out of the playtest.
    /// </summary>
    internal sealed class DemoLoopBootstrap : MonoBehaviour
    {
        private const string ForestScenePath = "Assets/Scenes/Biomes/MysticForest.unity";

        /// <summary>The Player prefab's pivot is the centre of its 2 m capsule.</summary>
        private const float PlayerPivotAboveGround = 1.1f;

        private const float AimConeDegrees = 25f;

        private static readonly Color RefusedColor = new Color(1f, 0.45f, 0.1f);

        /// <summary>How close the player must be to use the Champimaison hole, in metres.</summary>
        private const float HoleReach = 2f;

        [Header("Player")]
        [Tooltip("The player prefab, used offline: no network session is started in this scene.")]
        [SerializeField]
        private GameObject playerPrefab;

        [Tooltip("The item whose behaviour in hand is the capture net.")]
        [SerializeField]
        private ItemDefinition netItem;

        [Tooltip("What the player starts with, in the quick bar's slots from the first.")]
        [SerializeField]
        private ItemDefinition[] startingQuickBar = new ItemDefinition[0];

        [SerializeField]
        private StaminaSettings staminaSettings;

        [SerializeField]
        private NetSettings netSettings;

        [Header("World")]
        [Tooltip("The sticks and spore mushrooms of the forest.")]
        [SerializeField]
        private ForestGathering gathering;

        [Tooltip("The mushrooms to catch.")]
        [SerializeField]
        private CreaturePopulation creatures;

        [Tooltip("The outline around what can be gathered or aimed at.")]
        [SerializeField]
        private OutlineSettings outlineSettings;

        [Header("Interface")]
        [SerializeField]
        private Hotbar hotbar;

        [SerializeField]
        private InventoryScreen inventoryView;

        [SerializeField]
        private PauseMenu pauseMenu;

        [SerializeField]
        private ControlHintsPanel hintsPanel;

        [Tooltip("The key shown over what the player aims at.")]
        [SerializeField]
        private KeyPrompt keyPrompt;

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

        [Tooltip("Seed of every random draw of the demo. 0 picks a different seed every run.")]
        [SerializeField]
        private int randomSeed;

        private readonly List<IInteractable> interactables = new List<IInteractable>();
        private readonly List<Material> materials = new List<Material>();

        private PlayerControls controls;
        private ScreenStack screens;
        private FixedHints gameHints;
        private InputAction menuAction;
        private InputAction cancelAction;
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
        private Backpack backpack;
        private PlayerInteractor interactor;
        private FungalHole hole;
        private ItemOutlineFactory outlines;
        private PlayerHands hands;
        private NetTargetOutline targetOutline;

        /// <summary>True once the forest is loaded and the loop is built.</summary>
        public bool IsReady { get; private set; }

        public PlayerInventory Inventory => backpack != null ? backpack.Inventory : null;

        /// <summary>The inventory screen, opened with Tab.</summary>
        public InventoryScreen InventoryView => inventoryView;

        public NetThrower Thrower { get; private set; }

        public IReadOnlyList<NetDemoMushroom> Mushrooms => creatures.Mushrooms;

        /// <summary>Every stick and mushroom built so far, on the map or waiting for a spot.</summary>
        public IReadOnlyList<Gatherable> Gatherables => gathering.Gatherables;

        /// <summary>Every place a spore mushroom may appear, overlaps removed.</summary>
        public IReadOnlyList<GatherableSpot> MushroomSpots => gathering.MushroomSpots;

        /// <summary>Every place a stick may appear.</summary>
        public IReadOnlyList<GatherableSpot> StickSpots => gathering.StickSpots;

        /// <summary>Null while the hole is left out (see buildHole).</summary>
        public FungalHole Hole => hole;

        public bool IsInventoryOpen => inventoryView != null && inventoryView.gameObject.activeSelf;

        /// <summary>The key hints, bottom right of the screen.</summary>
        public ControlHintsPanel Hints => hintsPanel;

        /// <summary>The player's controls: whether a menu has them, the device in use.</summary>
        public PlayerControls Controls => controls;

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
        public ItemDefinition ItemFor(NetDemoMushroom mushroom) => creatures.ItemFor(mushroom);

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
                cancelAction.performed -= OnMenuPerformed;
            }

            foreach (var material in materials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }

            outlines?.Dispose();

            if (gathering != null)
            {
                gathering.GatherableCreated -= interactables.Add;
            }
        }

        private IEnumerator Start()
        {
            var problem = FindProblem();
            if (problem != null)
            {
                Debug.LogError($"DemoLoopBootstrap cannot start: {problem}", this);
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
            interactor.Initialize(firstPersonCamera.Camera, Inventory, interactables, AimConeDegrees, keyPrompt, controls);

            screens = new ScreenStack(pauseMenu);
            screens.Changed += ApplyScreens;

            BuildHints();
            menuAction = controls.Screens.Menu;
            menuAction.performed += OnMenuPerformed;
            // B on a gamepad goes back too; it only acts in a screen, where the UI map is on.
            cancelAction = controls.UI.Cancel;
            cancelAction.performed += OnMenuPerformed;
            inventoryAction = controls.Screens.Inventory;
            inventoryAction.performed += OnInventoryPerformed;
            // Development shortcut, not a player-facing control.
            resetAction = controls.Player.ResetDemo;
            resetAction.performed += OnResetPerformed;

            IsReady = true;
        }

        /// <summary>What is missing for the demo to start, or null when nothing is.</summary>
        private string FindProblem()
        {
            if (playerPrefab == null || netItem == null || staminaSettings == null || netSettings == null)
            {
                return "the player prefab, the net item, or the stamina or net settings are missing.";
            }

            if (hotbar == null || inventoryView == null || pauseMenu == null || hintsPanel == null || keyPrompt == null)
            {
                return "a piece of the interface is missing: quick bar, inventory, pause menu, key hints or key prompt.";
            }

            if (startingQuickBar.Length == 0 || System.Array.IndexOf(startingQuickBar, null) >= 0)
            {
                return "the starting quick bar is empty or has an empty slot.";
            }

            if (outlineSettings == null || outlineSettings.Shader == null)
            {
                return "the outline settings or their shader are missing.";
            }

            if (gathering == null || creatures == null)
            {
                return "the forest's gathering or its creatures are missing.";
            }

            return gathering.FindProblem() ?? creatures.FindProblem();
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
            firstPersonCamera.Initialize(player.GetComponent<PlayerMotor>(), spawnYaw, controls.Player.Look, controls.Player.UsePrimary, controls);
        }

        private void BuildHudAndInventory()
        {
            vitals = new PlayerVitals(100f, 100f);
            stamina = new StaminaRegulator(vitals, staminaSettings.ToRegulatorSettings());

            var hud = new GameObject("Vitals HUD").AddComponent<VitalsHud>();
            hud.Initialize(vitals);

            player.AddComponent<StaminaDriver>().Initialize(player.GetComponent<PlayerMotor>(), stamina);

            inventoryView.Bind(backpack.Inventory, backpack.Layout);
            inventoryView.SetControls(new InventoryScreenControls(
                controls.UI.Navigate,
                controls.UI.TakeHalf,
                controls.UI.GatherAll,
                controls.Screens.Inventory,
                controls.UI.Cancel));
            PutStartingItems();
        }

        /// <summary>The starting items, in the quick bar's slots from the first: the net.</summary>
        private void PutStartingItems()
        {
            for (var position = 0; position < startingQuickBar.Length; position++)
            {
                Inventory.AddToSlot(backpack.Layout.QuickBarSlot(position), startingQuickBar[position], 1);
            }
        }

        /// <summary>The quick bar shows the inventory's own quick bar slots.</summary>
        private void BuildHotbar()
        {
            var actions = controls.Player;
            hotbar.Initialize(
                Inventory,
                backpack.Layout.QuickBarFirstSlot,
                backpack.Layout.QuickBarSlotCount,
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
                gathering.Sticks.Item.Id,
                gathering.SporeMushrooms.Item.Id);

            var holeObject = new GameObject("Champimaison Hole");
            holeObject.transform.position = centre;
            hole = holeObject.AddComponent<FungalHole>();
            // Lit filaments take the forest's light and fog, like the ground they lie on.
            var filaments = CreateLit("Fungal Network", networkColor);
            var holeRing = CreateUnlit("Champimaison Hole Ring", networkColor);
            hole.Initialize(network, HoleReach, filaments, holeRing, refused, GroundHeight);
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

        /// <summary>The forest's sticks and spore mushrooms, kept around the player; each one can be gathered with E.</summary>
        private void BuildGatherables(Scene forest)
        {
            outlines = new ItemOutlineFactory(outlineSettings.Shader, outlineSettings.ToFactorySettings());
            gathering.GatherableCreated += interactables.Add;
            var fieldPlayers = new[] { new FieldPlayer(player.transform, firstPersonCamera.Camera) };
            gathering.Populate(forest, terrain, fieldPlayers, outlines, new System.Random(random.Next()));
        }

        private void SpawnMushrooms()
        {
            creatures.Populate(player.transform, GroundHeight, new System.Random(random.Next()));
        }

        private void SetUpThrower()
        {
            Thrower = player.AddComponent<NetThrower>();
            Thrower.Initialize(
                netSettings.ToThrowSettings(),
                firstPersonCamera.Camera,
                playerController,
                creatures.Mushrooms,
                new System.Random(random.Next()));
            Thrower.CatchReturned += HandleCatchReturned;

            targetOutline = new GameObject("Net Target Outline").AddComponent<NetTargetOutline>();
            targetOutline.Initialize(Thrower, creatures.Mushrooms, outlines);
        }

        private void HandleCatchReturned(NetDemoMushroom caught)
        {
            // RG_CA_03: the caught mushroom goes into the inventory as it is.
            if (Inventory.Add(creatures.ItemFor(caught), 1) == 0)
            {
                Debug.LogWarning($"The inventory is full: {caught.name} is lost.", this);
            }
        }

        /// <summary>Escape or B: closes the screen on top; Escape pauses when none is open.</summary>
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
            // Every system that stops under a menu listens to the controls itself.
            controls.SetMenuOpen(screens.IsAnyOpen);
            ShowHints();
        }

        /// <summary>
        /// The key hints, bottom right. In game: what E does with the target, what the item
        /// in hand does, then switching items and the inventory; switching is left out first
        /// when there are too many.
        /// </summary>
        private void BuildHints()
        {
            var actions = controls.Player;
            gameHints = new FixedHints(
                new ControlHint(UiText.HintSwitchItem, HintInput.Of(actions.HotbarNext, actions.HotbarPrevious), optional: true),
                new ControlHint(UiText.HintInventory, HintInput.Of(controls.Screens.Inventory)));

            hintsPanel.Initialize(controls);
            ShowHints();
        }

        /// <summary>The hints of the screen on top, or of the game when none is open; the pause shows none.</summary>
        private void ShowHints()
        {
            if (!screens.IsAnyOpen)
            {
                hintsPanel.ShowFrom(interactor, hands, gameHints);
            }
            else if (screens.Top is IHintSource screenHints)
            {
                hintsPanel.ShowFrom(screenHints);
            }
            else
            {
                hintsPanel.ShowFrom();
            }
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

            creatures.ResetAll();

            // The player goes back first: the fields fill their bubble around where they stand.
            // A CharacterController keeps its own cached position and would snap back to
            // it on the next Move, so it has to be off while the transform is teleported.
            playerController.enabled = false;
            player.transform.SetPositionAndRotation(spawnPosition, Quaternion.Euler(0f, spawnYaw, 0f));
            playerController.enabled = true;
            firstPersonCamera.SetView(spawnYaw, 0f);

            gathering.ResetFields();

            if (hole != null)
            {
                hole.ResetNetwork();
            }

            EmptyInventory();
            PutStartingItems();
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
