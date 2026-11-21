using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// The inventory screen: a 3-column by 4-row item panel and a three-slot quick bar on a
    /// wooden panel, as on the Inventaire artboard (playtest of 20/11, feedback #2). Presses
    /// act at once through an <see cref="InventoryHand"/>; what is held follows the mouse,
    /// and goes back to its slot when the screen closes. Items show as squares of their
    /// colour until their icons are drawn.
    /// </summary>
    public sealed class InventoryDemoController : MonoBehaviour
    {
        public const int MainColumnCount = 3;
        public const int MainRowCount = 4;
        public const int MainSlotCount = MainColumnCount * MainRowCount;
        public const int HotbarSlotCount = 3;
        public const int TotalSlotCount = MainSlotCount + HotbarSlotCount;

        [SerializeField] private int demoMaxStackSize = 5;

        [Tooltip("Fills the inventory with sample items on Awake. Off for a game loop that starts empty.")]
        [SerializeField] private bool seedDemoItems = true;

        private const string DemoMushroomId = "demo_mushroom";
        private const string DemoHerbId = "demo_herb";
        private const string DemoSporeId = "demo_spore";
        private const string DemoCrystalId = "demo_crystal";

        // Sizes of the Inventaire artboard, drawn on a 1280 x 720 canvas.
        private static readonly Vector2 ReferenceResolution = new Vector2(1280f, 720f);
        private static readonly Vector2 PanelSize = new Vector2(340f, 612f);
        private const float PanelTop = 36f;
        private const float PanelCorner = 20f;
        private const float GridWidth = 260f;
        private const float SlotSize = 80f;
        private const float SlotSpacing = 10f;
        private const float SlotCorner = 10f;
        private const float ItemSize = 52f;
        private const float HeldSize = 64f;
        private const float HeldTiltDegrees = -6f;
        private const float HoverBrightness = 1.45f;
        private const float HintsTop = 668f;

        // Palette: "Body light", "Wood dark", "Bark pale", the shadow brown and the night blue.
        private static readonly Color Cream = new Color32(0xF2, 0xE6, 0xCC, 0xFF);
        private static readonly Color WoodDark = new Color32(0x4F, 0x34, 0x24, 0xFF);
        private static readonly Color BarkPale = new Color32(0xA8, 0x90, 0x78, 0xFF);
        private static readonly Color Shadow = new Color32(0x2B, 0x27, 0x23, 0xFF);
        private static readonly Color Night = new Color32(0x1E, 0x26, 0x30, 0xFF);
        private static readonly Color Sand = new Color32(0xE0, 0xCB, 0xA4, 0xFF);
        private static readonly Color Veil = new Color(30f / 255f, 38f / 255f, 48f / 255f, 0.66f);
        private static readonly Color PanelFacet = new Color(43f / 255f, 39f / 255f, 35f / 255f, 0.28f);
        private static readonly Color SlotInnerShadow = new Color(0f, 0f, 0f, 0.4f);
        private static readonly Color Dashes = new Color(168f / 255f, 144f / 255f, 120f / 255f, 0.45f);
        private static readonly Color HeldShadow = new Color(30f / 255f, 38f / 255f, 48f / 255f, 0.55f);
        private static readonly Color HintText = new Color(Cream.r, Cream.g, Cream.b, 0.82f);

        private static readonly (string Key, string Action)[] Hints =
        {
            (UiText.InventoryHintLeftKey, UiText.InventoryHintLeft),
            (UiText.InventoryHintRightKey, UiText.InventoryHintRight),
            (UiText.InventoryHintDoubleKey, UiText.InventoryHintDouble),
            (UiText.InventoryHintCloseKey, UiText.InventoryHintClose),
        };

        private static Sprite uiSprite;

        private PlayerInventory inventory;
        private InventoryHand hand;
        private SlotView[] slotViews;
        private RectTransform canvasRect;
        private RectTransform held;
        private Image heldItem;
        private Text heldCount;
        private Text title;
        private Font font;

        public PlayerInventory Inventory => inventory;

        /// <summary>
        /// Whether Awake fills the inventory with sample items. Set it before Awake runs,
        /// that is on an inactive GameObject, to start empty.
        /// </summary>
        public bool SeedsDemoItems
        {
            get => seedDemoItems;
            set => seedDemoItems = value;
        }

        /// <summary>
        /// The font of the screen. Set it before Awake runs, like <see cref="SeedsDemoItems"/>;
        /// without one, Unity's built-in font is used.
        /// </summary>
        public Font Font
        {
            get => font;
            set => font = value;
        }

        /// <summary>What the mouse holds.</summary>
        internal InventoryHand Hand => hand;

        /// <summary>The screen's title, for tests.</summary>
        internal Text Title => title;

        private void Awake()
        {
            inventory = new PlayerInventory(TotalSlotCount);
            hand = new InventoryHand(inventory);
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            CreateEventSystemIfNeeded();
            BuildUi();
            inventory.Changed += RefreshSlots;
            hand.Changed += RefreshHeld;

            if (seedDemoItems)
            {
                SeedDemoItems();
            }
        }

        private void OnDisable()
        {
            // Closing the screen puts back what the mouse held.
            hand?.PutBack();
        }

        private void Update()
        {
            if (hand.IsHolding && Mouse.current != null)
            {
                FollowPointer(Mouse.current.position.ReadValue());
            }
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.Changed -= RefreshSlots;
            }

            if (hand != null)
            {
                hand.Changed -= RefreshHeld;
            }
        }

        /// <summary>A press on a slot, from its pointer handler or from a test.</summary>
        internal void PressSlot(int index, PointerEventData.InputButton button, Vector2 screenPosition)
        {
            if (button == PointerEventData.InputButton.Left)
            {
                hand.PressLeft(index, Time.unscaledTime);
            }
            else if (button == PointerEventData.InputButton.Right)
            {
                hand.PressRight(index);
            }

            FollowPointer(screenPosition);
        }

        /// <summary>The number shown on a slot, empty when there is none.</summary>
        internal string QuantityTextOf(int index) => slotViews[index].QuantityText;

        internal void SetHovered(int index, bool hovered)
        {
            slotViews[index].SetHovered(hovered);
        }

        private void SeedDemoItems()
        {
            var mushroom = new InventoryItemDefinition(DemoMushroomId, new Color(0.95f, 0.52f, 0.18f, 1f), demoMaxStackSize);
            var herb = new InventoryItemDefinition(DemoHerbId, new Color(0.25f, 0.78f, 0.35f, 1f), 10);
            var spore = new InventoryItemDefinition(DemoSporeId, new Color(0.55f, 0.35f, 0.92f, 1f), 16);
            var crystal = new InventoryItemDefinition(DemoCrystalId, new Color(0.25f, 0.75f, 0.95f, 1f), 4);

            inventory.Add(mushroom, 6);
            inventory.Add(herb, 3);
            inventory.Add(spore, 8);
            inventory.Add(crystal, 2);

            // Put three sample items in the quick bar so the move flow is visible immediately.
            inventory.MoveOrSwap(0, MainSlotCount);
            inventory.MoveOrSwap(2, MainSlotCount + 1);
            inventory.MoveOrSwap(3, MainSlotCount + 2);
        }

        private void BuildUi()
        {
            var canvasObject = CreateUiObject("InventoryCanvas", transform);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            canvasRect = (RectTransform)canvasObject.transform;

            var veil = CreateUiObject("Veil", canvasRect).AddComponent<Image>();
            veil.sprite = GetUiSprite();
            veil.color = Veil;
            veil.raycastTarget = false;
            Stretch(veil.rectTransform);

            var panelObject = CreateUiObject("InventoryPanel", canvasRect);
            var panel = (RectTransform)panelObject.transform;
            SetRect(panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), PanelSize, new Vector2(0f, -PanelTop));
            var panelGraphic = panelObject.AddComponent<CutCornerGraphic>();
            panelGraphic.raycastTarget = false;
            panelGraphic.SetShape(PanelCorner, WoodDark);
            panelGraphic.SetBands(BarkPale, 8f, Shadow, 12f);
            // The artboard's gradient at 100 degrees, from 68 % of its length.
            panelGraphic.SetFacet(PanelFacet, 0.9f, 0.58f);

            title = CreateLabel("InventoryTitle", panel, UiText.InventoryTitle, 40, Cream, new Vector2(0f, -26f), new Vector2(GridWidth, 46f));
            AddShadow(title, Shadow, 3f);

            var grid = CreateGrid("InventoryGrid_3x4", panel, MainColumnCount, MainRowCount, -84f);
            slotViews = new SlotView[TotalSlotCount];
            for (var index = 0; index < MainSlotCount; index++)
            {
                slotViews[index] = CreateSlot(grid, index, 0.22f);
            }

            CreateDashes(panel, -452f);
            CreateLabel("HotbarTitle", panel, UiText.InventoryHotbar, 20, Sand, new Vector2(0f, -464f), new Vector2(GridWidth, 26f));

            var hotbar = CreateGrid("Hotbar_3Slots", panel, HotbarSlotCount, 1, -500f);
            for (var index = 0; index < HotbarSlotCount; index++)
            {
                slotViews[MainSlotCount + index] = CreateSlot(hotbar, MainSlotCount + index, 0.5f);
            }

            CreateHints();
            CreateHeld();
            RefreshSlots();
            RefreshHeld();
        }

        private RectTransform CreateGrid(string gridName, RectTransform panel, int columns, int rows, float top)
        {
            var gridObject = CreateUiObject(gridName, panel);
            var rect = (RectTransform)gridObject.transform;
            var size = new Vector2(GridWidth, rows * SlotSize + (rows - 1) * SlotSpacing);
            SetRect(rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), size, new Vector2(0f, top));

            var grid = gridObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(SlotSize, SlotSize);
            grid.spacing = new Vector2(SlotSpacing, SlotSpacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.childAlignment = TextAnchor.UpperCenter;
            return rect;
        }

        private SlotView CreateSlot(RectTransform parent, int index, float rimAlpha)
        {
            var slotObject = CreateUiObject($"InventorySlot_{index}", parent);
            var background = slotObject.AddComponent<CutCornerGraphic>();
            background.SetShape(SlotCorner, Shadow);
            background.SetBands(SlotInnerShadow, 5f, new Color(BarkPale.r, BarkPale.g, BarkPale.b, rimAlpha), 3f);
            background.raycastTarget = true;
            slotObject.AddComponent<InventorySlotPointer>().Initialize(this, index);

            var item = CreateUiObject("ItemColor", slotObject.transform).AddComponent<Image>();
            item.sprite = GetUiSprite();
            item.raycastTarget = false;
            SetRect(item.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(ItemSize, ItemSize), Vector2.zero);

            var count = CreateQuantity(slotObject.transform, new Vector2(-8f, 3f));
            return new SlotView(background, item, count);
        }

        private Text CreateQuantity(Transform parent, Vector2 bottomRightOffset)
        {
            var count = CreateUiObject("Quantity", parent).AddComponent<Text>();
            count.font = font;
            count.fontSize = 22;
            count.alignment = TextAnchor.LowerRight;
            count.color = Cream;
            count.raycastTarget = false;
            count.horizontalOverflow = HorizontalWrapMode.Overflow;
            count.verticalOverflow = VerticalWrapMode.Overflow;
            var rect = count.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(0f, bottomRightOffset.y);
            rect.offsetMax = new Vector2(bottomRightOffset.x, 0f);
            AddShadow(count, Night, 2f);
            return count;
        }

        /// <summary>The dashed line between the items and the quick bar.</summary>
        private static void CreateDashes(RectTransform panel, float top)
        {
            const float dash = 6f;
            const float gap = 4f;
            var line = CreateUiObject("Separator", panel);
            var rect = (RectTransform)line.transform;
            SetRect(rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(GridWidth, 2f), new Vector2(0f, top));

            for (var x = 0f; x < GridWidth; x += dash + gap)
            {
                var piece = CreateUiObject("Dash", rect).AddComponent<Image>();
                piece.sprite = GetUiSprite();
                piece.color = Dashes;
                piece.raycastTarget = false;
                var pieceRect = piece.rectTransform;
                pieceRect.anchorMin = pieceRect.anchorMax = new Vector2(0f, 0.5f);
                pieceRect.pivot = new Vector2(0f, 0.5f);
                pieceRect.sizeDelta = new Vector2(Mathf.Min(dash, GridWidth - x), 2f);
                pieceRect.anchoredPosition = new Vector2(x, 0f);
            }
        }

        /// <summary>The controls, at the bottom of the screen: each key in a wooden pill, then what it does.</summary>
        private void CreateHints()
        {
            var rowObject = CreateUiObject("Hints", canvasRect);
            var row = (RectTransform)rowObject.transform;
            SetRect(row, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(ReferenceResolution.x, 28f), new Vector2(0f, -HintsTop));
            var layout = rowObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 28f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            foreach (var (key, action) in Hints)
            {
                var hintObject = CreateUiObject("Hint", row);
                var hint = hintObject.AddComponent<HorizontalLayoutGroup>();
                hint.spacing = 8f;
                hint.childAlignment = TextAnchor.MiddleCenter;
                hint.childControlWidth = true;
                hint.childControlHeight = true;
                hint.childForceExpandWidth = false;
                hint.childForceExpandHeight = false;

                var pillObject = CreateUiObject("Key", hintObject.transform);
                var pill = pillObject.AddComponent<CutCornerGraphic>();
                pill.SetShape(5f, WoodDark);
                pill.raycastTarget = false;
                var pillLayout = pillObject.AddComponent<HorizontalLayoutGroup>();
                pillLayout.padding = new RectOffset(10, 10, 2, 4);
                pillLayout.childControlWidth = true;
                pillLayout.childControlHeight = true;
                // Forced expansion would make the pill flexible: the row's spare width would stretch it.
                pillLayout.childForceExpandWidth = false;
                pillLayout.childForceExpandHeight = false;
                CreateHintText(pillObject.transform, key, Cream);

                CreateHintText(hintObject.transform, action, HintText);
            }
        }

        private void CreateHintText(Transform parent, string entryKey, Color textColor)
        {
            var text = CreateUiObject("Text", parent).AddComponent<Text>();
            text.font = font;
            text.fontSize = 17;
            text.color = textColor;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.gameObject.AddComponent<LocalizedLabel>().Initialize(entryKey);
        }

        /// <summary>What the mouse holds: its square, tilted, with a shadow under it and the number.</summary>
        private void CreateHeld()
        {
            var heldObject = CreateUiObject("HeldItem", canvasRect);
            held = (RectTransform)heldObject.transform;
            SetRect(held, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(HeldSize, HeldSize), Vector2.zero);
            held.localRotation = Quaternion.Euler(0f, 0f, HeldTiltDegrees);

            heldItem = heldObject.AddComponent<Image>();
            heldItem.sprite = GetUiSprite();
            heldItem.raycastTarget = false;
            var shadow = heldObject.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = HeldShadow;
            shadow.effectDistance = new Vector2(0f, -6f);

            heldCount = CreateQuantity(held, new Vector2(2f, -6f));
        }

        private void FollowPointer(Vector2 screenPosition)
        {
            if (held == null)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, null, out var local);
            held.anchoredPosition = local;
        }

        private void RefreshSlots()
        {
            if (slotViews == null)
            {
                return;
            }

            for (var index = 0; index < slotViews.Length; index++)
            {
                slotViews[index].Show(inventory.GetSlot(index));
            }
        }

        private void RefreshHeld()
        {
            if (held == null)
            {
                return;
            }

            held.gameObject.SetActive(hand.IsHolding);
            if (hand.IsHolding)
            {
                heldItem.color = hand.Item.Color;
                heldCount.text = QuantityLabel(hand.Quantity);
                held.SetAsLastSibling();
            }
        }

        /// <summary>A single item shows no number.</summary>
        private static string QuantityLabel(int quantity)
        {
            return quantity > 1 ? quantity.ToString() : string.Empty;
        }

        private Text CreateLabel(string labelName, RectTransform parent, string entryKey, int fontSize, Color textColor, Vector2 top, Vector2 size)
        {
            var label = CreateUiObject(labelName, parent).AddComponent<Text>();
            label.font = font;
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = textColor;
            label.raycastTarget = false;
            // A line taller than its box would be dropped, not clipped.
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            SetRect(label.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), size, top);
            label.gameObject.AddComponent<LocalizedLabel>().Initialize(entryKey);
            return label;
        }

        private static void AddShadow(Text text, Color shadowColor, float depth)
        {
            var shadow = text.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = shadowColor;
            shadow.effectDistance = new Vector2(0f, -depth);
        }

        private static GameObject CreateUiObject(string objectName, Transform parent)
        {
            var created = new GameObject(objectName, typeof(RectTransform));
            created.transform.SetParent(parent, false);
            return created;
        }

        private static Sprite GetUiSprite()
        {
            if (uiSprite != null)
            {
                return uiSprite;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "InventoryUiPixel",
                hideFlags = HideFlags.HideAndDontSave,
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            uiSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            uiSprite.name = "InventoryUiSprite";
            uiSprite.hideFlags = HideFlags.HideAndDontSave;
            return uiSprite;
        }

        /// <summary>Anchored on a point of its parent, its pivot on the same point, offset by <paramref name="position"/>.</summary>
        private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 position)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void CreateEventSystemIfNeeded()
        {
            var eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
            }

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }

        private sealed class SlotView
        {
            private readonly CutCornerGraphic background;
            private readonly Image item;
            private readonly Text count;

            public SlotView(CutCornerGraphic background, Image item, Text count)
            {
                this.background = background;
                this.item = item;
                this.count = count;
            }

            public string QuantityText => count.text;

            public void Show(InventorySlot slot)
            {
                item.enabled = !slot.IsEmpty;
                item.color = slot.Color;
                count.text = slot.IsEmpty ? string.Empty : QuantityLabel(slot.Quantity);
            }

            public void SetHovered(bool hovered)
            {
                background.Brightness = hovered ? HoverBrightness : 1f;
            }
        }
    }

    /// <summary>Hands the presses and the hovering of one slot to the inventory screen.</summary>
    internal sealed class InventorySlotPointer : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private InventoryDemoController owner;
        private int index;

        public void Initialize(InventoryDemoController controller, int slotIndex)
        {
            owner = controller;
            index = slotIndex;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Null propagation bypasses Unity's overloaded == (UNT0008).
            if (owner != null)
            {
                owner.PressSlot(index, eventData.button, eventData.position);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (owner != null)
            {
                owner.SetHovered(index, true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (owner != null)
            {
                owner.SetHovered(index, false);
            }
        }
    }
}
