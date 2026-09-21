using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// Builds the capture POC from Unity primitives so it can run without imported art.
    /// </summary>
    public sealed class MushroomCaptureDemo : MonoBehaviour
    {
        private static readonly Color IdleColor = new Color(0.45f, 0.2f, 0.8f);
        private static readonly Color AlertColor = new Color(1f, 0.75f, 0.1f);
        private static readonly Color FleeColor = new Color(0.95f, 0.12f, 0.08f);
        private static readonly Color CapturedColor = new Color(0.15f, 0.85f, 0.35f);

        [SerializeField] private float playerSpeed = 4f;
        [SerializeField] private float fleeSpeed = 5f;

        private readonly MushroomCaptureStateMachine.Configuration configuration =
            MushroomCaptureStateMachine.Configuration.Default;

        private MushroomCaptureStateMachine stateMachine;
        private InputAction moveAction;
        private InputAction captureAction;
        private InputAction resetAction;
        private Transform playerVisual;
        private Transform mushroomVisual;
        private Renderer mushroomCapRenderer;
        private Renderer stateMarkerRenderer;
        private Material mushroomMaterial;
        private Material markerMaterial;
        private Vector3 playerPosition;
        private Vector3 mushroomPosition;
        private Vector3 initialPlayerPosition;
        private Vector3 initialMushroomPosition;

        private void Awake()
        {
            stateMachine = new MushroomCaptureStateMachine(configuration);
            stateMachine.StateChanged += HandleStateChanged;

            initialPlayerPosition = new Vector3(-5f, 1f, 0f);
            initialMushroomPosition = new Vector3(0f, 0f, 0f);
            playerPosition = initialPlayerPosition;
            mushroomPosition = initialMushroomPosition;

            CreateMaterials();
            CreatePrototypeGeometry();
            ConfigureInput();
            ConfigureCamera();
            ApplyStateVisuals(stateMachine.CurrentState);
        }

        private void OnDestroy()
        {
            if (stateMachine != null)
            {
                stateMachine.StateChanged -= HandleStateChanged;
            }

            DisposeInput();

            if (mushroomMaterial != null)
            {
                Destroy(mushroomMaterial);
            }

            if (markerMaterial != null)
            {
                Destroy(markerMaterial);
            }
        }

        private void Update()
        {
            if (resetAction.WasPressedThisFrame())
            {
                ResetDemo();
            }

            var movement = moveAction.ReadValue<Vector2>();
            playerPosition += new Vector3(movement.x, 0f, movement.y) * (playerSpeed * Time.deltaTime);
            playerPosition.x = Mathf.Clamp(playerPosition.x, -7f, 7f);
            playerPosition.z = Mathf.Clamp(playerPosition.z, -3f, 3f);
            playerVisual.position = playerPosition;

            var observerDistance = Vector3.Distance(playerPosition, mushroomPosition + Vector3.up);
            stateMachine.Tick(observerDistance, Time.deltaTime);

            if (captureAction.WasPressedThisFrame())
            {
                stateMachine.TryCapture(observerDistance);
            }

            if (stateMachine.CurrentState == MushroomCaptureState.Flee)
            {
                MoveMushroomAwayFromPlayer();
            }

            mushroomVisual.position = mushroomPosition;
        }

        private void HandleStateChanged(MushroomCaptureState _, MushroomCaptureState nextState)
        {
            ApplyStateVisuals(nextState);
        }

        private void ResetDemo()
        {
            playerPosition = initialPlayerPosition;
            mushroomPosition = initialMushroomPosition;
            playerVisual.position = playerPosition;
            mushroomVisual.position = mushroomPosition;
            stateMachine.Reset();
            ApplyStateVisuals(stateMachine.CurrentState);
        }

        private void MoveMushroomAwayFromPlayer()
        {
            var awayFromPlayer = mushroomPosition - playerPosition;
            awayFromPlayer.y = 0f;

            if (awayFromPlayer.sqrMagnitude < 0.001f)
            {
                awayFromPlayer = Vector3.right;
            }

            mushroomPosition += awayFromPlayer.normalized * (fleeSpeed * Time.deltaTime);
            mushroomPosition.x = Mathf.Clamp(mushroomPosition.x, -7f, 7f);
            mushroomPosition.z = Mathf.Clamp(mushroomPosition.z, -3f, 3f);
        }

        private void CreatePrototypeGeometry()
        {
            var geometryRoot = new GameObject("Capture Prototype Geometry").transform;
            geometryRoot.SetParent(transform, false);

            var ground = CreatePrimitive(
                PrimitiveType.Cube,
                "Ground",
                geometryRoot,
                new Vector3(0f, -0.5f, 0f),
                new Vector3(16f, 1f, 8f),
                CreateMaterial("Ground Material", new Color(0.08f, 0.12f, 0.1f)));
            ground.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var player = CreatePrimitive(
                PrimitiveType.Capsule,
                "Player Marker",
                geometryRoot,
                playerPosition,
                new Vector3(0.8f, 1f, 0.8f),
                CreateMaterial("Player Material", new Color(0.1f, 0.65f, 0.95f)));
            playerVisual = player.transform;

            var mushroomObject = new GameObject("Mushroom Placeholder");
            mushroomVisual = mushroomObject.transform;
            mushroomVisual.SetParent(geometryRoot, false);
            mushroomVisual.position = mushroomPosition;

            CreatePrimitive(
                PrimitiveType.Cylinder,
                "Mushroom Stem",
                mushroomVisual,
                new Vector3(0f, 1f, 0f),
                new Vector3(0.45f, 1f, 0.45f),
                CreateMaterial("Stem Material", new Color(0.88f, 0.78f, 0.62f)));

            var cap = CreatePrimitive(
                PrimitiveType.Sphere,
                "Mushroom Cap",
                mushroomVisual,
                new Vector3(0f, 2f, 0f),
                new Vector3(1.35f, 0.5f, 1.35f),
                mushroomMaterial);
            mushroomCapRenderer = cap.GetComponent<Renderer>();

            var marker = CreatePrimitive(
                PrimitiveType.Cylinder,
                "State Color Marker",
                mushroomVisual,
                new Vector3(0f, 0.03f, 0f),
                new Vector3(1.8f, 0.03f, 1.8f),
                markerMaterial);
            stateMarkerRenderer = marker.GetComponent<Renderer>();
        }

        private void CreateMaterials()
        {
            mushroomMaterial = CreateMaterial("Mushroom State Material", IdleColor);
            markerMaterial = CreateMaterial("State Marker Material", IdleColor);
        }

        private void ConfigureInput()
        {
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            moveAction.AddBinding("<Gamepad>/leftStick");

            captureAction = new InputAction("Interact", InputActionType.Button);
            captureAction.AddBinding("<Keyboard>/e");
            captureAction.AddBinding("<Gamepad>/buttonSouth");

            resetAction = new InputAction("ResetCapturePrototype", InputActionType.Button);
            resetAction.AddBinding("<Keyboard>/r");
            resetAction.AddBinding("<Gamepad>/start");

            moveAction.Enable();
            captureAction.Enable();
            resetAction.Enable();
        }

        private void DisposeInput()
        {
            moveAction?.Dispose();
            captureAction?.Dispose();
            resetAction?.Dispose();
        }

        private void ConfigureCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            camera.transform.position = new Vector3(0f, 6.5f, -12f);
            camera.transform.LookAt(new Vector3(0f, 1f, 0f));
        }

        private void ApplyStateVisuals(MushroomCaptureState state)
        {
            var stateColor = GetStateColor(state);
            mushroomCapRenderer.sharedMaterial.color = stateColor;
            stateMarkerRenderer.sharedMaterial.color = stateColor;

            var scale = state == MushroomCaptureState.Captured ? 0.85f : 1f;
            mushroomVisual.localScale = new Vector3(scale, scale, scale);
        }

        private static Color GetStateColor(MushroomCaptureState state)
        {
            switch (state)
            {
                case MushroomCaptureState.Idle:
                    return IdleColor;
                case MushroomCaptureState.Alert:
                    return AlertColor;
                case MushroomCaptureState.Flee:
                    return FleeColor;
                case MushroomCaptureState.Captured:
                    return CapturedColor;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private static GameObject CreatePrimitive(
            PrimitiveType primitiveType,
            string objectName,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            var primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = objectName;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localScale = localScale;
            primitive.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.Destroy(primitive.GetComponent<Collider>());
            return primitive;
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
    }
}
