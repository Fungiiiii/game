using System;
using Fungiiiii.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Builds a deliberately small color-coded inventory UI for the POC scene.
    /// The plus/minus controls keep the placeholder independent of localized labels.
    /// </summary>
    public sealed class InventoryDemoController : MonoBehaviour
    {
        [SerializeField] private int slotCount = 6;
        [SerializeField] private int demoMaxStackSize = 5;
        [SerializeField] private Color demoItemColor = new Color(0.95f, 0.52f, 0.18f, 1f);

        private const string DemoItemId = "demo_mushroom";

        private PlayerInventory inventory;
        private SlotView[] slotViews;
        private InventoryItemDefinition demoItem;
        private static Sprite uiSprite;

        public PlayerInventory Inventory => inventory;

        private void Awake()
        {
            inventory = new PlayerInventory(slotCount);
            demoItem = new InventoryItemDefinition(DemoItemId, demoItemColor, demoMaxStackSize);

            CreateEventSystemIfNeeded();
            BuildUi();
            inventory.Changed += RefreshUi;

            // Start with one item so the stack slot is immediately visible in the POC.
            inventory.Add(demoItem, 1);
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.Changed -= RefreshUi;
            }
        }

        private void AddDemoItem()
        {
            inventory.Add(demoItem, 1);
        }

        private void RemoveDemoItem()
        {
            inventory.Remove(DemoItemId, 1);
        }

        private void BuildUi()
        {
            GameObject canvasObject = new GameObject("InventoryCanvas");
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            UnityEngine.UI.CanvasScaler scaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            GameObject panel = CreateUiObject("InventoryPanel", canvasObject.transform);
            UnityEngine.UI.Image panelImage = panel.AddComponent<UnityEngine.UI.Image>();
            panelImage.sprite = GetUiSprite();
            panelImage.color = new Color(0.035f, 0.06f, 0.075f, 0.96f);
            panelImage.raycastTarget = false;
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            SetRect(panelRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 360f), Vector2.zero);

            GameObject slotsObject = CreateUiObject("InventorySlots", panel.transform);
            RectTransform slotsRect = slotsObject.GetComponent<RectTransform>();
            SetRect(slotsRect, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), new Vector2(440f, 220f), Vector2.zero);
            UnityEngine.UI.GridLayoutGroup grid = slotsObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            grid.cellSize = new Vector2(128f, 96f);
            grid.spacing = new Vector2(12f, 12f);
            grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.childAlignment = TextAnchor.MiddleCenter;

            slotViews = new SlotView[inventory.SlotCount];
            for (int i = 0; i < slotViews.Length; i++)
            {
                slotViews[i] = CreateSlotView(slotsObject.transform, i);
            }

            GameObject controls = CreateUiObject("InventoryControls", panel.transform);
            RectTransform controlsRect = controls.GetComponent<RectTransform>();
            SetRect(controlsRect, new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.5f), new Vector2(240f, 72f), Vector2.zero);
            UnityEngine.UI.HorizontalLayoutGroup controlsLayout = controls.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            controlsLayout.spacing = 24f;
            controlsLayout.childAlignment = TextAnchor.MiddleCenter;
            controlsLayout.childControlWidth = false;
            controlsLayout.childControlHeight = false;
            controlsLayout.childForceExpandWidth = false;
            controlsLayout.childForceExpandHeight = false;

            CreateControlButton(controls.transform, "+", new Color(0.13f, 0.58f, 0.31f, 1f), AddDemoItem);
            CreateControlButton(controls.transform, "−", new Color(0.67f, 0.18f, 0.18f, 1f), RemoveDemoItem);

            RefreshUi();
        }

        private SlotView CreateSlotView(Transform parent, int index)
        {
            GameObject slot = CreateUiObject($"InventorySlot_{index}", parent);
            UnityEngine.UI.Image background = slot.AddComponent<UnityEngine.UI.Image>();
            background.sprite = GetUiSprite();
            background.color = new Color(0.10f, 0.15f, 0.17f, 1f);
            background.raycastTarget = false;

            GameObject item = CreateUiObject("ItemColor", slot.transform);
            UnityEngine.UI.Image itemImage = item.AddComponent<UnityEngine.UI.Image>();
            itemImage.sprite = GetUiSprite();
            itemImage.raycastTarget = false;
            RectTransform itemRect = item.GetComponent<RectTransform>();
            SetRect(itemRect, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(64f, 52f), Vector2.zero);

            GameObject count = CreateUiObject("Quantity", slot.transform);
            UnityEngine.UI.Text countText = count.AddComponent<UnityEngine.UI.Text>();
            countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            countText.fontSize = 28;
            countText.fontStyle = FontStyle.Bold;
            countText.alignment = TextAnchor.LowerRight;
            countText.color = Color.white;
            countText.raycastTarget = false;
            RectTransform countRect = count.GetComponent<RectTransform>();
            SetRect(countRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(-16f, -12f), new Vector2(-8f, -8f));

            return new SlotView(itemImage, countText);
        }

        private void CreateControlButton(Transform parent, string icon, Color color, Action callback)
        {
            GameObject buttonObject = CreateUiObject($"Control_{icon}", parent);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(88f, 64f);

            UnityEngine.UI.Image image = buttonObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = GetUiSprite();
            image.color = color;
            UnityEngine.UI.Button button = buttonObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => callback());

            GameObject labelObject = CreateUiObject("Icon", buttonObject.transform);
            UnityEngine.UI.Text label = labelObject.AddComponent<UnityEngine.UI.Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 38;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.text = icon;
            label.raycastTarget = false;
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            SetRect(labelRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        }

        private void RefreshUi()
        {
            if (slotViews == null)
            {
                return;
            }

            for (int i = 0; i < slotViews.Length; i++)
            {
                InventorySlot slot = inventory.GetSlot(i);
                slotViews[i].Set(slot);
            }
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject created = new GameObject(name, typeof(RectTransform));
            created.transform.SetParent(parent, false);
            return created;
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
}
