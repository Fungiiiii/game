using Fungiiiii.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fungiiiii.Survival
{
    /// <summary>
    /// Runtime-only driver for the poison prototype scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PoisonPrototypeController : MonoBehaviour
    {
        private const float DemoMaxHealth = 100f;
        private const float DemoApplicationAmount = 10f;
        private const float CanvasWidth = 1280f;
        private const float CanvasHeight = 720f;

        private PoisonHudPresenter hudPresenter;
        private Slider poisonSlider;

        public PoisonState Poison { get; private set; }

        public float CurrentHealth { get; private set; }

        public bool IsPoisonVisible => hudPresenter != null && hudPresenter.IsPoisonVisible;

        public float PoisonPercentage => hudPresenter == null ? 0f : hudPresenter.PoisonPercentage;

        private void Awake()
        {
            Poison = new PoisonState(DemoMaxHealth);
            CurrentHealth = DemoMaxHealth;
            BuildHud();
        }

        private void Update()
        {
            if (Poison == null)
            {
                return;
            }

            ApplyDamage(Poison.TickDamage(Time.deltaTime));

            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                ApplyPoison(DemoApplicationAmount);
            }

            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                RemovePoison(DemoApplicationAmount);
            }

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                ResetPrototype();
            }
        }

        public void ApplyPoison(float amount)
        {
            Poison.Apply(amount);
        }

        public void RemovePoison(float amount)
        {
            Poison.Remove(amount);
        }

        public void ResetPrototype()
        {
            Poison.Clear();
            CurrentHealth = DemoMaxHealth;
        }

        public void Simulate(float deltaTime)
        {
            ApplyDamage(Poison.TickDamage(deltaTime));
        }

        private void ApplyDamage(float damage)
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0f, DemoMaxHealth);
        }

        private void BuildHud()
        {
            GameObject canvasObject = new GameObject(
                "PoisonCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(CanvasWidth, CanvasHeight);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject container = CreateUiObject("PoisonGauge", canvasObject.transform);
            RectTransform containerRect = container.GetComponent<RectTransform>();
            SetRect(containerRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(420f, 42f), new Vector2(0f, -32f));

            poisonSlider = CreateSlider(container.transform);
            hudPresenter = gameObject.AddComponent<PoisonHudPresenter>();
            hudPresenter.Bind(Poison, poisonSlider, container);
        }

        private static Slider CreateSlider(Transform parent)
        {
            GameObject sliderObject = CreateUiObject("PoisonSlider", parent);
            RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
            SetRect(sliderRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            Slider slider = sliderObject.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.transition = Selectable.Transition.None;
            slider.interactable = false;

            GameObject backgroundObject = CreateUiObject("Track", sliderObject.transform);
            Image background = backgroundObject.AddComponent<Image>();
            background.color = new Color(0.04f, 0.05f, 0.06f, 0.95f);
            background.raycastTarget = false;
            SetRect(background.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            GameObject fillObject = CreateUiObject("Fill", sliderObject.transform);
            Image fill = fillObject.AddComponent<Image>();
            fill.color = new Color(0.76f, 0.2f, 0.86f, 1f);
            fill.raycastTarget = false;
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            SetRect(fillRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            slider.fillRect = fillRect;
            slider.targetGraphic = background;
            return slider;
        }

        private static GameObject CreateUiObject(string objectName, Transform parent)
        {
            GameObject created = new GameObject(objectName, typeof(RectTransform));
            created.transform.SetParent(parent, false);
            return created;
        }

        private static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 size,
            Vector2 position)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }
    }
}
