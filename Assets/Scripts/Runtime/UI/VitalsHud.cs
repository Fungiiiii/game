using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Runtime-built placeholder HUD with persistent health/stamina bars and a conditional poison bar.
    /// </summary>
    public sealed class VitalsHud : MonoBehaviour
    {
        private const float PanelWidth = 320f;
        private const float PanelHeight = 170f;
        private const float BarHeight = 26f;

        private static readonly Color PanelColor = new Color(0.025f, 0.04f, 0.05f, 0.9f);
        private static readonly Color TrackColor = new Color(0.04f, 0.06f, 0.07f, 1f);
        private static readonly Color HealthColor = new Color(0.18f, 0.85f, 0.3f, 1f);
        private static readonly Color StaminaColor = new Color(0.15f, 0.55f, 1f, 1f);
        private static readonly Color PoisonColor = new Color(0.76f, 0.2f, 0.86f, 1f);

        private PlayerVitals _model;
        private GameObject _poisonBar;
        private UnityEngine.UI.Image _healthFill;
        private UnityEngine.UI.Image _staminaFill;
        private UnityEngine.UI.Image _poisonFill;

        public bool HealthBarVisible => _healthFill != null && _healthFill.gameObject.activeSelf;

        public bool StaminaBarVisible => _staminaFill != null && _staminaFill.gameObject.activeSelf;

        public bool PoisonBarVisible => _poisonBar != null && _poisonBar.activeSelf;

        public void Initialize(PlayerVitals model)
        {
            if (model == null)
            {
                throw new System.ArgumentNullException(nameof(model));
            }

            if (_model != null)
            {
                _model.Changed -= Refresh;
            }

            _model = model;
            BuildUiIfNeeded();
            _model.Changed += Refresh;
            Refresh(_model.Current);
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.Changed -= Refresh;
            }
        }

        private void BuildUiIfNeeded()
        {
            if (_healthFill != null)
            {
                return;
            }

            var canvasObject = new GameObject(
                "VitalsCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(UnityEngine.UI.CanvasScaler),
                typeof(UnityEngine.UI.GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var panelObject = new GameObject(
                "VitalsPanel",
                typeof(RectTransform),
                typeof(UnityEngine.UI.Image),
                typeof(UnityEngine.UI.VerticalLayoutGroup));
            panelObject.transform.SetParent(canvasObject.transform, false);

            var panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(32f, -32f);
            panelRect.sizeDelta = new Vector2(PanelWidth, PanelHeight);

            var panelImage = panelObject.GetComponent<UnityEngine.UI.Image>();
            panelImage.color = PanelColor;
            panelImage.raycastTarget = false;

            var layout = panelObject.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            _healthFill = CreateBar(panelObject.transform, "HealthBar", HealthColor);
            _staminaFill = CreateBar(panelObject.transform, "StaminaBar", StaminaColor);
            _poisonFill = CreateBar(panelObject.transform, "PoisonBar", PoisonColor);
            _poisonBar = _poisonFill.transform.parent.gameObject;
            _poisonBar.SetActive(false);
        }

        private static UnityEngine.UI.Image CreateBar(Transform parent, string name, Color fillColor)
        {
            var barObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(UnityEngine.UI.Image),
                typeof(UnityEngine.UI.LayoutElement));
            barObject.transform.SetParent(parent, false);

            var barRect = barObject.GetComponent<RectTransform>();
            barRect.sizeDelta = new Vector2(0f, BarHeight);

            var layoutElement = barObject.GetComponent<UnityEngine.UI.LayoutElement>();
            layoutElement.preferredHeight = BarHeight;

            var barBackground = barObject.GetComponent<UnityEngine.UI.Image>();
            barBackground.color = TrackColor;
            barBackground.raycastTarget = false;

            var fillObject = new GameObject(
                "Fill",
                typeof(RectTransform),
                typeof(UnityEngine.UI.Image));
            fillObject.transform.SetParent(barObject.transform, false);

            var fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            var fillImage = fillObject.GetComponent<UnityEngine.UI.Image>();
            fillImage.color = fillColor;
            fillImage.raycastTarget = false;

            return fillImage;
        }

        private void Refresh(VitalsSnapshot snapshot)
        {
            SetFill(_healthFill, snapshot.HealthNormalized);
            SetFill(_staminaFill, snapshot.StaminaNormalized);
            SetFill(_poisonFill, snapshot.IsPoisoned ? 1f : 0f);

            if (_poisonBar != null)
            {
                _poisonBar.SetActive(snapshot.IsPoisoned);
            }
        }

        private static void SetFill(UnityEngine.UI.Image fill, float normalizedValue)
        {
            if (fill == null)
            {
                return;
            }

            var fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(Mathf.Clamp01(normalizedValue), 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }
    }
}
