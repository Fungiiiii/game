using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fungiiiii.Runtime.AI
{
    /// <summary>
    /// Creates a self-contained primitive-only demonstration at runtime.
    /// It keeps the sample scene free of prototype-only serialized objects.
    /// </summary>
    internal sealed class GenericPatrolDemoBootstrap : MonoBehaviour
    {
        private const string DemoRootName = "Generic Patrol Demo";
        private const string PrototypeScenePath = "Assets/Scenes/Prototype/MouvementIAChampignonScene.unity";

        private static readonly Vector3[] Route =
        {
            new(-4f, 0.5f, -2.5f),
            new(4f, 0.5f, -2.5f),
            new(4f, 0.5f, 2.5f),
            new(-4f, 0.5f, 2.5f)
        };

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
            root.AddComponent<GenericPatrolDemoBootstrap>();
        }

        private void Start()
        {
            var root = transform;
            var groundMaterial = CreateMaterial("Prototype Ground Material", new Color(0.18f, 0.28f, 0.2f));
            var markerMaterial = CreateMaterial("Prototype Waypoint Material", new Color(0.95f, 0.72f, 0.12f));
            var agentMaterial = CreateMaterial("Prototype Agent Material", new Color(0.65f, 0.22f, 0.78f));

            CreateGround(root, groundMaterial);

            var waypointTransforms = new Transform[Route.Length];
            for (var i = 0; i < Route.Length; i++)
            {
                waypointTransforms[i] = CreateWaypoint(root, i, Route[i], markerMaterial);
            }

            var agent = CreateAgent(root, Route[0], agentMaterial);
            agent.SetWaypoints(waypointTransforms);
            agent.MovementSpeed = 2f;
            agent.WaypointArrivalDistance = 0.08f;
            agent.Loop = true;

            ConfigureCamera();
            ConfigureLight(root);
        }

        private static void CreateGround(Transform parent, Material material)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Prototype Ground";
            ground.transform.SetParent(parent, false);
            ground.transform.localScale = new Vector3(1.25f, 1f, 0.9f);
            ApplyMaterial(ground, material);
            RemoveCollider(ground);
        }

        private static Transform CreateWaypoint(Transform parent, int index, Vector3 position, Material material)
        {
            var waypoint = new GameObject($"Waypoint {index + 1}");
            waypoint.transform.SetParent(parent, false);
            waypoint.transform.position = position;

            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = $"Waypoint Marker {index + 1}";
            marker.transform.SetParent(waypoint.transform, false);
            marker.transform.localPosition = Vector3.down * 0.45f;
            marker.transform.localScale = Vector3.one * 0.25f;
            ApplyMaterial(marker, material);
            RemoveCollider(marker);

            return waypoint.transform;
        }

        private static PatrolAgent CreateAgent(Transform parent, Vector3 position, Material material)
        {
            var agentObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            agentObject.name = "Patrol Agent (Cube Placeholder)";
            agentObject.transform.SetParent(parent, false);
            agentObject.transform.position = position;
            agentObject.transform.localScale = Vector3.one * 0.9f;
            ApplyMaterial(agentObject, material);
            RemoveCollider(agentObject);
            return agentObject.AddComponent<PatrolAgent>();
        }

        private static void ConfigureCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Prototype Camera");
                camera = cameraObject.AddComponent<Camera>();
                camera.tag = "MainCamera";
            }

            camera.transform.position = new Vector3(0f, 8.5f, -11f);
            camera.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 0.4f, 0f) - camera.transform.position);
            camera.fieldOfView = 55f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
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
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
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

        private static void RemoveCollider(GameObject target)
        {
            var collider = target.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
        }
    }
}
