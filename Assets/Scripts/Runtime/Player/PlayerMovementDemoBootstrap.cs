using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fungiiiii.Runtime.Player
{
    /// <summary>
    /// Creates a self-contained primitive-only movement playground at runtime.
    /// It keeps the prototype scene free of prototype-only serialized objects.
    /// </summary>
    internal sealed class PlayerMovementDemoBootstrap : MonoBehaviour
    {
        private const string DemoRootName = "Player Movement Demo";
        private const string PrototypeScenePath = "Assets/Scenes/Prototype/MouvementJoueurScene.unity";

        private static readonly Vector3 CameraOffset = new(0f, 9f, -9f);
        private static readonly Vector3 PlayerStartPosition = new(0f, 1.2f, 0f);

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

        private Transform playerTransform;
        private Camera demoCamera;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneLoadHandler()
        {
            SceneManager.sceneLoaded -= CreateDemoForScene;
            SceneManager.sceneLoaded += CreateDemoForScene;
        }

        private static void CreateDemoForScene(Scene scene, LoadSceneMode mode)
        {
            if (scene.path != PrototypeScenePath)
            {
                return;
            }

            if (GameObject.Find(DemoRootName) != null)
            {
                return;
            }

            var root = new GameObject(DemoRootName);
            root.AddComponent<PlayerMovementDemoBootstrap>();
        }

        private void Start()
        {
            var root = transform;
            var groundMaterial = CreateMaterial("Prototype Ground Material", new Color(0.18f, 0.28f, 0.2f));
            var obstacleMaterial = CreateMaterial("Prototype Obstacle Material", new Color(0.35f, 0.3f, 0.26f));
            var playerMaterial = CreateMaterial("Prototype Player Material", new Color(0.95f, 0.72f, 0.12f));

            CreateGround(root, groundMaterial);

            foreach (var position in ObstaclePositions)
            {
                CreateObstacle(root, position, obstacleMaterial);
            }

            CreateLowPassage(root, obstacleMaterial, CreateMaterial("Prototype Lintel Material", new Color(0.72f, 0.24f, 0.2f)));

            playerTransform = CreatePlayer(root, playerMaterial);

            demoCamera = ConfigureCamera();
            ConfigureLight(root);
        }

        private void LateUpdate()
        {
            if (playerTransform == null || demoCamera == null)
            {
                return;
            }

            var cameraTransform = demoCamera.transform;
            cameraTransform.position = playerTransform.position + CameraOffset;
            cameraTransform.rotation =
                Quaternion.LookRotation(playerTransform.position + Vector3.up - cameraTransform.position);
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

        private static Transform CreatePlayer(Transform parent, Material material)
        {
            var playerObject = new GameObject("Player");
            playerObject.transform.SetParent(parent, false);
            playerObject.transform.position = PlayerStartPosition;

            // The mesh lives on a child so crouching can squash it without scaling the
            // CharacterController along with it.
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Player Visual (Capsule Placeholder)";
            visual.transform.SetParent(playerObject.transform, false);
            ApplyMaterial(visual, material);

            // The CharacterController brings its own capsule; the primitive's collider
            // would fight with it.
            var visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
            {
                Destroy(visualCollider);
            }

            var controller = playerObject.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            controller.center = Vector3.zero;
            controller.slopeLimit = 45f;
            controller.stepOffset = 0.3f;

            var reader = playerObject.AddComponent<PlayerInputReader>();
            var motor = playerObject.AddComponent<PlayerMotor>();
            motor.Initialise(reader, visual.transform);

            return playerObject.transform;
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
