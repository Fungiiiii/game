using System;
using System.Collections.Generic;
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
    public sealed class InventoryScreen : MonoBehaviour, IGameScreen, IHintSource
    {
        /// <summary>The bag is drawn 3 slots wide, as on the artboard; the panel is sized for its 4 rows.</summary>
        private const int BagColumnCount = 3;

        private const int NoSlot = -1;

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

        private static Sprite uiSprite;

        private PlayerInventory inventory;
        private InventoryLayout layout = InventoryLayout.Standard;
        private InventoryHand hand;
        private SlotView[] slotViews;
        private Selectable[] slotSelectables;
        private int focusedSlot = NoSlot;
        private InputAction navigateAction;
        private InputAction takeHalfAction;
        private InputAction gatherAllAction;
        private ControlHint[] hints = new ControlHint[0];
        private RectTransform canvasRect;
        private RectTransform held;
        private ItemIcon heldItem;
        private Text heldCount;
        private Text title;
        private Font font;

        public PlayerInventory Inventory => inventory;

        /// <summary>Which slots the screen draws as the bag and which as the quick bar.</summary>
        public InventoryLayout Layout => layout;

        private int BagRowCount => (layout.BagSlotCount + BagColumnCount - 1) / BagColumnCount;

        /// <summary>
        /// Shows the inventory a player carries. Call it before Awake runs, that is on an
        /// inactive GameObject. Without it, the screen makes its own inventory, for the
        /// standalone prototype scene.
        /// </summary>
        public void Bind(PlayerInventory playerInventory, InventoryLayout inventoryLayout)
        {
            if (playerInventory == null)
            {
                throw new System.ArgumentNullException(nameof(playerInventory));
            }

            if (playerInventory.SlotCount < inventoryLayout.SlotCount)
            {
                throw new System.ArgumentException(
                    $"The layout has {inventoryLayout.SlotCount} slots; this inventory has {playerInventory.SlotCount}.",
                    nameof(playerInventory));
            }

            inventory = playerInventory;
            layout = inventoryLayout;
        }

        /// <summary>
        /// The font of the screen. Set it before Awake runs, like <see cref="Bind"/>;
        /// without one, Unity's built-in font is used.
        /// </summary>
        // It has no button of its own that asks to close: Tab and Escape close it.
        event Action IGameScreen.CloseRequested
        {
            add { }
            remove { }
        }

        /// <summary>Shows the screen; the screen stack calls it.</summary>
        public void Open()
        {
            gameObject.SetActive(true);
        }

        /// <summary>Hides the screen, putting back what the mouse held.</summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// The controls of the screen. With a gamepad, the first move selects a slot, X takes
        /// half or puts down one, Y gathers every stack of the item; A and the stick are the
        /// Submit and Navigate of the event system. Close and Cancel only show in the key hints:
        /// the screen stack closes the screen.
        /// </summary>
        public void SetControls(InventoryScreenControls controls)
        {
            navigateAction = controls.Navigate;
            takeHalfAction = controls.TakeHalf;
            gatherAllAction = controls.GatherAll;

            // The mouse buttons and A are read by the event system, not by an action of the game.
            hints = new[]
            {
                new ControlHint(UiText.HintTakeOrPlace, HintInput.At("<Mouse>/leftButton"), HintInput.At("<Gamepad>/buttonSouth")),
                new ControlHint(UiText.HintHalfOrOne, HintInput.At("<Mouse>/rightButton"), HintInput.Of(controls.TakeHalf)),
                new ControlHint(UiText.HintGatherAll, HintInput.At("<Mouse>/leftButton"), HintInput.Of(controls.GatherAll), twiceWithMouse: true),
                new ControlHint(UiText.HintClose, HintInput.Of(controls.Close), HintInput.Of(controls.Cancel)),
            };
        }

        // The screen's hints never change while it is open.
        event Action IHintSource.HintsChanged
        {
            add { }
            remove { }
        }

        /// <summary>What the mouse or the gamepad does on the screen, for the key hints.</summary>
        public IReadOnlyList<ControlHint> Hints => hints;

        /// <summary>The slot the mouse is over or the gamepad has selected, -1 for none.</summary>
        internal int FocusedSlot => focusedSlot;

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
            if (inventory == null)
            {
                inventory = new PlayerInventory(layout.SlotCount);
            }

            hand = new InventoryHand(inventory);
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            CreateEventSystemIfNeeded();
            BuildUi();
            inventory.Changed += RefreshSlots;
            hand.Changed += RefreshHeld;

            RefreshSlots();
        }

        private void OnDisable()
        {
            // Closing the screen puts back what the mouse held.
            hand?.PutBack();
            Unfocus();
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (hand.IsHolding && mouse != null && mouse.delta.ReadValue() != Vector2.zero)
            {
                FollowPointer(mouse.position.ReadValue());
            }

            ReadGamepad();
        }

        private void ReadGamepad()
        {
            if (focusedSlot == NoSlot)
            {
                // Nothing to move from yet: the first push selects the first slot.
                if (navigateAction != null && navigateAction.WasPerformedThisFrame() && navigateAction.ReadValue<Vector2>() != Vector2.zero)
                {
                    EventSystem.current.SetSelectedGameObject(slotSelectables[0].gameObject);
                }

                return;
            }

            if (takeHalfAction != null && takeHalfAction.WasPressedThisFrame())
            {
                TakeHalfOnFocusedSlot();
            }

            if (gatherAllAction != null && gatherAllAction.WasPressedThisFrame())
            {
                GatherAllOnFocusedSlot();
            }
        }

        /// <summary>X on a gamepad, the right click of the mouse.</summary>
        internal void TakeHalfOnFocusedSlot()
        {
            PressSlot(focusedSlot, PointerEventData.InputButton.Right, ScreenPointOf(focusedSlot));
        }

        /// <summary>Y on a gamepad, the double click of the mouse.</summary>
        internal void GatherAllOnFocusedSlot()
        {
            hand.GatherAll(focusedSlot);
            FollowPointer(ScreenPointOf(focusedSlot));
        }

        /// <summary>A slot gets the focus, from the mouse over it or the gamepad selecting it; the held item follows a gamepad.</summary>
        internal void Focus(int index, bool focused)
        {
            slotViews[index].SetHovered(focused);
            if (focused)
            {
                focusedSlot = index;
                var mouse = Mouse.current;
                if (hand.IsHolding && (mouse == null || mouse.delta.ReadValue() == Vector2.zero))
                {
                    FollowPointer(ScreenPointOf(index));
                }
            }
            else if (focusedSlot == index)
            {
                focusedSlot = NoSlot;
            }
        }

        /// <summary>Where a slot stands on the screen, in pixels.</summary>
        internal Vector2 ScreenPointOf(int index)
        {
            return slotSelectables[index].transform.position;
        }

        /// <summary>
        /// No slot keeps the focus once the screen is closed: the event system cannot tell a
        /// hidden slot it lost it, so the highlight would still show on the next opening.
        /// </summary>
        private void Unfocus()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem != null && focusedSlot != NoSlot && eventSystem.currentSelectedGameObject == slotSelectables[focusedSlot].gameObject)
            {
                eventSystem.SetSelectedGameObject(null);
            }

            if (slotViews != null)
            {
                foreach (var view in slotViews)
                {
                    view.SetHovered(false);
                }
            }

            focusedSlot = NoSlot;
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

            var grid = CreateGrid("InventoryGrid_3x4", panel, BagColumnCount, BagRowCount, -84f);
            slotViews = new SlotView[layout.SlotCount];
            slotSelectables = new Selectable[layout.SlotCount];
            for (var index = 0; index < layout.BagSlotCount; index++)
            {
                slotViews[index] = CreateSlot(grid, index, 0.22f);
            }

            CreateDashes(panel, -452f);
            CreateLabel("HotbarTitle", panel, UiText.InventoryHotbar, 20, Sand, new Vector2(0f, -464f), new Vector2(GridWidth, 26f));

            var hotbar = CreateGrid("Hotbar_3Slots", panel, layout.QuickBarSlotCount, 1, -500f);
            for (var index = 0; index < layout.QuickBarSlotCount; index++)
            {
                var slot = layout.QuickBarSlot(index);
                slotViews[slot] = CreateSlot(hotbar, slot, 0.5f);
            }

            LinkSlotNavigation();
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

            // Selectable for the gamepad; the slot draws its own highlight, so no transition.
            var selectable = slotObject.AddComponent<Selectable>();
            selectable.transition = Selectable.Transition.None;
            slotSelectables[index] = selectable;

            var itemImage = CreateUiObject("ItemIcon", slotObject.transform).AddComponent<Image>();
            itemImage.raycastTarget = false;
            SetRect(itemImage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(ItemSize, ItemSize), Vector2.zero);
            var item = itemImage.gameObject.AddComponent<ItemIcon>();

            var count = CreateQuantity(slotObject.transform, new Vector2(-8f, 3f));
            return new SlotView(background, item, count);
        }

        /// <summary>
        /// The stick and the D-pad move from slot to slot as they are laid out: the 3 x 4
        /// grid, then the quick bar under it.
        /// </summary>
        private void LinkSlotNavigation()
        {
            var rows = BagRowCount;
            for (var index = 0; index < layout.SlotCount; index++)
            {
                var isBag = !layout.IsInQuickBar(index);
                var column = isBag ? index % BagColumnCount : index - layout.QuickBarFirstSlot;
                var row = isBag ? index / BagColumnCount : rows;
                var lastColumn = isBag ? Mathf.Min(BagColumnCount, layout.BagSlotCount - row * BagColumnCount) - 1 : layout.QuickBarSlotCount - 1;

                var navigation = new Navigation { mode = Navigation.Mode.Explicit };
                navigation.selectOnLeft = column > 0 ? slotSelectables[index - 1] : null;
                navigation.selectOnRight = column < lastColumn ? slotSelectables[index + 1] : null;
                navigation.selectOnUp = row > 0 ? slotSelectables[Mathf.Min((row - 1) * BagColumnCount + column, layout.BagSlotCount - 1)] : null;
                if (isBag && index + BagColumnCount < layout.BagSlotCount)
                {
                    navigation.selectOnDown = slotSelectables[index + BagColumnCount];
                }
                else if (isBag)
                {
                    navigation.selectOnDown = slotSelectables[layout.QuickBarSlot(Mathf.Min(column, layout.QuickBarSlotCount - 1))];
                }

                slotSelectables[index].navigation = navigation;
            }
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

        /// <summary>What the mouse holds: its square, tilted, with a shadow under it and the number.</summary>
        private void CreateHeld()
        {
            var heldObject = CreateUiObject("HeldItem", canvasRect);
            held = (RectTransform)heldObject.transform;
            SetRect(held, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(HeldSize, HeldSize), Vector2.zero);
            held.localRotation = Quaternion.Euler(0f, 0f, HeldTiltDegrees);

            heldObject.AddComponent<Image>().raycastTarget = false;
            heldItem = heldObject.AddComponent<ItemIcon>();
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
                heldItem.Show(hand.Item);
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
            private readonly ItemIcon item;
            private readonly Text count;

            public SlotView(CutCornerGraphic background, ItemIcon item, Text count)
            {
                this.background = background;
                this.item = item;
                this.count = count;
            }

            public string QuantityText => count.text;

            public void Show(InventorySlot slot)
            {
                item.Show(slot.Item);
                count.text = slot.IsEmpty ? string.Empty : QuantityLabel(slot.Quantity);
            }

            public void SetHovered(bool hovered)
            {
                background.Brightness = hovered ? HoverBrightness : 1f;
            }
        }
    }

    /// <summary>Hands the presses and the hovering of one slot to the inventory screen.</summary>
    internal sealed class InventorySlotPointer : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        private InventoryScreen owner;
        private int index;

        public void Initialize(InventoryScreen controller, int slotIndex)
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

        // The mouse over a slot selects it, so that the mouse and the gamepad share one focus.
        public void OnPointerEnter(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (owner != null)
            {
                owner.Focus(index, true);
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (owner != null)
            {
                owner.Focus(index, false);
            }
        }

        /// <summary>A on a gamepad: the left click.</summary>
        public void OnSubmit(BaseEventData eventData)
        {
            if (owner != null)
            {
                owner.PressSlot(index, PointerEventData.InputButton.Left, owner.ScreenPointOf(index));
            }
        }
    }

    /// <summary>The actions the inventory screen uses or shows.</summary>
    public readonly struct InventoryScreenControls
    {
        public InventoryScreenControls(InputAction navigate, InputAction takeHalf, InputAction gatherAll, InputAction close, InputAction cancel)
        {
            Navigate = navigate;
            TakeHalf = takeHalf;
            GatherAll = gatherAll;
            Close = close;
            Cancel = cancel;
        }

        public InputAction Navigate { get; }

        public InputAction TakeHalf { get; }

        public InputAction GatherAll { get; }

        /// <summary>Closes the screen with the keyboard: Tab.</summary>
        public InputAction Close { get; }

        /// <summary>Closes the screen with a gamepad: B.</summary>
        public InputAction Cancel { get; }
    }
}
