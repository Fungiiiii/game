using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fungiiiii.Runtime.Player
{
    /// <summary>
    /// Builds a primitive-only movement playground at runtime from the bootstrap object
    /// placed in the prototype scene, which is its only entry point.
    ///
    /// Players are not built here. Netcode for GameObjects spawns one from the serialized
    /// player prefab for every client once a session starts: F1 starts a host, F2 joins
    /// a host running on this machine.
    /// </summary>
    internal sealed class PlayerMovementDemoBootstrap : MonoBehaviour
    {
        private static readonly Vector3 CameraOffset = new(0f, 9f, -9f);

        /// <summary>
        /// Height of the gap under the lintel. Above the crouched capsule (1.2 m) and
        /// below the standing one (2 m), so the wall can only be crossed crouched.
        /// </summary>
        private const float LowPassageClearance = 1.4f;

        private const float LowPassageZ = 8f;
        private const float LowPassageHalfWidth = 2f;
        private const float LowPassageReach = 10f;

        /// <summary>
        /// The lintel is kept thin, and the side walls no taller than the gap, so the
        /// wall blocks the player without hiding them from the overhead camera.
        /// </summary>
        private const float LowPassageLintelThickness = 0.5f;

        private static readonly Vector3[] ObstaclePositions =
        {
            new(-3.5f, 0.75f, 3f),
            new(3.5f, 0.75f, 3f),
            new(7f, 0.75f, 4f),
            new(-6f, 0.75f, -2f),
            new(6f, 0.75f, -2f)
        };

        [Tooltip("Networked player spawned for every client that joins the session.")]
        [SerializeField]
        private NetworkObject playerPrefab;

        private InputAction startHostAction;
        private InputAction startClientAction;
        private Transform localPlayer;
        private Camera demoCamera;

        private void Awake()
        {
            // Development shortcuts for this prototype, not player-facing controls.
            startHostAction = new InputAction("StartHost", InputActionType.Button, "<Keyboard>/f1");
            startClientAction = new InputAction("StartClient", InputActionType.Button, "<Keyboard>/f2");
            startHostAction.performed += OnStartHostPerformed;
            startClientAction.performed += OnStartClientPerformed;
        }

        private void OnEnable()
        {
            startHostAction.Enable();
            startClientAction.Enable();
        }

        private void OnDisable()
        {
            startHostAction.Disable();
            startClientAction.Disable();
        }

        private void OnDestroy()
        {
            startHostAction.performed -= OnStartHostPerformed;
            startClientAction.performed -= OnStartClientPerformed;
            startHostAction.Dispose();
            startClientAction.Dispose();
        }

        private void Start()
        {
            var root = transform;
            var groundMaterial = CreateMaterial("Prototype Ground Material", new Color(0.18f, 0.28f, 0.2f));
            var obstacleMaterial = CreateMaterial("Prototype Obstacle Material", new Color(0.35f, 0.3f, 0.26f));

            CreateGround(root, groundMaterial);

            foreach (var position in ObstaclePositions)
            {
                CreateObstacle(root, position, obstacleMaterial);
            }

            CreateLowPassage(root, obstacleMaterial, CreateMaterial("Prototype Lintel Material", new Color(0.72f, 0.24f, 0.2f)));

            demoCamera = ConfigureCamera();
            ConfigureLight(root);
            EnsureNetworkManager();
        }

        private void LateUpdate()
        {
            if (demoCamera == null)
            {
                return;
            }

            if (localPlayer == null)
            {
                localPlayer = FindLocalPlayer();
                if (localPlayer == null)
                {
                    return;
                }
            }

            var cameraTransform = demoCamera.transform;
            cameraTransform.position = localPlayer.position + CameraOffset;
            cameraTransform.rotation =
                Quaternion.LookRotation(localPlayer.position + Vector3.up - cameraTransform.position);
        }

        private void OnStartHostPerformed(InputAction.CallbackContext context)
        {
            StartSession(asHost: true);
        }

        private void OnStartClientPerformed(InputAction.CallbackContext context)
        {
            StartSession(asHost: false);
        }

        private static void StartSession(bool asHost)
        {
            var manager = NetworkManager.Singleton;
            if (manager == null || manager.IsListening)
            {
                return;
            }

            var started = asHost ? manager.StartHost() : manager.StartClient();
            Debug.Log(asHost
                ? $"Player movement prototype: host start {(started ? "succeeded" : "failed")}."
                : $"Player movement prototype: client start {(started ? "succeeded" : "failed")}.");
        }

        private void EnsureNetworkManager()
        {
            // NetworkManager moves itself to DontDestroyOnLoad, so one can survive a reload
            // of this scene. A second one would fight it for the singleton.
            if (NetworkManager.Singleton != null)
            {
                return;
            }

            if (playerPrefab == null)
            {
                Debug.LogError("PlayerMovementDemoBootstrap has no player prefab assigned; networking is disabled.", this);
                return;
            }

            // A freshly added NetworkManager has a null NetworkConfig, which its OnEnable
            // reads, so the object stays inactive until the configuration is in place.
            var managerObject = new GameObject("Network Manager");
            managerObject.SetActive(false);

            var transport = managerObject.AddComponent<UnityTransport>();
            var manager = managerObject.AddComponent<NetworkManager>();
            manager.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport,
                PlayerPrefab = playerPrefab.gameObject,

                // Every peer already runs this single prototype scene and it holds no
                // in-scene network objects, so there is nothing for the host to synchronise.
                EnableSceneManagement = false
            };

            managerObject.SetActive(true);
        }

        private static Transform FindLocalPlayer()
        {
            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsClient)
            {
                return null;
            }

            var localClient = manager.LocalClient;
            if (localClient == null)
            {
                return null;
            }

            var playerObject = localClient.PlayerObject;
            return playerObject != null ? playerObject.transform : null;
        }

        private static void CreateGround(Transform parent, Material material)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Prototype Ground";
            ground.transform.SetParent(parent, false);
            ground.transform.localScale = new Vector3(2f, 1f, 2f);
            ApplyMaterial(ground, material);
        }

        private static void CreateObstacle(Transform parent, Vector3 position, Material material)
        {
            var obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = "Prototype Obstacle";
            obstacle.transform.SetParent(parent, false);
            obstacle.transform.position = position;
            obstacle.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            ApplyMaterial(obstacle, material);
        }

        /// <summary>
        /// Builds a wall with a single opening too low to walk through, so crouching can
        /// be verified against collision rather than by eye.
        /// </summary>
        private static void CreateLowPassage(Transform parent, Material wallMaterial, Material lintelMaterial)
        {
            var sideWallWidth = LowPassageReach - LowPassageHalfWidth;
            var sideWallCentre = LowPassageHalfWidth + sideWallWidth * 0.5f;

            CreateBlock(
                parent,
                "Low Passage Wall (Left)",
                new Vector3(-sideWallCentre, LowPassageClearance * 0.5f, LowPassageZ),
                new Vector3(sideWallWidth, LowPassageClearance, 1f),
                wallMaterial);

            CreateBlock(
                parent,
                "Low Passage Wall (Right)",
                new Vector3(sideWallCentre, LowPassageClearance * 0.5f, LowPassageZ),
                new Vector3(sideWallWidth, LowPassageClearance, 1f),
                wallMaterial);

            CreateBlock(
                parent,
                "Low Passage Lintel (Crouch To Pass)",
                new Vector3(0f, LowPassageClearance + LowPassageLintelThickness * 0.5f, LowPassageZ),
                new Vector3(LowPassageHalfWidth * 2f, LowPassageLintelThickness, 1f),
                lintelMaterial);
        }

        private static void CreateBlock(
            Transform parent,
            string blockName,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = blockName;
            block.transform.SetParent(parent, false);
            block.transform.position = position;
            block.transform.localScale = scale;
            ApplyMaterial(block, material);
        }

        private static Camera ConfigureCamera()
        {
            var demoCamera = Camera.main;
            if (demoCamera == null)
            {
                var cameraObject = new GameObject("Prototype Camera");
                demoCamera = cameraObject.AddComponent<Camera>();
                demoCamera.tag = "MainCamera";
            }

            // Framing used until a session starts and the local player exists to follow.
            demoCamera.transform.position = CameraOffset;
            demoCamera.transform.rotation = Quaternion.LookRotation(Vector3.up - CameraOffset);
            demoCamera.fieldOfView = 55f;
            demoCamera.nearClipPlane = 0.1f;
            demoCamera.farClipPlane = 100f;
            return demoCamera;
        }

        private static void ConfigureLight(Transform parent)
        {
            var light = Object.FindFirstObjectByType<Light>();
            if (light == null)
            {
                var lightObject = new GameObject("Prototype Directional Light");
                lightObject.transform.SetParent(parent, false);
                light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
            }

            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.intensity = 1.2f;
        }

        private static Material CreateMaterial(string materialName, Color color)
        {
            // Null coalescing is not usable here: Unity overloads ==, but not ??, so a
            // destroyed object would slip through instead of falling back (UNT0007).
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader)
            {
                name = materialName,
                color = color
            };
            return material;
        }

        private static void ApplyMaterial(GameObject target, Material material)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }
    }
}
