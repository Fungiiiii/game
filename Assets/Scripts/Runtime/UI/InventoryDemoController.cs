using System.Collections;
using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Builds a Minecraft-inspired inventory: a 3-column by 4-row item panel and a
    /// three-slot quick bar. Slots support cursor pickup, stack splitting, drag
    /// distribution, stack merges and swaps.
    /// </summary>
    public sealed class InventoryDemoController : MonoBehaviour
    {
        public const int MainColumnCount = 3;
        public const int MainRowCount = 4;
        public const int MainSlotCount = MainColumnCount * MainRowCount;
        public const int HotbarSlotCount = 3;
        public const int TotalSlotCount = MainSlotCount + HotbarSlotCount;

        [SerializeField] private int demoMaxStackSize = 5;

        private const string DemoMushroomId = "demo_mushroom";
        private const string DemoHerbId = "demo_herb";
        private const string DemoSporeId = "demo_spore";
        private const string DemoCrystalId = "demo_crystal";

        private static readonly Color PanelColor = new Color(0.035f, 0.045f, 0.055f, 0.98f);
        private static readonly Color PanelBorderColor = new Color(0.24f, 0.27f, 0.3f, 1f);
        private static readonly Color SlotColor = new Color(0.12f, 0.14f, 0.16f, 1f);
        private static readonly Color HotbarSlotColor = new Color(0.16f, 0.18f, 0.2f, 1f);
        private static readonly Color TitleColor = new Color(0.93f, 0.89f, 0.74f, 1f);
        private const float DoubleClickWindowSeconds = 0.22f;

        private static Sprite uiSprite;

        private PlayerInventory inventory;
        private SlotView[] slotViews;
        private RectTransform dragLayer;
        private GameObject dragGhost;
        private UnityEngine.UI.Text dragGhostCount;
        private InventoryItemDefinition carriedItem;
        private int carriedQuantity;
        private Coroutine pendingLeftClick;
        private int pendingLeftClickIndex = -1;
        private Vector2 pendingLeftClickPosition;
        private Camera pendingLeftClickCamera;

        public PlayerInventory Inventory => inventory;

        private bool IsCarrying => carriedQuantity > 0;

        private void Awake()
        {
            inventory = new PlayerInventory(TotalSlotCount);

            CreateEventSystemIfNeeded();
            BuildUi();
            inventory.Changed += RefreshUi;
            SeedDemoItems();
        }

        private void Update()
        {
            if (IsCarrying && Mouse.current != null)
            {
                UpdateDragGhost(Mouse.current.position.ReadValue(), null);
            }
        }

        private void OnDestroy()
        {
            CancelPendingLeftClick();

            if (inventory != null)
            {
                inventory.Changed -= RefreshUi;
            }

            DestroyDragGhost();
        }

        private void SeedDemoItems()
        {
            InventoryItemDefinition mushroom = new InventoryItemDefinition(
                DemoMushroomId,
                new Color(0.95f, 0.52f, 0.18f, 1f),
                demoMaxStackSize);
            InventoryItemDefinition herb = new InventoryItemDefinition(
                DemoHerbId,
                new Color(0.25f, 0.78f, 0.35f, 1f),
                10);
            InventoryItemDefinition spore = new InventoryItemDefinition(
                DemoSporeId,
                new Color(0.55f, 0.35f, 0.92f, 1f),
                16);
            InventoryItemDefinition crystal = new InventoryItemDefinition(
                DemoCrystalId,
                new Color(0.25f, 0.75f, 0.95f, 1f),
                4);

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
            GameObject canvasObject = CreateUiObject("InventoryCanvas", transform);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            UnityEngine.UI.CanvasScaler scaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            dragLayer = canvasObject.GetComponent<RectTransform>();

            GameObject panel = CreateUiObject("InventoryPanel", canvasObject.transform);
            UnityEngine.UI.Image panelImage = panel.AddComponent<UnityEngine.UI.Image>();
            panelImage.sprite = GetUiSprite();
            panelImage.color = PanelColor;
            panelImage.raycastTarget = false;
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            SetRect(panelRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 640f), Vector2.zero);

            GameObject border = CreateUiObject("InventoryPanelBorder", panel.transform);
            UnityEngine.UI.Image borderImage = border.AddComponent<UnityEngine.UI.Image>();
            borderImage.sprite = GetUiSprite();
            borderImage.color = PanelBorderColor;
            borderImage.raycastTarget = false;
            RectTransform borderRect = border.GetComponent<RectTransform>();
            SetRect(borderRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(536f, 616f), Vector2.zero);
            border.transform.SetAsFirstSibling();

            CreateLabel("InventoryTitle", panel.transform, "INVENTORY", 30, TitleColor, TextAnchor.MiddleCenter, new Vector2(0f, 276f), new Vector2(500f, 48f));
            CreateLabel("InventorySubtitle", panel.transform, "LEFT CLICK PICK UP  /  RIGHT CLICK SPLIT  /  DOUBLE CLICK STACK", 13, new Color(0.7f, 0.73f, 0.76f, 1f), TextAnchor.MiddleCenter, new Vector2(0f, 242f), new Vector2(520f, 28f));

            GameObject gridObject = CreateUiObject("InventoryGrid_3x4", panel.transform);
            RectTransform gridRect = gridObject.GetComponent<RectTransform>();
            SetRect(gridRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(390f, 350f), new Vector2(0f, 55f));
            UnityEngine.UI.GridLayoutGroup grid = gridObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            grid.cellSize = new Vector2(116f, 76f);
            grid.spacing = new Vector2(10f, 10f);
            grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = MainColumnCount;
            grid.childAlignment = TextAnchor.MiddleCenter;

            slotViews = new SlotView[TotalSlotCount];
            for (int i = 0; i < MainSlotCount; i++)
            {
                slotViews[i] = CreateSlotView(gridObject.transform, i, false);
            }

            CreateLabel("HotbarTitle", panel.transform, "QUICK BAR", 18, TitleColor, TextAnchor.MiddleCenter, new Vector2(0f, -142f), new Vector2(500f, 34f));

            GameObject hotbarObject = CreateUiObject("Hotbar_3Slots", panel.transform);
            RectTransform hotbarRect = hotbarObject.GetComponent<RectTransform>();
            SetRect(hotbarRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(390f, 92f), new Vector2(0f, -214f));
            UnityEngine.UI.GridLayoutGroup hotbarGrid = hotbarObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            hotbarGrid.cellSize = new Vector2(116f, 76f);
            hotbarGrid.spacing = new Vector2(10f, 10f);
            hotbarGrid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            hotbarGrid.constraintCount = HotbarSlotCount;
            hotbarGrid.childAlignment = TextAnchor.MiddleCenter;

            for (int i = 0; i < HotbarSlotCount; i++)
            {
                int slotIndex = MainSlotCount + i;
                slotViews[slotIndex] = CreateSlotView(hotbarObject.transform, slotIndex, true);
            }

            RefreshUi();
        }

        private SlotView CreateSlotView(Transform parent, int index, bool hotbar)
        {
            GameObject slot = CreateUiObject($"InventorySlot_{index}", parent);
            UnityEngine.UI.Image background = slot.AddComponent<UnityEngine.UI.Image>();
            background.sprite = GetUiSprite();
            background.color = hotbar ? HotbarSlotColor : SlotColor;
            background.raycastTarget = true;

            InventorySlotDragHandler dragHandler = slot.AddComponent<InventorySlotDragHandler>();
            dragHandler.Initialize(this, index);

            GameObject item = CreateUiObject("ItemColor", slot.transform);
            UnityEngine.UI.Image itemImage = item.AddComponent<UnityEngine.UI.Image>();
            itemImage.sprite = GetUiSprite();
            itemImage.raycastTarget = false;
            RectTransform itemRect = item.GetComponent<RectTransform>();
            SetRect(itemRect, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(52f, 52f), Vector2.zero);

            GameObject count = CreateUiObject("Quantity", slot.transform);
            UnityEngine.UI.Text countText = count.AddComponent<UnityEngine.UI.Text>();
            countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            countText.fontSize = 22;
            countText.fontStyle = FontStyle.Bold;
            countText.alignment = TextAnchor.LowerRight;
            countText.color = Color.white;
            countText.raycastTarget = false;
            RectTransform countRect = count.GetComponent<RectTransform>();
            SetRect(countRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(-14f, -10f), new Vector2(-6f, -6f));

            return new SlotView(itemImage, countText);
        }

        internal void ClickSlot(int index, PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                CancelPendingLeftClick();
                HandleRightClick(index, eventData.position, eventData.pressEventCamera);
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (eventData.clickCount >= 2)
                {
                    CancelPendingLeftClick();
                    HandleDoubleClick(index, eventData);
                    return;
                }

                ScheduleSingleLeftClick(index, eventData);
            }
        }

        private void ScheduleSingleLeftClick(int index, PointerEventData eventData)
        {
            CancelPendingLeftClick();
            pendingLeftClickIndex = index;
            pendingLeftClickPosition = eventData.position;
            pendingLeftClickCamera = eventData.pressEventCamera;
            pendingLeftClick = StartCoroutine(ExecuteSingleLeftClick());
        }

        private IEnumerator ExecuteSingleLeftClick()
        {
            yield return new WaitForSeconds(DoubleClickWindowSeconds);

            if (pendingLeftClickIndex >= 0)
            {
                HandleLeftClick(pendingLeftClickIndex, pendingLeftClickPosition, pendingLeftClickCamera);
            }

            pendingLeftClick = null;
            pendingLeftClickIndex = -1;
        }

        private void CancelPendingLeftClick()
        {
            if (pendingLeftClick != null)
            {
                StopCoroutine(pendingLeftClick);
                pendingLeftClick = null;
            }

            pendingLeftClickIndex = -1;
        }

        private void HandleDoubleClick(int index, PointerEventData eventData)
        {
            if (IsCarrying)
            {
                HandleLeftClick(index, eventData.position, eventData.pressEventCamera);
                return;
            }

            InventorySlot slot = inventory.GetSlot(index);
            if (!slot.IsEmpty)
            {
                inventory.StackAll(slot.ItemId, index);
            }
        }

        private void HandleLeftClick(int index, Vector2 screenPosition, Camera eventCamera)
        {
            if (!IsCarrying)
            {
                PickUpAll(index, screenPosition, eventCamera);
                return;
            }

            DropCarried(index, false, screenPosition, eventCamera);
        }

        private void HandleRightClick(int index, Vector2 screenPosition, Camera eventCamera)
        {
            if (!IsCarrying)
            {
                PickUpHalf(index, screenPosition, eventCamera);
                return;
            }

            DropCarried(index, true, screenPosition, eventCamera);
        }

        private void PickUpAll(int index, Vector2 screenPosition, Camera eventCamera)
        {
            InventorySlot slot = inventory.GetSlot(index);
            if (slot.IsEmpty)
            {
                return;
            }

            carriedQuantity = inventory.TakeFromSlot(index, slot.Quantity, out carriedItem);
            CreateDragGhost(screenPosition, eventCamera);
        }

        private void PickUpHalf(int index, Vector2 screenPosition, Camera eventCamera)
        {
            InventorySlot slot = inventory.GetSlot(index);
            if (slot.IsEmpty)
            {
                return;
            }

            int halfQuantity = Mathf.CeilToInt(slot.Quantity / 2f);
            carriedQuantity = inventory.TakeFromSlot(index, halfQuantity, out carriedItem);
            CreateDragGhost(screenPosition, eventCamera);
        }

        private void DropCarried(int index, bool oneOnly, Vector2 screenPosition, Camera eventCamera)
        {
            if (!IsCarrying)
            {
                return;
            }

            InventorySlot target = inventory.GetSlot(index);
            if (target.IsEmpty || IsCompatible(target, carriedItem))
            {
                int requestedQuantity = oneOnly ? 1 : carriedQuantity;
                int added = inventory.AddToSlot(index, carriedItem, requestedQuantity);
                carriedQuantity -= added;
                UpdateDragGhost(screenPosition, eventCamera);
                RefreshDragGhost();
                return;
            }

            // A left click on a different item swaps the carried stack and the target stack.
            if (oneOnly)
            {
                return;
            }

            InventoryItemDefinition targetItem;
            int targetQuantity = inventory.TakeFromSlot(index, target.Quantity, out targetItem);
            int addedToTarget = inventory.AddToSlot(index, carriedItem, carriedQuantity);
            if (addedToTarget != carriedQuantity)
            {
                inventory.AddToSlot(index, targetItem, targetQuantity);
                return;
            }

            carriedItem = targetItem;
            carriedQuantity = targetQuantity;
            UpdateDragGhost(screenPosition, eventCamera);
            RefreshDragGhost();
        }

        private void CreateDragGhost(Vector2 screenPosition, Camera eventCamera)
        {
            DestroyDragGhost();
            dragGhost = CreateUiObject("DraggedItem", dragLayer);

            UnityEngine.UI.Image ghostImage = dragGhost.AddComponent<UnityEngine.UI.Image>();
            ghostImage.sprite = GetUiSprite();
            ghostImage.color = new Color(carriedItem.Color.r, carriedItem.Color.g, carriedItem.Color.b, 0.78f);
            ghostImage.raycastTarget = false;

            GameObject quantityObject = CreateUiObject("Quantity", dragGhost.transform);
            dragGhostCount = quantityObject.AddComponent<UnityEngine.UI.Text>();
            dragGhostCount.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dragGhostCount.fontSize = 20;
            dragGhostCount.fontStyle = FontStyle.Bold;
            dragGhostCount.alignment = TextAnchor.LowerRight;
            dragGhostCount.color = Color.white;
            dragGhostCount.raycastTarget = false;
            SetRect(quantityObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(-12f, -8f), new Vector2(-5f, -5f));

            RectTransform ghostRect = dragGhost.GetComponent<RectTransform>();
            SetRect(ghostRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(58f, 58f), Vector2.zero);
            dragGhost.transform.SetAsLastSibling();
            UpdateDragGhost(screenPosition, eventCamera);
            RefreshDragGhost();
        }

        private void UpdateDragGhost(Vector2 screenPosition, Camera eventCamera)
        {
            if (dragGhost == null || dragLayer == null)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragLayer,
                screenPosition,
                eventCamera,
                out Vector2 localPosition);
            dragGhost.GetComponent<RectTransform>().anchoredPosition = localPosition;
        }

        private void RefreshDragGhost()
        {
            if (!IsCarrying)
            {
                carriedItem = default;
                DestroyDragGhost();
                return;
            }

            if (dragGhostCount != null)
            {
                dragGhostCount.text = carriedQuantity.ToString();
            }
        }

        private void DestroyDragGhost()
        {
            if (dragGhost != null)
            {
                Destroy(dragGhost);
                dragGhost = null;
            }

            dragGhostCount = null;
        }

        private void RefreshUi()
        {
            if (slotViews == null)
            {
                return;
            }

            for (int i = 0; i < slotViews.Length; i++)
            {
                slotViews[i].Set(inventory.GetSlot(i));
            }
        }

        private static bool IsCompatible(InventorySlot slot, InventoryItemDefinition item)
        {
            return slot.ItemId == item.Id &&
                   slot.MaxStackSize == item.MaxStackSize &&
                   slot.Color == item.Color;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject created = new GameObject(name, typeof(RectTransform));
            created.transform.SetParent(parent, false);
            return created;
        }

        private static UnityEngine.UI.Text CreateLabel(
            string name,
            Transform parent,
            string value,
            int fontSize,
            Color color,
            TextAnchor alignment,
            Vector2 position,
            Vector2 size)
        {
            GameObject labelObject = CreateUiObject(name, parent);
            UnityEngine.UI.Text label = labelObject.AddComponent<UnityEngine.UI.Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.fontStyle = FontStyle.Bold;
            label.alignment = alignment;
            label.color = color;
            label.text = value;
            label.raycastTarget = false;
            SetRect(label.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, position);
            return label;
        }

        private static Sprite GetUiSprite()
        {
            if (uiSprite != null)
            {
                return uiSprite;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "InventoryUiPixel",
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            uiSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            uiSprite.name = "InventoryUiSprite";
            uiSprite.hideFlags = HideFlags.HideAndDontSave;
            return uiSprite;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 position)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void CreateEventSystemIfNeeded()
        {
            EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemObject = new GameObject("EventSystem");
                eventSystem = eventSystemObject.AddComponent<EventSystem>();
            }

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }

        private sealed class SlotView
        {
            private readonly UnityEngine.UI.Image itemImage;
            private readonly UnityEngine.UI.Text countText;

            public SlotView(UnityEngine.UI.Image itemImage, UnityEngine.UI.Text countText)
            {
                this.itemImage = itemImage;
                this.countText = countText;
            }

            public void Set(InventorySlot slot)
            {
                itemImage.enabled = !slot.IsEmpty;
                itemImage.color = slot.Color;
                countText.text = slot.IsEmpty ? string.Empty : slot.Quantity.ToString();
            }
        }
    }

    internal sealed class InventorySlotDragHandler : MonoBehaviour, IPointerClickHandler
    {
        private InventoryDemoController owner;
        private int index;

        public void Initialize(InventoryDemoController controller, int slotIndex)
        {
            owner = controller;
            index = slotIndex;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Null propagation bypasses Unity's overloaded == (UNT0008).
            if (owner != null)
            {
                owner.ClickSlot(index, eventData);
            }
        }
    }
}
