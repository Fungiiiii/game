using System.Collections;
using System.Collections.Generic;
using Fungiiiii.Capture;
using Fungiiiii.Champimaison;
using Fungiiiii.Combat;
using Fungiiiii.Companions;
using Fungiiiii.Controls;
using Fungiiiii.Crafting;
using Fungiiiii.Gathering;
using Fungiiiii.Interaction;
using Fungiiiii.Inventory;
using Fungiiiii.Items;
using Fungiiiii.Outlines;
using Fungiiiii.Runtime.Champimaison;
using Fungiiiii.Runtime.Player;
using Fungiiiii.Runtime.World;
using Fungiiiii.Survival;
using Fungiiiii.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Fungiiiii.Game
{
    /// <summary>
    /// Entry point of the game (find the seed, plant the Champimaison, gather, craft,
    /// capture, feed the network), placed
    /// alone in the Game scene. It loads Hugo's MysticForest additively without changing
    /// it: the forest's test player is switched off and its Main Camera, which carries the
    /// zone audio, the spores and the ground mist, becomes the first-person view of the
    /// real Player prefab, used offline as in NetThrowScene.
    ///
    /// The world and the interface are the scene's objects, set in the Inspector: the
    /// forest's sticks and spore mushrooms (<see cref="ForestGathering"/>), the mushrooms
    /// to catch (<see cref="CreaturePopulation"/>), the plants that fill up by chance
    /// (<see cref="RefillingPlants"/>), the companions taken out of the quick bar
    /// (<see cref="CompanionKeeper"/>), the mole holes where the seed is planted
    /// (<see cref="MoleHoles"/>), tuned by data assets, and the screens, quick bar and key
    /// hints, instances of their prefabs. This hands them the player once it is spawned.
    /// </summary>
    internal sealed class GameBootstrap : MonoBehaviour
    {
        private const string ForestScenePath = "Assets/Scenes/Biomes/MysticForest.unity";

        /// <summary>The Player prefab's pivot is the centre of its 2 m capsule.</summary>
        private const float PlayerPivotAboveGround = 1.1f;

        /// <summary>What a felled mushroom lets go of falls from this height, in metres.</summary>
        private const float LootFallHeight = 0.6f;

        [Header("Player")]
        [Tooltip("The player, with its hands, interact control, net and stamina; used offline: no network session is started in this scene.")]
        [SerializeField]
        private GameObject playerPrefab;

        [Tooltip("The item whose behaviour in hand is the capture net.")]
        [SerializeField]
        private ItemDefinition netItem;

        [Tooltip("What the player starts with, in the quick bar's slots from the first. Empty: the player starts empty-handed.")]
        [SerializeField]
        private ItemDefinition[] startingQuickBar = new ItemDefinition[0];

        [SerializeField]
        private StaminaSettings staminaSettings;

        [SerializeField]
        private NetSettings netSettings;

        [Tooltip("The item whose behaviour in hand is the slingshot.")]
        [SerializeField]
        private ItemDefinition slingshotItem;

        [SerializeField]
        private SlingshotSettings slingshotSettings;

        [Header("World")]
        [Tooltip("The sticks and spore mushrooms of the forest.")]
        [SerializeField]
        private ForestGathering gathering;

        [Tooltip("The mushrooms to catch.")]
        [SerializeField]
        private CreaturePopulation creatures;

        [Tooltip("The plants that fill up by chance and that only a companion can empty: the water ferns of the mist.")]
        [SerializeField]
        private RefillingPlants refillingPlants;

        [Tooltip("The caught creatures that can be taken out of the quick bar to follow the player: the Champonge.")]
        [SerializeField]
        private CompanionKeeper companions;

        [Tooltip("The outline around what can be gathered or aimed at.")]
        [SerializeField]
        private OutlineSettings outlineSettings;

        [Header("Interface")]
        [Tooltip("The quick bar: the item of its selected slot is the one in hand.")]
        [SerializeField]
        private Hotbar hotbar;

        [SerializeField]
        private GameScreens screens;

        [Tooltip("The key shown over what the player aims at.")]
        [SerializeField]
        private KeyPrompt keyPrompt;

        [Tooltip("The name of the creature the player looks at, above it.")]
        [SerializeField]
        private CreatureNameTag nameTag;

        [Tooltip("The crouch key by a full creature looked at, until the first squeeze.")]
        [SerializeField]
        private SneakHint sneakHint;

        [Tooltip("Top centre: the cardinal points and a yellow mark where the objective lies.")]
        [SerializeField]
        private Fungiiiii.UI.Compass compass;

        [Tooltip("Health and stamina, top left.")]
        [SerializeField]
        private VitalGauges vitalGauges;

        [Tooltip("What the player just collected, above the quick bar.")]
        [SerializeField]
        private PickupFeed pickupFeed;

        [Tooltip("Top right: the objective and the recipes tracked.")]
        [SerializeField]
        private ObjectiveHud objectiveHud;

        [Tooltip("What the Champimaison's next stage costs, over it.")]
        [SerializeField]
        private RequirementPanel requirementPanel;

        [Tooltip("What the player drops from the inventory, until picked up again.")]
        [SerializeField]
        private DroppedItems droppedItems;

        [Tooltip("The mole holes where the seed is planted, and the Champimaison it grows into.")]
        [SerializeField]
        private MoleHoles moleHoles;

        [Tooltip("Where the seed waits at the start: at the foot of the giant tree.")]
        [SerializeField]
        private Transform seedSpot;

        [Tooltip("The light violet glow that draws the eye to the seed waiting at the giant tree; it goes with the seed.")]
        [SerializeField]
        private GameObject seedGlow;

        [Tooltip("The opening shot: the seed falling from the giant tree.")]
        [SerializeField]
        private SeedFallIntro intro;

        [Tooltip("Inside the Champimaison: the end of the demo. Loaded by path, so it must be in the build settings.")]
        [SerializeField]
        private string interiorScenePath = "Assets/Scenes/Game/ChampimaisonInterior.unity";

        [Tooltip("Seed of every random draw of the game. 0 picks a different seed every run.")]
        [SerializeField]
        private int randomSeed;

        private readonly List<IInteractable> interactables = new List<IInteractable>();

        private PlayerControls controls;
        private System.Random random;
        private Terrain terrain;
        private Vector3 spawnPosition;
        private float spawnYaw;
        private GameObject player;
        private CharacterController playerController;
        private FirstPersonCamera firstPersonCamera;
        private PlayerVitals vitals;
        private StaminaRegulator stamina;
        private Backpack backpack;
        private PlayerInteractor interactor;
        private ItemOutlineFactory outlines;
        private PlayerHands hands;
        private NetTargetOutline targetOutline;
        private SessionTerrain sessionTerrain;
        private GameObjectives objectives;
        private ObjectiveTarget objectiveTarget;

        /// <summary>True once the forest is loaded and the loop is built.</summary>
        public bool IsReady { get; private set; }

        public PlayerInventory Inventory => backpack != null ? backpack.Inventory : null;

        /// <summary>The inventory screen, opened with Tab.</summary>
        public InventoryScreen InventoryView => screens.Inventory;

        public NetThrower Thrower { get; private set; }

        /// <summary>The item whose behaviour in hand is the net.</summary>
        public ItemDefinition NetItem => netItem;

        public Slingshot Slingshot { get; private set; }

        public IReadOnlyList<WildMushroom> Mushrooms => creatures.Mushrooms;

        /// <summary>Every stick and mushroom built so far, on the map or waiting for a spot.</summary>
        public IReadOnlyList<Gatherable> Gatherables => gathering.Gatherables;

        /// <summary>Every place a spore mushroom may appear, overlaps removed.</summary>
        public IReadOnlyList<GatherableSpot> MushroomSpots => gathering.MushroomSpots;

        /// <summary>Every place a stick may appear.</summary>
        public IReadOnlyList<GatherableSpot> StickSpots => gathering.StickSpots;

        /// <summary>The planted Champimaison; null until the seed is planted.</summary>
        public FungalHole Hole => moleHoles.Champimaison;

        public MoleHoles MoleHoles => moleHoles;

        /// <summary>The recipes the player follows.</summary>
        public RecipeTracker Tracker { get; } = new RecipeTracker();

        public Objective CurrentObjective => objectives.Current;

        public ObjectiveHud ObjectiveHud => objectiveHud;

        public CraftingPanel Crafting => screens.Inventory.Crafting;

        public bool IsInventoryOpen => screens.IsInventoryOpen;

        /// <summary>The key hints, bottom right of the screen.</summary>
        public ControlHintsPanel Hints => screens.Hints;

        /// <summary>The player's controls: whether a menu has them, the device in use.</summary>
        public PlayerControls Controls => controls;

        /// <summary>The opening shot, played once the game is ready.</summary>
        public SeedFallIntro Intro => intro;

        /// <summary>The screens open over the game: Escape closes the top one, or pauses.</summary>
        public ScreenStack Screens => screens.Stack;

        /// <summary>The pause menu, opened with Escape.</summary>
        public PauseMenu Pause => screens.Pause;

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

        /// <summary>What the player just collected, above the quick bar.</summary>
        public PickupFeed Pickups => pickupFeed;

        /// <summary>What the player dropped from the inventory and has not picked up again.</summary>
        public DroppedItems Dropped => droppedItems;

        /// <summary>The water ferns of the mist, and any other plant that fills up by chance.</summary>
        public RefillingPlants RefillingPlants => refillingPlants;

        /// <summary>The companion out of the inventory, following the player.</summary>
        public CompanionKeeper Companions => companions;

        public CreatureNameTag NameTag => nameTag;

        public SneakHint SneakHint => sneakHint;

        public Fungiiiii.UI.Compass Compass => compass;

        /// <summary>Where the compass's mark points now, or null.</summary>
        public Vector3? ObjectiveTargetPoint => objectiveTarget?.Current();

        /// <summary>The inventory item a mushroom becomes once caught.</summary>
        public ItemDefinition ItemFor(WildMushroom mushroom) => creatures.ItemFor(mushroom);

        private void OnDestroy()
        {
            if (Thrower != null)
            {
                Thrower.CatchReturned -= HandleCatchReturned;
            }

            outlines?.Dispose();
            if (objectives != null)
            {
                objectives.Changed -= objectiveHud.ShowObjective;
                objectives.Dispose();
            }

            sessionTerrain?.Dispose();

            if (gathering != null)
            {
                gathering.GatherableCreated -= interactables.Add;
            }

            if (moleHoles != null)
            {
                moleHoles.Planted -= HandlePlanted;
            }
        }

        private IEnumerator Start()
        {
            var problem = FindProblem();
            if (problem != null)
            {
                Debug.LogError($"GameBootstrap cannot start: {problem}", this);
                enabled = false;
                yield break;
            }

            random = randomSeed != 0 ? new System.Random(randomSeed) : new System.Random();

            yield return LoadForest();

            var forest = SceneManager.GetSceneByPath(ForestScenePath);
            if (!forest.IsValid() || !forest.isLoaded)
            {
                Debug.LogError($"GameBootstrap could not load {ForestScenePath}.", this);
                enabled = false;
                yield break;
            }

            // The forest's fog, sky and ambient light belong to it: it has to be the active scene.
            SceneManager.SetActiveScene(forest);
            terrain = Terrain.activeTerrain;

            var forestCamera = TakeOverForestPlayer();
            if (forestCamera == null)
            {
                Debug.LogError("GameBootstrap found no FirstPersonTestPlayer with a camera in the forest.", this);
                enabled = false;
                yield break;
            }

            SpawnPlayer(forestCamera);
            SetUpVitals();
            screens.Inventory.Bind(backpack.Inventory, backpack.Layout);
            PutStartingItems();

            BuildGatherables(forest);
            pickupFeed.Bind(Inventory);
            droppedItems.Bind(
                screens.Inventory,
                firstPersonCamera.transform,
                GroundHeight,
                outlines,
                interactables,
                new System.Random(random.Next()));
            var seed = droppedItems.Place(moleHoles.Seed, 1, seedSpot.position);
            var playerStart = player.transform.position;
            Instantiate(seedGlow, seed.transform, false);
            // The ferns first: the wild Champonge go and drink at them.
            refillingPlants.Populate(forest, sessionTerrain, outlines, interactables, new System.Random(random.Next()));
            SpawnMushrooms();
            SetUpThrower();
            SetUpSlingshot();
            SetUpHands();
            moleHoles.Populate(forest, GroundHeight, outlines, interactables, hotbar, Inventory, new System.Random(random.Next()));
            moleHoles.Planted += HandlePlanted;
            screens.Inventory.Crafting.Bind(Inventory, Tracker, controls, screens.Inventory);
            objectives = new GameObjectives(Inventory, moleHoles);
            objectiveHud.Bind(() => ObjectiveRules.KeyFor(objectives.Current), Tracker, Inventory);
            objectiveTarget = new ObjectiveTarget(objectives, player.transform, seed.transform, moleHoles, refillingPlants.Plants, Inventory);
            compass.Bind(firstPersonCamera, objectiveTarget.Current);
            objectives.Changed += objectiveHud.ShowObjective;

            interactor = player.GetComponent<PlayerInteractor>();
            interactor.Initialize(firstPersonCamera, Inventory, interactables, keyPrompt, controls);

            nameTag.Bind(firstPersonCamera, creatures.Mushrooms, creatures.ItemFor, companions);
            sneakHint.Bind(firstPersonCamera, creatures, player.GetComponent<PlayerMotor>(), controls);
            screens.Bind(controls, hands);
            IsReady = true;
            intro.Play(firstPersonCamera.Camera, firstPersonCamera, controls, seed.transform, playerStart, GroundHeight);
        }

        /// <summary>What is missing for the game to start, or null when nothing is.</summary>
        private string FindProblem()
        {
            if (playerPrefab == null || netItem == null || staminaSettings == null || netSettings == null || slingshotItem == null || slingshotSettings == null)
            {
                return "the player prefab, the net or slingshot item, or the stamina, net or slingshot settings are missing.";
            }

            if (playerPrefab.GetComponent<PlayerHands>() == null
                || playerPrefab.GetComponent<PlayerInteractor>() == null
                || playerPrefab.GetComponent<NetThrower>() == null
                || playerPrefab.GetComponent<NetTargetOutline>() == null
                || playerPrefab.GetComponent<Slingshot>() == null
                || playerPrefab.GetComponent<StaminaDriver>() == null)
            {
                return "the player prefab lacks its hands, interact control, net, net target outline, slingshot or stamina.";
            }

            if (screens == null || hotbar == null || keyPrompt == null || vitalGauges == null || pickupFeed == null || droppedItems == null || requirementPanel == null || objectiveHud == null)
            {
                return "the screens, the quick bar, the key prompt, the vital gauges, the pickup feed, the dropped items, the requirement panel or the objective are missing.";
            }

            if (nameTag == null || compass == null || sneakHint == null)
            {
                return "the creature name tag, the compass or the sneak hint is missing.";
            }

            if (screens.Inventory == null || screens.Inventory.Crafting == null)
            {
                return "the inventory has no crafting panel.";
            }

            if (System.Array.IndexOf(startingQuickBar, null) >= 0)
            {
                return "the starting quick bar has an empty slot.";
            }

            if (outlineSettings == null || outlineSettings.Shader == null)
            {
                return "the outline settings or their shader are missing.";
            }

            if (gathering == null || creatures == null || refillingPlants == null || companions == null || moleHoles == null || seedSpot == null || seedGlow == null || intro == null)
            {
                return "the forest's gathering, its creatures, its refilling plants, the companions, the mole holes, the seed spot, its glow or the opening shot are missing.";
            }

            return screens.FindProblem()
                ?? screens.Inventory.Crafting.FindProblem()
                ?? gathering.FindProblem()
                ?? creatures.FindProblem()
                ?? refillingPlants.FindProblem()
                ?? intro.FindProblem()
                ?? droppedItems.FindProblem()
                ?? nameTag.FindProblem()
                ?? compass.FindProblem()
                ?? sneakHint.FindProblem()
                ?? companions.FindProblem()
                ?? moleHoles.FindProblem();
        }

        private static IEnumerator LoadForest()
        {
            if (SceneManager.GetSceneByPath(ForestScenePath).isLoaded)
            {
                yield break;
            }

#if UNITY_EDITOR
            // Played from the Editor: the forest need not be in the build settings.
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

            firstPersonCamera = forestCamera.gameObject.AddComponent<FirstPersonCamera>();
            firstPersonCamera.Initialize(player.GetComponent<PlayerMotor>(), spawnYaw, controls.Player.Look, controls.Player.UsePrimary, controls);
        }

        private void SetUpVitals()
        {
            vitals = new PlayerVitals(100f, 100f);
            stamina = new StaminaRegulator(vitals, staminaSettings.ToRegulatorSettings());
            vitalGauges.Initialize(vitals);
            player.GetComponent<StaminaDriver>().Initialize(player.GetComponent<PlayerMotor>(), stamina);
        }

        /// <summary>The starting items, in the quick bar's slots from the first: the net.</summary>
        private void PutStartingItems()
        {
            for (var position = 0; position < startingQuickBar.Length; position++)
            {
                Inventory.AddToSlot(backpack.Layout.QuickBarSlot(position), startingQuickBar[position], 1);
            }
        }

        /// <summary>
        /// The quick bar shows the inventory's own quick bar slots; the hands hold the item of
        /// its selected slot: the net throws, a companion's item takes the companion out.
        /// </summary>
        private void SetUpHands()
        {
            var actions = controls.Player;
            hotbar.Initialize(
                Inventory,
                backpack.Layout.QuickBarFirstSlot,
                backpack.Layout.QuickBarSlotCount,
                actions.HotbarNext,
                actions.HotbarPrevious,
                new[] { actions.HotbarSlot1, actions.HotbarSlot2, actions.HotbarSlot3 });

            companions.Bind(
                player.transform,
                firstPersonCamera.transform,
                GroundHeight,
                Inventory,
                hotbar,
                outlines,
                interactables,
                refillingPlants.Absorbable);

            hands = player.GetComponent<PlayerHands>();
            hands.Register(netItem, Thrower);
            hands.Register(slingshotItem, Slingshot);
            foreach (var (item, behaviour) in companions.HeldBehaviours)
            {
                hands.Register(item, behaviour);
            }

            hands.Initialize(hotbar, controls, firstPersonCamera.transform);
        }

        /// <summary>The forest's sticks and spore mushrooms, kept around the player; each one can be gathered with E.</summary>
        private void BuildGatherables(Scene forest)
        {
            outlines = new ItemOutlineFactory(outlineSettings.Shader, outlineSettings.ToFactorySettings());
            // What the game takes out of the forest's Terrain is taken out of one copy, for this session.
            sessionTerrain = new SessionTerrain(terrain);
            gathering.GatherableCreated += interactables.Add;
            var fieldPlayers = new[] { new FieldPlayer(player.transform, firstPersonCamera.Camera) };
            gathering.Populate(forest, sessionTerrain, fieldPlayers, outlines, new System.Random(random.Next()));
        }

        private void SpawnMushrooms()
        {
            var motor = player.GetComponent<PlayerMotor>();
            creatures.Populate(
                player.transform,
                firstPersonCamera.transform,
                GroundHeight,
                new System.Random(random.Next()),
                refillingPlants.Absorbable,
                refillingPlants.FirstMet,
                (item, point) => droppedItems.Place(item, 1, point, LootFallHeight),
                () => motor.IsCrouching,
                interactables);
        }

        private void SetUpThrower()
        {
            Thrower = player.GetComponent<NetThrower>();
            Thrower.Initialize(
                netSettings.ToThrowSettings(),
                firstPersonCamera.Camera,
                playerController,
                creatures.Mushrooms,
                new System.Random(random.Next()));
            Thrower.CatchReturned += HandleCatchReturned;

            targetOutline = player.GetComponent<NetTargetOutline>();
            targetOutline.Initialize(Thrower, creatures.Mushrooms, outlines);
        }

        private void SetUpSlingshot()
        {
            Slingshot = player.GetComponent<Slingshot>();
            Slingshot.Initialize(
                slingshotSettings,
                firstPersonCamera.Camera,
                playerController,
                creatures.Mushrooms,
                Inventory,
                gathering.Pebbles.Item);
        }

        private void HandlePlanted(FungalHole champimaison)
        {
            requirementPanel.Bind(champimaison, Inventory, firstPersonCamera.Camera, controls, interactor);
            objectiveHud.ShowNeedsOf(champimaison.Network);
            objectiveHud.SuggestCapture(netItem, System.Linq.Enumerable.Select(companions.HeldBehaviours, held => held.Item));
            champimaison.Entered += EnterChampimaison;
        }

        /// <summary>The last stage opens the Champimaison: entering it is the end of the demo, in its own scene.</summary>
        private void EnterChampimaison()
        {
            if (Hole != null)
            {
                Hole.Entered -= EnterChampimaison;
            }

            StartCoroutine(LoadInterior());
        }

        private IEnumerator LoadInterior()
        {
#if UNITY_EDITOR
            // Played from the Editor: the scene need not be in the build settings.
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                interiorScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync(interiorScenePath, LoadSceneMode.Single);
#endif
        }

        private void HandleCatchReturned(WildMushroom caught)
        {
            // RG_CA_03: the caught mushroom goes into the inventory as it is.
            if (Inventory.Collect(creatures.ItemFor(caught), 1) == 0)
            {
                Debug.LogWarning($"The inventory is full: {caught.name} is lost.", this);
            }
        }

        /// <summary>Opens or closes the inventory, as Tab does.</summary>
        public void SetInventoryOpen(bool open)
        {
            screens.SetInventoryOpen(open);
        }

        private float GroundHeight(Vector3 point)
        {
            return terrain != null ? terrain.SampleHeight(point) + terrain.GetPosition().y : point.y;
        }
    }
}
