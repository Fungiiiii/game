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

        private static readonly Vector3[] ObstaclePositions =
        {
            new(-3.5f, 0.75f, 3f),
            new(3.5f, 0.75f, 3f),
            new(0f, 0.75f, 6f),
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

        private static Transform CreatePlayer(Transform parent, Material material)
        {
            var playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObject.name = "Player (Capsule Placeholder)";
            playerObject.transform.SetParent(parent, false);
            playerObject.transform.position = PlayerStartPosition;
            ApplyMaterial(playerObject, material);

            // The CharacterController brings its own capsule; the primitive's collider
            // would fight with it.
            var primitiveCollider = playerObject.GetComponent<Collider>();
            if (primitiveCollider != null)
            {
                Destroy(primitiveCollider);
            }

            var controller = playerObject.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            controller.center = Vector3.zero;
            controller.slopeLimit = 45f;
            controller.stepOffset = 0.3f;

            var reader = playerObject.AddComponent<PlayerInputReader>();
            var motor = playerObject.AddComponent<PlayerMotor>();
            motor.SetInputReader(reader);

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
