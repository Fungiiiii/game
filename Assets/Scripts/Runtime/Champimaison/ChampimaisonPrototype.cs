using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Fungiiiii.Champimaison
{
    /// <summary>
    /// Runtime-only Champimaison proof of concept. It creates its tiny scene, visuals and uGUI
    /// controls at runtime so the flow is usable from the existing SampleScene without imported art.
    /// </summary>
    public sealed class ChampimaisonPrototype : MonoBehaviour
    {
        private static readonly Color GroundColor = new Color(0.09f, 0.18f, 0.14f);
        private static readonly Color SoilColor = new Color(0.27f, 0.16f, 0.11f);
        private static readonly Color GrassColor = new Color(0.25f, 0.49f, 0.25f);
        private static readonly Color StemColor = new Color(0.82f, 0.72f, 0.46f);
        private static readonly Color CapColor = new Color(0.79f, 0.19f, 0.24f);
        private static readonly Color Tier1BodyColor = new Color(0.77f, 0.43f, 0.23f);
        private static readonly Color Tier1RoofColor = new Color(0.25f, 0.55f, 0.75f);
        private static readonly Color HighlightColor = new Color(0.98f, 0.83f, 0.37f);
        private static readonly Color PanelColor = new Color(0.03f, 0.06f, 0.08f, 0.92f);
        private static readonly Color PanelAccentColor = new Color(0.11f, 0.22f, 0.23f, 0.94f);

        private readonly List<Material> _runtimeMaterials = new List<Material>();

        private ChampimaisonState _state;
        private Transform _worldRoot;
        private Transform _buildingRoot;
        private Canvas _canvas;
        private Text _tierLabel;
        private Text _statusLabel;
        private Text _resourcesLabel;
        private Text _costLabel;
        private Text _hintLabel;
        private Text _plantButtonLabel;
        private Text _upgradeButtonLabel;
        private Button _plantButton;
        private Button _upgradeButton;

        public ChampimaisonState State => _state;

        private void Awake()
        {
            _state = new ChampimaisonState();
            _state.Changed += HandleStateChanged;
        }

        private void Start()
        {
            ConfigureCamera();
            BuildWorld();
            BuildUi();
            RefreshView();
        }

        private void OnDestroy()
        {
            if (_state != null)
            {
                _state.Changed -= HandleStateChanged;
            }

            foreach (Material material in _runtimeMaterials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }

            _runtimeMaterials.Clear();
        }

        public bool TryPlantSeed()
        {
            return _state != null && _state.TryPlantSeed();
        }

        public bool TryUpgradeToTier1()
        {
            return _state != null && _state.TryUpgradeToTier1();
        }

        public void ResetPrototype()
        {
            _state?.Reset();
        }

        private void OnPlantButtonClicked()
        {
            TryPlantSeed();
        }

        private void OnUpgradeButtonClicked()
        {
            TryUpgradeToTier1();
        }

        private void HandleStateChanged()
        {
            if (_buildingRoot == null || _canvas == null)
            {
                return;
            }

            RefreshView();
        }

        private void ConfigureCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Champimaison Camera");
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
                cameraObject.AddComponent<AudioListener>();
            }

            camera.orthographic = true;
            camera.orthographicSize = 6.2f;
            camera.transform.position = new Vector3(0f, 8.5f, -10f);
            camera.transform.LookAt(new Vector3(0f, 1.15f, 0f));
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.09f, 0.13f);

            if (FindFirstObjectByType<Light>() == null)
            {
                GameObject lightObject = new GameObject("Champimaison Key Light");
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.7f;
                light.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            }
        }

        private void BuildWorld()
        {
            _worldRoot = new GameObject("Champimaison World").transform;
            _worldRoot.SetParent(transform, false);
            _buildingRoot = new GameObject("Champimaison Building").transform;
            _buildingRoot.SetParent(_worldRoot, false);

            CreatePrimitive("Ground", PrimitiveType.Cube, _worldRoot, new Vector3(0f, -0.35f, 0f), new Vector3(15f, 0.3f, 12f), GroundColor);
            CreatePrimitive("Plot", PrimitiveType.Cube, _worldRoot, new Vector3(0f, -0.05f, 0f), new Vector3(4.8f, 0.3f, 4.8f), SoilColor);
            CreatePrimitive("Plot Grass North", PrimitiveType.Cube, _worldRoot, new Vector3(0f, 0.11f, 2.14f), new Vector3(4.8f, 0.12f, 0.28f), GrassColor);
            CreatePrimitive("Plot Grass South", PrimitiveType.Cube, _worldRoot, new Vector3(0f, 0.11f, -2.14f), new Vector3(4.8f, 0.12f, 0.28f), GrassColor);
            CreatePrimitive("Plot Grass East", PrimitiveType.Cube, _worldRoot, new Vector3(2.14f, 0.11f, 0f), new Vector3(0.28f, 0.12f, 4.8f), GrassColor);
            CreatePrimitive("Plot Grass West", PrimitiveType.Cube, _worldRoot, new Vector3(-2.14f, 0.11f, 0f), new Vector3(0.28f, 0.12f, 4.8f), GrassColor);

            CreatePrimitive("Corner Marker North East", PrimitiveType.Cylinder, _worldRoot, new Vector3(2.4f, 0.35f, 2.4f), new Vector3(0.12f, 0.35f, 0.12f), HighlightColor);
            CreatePrimitive("Corner Marker North West", PrimitiveType.Cylinder, _worldRoot, new Vector3(-2.4f, 0.35f, 2.4f), new Vector3(0.12f, 0.35f, 0.12f), HighlightColor);
            CreatePrimitive("Corner Marker South East", PrimitiveType.Cylinder, _worldRoot, new Vector3(2.4f, 0.35f, -2.4f), new Vector3(0.12f, 0.35f, 0.12f), HighlightColor);
            CreatePrimitive("Corner Marker South West", PrimitiveType.Cylinder, _worldRoot, new Vector3(-2.4f, 0.35f, -2.4f), new Vector3(0.12f, 0.35f, 0.12f), HighlightColor);
        }

        private void RebuildBuildingVisual()
        {
            for (int index = _buildingRoot.childCount - 1; index >= 0; index--)
            {
                Destroy(_buildingRoot.GetChild(index).gameObject);
            }

            switch (_state.Tier)
            {
                case ChampimaisonTier.Empty:
                    BuildSeedVisual();
                    break;
                case ChampimaisonTier.Tier0:
                    BuildTier0Visual();
                    break;
                case ChampimaisonTier.Tier1:
                    BuildTier1Visual();
                    break;
            }
        }

        private void BuildSeedVisual()
        {
            CreatePrimitive("Unplanted Seed", PrimitiveType.Sphere, _buildingRoot, new Vector3(0f, 0.35f, 0f), new Vector3(0.35f, 0.35f, 0.35f), HighlightColor);
            CreatePrimitive("Seed Marker", PrimitiveType.Cylinder, _buildingRoot, new Vector3(0f, 0.17f, 0f), new Vector3(0.65f, 0.05f, 0.65f), HighlightColor);
        }

        private void BuildTier0Visual()
        {
            CreatePrimitive("Tier 0 Stem", PrimitiveType.Cylinder, _buildingRoot, new Vector3(0f, 1f, 0f), new Vector3(1.05f, 1f, 1.05f), StemColor);
            CreatePrimitive("Tier 0 Cap", PrimitiveType.Sphere, _buildingRoot, new Vector3(0f, 2.0f, 0f), new Vector3(2.35f, 0.82f, 2.35f), CapColor);
            CreatePrimitive("Tier 0 Spot Left", PrimitiveType.Sphere, _buildingRoot, new Vector3(-0.75f, 2.25f, -0.85f), new Vector3(0.27f, 0.13f, 0.27f), HighlightColor);
            CreatePrimitive("Tier 0 Spot Right", PrimitiveType.Sphere, _buildingRoot, new Vector3(0.78f, 2.22f, -0.55f), new Vector3(0.2f, 0.1f, 0.2f), HighlightColor);
        }

        private void BuildTier1Visual()
        {
            CreatePrimitive("Tier 1 Foundation", PrimitiveType.Cube, _buildingRoot, new Vector3(0f, 0.38f, 0f), new Vector3(2.9f, 0.5f, 2.9f), SoilColor);
            CreatePrimitive("Tier 1 Body", PrimitiveType.Cube, _buildingRoot, new Vector3(0f, 1.25f, 0f), new Vector3(2.35f, 1.75f, 2.35f), Tier1BodyColor);
            CreatePrimitive("Tier 1 Roof", PrimitiveType.Sphere, _buildingRoot, new Vector3(0f, 2.35f, 0f), new Vector3(3.35f, 1.15f, 3.35f), Tier1RoofColor);
            CreatePrimitive("Tier 1 Door", PrimitiveType.Cube, _buildingRoot, new Vector3(0f, 0.92f, -1.22f), new Vector3(0.62f, 1.08f, 0.12f), SoilColor);
            CreatePrimitive("Tier 1 Window Left", PrimitiveType.Cube, _buildingRoot, new Vector3(-0.72f, 1.45f, -1.22f), new Vector3(0.46f, 0.46f, 0.12f), HighlightColor);
            CreatePrimitive("Tier 1 Window Right", PrimitiveType.Cube, _buildingRoot, new Vector3(0.72f, 1.45f, -1.22f), new Vector3(0.46f, 0.46f, 0.12f), HighlightColor);
            CreatePrimitive("Tier 1 Chimney", PrimitiveType.Cube, _buildingRoot, new Vector3(0.9f, 3.05f, 0.35f), new Vector3(0.4f, 1.1f, 0.4f), SoilColor);
            CreatePrimitive("Tier 1 Flag", PrimitiveType.Cylinder, _buildingRoot, new Vector3(-1.03f, 3.3f, 0.25f), new Vector3(0.07f, 0.9f, 0.07f), StemColor);
            CreatePrimitive("Tier 1 Flag Cloth", PrimitiveType.Cube, _buildingRoot, new Vector3(-0.75f, 3.62f, 0.25f), new Vector3(0.55f, 0.28f, 0.08f), HighlightColor);
        }

        private void BuildUi()
        {
            EnsureEventSystem();

            GameObject canvasObject = new GameObject("Champimaison UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            _canvas = canvasObject.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform header = CreatePanel("Header", _canvas.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -80f), new Vector2(0f, 140f), PanelColor);
            CreateLabel("Title", header, "CHAMPIMAISON  /  BASE BUILDING POC", 34, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold, new Vector2(46f, -5f), new Vector2(1040f, 60f));
            _tierLabel = CreateLabel("Tier Badge", header, string.Empty, 30, HighlightColor, TextAnchor.MiddleRight, FontStyle.Bold, new Vector2(-45f, -5f), new Vector2(620f, 60f));

            RectTransform statusPanel = CreatePanel("Status Panel", _canvas.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -210f), new Vector2(720f, 150f), PanelAccentColor);
            _statusLabel = CreateLabel("Status", statusPanel, string.Empty, 30, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold, new Vector2(0f, 25f), new Vector2(660f, 64f));
            _hintLabel = CreateLabel("Hint", statusPanel, string.Empty, 22, new Color(0.8f, 0.9f, 0.86f), TextAnchor.MiddleCenter, FontStyle.Normal, new Vector2(0f, -28f), new Vector2(660f, 48f));

            RectTransform resourcesPanel = CreatePanel("Resources Panel", _canvas.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(255f, 150f), new Vector2(470f, 220f), PanelColor);
            CreateLabel("Resources Header", resourcesPanel, "FICTIONAL RESOURCES", 22, HighlightColor, TextAnchor.UpperLeft, FontStyle.Bold, new Vector2(20f, -22f), new Vector2(430f, 36f));
            _resourcesLabel = CreateLabel("Resources", resourcesPanel, string.Empty, 30, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold, new Vector2(20f, 22f), new Vector2(430f, 54f));
            _costLabel = CreateLabel("Cost", resourcesPanel, string.Empty, 20, new Color(0.8f, 0.9f, 0.86f), TextAnchor.LowerLeft, FontStyle.Normal, new Vector2(20f, 68f), new Vector2(430f, 52f));

            RectTransform actionsPanel = CreatePanel("Actions Panel", _canvas.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-300f, 170f), new Vector2(570f, 260f), PanelColor);
            CreateLabel("Actions Header", actionsPanel, "BUILD FLOW", 22, HighlightColor, TextAnchor.UpperLeft, FontStyle.Bold, new Vector2(22f, -22f), new Vector2(520f, 36f));
            _plantButton = CreateButton("Plant Button", actionsPanel, new Vector2(0f, -18f), new Vector2(520f, 62f), Color.Lerp(GrassColor, Color.white, 0.12f), OnPlantButtonClicked, out _plantButtonLabel);
            _upgradeButton = CreateButton("Upgrade Button", actionsPanel, new Vector2(0f, -92f), new Vector2(520f, 62f), Color.Lerp(Tier1RoofColor, Color.white, 0.12f), OnUpgradeButtonClicked, out _upgradeButtonLabel);
            CreateButton("Reset Button", actionsPanel, new Vector2(0f, -165f), new Vector2(520f, 48f), new Color(0.25f, 0.25f, 0.28f), ResetPrototype, out _);
        }

        private void RefreshView()
        {
            if (_state == null || _statusLabel == null)
            {
                return;
            }

            string tierText;
            string statusText;
            string hintText;
            switch (_state.Tier)
            {
                case ChampimaisonTier.Tier0:
                    tierText = "TIER 0  /  SPROUT";
                    statusText = "TIER 0  •  SEED PLANTED";
                    hintText = "Spend resources to grow the Champimaison to Tier 1.";
                    break;
                case ChampimaisonTier.Tier1:
                    tierText = "TIER 1  /  COMPLETE";
                    statusText = "TIER 1  •  CHAMPIMAISON BUILT";
                    hintText = "The prototype upgrade path is complete. Reset to plant again.";
                    break;
                default:
                    tierText = "TIER -1  /  EMPTY PLOT";
                    statusText = "EMPTY PLOT  •  READY TO PLANT";
                    hintText = "Plant a seed to create the Tier 0 sprout.";
                    break;
            }

            _tierLabel.text = tierText;
            _statusLabel.text = statusText;
            _hintLabel.text = hintText;
            _resourcesLabel.text = $"SPORES  {_state.Spores}     DEW  {_state.Dew}";

            if (_state.Tier == ChampimaisonTier.Empty)
            {
                _costLabel.text = "PLANT COST  •  1 SPORE";
            }
            else if (_state.Tier == ChampimaisonTier.Tier0)
            {
                _costLabel.text = "TIER 1 COST  •  2 SPORES + 1 DEW";
            }
            else
            {
                _costLabel.text = "TIER 1 COST  •  PAID";
            }

            _plantButtonLabel.text = _state.Tier == ChampimaisonTier.Empty ? "PLANT SEED  •  1 SPORE" : "SEED PLANTED";
            _upgradeButtonLabel.text = _state.Tier == ChampimaisonTier.Tier0 ? "UPGRADE TO TIER 1  •  2 SPORES + 1 DEW" : "UPGRADE TO TIER 1";
            _plantButton.interactable = _state.CanPlantSeed;
            _upgradeButton.interactable = _state.CanUpgradeToTier1;
            RebuildBuildingVisual();
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("Champimaison EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystemObject.hideFlags = HideFlags.DontSave;
        }

        private GameObject CreatePrimitive(string objectName, PrimitiveType primitiveType, Transform parent, Vector3 position, Vector3 scale, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = objectName;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = position;
            primitive.transform.localScale = scale;

            Renderer renderer = primitive.GetComponent<Renderer>();
            renderer.sharedMaterial = CreateMaterial(objectName + " Material", color);
            return primitive;
        }

        private Material CreateMaterial(string materialName, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader)
            {
                name = materialName,
                color = color
            };
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.28f);
            }

            _runtimeMaterials.Add(material);
            return material;
        }

        private static RectTransform CreateRect(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            GameObject rectObject = new GameObject(objectName, typeof(RectTransform));
            rectObject.transform.SetParent(parent, false);
            RectTransform rect = rectObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private static RectTransform CreatePanel(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Color color)
        {
            RectTransform rect = CreateRect(objectName, parent, anchorMin, anchorMax, position, size);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return rect;
        }

        private static Text CreateLabel(string objectName, Transform parent, string value, int fontSize, Color color, TextAnchor alignment, FontStyle fontStyle, Vector2 position, Vector2 size)
        {
            RectTransform rect = CreateRect(objectName, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
            Text label = rect.gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = value;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            label.fontStyle = fontStyle;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            return label;
        }

        private static Button CreateButton(string objectName, Transform parent, Vector2 position, Vector2 size, Color color, UnityAction action, out Text label)
        {
            RectTransform rect = CreateRect(objectName, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);

            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.22f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
            colors.disabledColor = new Color(color.r * 0.45f, color.g * 0.45f, color.b * 0.45f, 0.7f);
            button.colors = colors;

            label = CreateLabel(objectName + " Label", rect, string.Empty, 19, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold, Vector2.zero, size - new Vector2(20f, 8f));
            return button;
        }
    }

    internal static class ChampimaisonPrototypeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreatePrototype()
        {
            if (Object.FindFirstObjectByType<ChampimaisonPrototype>() != null)
            {
                return;
            }

            GameObject prototypeObject = new GameObject("Champimaison Prototype");
            prototypeObject.AddComponent<ChampimaisonPrototype>();
        }
    }
}
