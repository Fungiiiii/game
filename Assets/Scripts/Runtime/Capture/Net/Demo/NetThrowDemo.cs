using System.Collections.Generic;
using Fungiiiii.Runtime.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// Builds the capture net prototype at runtime from the bootstrap object placed in
    /// NetThrowScene, which is its only entry point: an arena of primitives, the real
    /// player (<see cref="PlayerMotor"/>) seen in first person, and three mushrooms with
    /// fixed profiles (common, common weakened, rare).
    ///
    /// The player prefab is the networked one, used offline: no NetworkManager is created,
    /// so nothing is spawned and the motor and input reader stay enabled, as in the prefab.
    /// </summary>
    internal sealed class NetThrowDemo : MonoBehaviour
    {
        private const float ArenaHalfSize = 18f;
        private const float SpawnYawDegrees = 0f;

        private static readonly Vector3 PlayerSpawnPosition = new Vector3(0f, 1.1f, -6f);

        private static readonly Vector3[] ObstaclePositions =
        {
            new Vector3(-5f, 0.75f, 4f),
            new Vector3(6f, 0.75f, 7f),
            new Vector3(-2f, 0.75f, 13f),
            new Vector3(9f, 0.75f, -2f)
        };

        [Tooltip("The player prefab, used offline: no network session is started in this scene.")]
        [SerializeField]
        private GameObject playerPrefab;

        [Header("Net — starting values of the design note, to balance in playtest")]
        [SerializeField, Min(0.1f)]
        private float launchSpeed = 11f;

        [Tooltip("Downward acceleration of the net, in m/s². Tuned by feel.")]
        [SerializeField, Min(0f)]
        private float netGravity = 9.81f;

        [SerializeField, Min(0.1f)]
        private float impactRadius = 1.5f;

        [Tooltip("After a miss, mushrooms this close to the impact flee.")]
        [SerializeField, Min(0f)]
        private float scareRadius = 3f;

        [Tooltip("Freeze of the net and its target when it lands on a mushroom.")]
        [SerializeField, Min(0f)]
        private float holdSeconds = 0.5f;

        [Tooltip("Return of the net to the hand. This is the recharge.")]
        [SerializeField, Min(0.1f)]
        private float returnSeconds = 1f;

        [Tooltip("PROVISIONAL: replaced by the CaptureSystem's TryCapture.")]
        [SerializeField]
        private NetCaptureRates captureRates = NetCaptureRates.Default;

        [Tooltip("Seed of the capture roll. 0 picks a different seed every run.")]
        [SerializeField]
        private int randomSeed;

        [Header("Mushrooms")]
        [SerializeField, Min(0f)]
        private float mushroomFleeSpeed = 4f;

        private readonly List<NetDemoMushroom> mushrooms = new List<NetDemoMushroom>();
        private readonly List<Material> sceneMaterials = new List<Material>();

        private InputAction resetAction;
        private GameObject player;
        private CharacterController playerController;
        private PrototypeFirstPersonCamera firstPersonCamera;

        public IReadOnlyList<NetDemoMushroom> Mushrooms => mushrooms;

        public NetThrower Thrower { get; private set; }

        private void Awake()
        {
            // Development shortcut for this prototype, not a player-facing control.
            resetAction = new InputAction("ResetNetPrototype", InputActionType.Button);
            resetAction.AddBinding("<Keyboard>/r");
            resetAction.AddBinding("<Gamepad>/start");
            resetAction.performed += OnResetPerformed;
        }

        private void OnEnable()
        {
            resetAction.Enable();
        }

        private void OnDisable()
        {
            resetAction.Disable();
        }

        private void OnDestroy()
        {
            resetAction.performed -= OnResetPerformed;
            resetAction.Dispose();

            foreach (var material in sceneMaterials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
        }

        private void Start()
        {
            if (playerPrefab == null)
            {
                Debug.LogError("NetThrowDemo needs the player prefab.", this);
                enabled = false;
                return;
            }

            BuildArena();
            SpawnPlayer();
            SpawnMushrooms();
            SetUpThrower();
        }

        private void BuildArena()
        {
            var root = transform;
            var groundMaterial = SceneMaterial("Net Prototype Ground", new Color(0.18f, 0.28f, 0.2f));
            var obstacleMaterial = SceneMaterial("Net Prototype Obstacle", new Color(0.35f, 0.3f, 0.26f));

            var ground = NetDemoPrimitives.Create(
                PrimitiveType.Cube,
                "Ground",
                root,
                new Vector3(0f, -0.5f, 0f),
                new Vector3(ArenaHalfSize * 2f + 4f, 1f, ArenaHalfSize * 2f + 4f),
                groundMaterial,
                keepCollider: true);
            ground.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;

            foreach (var position in ObstaclePositions)
            {
                NetDemoPrimitives.Create(
                    PrimitiveType.Cube,
                    "Obstacle",
                    root,
                    position,
                    new Vector3(2f, 1.5f, 2f),
                    obstacleMaterial,
                    keepCollider: true);
            }

            // Edge walls keep the player on the ground; mushrooms are clamped inside instead.
            var edge = ArenaHalfSize + 1.5f;
            CreateWall(root, new Vector3(0f, 1f, edge), new Vector3(edge * 2f, 2f, 0.5f), obstacleMaterial);
            CreateWall(root, new Vector3(0f, 1f, -edge), new Vector3(edge * 2f, 2f, 0.5f), obstacleMaterial);
            CreateWall(root, new Vector3(edge, 1f, 0f), new Vector3(0.5f, 2f, edge * 2f), obstacleMaterial);
            CreateWall(root, new Vector3(-edge, 1f, 0f), new Vector3(0.5f, 2f, edge * 2f), obstacleMaterial);
        }

        private static void CreateWall(Transform root, Vector3 position, Vector3 size, Material material)
        {
            NetDemoPrimitives.Create(PrimitiveType.Cube, "Edge Wall", root, position, size, material, keepCollider: true);
        }

        private void SpawnPlayer()
        {
            player = Instantiate(playerPrefab, PlayerSpawnPosition, Quaternion.Euler(0f, SpawnYawDegrees, 0f));
            player.name = "Player";
            playerController = player.GetComponent<CharacterController>();

            // First person: the body is not drawn for its own camera, but still casts its shadow.
            foreach (var bodyRenderer in player.GetComponentsInChildren<Renderer>())
            {
                bodyRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }

            var viewCamera = Camera.main;
            if (viewCamera == null)
            {
                var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
                viewCamera = cameraObject.AddComponent<Camera>();
            }

            firstPersonCamera = viewCamera.gameObject.AddComponent<PrototypeFirstPersonCamera>();
            firstPersonCamera.Initialize(player.GetComponent<PlayerMotor>(), SpawnYawDegrees);
        }

        private void SpawnMushrooms()
        {
            var arena = new Rect(-ArenaHalfSize, -ArenaHalfSize, ArenaHalfSize * 2f, ArenaHalfSize * 2f);

            SpawnMushroom(NetTargetProfile.Common, new Vector3(-4f, 0f, 6f), 1f, new Color(0.85f, 0.2f, 0.15f), arena);
            SpawnMushroom(NetTargetProfile.CommonWeakened, new Vector3(1.5f, 0f, 9f), 1f, new Color(0.6f, 0.35f, 0.3f), arena);
            SpawnMushroom(NetTargetProfile.Rare, new Vector3(5f, 0f, 14f), 1.6f, new Color(0.55f, 0.25f, 0.85f), arena);
        }

        private void SpawnMushroom(NetTargetProfile profile, Vector3 position, float size, Color capColor, Rect arena)
        {
            var mushroomObject = new GameObject($"Mushroom ({profile})");
            mushroomObject.transform.SetParent(transform, false);
            mushroomObject.transform.position = position;

            var mushroom = mushroomObject.AddComponent<NetDemoMushroom>();
            mushroom.Initialize(profile, player.transform, arena, mushroomFleeSpeed, size, capColor);
            mushrooms.Add(mushroom);
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
                captureRates);

            var random = randomSeed != 0 ? new System.Random(randomSeed) : new System.Random();

            Thrower = player.AddComponent<NetThrower>();
            Thrower.Initialize(settings, firstPersonCamera.Camera, playerController, mushrooms, random);
        }

        private void OnResetPerformed(InputAction.CallbackContext _)
        {
            if (player == null)
            {
                return;
            }

            Thrower.ResetNet();

            foreach (var mushroom in mushrooms)
            {
                mushroom.ResetToSpawn();
            }

            // A CharacterController keeps its own cached position and would snap back to
            // it on the next Move, so it has to be off while the transform is teleported.
            playerController.enabled = false;
            player.transform.SetPositionAndRotation(PlayerSpawnPosition, Quaternion.Euler(0f, SpawnYawDegrees, 0f));
            playerController.enabled = true;
            firstPersonCamera.SetView(SpawnYawDegrees, 0f);
        }

        private Material SceneMaterial(string materialName, Color color)
        {
            var material = NetDemoPrimitives.CreateMaterial(materialName, color);
            sceneMaterials.Add(material);
            return material;
        }
    }
}
